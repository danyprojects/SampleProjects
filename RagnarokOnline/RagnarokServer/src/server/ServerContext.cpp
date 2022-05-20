#include "ServerContext.h"

#include <server/lobby/LobbyConnector.h>

ServerContext::ServerContext(std::promise<ServerCommand>& command, LogController& logController)
	:_msgNodePool()
	,_msgMultiPool()
	,_timerPool()
	,_monsterPool()
	,_skillPool()
	,_itemPool()
	,_sessionPool(*_msgMultiPool, *_msgNodePool, _packetPreProcessor)
	,_monsterDb()
	,_itemDb()
	,_npcDb()
	,_accountRepo()
	,_characterRepo()
	,_partyRepo()
	,_guildRepo()
	,_mapDataRepo()
	,_sessionRepo(*_sessionPool)
	,_lobbyConnector(std::make_unique<LobbyConnector>())
	,_packetPreProcessor(*_sessionRepo, *_msgMultiPool)
	,_mapManager(logController, *_mapDataRepo, *this)
	,_lobby(command, *_mapManager, *_accountRepo, *_characterRepo, logController, *_lobbyConnector, *_sessionPool)
{}

ServerContext::~ServerContext() = default;