using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bacterio.UI
{
    public class BackgroundCanvas : MonoBehaviour
    {
        [SerializeField] private RawImage _backgroundImage = null;
        private Texture _screenGlow = null;

        public void ReloadBackgrounds()
        {
            _screenGlow = GlobalContext.assetBundleProvider.LoadUIImageAsset<Sprite>("Background_ScreenGlow").texture;
    }

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        public void SetDimmedBackground()
        {
            GetComponent<Canvas>().sortingLayerID = Constants.OVERLAY_SPRITE_LAYER;
            _backgroundImage.texture = _screenGlow;
        }
    }
}
