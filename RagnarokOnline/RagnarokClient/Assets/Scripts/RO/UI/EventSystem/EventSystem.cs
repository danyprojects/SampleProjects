using System;
using System.Collections.Generic;
using UnityEngine;

namespace RO.UI
{
    public class EventSystem : MonoBehaviour
    {
        private const int PixelDragThreshold = 10;

        private static readonly Comparison<RaycastResult> _raycastComparer = RaycastComparer;

        private List<RaycastResult> _raycastResultCache = new List<RaycastResult>();

        private PointerEventData _leftButtonEventData;
        private PointerEventData _rightButtonEventData;
        private PointerEventData _middleButtonEventData;

        private PointerEventData _inputPointerEvent;
        private static bool _selectionGuard; // for selected gameobject 
        private bool _hasFocus = true;
        private Event _processingEvent = new Event();

        public static IKeyboardHandler DefaultKeyboardHandler { get; set; }

        private static IKeyboardHandler _currentKeyboardHandler;
        public static IKeyboardHandler CurrentKeyboardHandler
        {
            get { return _currentKeyboardHandler; }
            set
            {
                _currentKeyboardHandler?.OnKeyboardFocus(false);
                _currentKeyboardHandler = value == null ? DefaultKeyboardHandler : value;
                _currentKeyboardHandler?.OnKeyboardFocus(true);
            }
        }
        public static GameObject CurrentSelectedGameObject { get; private set; }

        void Awake()
        {
            _leftButtonEventData = new PointerEventData()
            {
                button = PointerEventData.InputButton.Left
            };

            _rightButtonEventData = new PointerEventData()
            {
                button = PointerEventData.InputButton.Right
            };

            _middleButtonEventData = new PointerEventData()
            {
                button = PointerEventData.InputButton.Middle
            };
        }

        void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;
        }

        public void Process()
        {
            if (!_hasFocus)
            {
                if (_inputPointerEvent != null &&
                    _inputPointerEvent.pointerDrag != null &&
                    _inputPointerEvent.dragging)
                {
                    ReleaseMouse(_inputPointerEvent, _inputPointerEvent.pointerCurrentRaycast.gameObject);
                }

                _inputPointerEvent = null;

                return;
            }

            while (CurrentKeyboardHandler != null && Event.PopEvent(_processingEvent))
            {
                if (_processingEvent.rawType == EventType.KeyDown)
                    CurrentKeyboardHandler.OnKeyDown(_processingEvent);
            }

            if (Input.mousePresent)
                ProcessMouseEvents();
        }

