using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class GraphicRaycaster : MonoBehaviour
    {
        private static readonly List<GraphicRaycaster> _raycasterList = new List<GraphicRaycaster>();

        private Canvas _canvas;
        [NonSerialized] static readonly List<Graphic> _sortedGraphics = new List<Graphic>();
        [NonSerialized] private List<Graphic> _raycastResults = new List<Graphic>();

        // Whether Graphics facing away from the raycaster are checked for raycasts.
        [SerializeField]
        private bool _ignoreReversedGraphics = true;

        // Priority of the raycaster based upon sort order.
        public int SortOrderPriority => _canvas.sortingOrder;

        // Priority of the raycaster based upon render order.
        public int RenderOrderPriority => _canvas.rootCanvas.renderOrder;

        /// Raycaster on root canvas
        private GraphicRaycaster _rootRaycaster;
        public GraphicRaycaster RootRaycaster
        {
            get
            {
                if (_rootRaycaster == null)
                {
                    var raycasters = GetComponentsInParent<GraphicRaycaster>();
                    if (raycasters.Length != 0)
                        _rootRaycaster = raycasters[raycasters.Length - 1];
                }

                return _rootRaycaster;
            }
        }

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        void OnEnable()
        {
            if (!_raycasterList.Contains(this))
                _raycasterList.Add(this);
        }

        void OnDisable()
        {
            _raycasterList.Remove(this);
        }

        public static List<GraphicRaycaster> GetAllRaycasters()
        {
            return _raycasterList;
        }

        // Perform the raycast against the list of graphics associated with the Canvas.
        public void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            var canvasGraphics = GraphicRegistry.GetGraphicsForCanvas(_canvas);
            if (canvasGraphics == null || canvasGraphics.Count == 0)
                return;

            int displayIndex = _canvas.targetDisplay;

            var eventPosition = Display.RelativeMouseAt(eventData.position);
            if (eventPosition != Vector3.zero)
            {
                // We support multiple display and display identification based on event position.

                int eventDisplayIndex = (int)eventPosition.z;

                // Discard events that are not part of this display so the user does not interact with multiple displays at once.
                if (eventDisplayIndex != displayIndex)
                    return;
            }
            else
            {
                // The multiple display system is not supported on all platforms, when it is not supported the returned position
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                eventPosition = eventData.position;

                // We dont really know in which display the event occured. We will process the event assuming it occured in our display.
            }

            // Multiple display support only when not the main display. For display 0 the reported
            // resolution is always the desktops resolution since its part of the display API,
            // so we use the standard none multiple display method. (case 741751)
            float w = Screen.width;
            float h = Screen.height;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
                w = Display.displays[displayIndex].systemWidth;
                h = Display.displays[displayIndex].systemHeight;
            }

            Vector2 pos = new Vector2(eventPosition.x / w, eventPosition.y / h);

            // If it's outside the camera's viewport, do nothing
            if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
                return;

            Ray ray = new Ray();

            _raycastResults.Clear();

            Raycast(eventPosition, canvasGraphics, _raycastResults);

            int totalCount = _raycastResults.Count;
            for (var index = 0; index < totalCount; index++)
            {
                var go = _raycastResults[index].gameObject;
                bool appendGraphic = true;

                if (_ignoreReversedGraphics)
                {
                    // If we dont have a camera we know that we should always be facing forward
                    var dir = go.transform.rotation * Vector3.forward;
                    appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
                }

                if (appendGraphic)
                {
                    Transform trans = go.transform;
                    Vector3 transForward = trans.forward;

                    var castResult = new RaycastResult
                    {
                        gameObject = go,
                        module = this,
                        screenPosition = eventPosition,
                        index = resultAppendList.Count,
                        depth = _raycastResults[index].depth,
                        sortingLayer = _canvas.sortingLayerID,
                        sortingOrder = _canvas.sortingOrder,
                        worldPosition = ray.origin,
                        worldNormal = -transForward
                    };
                    resultAppendList.Add(castResult);
                }
            }
        }

        // Perform a raycast into the screen and collect all graphics underneath it.
        private static void Raycast(Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
        {
            // Necessary for the event system
            int totalCount = foundGraphics.Count;
            for (int i = 0; i < totalCount; ++i)
            {
                Graphic graphic = foundGraphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition))
                    continue;

                if (graphic.Raycast(pointerPosition, null))
                    _sortedGraphics.Add(graphic);
            }

            _sortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            totalCount = _sortedGraphics.Count;
            for (int i = 0; i < totalCount; ++i)
                results.Add(_sortedGraphics[i]);

            _sortedGraphics.Clear();
        }
    }
}
