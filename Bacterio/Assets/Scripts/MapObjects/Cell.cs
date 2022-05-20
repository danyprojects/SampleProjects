using System;
using UnityEngine;
using Mirror;

namespace Bacterio.MapObjects
{
    public sealed class Cell : Network.NetworkPlayerObject
    {
        public struct Status
        {
            //Status with getters already
            public int _currentHp;
            public int _baseMaxHp;
            public int _lives;
            public int _ammunition;
            public int _upgradePoints;

            //Status without getters
            public short _baseMoveSpeed;
            public short _baseAtk;
            public short _baseDef;
            public int _baseReloadTime;
            public int _baseBulletRegenTime;

            public short _hpMult;
            public short _reloadSpeedMult;
            public short _moveSpeedMult;
            public short _atkMult;
            public short _defMult;
        }

        public struct BulletData
        {
            public Databases.BulletDbId _dbId;
            public Databases.ShootAttribute _shootAttribute;
            public Databases.MovementAttribute _bulletMovementAttribute;
            public Databases.HitAttribute _bulletHitAttribute;

            public byte _bulletBurstCounter;
            public byte _bulletBurstMax;
            public byte _ammunitionCost;
        }

        public static event Action<Cell, int> CurrentHpChanged;
        public static event Action<Cell, int> MaxHpChanged;
        public static event Action<Cell, int> AmmunitionChanged;
        public static event Action<Cell, int> UpgradePointsChanged;
        public static event Action<Cell, int> LivesChanged;

        public Animators.CellAnimator _animator = null;
        public Databases.CellDbId _dbId;

        //Flags
        public bool _isShooting;
        public bool _isMoving;
        public bool _canMove;
        public bool _isInvincible;
        public bool _isKnockImmune;

        //Timestamps
        public long _nextShootTimeMs;
        public int _nextBulletRegenMs;
        public long _respawnTimeMs;

        //Other
        private Status _status;
        public short[] _upgrades;
        public Databases.BuffData _buffData;
        public BulletData _bulletData;

        //Cached variables
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;

        //Utility getters and setters
        public bool IsAlive { get { return _status._currentHp > 0; } }

        //Getters and setters for the status notified in network
        public int CurrentHp { get { return _status._currentHp; }  set { NotifyCurrentHpChanged(value); } }
        public int BaseMaxHp { get { return _status._baseMaxHp; } set { NotifyBaseMaxHpChanged(value); } }
        public int Lives { get { return _status._lives; } set { NotifyLivesChanged(value); } }
        public int Ammunition { get { return _status._ammunition; } set { NotifyAmmunitionChanged(value); }}
        public int UpgradePoints { get { return _status._upgradePoints; } set { NotifyUpgradePointsChanged(value); } }
        
        //Getters and setters for the status not notified in the network
        public short MoveSpeedMultiplier { get { return _status._moveSpeedMult; } set { _status._moveSpeedMult = value; } }
        public short AttackMultiplier { get { return _status._atkMult; } set { _status._atkMult = value; } }
        public int BaseReloadSpeed { get { return _status._baseReloadTime; } set { _status._baseReloadTime = value; } }
        public short ReloadSpeedMultiplier { get { return _status._reloadSpeedMult; } set { _status._reloadSpeedMult = value; } }

        //Some QoL getters
        public bool HasSingleAttribute { get { return (_bulletData._shootAttribute & Databases.ShootAttribute.Single) > 0; } }
        public bool HasBurstAttribute { get { return (_bulletData._shootAttribute & Databases.ShootAttribute.Burst) > 0; } }
        public bool HasClusterAttribute { get { return (_bulletData._shootAttribute & Databases.ShootAttribute.Cluster) > 0; } }


        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _upgrades = new short[(int)Databases.ShopItemId.Last + 1];
        }

        public void Configure(Databases.CellDbId dbId)
        {
            _dbId = dbId;

            //get cell data from DB
            ref var cellData = ref GlobalContext.cellDb.GetCellData(_dbId);

            //Init status according to the db
            _status._baseMaxHp = cellData.maxHp;
            _status._currentHp = cellData.maxHp;
            _status._baseMoveSpeed = cellData.moveSpeed;
            _status._baseAtk = cellData.atk;
            _status._baseDef = cellData.def;
            LoadBulletData(cellData.bulletDbId);

            //Initialize multipliers. These are always fixed
            _status._hpMult = 0;
            _status._moveSpeedMult = 0;
            _status._reloadSpeedMult = 0;
            _status._atkMult = 100;
            _status._defMult = 0;

            //Initialize others
            _status._upgradePoints = 0;
            _status._lives = 1;

            //Flags
            _isShooting = false;
            _canMove = true;
            _isInvincible = false;
            _isKnockImmune = false;

            //Initialize timestamps
            _nextShootTimeMs = 0;
            _nextBulletRegenMs = int.MaxValue;
            _respawnTimeMs = 0;
        }

