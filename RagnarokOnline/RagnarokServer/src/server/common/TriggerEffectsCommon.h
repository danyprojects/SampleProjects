#pragma once
#include <cstdint>

enum class OperationType : int8_t
{
    Apply = 1,
    Revert = -1 //this way we can use it by multiplying it with constants
};

//Flags that will be used to evaluate if a trigger effect meets the conditions to be triggered
enum class TriggerConditions : uint8_t
{
    ShortRange  = 1 << 0,    //Trigger on melee attacks
    LongRange   = 1 << 1,    //Trigger on ranged attacks
    Weapon      = 1 << 2,    //Trigger on weapon skills
    Magic       = 1 << 3,    //Trigger on magic skills
    Misc        = 1 << 4,    //Trigger on misc skills
    Skill       = 1 << 5,    //Trigger on skills
    Normal      = 1 << 6,    //Trigger on normal attacks

    Default     = ShortRange | LongRange | Weapon | Normal,
    All         = 0xFF,

    None        = 0
};
inline TriggerConditions operator|(TriggerConditions triggerA, TriggerConditions triggerB)
{
    return static_cast<TriggerConditions>(static_cast<uint8_t>(triggerA) | static_cast<uint8_t>(triggerB));
}

inline TriggerConditions operator&(TriggerConditions triggerA, TriggerConditions triggerB)
{
    return static_cast<TriggerConditions>(static_cast<uint8_t>(triggerA) & static_cast<uint8_t>(triggerB));
}


//Flags that will be passed as an int parameter to trigger effect functions
enum class AttackTriggerFlags : uint8_t
{    
    Self        = 1 << 0,    //Trigger at self
    Target      = 1 << 1,    //Trigger at target
    ShortRange  = 1 << 2,    //Trigger on short range attacks
    LongRange   = 1 << 3,    //Trigger on long range attacks
    Weapon      = 1 << 4,    //Trigger on weapon skills
    Magic       = 1 << 5,    //Trigger on magic attacks
    Misc        = 1 << 6,    //Trigger on misc skills
    Skill       = 1 << 7,    //Trigger on skill attacks

    All         = 0xFF
};
inline AttackTriggerFlags operator|(AttackTriggerFlags triggerA, AttackTriggerFlags triggerB)
{
    return static_cast<AttackTriggerFlags>(static_cast<uint8_t>(triggerA) | static_cast<uint8_t>(triggerB));
}

inline AttackTriggerFlags operator&(AttackTriggerFlags triggerA, AttackTriggerFlags triggerB)
{
    return static_cast<AttackTriggerFlags>(static_cast<uint8_t>(triggerA) & static_cast<uint8_t>(triggerB));
}

enum class AutoTriggerType : uint8_t
{
    OnAttack = 0,   //when unit attacks with weapon
    OnAttacked,     //when unit gets hit
    OnSkill,        //when unit uses a skill
    
    Last = OnSkill
};

enum class TriggerEffectRace : int32_t
{
    //Base races
    Formless    = 1 << 0,
    Undead      = 1 << 1,
    Brute       = 1 << 2,
    Plant       = 1 << 3,
    Insect      = 1 << 4,
    Fish        = 1 << 5,
    Demon       = 1 << 6,
    DemiHuman   = 1 << 7,
    Human       = 1 << 8,
    Angel       = 1 << 9,
    Dragon      = 1 << 10,

    //Trigger effect only races
    Player      = 1 << 11,
    Boss        = 1 << 12,
    NonBoss     = 1 << 13,
    NonDemiHuman= 1 << 14,

    All         = 0x7FFFFFFF
};

enum class TriggerEffectElement : int32_t
{
    //Base elements
    Neutral     = 1 << 0,
    Water       = 1 << 1,
    Earth       = 1 << 2,
    Fire        = 1 << 3,
    Wind        = 1 << 4,
    Poison      = 1 << 5,
    Holy        = 1 << 6,
    Dark        = 1 << 7,
    Ghost       = 1 << 8,
    Undead      = 1 << 9,

