using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

namespace ProjectJS.UI.Settings
{
    public class SoundSettingsUI : MonoBehaviour
    {
        private const string MasterVolumeKey = "Setting_MasterVolume";
        private const string BGMVolumeKey = "Setting_BGMVolume";
        private const string SFXVolumeKey = "Setting_SFXVolume";

        private const string MasterVolumeParam = "MasterVolume";
        private const string BGMVolumeParam = "BGMVolume";
        private const string SFXVolumeParam = "SFXVolume";

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeValueText;
        [SerializeField] private TMP_Text bgmVolumeValueText;
        [SerializeField] private TMP_Text sfxVolumeValueText;

        private bool isInitialized;

        public void Init()
        {
            if (isInitialized) return;

            LoadVolumes();
            AddListeners();

            isInitialized = true;
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void LoadVolumes()
        {
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 0.5f);
            float bgmVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 0.5f);
            float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.5f);

            masterVolumeSlider.SetValueWithoutNotify(masterVolume);
            bgmVolumeSlider.SetValueWithoutNotify(bgmVolume);
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);

            ApplyVolume(MasterVolumeParam, masterVolume);
            ApplyVolume(BGMVolumeParam, bgmVolume);
            ApplyVolume(SFXVolumeParam, sfxVolume);

            RefreshText(masterVolumeValueText, masterVolume);
            RefreshText(bgmVolumeValueText, bgmVolume);
            RefreshText(sfxVolumeValueText, sfxVolume);
        }

        private void AddListeners()
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        private void RemoveListeners()
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }

        private void OnMasterVolumeChanged(float value)
        {
            SetVolume(MasterVolumeKey, MasterVolumeParam, value, masterVolumeValueText);
        }

        private void OnBGMVolumeChanged(float value)
        {
            SetVolume(BGMVolumeKey, BGMVolumeParam, value, bgmVolumeValueText);
        }

        private void OnSFXVolumeChanged(float value)
        {
            SetVolume(SFXVolumeKey, SFXVolumeParam, value, sfxVolumeValueText);
        }

        private void SetVolume(string saveKey, string mixerParam, float value, TMP_Text volumeText)
        {
            value = Mathf.Clamp01(value);

            ApplyVolume(mixerParam, value);
            PlayerPrefs.SetFloat(saveKey, value);
            PlayerPrefs.Save();
            RefreshText(volumeText, value);
        }

        private void ApplyVolume(string mixerParam, float value)
        {
            if (audioMixer == null)
            {
                Debug.LogWarning("[SoundSettingsUI] : Audio Mixer is null");
                return;
            }

            float dbValue = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(mixerParam, dbValue);
        }

        private void RefreshText(TMP_Text volumeValueText, float value)
        {
            if (volumeValueText == null)
            {
                Debug.LogWarning($"[SoundSettingsUI] : Volume Value Text({volumeValueText.name})is null");
                return;
            }

            volumeValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }
}