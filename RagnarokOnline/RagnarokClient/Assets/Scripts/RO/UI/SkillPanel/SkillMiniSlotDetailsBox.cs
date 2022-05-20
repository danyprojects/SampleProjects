using UnityEngine;

namespace RO.UI
{
    public sealed class SkillMiniSlotDetailsBox : MonoBehaviour,
        IPointerExitHandler, IPointerEnterHandler
    {
        [SerializeField]
        private SkillMiniSlotController _controller = default;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _controller.DetailsBoxOnPointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _controller.DetailsBoxOnPointerExit(eventData);
        }
    }
}