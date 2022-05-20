using RO;
using RO.MapObjects;
using RO.Media;
using UnityEngine;

namespace Tests
{
    public class MonsterAnimatorControllerTest : MonoBehaviour
    {
        public enum Directions : int
        {
            BottomLeft = 1,
            TopLeft = 3,
            TopRight = 5,
            BottomRight = 7,

            None
        }

        public enum Animations
        {
            Idle,
            Walking,
            Attacking,
            Casting,
            ReceiveDmg,
            Dead,
            None
        }

        public int NumOfMonsters = 1, NumOfCachedMonsters = 5;
        public Animations Animation = Animations.Idle;
        public RO.Databases.MonsterSpriteIDs Monster = RO.Databases.MonsterSpriteIDs.Morroc;
        public Directions Direction = Directions.BottomLeft;
        public int movementSpeed = 150, attackSpeed = 170;
        public float startDelay = 0;
        public Texture Palette = null;
        public Color Color = Color.black;

        private MonsterAnimatorController[] _monsterAnimators;
        private Monster.Direction _direction = new Monster.Direction(0);

        public void Start()
        {
            GameObject cached = ObjectPoll.MonsterAnimatorControllersPoll;
            for (int i = 0; i < NumOfCachedMonsters; i++)
                ObjectPoll.MonsterAnimatorControllersPoll = Instantiate(cached);
            Destroy(cached);

            _monsterAnimators = new MonsterAnimatorController[NumOfMonsters];
            for (int i = 0; i < _monsterAnimators.Length; i++)
            {
                _monsterAnimators[i] = ObjectPoll.MonsterAnimatorControllersPoll.GetComponent<MonsterAnimatorController>();
                _monsterAnimators[i].AnimateMonster(null);
                _monsterAnimators[i].PlayIdleAnimation();
                _monsterAnimators[i].transform.SetParent(transform, false);
                _monsterAnimators[i].transform.position = new Vector3(i * 20, 0, 0);
            }

            RO.Common.Globals.Time = Time.time;
        }

        public void Update()
        {
            RO.Common.Globals.Time += Time.deltaTime;

            if (Monster != RO.Databases.MonsterSpriteIDs.None)
            {
                for (int i = 0; i < _monsterAnimators.Length; i++)
                    _monsterAnimators[i].AnimateMonster(null);
                Monster = RO.Databases.MonsterSpriteIDs.None;
            }
            if (Direction != Directions.None)
            {
                _direction.BodyCamera = (int)Direction;
                for (int i = 0; i < _monsterAnimators.Length; i++)
                    _monsterAnimators[i].ChangedDirection();
                Direction = Directions.None;
            }
            if (Animation != Animations.None)
            {
                for (int i = 0; i < _monsterAnimators.Length; i++)
                    switch (Animation)
                    {
                        case Animations.Idle: _monsterAnimators[i].PlayIdleAnimation(); break;
                        case Animations.Walking: _monsterAnimators[i].PlayWalkAnimation(movementSpeed); break;
                        case Animations.Attacking: _monsterAnimators[i].PlayAttackAnimation(attackSpeed); break;
                        case Animations.ReceiveDmg: _monsterAnimators[i].PlayReceiveDamageAnimation(); break;
                        case Animations.Casting: _monsterAnimators[i].PlayCastAnimation(startDelay); break;
                        case Animations.Dead: _monsterAnimators[i].PlayDeadAnimation(false); break;
                    }
                Animation = Animations.None;
            }
            if (Palette != null)
            {
                for (int i = 0; i < _monsterAnimators.Length; i++)
                    _monsterAnimators[i].SetPalette(Palette);
                Palette = null;
            }
            if (Color != Color.black)
            {
                for (int i = 0; i < _monsterAnimators.Length; i++)
                    _monsterAnimators[i].SetColor(ref Color);
                Color = Color.black;
            }

            for (int i = 0; i < _monsterAnimators.Length; i++)
                _monsterAnimators[i].UpdateAnimations();
        }
    }
}