    All         = 0x7FFFFFFF
};

enum class TriggerEffectTribes : int32_t
{
    Goblin      = 1 << 0,
    Kobold      = 1 << 1,
    Orc         = 1 << 2,
    Golem       = 1 << 3,
    Guardian    = 1 << 4,
    Ninja       = 1 << 5,
    Scaraba     = 1 << 6,
    Turtle      = 1 << 7,

    All         = 0x7FFFFFFF
};

enum class TriggerEffect1ArgId : uint8_t
{
    //Base stats, all flat bonuses
    bStr = 0,
    bAgi,
    bVit,
    bInt,
    bDex,
    bLuk,
    bAgiVit,
    bAgiDexStr,
    bAllStats,

    //Hp / SP
    bMaxHP,             //+ flat Max HP
    bMaxHPRate,         //+ % Max HP 
    bMaxSP,             //+ flat Max SP 
    bMaxSPRate,         //+ % Max SP 
    bHPrecovRate,       //+ % Natural HP recovery ratio 
    bSPrecovRate,       //+ % Natural SP recovery ratio
    bUseSPRate,         //+ % SP consumption 
    bNoRegen,           //Stops regeneration for HP OR SP (1 = HP, 2 = SP)
    bHPDrainValue,      //+ Flat Heal HP with each weapon attack
    bSPDrainValue,      //+ Flat Heal SP with each weapon attack
    bHPDrainRate,       //+ % Heal HP with each weapon attack
    bSPDrainRate,       //+ % Heal SP with each weapon attack
    bHPGainValue,       //+ flat Heal HP when killing any monster with physical attack
    bSPGainValue,       //+ flat Heal SP when killing any monster with physical attack
    bMagicHPGainValue,  //+ flat Heal HP when killing any monster with magical attack
    bMagicSPGainValue,  //+ flat Heal SP when killing any monster with magical attack

    //Attack / Def
    bAtk,               //+ Flat Atk
    bAtk2,              //+ Flat Atk2
    bAtkRate,           //+ % Attack power
    bBaseAtk,           //+ Flat basic attack power 
    bDef,               //+ Flat equipment defense 
    bDef2,              //+ Flat vit based defense 
    bDefRate,           //+ % equipment defense 
    bDef2Rate,          //+ % vit based defense 
    bNearAtkDef,        //+ % damage reduction vs melee physical attacks
    bLongAtkDef,        //+ % damage reduction vs ranged physical attacks
    bMagicAtkDef,       //+ % damage reduction vs magical attacks
    bMiscAtkDef,        //+ % damage reduction vs misc attacks (traps, falcon, ...)
    bCriticalDef,       //+ % reduced chance of being hit by critical
    bLongAtkRate,       //+ % ranged attack damage
    bCritAtkRate,       //+ % critical damage
    bNoWeaponDamage,    //% chance to nullify physical damage
    bNoMagicDamage,     //% chance to nullify magical effect (Attack, Healing, Support spells are all blocked)
    bNoMiscDamage,      //+ % reduction to received misc damage
    bAtkEle,            //Gives the player attack the element
    bDefEle,            //Gives the player armor the element
    bDefRatioAtkEle,    //Deals more damage against enemies with higher defense of element 
    bDefRatioAtkRace,   //Deals more damage against enemies with higher defense of race 
    bDefRatioAtkClass,   //Deals more damage against enemies of class

    //Magic attack / def
    bMatk,              //+ Flat matk
    bMatkRate,          //+ % matk
    bMdef,              //+ Flat equipment mdefense 
    bMdef2,             //+ Flat int based mdefense 
    bMdefRate,          //+ % Equipment mdefense 
    bMdef2Rate,         //+ % Int based mdefense 

    //Heal
    bHealPower,         //+ % heal amount of all heal skills used by SELF on SELF
    bHealPower2,        //+ % heal amount of all heal skills used by OTHERS on SELF
    bAddItemHealRate,   //+ % heal amount for healing items

