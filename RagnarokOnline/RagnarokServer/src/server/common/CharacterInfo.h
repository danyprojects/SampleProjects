#pragma once

#include <server/common/Job.h>
#include <server/common/MapId.h>
#include <server/map/MapInstanceId.h>
#include <server/common/CommonTypes.h>
#include <server/common/CharacterId.h>
#include <server/common/ItemDbId.h>

#include <sdk/uid/Intrusive.hpp>

namespace uid
{
	class Name;
}

class CharacterInfo final
	: public uid::Intrusive<CharacterInfo>
{
public:
	CharacterInfo(uid::Name& name)
		:name(name)
	{}

	CharacterId id() const { return CharacterId(Intrusive::uid()); }

	uid::Name& name;
	Job job;
	Gender gender;
	MapId mapId;
	uint32_t expNextLvl;
	MapInstanceId mapInstanceId;
	HairStyle hairStyle;
	ItemDbId upperHeadgear;
	ItemDbId middleHeadgear;
	ItemDbId lowerHeadgear;
	uint8_t jobLvl;
	uint8_t lvl;
	struct
	{
		Hp hp;
		Sp sp;
		Str str;
		Agi agi;
		Vit vit;
		Int int_;
		Dex dex;
		Luck luck;
	}offline;
};