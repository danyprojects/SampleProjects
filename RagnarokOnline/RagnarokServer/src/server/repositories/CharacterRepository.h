#pragma once

#include <server/lobby/Account.h>
#include <server/common/ItemDbId.h>
#include <server/common/CharacterId.h>
#include <server/common/CommonTypes.h>
#include <server/common/ServerLimits.h>
#include <server/common/CharacterInfo.h>
#include <server/map_objects/Character.h>

#include <sdk/log/Log.h>
#include <sdk/uid/Name.h>
#include <sdk/uid/IdUnorderedMap.hpp>
#include <sdk/uid/NameUnorderedMap.h>


class CharacterRepository
{
public:
	struct NewCharacterInfo
	{
		Agi baseAgi;
		Str baseStr;
		Vit baseVit;
		Int baseInt;
		Dex baseDex;
		Luck baseLuck;
		HairStyle hairStyle;
		Gender gender;
	};

	// Returns valid CharInfo if it finds it nullptr otherwise
	uid::SharedPtr<CharacterInfo> findCharInfo(CharacterId id) const
	{
		return _charInfo.find(id);
	}

	// Handler: void(uid::SharedPtr<Character> character)
	// A valid Character if found, otherwise an empty Character
	// If we need to hit DB to load the character the handler is called asynchronously
	template<typename Handler>
	void asyncGetCharacter(CharacterId characterId, Handler&& handler)
	{
		auto character = _char.find(characterId);
		if (character)
		{
			return handler(std::move(character));
		}

		// TODO hit DB to find it, for now we return empty character
		return handler(uid::SharedPtr<Character>());
	}

	// Handler: void(uid::SharedPtr<CharacterIfo> charInfo)
	// A valid Charinfo if found, otherwise nullptr
	// If we need to hit DB to load the CharacterInfo the handler is called asynchronously
	template<typename Handler>
	void asyncGetCharInfo(CharacterId characterId, Handler&& handler)
	{
		auto character = _charInfo.find(characterId);
		if (character)
		{
			return handler(std::move(character));
		}


		//TODO hit DB to find it, for now we return empty character, and update name map
		// auto [success, n] = _names.updateId(name, uid::Id(id));
		return handler(uid::SharedPtr<CharacterInfo>());
	}

	// Handler: void(std::array<uid::SharedPtr<CharacterInfo>, MAX_ACCOUNT_CHARACTERS>& charInfos)
	// A valid Charinfo array if all found, otherwise nullptr
	// If we need to hit DB to load the CharacterInfo the handler is called asynchronously
	template<typename Handler>
	void asyncGetCharInfos(const std::array<CharacterId, MAX_ACCOUNT_CHARACTERS>& ids, Handler&& handler)
	{
		std::array<uid::SharedPtr<CharacterInfo>, MAX_ACCOUNT_CHARACTERS> infos{};

		for (int i = 0; i < static_cast<int>(ids.size()); i++)
		{
			auto id = ids[i];
			if (id.isValid())
				infos[i] = _charInfo.find(id);
		}

		// TODO hit DB to find remaining ones and insert them, then update name map
		
		for (int i = 0; i < static_cast<int>(ids.size()); i++)
			assert(!ids[i].isValid() || infos[i]);

		handler(infos);
	}

	ErrorCode renameCharacter(CharacterId characterId, std::string_view newName);

	//Handler: void(uid::SharedPtr<Character> character, ErrorCode ec)
	template<typename Handler>
	void asyncInsert(const char(&nameStr)[uid::Name::MAX_LEN], uint8_t index, Account& account, const NewCharacterInfo& newCharInfo,
		Handler&& handler)
	{
		auto* name = _names.insert(nameStr);

		if (name == nullptr)
			return handler({}, ErrorCode::DuplicateName);

		auto uid = uid::Id(_nextAvailableId.fetch_add(1));

		// In practice should never fail
		if (!_names.updateId(nameStr, uid))
		{
			Log::error("Failed to update id for char=%s, charId=%d has been leeked", TAG, nameStr, uid.hash());
			return handler(uid::SharedPtr<Character>(), ErrorCode::InternalError);
		}

		//TODO insert on DB here for now we do nothing

		auto charInfo = _charInfo.makeShared(uid, *name);
		assert(charInfo);

		// TODO:: move elsewhere
		charInfo->offline.hp = 100;
		charInfo->offline.sp = 100;
		charInfo->offline.str = newCharInfo.baseStr;
		charInfo->offline.agi = newCharInfo.baseAgi;
		charInfo->offline.vit = newCharInfo.baseVit;
		charInfo->offline.int_ = newCharInfo.baseInt;
		charInfo->offline.dex = newCharInfo.baseDex;
		charInfo->offline.luck = newCharInfo.baseLuck;
		charInfo->hairStyle = newCharInfo.hairStyle;
		charInfo->upperHeadgear = ItemDbId::None;
		charInfo->middleHeadgear = ItemDbId::None;
		charInfo->lowerHeadgear = ItemDbId::None;
		charInfo->gender = newCharInfo.gender;
		charInfo->mapId = MapId::GlKnt01;

		auto character = _char.makeShared(uid, account, std::move(charInfo));
		assert(character);

		//Initialize character block stuff
		character->_position = { 160, 115 };
		character->_direction = Direction::Down;
		character->_headDirection = Direction::Down;
		character->setLvl(1);
		character->setJobLvl(1);
		character->setExpNextLvl(0);

		account.addCharacter(index, character->uid());

		return handler(std::move(character), ErrorCode::Ok);
	}

	CharacterRepository()
		:_char(CHARACTER_POOL_SIZE)
		,_charInfo(MAX_CONNECTIONS)
		,_names(MAX_UNIQUE_CHARACTER_NAMES, CHARACTER_NAME_POOL_DELETE_THRESHOLD)
	{
		Log::info("Started with %d max unique names", TAG, MAX_UNIQUE_CHARACTER_NAMES);
	}
private:
	static constexpr char* const TAG = "CharRepo";
	std::atomic<uint32_t> _nextAvailableId = 1; // first valid id, TODO::load from db the last one
	uid::IdUnorderedMap<Character> _char;
	uid::IdUnorderedMap<CharacterInfo> _charInfo;
	uid::NameUnorderedMap _names;
};