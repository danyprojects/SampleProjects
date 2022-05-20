#pragma once

#include <server/network/PacketId.h>
#include <server/network/IPacketHandler.h>
#include <server/network/MessageId.h>
#include <server/common/MapId.h>
#include <server/map/data/Tile.h>
#include <server/map/BlockGrid.hpp>
#include <server/map/BlockArray.h>
#include <server/map/IMapLoop.hpp>
#include <server/lobby/ILobbySessionHandler.h>
#include <server/map/pathfinding/Pathfinder.h>
#include <server/map/controllers/UnitController.h>
#include <server/map/controllers/SkillController.h>
#include <server/map/controllers/NpcController.h>
#include <server/map/controllers/BuffController.h>
#include <server/map/controllers/MonsterController.h>
#include <server/map/controllers/CharacterController.h>
#include <server/map/controllers/ItemController.h>
#include <server/map/controllers/BattleController.h>
#include <server/map/controllers/scripts/TriggerEffects.h>
#include <server/map/controllers/scripts/ItemScripts.h>
#include <server/map/controllers/scripts/NpcScripts.h>

#include <sdk/Tick.h>
#include <sdk/Random.h>
#include <sdk/IntrusiveList.hpp>
#include <sdk/uid/SharedPtr.hpp>
#include <sdk/timer/TimerController.h>
#include <sdk/array/FixedSizeArray.hpp>
#include <sdk/array/FixedSizeArray2D.hpp>
#include <sdk/MultiProducerIntrusiveQueue.hpp>

#include <cstdint>
#include <functional>
#include <chrono>

namespace msg
{
	struct Message;
}

class Guild;
class MapManager;
class MapData;
class Block;
class ServerContext;
class LobbyConnector;
class SessionRepository;
class ConcurrentMultiPool;

class Map final
	: public ILobbySessionHandler<Map> 
	, public IPacketHandler<Map>
	, public IMapLoop<MapManager, Map>
{
public:
	Map(const MapData& mapData, MapInstanceIndex index, MapManager& mapManager, ServerContext& ctx);
	~Map();

	const Tile* tryGetTile(int x, int y) const
	{
		return (x >= 0 && y >= 0 && x < _tiles.width() && y < _tiles.height()) ? &_tiles(x, y) : nullptr;
	}

	void swapToMap(Character& character, MapId mapId);

	template<typename T>
	void writePacket(BlockId id, const T& packet)
	{
		static_assert(std::is_base_of<packet::SND_FixedSizePacket, T>::value);

		writePacket(id, ArrayView<std::byte, uint8_t>{ (std::byte*)&packet, sizeof(T) });
	}

	template<typename T>
	void writePacket(const Character& character, const T& packet)
	{
		static_assert(std::is_base_of<packet::SND_FixedSizePacket, T>::value);

		writePacket(character, { (std::byte*) & packet, sizeof(T) });
	}

	void writePacket(BlockId id, const ArrayView<std::byte, uint8_t>& packet)
	{
		assert(_blockArray.get(id) && _blockArray.get(id)->_type == BlockType::Character);

		writePacket(reinterpret_cast<Character&>(_blockArray.unsafeGet(id)), packet);
	}

	void writePacket(const Character& character, const ArrayView<std::byte, uint8_t>& packet);

private:
	friend class ILobbySessionHandler<Map>;
	friend class IPacketHandler<Map>;
	friend class IMapLoop<MapManager, Map>;

	typedef PacketHandlerResult(Map::* PacketHandlerPtr)(const packet::Packet& packet, Character& src);
	typedef void(Map::* MessageHandlerPtr)(const msg::Message& msg, Character& src);

	// This is EXCEPTIONALY multithreaded and called Outside of maploop, will get called by lobby
	void onNewSession(uid::SharedPtr<Session> session);

	// Called only once during MapLoop
	void processSessions();

	void removeSession(Session& session);
	void redirectToLobby(Session& session);
	void readMessages(Session& session);

	//returns true if map still want's to run again
	//false if it should be released, this way we can reuse the instance
	bool onMapLoop(Tick currentTick);

	PacketHandlerResult onRCV_Packet(const packet::Packet& packet, Session& session);

	//************ Packet handlers
	PacketHandlerResult onRCV_PlayerMove(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_PickUpItem(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_DropItem(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_UseItem(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_UnequipItem(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_NpcAction(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_CastAoESkill(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_CastTargetSkill(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_CastSelfTargetSkill(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_Attack(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_AutoAttack(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_ElevatedAtCommand(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_LevelUpSingleSkill(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_LevelUpMultipleSkills(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_LevelUpAllTreeSkills(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_ReturnToCharSelect(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_GroupChat(const packet::Packet& rawPacket, Character& src);

	//************ Message handlers
	void onMSG_Chat(const msg::Message& rawMessage, Character& src);
	void onMSG_GuildRecall(const msg::Message& rawMessage, Character& src);
	void onMSG_GuildInvite(const msg::Message& rawMessage, Character& src);
	void onMSG_GuildKick(const msg::Message& rawMessage, Character& src);
	void onMSG_PartyInvite(const msg::Message& rawMessage, Character& src);
	void onMSG_PartyKick(const msg::Message& rawMessage, Character& src);

	template<size_t index, packet::Id _id>
	static constexpr auto AssertIdIndex(PacketHandlerPtr ptr);
	template<size_t index, msg::Id _id>
	static constexpr auto AssertIdIndex(MessageHandlerPtr ptr);
public:
	const Tick _currentTick;
	const MapInstanceIndex _instanceIndex;  //we need this for warp portals in instances
	const MapData& _mapData;
	BlockArray _blockArray;
	FixedSizeArray2D<Tile, uint16_t> _tiles;
	BlockGrid _blockGrid;
	TimerController::Instance<Map> _timerController;
	thread_local static Random _rand;
	thread_local static Pathfinder _pathfinder;

	// controllers list
	CharacterController _characterController;
	UnitController _unitController;
	SkillController _skillController;
	NpcController _npcController;
	MonsterController _monsterController;
	BuffController _buffController;
	ItemController _itemController;
	BattleController _batteController;
	TriggerEffects _triggerEffects;
	ItemScripts _itemScripts;
	NpcScripts _npcScripts;
private:
	MapManager& _mapManager;
	SessionRepository& _sessionRepo;
	ConcurrentMultiPool& _msgPool;
	LobbyConnector& _lobbyConnector;
	IntrusiveList<Session> _sessions;
	MultiProducerIntrusiveQueue<Session> _inboundSessions;

	static const PacketHandlerPtr packetHandlerTable[enum_cast(packet::Id::LastReceivePacket) + 1];
	static const MessageHandlerPtr msgHandlerTable[enum_cast(msg::Id::Last) + 1];
#ifndef NDEBUG
	std::atomic<bool> _debugMapLoopLock = false;
#endif
};

template<size_t index, packet::Id _id>
inline constexpr auto Map::AssertIdIndex(PacketHandlerPtr ptr)
{
	static_assert(enum_cast(_id) == index, "Index doesn't match packet id");
	return ptr;
}

template<size_t index, msg::Id _id>
inline constexpr auto Map::AssertIdIndex(MessageHandlerPtr ptr)
{
	static_assert(enum_cast(_id) == index, "Index doesn't match msg id");
	return ptr;
}