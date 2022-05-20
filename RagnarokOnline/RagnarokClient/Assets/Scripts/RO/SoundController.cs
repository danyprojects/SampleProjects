using RO.Databases;
using UnityEngine;
using UnityEngine.Audio;

namespace RO
{
    public sealed class SoundController
    {
        private const string EFFECT_STR = "Effects";
        private const string BACKGROUND_STR = "Background";

        private static readonly AudioMixer _audioMixer = null;
        private static readonly AudioSource _bgm = null;
        private static readonly AudioListener _listener = null;
        public readonly static AudioMixerGroup MixerEffectGroup;
        public readonly static AudioMixerGroup MixerBackgroundGroup;

        private static float _effectsVolume = 0;
        private static float _bgmVolume = 0;
        private static bool _effectsMuted = false;
        private static bool _bgmMuted = false;

        public const float DEFAULT_EFFECTS_VOLUME = 0.25f; //25%
        public const float DEFAULT_BGM_VOLUME = 0.25f; //25%

        public static void PlayLoginBgm()
        {
            _bgm.clip = AssetBundleProvider.LoadSoundBundleAsset("login");
            if (_bgm.clip != null)
                _bgm.Play();
        }

        public static void PlayMapBgm(int mapId)
        {
            _bgm.clip = AssetBundleProvider.LoadSoundBundleAsset(MapDb.MapBgms[mapId]);
            if (_bgm.clip != null)
                _bgm.Play();
        }

        public static void PlayAudioEffect(SoundDb.SoundIds soundId, AudioSource audioSource)
        {
            if (soundId == SoundDb.SoundIds.None)
                return;

            audioSource.clip = AssetBundleProvider.LoadSoundBundleAsset(SoundDb.AudioEffects[(int)soundId]);
            if (audioSource.clip != null)
                audioSource.Play();
        }

        public static void SetEffectsVolume(float volume)
        {
            volume += 0.0001f;
            volume = Mathf.Log10(volume * 0.4f) * 24 + 20 * volume;
            if (!_effectsMuted)
                _audioMixer.SetFloat(EFFECT_STR, volume);
            _effectsVolume = volume;
        }

        public static void SetBgmVolume(float volume)
        {
            volume += 0.0001f;
            volume = Mathf.Log10(volume * 0.4f) * 24 + 20 * volume;
            if (!_bgmMuted)
                _audioMixer.SetFloat(BACKGROUND_STR, volume);
            _bgmVolume = volume;
        }

        public static void MuteBgm(bool mute)
        {
            if (mute)
                _audioMixer.SetFloat(BACKGROUND_STR, -80);
            else
                _audioMixer.SetFloat(BACKGROUND_STR, _bgmVolume);

            _bgmMuted = mute;
        }

        public static void MuteEffects(bool mute)
        {
            if (mute)
                _audioMixer.SetFloat(BACKGROUND_STR, -80);
            else
                _audioMixer.SetFloat(BACKGROUND_STR, _effectsVolume);

            _effectsMuted = mute;
        }

        public static void SetUnitAudioSourceParams(AudioSource source)
        {
            source.outputAudioMixerGroup = MixerEffectGroup;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1;
            source.maxDistance = 80;
            source.spread = 130;
            source.spatialBlend = 0.7f;
            source.pitch = 1;
        }

        public static void MoveAudioListenerToObject(Transform transform)
        {
            if (transform == null)
                _listener.transform.SetParent(_bgm.transform);
            else
                _listener.transform.SetParent(transform);
        }

        static SoundController()
        {
            _audioMixer = AssetBundleProvider.LoadMiscBundleAsset<AudioMixer>("RagnarokMixer");

            MixerEffectGroup = _audioMixer.FindMatchingGroups(EFFECT_STR)[0];
            MixerBackgroundGroup = _audioMixer.FindMatchingGroups(BACKGROUND_STR)[0];

            _bgm = GameObject.Find("SoundController").GetComponent<AudioSource>();
            _bgm.outputAudioMixerGroup = MixerBackgroundGroup; //Unity will create a copy of the mixer due to asset bundles so we need to do this...

            _listener = new GameObject("Audio Listener").AddComponent<AudioListener>();
            _listener.transform.SetParent(_bgm.transform);

            GameObject.DontDestroyOnLoad(_bgm.gameObject);
            GameObject.DontDestroyOnLoad(_listener.gameObject);
        }
    }
}