        public void Configure(ref Status status)
        {
            _nextShootTimeMs = 0;
            _nextBulletRegenMs = int.MaxValue;

            //Copy the status into our own
            _status = status;

            // if status had 0 hp, then start with collider and renderer disabled
            _collider.enabled = _status._currentHp > 0;
            _renderer.enabled = _status._currentHp > 0;

            //Flags
            _isShooting = false;
            _canMove = true;
            _isInvincible = false;
            _isKnockImmune = false;
        }

        public Status GetStatus()
        {
            return _status; //Return a copy so it's not modifiable
        }

        //Other
        public void SendAllNotifications()
        {
            MaxHpChanged?.Invoke(this, GetMaxHp());
            CurrentHpChanged?.Invoke(this, _status._currentHp);
            AmmunitionChanged?.Invoke(this, _status._ammunition);
            UpgradePointsChanged?.Invoke(this, _status._upgradePoints);
            LivesChanged?.Invoke(this, _status._lives);
        }

        //**************** Methods to get the status after multipliers
        public int GetAttackPower()
        {
            return _status._baseAtk + _status._baseAtk * _status._atkMult / 100;
        }

        public int GetDefense()
        {
            return _status._baseDef + _status._baseDef * _status._defMult / 100;
        }

        public int GetMaxHp()
        {
            return _status._baseMaxHp + _status._baseMaxHp * _status._hpMult / 100;
        }

        public uint GetShootDelay()
        {
            return (uint)(_status._baseReloadTime - _status._baseReloadTime * _status._reloadSpeedMult / 100);
        }

        public float GetMovementSpeed()
        {
            return _status._baseMoveSpeed + _status._baseMoveSpeed * _status._moveSpeedMult / 100.0f;
        }

        public int GetBulletRegenTime()
        {
            return _status._baseBulletRegenTime;
        }

        //************************************************************************ Client / server rpcs to replace syncVar hooks, they're kinda buggy and weird
        #region ***************************************************  Current Hp
        private void NotifyCurrentHpChanged(int newHp)
        {
            // if cell died, disable collider and renderer so it stops triggering functionality. Otherwise enable
            _respawnTimeMs = newHp > 0 ? 0 : Game.GameContext.serverTimeMs + Constants.DEFAULT_CELL_RESPAWN_TIME;
            _collider.enabled = newHp > 0;
            _renderer.enabled = newHp > 0;

            _status._currentHp = newHp;
            CurrentHpChanged?.Invoke(this, _status._currentHp);
            if (Network.NetworkInfo.IsHost)
                OnCurrentHpUpdateClientRpc(_status._currentHp);
            else
                OnCurrentHpUpdateServerCmd(_status._currentHp);
        }

        [ClientRpc(includeOwner = false)]
        private void OnCurrentHpUpdateClientRpc(int newHp)
        {
            // if cell died, disable collider and renderer so it stops triggering functionality. Otherwise enable
            _collider.enabled = newHp > 0;
            _renderer.enabled = newHp > 0;

            _status._currentHp = newHp;
            CurrentHpChanged?.Invoke(this, _status._currentHp);
        }

        [Command(requiresAuthority = false)]
        private void OnCurrentHpUpdateServerCmd(int newHp)
        {
            NotifyCurrentHpChanged(newHp); //Update and forward to other clients as well
        }
        #endregion

        #region *************************************************** Base max HP
        private void NotifyBaseMaxHpChanged(int newBaseMaxHp)
        {
            _status._baseMaxHp = newBaseMaxHp;
            MaxHpChanged?.Invoke(this, GetMaxHp());
            if (Network.NetworkInfo.IsHost)
                OnBaseMaxHpUpdateClientRpc(_status._baseMaxHp);
            else
                OnBaseMaxHpUpdateServerCmd(_status._baseMaxHp);
        }

