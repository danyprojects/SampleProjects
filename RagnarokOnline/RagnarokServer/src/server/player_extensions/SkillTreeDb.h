#pragma once

#include <server/common/Job.h>
#include <server/common/SkillId.h>
#include <server/common/ServerLimits.h>

#include <sdk/enum_cast.hpp>
#include <sdk/array/ArrayView.hpp>

#include <iterator>
#include <initializer_list>
#include <array>


class SkillTreeDb final
{
public:
	class SkillEntry
	{
	public:
		class Requirement
		{
		public:
			SkillId _id = SkillId::None;
			int _dependencyIndex = 0;
			int _lvl = 0;
		private:
			friend class SkillEntry;
			friend class SkillTreeDb;

			constexpr Requirement(SkillId id, int lvl, int dependencyIndex)
				:_id(id), _lvl(lvl)
				, _dependencyIndex(dependencyIndex)
			{}
			constexpr Requirement() = default;
		};

		constexpr const ArrayView<Requirement> requirements() const
		{
			return { const_cast<Requirement*>(_requirements.data()), _requirementsCount };
		}

		SkillId _id = SkillId::None;
	private:
		friend class SkillTreeDb;

		constexpr SkillEntry(SkillId id, std::initializer_list<Requirement> list = {}, bool skipBasicSkill = false)
			:_id(id)
			,_requirementsCount(static_cast<int>(list.size()) + !skipBasicSkill)
		{
			int i = 0;

			if (!skipBasicSkill)
				_requirements[i++] = Requirement(SkillId::BasicSkill, 9, 0);

			for (const auto& val : list)
			{
				_requirements[i++] = val;
			}
		}

		constexpr SkillEntry() = default;

		int _requirementsCount = 0;
		std::array<Requirement, MAX_SKILL_REQUIREMENTS> _requirements = {};
	};
	
	constexpr size_t size() const { return _skillsCount; }

	typedef const SkillEntry* const_iterator;

	constexpr const_iterator begin() const;
	constexpr const_iterator end() const ;

	constexpr const auto& operator[](size_t index) const { return _skills[index]; }

	static constexpr const SkillTreeDb& getSkillTree(Job job);
private:
	typedef SkillEntry::Requirement Requirement;
	struct Private;

	constexpr SkillTreeDb(std::initializer_list<SkillEntry> list)
		:_skillsCount(static_cast<int>(list.size()))
	{
		int i = 0;
		for (const auto& val : list)
		{
			_skills[i++] = val;
		}
	}

	int _skillsCount;
	std::array<SkillEntry, MAX_SKILLS_PER_JOB> _skills = {};
};

struct SkillTreeDb::Private
{
	template<size_t index, Job::Value job>
	static constexpr SkillTreeDb SkillTree(SkillTreeDb skillDb)
	{
		static_assert(enum_cast(job) == index, "Index doesn't match job id");
		return skillDb;
	}

	template<size_t index, Job::Value job>
	static constexpr auto SkillEntryArray(ArrayView<uint32_t, uint8_t> arrayView)
	{
		static_assert(enum_cast(job) == index, "Index doesn't match job id");
		return arrayView;
	}

	static constexpr SkillTreeDb _skillTree[Job::Last + 1] =
	{
		SkillTree<0, Job::Novice>
		({
			SkillEntry(SkillId::BasicSkill, {}, true),
			SkillEntry(SkillId::FirstAid, {}, true),
			SkillEntry(SkillId::TrickDead, {}, true)
		}),

		// First jobs	

		SkillTree<1, Job::Swordsman>
		({
			SkillEntry(SkillId::SwordMastery),
			SkillEntry(SkillId::HpRecovery),
			SkillEntry(SkillId::Bash),
			SkillEntry(SkillId::Provoke),
			SkillEntry(SkillId::AutoBerserk),
			SkillEntry(SkillId::MovingHpRecovery),
			SkillEntry(SkillId::TwoHandSwordMastery, { Requirement(SkillId::SwordMastery, 1, 3) }),
			SkillEntry(SkillId::MagnumBreak,		 { Requirement(SkillId::Bash, 5, 5) }),
			SkillEntry(SkillId::Endure,				 { Requirement(SkillId::Provoke, 5, 6) }),
			SkillEntry(SkillId::FatalBlow)
		}),	
		SkillTree<2, Job::Archer>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<3, Job::Acolyte>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<4, Job::Mage>
		({
			SkillEntry(SkillId::StoneCurse),
			SkillEntry(SkillId::ColdBolt),
			SkillEntry(SkillId::LightningBolt),
			SkillEntry(SkillId::NapalmBeat),
			SkillEntry(SkillId::FireBolt),
			SkillEntry(SkillId::Sight),
			SkillEntry(SkillId::SpRecovery),
			SkillEntry(SkillId::FrostDiver,		{Requirement(SkillId::ColdBolt, 5, 4)}),
			SkillEntry(SkillId::ThunderStorm,   {Requirement(SkillId::LightningBolt, 4, 5)}),
			SkillEntry(SkillId::SoulStrike,		{Requirement(SkillId::NapalmBeat, 4, 6)}),
			SkillEntry(SkillId::FireBall,		{Requirement(SkillId::FireBolt, 4, 7)}),
			SkillEntry(SkillId::EnergyCoat),
			SkillEntry(SkillId::SafetyWall,		{Requirement(SkillId::NapalmBeat, 7, 6), Requirement(SkillId::SoulStrike, 5, 12)}),
			SkillEntry(SkillId::FireWall,		{Requirement(SkillId::FireBall, 5, 13),  Requirement(SkillId::Sight, 1, 8)})
		}),
		SkillTree<5, Job::Thief>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<6, Job::Merchant>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<7, Job::SuperNovice>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<8, Job::Taekwon>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<9, Job::Gunslinger>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<10, Job::Ninja>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),

