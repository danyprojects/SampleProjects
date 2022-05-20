#include "Map.h"

#include <server/ServerContext.h>
#include <server/network/Session.h>
#include <server/network/Packets.h>
#include <server/network/MessageBase.h>
#include <server/map/MapManager.h>
#include <server/map_objects/Block.h>
#include <server/map/data/MapData.h>
#include <server/lobby/LobbyConnector.h>
#include <server/network/SessionRepository.h>

#include <sdk/pool/ConcurrentMultiPool.hpp>

#include <cstring>
#include <chrono>

using namespace std::chrono;

namespace
{
	Tick initTick()
	{
		Tick tick;
		tick.set(duration_cast<milliseconds>(steady_clock::now().time_since_epoch()));

		return tick;
	}
}

using Packet = packet::Packet;

thread_local Random Map::_rand{};
thread_local Pathfinder Map::_pathfinder{};

void Map::onNewSession(uid::SharedPtr<Session> session)
{
	session->_character->_session = session.get();
	_inboundSessions.push(session.get());

	session->resume();
	_sessionRepo.insert(session);
}

bool Map::onMapLoop(Tick currentTick)
{
	assert(_debugMapLoopLock.exchange(true) == false);

	// We only ever write to it here so this prevents us from mistakenly writing to it somewhere else
	const_cast<Tick&>(_currentTick) = currentTick;

	// assign the tiles to astar for the current run since pathfinder is now thread local
	_pathfinder.setTiles(&_mapData._tiles);

	_timerController.runOnce(_currentTick);

	_characterController.I_beginMapLoop();
	_monsterController.processMobs();

	processSessions();

	// insert inbound sessions
	while (Session* session = _inboundSessions.tryPop())
	{
		_sessions.insert(session);

		_characterController.addCharacter(*session->_character);
	}

	_timerController.releasePoolMemory();
	_skillController.I_endMapLoop();
	_itemController.I_endMapLoop();
	_monsterController.I_endMapLoop();

	assert(_debugMapLoopLock.exchange(false) == true);
	return true;
}

void Map::processSessions()
{
	// we delay it 1 loop so iterators don't invalidate
	Session* pendingRemove = nullptr;
	Session* pendingLobbyRedirect = nullptr;

	for (auto&& session : _sessions)
	{
		if (pendingRemove)
			removeSession(*pendingRemove);
		if (pendingLobbyRedirect)
			redirectToLobby(*pendingLobbyRedirect);

		pendingRemove = nullptr;
		pendingLobbyRedirect = nullptr;

		auto oldState = session.getState();

		switch (session.update(*this))
		{
		case Session::State::Disconnecting:
			if (oldState == Session::State::Active) // disconnect event
			{
				
			}
			continue;
		case Session::State::Suspended: // terminal state always go to lobby
			pendingLobbyRedirect = &session;
			continue;
		case Session::State::Disconnected:
			pendingRemove = &session; // TODO check if we want to keep it in map for a while

			if (oldState != Session::State::Suspending) // was already removed
			{
				_characterController.notifyWarpOut(*session._character, CharacterController::WarpType::Teleport);
				_characterController.removeCharacter(*session._character);
			}
			continue;
		case Session::State::Active:
			break;
		}

		readMessages(session);
	}

	// For the case of the last iteration
	if (pendingRemove)
		removeSession(*pendingRemove);
	if (pendingLobbyRedirect)
		redirectToLobby(*pendingLobbyRedirect);
}

void Map::removeSession(Session& session)
{
	session._character->updateCharInfo();

	_sessions.remove(&session);
	_sessionRepo.remove(session);
}

void Map::redirectToLobby(Session& session)
{
	session._character->updateCharInfo();

	_sessions.remove(&session);
	auto oldSession = _sessionRepo.remove(session);

	session.discardMessages();
	_lobbyConnector.moveToLobby(std::move(oldSession));
}

