using Algorithms;
using RO.Common;
using RO.Media;
using UnityEngine;

namespace RO.MapObjects
{
    //Unit contains data that is required for all battle objects
    public abstract class Unit : Block
    {
        //Struct with data required to smothly move an object
        public struct MovementInfo
        {
            public Vector2Int[] path;
            public int pathLength;
            public int currentIndex;
        }

        //each unit may only have 1 of each active at a time so we can just allocate fixed space for them
        public struct CastInfo
        {
            public EffectCancelToken castAuraToken;
            public EffectCancelToken castCircleToken;
            public EffectCancelToken castLockOnToken;
            public EffectCancelToken fillBarToken;

            public void CancelCastAnimations()
            {
                castAuraToken?.Cancel();
                castCircleToken?.Cancel();
                fillBarToken?.Cancel();
                castLockOnToken?.Cancel();

                castAuraToken = null;
                castCircleToken = null;
                fillBarToken = null;
                castLockOnToken = null;
            }
            public void RemoveAura(EffectCancelToken token)
            {
                if (!EffectCancelToken.IsNull(castAuraToken) && token == castAuraToken)
                    castAuraToken = null;
            }
            public void RemoveCastCircle(EffectCancelToken token)
            {
                if (!EffectCancelToken.IsNull(castCircleToken) && token == castCircleToken)
                    castCircleToken = null;
            }
            public void RemoveLockOn(EffectCancelToken token)
            {
                if (!EffectCancelToken.IsNull(castLockOnToken) && token == castLockOnToken)
                    castLockOnToken = null;
            }
            public void RemoveFillBar(EffectCancelToken token)
            {
                if (!EffectCancelToken.IsNull(fillBarToken) && token == fillBarToken)
                    fillBarToken = null;
            }
        }

        public struct LerpInfo
        {
            public const float TIME_TO_LERP = Constants.MIN_WALK_SPEED * 2 / 1000f; //lets take twice the min walk speed to lerp for now
            public float elapsedTime, endTime, moveTime;
            public Vector3 target, start;
        }

        //Struct with battle flags common to characters, monsters, homunculus, mercenaries
        public struct UnitFlags
        {
            public bool IsMoving;
            public bool IsFixingPosition;
        }

        //Struct with status common to characters, monsters, homunculus, mercenaries
        public struct Status
        {
            public int jobLvl, baseLvl;
            public int maxHp, currentHp, maxSp, currentSp;
            public int moveSpd, atkSpd;
            public int str, agi, vit, dex, int_, luk;
            public int atk, matk;
            public int def, mdef;
            public int hit, flee;
            public int crit;
            Elements _defElement;
            ElementLvls _defElementLvl;
            Sizes size;
            Races race;
            public System.Collections.BitArray buffs;
        }

        public UnitFlags flags;
        public MovementInfo moveInfo;
        public CastInfo castInfo;
        public LerpInfo lerpInfo;
        public Status status;
        public bool isFriendly;

        public Unit(int sessionId, BlockTypes blockType)
            : base(sessionId, blockType)
        {
            status.buffs = new System.Collections.BitArray((int)Databases.BuffIDs.Last);
        }

        public bool IsDead { get { return status.currentHp == 0; } }

        //Protected methods that will be used by inheriting classes to not multiply code
        protected void Unit_AppearAtCell(int x, int y, out Vector3 worldPosition)
        {
            Utility.GameToWorldCoordinatesCenter(x, y, out Vector3 pos);
            Physics.Raycast(pos, Vector3.down, out RaycastHit hit, Utility.RAYCAST_DISTANCE, LayerMasks.Map);
            worldPosition = hit.point;

            //Reset lerp info start and target so it won't use previous position during a walk right after enter range
            lerpInfo.start = worldPosition;
            lerpInfo.start.y = Utility.DEFAULT_Y;  //for proper raycasting
            lerpInfo.target = lerpInfo.start;

            position.x = x;
            position.y = y;
        }

        protected void Unit_LerpToCell(int x, int y, Vector3 startPosition)
        {
            Utility.GameToWorldCoordinatesCenter(x, y, out Vector3 pos);

            //Set lerp info
            lerpInfo.start = startPosition;
            lerpInfo.target = pos;
            lerpInfo.moveTime = LerpInfo.TIME_TO_LERP;
            lerpInfo.endTime = Globals.Time + lerpInfo.moveTime;
            lerpInfo.elapsedTime = 0;

            //Can't be moving and fixing position at the same time
            flags.IsFixingPosition = true;
            flags.IsMoving = false;

            //Graphically we're sliding but internally we're already there
            position.x = x;
            position.y = y;
        }

        protected void Unit_SetMoveDestination(int posX, int posY, int destX, int destY, int startDelay)
        {
            //Update end time to change positions at the time asked by server
            lerpInfo.endTime = Globals.Time + startDelay / 1000f;

            //Update the internal position
            //The lerp position from the previous movement will keep on playing until time hits startDelay
            position.x = posX;
            position.y = posY;

            //Calculate new path
            moveInfo.pathLength = Pathfinder.FindPath(ref position, destX, destY, ref moveInfo.path);

            //Reset movement
            moveInfo.currentIndex = 0;
            flags.IsMoving = true;

            // Although we can't be moving and fixing position at the same time, we can still finish the fixing position during startTime
        }

        protected void Unit_OnMovementCellReached(int direction, Transform transform)
        {
            //Set the variables needed by lerp

            //Check if we're moving diagonal as the speed is different
            if (direction % 2 != 0) //we're in a diagonal
                lerpInfo.moveTime = Constants.DIAGONAL_TO_UNIT_SIZE * status.moveSpd / Constants.CELL_TO_UNIT_SIZE / 1000f;
            else
                lerpInfo.moveTime = status.moveSpd / 1000f;

            lerpInfo.endTime += lerpInfo.moveTime;
            lerpInfo.elapsedTime = 0;
            lerpInfo.start = transform.position;
            lerpInfo.start.y = Utility.DEFAULT_Y; //For proper raycasting
            Utility.GameToWorldCoordinatesCenter(moveInfo.path[moveInfo.currentIndex], out lerpInfo.target);

            //Update internal position
            position = moveInfo.path[moveInfo.currentIndex];

            //increment index of current movement in path
            moveInfo.currentIndex++;
        }

        protected void Unit_LerpPosition(out Vector3 worldPosition)
        {
            //Updates the transform position on x and z coordinates
            lerpInfo.elapsedTime += Time.deltaTime / lerpInfo.moveTime;
            Vector3 worldPos = Vector3.Lerp(lerpInfo.start, lerpInfo.target, lerpInfo.elapsedTime);

            //Updates the position on the Y coordinate
            Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, Utility.RAYCAST_DISTANCE, LayerMasks.Map);
            worldPosition = hit.point;
        }
    }
}
