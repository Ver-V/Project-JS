using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.Settings
{
    public class DisplaySettingsUI : MonoBehaviour
    {
        private const string ScreenWidthKey = "Setting_ScreenWidth";
        private const string ScreenHeightKey = "Setting_ScreenHeight";
        private const string WindowModeKey = "Setting_WindowMode";

        [Header("Resolution")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [Header("Window Mode Toggles")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle borderlessToggle;
        [SerializeField] private Toggle windowedToggle;

        private readonly List<Vector2Int> supportedResolutions = new()
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440)
        };

        private bool isInitialized;

        public void Init()
        {
            if (isInitialized) return;

            InitResolutionDropdown();
            LoadSettings();

            isInitialized = true;
        }

        public void Apply()
        {
            Vector2Int selectedResolution = supportedResolutions[resolutionDropdown.value];
            int selectedWindowModeIndex = GetSelectedWindowModeIndex();
            FullScreenMode selectedMode = GetSelectedFullScreenMode(selectedWindowModeIndex);

            Screen.SetResolution(selectedResolution.x, selectedResolution.y, selectedMode);

            PlayerPrefs.SetInt(ScreenWidthKey, selectedResolution.x);
            PlayerPrefs.SetInt(ScreenHeightKey, selectedResolution.y);
            PlayerPrefs.SetInt(WindowModeKey, selectedWindowModeIndex);
            PlayerPrefs.Save();
        }

        public void Refresh()
        {
            LoadSettings();
        }

        private void InitResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();

            List<string> options = new();
            int currentIndex = 0;

            for (int i = 0; i < supportedResolutions.Count; i++)
            {
                Vector2Int res = supportedResolutions[i];
                options.Add($"{res.x} x {res.y}");

                if (Screen.width == res.x && Screen.height == res.y)
                    currentIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.SetValueWithoutNotify(currentIndex);
            resolutionDropdown.RefreshShownValue();
        }

        private void LoadSettings()
        {
            int savedWidth = PlayerPrefs.GetInt(ScreenWidthKey, Screen.width);
            int savedHeight = PlayerPrefs.GetInt(ScreenHeightKey, Screen.height);
            int savedWindowMode = PlayerPrefs.GetInt(WindowModeKey, 2);

            int resolutionIndex = FindResolutionIndex(savedWidth, savedHeight);

            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
            resolutionDropdown.RefreshShownValue();

            SetWindowModeToggle(savedWindowMode);
        }

        private int FindResolutionIndex(int width, int height)
        {
            for (int i = 0; i < supportedResolutions.Count; i++)
            {
                if (supportedResolutions[i].x == width && supportedResolutions[i].y == height)
                    return i;
            }

            return 0;
        }

        private int GetSelectedWindowModeIndex()
        {
            if (fullscreenToggle.isOn) return 0;
            if (borderlessToggle.isOn) return 1;
            return 2;
        }

        private FullScreenMode GetSelectedFullScreenMode(int index)
        {
            return index switch
            {
                0 => FullScreenMode.ExclusiveFullScreen,
                1 => FullScreenMode.FullScreenWindow,
                2 => FullScreenMode.Windowed,
                _ => FullScreenMode.Windowed
            };
        }

        private void SetWindowModeToggle(int windowModeIndex)
        {
            fullscreenToggle.SetIsOnWithoutNotify(windowModeIndex == 0);
            borderlessToggle.SetIsOnWithoutNotify(windowModeIndex == 1);
            windowedToggle.SetIsOnWithoutNotify(windowModeIndex == 2);
        }
    }
}