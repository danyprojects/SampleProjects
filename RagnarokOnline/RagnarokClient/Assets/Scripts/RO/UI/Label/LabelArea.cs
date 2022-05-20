using System;
using UnityEngine;

namespace RO.UI
{
    public partial class UIController : MonoBehaviour
    {
        public abstract partial class Panel : MonoBehaviour, ICanvasRaycastFilter
        {
            public class LabelArea : MonoBehaviour,
                IPointerEnterHandler, IPointerExitHandler
            {
                public Action OnEnter;
                private Panel _panel;

                void Awake()
                {
                    _panel = GetComponentInParent<Panel>();
                }

                public void OnPointerEnter(PointerEventData eventData)
                {
                    OnEnter();
                }

                public void OnPointerExit(PointerEventData eventData)
                {
                    _panel.LabelController.HideLabel();
                }
            }
        }
    }

    public sealed class LabelArea : UIController.Panel.LabelArea { }
}
