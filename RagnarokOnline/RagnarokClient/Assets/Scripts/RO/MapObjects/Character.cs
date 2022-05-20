using RO.Common;
using RO.Databases;
using RO.Media;
using UnityEngine;

namespace RO.MapObjects
{
    public class Character : Unit
    {
        public struct Gear
        {
            private int[] _gears; // maybe make this public with a class that handles gears         
        }

        public struct CharacterInfo
        {
            public string name;
            public Jobs job;
            public Gender gender;
            public int hairstyle;

            public int currentBaseExp, currentJobExp;
            public int weight, maxWeight;
            public int zeny;

            public Gear gear;

            public bool isMounted;
        }

        //Direction for players has head, for other things it doesnt. So this should remain nested
        public sealed class Direction
        {
            public int HeadCamera;
            public int BodyCamera;
            public int Body { get; private set; }
            public int Head { get; private set; }
            public int Camera { get; private set; }

            public Direction(int head, int body)
            {
                HeadCamera = Head = head;
                BodyCamera = Body = body;
                Camera = 0;
            }

            public void UpdateDirection(int camera)
            {
                Camera = 8 - camera;
                HeadCamera = (Head + Camera) % 8;
                BodyCamera = (Body + Camera) % 8;
            }

            public void UpdateDirection(Direction direction)
            {
                Head = direction.Head;
                Body = direction.Body;
                HeadCamera = (Head + Camera) % 8;
                BodyCamera = (Body + Camera) % 8;
            }

            public void UpdateDirection(int head, int body)
            {
                Head = head;
                Body = body;
                HeadCamera = (Head + Camera) % 8;
                BodyCamera = (Body + Camera) % 8;
            }
        }

        public Character _nextCharacter = null, _previousCharacter = null;

        public CharacterInfo _charInfo = new CharacterInfo();
        public Direction _direction = new Direction(0, 0);

        public PlayerAnimatorController PlayerAnimator { get; private set; } = null;

        public override bool IsEnabled
        {
            get
            {
                return PlayerAnimator.gameObject.activeSelf;
            }
            set
            {
                //Clear effects before setting active due to parenting issues
                if (!value)
                    ClearEffects();

                PlayerAnimator.gameObject.SetActive(value);
                PlayerAnimator.enabled = value;
            }
        }

        public Character(int sessionId)
            : base(sessionId, BlockTypes.Character)
        {
            PlayerAnimator = ObjectPoll.PlayerAnimatorControllersPoll.GetComponent<PlayerAnimatorController>();
            PlayerAnimator.transform.SetParent(null);

            SetGameObject(PlayerAnimator.gameObject);

            status.moveSpd = Constants.DEFAULT_WALK_SPEED;
            moveInfo.path = new Vector2Int[Constants.MAX_WALK];
        }

        public void UpdateDirections(Direction direction, int cameraDirection)
        {
            _direction.UpdateDirection(cameraDirection);
            _direction.UpdateDirection(direction);
            PlayerAnimator.ChangedDirection();
        }

        public void UpdateDirection(int cameraDirection)
        {
            if (cameraDirection == _direction.Camera)
                return;
            _direction.UpdateDirection(cameraDirection);
            PlayerAnimator.ChangedDirection();
        }

        public void UpdateDirection(int headDirection, int bodyDirection)
        {
            if (bodyDirection == _direction.Body && headDirection == _direction.Head)
                return;
            _direction.UpdateDirection(headDirection, bodyDirection);
            PlayerAnimator.ChangedDirection();
        }

        public void UpdateDirection(Direction direction)
        {
            if (direction.Body == _direction.Body && direction.Head == _direction.Head)
                return;
            _direction.UpdateDirection(direction);
            PlayerAnimator.ChangedDirection();
        }

        public void UpdateRotationAsCenter(Transform camera)
        {
            PlayerAnimator.transform.localEulerAngles = camera.localEulerAngles;
            PlayerAnimator.transform.Rotate(Vector3.up, 0f);
        }

        public void UpdateRotationFromCenter(Transform centerObject)
        {
            PlayerAnimator.transform.localEulerAngles = centerObject.localEulerAngles;
        }

        //Public command API

        public void Animate()
        {
            PlayerAnimator.AnimateCharacter(this);
        }

        //Use this for cases that require instant position adjusting
        public void InstantAppearAtCell(int x, int y, bool keepAnimation = false)
        {
            Unit_AppearAtCell(x, y, out Vector3 worldPosition);
            PlayerAnimator.transform.position = worldPosition;

            //In case we want to overwrite animation
            if (!keepAnimation)
                PlayerAnimator.PlayIdleAnimation();
        }