void Map::readMessages(Session& session)
{
	auto& msgPool = _msgPool;
	auto& queue = session.messageQueue();

	while (auto* msg = reinterpret_cast<msg::Message*>(queue.pop()))
	{
		(this->*msgHandlerTable[enum_cast(msg->_id)])(*msg, *session._character);
		msgPool.free(msg, msg->_size);
	}
}

void Map::swapToMap(Character& character, MapId mapId)
{
	_characterController.notifyWarpOut(character, CharacterController::WarpType::Teleport);
	character.setMapId(mapId);

	// TODO:: mapinstanceid
	_characterController.removeCharacter(character);
	_sessions.remove(character._session);

	_mapManager.getMap(mapId)->_inboundSessions.push(character._session);
}

void Map::writePacket(const Character& character, const ArrayView<std::byte, uint8_t>& packet)
{
	character._session->writePacket(packet);
}

PacketHandlerResult Map::onRCV_Packet(const packet::Packet& packet, Session& session)
{
	uint16_t id = enum_cast(packet._id);

	if (id < sizeof(Map::packetHandlerTable) / sizeof(PacketHandlerPtr))
		return (this->*packetHandlerTable[id])(packet, *session._character);

	return PacketHandlerResult::INVALID();
}

//************ Packet handlers
PacketHandlerResult Map::onRCV_PlayerMove(const Packet& rawPacket, Character& src)
{
	return _unitController.onRCV_PlayerMove(rawPacket, src);
}

PacketHandlerResult Map::onRCV_PickUpItem(const Packet& rawPacket, Character& src)
{
	return _itemController.onRCV_PickUpItem(rawPacket, src);
}

PacketHandlerResult Map::onRCV_DropItem(const Packet& rawPacket, Character& src)
{
	return _itemController.onRCV_DropItem(rawPacket, src);
}

PacketHandlerResult Map::onRCV_UseItem(const Packet& rawPacket, Character& src)
{
	return _itemController.onRCV_UseItem(rawPacket, src);
}

PacketHandlerResult Map::onRCV_UnequipItem(const Packet& rawPacket, Character& src)
{
	return _itemController.onRCV_UnequipItem(rawPacket, src);
}

PacketHandlerResult Map::onRCV_NpcAction(const Packet& rawPacket, Character& src)
{
	return _npcController.onRCV_NpcAction(rawPacket, src);
}

PacketHandlerResult Map::onRCV_CastAoESkill(const Packet& rawPacket, Character& src)
{
	return _characterController.onRCV_CastAoESkill(rawPacket, src);
}

PacketHandlerResult Map::onRCV_CastTargetSkill(const Packet& rawPacket, Character& src)
{
	return _characterController.onRCV_CastTargetSkill(rawPacket, src);
}

PacketHandlerResult Map::onRCV_CastSelfTargetSkill(const packet::Packet& rawPacket, Character& src)
{
	return _characterController.onRCV_CastSelfTargetSkill(rawPacket, src);
}

PacketHandlerResult Map::onRCV_Attack(const packet::Packet& rawPacket, Character& src)
{
	auto& packet = reinterpret_cast<const packet::RCV_Attack&>(rawPacket);
	//_characterController.basicAttack(src, packet._target, false);

	return PacketHandlerResult::CONSUMED(packet);
}

PacketHandlerResult Map::onRCV_AutoAttack(const packet::Packet& rawPacket, Character& src)
{
	auto& packet = reinterpret_cast<const packet::RCV_Attack&>(rawPacket);
	//_characterController.basicAttack(src, packet._target, true);

	return PacketHandlerResult::CONSUMED(packet);
}

