using System.Collections.Generic;
using UnityEngine;
using Bacterio.Databases;

namespace Bacterio.MapObjects
{
    public class Bullet : Block
    {
        public Animators.BulletAnimator _animator;
        public SpriteRenderer _renderer;

        //********* Bullet status
        public BulletDbId _dbId;
        public MovementAttribute _movementAttribute;
        public HitAttribute _hitAttribute;
        public int _attackPower;
        public short _moveSpeed;

        //********* Owner variables
        public BlockType _ownerBlockType;
        public MonoBehaviour _owner;
        public bool _clientIsOwnerOfBullet;

        //********* Bullet configs to be used by the attributes
        public short _hitCount; //To be shared by ricochet and pierce
        public float _initialAngle;

        //These should be loaded once on config and not change for the lifetime of the bullet
        public short _maxHits;
        public int _hitDelayMs;
        public float _rotationSpeed; //To be shared by movement rotative, immobile
        public float _waveSpeed;
        public int _maxAngle; //To be shared by wave and immobile

        //********* Timestamps
        public int _spawnTimeMs;
        public int _lifeTimeMs;
        public int _removeTimeMs;

        //********* Structures to hold the timestamps of what we've hit
        public Dictionary<Network.NetworkArrayObject.UniqueId, int> _bacteriaHitsMs;
        public int[] _cellHitMs;

        //********* Cluster variables
        public byte _clusterBulletCount;
        public Bullet[] _clusterBullets;
        public Bullet _clusterParent;
        public Transform _clusterTransform;

        //Some QoL getters for cluster
        public bool IsClusterParent { get { return _clusterTransform != null; } }
        public bool IsClusterChild { get { return _clusterParent != null; } }
        public bool IsDetachedFromCluster { get { return transform.parent == null; } }

        //Some QoL getters for attributes
        public bool IsRotative { get { return (_movementAttribute & MovementAttribute.Rotative) > 0; } }
        public bool IsImmovable { get { return (_movementAttribute & MovementAttribute.Immovable) > 0; } }
        public bool IsHoming { get { return (_movementAttribute & MovementAttribute.Homing) > 0; } }
        public bool IsBoomerang { get { return (_movementAttribute & MovementAttribute.Boomerang) > 0; } }
        public MovementAttribute MoveAttributeNoRotative { get { return _movementAttribute & ~MovementAttribute.Rotative; } }
        public HitAttribute HitAttributeArea { get { return _hitAttribute & HitAttribute.Area; } }
        public HitAttribute HitAttributeNoArea { get { return _hitAttribute & ~HitAttribute.Area; } }
        public bool HasNoDamageHitAttribute{ get { return (_hitAttribute & HitAttribute.NoDamage) > 0; } }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _bacteriaHitsMs = new Dictionary<Network.NetworkArrayObject.UniqueId, int>();
            _cellHitMs = new int[Constants.MAX_PLAYERS];
        }

        public void Configure(Cell cell, Vector2 direction, int spawnTimeMs, int durationMs)
        {
            _ownerBlockType = BlockType.Cell;
            _owner = cell;

            //Get config from bullet db
            ref var bulletDbData = ref GlobalContext.bulletDb.GetBulletData(cell._bulletData._dbId);

            //Shoot attribute is only needed by the cell. Once bullet is created it doesn't care how it was shot
            _movementAttribute = cell._bulletData._bulletMovementAttribute;
            _hitAttribute = cell._bulletData._bulletHitAttribute;

            //Other stats
            _attackPower = cell.GetAttackPower();
            _hitCount = 0;

            //General fields
            _maxHits = bulletDbData.maxHits;
            _hitDelayMs = bulletDbData.hitDelay;
            _moveSpeed = bulletDbData.moveSpeed;

            //Rotation fields
            _waveSpeed = bulletDbData.rotationConfig.waveSpeed;
            _maxAngle = bulletDbData.rotationConfig.maxAngle;
            _rotationSpeed = bulletDbData.rotationConfig.rotationSpeed;

            //cluster fields
            _clusterBulletCount = bulletDbData.clusterConfig.clusterSize;

            //Timestamps
            _spawnTimeMs = spawnTimeMs;
            _lifeTimeMs = spawnTimeMs + durationMs;

            //Update authority relevent vars
            if (cell.hasAuthority)
            {
                _clientIsOwnerOfBullet = true;
                _removeTimeMs = _lifeTimeMs + Constants.BULLET_HOST_ADDITIONAL_REMOVE_TIME;
            }
            else
            {
                _clientIsOwnerOfBullet = false;
                _removeTimeMs = _lifeTimeMs;
            }

            //Update transform part
            if (IsImmovable)
            {
                transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                transform.SetParent(cell.transform, false);
            }
            else
            {
                transform.localPosition = cell.transform.position;
                transform.localEulerAngles = new Vector3(0, 0, Vector3.SignedAngle(Constants.DEFAULT_UNIT_DIRECTION, direction, Vector3.forward));
            }

            _initialAngle = transform.localEulerAngles.z;
            this.enabled = true;
        }

        public void Configure(Bacteria bacteria, int attackPower, int spawnTimeMs, int durationMs)
        {
            _ownerBlockType = BlockType.Bacteria;
            _owner = bacteria; 
            
            _movementAttribute = bacteria._bulletMovementAttribute;
            _hitAttribute = bacteria._bulletHitAttribute;

            _attackPower = attackPower;
            _spawnTimeMs = spawnTimeMs;
            _lifeTimeMs = spawnTimeMs + durationMs;
        }

        public void Configure(Bullet clusterParent)
        {
            _clusterParent = clusterParent;

            //Copy attributes, only without the rotative in movement
            _movementAttribute = clusterParent._movementAttribute & ~MovementAttribute.Rotative;
            _hitAttribute = clusterParent._hitAttribute;

            //Copy stats and other vars needed when bullet hits something
            _attackPower = clusterParent._attackPower;
            _moveSpeed = clusterParent._moveSpeed;
            _hitDelayMs = clusterParent._hitDelayMs;
            _hitCount = clusterParent._hitCount;
            _maxHits = clusterParent._maxHits;
            _waveSpeed = clusterParent._waveSpeed;
            _maxAngle = clusterParent._maxAngle;
            _clientIsOwnerOfBullet = clusterParent._clientIsOwnerOfBullet;

            //No need to copy lifetime until the bullet is detached from cluster. 
        }
    }
}
