using RO.Common;
using RO.Media;
using UnityEngine;

namespace RO.MapObjects
{
    public sealed class Monster : Unit
    {
        public sealed class Direction
        {
            public int BodyCamera;
            public int Body { get; private set; }
            public int Camera { get; private set; }

            public Direction(int body)
            {
                BodyCamera = Body = body;
                Camera = 0;
            }

            public void UpdateCameraDirection(int camera)
            {
                Camera = 8 - camera;
                BodyCamera = (Body + Camera) % 8;
            }

            public void UpdateBodyDirection(int body)
            {
                Body = body;
                BodyCamera = (Body + Camera) % 8;
            }
        }

        public struct MonsterInfo
        {
            public Databases.MonsterIDs dbId;
        }

        public Monster _nextMonster = null, _previousMonster = null;

        public MonsterInfo _monsterInfo = new MonsterInfo();
        public Direction _direction = new Direction(0);

        public MonsterAnimatorController MonsterAnimator { get; private set; } = null;

        public override bool IsEnabled
        {
            get
            {
                return MonsterAnimator.gameObject.activeSelf;
            }
            set
            {
                if (!value)
                    ClearEffects();

                MonsterAnimator.gameObject.SetActive(value);
                MonsterAnimator.enabled = value;
            }
        }

        public Monster(int sessionId, Databases.MonsterIDs monsterId)
            : base(sessionId, BlockTypes.Monster)
        {
            MonsterAnimator = ObjectPoll.MonsterAnimatorControllersPoll.GetComponent<MonsterAnimatorController>();
            MonsterAnimator.transform.SetParent(null);

            SetGameObject(MonsterAnimator.gameObject);

            _monsterInfo.dbId = monsterId;

            status.moveSpd = Constants.DEFAULT_WALK_SPEED;
            moveInfo.path = new Vector2Int[Constants.MAX_WALK];
        }

        //Main API for interacting with the player
        public void UpdateDirections(int bodyDirection, int cameraDirection)
        {
            _direction.UpdateCameraDirection(cameraDirection);
            _direction.UpdateBodyDirection(bodyDirection);
            MonsterAnimator.ChangedDirection();
        }

        public void UpdateCameraDirection(int cameraDirection)
        {
            if (cameraDirection == _direction.Camera)
                return;

            _direction.UpdateCameraDirection(cameraDirection);
            MonsterAnimator.ChangedDirection();
        }

        public void UpdateBodyDirection(int bodyDirection)
        {
            if (bodyDirection == _direction.Body)
                return;

            _direction.UpdateBodyDirection(bodyDirection);
            MonsterAnimator.ChangedDirection();
        }

        public void UpdateRotationAsCenter(Transform camera)
        {
            MonsterAnimator.transform.localEulerAngles = camera.localEulerAngles;
            MonsterAnimator.transform.Rotate(Vector3.up, 0f);
        }

        public void UpdateRotationFromCenter(Transform centerObject)
        {
            MonsterAnimator.transform.localEulerAngles = centerObject.localEulerAngles;
        }

        public void Animate()
        {
            MonsterAnimator.AnimateMonster(this);
        }

        public void AppearAtCell(int x, int y, EnterRangeType enterRangeType, bool keepAnimation = false)
        {
            Unit_AppearAtCell(x, y, out Vector3 worldPosition);
            MonsterAnimator.transform.position = worldPosition;

            //RO always showed the fade in even in teleport enter range of mobs
            if ((enterRangeType & (EnterRangeType.Teleport | EnterRangeType.Default)) > 0)
                MonsterAnimator.Fade(FadeDirection.In);

            //TODO: Handle the other enter range types

            //In case we want to overwrite animation
            if (!keepAnimation)
                MonsterAnimator.PlayIdleAnimation();
        }

        public void LerpToCell(int x, int y, bool keepAnimation = true)
        {
            //update direction if necessary
            if (x != position.x || y != position.y)
                UpdateMonsterDirection(x, y);

            Unit_LerpToCell(x, y, MonsterAnimator.transform.position);

            if (!keepAnimation)
                MonsterAnimator.PlayIdleAnimation();
        }

        public void SetMoveDestination(int posX, int posY, int destX, int destY, int startDelay)
        {
            //If we're moving and we aren't where server wants us to be. Start a new translation to the server position
            if (flags.IsMoving && (position.x != posX || position.y != posY))
            {
                UpdateMonsterDirection(posX, posY);

                //This is how long we have left to move
                lerpInfo.moveTime = Mathf.Max(0, Globals.Time + startDelay / 1000f - lerpInfo.endTime);

                lerpInfo.elapsedTime = 0;
                lerpInfo.start = transform.position;
                Utility.GameToWorldCoordinatesCenter(moveInfo.path[moveInfo.currentIndex], out lerpInfo.target);
            }

            //This middle part is generic
            Unit_SetMoveDestination(posX, posY, destX, destY, startDelay);

            //If we couldn't calculate. Should not happen. 
            if (moveInfo.pathLength == 0)
            {
                //todo: maybe try to calculate to posX and posY to finish moving there
                moveInfo.currentIndex = int.MaxValue; //invalidate for now
                flags.IsMoving = false;
                MonsterAnimator.PlayIdleAnimation();
                return;
            }
        }

        public void Move()
        {
            //When we finished walking to a cell
            if (Globals.Time >= lerpInfo.endTime)
            {
                //Check if previously prepared cell is valid
                if (moveInfo.currentIndex >= moveInfo.pathLength)
                {
                    flags.IsMoving = false;
                    MonsterAnimator.PlayIdleAnimation();
                    return;
                }
                OnMovementCellReached();

                //Frame skip: if after updating we're still ahead of update time, skip if we still have cells to skip
                if (Globals.Time >= lerpInfo.endTime && moveInfo.currentIndex < moveInfo.pathLength - 1)
                {
                    Move();
                    return; //don't update here. The recursive one will do the update
                }
            }

            Unit_LerpPosition(out Vector3 worldPosition);
            MonsterAnimator.transform.position = worldPosition;
        }

        public void FixPosition()
        {
            //If we reach the time limit. Stop lerping and make sure we're at designated position
            if (Globals.Time >= lerpInfo.endTime)
            {
                flags.IsFixingPosition = false;
                //to force next lerp to hit the target position
                lerpInfo.elapsedTime = 1;
            }

            Unit_LerpPosition(out Vector3 worldPosition);
            MonsterAnimator.transform.position = worldPosition;
        }

        public void Cleanup()
        {
            IsEnabled = false;
            ObjectPoll.MonsterAnimatorControllersPoll = MonsterAnimator.gameObject;
            MonsterAnimator = null;
        }

        //Internal methods
        private void OnMovementCellReached()
        {
            //Start / Update move animation speed
            MonsterAnimator.PlayWalkAnimation(status.moveSpd);
            //Movement should always be a diff of 1 cell. Can use the unsafe approach
            UpdateBodyDirection(Utility.LookUpDirectionUnsafe(position, moveInfo.path[moveInfo.currentIndex]));

            //In case we were fixing position, this is where we should disable it so we give it time to finish
            if (flags.IsFixingPosition)
            {
                //force fix position to end 
                lerpInfo.endTime = float.MinValue;
                FixPosition();
            }

            //The rest of the code is generic and is in unit class
            Unit_OnMovementCellReached(_direction.Body, transform);
        }

        private void UpdateMonsterDirection(int nextX, int nextY)
        {
            UpdateBodyDirection(Utility.LookUpDirectionSafe(position.x, position.y, nextX, nextY));
        }
    }
}
