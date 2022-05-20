using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed class SoundPanelController : UIController.Panel
        , IPointerDownHandler
    {
        [SerializeField]
        private Scrollbar scrollbarEffect = default;
        [SerializeField]
        private Scrollbar scrollbarBGM = default;
        [SerializeField]
        private Toggle effectOnCheckbox = default;
        [SerializeField]
        private Toggle bgmOnCheckbox = default;
        [SerializeField]
        private SimpleCursorButton effectOnCheckboxBtn = default;
        [SerializeField]
        private SimpleCursorButton bgmOnCheckboxBtn = default;
        [SerializeField]
        private Button closeButton = default;

        private void Awake()
        {
            scrollbarEffect.onValueChanged.AddListener(OnEffectScrollBarValueChanged);
            scrollbarBGM.onValueChanged.AddListener(OnBGMScrollBarValueChanged);

            effectOnCheckbox.OnValueChanged = OnEffectCheckboxChanged;
            bgmOnCheckbox.OnValueChanged = OnBGMCheckboxChanged;

            effectOnCheckboxBtn.OnClick = () => effectOnCheckbox.OnPointerClick(null);
            bgmOnCheckboxBtn.OnClick = () => bgmOnCheckbox.OnPointerClick(null);

            scrollbarEffect.value = SoundController.DEFAULT_EFFECTS_VOLUME;
            scrollbarBGM.value = SoundController.DEFAULT_BGM_VOLUME;

            closeButton.OnClick = () => gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        private void OnBGMCheckboxChanged(bool value)
        {
            SoundController.MuteBgm(value);
        }

        private void OnEffectCheckboxChanged(bool value)
        {
            SoundController.MuteEffects(value);
        }

        private void OnBGMScrollBarValueChanged(float value)
        {
            SoundController.SetBgmVolume(value);
        }

        private void OnEffectScrollBarValueChanged(float value)
        {
            SoundController.SetEffectsVolume(value);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public static SoundPanelController Instantiate(UIController uiController, Transform parent)
        {
            var controller = Instantiate<SoundPanelController>(uiController, parent, "SoundPanel");
            controller.gameObject.SetActive(true);

            return controller;
        }
    }
}