using RO.Common;
using RO.MapObjects;
using RO.Media;
using RO.Network;
using System;
using UnityEngine;

namespace RO.IO
{
    public class InputHandler : MonoBehaviour
    {
        public enum MouseButtons : int
        {
            Left = 1 << 0,
            Right = 1 << 1,
            Middle = 1 << 2,
            LeftRight = Left | Right,
            LeftMiddle = Left | Middle,
            LeftRightMiddle = Left | Right | Middle,

            None = 0
        }

        [SerializeField] private readonly bool _destroyOnLoad = false;
        [SerializeField] private GameController _gameController = null;
        [SerializeField] private TargetHandler _targetHandler = null;

        //Fields for cell highlighting
        private MeshFilter _meshFilter = null;
        private MeshRenderer _meshRenderer = null;
        private Vector3[] _vertices = new Vector3[4];
        private Vector3 _origin = Vector3.zero;
        private Vector2Int _highlightedCell = Vector2Int.zero;
        private Map _map = null;

        //Fields for input
        private KeyBinder _keyBinder = null;
        private bool _keyIsDown = false;
        private bool _requireRelease = false;
        private Action<Vector2Int> _requestedClickCoordinates = null;
        private Action<Block> _requestedClickTarget = null;
        private Action<int> _requestMouseScroll = null;
        private Action _requestCanceled = null;
        private Action _onMouseLeftDown = null;
        const float MOVE_SEND_DELAY = 1 / 10f; //lets say we can only send 10 move packets a second
        private float _nextDefaultClickSend = 0;

        //Fields for mouse over objects
        private int _mouseOverLayers = 0;
        private int _defaultLayerMask = 0;


        public bool HeldDuringUI { get; private set; } = false;
        public int MouseScrollDelta { get; private set; } = 0;
        public Vector2Int HighlightedCell { get { return _highlightedCell; } }
        public MouseButtons ButtonsHeld { get; private set; } = MouseButtons.None;
        public MouseButtons ButtonsReleased { get; private set; } = MouseButtons.None;
        public MouseButtons ButtonsPressed { get; private set; } = MouseButtons.None;
        public KeyBinder.CommandKeys CommandKeys { get; private set; } = KeyBinder.CommandKeys.None;

        void Start()
        {
            if (!_destroyOnLoad)
                DontDestroyOnLoad(gameObject);

            _keyBinder = new KeyBinder();
            InitializeHighlightMesh();
            _onMouseLeftDown = DefaultClick;
            _defaultLayerMask = LayerMasks.Player | LayerMasks.Monster | LayerMasks.Mercenary |
                           LayerMasks.Homunculus | LayerMasks.Pet | LayerMasks.Npc | LayerMasks.Item;
            _mouseOverLayers = _defaultLayerMask;
        }

