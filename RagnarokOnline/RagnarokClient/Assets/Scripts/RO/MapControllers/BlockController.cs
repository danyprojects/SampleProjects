using RO.Common;
using RO.MapObjects;
using UnityEngine;

namespace RO
{
    public partial class GameController : MonoBehaviour
    {
        private partial class BlockController
        {
            private Block[] _blocks;

            public Character FirstCharacter { get; private set; }
            public Monster FirstMonster { get; private set; }

            public Item FirstItem { get; private set; }

            public BlockController()
            {
                _blocks = new Block[Constants.MAX_BLOCK_COUNT];

                FirstCharacter = null;
                FirstMonster = null;
                FirstItem = null;
            }

            public Block GetBlock(int sessionId)
            {
                return _blocks[sessionId];
            }

            public Character GetChacter(int sessionId)
            {
                return (Character)_blocks[sessionId];
            }

            public Character CreateCharacter(int sessionId)
            {
                Character character = new Character(sessionId);
                _blocks[sessionId] = character;
                character.IsEnabled = true;

                character._nextCharacter = FirstCharacter;
                character._previousCharacter = null;

                if (FirstCharacter != null)
                    FirstCharacter._previousCharacter = character;
                FirstCharacter = character;
                return character;
            }

            public void FreeAndCleanupCharacter(Character character)
            {
                character.Cleanup();
                _blocks[character.SessionId] = null;
            }

            public void CleanupCharacter(Character character)
            {
                if (FirstCharacter == character)
                    FirstCharacter = character._nextCharacter;

                if (character._previousCharacter != null)
                    character._previousCharacter._nextCharacter = character._nextCharacter;
                if (character._nextCharacter != null)
                    character._nextCharacter._previousCharacter = character._previousCharacter;

                character.Cleanup();
            }

            public void FreeCharacterId(Character character)
            {
                _blocks[character.SessionId] = null;
            }

            public void ClearCharacters()
            {
                while (FirstCharacter != null)
                {
                    FirstCharacter.Cleanup();
                    int sessionId = FirstCharacter.SessionId;
                    FirstCharacter = FirstCharacter._nextCharacter;
                    _blocks[sessionId] = null;
                }
            }

            public Monster GetMonster(int sessionId)
            {
                return (Monster)_blocks[sessionId];
            }

            public Monster CreateMonster(int sessionId, Databases.MonsterIDs monsterDbId)
            {
                Monster monster = new Monster(sessionId, monsterDbId);
                _blocks[sessionId] = monster;
                monster.IsEnabled = true;

                monster._nextMonster = FirstMonster;
                monster._previousMonster = null;

                if (FirstMonster != null)
                    FirstMonster._previousMonster = monster;
                FirstMonster = monster;
                return monster;
            }

            public void FreeAndCleanupMonster(Monster monster)
            {
                monster.Cleanup();
                _blocks[monster.SessionId] = null;
            }

            public void CleanupMonster(Monster monster)
            {
                if (FirstMonster == monster)
                    FirstMonster = monster._nextMonster;

                if (monster._previousMonster != null)
                    monster._previousMonster._nextMonster = monster._nextMonster;
                if (monster._nextMonster != null)
                    monster._nextMonster._previousMonster = monster._previousMonster;

                monster.Cleanup();
            }

            /// <summary>
            /// Will remove the monster from the block list, freeing the session id for reuse
            /// </summary>
            public void FreeMonsterId(Monster monster)
            {
                _blocks[monster.SessionId] = null;
            }

            public void ClearMonsters()
            {
                while (FirstMonster != null)
                {
                    FirstMonster.Cleanup();
                    int sessionId = FirstMonster.SessionId;
                    FirstMonster = FirstMonster._nextMonster;
                    _blocks[sessionId] = null;
                }
            }


            public Item GetItem(int sessionId)
            {
                return (Item)_blocks[sessionId];
            }

            public Item CreateItem(int sessionId, Databases.ItemIDs itemDbId)
            {
                Item item = new Item(sessionId, itemDbId);
                _blocks[sessionId] = item;
                item.IsEnabled = true;

                item._nextItem = FirstItem;
                item._previousItem = null;

                if (FirstItem != null)
                    FirstItem._previousItem = item;
                FirstItem = item;
                return item;
            }

            public void DeleteItem(Item item)
            {
                if (FirstItem == item)
                    FirstItem = item._nextItem;

                if (item._previousItem != null)
                    item._previousItem._nextItem = item._nextItem;
                if (item._nextItem != null)
                    item._nextItem._previousItem = item._previousItem;

                item.Cleanup();
                _blocks[item.SessionId] = null;
            }

            public void ClearItems()
            {
                while (FirstItem != null)
                {
                    FirstItem.Cleanup();
                    int sessionId = FirstItem.SessionId;
                    FirstItem = FirstItem._nextItem;
                    _blocks[sessionId] = null;
                }
            }
        }
    }
}
