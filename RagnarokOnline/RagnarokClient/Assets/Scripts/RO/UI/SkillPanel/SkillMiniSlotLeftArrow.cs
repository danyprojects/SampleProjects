using UnityEngine;

public sealed class SkillMiniSlotLeftArrow : RO.UI.SimpleCursorButton
{
    [SerializeField]
    private RectTransform lvl = default;

    private void OnEnable()
    {
        // adjusting 1pixel for alignment reasons
        lvl.anchoredPosition += new Vector2(((RectTransform)transform).rect.width + 1, 0);
    }

    private void OnDisable()
    {
        // adjusting 1pixel for alignment reasons
        lvl.anchoredPosition -= new Vector2(((RectTransform)transform).rect.width + 1, 0);
    }
}
