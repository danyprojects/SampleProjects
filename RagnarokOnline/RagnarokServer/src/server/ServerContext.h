#pragma once

#include <server/lobby/ServerCommand.h>
#include <server/wrappers/AccountRepositoryWrapper.h>
#include <server/wrappers/CharacterRepositoryWrapper.h>
#include <server/wrappers/PartyRepositoryWrapper.h>
#include <server/wrappers/GuildRepositoryWrapper.h>
#include <server/wrappers/LobbyWrapper.h>
#include <server/wrappers/MapManagerWrapper.h>
#include <server/wrappers/LogControllerWrapper.h>
#include <server/wrappers/SessionObjectPoolWrapper.h>
#include <server/wrappers/SessionRepositoryWrapper.h>
#include <server/wrappers/MapDataRepositoryWrapper.h>
#include <server/wrappers/MonsterDbWrapper.h>
#include <server/wrappers/ItemDbWrapper.h>
#include <server/wrappers/NpcDbWrapper.h>
#include <server/wrappers/ConcurrentMonsterPoolWrapper.h>
#include <server/wrappers/ConcurrentFieldItemPoolWrapper.h>
#include <server/wrappers/ConcurrentFieldSkillPoolWrapper.h>
#include <server/wrappers/ConcurrentTimerPoolWrapper.h>
#include <server/wrappers/PacketPreProcessorWrapper.h>
#include <server/wrappers/MessageNodePoolWrapper.h>
#include <server/wrappers/MessageMultiPoolWrapper.h>

#include <future>
#include <memory>

class LobbyConnector;
class LogController;

// We ENFORCE correct dependency contruction/destruction order here
// It also simplifies injecting dependencies to the maps
class ServerContext
{
public:
	ServerContext(std::promise<ServerCommand>& command, LogController& logController);
	~ServerContext();

	ServerContext(ConcurrentMonsterPoolWrapper&& lhr) = delete;
	ServerContext& operator=(ConcurrentMonsterPoolWrapper&& lhr) = delete;
	ServerContext(const ConcurrentMonsterPoolWrapper& lhr) = delete;
	ServerContext& operator=(const ConcurrentMonsterPoolWrapper& lhr) = delete;

	auto& timerPool() { return *_timerPool; }
	auto& monsterPool() { return *_monsterPool; }
	auto& skillPool() { return *_skillPool; }
	auto& itemPool() { return *_itemPool; }
	auto& msgMultiPool() { return *_msgMultiPool; }
	auto& sessionRepo() { return *_sessionRepo; }
	auto& lobbyConnector() { return *_lobbyConnector; }
private:
	// pools
	MessageNodePoolWrapper _msgNodePool;
	MessageMultiPoolWrapper _msgMultiPool;
	ConcurrentTimerPoolWrapper _timerPool;
	ConcurrentMonsterPoolWrapper _monsterPool;
	ConcurrentFieldSkillPoolWrapper _skillPool;
	ConcurrentFieldItemPoolWrapper _itemPool;
	SessionObjectPoolWrapper _sessionPool;   // Must be after message pools

	// dbs
	MonsterDbWrapper _monsterDb;
	ItemDbWrapper _itemDb;
	NpcDbWrapper _npcDb;

	// repos
	AccountRepositoryWrapper _accountRepo;
	CharacterRepositoryWrapper _characterRepo;
	PartyRepositoryWrapper _partyRepo;
	GuildRepositoryWrapper _guildRepo;
	MapDataRepositoryWrapper _mapDataRepo;
	SessionRepositoryWrapper _sessionRepo;

	std::unique_ptr<LobbyConnector> _lobbyConnector;
	PacketPreProcessorWrapper _packetPreProcessor;
	MapManagerWrapper _mapManager;
	LobbyWrapper _lobby;
};