PacketHandlerResult Map::onRCV_ElevatedAtCommand(const Packet& rawPacket, Character& src)
{
	const auto& packet = reinterpret_cast<const packet::RCV_ElevatedAtCommand&>(rawPacket);

	switch (packet.cmd)
	{
		case ElevatedAtCommand::Size:
		case ElevatedAtCommand::Dex:
		case ElevatedAtCommand::Int:
		case ElevatedAtCommand::Agi:
		case ElevatedAtCommand::Vit:
		case ElevatedAtCommand::Luk:
		case ElevatedAtCommand::Str:
		case ElevatedAtCommand::Go:
		case ElevatedAtCommand::Warp:
		case ElevatedAtCommand::Zeny:
		case ElevatedAtCommand::Item:
		case ElevatedAtCommand::BaseLvl:
		case ElevatedAtCommand::GuildLvl:
		case ElevatedAtCommand::Skill:
			assert(!"No implememented");
			return PacketHandlerResult::INVALID();
		case ElevatedAtCommand::AllSkills:
			_characterController.maxAllSkills(src);
			break;
		case ElevatedAtCommand::JobChange:
			_characterController.resetJob(src, packet._CmdJobChange._job);
			break;
		case ElevatedAtCommand::JobLvl:	
			_characterController.changeJobLvl(src, packet._CmdJobLvl._jobLvl);
			break;
	}

	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, packet.size());
}

PacketHandlerResult Map::onRCV_LevelUpSingleSkill(const packet::Packet& rawPacket, Character & src)
{
	return _characterController.onRCV_LevelUpSingleSkill(rawPacket, src);
}

PacketHandlerResult Map::onRCV_LevelUpMultipleSkills(const packet::Packet& rawPacket, Character & src)
{
	return _characterController.onRCV_LevelUpMultipleSkills(rawPacket, src);
}

PacketHandlerResult Map::onRCV_LevelUpAllTreeSkills(const packet::Packet& rawPacket, Character & src)
{
	return _characterController.onRCV_LevelUpAllTreeSkills(rawPacket, src);
}

PacketHandlerResult Map::onRCV_ReturnToCharSelect(const packet::Packet& rawPacket, Character& src)
{
	_characterController.notifyWarpOut(src, CharacterController::WarpType::Teleport);
	_characterController.removeCharacter(src);

	src._session->suspend();

	return PacketHandlerResult::CONSUMED(reinterpret_cast<const packet::RCV_ReturnToCharacterSelect&>(rawPacket));
}

PacketHandlerResult Map::onRCV_GroupChat(const packet::Packet& rawPacket, Character& src)
{
	const auto& packet = reinterpret_cast<const packet::RCV_GroupChat&>(rawPacket);

	assert(packet._group == GroupChat::Map); // only map packet should pass throught 

	// TODO:: iterate map and send chat msg to all in range

	return PacketHandlerResult::CONSUMED(packet);
}

void Map::onMSG_Chat(const msg::Message& rawMessage, Character& src)
{
}

void Map::onMSG_GuildRecall(const msg::Message& rawMessage, Character& src)
{
}

void Map::onMSG_GuildInvite(const msg::Message& rawMessage, Character& src)
{
}

void Map::onMSG_GuildKick(const msg::Message& rawMessage, Character& src)
{
}

void Map::onMSG_PartyInvite(const msg::Message& rawMessage, Character& src)
{

}

void Map::onMSG_PartyKick(const msg::Message& rawMessage, Character& src)
{
}

//Lobby and PreProcessor are just transitive dependencies for the SessionManager
Map::Map(const MapData& mapData, MapInstanceIndex index, MapManager& mapManager, ServerContext& ctx)
	:_currentTick(initTick())
	,_instanceIndex(index)
	,_mapData(mapData)
	,_blockArray()
	,_tiles(_mapData._tiles)
	,_blockGrid(_tiles.width() / MAP_BLOCK_WIDTH + 1, _tiles.height() / MAP_BLOCK_HEIGHT + 1)
	,_timerController(_currentTick, ctx.timerPool())
	,_characterController(*this)
	,_unitController(*this)
	,_skillController(*this, ctx.skillPool())
	,_npcController(*this)
	,_monsterController(*this, ctx.monsterPool())
	,_buffController(*this)
	,_itemController(*this, ctx.itemPool())
	,_batteController(*this)
	,_triggerEffects(*this)
	,_itemScripts(*this)
	,_npcScripts(*this)
	,_mapManager(mapManager)
	,_sessionRepo(ctx.sessionRepo())
	,_msgPool(ctx.msgMultiPool())
	,_lobbyConnector(ctx.lobbyConnector())
{}