    //Skill cast
    bCastRate,          //+ % all skills cast time 
    bFixedCastRate,     //+ % all skills fixed cast time
    bFixedCast,         //+ Flat all skills fixed cast time in milliseconds
    bVariableCastRate,  //+ % all skills variable cast time
    bVariableCast,      //+ Flat all skills variable cast time in milliseconds
    bNoCastCancel,      //Casting cannot be interrupted when hit (does not work in GvG | ignores no argument)
    bNoCastCancel2,     //Casting cannot be interrupted when hit (works in GvG | ignores argument)
    bDelayRate,         //+ % all skills delay

    //Other stats
    bHit,               //+ flat hit 
    bHitRate,           //+ % hit 
    bCritical,          //+ flat crit 
    bCriticalRate,      //+ % crit 
    bCriticalLong,      //+ flat crit for long range physical attacks
    bFlee,              //+ flat flee 
    bFleeRate,          //+ % flee 
    bFlee2,             //+ flat perfect dodge 
    bFlee2Rate,         //+ % perfect dodge 
    bPerfectHitRate,    //Perfect hit rate %. Only the highest of these bonuses will apply
    bPerfectHitAddRate, //+ % Perfect hit rate. Adds to perfect hit rate regardless of how many there are active
    bSpeedRate,         //Move speed %. Only the highest of these bonuses will apply
    bSpeedAddRate,      //+ % Move speed. Adds to move speed regardless of how many there are active
    bAspd,              //+ flat aspd 
    bAspdRate,          //+ % Aspd 
    bAtkRange,          //+ flat attack range 
    bAddMaxWeight,      //+ flat max weight in units of 0.1

    //Ignore def
    bIgnoreDefRace,     //Ignore defense from race 
    bIgnoreMdefRace,    //Ignore magic defense from race
    bIgnoreDefEle,      //Ignore defense from element 
    bIgnoreMdefEle,     //Ignore magic defense from element
    bIgnoreMdefRate,    //Ignore % of target's magic defense
    bIgnoreDefClass,    //Ignore all defense of monster / player class 
    bIgnoreMdefClass,   //Ignore all magic defense of monster / player class 

    //Damage reflect
    bMagicDamageReturn, //+ % chance to reflect TARGETTED magic spells back to the caster
    bShortWeaponDamageReturn, //Reflects % of received melee damage back to attacker 
    bLongWeaponDamageReturn, //Reflects % of received ranged damage back to attacker 

    //Strip / break equipement. If no variable is mentioned, it is ignored
    bUnstripable,       //Gear cannot be taken off via strip
    bUnstripableWeapon, //Weapon cannot be taken off via strip
    bUnstripableArmor,  //Armor cannot be taken off via strip
    bUnstripableHelm,   //Helm cannot be taken off via strip
    bUnstripableShield, //Shield cannot be taken off via strip
    bUnbreakable,       //+ % reduced break chance of equipped gears
    bUnbreakableGarment,//Garment cannot be broken by any means
    bUnbreakableWeapon, //Weapon cannot be broken by any means
    bUnbreakableArmor,  //Armor cannot be broken by any means
    bUnbreakableHelm,   //Helm cannot be broken by any means
    bUnbreakableShield, //Shield cannot be broken by any means
    bUnbreakableShoes,  //Shoes cannot be broken by any means
    bBreakWeaponRate,   //+ % /100 chance to break enemy weapon while attacking (stacks)
    bBreakArmorRate,    //+ % /100 chance to break enemy armor while attacking (stacks)

    //Monster related
    bAddMonsterDropChainItem,   //Gets an item of "chain" when any monster is killed

