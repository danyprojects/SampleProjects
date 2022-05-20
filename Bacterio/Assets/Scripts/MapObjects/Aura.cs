using UnityEngine;
using Bacterio.Databases;

namespace Bacterio.MapObjects
{
    public class Aura : Block
    {
        public AuraDbId _dbId;
        public float _radius;
        public BlockType _ownerBlockType;
        public MonoBehaviour _owner;
        public Animators.AuraAnimator _animator;
    }
}
