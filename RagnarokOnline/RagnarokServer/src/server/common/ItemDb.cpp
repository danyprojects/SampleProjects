#include "ItemDb.h"

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
    static void loadItem(ItemDb::Data& itemData, rjson::Value& item)
    {
        itemData._type  = static_cast<ItemType>(item["type"].GetUint());
        itemData._buy  = item["buy"].GetUint();
        itemData._sell  = item["sell"].GetUint();
        itemData._weight = item["weight"].GetUint();
        itemData._requiredLvl = item["reqLvl"].GetUint();
        itemData._jobsMask = JobMask(static_cast<JobMask::Value>(item["jobs"].GetUint64()));
        itemData._transOnly = item["trans"].GetBool();

        switch (itemData._type)
        {
        case ItemType::Weapon:
            itemData._weaponLvl = item["weaponLvl"].GetUint();
            itemData._weaponType = static_cast<WeaponType>(item["weaponType"].GetUint());
            itemData._range = item["range"].GetUint();
        case ItemType::Armor:
            itemData._slots = item["slots"].GetUint();
            itemData._refinable = item["refinable"].GetBool();
            itemData._def = item["def"].GetUint();
        case ItemType::Ammo:
            itemData._atk = item["atk"].GetUint();
            itemData._gender = static_cast<Gender>(item["gender"].GetUint());
        case ItemType::Card:
            itemData._equipMask = EquipMask(static_cast<EquipMask::Value>(item["equipSlot"].GetUint()));
            break;
        }

        assert(itemData._sell <= itemData._buy);
    }
}

ItemDb::ItemDb()
{
    const auto path = fs::path(GlobalConfig::dataPath()).append("item_db.json");

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

    for (auto& item : document.GetArray())
    {
        auto id = item["id"].GetUint();
        assert(id <= _itemData.size());

        auto& itemData = _itemData[id];
        itemData._dbId = static_cast<ItemDbId>(id);

        loadItem(itemData, item);
    }
}

ItemDb::Data::Data()
    :_transOnly(false)
    ,_refinable(false)
    ,_range(0)
    ,_slots(0)
{}

bool ItemDb::Data::allowsJob(Job job, bool isTrans) const
{
    auto val = static_cast<JobMask::Value>(1ull << job.toInt());

    return _jobsMask & val && (!_transOnly || isTrans);
}

bool ItemDb::Data::isEquipable() const
{
    return _type == ItemType::Armor || _type == ItemType::Weapon || _type == ItemType::Ammo;
}

bool ItemDb::Data::isUsable() const
{
    return _type == ItemType::Usable || _type == ItemType::DelayConsume || _type == ItemType::Healing || _type == ItemType::Card;
}