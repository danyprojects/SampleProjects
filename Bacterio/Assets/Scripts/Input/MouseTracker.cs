using UnityEngine;
using UnityEngine.EventSystems;

namespace Bacterio.Input
{
    public sealed class MouseTracker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static bool MouseIsInUI = false;
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            MouseIsInUI = false;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            MouseIsInUI = true;
        }
    }
}
