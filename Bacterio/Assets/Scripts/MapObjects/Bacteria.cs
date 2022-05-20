using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Bacterio.MapObjects
{
    public sealed class Bacteria : Network.NetworkArrayObject
    {
        public struct MovementData
        {
            public List<Vector2> path;
            public byte currentIndex;
            public int nextWanderTime;

            public Vector3 Target { get { return path[currentIndex]; } }
            public bool IsAtEnd { get { return currentIndex == path.Count - 1; } }
        }

        public static event Action<Bacteria> BacteriaSpawned;
        public static event Action<Bacteria> BacteriaDespawned;

        public Animators.BacteriaAnimator _animator = null;

        //Bullet info
        public Databases.ShootAttribute _shootingAttribute;
        public Databases.MovementAttribute _bulletMovementAttribute;
        public Databases.HitAttribute _bulletHitAttribute;

        //Only the host should use this
        public MovementData _movement;


        public bool IsAlive { get { return _currentHp > 0; } }

        //Status. As long as these don't change anything in the UI, we can make do with leaving them as public. Otherwise we need to make them private with getters like the cells
        [SyncVar(hook = nameof(HookCurrentHpChanged))]
        public int _currentHp;
        public int _baseMaxHp;
        public int _baseShootSpeed;
        public short _baseMoveSpeed;
        public short _baseAtk;
        public short _baseDef;

        public short _hpMult;
        public short _shootSpeedMult;
        public short _moveSpeedMult;
        public short _atkMult;
        public short _defMult;

        //Flags
        public bool _isShooting;
        public bool _isMoving;
        public bool _canMove;
        public bool _isInvincible;
        public bool _isKnockImmune;

        public int _nextShootTimeMs;

        public override void OnStartClient()
        {
            base.OnStartClient();

            BacteriaSpawned?.Invoke(this);
        }

        public override void OnStopClient()
        {
            BacteriaDespawned?.Invoke(this);

            base.OnStopClient();
        }

        public void Configure(/*Dbid*/)
        {
            _nextShootTimeMs = 0;
            _movement.path = new List<Vector2>();
            _movement.nextWanderTime = 0;

            //Init status. For now these are default. Later we should get them from a database
            _baseMaxHp = 100;
            _baseMoveSpeed = 1;
            _baseShootSpeed = 1;
            _baseAtk = 10;
            _baseDef = 5;

            _hpMult = 0;
            _moveSpeedMult = 0;
            _shootSpeedMult = 0;
            _atkMult = 0;
            _defMult = 0;

            //Flags
            _isShooting = false;
            _isMoving = false;
            _canMove = true;
            _isInvincible = false;

            //Stats that rely on base and multipliers
            _currentHp = GetMaxHp();
        }

        private void HookCurrentHpChanged(int oldHp, int newHp)
        {
            WDebug.Log("Bacteria ID: " + _uniqueId.index + " remaining HP: " + newHp);
        }


        public int GetAttackPower()
        {
            return _baseAtk + _baseAtk * _atkMult / 100; //temporarily until proper stat initialization is done
        }

        public int GetDefense()
        {
            return _baseDef + _baseDef * _defMult / 100;
        }

        public int GetMaxHp()
        {
            return _baseMaxHp + _baseMaxHp * _hpMult / 100;
        }
    }
}
