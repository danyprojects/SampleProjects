#pragma once

#include <server/common/JobMask.h>
#include <server/common/ItemDbId.h>
#include <server/common/EquipMask.h>
#include <server/common/CommonTypes.h>

#include <sdk/enum_cast.hpp>
#include <sdk/SingletonUniquePtr.hpp>

#include <array>

class ItemDb final
{
public:
    class Data final
    {
    public:
		ItemDbId _dbId = ItemDbId::None;
        ItemType _type = ItemType::Etc;
        Gender _gender = Gender::Any;
        uint32_t _buy = 0;
        uint32_t _sell = 0;
        uint16_t _weight = 0;
        EquipMask _equipMask;
        uint8_t _requiredLvl = 0;
        uint8_t _weaponLvl = 0;
        uint16_t _atk = 0;
        uint16_t _def = 0;
        struct
        {
            bool _transOnly : 1;
            bool _refinable : 1;
            uint8_t _slots : 3;
            uint8_t _range : 3;
        };
        WeaponType _weaponType;
        JobMask _jobsMask;

        Data();

        bool isEquipable() const;
        bool isUsable() const;
        bool allowsJob(Job job, bool isTrans) const;
    };

	ItemDb();

    static const Data& getItem(ItemDbId id)
    {
        return SingletonUniquePtr<const ItemDb>::get()->_itemData[enum_cast(id)];
    }

private:
	std::array<Data, enum_cast(ItemDbId::Last) + 1> _itemData;
};