		// Second jobs

		SkillTree<11, Job::Knight>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<12, Job::Crusader>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<13, Job::Hunter>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<14, Job::Bard>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<15, Job::Dancer>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<16, Job::Priest>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<17, Job::Monk>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<18, Job::Wizard>
		({
			SkillEntry(SkillId::Sense),
			SkillEntry(SkillId::IceWall,		{Requirement(SkillId::StoneCurse, 1, 3),	Requirement(SkillId::FrostDiver, 1, 10)}),
			SkillEntry(SkillId::JupitelThunder, {Requirement(SkillId::NapalmBeat, 1, 6),	Requirement(SkillId::LightningBolt, 1, 5)}),
			SkillEntry(SkillId::EarthSpike,		{Requirement(SkillId::StoneCurse, 1, 3)}),
			SkillEntry(SkillId::SightTrasher,	{Requirement(SkillId::Sight, 1, 8),			Requirement(SkillId::LightningBolt, 1, 5)}),
			SkillEntry(SkillId::FirePillar,		{Requirement(SkillId::FireWall, 1, 16)}),
			SkillEntry(SkillId::SightBlaster),
			SkillEntry(SkillId::FrostNova,		{Requirement(SkillId::IceWall, 1, 18)}),
			SkillEntry(SkillId::LordOfVermilion,{Requirement(SkillId::ThunderStorm, 1, 11), Requirement(SkillId::JupitelThunder, 5, 19)}),
			SkillEntry(SkillId::HeavensDrive,	{Requirement(SkillId::EarthSpike, 3, 20)}),
			SkillEntry(SkillId::MeteorStorm,	{Requirement(SkillId::SightTrasher, 2, 21), Requirement(SkillId::ThunderStorm, 1, 11)}),
			SkillEntry(SkillId::WaterBall,		{Requirement(SkillId::ColdBolt, 1, 4),		Requirement(SkillId::LightningBolt, 1, 5)}),
			SkillEntry(SkillId::Quagmire,		{Requirement(SkillId::HeavensDrive, 1, 26)}),
			SkillEntry(SkillId::StormGust,		{Requirement(SkillId::FrostDiver, 1, 10),	Requirement(SkillId::JupitelThunder, 3, 19)})
		}),
		SkillTree<19, Job::Sage>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<20, Job::Assassin>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<21, Job::Rogue>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<22, Job::Blacksmith>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<23, Job::Alchemist>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<24, Job::SoulLinker>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<25, Job::StarGladiator>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),

		// Trans jobs

		SkillTree<26, Job::LordKnight>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<27, Job::Paladin>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<28, Job::Sniper>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<29, Job::Clown>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<30, Job::Gypsy>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<31, Job::HighPriest>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<32, Job::Champion>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<33, Job::HighWizard>
		({
			SkillEntry(SkillId::Ganbantein,			{Requirement(SkillId::Sense, 1, 17),	 Requirement(SkillId::IceWall, 1, 18)}),
			SkillEntry(SkillId::MagicCrasher,		{Requirement(SkillId::SpRecovery, 1, 9)}),
			SkillEntry(SkillId::SoulDrain,			{Requirement(SkillId::SpRecovery, 5, 9), Requirement(SkillId::SoulStrike, 7, 12)}),
			SkillEntry(SkillId::NapalmVulcan,		{Requirement(SkillId::NapalmBeat, 5, 6)}),
			SkillEntry(SkillId::MysticAmplification),
			SkillEntry(SkillId::GravitationalField, {Requirement(SkillId::Quagmire, 1, 29),  Requirement(SkillId::MagicCrasher, 1, 32), Requirement(SkillId::MysticAmplification, 10, 35)})
		}),
		SkillTree<34, Job::Professor>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<35, Job::AssassinCross>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<36, Job::Stalker>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<37, Job::Whitesmith>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
		SkillTree<38, Job::Creator>
		({
			SkillEntry {SkillId::Last, {Requirement{SkillId::Devotion, 0, 0}}}
		}),
	};
};

inline constexpr const SkillTreeDb& SkillTreeDb::getSkillTree(Job job)
{
	return Private::_skillTree[job.toInt()];
}

inline constexpr SkillTreeDb::const_iterator SkillTreeDb::begin() const
{
	return _skills.data();
}

inline constexpr SkillTreeDb::const_iterator SkillTreeDb::end() const
{
	return _skills.data() + _skillsCount;
}