    //Misc effects (if arg is not mentioned, it is ignored)
    bDoubleRate,        //+ % chance of double attack with any weapon. Only highest value of same effect applies
    bDoubleAddRate,     //+ % chance of double attack with any weapon. Stacks
    bSplashRange,       //+ flat splash attack (square) radius. Only highest value of same effect applies
    bSplashAddRange,    //+ flat splash attack (square) radius. Stacks
    bClassChange,       //+ % / 100 chance with normal attack to transform monster
    bAddStealRate,      //+ % / 100 chance for Steal skil to succeed
    bRestartFullRecover,//Fully heal HP and SP when revived
    bNoSizeFix,         //Nullify reduction in damage inflicted on monsters resulting from monster's size
    bNoGemStone,        //Skills requiring Gemstones do no require them (Hocus Pocus will still require 1 Yellow Gemstone)
    bIntravision,       //Always see Hiding and Cloaking players/mobs
    bNoKnockback,       //Player cannot be knocked back by enemy skills
    bPerfectHide,       //Hidden / cloacked character cannot be detected by monsters with "detector"

    Last = bPerfectHide
};

enum class TriggerEffect2ArgId : uint8_t
{
    //Hp / SP
    bHPRegenRate = 0,   //+ flat HP every t milliseconds                                - HP - milliseconds
    bHPLossRate,        //- flat HP every t milliseconds                                - HP - milliseconds
    bSPRegenRate,       //+ flat SP every t milliseconds                                - HP - milliseconds
    bSPLossRate,        //- flat SP every t milliseconds                                - HP - milliseconds
    bSkillUseSP,        //- flat SP consumption of skill                                - Skill ID - amount flat
    bSkillUseSPRate,    //-  %   SP consumption of skill                                - Skill ID - amount %
    bHPDrainValue,      //+ Flat Heal / drain HP with each weapon attack                - amount - flag: 0 - heal, != 0 drain
    bHPDrainRate,       //+ % chance to drain HP % when attacking                       - chance % / 10 - drain %
    bSPDrainValue,      //+ Flat Heal / drain SP with each weapon attack                - amount - flag: 0 - heal, != 0 drain
    bSPDrainRate,       //+ % chance to drain SP % when attacking                       - chance % / 10 - drain %
    bHPDrainValueRace,  //+ Flat HP heal when attacking race with weapon attack         - race - flat amount
    bSPDrainValueRace,  //+ Flat SP heal when attacking race with weapon attack         - race - flat amount
    bHPVanishRate,      //+ % chance of decreasing enemy HP by % when attacking         - chance % / 10 - amount %
    bSPVanishRate,      //+ % chance of decreasing enemy SP by % when attacking         - chance % / 10 - amount %
    bHPGainRaceAttack,  //+ Flat HP heal every hit when attacking race                  - race - amount flat
    bSPGainRaceAttack,  //+ Flat SP heal every hit when attacking race                  - race - amount flat
    bSPGainRace,        //+ Flat SP when killing a monster of race                      - race - amount flat

    //Attack / Def
    bSkillAtk,          //+ % increased skill damage                                    - Skill ID - amount %
    bWeaponAtk,         //+ flat ATK when weapon type is equipped                       - Weapon type - flat ATK
    bWeaponAtkRate,     //+ % damage to weapon attacks when weapon type is equipped     - Weapon type - % damage

    //Heal
    bSkillHeal,         //+ % heal amount of skill                                      - Skill ID - amount %
    bSkillHeal2,        //+ % heal amount when healed by skill                          - Skill ID - amount %
    bAddItemHealRate,   //+ % heal amount for item                                      - Item ID  - amount %
    bAddItemGroupHealRate,   //+ % heal amount for item                                 - Item Group  - amount %

    //Skill cast
    bCastRate,          //+ % specific skill cast time                                  - Skill ID - amount %
    bFixedCastRate,     //+ % specific skill fixed cast time                            - Skill ID - amount %
    bSkillFixedCast,    //+ flat specific skill fixed cast time in milliseconds         - Skill ID - amount flat
    bVariableCastRate,  //+ % specific skill variable cast time                         - Skill ID - amount %
    bSkillVariableCast, //+ flat specific skill variable cast time  in milliseconds     - Skill ID - amount flat
    bSkillCooldown,     //+ flat specific skill cooldown in milliseconds                - Skill ID - amount flat

