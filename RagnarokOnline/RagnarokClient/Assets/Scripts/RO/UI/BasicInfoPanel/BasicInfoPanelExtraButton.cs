using System;
using UnityEngine;

namespace RO.UI
{
    class BasicInfoPanelExtraButton : Button
        , IPointerExitHandler, IPointerEnterHandler
    {
        public Action PointerEnter;
        public Action PointerExit;

        public void setAlpha(float value)
        {
            _image.color = new Color(_image.color.r, _image.color.g,
                                     _image.color.b, value);
        }

        public new void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            PointerEnter();
        }

        public new void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            PointerExit();
        }
    }
}
