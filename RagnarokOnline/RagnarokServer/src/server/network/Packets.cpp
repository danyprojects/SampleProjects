#include "Packets.h"

#include <server/common/ServerRegexes.h>

#include <sdk/enum_cast.hpp>

#include <regex>
#include <array>
#include <string_view>

namespace
{
	bool sanitizeCString(const char* str, int maxLen, const std::regex& regex)
	{
		const char* end = (const char*)memchr(str, '\0', maxLen);
		if (end)
		{
			try
			{
				return std::regex_match(str, end, regex);
			}
			catch (const std::regex_error& e)
			{
				assert(false);
			}
		}

		return false;
	}
}

namespace packet
{
	bool RCV_RegisterAccount::validate() const
	{
		bool valid = _id == ID;
		valid &= sanitizeCString(name, sizeof(name), ACCOUNT_NAME_REGEX);
		valid &= sanitizeCString(password, sizeof(password), ACCOUNT_PASSWORD_REGEX);

		return valid;
	}

	bool RCV_Login::validate() const
	{
		bool valid = _id == ID;
		valid &= sanitizeCString(name, sizeof(name), ACCOUNT_NAME_REGEX);
		valid &= sanitizeCString(password, sizeof(password), ACCOUNT_PASSWORD_REGEX);

		return valid;
	}

	bool RCV_CreateCharacter::validate() const
	{
		static constexpr size_t MAX_POINT_DISTRIBUTION = 30;

		bool valid = _id == ID;

		valid &= index < MAX_ACCOUNT_CHARACTERS;
		valid &= (agi + str + vit + int_ + dex + luck) == MAX_POINT_DISTRIBUTION;
		valid &= hairStyle <= MAX_HAIR_STYLE;
		valid &= gender == Gender::Male || gender == Gender::Female;
		valid &= sanitizeCString(name, sizeof(name), CHARACTER_NAME_REGEX);

		return valid;
	}

	int RCV_ElevatedAtCommand::size() const
	{
		static constexpr std::array<size_t, enum_cast(ElevatedAtCommand::None)> sizes = { []()constexpr
		{
			std::array<size_t, enum_cast(ElevatedAtCommand::None)> local = {};

			local[enum_cast(ElevatedAtCommand::JobChange)] = HeaderSize + sizeof(RCV_ElevatedAtCommand::_CmdJobChange);
			local[enum_cast(ElevatedAtCommand::JobLvl)] = HeaderSize + sizeof(RCV_ElevatedAtCommand::_CmdJobLvl);
			local[enum_cast(ElevatedAtCommand::AllSkills)] = HeaderSize;

			return local;
		}() };

		return static_cast<int>(sizes[enum_cast(cmd)]);
	}
}