        public void SmoothAppearAtCell(int x, int y, EnterRangeType enterRangeType, bool keepAnimation = false)
        {
            InstantAppearAtCell(x, y, keepAnimation);

            PlayerAnimator.Fade(FadeDirection.In);

            if (enterRangeType == EnterRangeType.Teleport)
                effects.Add(EffectsAnimatorController.PlayEffect(CylinderEffectIDs.WarpIn, transform, RemoveEffect));
        }

        //For smooth sliding into a cell
        public void LerpToCell(int x, int y, bool keepAnimation = true)
        {
            //update direction if necessary
            if (x != position.x || y != position.y)
                UpdatePlayerDirection(x, y);

            Unit_LerpToCell(x, y, PlayerAnimator.transform.position);

            if (!keepAnimation)
                PlayerAnimator.PlayIdleAnimation();
        }

        public void Move()
        {
            //When we finished walking to a cell
            if (Globals.Time >= lerpInfo.endTime)
            {
                //Check if next previously prepared cell is valid
                if (moveInfo.currentIndex >= moveInfo.pathLength)
                {
                    flags.IsMoving = false;
                    PlayerAnimator.PlayIdleAnimation();
                    return;
                }
                OnMovementCellReached();

                //Frame skip: if after updating we're still ahead of update time, skip if we still have cells to skip
                if (lerpInfo.endTime < Globals.Time && moveInfo.currentIndex < moveInfo.pathLength - 1)
                {
                    Move();
                    return; //don't update here. The recursive one will do the update
                }
            }

            Unit_LerpPosition(out Vector3 worldPosition);
            PlayerAnimator.transform.position = worldPosition;
        }

        public void FixPosition()
        {
            //If we reach the time limit. Stop lerping and make sure we're at designated position
            if (Globals.Time >= lerpInfo.endTime)
            {
                flags.IsFixingPosition = false;
                lerpInfo.elapsedTime = 1;
            }

            Unit_LerpPosition(out Vector3 worldPosition);
            PlayerAnimator.transform.position = worldPosition;
        }

        public void SetMoveDestination(int posX, int posY, int destX, int destY, int startDelay)
        {
            //If we're moving and we aren't where server wants us to be. Start a new lerp to the server position
            if (flags.IsMoving && (position.x != posX || position.y != posY))
            {
                UpdatePlayerDirection(posX, posY);

                //This is how long we have left to move
                lerpInfo.moveTime = Mathf.Max(0, Globals.Time + startDelay / 1000f - lerpInfo.endTime);

                lerpInfo.elapsedTime = 0;
                lerpInfo.start = transform.position;
                Utility.GameToWorldCoordinatesCenter(posX, posY, out lerpInfo.target);
            }

            //This middle part is generic
            Unit_SetMoveDestination(posX, posY, destX, destY, startDelay);

            //If we couldn't calculate. Should not happen. 
            if (moveInfo.pathLength == 0)
            {
                //todo: maybe try to calculate to posX and posY to finish moving there
                moveInfo.currentIndex = int.MaxValue; //invalidate for now
                flags.IsMoving = false;
                PlayerAnimator.PlayIdleAnimation();
                return;
            }
        }

        public void Cleanup()
        {
            IsEnabled = false;
            ObjectPoll.PlayerAnimatorControllersPoll = PlayerAnimator.gameObject;
            PlayerAnimator = null;
        }

        //Internal methods
        private void OnMovementCellReached()
        {
            //Start / Update move animation speed
            PlayerAnimator.PlayWalkAnimation(status.moveSpd);

            //Movement should always be a diff of 1 cell. Can use the unsafe approach
            int direction = Utility.LookUpDirectionUnsafe(position, moveInfo.path[moveInfo.currentIndex]);
            UpdateDirection(direction, direction);

            //In case we were fixing position, this is where we should disable it so we give it time to finish
            if (flags.IsFixingPosition)
            {
                //force fix position to end at it's destination
                lerpInfo.elapsedTime = 1;
                flags.IsFixingPosition = false;
            }

            //The rest of the code is generic and is in unit class
            Unit_OnMovementCellReached(_direction.Body, transform);
        }

        private void UpdatePlayerDirection(int nextX, int nextY)
        {
            int direction = Utility.LookUpDirectionSafe(position.x, position.y, nextX, nextY);
            UpdateDirection(direction, direction);
        }
    }
}

