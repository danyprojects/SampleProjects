using UnityEngine;

namespace Tests
{
    public class SoundTest : MonoBehaviour
    {
        public float bgmVolume = -0.3f;
        public float effectsVolume = -0.3f;

        private void Update()
        {
            RO.SoundController.SetBgmVolume(bgmVolume);
            RO.SoundController.SetEffectsVolume(effectsVolume);
        }
    }
}