        public void UpdateInput()
        {
            if (!Globals.UI.IsOverUI)
                RunRaycasts();
            else
                _targetHandler.RemoveHoveringBlock();

            _targetHandler.UpdateHoveringPosition();

            //If someone is hooking mouse scroll. Mouse scroll isnt an input so it needs to be handled here
            if (Input.mouseScrollDelta.y != 0)
            {
                MouseScrollDelta = Input.mouseScrollDelta.y > 0 ? 1 : -1;
                if (_requestMouseScroll != null)
                {
                    _requestMouseScroll(MouseScrollDelta);
                    MouseScrollDelta = 0; //So that no1 else uses this event
                }
            }
            else
                MouseScrollDelta = 0;

            if (!Input.anyKey && !_keyIsDown)
            {
                ButtonsHeld = MouseButtons.None;
                ButtonsReleased = MouseButtons.None;
                return;
            }
            else if (!Input.anyKey && _keyIsDown) //Do one more loop if there's no key down but we had a key down last loop
                _keyIsDown = false;
            else
                _keyIsDown = true;

            //Get command keys
            CommandKeys = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? KeyBinder.CommandKeys.Alt : KeyBinder.CommandKeys.None;
            CommandKeys |= (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? KeyBinder.CommandKeys.Shift : KeyBinder.CommandKeys.None;
            CommandKeys |= (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) ? KeyBinder.CommandKeys.Ctrl : KeyBinder.CommandKeys.None;

            //Get mouse left right middle states
            ButtonsHeld = Input.GetKey(KeyCode.Mouse0) ? MouseButtons.Left : MouseButtons.None;
            ButtonsHeld |= Input.GetKey(KeyCode.Mouse1) ? MouseButtons.Right : MouseButtons.None;
            ButtonsHeld |= Input.GetKey(KeyCode.Mouse2) ? MouseButtons.Middle : MouseButtons.None;

            ButtonsReleased = Input.GetKeyUp(KeyCode.Mouse0) ? MouseButtons.Left : MouseButtons.None;
            ButtonsReleased |= Input.GetKeyUp(KeyCode.Mouse1) ? MouseButtons.Right : MouseButtons.None;
            ButtonsReleased |= Input.GetKeyUp(KeyCode.Mouse2) ? MouseButtons.Middle : MouseButtons.None;

            ButtonsPressed = Input.GetKeyDown(KeyCode.Mouse0) ? MouseButtons.Left : MouseButtons.None;
            ButtonsPressed |= Input.GetKeyDown(KeyCode.Mouse1) ? MouseButtons.Right : MouseButtons.None;
            ButtonsPressed |= Input.GetKeyDown(KeyCode.Mouse2) ? MouseButtons.Middle : MouseButtons.None;

            _keyBinder.ProcessBoundKeys(CommandKeys);

            //Only process mouse if there were mouse events
            if (ButtonsHeld != MouseButtons.None || ButtonsReleased != MouseButtons.None)
                ProcessMouse();
        }

        private void ProcessMouse()
        {
            //In case we're on pick up or point
            if ((ButtonsReleased & MouseButtons.Left) > 0)
            {
                CursorAnimator.OnCursorUp();
                HeldDuringUI = false;
            }
            //When there was a click, save wether we were in UI or not 
            else if ((ButtonsPressed & MouseButtons.Left) > 0)
            {
                //this is mandatory in case cursor is showing pick up or click
                CursorAnimator.OnCursorDown();
                HeldDuringUI = Globals.UI.IsOverUI;
            }

            //Check if this loop event was from UI or not
            if (!Globals.UI.IsOverUI)
            {
                //If we're not in UI but current press originated in UI, then let UI handle it
                if (HeldDuringUI)
                    return;

                //Most common case is that we are not requiring release
                if (!_requireRelease && (ButtonsHeld & MouseButtons.Left) > 0)
                    _onMouseLeftDown();
                else if ((ButtonsReleased & MouseButtons.Left) > 0)
                    _requireRelease = false;

                //Process right button
                if ((ButtonsPressed & MouseButtons.Right) > 0)
                {
                    //Cancel any requests if there's been a right key press
                    if (_requestCanceled != null)
                    {
                        _requestCanceled();
                        _requestCanceled = null;
                        _onMouseLeftDown = DefaultClick;
                    }
                }
            }
            //If we are in UI and there's been a press, cancel any requests
            else if (ButtonsHeld > 0 && _requestCanceled != null)
            {
                _requestCanceled();
                _requestCanceled = null;
                _onMouseLeftDown = DefaultClick;
            }
        }

        //API to be used by controllers requesting a coordinates or click target callback
        public void RequestClickCoordinates(Action<Vector2Int> callback, bool quickCast, Action onCancel)
        {
            //If no quick cast then set all the callbacks ready to call on user mouse input
            if (!quickCast)
            {
                _requestedClickCoordinates = callback;
                _requestCanceled = onCancel;
                _onMouseLeftDown = OnRequestingClickCoordinates;
                return;
            }

            //Mouse function runs raycasts at the very start so we already have the highlighted cell for this loop
            //With quick cast and not in UI, call callback immediatly. If we're in UI then cancel request
            if (!Globals.UI.IsOverUI)
                callback(HighlightedCell);
            else
                onCancel();

            //If quickcast is enabled and we're in UI then skip waiting for mouse click. If we're not in UI just cancel the request
            _onMouseLeftDown = DefaultClick;
            _requireRelease = true;
        }

        public void RequestClickTarget(Action<Block> callback, bool quickCast, int rayLayers, Action onCancel)
        {
            //Layers containing which type of blocks we want to trigger the callback for
            _mouseOverLayers = rayLayers;

            //If no quick cast then set all the callbacks ready to call on user mouse input
            if (!quickCast)
            {
                _requestedClickTarget = callback;
                _requestCanceled = onCancel;
                _onMouseLeftDown = OnRequestingClickTarget;
                return;
            }

            //If quickcast is enabled and we're in UI then skip waiting for mouse click. If we're not in UI just cancel the request
            if (!Globals.UI.IsOverUI)
            {
                UpdateMouseOver(ref _origin); //Redo mouse over due to new mask
                callback(_targetHandler.HoveringBlock);
            }
            else
                onCancel();

            //Need to cleanup in cast a sequence like Non-quickcast skill requested -> no click -> quickCast skill requested
            _mouseOverLayers = _defaultLayerMask;
            _onMouseLeftDown = DefaultClick;
            _requireRelease = true;
        }

        public void RequestMouseScroll(Action<int> callback)
        {
            _requestMouseScroll = callback;
        }

        //Internal actions for input
        private void DefaultClick()
        {
            //Some block types have different behaviour when clicking on them
            if (_targetHandler.HoveringBlock != null)
            {
                switch (_targetHandler.HoveringBlock.BlockType)
                {
                    case BlockTypes.Npc:
                        {
                            //TODO: Talk to npc packet
                            return;
                        }
                    case BlockTypes.Item:
                        {
                            //TODO: pick up item
                            return;
                        }
                    case BlockTypes.Character:
                    case BlockTypes.Monster:
                    case BlockTypes.Homunculus:
                    case BlockTypes.Mercenary:
                        {
                            //If we clicked on an enemy then request a basic attack 
                            if (!_targetHandler.HoveredBlockIsFriendly)
                            {
                                //TODO:
                                return;
                            }
                            //If block is friendly then it should also default to movement packet
                        }
                        break;
                }
            }

            //Any cases not covered above should default to a movement packet
            if (Globals.Time >= _nextDefaultClickSend && _map.MapData.GetTile(_highlightedCell).IsWalkable &&
                Utility.IsInRectangularDistance(ref _gameController.LocalPlayer.position, ref _highlightedCell, Constants.MAX_WALK))
            {
                _nextDefaultClickSend = Globals.Time + MOVE_SEND_DELAY;

                SND_PlayerMove packet = new SND_PlayerMove
                {
                    destX = (short)HighlightedCell.x,
                    destY = (short)HighlightedCell.y
                };
                NetworkController.SendPacket(packet);
            }
        }

        private void OnRequestingClickCoordinates()
        {
            _requestedClickCoordinates(HighlightedCell);
            _onMouseLeftDown = DefaultClick;
            _requireRelease = true;
        }

        private void OnRequestingClickTarget()
        {
            _requestedClickTarget(_targetHandler.HoveringBlock);
            _onMouseLeftDown = DefaultClick;
            _mouseOverLayers = _defaultLayerMask;
            _requireRelease = true;
        }

        //Methods for cell highlighting
        public void AssignMap(Map map)
        {
            _map = map;
            _meshRenderer.enabled = map == null ? false : true;
        }

        private void RunRaycasts()
        {
            Vector3 mouse = Input.mousePosition;
            mouse.z = -300;
            _origin = Globals.Camera.ScreenToWorldPoint(mouse);

            //Update mouse over first to know if we should or not update highlighted cell
            UpdateMouseOver(ref _origin);

            //Only update highlight cell when map isn't null and when we're not hovering a block, or when we're hovering a friendly unit block
            //Todo: properly use cursor animations to update highlighted cell
            if (_map != null && (_targetHandler.HoveringBlock == null || CursorAnimator.GetAnimation() == CursorAnimator.Animations.Cast ||
                (_targetHandler.HoveringBlock.BlockType <= BlockTypes.LastUnit && _targetHandler.HoveredBlockIsFriendly)))
            {
                UpdateHighlightedCell(ref _origin);
            }
        }

        private void UpdateHighlightedCell(ref Vector3 origin)
        {
            if (Physics.Raycast(origin, Globals.Camera.transform.forward, out RaycastHit hit, 600, LayerMasks.Map))
            {
                Vector2Int cell = Vector2Int.zero;
                Utility.WorldToGameCoordinates(hit.point, ref _map.MapData, ref cell);
                if (!_map.MapData.GetTile(cell).IsWalkable || cell == HighlightedCell)
                    return;

                _highlightedCell = cell;
                _map.GetTileVerticesHeights(HighlightedCell.x, HighlightedCell.y, ref _vertices);
                _meshFilter.mesh.vertices = _vertices;
                Utility.GameToWorldCoordinates(HighlightedCell.x, HighlightedCell.y, out Vector3 position);
                transform.position = new Vector3(position.x, 0.05f, position.z);
            }
        }

        private void UpdateMouseOver(ref Vector3 origin)
        {
            Debug.DrawRay(origin, Globals.Camera.transform.forward * 600, Color.yellow);
            if (Physics.Raycast(origin, Globals.Camera.transform.forward, out RaycastHit hit, 600, _mouseOverLayers))
                _targetHandler.ChangeHoveringBlock((BlockTypes)hit.transform.gameObject.layer - LayerIndexes.BLOCK_LAYER_START, hit.transform.parent);
            else
                _targetHandler.RemoveHoveringBlock();
        }

        //Initializations
        private void InitializeHighlightMesh()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            //Construct the starting mesh
            _meshFilter.sharedMesh = new Mesh();

            Vector2[] UVs = new Vector2[4];
            UVs[0] = new Vector2(0, 1); // left, bottom
            UVs[1] = new Vector2(0, 0); // left, top
            UVs[2] = new Vector2(1, 0); // right, top
            UVs[3] = new Vector2(1, 1); // right, bottom

            _vertices[0] = new Vector3(0, 0, 0);
            _vertices[1] = new Vector3(0, 0, Constants.CELL_TO_UNIT_SIZE);
            _vertices[2] = new Vector3(Constants.CELL_TO_UNIT_SIZE, 0, Constants.CELL_TO_UNIT_SIZE);
            _vertices[3] = new Vector3(Constants.CELL_TO_UNIT_SIZE, 0, 0);

            int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

            _meshFilter.sharedMesh.vertices = _vertices;
            _meshFilter.sharedMesh.triangles = triangles;
            _meshFilter.sharedMesh.uv = UVs;

            transform.position = Vector3.zero;

            //Enlarge the Y bound since it's the only coordinate that might go off screen and we still want to render
            Bounds bounds = _meshFilter.sharedMesh.bounds;
            _meshFilter.sharedMesh.bounds = new Bounds(bounds.center, new Vector3(bounds.size.x, 300, bounds.size.z));

            _meshRenderer.enabled = false;
        }
    }
}
