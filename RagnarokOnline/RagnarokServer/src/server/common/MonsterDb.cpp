#include "MonsterDb.h"

#include <server/common/GlobalConfig.h>

#include <rapidjson/filereadstream.h>
#include <rapidjson/document.h>

#include <cstdio>
#include <cassert>
#include <filesystem>

namespace fs = std::filesystem;
namespace rjson = rapidjson;

namespace
{
    static void loadMob(MonsterDb::Data& mobData, rjson::Value& mob)
    {
        mobData.str  = mob["str"].GetUint();
        mobData.agi  = mob["agi"].GetUint();
        mobData.vit  = mob["vit"].GetUint();
        mobData.int_ = mob["int"].GetUint();
        mobData.dex  = mob["dex"].GetUint();
        mobData.luk = mob["luk"].GetUint();

        mobData.flag.canMove           = mob["canMove"].GetBool();
        mobData.flag.looter            = mob["looter"].GetBool();
        mobData.flag.aggressive        = mob["aggressive"].GetBool();
        mobData.flag.assist            = mob["assist"].GetBool();
        mobData.flag.castSensorIdle    = mob["castSensorIdle"].GetBool();
        mobData.flag.noRandomWalk      = mob["noRandomWalk"].GetBool();
        mobData.flag.noCastSkill       = mob["noCastSkill"].GetBool();
        mobData.flag.canAttack         = mob["canAttack"].GetBool();
        mobData.flag.castSensorChase   = mob["castSensorChase"].GetBool();
        mobData.flag.changeChase       = mob["changeChase"].GetBool();
        mobData.flag.angry             = mob["angry"].GetBool();
        mobData.flag.changeTargetMelee = mob["changeTargetMelee"].GetBool();
        mobData.flag.changeTargetChase = mob["changeTargetChase"].GetBool();
        mobData.flag.targetWeak        = mob["targetWeak"].GetBool();
        mobData.flag.randomTarget      = mob["randomTarget"].GetBool();
        mobData.flag.ignoreMelee       = mob["ignoreMelee"].GetBool();
        mobData.flag.ignoreMagic       = mob["ignoreMagic"].GetBool();
        mobData.flag.ignoreRanged      = mob["ignoreRanged"].GetBool();
        mobData.flag.mvp               = mob["mvp"].GetBool();
        mobData.flag.ignoreMisc        = mob["ignoreMisc"].GetBool();
        mobData.flag.noKnockback       = mob["noKnockback"].GetBool();
        mobData.flag.teleportBlock     = mob["teleportBlock"].GetBool();
        mobData.flag.fixedItemDrop     = mob["fixedItemDrop"].GetBool();
        mobData.flag.detector          = mob["detector"].GetBool();
        mobData.flag.statusImmune      = mob["statusImmune"].GetBool();
        mobData.flag.skillImmune       = mob["skillImmune"].GetBool();

        mobData.lvl          = mob["lvl"].GetUint();
        mobData.hp           = mob["hp"].GetUint();
        mobData.sp           = mob["sp"].GetUint();
        mobData.exp          = mob["exp"].GetUint();
        mobData.jobExp       = mob["jExp"].GetUint();
        mobData.minAttack    = mob["minAttack"].GetUint();
        mobData.maxAttack    = mob["maxAttack"].GetUint();
        mobData.attackRange  = mob["attackRange"].GetUint();
        mobData.def          = mob["def"].GetUint();
        mobData.mdef         = mob["mdef"].GetUint();
        mobData.viewRange    = mob["viewRange"].GetUint();
        mobData.chaseRange   = mob["chaseRange"].GetUint();
        mobData.size         = static_cast<Size>(mob["size"].GetUint());
        mobData.race         = static_cast<Race>(mob["race"].GetUint());
        mobData.element      = static_cast<Element>(mob["element"].GetUint());
        mobData.elementLvl   = static_cast<ElementLvl>(mob["elementLvl"].GetUint());
        mobData.moveSpeed    = mob["moveSpeed"].GetUint();
        mobData.attackDelay  = mob["attackDelay"].GetUint();
        mobData.attackMotion = mob["attackMotion"].GetUint();
        mobData.damageMotion = mob["damageMotion"].GetUint();

        if (auto itr = mob.FindMember("mvpExp"); itr != mob.MemberEnd())
            mobData.mvpExp = itr->value.GetUint();

        using Item = MonsterDb::Data::Item;

        if (auto itr = mob.FindMember("mvpDrops"); itr != mob.MemberEnd())
        {
            mobData.mvpDrops.reserve(itr->value.GetArray().Size());

            for (auto& drop : itr->value.GetArray())
                mobData.mvpDrops.emplace_back(Item{ static_cast<ItemDbId>(drop["id"].GetUint()), drop["dropChance"].GetUint() });
        }

        if (auto itr = mob.FindMember("drops"); itr != mob.MemberEnd())
        {
            mobData.drops.reserve(itr->value.GetArray().Size());

            for (auto& drop : itr->value.GetArray())
                mobData.drops.emplace_back(Item { static_cast<ItemDbId>(drop["id"].GetUint()), drop["dropChance"].GetUint() });
        }
    }
}

MonsterDb::MonsterDb()
{
    const auto path = fs::path(GlobalConfig::dataPath()).append("monster_db.json");

#ifdef _WIN32
    FILE* file;
    if(fopen_s(&file, path.string().c_str(), "r") != 0)
        throw "Could not open file for reading!";
#else
    FILE* file = fopen(path.string().c_str(), "r");
    if (file == nullptr)
        throw "Could not open file for reading!";
#endif

    struct FileGuard
    {
        ~FileGuard() { std::fclose(_file); }
        FILE* _file;
    }guard {file};

    const size_t fileSize = static_cast<size_t>(fs::file_size(path));
    auto buffer = std::make_unique<char[]>(fileSize);

    rjson::FileReadStream inputStream(file, buffer.get(), fileSize);
    rjson::Document document;
    document.ParseStream(inputStream);

    for (auto& mob : document.GetArray())
    {
        auto id = mob["id"].GetUint();
        assert(id <= _mobData.size());

        auto& mobData = _mobData[id];
        mobData.id   = static_cast<MonsterId>(id);

        loadMob(mobData, mob);
    }
}