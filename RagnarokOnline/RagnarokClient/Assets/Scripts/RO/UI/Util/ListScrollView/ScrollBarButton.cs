using RO.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ScrollBarButton : SimpleCursorButton
    , IPointerClickHandler
{
    [SerializeField]
    [FormerlySerializedAs("(Prev -1f) (Next 1f)")]
    private float operation = default;
    [SerializeField]
    private Scrollbar scrollBar = default;

    public new void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        scrollBar.value = Mathf.Clamp01(scrollBar.value + (operation / (scrollBar.numberOfSteps - 1)));
    }
}