        [ClientRpc(includeOwner = false)]
        private void OnBaseMaxHpUpdateClientRpc(int newMaxHp)
        {
            _status._baseMaxHp = newMaxHp;
            MaxHpChanged?.Invoke(this, GetMaxHp());
        }

        [Command(requiresAuthority = false)]
        private void OnBaseMaxHpUpdateServerCmd(int newMaxHp)
        {
            NotifyBaseMaxHpChanged(newMaxHp); //Update and forward to other clients as well
        }
        #endregion

        #region *************************************************** Lives
        private void NotifyLivesChanged(int newLives)
        {
            _respawnTimeMs = newLives < 0 ? Constants.INVALID_CELL_RESPAWN_TIME : _respawnTimeMs;
            _status._lives = newLives;
            LivesChanged?.Invoke(this, _status._lives);
            if (Network.NetworkInfo.IsHost)
                OnLivesUpdateClientRpc(_status._lives);
            else
                OnLivesUpdateServerCmd(_status._lives);
        }

        [ClientRpc(includeOwner = false)]
        private void OnLivesUpdateClientRpc(int newLives)
        {
            _status._lives = newLives;
            LivesChanged?.Invoke(this, _status._lives);
        }

        [Command(requiresAuthority = false)]
        private void OnLivesUpdateServerCmd(int newLives)
        {
            NotifyLivesChanged(newLives); //Update and forward to other clients as well
        }
        #endregion

        #region *************************************************** Ammunition
        private void NotifyAmmunitionChanged(int newAmmunition)
        {
            _status._ammunition = newAmmunition;
            AmmunitionChanged?.Invoke(this, _status._ammunition);
            if (Network.NetworkInfo.IsHost)
                OnAmmunitionUpdateClientRpc(_status._ammunition);
            else
                OnAmmunitionUpdateServerCmd(_status._ammunition);
        }

        [ClientRpc(includeOwner = false)]
        private void OnAmmunitionUpdateClientRpc(int newAmmunition)
        {
            _status._ammunition = newAmmunition;
            AmmunitionChanged?.Invoke(this, _status._ammunition);
        }

        [Command(requiresAuthority = false)]
        private void OnAmmunitionUpdateServerCmd(int newAmmunition)
        {
            NotifyAmmunitionChanged(newAmmunition);  //Update and forward to other clients as well
        }
        #endregion

        #region *************************************************** UpgradePoints
        private void NotifyUpgradePointsChanged(int newUpgradePoints)
        {
            _status._upgradePoints = newUpgradePoints;
            UpgradePointsChanged?.Invoke(this, _status._upgradePoints);
            if (Network.NetworkInfo.IsHost)
                OnUpgradePointsUpdateClientRpc(_status._upgradePoints);
            else
                OnUpgradePointsUpdateServerCmd(_status._upgradePoints);
        }

        [ClientRpc(includeOwner = false)]
        private void OnUpgradePointsUpdateClientRpc(int newUpgradePoints)
        {
            _status._upgradePoints = newUpgradePoints;
            UpgradePointsChanged?.Invoke(this, _status._upgradePoints);
        }

        [Command(requiresAuthority = false)]
        private void OnUpgradePointsUpdateServerCmd(int newUpgradePoints)
        {
            NotifyUpgradePointsChanged(newUpgradePoints);  //Update and forward to other clients as well
        }
        #endregion


        //************************************************************************ Utility
        private void LoadBulletData(Databases.BulletDbId bulletId)
        {
            //Each cell should only call loadbullet once when it starts up. During the gameplay the bullet itself cannot change, but it is possible to modify some parameters temporarily

            //Get initial config from bullet db
            ref var bulletDbData = ref GlobalContext.bulletDb.GetBulletData(bulletId);

            //Fill the local data
            _status._baseReloadTime = bulletDbData.reloadTime;
            _status._ammunition = bulletDbData.startingAmmunition;
            _status._baseBulletRegenTime = bulletDbData.regenTime;

            _bulletData._dbId = bulletId;
            _bulletData._shootAttribute = bulletDbData.shootAttribute;
            _bulletData._bulletMovementAttribute = bulletDbData.movementAttribute;
            _bulletData._bulletHitAttribute = bulletDbData.hitAttribute;

            _bulletData._bulletBurstCounter = 0; //Always start at 0 to have it reset
            _bulletData._bulletBurstMax = bulletDbData.burstConfig.burstMax;
        }
    }
}
