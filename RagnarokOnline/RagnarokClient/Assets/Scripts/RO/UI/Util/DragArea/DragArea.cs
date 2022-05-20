using UnityEngine;

namespace RO.UI
{
    public partial class DragAreaController : MonoBehaviour
    {
        public class DragArea : MonoBehaviour,
            IDragHandler, IEndDragHandler, IBeginDragHandler
        {
            static private readonly float snapValue = 16f;

            private Vector2 pointerOffset;
            private RectTransform canvasRectTransform;
            private RectTransform panelRectTransform;
            private DragAreaController dragController;

            protected void Awake()
            {
                Canvas canvas = GetComponentInParent<Canvas>();

                dragController = GetComponentInParent<DragAreaController>();
                canvasRectTransform = canvas.rootCanvas.transform as RectTransform;

                if (canvas.isRootCanvas)
                    panelRectTransform = dragController.GetComponent<RectTransform>();
                else
                    panelRectTransform = canvas.transform as RectTransform;
            }

            public void OnDrag(PointerEventData data)
            {
                if (!dragController._isDraging)
                    return;

                Vector3[] canvasCorners = new Vector3[4];
                canvasRectTransform.GetWorldCorners(canvasCorners);

                float clampedX = Mathf.Clamp(data.position.x, canvasCorners[0].x, canvasCorners[2].x);
                float clampedY = Mathf.Clamp(data.position.y, canvasCorners[0].y, canvasCorners[2].y);

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform, new Vector2(clampedX, clampedY), null, out Vector2 localPointerPosition))
                {
                    panelRectTransform.localPosition = localPointerPosition - pointerOffset;

                    SnapPosition(canvasCorners);
                }
            }

            void SnapPosition(Vector3[] canvasCorners)
            {
                Vector3[] panelCorners = new Vector3[4];
                panelRectTransform.GetWorldCorners(panelCorners);

                if (panelCorners[0].y > canvasCorners[0].y) //snapDown
                {
                    float distance = Vector3.Distance(canvasCorners[0], new Vector3(canvasCorners[0].x, panelCorners[0].y, canvasCorners[0].z));
                    if (distance < snapValue)
                    {
                        panelRectTransform.Translate(new Vector3(0, -distance, 0));
                    }
                }

                if (panelCorners[0].x > canvasCorners[0].x) //snapLeft
                {
                    float distance = Vector3.Distance(canvasCorners[0], new Vector3(panelCorners[0].x, canvasCorners[0].y, canvasCorners[0].z));
                    if (distance < snapValue)
                    {
                        panelRectTransform.Translate(new Vector3(-distance, 0, 0));
                    }
                }

                if (panelCorners[2].y < canvasCorners[2].y) //snapUp
                {
                    float distance = Vector3.Distance(canvasCorners[2], new Vector3(canvasCorners[2].x, panelCorners[2].y, canvasCorners[2].z));
                    if (distance < snapValue)
                    {
                        panelRectTransform.Translate(new Vector3(0, distance, 0));
                    }
                }

                if (panelCorners[2].x < canvasCorners[2].x) //snapRight
                {
                    float distance = Vector3.Distance(canvasCorners[2], new Vector3(panelCorners[2].x, canvasCorners[2].y, canvasCorners[2].z));
                    if (distance < snapValue)
                    {
                        panelRectTransform.Translate(new Vector3(distance, 0, 0));
                    }
                }
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (eventData.button != PointerEventData.InputButton.Left)
                    return;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, eventData.pressPosition, null, out pointerOffset);

                RO.Media.CursorAnimator.CursorMode = RO.Media.CursorAnimator.CursorModes.Software;
                dragController.DragBegin();

                canvasRectTransform.SetAsLastSibling();
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (!dragController._isDraging)
                    return;

                RO.Media.CursorAnimator.CursorMode = RO.Media.CursorAnimator.CursorModes.Hardware;
                dragController.DragEnd();
            }
        }
    }

    public class DragArea : DragAreaController.DragArea { }
}