Map::~Map()
{
	// TODO:: clear up all _pendingCharSessions
}

#define HANDLER(Index, Id_, Callback) AssertIdIndex<Index, Id_>(&Map::Callback)
#define NULLPTR(Index, Id_, Callback) AssertIdIndex<Index, Id_>((PacketHandlerPtr)nullptr)

decltype(Map::packetHandlerTable) Map::packetHandlerTable =
{
	NULLPTR(0,	packet::Id::RCV_RegisterAccount,         nullptr),
	NULLPTR(1,	packet::Id::RCV_Login,                   nullptr),
	NULLPTR(2,	packet::Id::RCV_SelectCharacter,         nullptr),
	NULLPTR(3,	packet::Id::RCV_CreateCharacter,         nullptr),
	NULLPTR(4,	packet::Id::RCV_DeleteCharacter,         nullptr),
	HANDLER(5,	packet::Id::RCV_ReturnToCharacterSelect, onRCV_ReturnToCharSelect),
	HANDLER(6,	packet::Id::RCV_PlayerMove,              onRCV_PlayerMove),
	NULLPTR(7,	packet::Id::RCV_OtherMove,				 nullptr),
	HANDLER(8,	packet::Id::RCV_PickUpItem,				 onRCV_PickUpItem),
	HANDLER(9,	packet::Id::RCV_DropItem,				 onRCV_DropItem),
	HANDLER(10,	packet::Id::RCV_UseItem,				 onRCV_UseItem),
	HANDLER(11,	packet::Id::RCV_UnequipItem,			 onRCV_UnequipItem),
	HANDLER(12,	packet::Id::RCV_NpcAction,				 onRCV_NpcAction),
	HANDLER(13,	packet::Id::RCV_CastAreaOfEffectSkill,   onRCV_CastAoESkill),
	HANDLER(14,	packet::Id::RCV_CastSingleTargetSkill,   onRCV_CastTargetSkill),
	HANDLER(15, packet::Id::RCV_CastSelfTargetSkill,	 onRCV_CastSelfTargetSkill),
	HANDLER(16, packet::Id::RCV_Attack,					 onRCV_Attack),
	HANDLER(17, packet::Id::RCV_AutoAttack,				 onRCV_AutoAttack),
	HANDLER(18,	packet::Id::RCV_ElevatedAtCommand,		 onRCV_ElevatedAtCommand),
	HANDLER(19,	packet::Id::RCV_LevelUpSingleSkill,		 onRCV_LevelUpSingleSkill),
	HANDLER(20,	packet::Id::RCV_LevelUpMultipleSkills,	 onRCV_LevelUpMultipleSkills),
	HANDLER(21,	packet::Id::RCV_LevelUpAllTreeSkills,	 onRCV_LevelUpAllTreeSkills),
	NULLPTR(22,	packet::Id::RCV_Whisper1,				 nullptr),
	NULLPTR(23,	packet::Id::RCV_Whisper2,				 nullptr),
	HANDLER(24,	packet::Id::RCV_GroupChat,				 onRCV_GroupChat),
};
#undef NULLPTR

decltype(Map::msgHandlerTable) Map::msgHandlerTable =
{
	HANDLER(0,	msg::Id::Chat,		  onMSG_Chat),
	HANDLER(1,	msg::Id::GuildRecall, onMSG_GuildRecall),
	HANDLER(2,	msg::Id::PartyInvite, onMSG_PartyInvite),
	HANDLER(3,	msg::Id::PartyKick,	  onMSG_PartyKick),
	HANDLER(4,	msg::Id::GuildInvite, onMSG_GuildInvite),
	HANDLER(5,	msg::Id::GuildKick,	  onMSG_GuildKick)
};
#undef HANDLER