    //Damage
    bAddSize,           //+ % physical damage against size                              - Size - amount %
    bMagicAddSize,      //+ % magical damage against size                               - Size - amount %
    bSubSize,           //+ % damage reduction from size                                - Size - amount %
    bAddRaceTolerance,  //+ % resistance against race                                   - Race - amount %
    bAddRace,           //+ % physical damage against race                              - Race - amount %
    bMagicAddRace,      //+ % Magical  damage against race                              - Race - amount %
    bSubRace,           //+ % Damage reduction from race                                - Race - amount %
    bCriticalAddRace,   //+ flat critical rate against race                             - Race - amount flat
    bAddRace2,          //+ % all damage against tribe                                  - tribe - amount %
    bSubRace2,          //+ % all damage reduction from tribe                           - tribe - amount %
    bAddEle,            //+ % physical damage against element                           - element - amount %
    bSubEle,            //+ % damage reduction from element                             - element - amount %
    bMagicAddEle,       //+ % magical  damage against element                           - element - amount %
    bMagicAtkEle,       //+ % magical damage of element                                 - element - amount %
    bAddDamageMonster,    //+ % extra physical damage against monster id                  - monster id - amount %
    bAddMagicDamageMonster,//+ % extra magical damage against monster id                  - monster id - amount %
    bAddDefMonster,      //+ % physical damage reduction from monster id               - monster id - amount %
    bAddMdefMonster,     //+ % magical damage reduction from monster id                - monster id - amount %
    bSubClass,          //+ % damage reduction from monster of class                    - monster class - amount %
    bAddClass,          //+ % damage against monster of class                           - monster class - amount %

    //Ignore defense
    bIgnoreDefRaceRate,      //Ignore % of target's defense if target is of race            - Race - amount %
    bIgnoreMdefRaceRate,     //Ignore % of target's magic defense if target is of race      - Race - amount %
    bIgnoreDefClassRate,     //Ignore % of target's defense if target is of class           - Class - amount %
    bIgnoreMdefClassRate,    //Ignore % of target's magic defense if target is of class     - Class - amount %

    //Experience
    bExpAddRace,         //+ % experience from enemies of race                          - Race - amount %

    //Status related bonuses
    bResEff,            //+ % resistance to effect                                      - effect - amount % / 100
    bAddEff,            //+ % chance to cause effect to target when attacking           - effect - amount % / 100
    bAddEff2,           //+ % chance to cause effect on self when attacking             - effect - amount % / 100
    bAddEffWhenHit,     //+ % chance to cause effect to enemy when hit by physical dmg  - effect - amount % / 100
    bComaRace,          //+ % chance to cause coma when attacking race with weapon atk  - race   - amount % / 100
    bComaEle,           //+ % chance to cause coma when attacking element with weapon atk - element - amount % / 100

    //Monster related
    bAddRaceDropItem,//+ % chance of dropping item when killing any monster          - item id - chance % / 100
    bAddRaceDropChainItem,   //Gets an item of "chain" when monster of race is killed - chain - race
    bAddMonsterDropItemGroup,   //+ % chance of geting an item of "group"               - item group - chance % /100
    bGetZenyNum,        //% chance of gaining 1~X zeny when killing a monster (only highest chance of same effect apply) - max zeny - chance %
    bAddGetZenyNum,     //+ % chance of gaining 1~X zeny when killing a monster (stackable) - max zeny - chance %.  If max zeny < 0 then max zeny = -value*monster_level

    //Misc effects
    bAddSkillBlow,      //Knocks the target n cells when using specific skill           - skill id - n cells

    Last = bAddSkillBlow
};

enum class TriggerEffect3ArgId : uint8_t
{
    //HP / SP
    bHPVanishRate = 0,  //+ % chance of decreasing enemy HP by % when attacking         - chance % / 10 - amount % - BF
    bSPVanishRate,      //+ % chance of decreasing enemy SP by % when attacking         - chance % / 10 - amount % - BF

    //Damage
    bAddEle,            //+ % physical damage against element                           - element - amount % - bonus flag
    bSubEle,            //+ % damage reduction from element                             - element - amount % - bonus flag

