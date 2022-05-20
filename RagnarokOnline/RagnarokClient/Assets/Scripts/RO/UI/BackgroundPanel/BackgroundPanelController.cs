using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    // This one does NOT extend UI.Panel on purpose we don't need it
    // and we don't want to take part in panel ordering 
    public class BackgroundPanelController : MonoBehaviour
    {
        [SerializeField] private Sprite[] _backgroudImages = default;
        [SerializeField] private Sprite[] _loadingImages = default;
        [SerializeField] private Slider _slider = default;
        [SerializeField] private Text _loadingPercentage = default;
        [SerializeField] private Material _blackScreenMaterial = default;

        //Shader will lerp alpha between r and g, meaning it will always be alpha of 1 (stays black)
        private readonly Color _stayBlack = new Color(1 / 255f, 1 / 255f, Media.MediaConstants.BLACK_FADE_TIME / 2f); //Fade can go up to 2 seconds
        //Shader will lerp alpha between r and g, meaning it will become blacker with elapsed time
        private readonly Color _fadeOutToBlack = new Color(0, 1 / 255f, Media.MediaConstants.BLACK_FADE_TIME / 2f);
        //Shader will lerp alpha between r and g, meaning black will disappear with elapsed time
        private readonly Color _fadeInFromBlack = new Color(1 / 255f, 0, Media.MediaConstants.BLACK_FADE_TIME / 2f);

        private Image _background;
        private System.Random _random = new System.Random();

        private void Awake()
        {
            _background = GetComponent<Image>();
            gameObject.SetActive(false);
        }

        public void ShowBackgroundImage()
        {
            var index = _random.Next(0, _backgroudImages.Length);
            _background.sprite = _backgroudImages[index];

            _background.raycastTarget = true;
            _background.material = null;
            _background.color = Color.white;
            _background.gameObject.SetActive(true);
        }

        public void ShowBlackScreen()
        {
            _background.raycastTarget = true;
            _background.gameObject.SetActive(true);
            _background.material = _blackScreenMaterial;
            _background.color = _stayBlack;
        }

        public void ShowFadeOut()
        {
            _background.raycastTarget = false;
            _background.gameObject.SetActive(true);
            _blackScreenMaterial.SetFloat(Media.MediaConstants.SHADER_UI_FADER_START_TIME_ID, Common.Globals.TimeSinceLevelLoad);
            _background.material = _blackScreenMaterial;
            _background.color = _fadeOutToBlack;
        }

        public void ShowFadeIn()
        {
            _background.raycastTarget = false;
            _background.gameObject.SetActive(true);
            _blackScreenMaterial.SetFloat(Media.MediaConstants.SHADER_UI_FADER_START_TIME_ID, Common.Globals.TimeSinceLevelLoad);
            _background.material = _blackScreenMaterial;
            _background.color = _fadeInFromBlack;
        }

        public void EnterLoadingScreen()
        {
            _slider.gameObject.SetActive(true);

            var index = _random.Next(0, _loadingImages.Length);
            _background.sprite = _loadingImages[index];

            _background.material = null;
            _background.color = Color.white;
            _background.gameObject.SetActive(true);

            _slider.value = 0;
            _loadingPercentage.text = $"0%";
        }

        public void SetLoadProgress(float progress)
        {
            _slider.value = progress;
            _loadingPercentage.text = $"{(int)(progress * 100) }%";
        }

        public void ExitLoadingScreen()
        {
            _slider.gameObject.SetActive(false);
            _background.gameObject.SetActive(false);
        }

        public static BackgroundPanelController Instantiate(UIController uiController, Transform parent)
        {
            var panel = AssetBundleProvider.LoadUiBundleAsset<GameObject>("BackgroundPanel");
            panel = Instantiate(panel, parent, false);

            var controller = panel.GetComponentInChildren<BackgroundPanelController>();
            controller.gameObject.SetActive(true);

            return controller;
        }
    }
}