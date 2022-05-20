using UnityEngine;

namespace RO.UI
{
    // Just here for item events and such
    class QuickSlotsPanel : UIController.Panel
        , ICanvasRaycastFilter, IPointerDownHandler
    {
        public new bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter filter = ~(CanvasFilter.NpcDialog | CanvasFilter.ItemDrag | CanvasFilter.SkillDrag);

            return (filter & CanvasFilter) == 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public new void ForceSetUiController(UIController controller)
        {
            base.ForceSetUiController(controller);
        }
    }
}
