using RO.Common;
using RO.MapObjects;
using RO.Media;
using System;
using UnityEngine;

namespace RO.IO
{
    public class TargetHandler : MonoBehaviour
    {
        public Block HoveringBlock { get; private set; } = null;
        public Block LockedBlock { get; private set; } = null;
        public bool HoveredBlockIsFriendly { get; private set; } = false;

        [SerializeField] private Transform _uiRoot = null;

        //For optimized internal lookups
        private Transform _hoveringBlockTransform = null;
        private bool _isHoveringPortal = false;
        private Vector2 _lastScreenPos = Vector2.zero;

        private UnitInfoText _unitInfoText = null;

        private Action<Transform>[] _OnMouseHoveringBlocks = new Action<Transform>[(int)BlockTypes.None];
        private Action<Transform>[] _OnMouseExitBlocks = new Action<Transform>[(int)BlockTypes.None];

        public void RemoveHoveringBlock()
        {
            //ignore if there is no block being hovered
            if (HoveringBlock == null)
            {
                //because warps have no animator we can't get the block
                if (_isHoveringPortal)
                {
                    CursorAnimator.UnsetAnimation(CursorAnimator.Animations.Portal);
                    _isHoveringPortal = false;
                    _hoveringBlockTransform = null;
                }
                return;
            }

            _OnMouseExitBlocks[(int)HoveringBlock.BlockType](_hoveringBlockTransform);
            HoveringBlock = null;
            _hoveringBlockTransform = null;

            _unitInfoText.Hide();
        }

        public void ChangeHoveringBlock(BlockTypes blockType, Transform blockTransform)
        {
            //Do nothing if it's the same block
            if (_hoveringBlockTransform == blockTransform)
                return;

            //Try to cleaup old block
            RemoveHoveringBlock();

            //Handle new hovering block
            _OnMouseHoveringBlocks[(int)blockType](blockTransform);
            _hoveringBlockTransform = blockTransform;

            //Don't update info text if it's a warp portal
            if (_isHoveringPortal)
                return;

            //Move unit info text
            Vector2 screenPos = Globals.Camera.WorldToScreenPoint(blockTransform.position);
            _unitInfoText.Show(screenPos);
            _lastScreenPos = screenPos;
        }

        public void UpdateHoveringPosition()
        {
            if (_hoveringBlockTransform == null || _isHoveringPortal)
                return;

            //Move unit info text
            Vector2 screenPos = Globals.Camera.WorldToScreenPoint(_hoveringBlockTransform.position);
            if (screenPos == _lastScreenPos)
                return;

            _unitInfoText.Show(screenPos);
            _lastScreenPos = screenPos;
        }

        void Start()
        {
            //Instantiate the info texts
            _unitInfoText = UnitInfoText.Instantiate(_uiRoot);

            InitializeBlockHoveringLookup();
            InitializeBlockExitingLookup();

            DontDestroyOnLoad(this);
        }

        private void InitializeBlockHoveringLookup()
        {
            _OnMouseHoveringBlocks[(int)BlockTypes.Character] = (Transform obj) =>
            {
                var controller = obj.GetComponent<PlayerAnimatorController>();

                _unitInfoText.SetUnitName(controller.CharacterInstance._charInfo.name);

                //Add party and other stuff here
                HoveredBlockIsFriendly = controller.CharacterInstance.isFriendly;
                if (!HoveredBlockIsFriendly)
                    CursorAnimator.SetAnimation(CursorAnimator.Animations.Attack);

                HoveringBlock = controller.CharacterInstance;
            };
            _OnMouseHoveringBlocks[(int)BlockTypes.Monster] = (Transform obj) =>
            {
                var controller = obj.GetComponent<MonsterAnimatorController>();
                //Do stuff with monster controller
                _unitInfoText.SetUnitName(Databases.MonsterDb.Monsters[(int)controller.MonsterInstance._monsterInfo.dbId].name);

                HoveredBlockIsFriendly = controller.MonsterInstance.isFriendly;
                if (!HoveredBlockIsFriendly)
                    CursorAnimator.SetAnimation(CursorAnimator.Animations.Attack);

                HoveringBlock = controller.MonsterInstance;
            };
            _OnMouseHoveringBlocks[(int)BlockTypes.Mercenary] = (Transform obj) =>
            {
            };
            _OnMouseHoveringBlocks[(int)BlockTypes.Homunculus] = (Transform obj) =>
            {
            };
            _OnMouseHoveringBlocks[(int)BlockTypes.Pet] = (Transform obj) =>
            {
            };
            _OnMouseHoveringBlocks[(int)BlockTypes.Npc] = (Transform obj) =>
            {
                //todo: when npcs have the animator, replace next line for: var controller obj.GetComponent<NpcAnimatorController>();
                object controller = null; // 
                //map portals don't have animators
                if (controller == null)
                {
                    CursorAnimator.SetAnimation(CursorAnimator.Animations.Portal);
                    _isHoveringPortal = true;
                }
            };
            _OnMouseHoveringBlocks[(int)BlockTypes.Item] = (Transform obj) =>
            {
                var animator = obj.GetComponent<ItemAnimator>();
                //Do stuff with monster controller
                _unitInfoText.SetUnitName(animator.ItemInstance.ItemData.name);
                HoveringBlock = animator.ItemInstance;

                CursorAnimator.SetAnimation(CursorAnimator.Animations.PickUp);
            };
        }

        private void InitializeBlockExitingLookup()
        {
            _OnMouseExitBlocks[(int)BlockTypes.Character] = (Transform obj) =>
            {
                CursorAnimator.UnsetAnimation(CursorAnimator.Animations.Attack);
            };
            _OnMouseExitBlocks[(int)BlockTypes.Monster] = (Transform obj) =>
            {
                CursorAnimator.UnsetAnimation(CursorAnimator.Animations.Attack);
            };
            _OnMouseExitBlocks[(int)BlockTypes.Mercenary] = (Transform obj) =>
            {
            };
            _OnMouseExitBlocks[(int)BlockTypes.Homunculus] = (Transform obj) =>
            {
            };
            _OnMouseExitBlocks[(int)BlockTypes.Pet] = (Transform obj) =>
            {
            };
            _OnMouseExitBlocks[(int)BlockTypes.Npc] = (Transform obj) =>
            {
            };
            _OnMouseExitBlocks[(int)BlockTypes.Item] = (Transform obj) =>
            {
                CursorAnimator.UnsetAnimation(CursorAnimator.Animations.PickUp);
            };
        }
    }
}