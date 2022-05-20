#pragma once
#include <server/common/NpcDb.h>

#include <array>
#include <cstdint>

class Character;
class Map;

class NpcScripts final
{
public:
	NpcScripts(Map& map)
		:_map(map)
	{}

	void runScript(Character& src, NpcSubId npcSubId);

private:
	typedef void(NpcScripts::* NpcTalkHandler)(Character&);	

	//Empty methods so we never have nullptrs in the npc table.
	constexpr void defaultTalk(Character&) {}
	
	//Npc common method declarations go here

	//Npc script declaration goes here
	void dummy(Character& src);

	Map& _map;
};