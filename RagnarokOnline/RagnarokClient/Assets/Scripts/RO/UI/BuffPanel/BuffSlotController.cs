using UnityEngine;

namespace RO.UI
{
    public sealed partial class BuffPanelController : UIController.Panel
    {
        public class SlotController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            [SerializeField]
            private BuffPanelController _buffPanel = default;
            public int Index = -1;

            public void OnPointerEnter(PointerEventData eventData)
            {
                Debug.Log("Mouse entered " + _buffPanel._buffSlotIds[Index]);
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                Debug.Log("Mouse Exited " + _buffPanel._buffSlotIds[Index]);
            }
        }
    }

    public class BuffSlotController : BuffPanelController.SlotController { }
}