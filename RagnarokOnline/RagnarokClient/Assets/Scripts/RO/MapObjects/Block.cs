using System.Collections.Generic;
using UnityEngine;

namespace RO.MapObjects
{
    public enum BlockTypes : int
    {
        Character = 0,
        Homunculus,
        Mercenary,
        Monster,

        LastUnit = Monster,

        Npc,
        Skill,
        Pet,
        Item,
        None
    }

    public abstract class Block
    {

        public readonly int SessionId;
        public readonly BlockTypes BlockType;

        //This so that we can always read the properties without needing to check the block type
        public GameObject gameObject { get; private set; }
        public Transform transform { get; private set; }

        //Will be called by the derived block type
        protected void SetGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
        }

        public abstract bool IsEnabled { get; set; }

        public Vector2Int position = Vector2Int.zero;
        public List<Media.EffectCancelToken> effects;

        public Block(int sessionId, BlockTypes blockType)
        {
            SessionId = sessionId;
            BlockType = blockType;

            effects = new List<Media.EffectCancelToken>(Media.MediaConstants.DEFAULT_UNIT_EFFECTS);
        }

        public void RemoveEffect(Media.EffectCancelToken cancelToken)
        {
            effects.Remove(cancelToken);
        }

        public void ClearEffects()
        {
            for (int i = 0; i < effects.Count; i++)
                effects[i].Cancel();

            effects.Clear();
        }
    }
}