    //Status related bonuses
    bAddEff,            //+ % chance to cause effect to target when attacking for target ATF - effect - amount % /100 - ATF
    bAddEffOnSkill,     //+ % chance to cause effect on enemy when using skill          - Skill - effect - amount % / 100
    bAddEffWhenHit,     //+ % chance to cause effect to enemy when hit by physical dmg  - effect - amount % / 100 - ATF
    bAutoSpell,         //+ % chance of auto cast skill at level when attacking         - skill - level - amount % / 10 
    bAutoSpellWhenHit,  //+ % chance of auto cast skill at level when hit by direct atk - skill - level - amount % / 10
    bSPDrainRate,       //+ % chance to either gain SP equivalent to % damage dealt OR drain SP from the enemy - chance % / 10 - damage % - Flag: 0- Gain SP, 1- Drain SP
    bHPDrainValueRace,  //% chance to receive % of damage dealt as HP from race with weapon attack - race - amount % / 10 - damage %
    bSPDrainValueRace,  //% chance to receive % of damage dealt as SP from race with weapon attack - race - amount % / 10 - damage %

    //Monster related
    bAddMonsterDropItem,  //+ % chance of dropping item when killing specific monster     - item id - monster id - chance % / 100
    bAddRaceDropItem,    //+ % chance of dropping item when killing monster of race      - item id - race - chance % / 100 OR if negative: chance= -arg*(killed_mob_level/10)+1
    
    Last = bAddRaceDropItem
};

enum class TriggerEffect4ArgId : uint8_t
{
    //Attack / Def
    bSetDefRace = 0,    //`r`,`n`,`t`,`y`;    | Set DEF  to `y` of an enemy of race `r` at `n`/100% for `t` milliseconds with normal attack
    bSetMDefRace,       //`r`,`n`,`t`,`y`;    | Set MDEF to `y` of an enemy of race `r` at `n`/100% for `t` milliseconds with normal attack

    //Status related bonuses
    bAddEff,            //For X ms + % chance to cause effect to target when attacking for target ATF - effect - amount % /100 - ATF - milliseconds
    bAddEffOnSkill,     //+ % chance to cause effect on enemy when using skill          - Skill - effect - amount % / 100 - ATF

    //Auto cast bonuses
    bAutoSpellOnSkill,  //+ % chance to auto cast skill at level while using skill      - Used skill - Auto skill - Skill level - amount % / 10
    bAutoSpell,         //+ % chance of auto cast skill at level when attacking         - skill - level - amount % / 10 - Flag
                        //Flag: 0 = cast on self. 1 = cast on enemy, not on self. 2 = use random skill lvl in 1 to skill lvl. 3 = both (random lv on enemy)
    bAutoSpellWhenHit,  //+ % chance of auto cast skill at level when hit by direct atk - skill - level - amount % / 10 - Flag
                        //Flag: 0 = cast on self. 1 = cast on enemy, not on self. 2 = use random skill lvl in 1 to skill lvl. 3 = both (random lv on enemy)
    
    Last = bAutoSpellWhenHit
};

enum class TriggerEffect5ArgId : uint8_t
{
    //Auto cast bonuses
    bAutoSpellOnSkill = 0,  //+ % chance to auto cast skill at level while using skill      - Used skill - Auto skill - Skill level - amount % / 10 - Bitfield Flags
                            //Bitfield: (&1: Forces the skill to be casted on self, rather than on the target of used skill. &2: Random skill level between 1 and skill lvl is chosen.
    bAutoSpell,             //+ % chance of auto casting skill at level when attacking      - skill - level - amount % / 10 - BF - Flag
                            //Flag: 0 = cast on self. 1 = cast on enemy, not on self. 2 = use random skill lvl in 1 to skill lvl. 3 = both (random lv on enemy)
    bAutoSpellWhenHit,      //+ % chance of auto cast skill at level when hit by direct atk - skill - level - amount % / 10 - BF - Flag 
                            //Flag: 0 = cast on self. 1 = cast on enemy, not on self. 2 = use random skill lvl in 1 to skill lvl. 3 = both (random lv on enemy)
    Last = bAutoSpellWhenHit
};