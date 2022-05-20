using UnityEngine;

namespace RO.UI
{
    public class SimpleCursorButton : SimpleButton
        , IPointerExitHandler, IPointerEnterHandler
        , ICanvasRaycastFilter
    {
        public new bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter filter = ~(CanvasFilter.NpcDialog
                 | CanvasFilter.ModalMsgDialog | CanvasFilter.DisconnectDialog);

            return (filter & UIController.Panel.CanvasFilter) == 0 && enabled;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.SetAnimation(RO.Media.CursorAnimator.Animations.Click);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);
        }
    }
}
