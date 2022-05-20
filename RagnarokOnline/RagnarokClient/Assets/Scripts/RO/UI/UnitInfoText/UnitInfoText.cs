using TMPro;
using UnityEngine;

public sealed class UnitInfoText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameAndParty = default;
    [SerializeField] private TextMeshProUGUI _guildAndTitle = default;

    private string _unitName = "";
    private string _partyName = "";
    private string _guildName = "";
    private string _titleName = "";

    public void Show(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, screenPosition,
                                                                    null, out Vector2 movePos);
        transform.localPosition = movePos + new Vector2(20, -12);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetUnitName(string name)
    {
        _unitName = name;
        _nameAndParty.SetText($"{_unitName} ({_partyName})");
    }

    public void SetPartyName(string name)
    {
        _partyName = name;
        _nameAndParty.SetText($"{_unitName} ({_partyName})");
    }

    public void SetGuildName(string name)
    {
        _guildName = name;
        _guildAndTitle.SetText($"{_guildName} [{_titleName}]");
    }

    public void SetTitleName(string name)
    {
        _titleName = name;
        _guildAndTitle.SetText($"{_guildName} [{_titleName}]");
    }

    public static UnitInfoText Instantiate(Transform parent)
    {
        var unitInfoObj = AssetBundleProvider.LoadUiBundleAsset<GameObject>("UnitInfoText");

        unitInfoObj = Instantiate(unitInfoObj, parent, true);
        var unitInfo = unitInfoObj.GetComponentInChildren<UnitInfoText>();
        unitInfo.transform.SetAsFirstSibling();

        return unitInfo;
    }
}