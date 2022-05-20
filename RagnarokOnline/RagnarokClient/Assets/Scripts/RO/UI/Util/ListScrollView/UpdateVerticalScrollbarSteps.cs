using UnityEngine;
using UnityEngine.UI;

public class UpdateVerticalScrollbarSteps : MonoBehaviour
{
    [SerializeField]
    private Scrollbar verticalScrollbar = default;
    [SerializeField]
    private RectTransform contentRectTransform = default;
    [SerializeField]
    private RectTransform scrollViewRectTransform = default;
    [SerializeField]
    private float rowHeight = default;

    private int lastRowCount = -1;
    private float lastHeight = -1;


    void OnRectTransformDimensionsChange()
    {
        updateStep();
    }

    private void updateStep()
    {
        var rowCount = (int)(contentRectTransform.rect.height / rowHeight);
        var height = scrollViewRectTransform.rect.height;

        if (rowCount != lastRowCount || height != lastHeight)
        {
            lastRowCount = rowCount;
            lastHeight = height;

            var visibleRows = (int)(height / rowHeight);
            verticalScrollbar.numberOfSteps = rowCount > visibleRows ? rowCount - visibleRows + 1 : 1;
        }
    }
}
