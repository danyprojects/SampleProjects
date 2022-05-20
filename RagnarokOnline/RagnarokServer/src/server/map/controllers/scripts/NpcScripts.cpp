#include "NpcScripts.h"

#include <server/network/Packets.h>
#include <server/map_objects/Character.h>
#include <server/map/Map.h>

#include <sdk/enum_cast.hpp>

#include <array>

void NpcScripts::runScript(Character& src, NpcSubId npcSubId)
{
	using ArrayType = std::array<NpcScripts::NpcTalkHandler, enum_cast(NpcHandlerId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		for (int i = 0; i <= enum_cast(NpcHandlerId::Last); i++)
			table[i] = &NpcScripts::defaultTalk;

		//Construct table here

		return table;
	}();

	static_assert([]() constexpr
		{
#ifdef NDEBUG
			for (auto&& v : _table)
				if (v == nullptr)
					return false;
#endif
			return true;
		}());


	(this->*_table[npcSubId])(src);
}

//*************** Function definitions for item handlers
void NpcScripts::dummy(Character& src)
{

}
