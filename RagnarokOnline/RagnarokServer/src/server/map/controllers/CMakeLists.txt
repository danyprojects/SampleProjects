cmake_minimum_required (VERSION 3.2)

include_directories(
	${CMAKE_CURRENT_SOURCE_DIR}
)

set(MAP_CONTROLLERS_HEADERS 
	"${CMAKE_CURRENT_SOURCE_DIR}/UnitController.h" 
	"${CMAKE_CURRENT_SOURCE_DIR}/SkillController.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/NpcController.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/MonsterController.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/BuffController.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/CharacterController.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/ItemController.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/BattleController.h"
PARENT_SCOPE)

set(MAP_CONTROLLERS_SOURCES 
	"${CMAKE_CURRENT_SOURCE_DIR}/UnitController.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/SkillController.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/NpcController.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/MonsterController.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/BuffController.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/CharacterController.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/ItemController.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/BattleController.cpp"
PARENT_SCOPE)

set(RAGNAROK_SERVER_TEST_SOURCES ${RAGNAROK_SERVER_TEST_SOURCES} PARENT_SCOPE)