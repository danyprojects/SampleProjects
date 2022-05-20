using System;

namespace Bacterio.MapObjects
{
    public abstract class Block : UnityEngine.MonoBehaviour
    {
        public struct UniqueId
        {
            public short arrayIndex;
            public short tag;

            public UniqueId(short arrayIndex, short tag)
            {
                this.arrayIndex = arrayIndex;
                this.tag = tag;
            }
        }

        public BlockType _blockType;
        public UniqueId _uniqueId;
    }
}
