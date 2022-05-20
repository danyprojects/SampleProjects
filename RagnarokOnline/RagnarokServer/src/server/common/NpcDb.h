#pragma once

#include <server/common/ItemDbId.h>
#include <server/common/NpcId.h>
#include <server/common/CommonTypes.h>

#include <sdk/enum_cast.hpp>
#include <sdk/SingletonUniquePtr.hpp>
#include <sdk/array/FixedSizeArray.hpp>

#include <array>

typedef int16_t NpcSubId;

class NpcDb final
{
public:
    class NpcData final
    {
    public:
        NpcId _dbId;
        NpcSubId _subId;
        bool _isShop;
    };

    class ShopData final
    {
    public:
        class Item final
        {
        public:
            ItemDbId _itemId;
            uint32_t _price;
        };

        NpcSubId _subId;
        Currency _currency;
        FixedSizeArray<Item, uint8_t> _items;
    };

    NpcDb();

    static const NpcData& getNpc(NpcId id)
    {
        return SingletonUniquePtr<const NpcDb>::get()->_npcData[id];
    }

    static const ShopData& getShop(NpcSubId id)
    {
        return SingletonUniquePtr<const NpcDb>::get()->_shopData[id];
    }

private:
    FixedSizeArray<NpcData, uint16_t> _npcData;
    FixedSizeArray<ShopData, uint16_t> _shopData;
};