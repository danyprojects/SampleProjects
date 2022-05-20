#pragma once

#include <server/common/ItemDbId.h>
#include <server/common/MonsterId.h>
#include <server/common/CommonTypes.h>

#include <sdk/enum_cast.hpp>
#include <sdk/SingletonUniquePtr.hpp>

#include <array>
#include <vector>

class MonsterDb final
{
public:
    class Data final
    {
    public:
        struct
        {
            uint8_t str;
            uint8_t agi;
            uint8_t vit;
            uint8_t int_;
            uint8_t dex;
            uint8_t luk;
        };

        struct
        {
            uint32_t canMove : 1,
            looter : 1,
            aggressive : 1,
            assist : 1,
            castSensorIdle : 1,
            noRandomWalk : 1,
            noCastSkill : 1,
            canAttack : 1,
            castSensorChase : 1,
            changeChase : 1,
            angry : 1,
            changeTargetMelee : 1,
            changeTargetChase : 1,
            targetWeak : 1,
            randomTarget : 1,
            ignoreMelee : 1,
            ignoreMagic : 1,
            ignoreRanged : 1,
            mvp : 1,
            ignoreMisc : 1,
            noKnockback : 1,
            teleportBlock : 1,
            fixedItemDrop : 1,
            detector : 1,
            statusImmune : 1,
            skillImmune : 1,
            reserved : 6;
        }flag;

        struct Item
        {
            ItemDbId id;
            uint32_t dropChance;
        };

        /*public class Skill TODO
        {
            public string State{ get; set; }
            public int SkillId{ get; set; }
            public int SkillLvl{ get; set; }
            public int Rate{ get; set; }
            public int CastTime{ get; set; }
            public int Delay{ get; set; }
            public bool Cancelable{ get; set; }
            public string Target{ get; set; }
            public string ConditionType{ get; set; }
            public string ConditionValue{ get; set; }
            public string[] Values{ get; set; }
            public int Emotion{ get; set; }
            public string Chat{ get; set; }
        }*/

        MonsterId id;
        int16_t lvl;
        int32_t hp;
        int32_t sp;
        uint32_t exp;
        uint32_t jobExp;
        uint32_t minAttack;
        uint32_t maxAttack;
        uint16_t attackRange;
        uint16_t def;
        uint16_t mdef;
        uint8_t viewRange;
        uint8_t chaseRange;
        Size size;
        Race race;
        Element element;
        ElementLvl elementLvl;
        WalkSpd moveSpeed;
        int32_t attackDelay;
        int32_t attackMotion;
        int32_t damageMotion;
        int32_t mvpExp = 0;
        std::vector<Item> mvpDrops;
        std::vector<Item> drops;
        //public List<Skill> Skills{ get; set; } = new List<Skill>();
    };

    MonsterDb();

    static const Data& getMob(MonsterId id)
    {
        return SingletonUniquePtr<const MonsterDb>::get()->_mobData[enum_cast(id)];
    }

private:
	std::array<Data, enum_cast(MonsterId::Last) + 1> _mobData;
};