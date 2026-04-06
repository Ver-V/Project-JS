using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.MainScene
{
    public class DisplaySettingsUI : MonoBehaviour
    {
        [Header("Resolution")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [Header("Window Mode Toggles")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle borderlessToggle;
        [SerializeField] private Toggle windowedToggle;

        private readonly List<Vector2Int> _supportedResolutions = new()
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440)
        };

        private void Start()
        {
            InitResolutionDropdown();
            LoadSettings();
        }

        private void InitResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();

            List<string> options = new();
            int currentIndex = 0;

            for (int i = 0; i < _supportedResolutions.Count; i++)
            {
                Vector2Int res = _supportedResolutions[i];
                options.Add($"{res.x} x {res.y}");

                if (Screen.width == res.x && Screen.height == res.y)
                {
                    currentIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentIndex;
            resolutionDropdown.RefreshShownValue();
        }

        public void OnClickApply()
        {
            Vector2Int selectedResolution = _supportedResolutions[resolutionDropdown.value];
            int selectedWindowModeIndex = GetSelectedWindowModeIndex();
            FullScreenMode selectedMode = GetSelectedFullScreenMode(selectedWindowModeIndex);

            Screen.SetResolution(selectedResolution.x, selectedResolution.y, selectedMode);

            PlayerPrefs.SetInt("ScreenWidth", selectedResolution.x);
            PlayerPrefs.SetInt("ScreenHeight", selectedResolution.y);
            PlayerPrefs.SetInt("WindowMode", selectedWindowModeIndex);
            PlayerPrefs.Save();
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

        private void LoadSettings()
        {
            int savedWidth = PlayerPrefs.GetInt("ScreenWidth", 1920);
            int savedHeight = PlayerPrefs.GetInt("ScreenHeight", 1080);
            int savedWindowMode = PlayerPrefs.GetInt("WindowMode", 2);

            int resolutionIndex = 0;

            for (int i = 0; i < _supportedResolutions.Count; i++)
            {
                if (_supportedResolutions[i].x == savedWidth &&
                    _supportedResolutions[i].y == savedHeight)
                {
                    resolutionIndex = i;
                    break;
                }
            }

            resolutionDropdown.value = resolutionIndex;
            resolutionDropdown.RefreshShownValue();

            SetWindowModeToggle(savedWindowMode);
        }

        private void SetWindowModeToggle(int windowModeIndex)
        {
            fullscreenToggle.isOn = (windowModeIndex == 0);
            borderlessToggle.isOn = (windowModeIndex == 1);
            windowedToggle.isOn = (windowModeIndex == 2);
        }
    }
}