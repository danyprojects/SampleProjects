#pragma once

#include <regex>

#define REGEX_FLAGS std::regex::ECMAScript | std::regex::optimize | std::regex::icase | std::regex::nosubs

inline const std::regex ACCOUNT_NAME_REGEX     { "[a-zA-Z]([0-9a-zA-Z]){3,}" , REGEX_FLAGS };
inline const std::regex ACCOUNT_PASSWORD_REGEX { "[[:print:]]{8,}", REGEX_FLAGS };
inline const std::regex CHARACTER_NAME_REGEX   { "[a-zA-Z](\\s?[0-9a-zA-Z]){2,}", REGEX_FLAGS };
inline const std::regex PARTY_NAME_REGEX	   { "[a-zA-Z](\\s?[0-9a-zA-Z]){2,}", REGEX_FLAGS };
inline const std::regex GUILD_NAME_REGEX	   { "[a-zA-Z](\\s?[0-9a-zA-Z]){2,}", REGEX_FLAGS };

#undef REGEX_FLAGS