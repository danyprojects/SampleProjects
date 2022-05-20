project(RagnarokServer)

if(MSVC)
	cmake_minimum_required (VERSION 3.13.5)
else()
	cmake_minimum_required (VERSION 3.2)
endif()

include(${CMAKE_CURRENT_SOURCE_DIR}/dependencies/dependencies.cmake)
download_and_build_dependencies()

if(MSVC)
	add_definitions(-DASIO_STANDALONE -DASIO_NO_TYPEID /GR- -D_WIN32_WINNT=0x0501
				    -D_SILENCE_CXX17_ITERATOR_BASE_CLASS_DEPRECATION_WARNING
				    -D_SILENCE_CXX17_ALLOCATOR_VOID_DEPRECATION_WARNING
				    -D_ENABLE_ATOMIC_ALIGNMENT_FIX)
else()
	add_definitions(-DASIO_STANDALONE -DASIO_NO_TYPEID -fno-rtti)
endif()

include_directories(${CMAKE_CURRENT_SOURCE_DIR}/src)

add_subdirectory(src/sdk)
add_subdirectory(src/server)

if(MSVC)
	set(COMANDLINE_ARGS dataPath=\"${CMAKE_CURRENT_SOURCE_DIR}/data\"
						certificatePath=\"${CMAKE_CURRENT_SOURCE_DIR}/certificates\"
						listenPort=9999)
						
	string(REPLACE ";" " " COMANDLINE_ARGS "${COMANDLINE_ARGS}")
	set_target_properties(ragnarok_server PROPERTIES VS_DEBUGGER_COMMAND_ARGUMENTS "${COMANDLINE_ARGS}")
endif()