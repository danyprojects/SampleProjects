#pragma once

#include <cstdint>

typedef int16_t NpcId;

enum class NpcHandlerId : uint16_t
{
	general_228 = 10, //MailBox
	gonryun_0 = 0, //Kunlun Envoy#gon4
	gonryun_1 = 1, //Jian Chung Xun#gon
	gonryun_2 = 2, //Liang Zhun Bu#gon
	gonryun_3 = 3, //Qian Yuen Shuang#gon
	gonryun_4 = 4, //Jing Wen Zhen#gon
	gonryun_5 = 5, //Gatekeeper#gon
	gonryun_6 = 6, //Gatekeeper#gon2
	gonryun_7 = 7, //Soldier#gon
	gonryun_8 = 8, //Guidev#gon
	gonryun_9 = 9, //kaf_gonryun
	gonryun_10 = 11, //Iron man#gnp
	gonryun_11 = 12, //Kunlun Guide#gon
	gonryun_12 = 13, //Girl##gnbs1
	gonryun_13 = 14, //Stranger#gnbs
	gonryun_14 = 15, //Han Ran Jiao#gon
	gonryun_15 = 16, //Kunlun Guide#01gonryun
	gonryun_16 = 17, //Signpost#Kunlun Dungeon

	Last = gonryun_16
};