        // Process all mouse events.
        private void ProcessMouseEvents()
        {
            FillMousePointerEventData();

            // left
            ProcessMousePress(_leftButtonEventData);
            ProcessMove(_leftButtonEventData);
            ProcessDrag(_leftButtonEventData);

            // right
            ProcessMousePress(_rightButtonEventData);
            ProcessDrag(_rightButtonEventData);

            // middle
            ProcessMousePress(_middleButtonEventData);
            ProcessDrag(_middleButtonEventData);

            // Sends scroll event
            if (Common.Globals.UI.IsScrollingAllowed &&
                !Mathf.Approximately(_leftButtonEventData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(_leftButtonEventData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy<IScrollHandler, PointerEventData>
                    (scrollHandler, _leftButtonEventData, (h, c) => h.OnScroll(c));
            }
        }

        private void FillMousePointerEventData()
        {
            // Populate the left button...
            var leftData = _leftButtonEventData;

            Vector2 pos = Input.mousePosition;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // We don't want to do ANY cursor-based interaction when the mouse is locked
                leftData.position = new Vector2(-1.0f, -1.0f);
                leftData.delta = Vector2.zero;
            }
            else
            {
                leftData.delta = pos - leftData.position;
                leftData.position = pos;
            }

            leftData.scrollDelta = Input.mouseScrollDelta;

            RaycastAll(leftData);
            leftData.pointerCurrentRaycast = new RaycastResult();

            // Find first raycast
            foreach (var result in _raycastResultCache)
            {
                if (result.gameObject == null)
                    continue;

                leftData.pointerCurrentRaycast = result;
                break;
            }

            // copy the apropriate data into right and middle slots
            CopyFromTo(leftData, _rightButtonEventData);
            CopyFromTo(leftData, _middleButtonEventData);
        }

        // Process movement for the current frame with the given pointer event.
        private void ProcessMove(PointerEventData pointerEvent)
        {
            var targetGO = (Cursor.lockState == CursorLockMode.Locked ? null
                : pointerEvent.pointerCurrentRaycast.gameObject);
            HandlePointerExitAndEnter(pointerEvent, targetGO);
        }

        // Process the drag for the current frame with the given pointer event.
        private void ProcessDrag(PointerEventData pointerEvent)
        {
            if (!pointerEvent.IsPointerMoving() ||
                Cursor.lockState == CursorLockMode.Locked ||
                pointerEvent.pointerDrag == null)
                return;

            if (!pointerEvent.dragging
                && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, PixelDragThreshold, pointerEvent.useDragThreshold))
            {
                ExecuteEvents.Execute<IBeginDragHandler, PointerEventData>
                    (pointerEvent.pointerDrag, pointerEvent, (h, c) => h.OnBeginDrag(c));
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute<IPointerUpHandler, PointerEventData>
                        (pointerEvent.pointerPress, pointerEvent, (h, c) => h.OnPointerUp(c));

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }

                ExecuteEvents.Execute<IDragHandler, PointerEventData>
                    (pointerEvent.pointerDrag, pointerEvent, (h, c) => h.OnDrag(c));
            }
        }

        // Calculate and process any mouse button state changes.
        private void ProcessMousePress(PointerEventData pointerEvent)
        {
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (Input.GetMouseButtonDown((int)pointerEvent.button))
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(pointerEvent, currentOverGo);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler, PointerEventData>
                    (currentOverGo, pointerEvent, (h, c) => h.OnPointerDown(c));

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute<IInitializePotentialDragHandler, PointerEventData>(
                        pointerEvent.pointerDrag, pointerEvent, (h, c) => h.OnInitializePotentialDrag(c));

                _inputPointerEvent = pointerEvent;
            }

            // PointerUp notification
            if (Input.GetMouseButtonUp((int)pointerEvent.button))
            {
                ReleaseMouse(pointerEvent, currentOverGo);
            }
        }

        private void ReleaseMouse(PointerEventData pointerEvent, GameObject currentOverGo)
        {
            ExecuteEvents.Execute<IPointerUpHandler, PointerEventData>(
                        pointerEvent.pointerPress, pointerEvent, (h, c) => h.OnPointerUp(c));

            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
            {
                ExecuteEvents.Execute<IPointerClickHandler, PointerEventData>(
                        pointerEvent.pointerPress, pointerEvent, (h, c) => h.OnPointerClick(c));
            }
            else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {
                ExecuteEvents.ExecuteHierarchy<IDropHandler, PointerEventData>(
                        currentOverGo, pointerEvent, (h, c) => h.OnDrop(c));
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                ExecuteEvents.Execute<IEndDragHandler, PointerEventData>(
                      pointerEvent.pointerDrag, pointerEvent, (h, c) => h.OnEndDrag(c));

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            // redo pointer enter / exit to refresh state
            // so that if we moused over something that ignored it before
            // due to having pressed on something else it now gets it.
            if (currentOverGo != pointerEvent.pointerEnter)
            {
                HandlePointerExitAndEnter(pointerEvent, null);
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
            }

            _inputPointerEvent = pointerEvent;
        }

        // walk up the tree till a common root between the last entered and the current entered is foung
        // send exit events up to (but not inluding) the common root. Then send enter events up to
        // (but not including the common root).
        private void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            // if we have no target / pointerEnter has been deleted
            // just send exit events to anything we are tracking
            // then exit
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                for (var i = 0; i < currentPointerData.hovered.Count; ++i)
                    ExecuteEvents.Execute<IPointerExitHandler, PointerEventData>(
                      currentPointerData.hovered[i], currentPointerData, (h, c) => h.OnPointerExit(c));

                currentPointerData.hovered.Clear();

                const CanvasFilter filter = ~(CanvasFilter.NpcDialog | CanvasFilter.ItemDrag);
                Common.Globals.UI.IsOverUI = (filter & UIController.Panel.CanvasFilter) != 0;

                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = null;
                    return;
                }
            }

            // if we have not changed hover target
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget)
                return;

            GameObject commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // and we already an entered object from last time
            if (currentPointerData.pointerEnter != null)
            {
                // send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                Transform t = currentPointerData.pointerEnter.transform;

                while (t != null)
                {
                    // if we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                        break;

                    ExecuteEvents.Execute<IPointerExitHandler, PointerEventData>(
                      t.gameObject, currentPointerData, (h, c) => h.OnPointerExit(c));

                    currentPointerData.hovered.Remove(t.gameObject);
                    t = t.parent;
                }
            }

            // now issue the enter call up to but not including the common root
            currentPointerData.pointerEnter = newEnterTarget;
            if (newEnterTarget != null)
            {
                Transform t = newEnterTarget.transform;

                while (t != null && t.gameObject != commonRoot)
                {
                    ExecuteEvents.Execute<IPointerEnterHandler, PointerEventData>(
                        t.gameObject, currentPointerData, (h, c) => h.OnPointerEnter(c));

                    currentPointerData.hovered.Add(t.gameObject);
                    t = t.parent;

                    Common.Globals.UI.IsOverUI = true;
                }
            }
        }

        // Deselect the current selected GameObject if the currently pointed-at GameObject is different.
        private void DeselectIfSelectionChanged(PointerEventData pointerEvent, GameObject currentOverGo)
        {
            var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);

            // if we have clicked something new, deselect the old thing
            // leave 'selection handling' up to the press event though.
            if (selectHandlerGO != CurrentSelectedGameObject)
                SetSelectedGameObject(null);
        }

        // Set the object as selected. 
        // Will send an OnDeselect the the old selected object and OnSelect to the new selected object.
        public static void SetSelectedGameObject(GameObject selected)
        {
            if (_selectionGuard)
            {
                Debug.LogError("Attempting to select " + selected + "while already selecting an object.");
                return;
            }

            _selectionGuard = true;
            if (selected != CurrentSelectedGameObject)
            {
                ExecuteEvents.Execute<IDeselectHandler, object>(CurrentSelectedGameObject, null, (h, c) => h.OnDeselect());
                CurrentSelectedGameObject = selected;
                ExecuteEvents.Execute<ISelectHandler, object>(CurrentSelectedGameObject, null, (h, c) => h.OnSelect());
            }
            _selectionGuard = false;
        }

        private static void CopyFromTo(PointerEventData from, PointerEventData to)
        {
            to.position = from.position;
            to.delta = from.delta;
            to.scrollDelta = from.scrollDelta;
            to.pointerCurrentRaycast = from.pointerCurrentRaycast;
            to.pointerEnter = from.pointerEnter;
        }

        // Given 2 GameObjects, return a common root GameObject (or null).
        private static GameObject FindCommonRoot(GameObject g1, GameObject g2)
        {
            if (g1 == null || g2 == null)
                return null;

            var t1 = g1.transform;
            while (t1 != null)
            {
                var t2 = g2.transform;
                while (t2 != null)
                {
                    if (t1 == t2)
                        return t1.gameObject;
                    t2 = t2.parent;
                }
                t1 = t1.parent;
            }
            return null;
        }

        private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                if (lhs.module.SortOrderPriority != rhs.module.SortOrderPriority)
                    return rhs.module.SortOrderPriority.CompareTo(lhs.module.SortOrderPriority);

                if (lhs.module.RenderOrderPriority != rhs.module.RenderOrderPriority)
                    return rhs.module.RenderOrderPriority.CompareTo(lhs.module.RenderOrderPriority);
            }

            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder)
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

            // comparing depth only makes sense if the two raycast results have the same root canvas (case 912396)
            if (lhs.depth != rhs.depth && lhs.module.RootRaycaster == rhs.module.RootRaycaster)
                return rhs.depth.CompareTo(lhs.depth);

            return lhs.index.CompareTo(rhs.index);
        }

        // Raycast into the scene using all configured GraphicRaycasters.
        private void RaycastAll(PointerEventData eventData)
        {
            _raycastResultCache.Clear();

            foreach (var raycaster in GraphicRaycaster.GetAllRaycasters())
            {
                if (raycaster.isActiveAndEnabled)
                    raycaster.Raycast(eventData, _raycastResultCache);
            }

            _raycastResultCache.Sort(_raycastComparer);
        }
    }
}