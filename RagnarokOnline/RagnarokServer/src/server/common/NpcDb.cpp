#include "NpcDb.h"

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
    static void loadNpc(NpcDb::NpcData& npcData, rjson::Value& npc)
    {
        npcData._subId = static_cast<NpcSubId>(npc["subId"].GetUint());
        npcData._isShop = npc["isShop"].GetBool();
    }

    static void loadShop(NpcDb::ShopData& shopData, rjson::Value& shop)
    {
        shopData._currency = static_cast<Currency>(shop["currency"].GetUint());
        auto itemCount = shop["itemCount"].GetUint();

        new (&shopData._items) decltype(shopData._items)(itemCount);
        uint8_t i = 0;
        for (auto& item : shop["items"].GetArray())
        {
            auto id = item["id"].GetUint();
            assert(id <= enum_cast(ItemDbId::Last));

            auto& itemData = shopData._items[i];
            itemData._itemId = static_cast<ItemDbId>(id);
            itemData._price = item["price"].GetUint();
            i++;
        }
    }
}

NpcDb::NpcDb()
{
    const auto path = fs::path(GlobalConfig::dataPath()).append("npc_db.json");

#ifdef _WIN32
    FILE* file;
    if (fopen_s(&file, path.string().c_str(), "r") != 0)
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
    }guard{ file };

    const size_t fileSize = static_cast<size_t>(fs::file_size(path));
    auto buffer = std::make_unique<char[]>(fileSize);

    rjson::FileReadStream inputStream(file, buffer.get(), fileSize);
    rjson::Document document;
    document.ParseStream(inputStream);

    //load npcs
    auto npcCount = document["npcCount"].GetUint();
    new (&_npcData) decltype(_npcData)(npcCount);

    for (auto& npc : document["npcs"].GetArray())
    {
        auto id = npc["id"].GetUint();
        assert(id < _npcData.size());

        auto& npcData = _npcData[id];
        npcData._dbId = static_cast<NpcId>(id);

        loadNpc(npcData, npc);
    }

    //load shops
    auto shopCount = document["shopCount"].GetUint();
    new (&_shopData) decltype(_shopData)(shopCount);

    for (auto& shop : document["shops"].GetArray())
    {
        auto id = shop["id"].GetUint();
        assert(id < _shopData.size());

        auto& shopData = _shopData[id];
        shopData._subId = static_cast<NpcSubId>(id);

        loadShop(shopData, shop);
    }
}