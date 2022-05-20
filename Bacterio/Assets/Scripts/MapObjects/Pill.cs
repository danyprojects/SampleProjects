using Bacterio.Databases;

namespace Bacterio.MapObjects
{
    public class Pill : Block
    {
        public Databases.PillDbId _dbId;
        public float _spawnTime;
        public float _lifeTime;
        public Animators.PillAnimator _animator;

        public void Configure(float spawnTime, float duration)
        {
            _spawnTime = spawnTime;
            _lifeTime = spawnTime + duration;
        }
    }
}
