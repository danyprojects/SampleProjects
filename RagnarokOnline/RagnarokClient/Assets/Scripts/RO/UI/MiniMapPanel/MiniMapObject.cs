using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class MiniMapObject : MonoBehaviour,
        IPointerExitHandler, IPointerEnterHandler
    {
        [SerializeField]
        private Image image = default;
        public Sprite OnHoverSprite;

        public void OnPointerEnter(PointerEventData eventData)
        {
            image.overrideSprite = OnHoverSprite;
            image.SetNativeSize();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            image.overrideSprite = null;
            image.SetNativeSize();
        }
    }
}