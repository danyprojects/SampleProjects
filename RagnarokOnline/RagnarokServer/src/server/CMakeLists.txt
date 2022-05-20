cmake_minimum_required (VERSION 3.2)


set(SERVER_HEADERS
	"Party.h"
	"Guild.h"
	"ServerContext.h"
)

set(SERVER_SOURCES 
	"main.cpp"
	"Party.cpp"
	"Guild.cpp"
	"ServerContext.cpp"
)

set(RAGNAROK_SERVER_TEST_SOURCES
	"catch_main.cpp"
)

add_subdirectory(map)
add_subdirectory(wrappers)
add_subdirectory(network)
add_subdirectory(lobby)
add_subdirectory(map_objects)
add_subdirectory(repositories)
add_subdirectory(map/data)
add_subdirectory(map/controllers)
add_subdirectory(map/controllers/scripts)
add_subdirectory(map/pathfinding)
add_subdirectory(player_extensions)
add_subdirectory(common)


add_executable (ragnarok_server ${SERVER_HEADERS} ${SERVER_SOURCES} ${MAP_HEADERS} ${MAP_SOURCES}
				${WRAPPERS_HEADERS} ${WRAPPERS_SOURCES} ${NETWORK_HEADERS} ${NETWORK_SOURCES}
				${LOBBY_HEADERS} ${LOBBY_SOURCES} ${MAP_OBJECTS_HEADERS} ${MAP_OBJECTS_SOURCES} 
				${REPOSITORIES_HEADERS} ${REPOSITORIES_SOURCES} ${MAP_DATA_HEADERS} ${MAP_DATA_SOURCES}
				${MAP_CONTROLLERS_HEADERS} ${MAP_CONTROLLERS_SOURCES} ${PATHFINDING_HEADERS} ${PATHFINDING_SOURCES}
				${PLAYER_EXTENSIONS_HEADERS} ${PLAYER_EXTENSIONS_SOURCES} ${SCRIPT_HEADERS} ${SCRIPT_SOURCES}
				${TIMERS_HEADERS} ${TIMERS_SOURCES} ${COMMON_HEADERS} ${COMMON_SOURCES}
				${CMAKE_CURRENT_SOURCE_DIR}/map/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/wrappers/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/network/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/lobby/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/map_objects/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/repositories/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/map/data/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/map/controllers/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/map/controllers/scripts/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/map/pathfinding/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/player_extensions/CMakeLists.txt
				${CMAKE_CURRENT_SOURCE_DIR}/common/CMakeLists.txt)

target_link_libraries(ragnarok_server crypto-1_1 ssl-1_1)

set_property(TARGET ragnarok_server PROPERTY CXX_STANDARD 17)

source_group("server" FILES ${SERVER_HEADERS})
source_group("server" FILES ${SERVER_SOURCES})

source_group("map" FILES ${MAP_HEADERS})
source_group("map" FILES ${MAP_SOURCES})
source_group("map" FILES ${CMAKE_CURRENT_SOURCE_DIR}/map/CMakeLists.txt)


source_group("wrappers" FILES ${WRAPPERS_HEADERS})
source_group("wrappers" FILES ${WRAPPERS_SOURCES})
source_group("wrappers" FILES ${CMAKE_CURRENT_SOURCE_DIR}/wrappers/CMakeLists.txt)

source_group("network" FILES ${NETWORK_HEADERS})
source_group("network" FILES ${NETWORK_SOURCES})
source_group("network" FILES ${CMAKE_CURRENT_SOURCE_DIR}/network/CMakeLists.txt)

source_group("lobby" FILES ${LOBBY_HEADERS})
source_group("lobby" FILES ${LOBBY_SOURCES})
source_group("lobby" FILES ${CMAKE_CURRENT_SOURCE_DIR}/lobby/CMakeLists.txt)

source_group("map_objects" FILES ${MAP_OBJECTS_HEADERS})
source_group("map_objects" FILES ${MAP_OBJECTS_SOURCES})
source_group("map_objects" FILES ${CMAKE_CURRENT_SOURCE_DIR}/map_objects/CMakeLists.txt)

source_group("repositories" FILES ${REPOSITORIES_HEADERS})
source_group("repositories" FILES ${REPOSITORIES_SOURCES})
source_group("repositories" FILES ${CMAKE_CURRENT_SOURCE_DIR}/repositories/CMakeLists.txt)

source_group("map\\data" FILES ${MAP_DATA_HEADERS})
source_group("map\\data" FILES ${MAP_DATA_SOURCES})
source_group("map\\data" FILES ${CMAKE_CURRENT_SOURCE_DIR}/map/data/CMakeLists.txt)

source_group("map\\controllers" FILES ${MAP_CONTROLLERS_HEADERS})
source_group("map\\controllers" FILES ${MAP_CONTROLLERS_SOURCES})
source_group("map\\controllers" FILES ${CMAKE_CURRENT_SOURCE_DIR}/map/controllers/CMakeLists.txt)

source_group("map\\controllers\\scripts" FILES ${SCRIPT_HEADERS})
source_group("map\\controllers\\scripts" FILES ${SCRIPT_SOURCES})
source_group("map\\controllers\\scripts" FILES ${CMAKE_CURRENT_SOURCE_DIR}/map/controllers/scripts/CMakeLists.txt)


source_group("map\\pathfinding" FILES ${PATHFINDING_HEADERS})
source_group("map\\pathfinding" FILES ${PATHFINDING_SOURCES})
source_group("map\\pathfinding" FILES ${CMAKE_CURRENT_SOURCE_DIR}/map/pathfinding/CMakeLists.txt)

source_group("map\\timers" FILES ${TIMERS_HEADERS})
source_group("map\\timers" FILES ${TIMERS_SOURCES})
source_group("map\\timers" FILES ${CMAKE_CURRENT_SOURCE_DIR}/map/timers/CMakeLists.txt)

source_group("player_extensions" FILES ${PLAYER_EXTENSIONS_HEADERS})
source_group("player_extensions" FILES ${PLAYER_EXTENSIONS_SOURCES})
source_group("player_extensions" FILES ${CMAKE_CURRENT_SOURCE_DIR}/player_extensions/CMakeLists.txt)

source_group("common" FILES ${COMMON_HEADERS})
source_group("common" FILES ${COMMON_SOURCES})
source_group("common" FILES ${CMAKE_CURRENT_SOURCE_DIR}/common/CMakeLists.txt)

target_link_libraries(ragnarok_server sdk)



