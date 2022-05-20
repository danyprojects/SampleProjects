cmake_minimum_required (VERSION 3.2)

include_directories(
	${CMAKE_CURRENT_SOURCE_DIR}
)

set(MAP_HEADERS 
	"${CMAKE_CURRENT_SOURCE_DIR}/Map.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/MapInstanceId.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/MapManager.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/BlockGrid.hpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/BlockArray.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/DynamicBlockArray.hpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/FixedBlockArray.hpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/BlockGridIntrusive.h"
	"${CMAKE_CURRENT_SOURCE_DIR}/IMapLoop.hpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/SessionDescriptor.h"
PARENT_SCOPE)

set(MAP_SOURCES 
	"${CMAKE_CURRENT_SOURCE_DIR}/Map.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/MapManager.cpp"
 PARENT_SCOPE)