using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.Settings
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private DisplaySettingsUI displaySettingsUI;
        [SerializeField] private SoundSettingsUI soundSettingsUI;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;

        private Action onCloseRequested;
        private bool isInitialized;

        public void Init(Action onClose)
        {
            if (isInitialized) return;

            onCloseRequested = onClose;

            displaySettingsUI.Init();
            soundSettingsUI.Init();

            applyButton.onClick.AddListener(ApplySettings);
            closeButton.onClick.AddListener(RequestClose);

            isInitialized = true;
        }

        private void OnDestroy()
        {
            applyButton.onClick.RemoveListener(ApplySettings);
            closeButton.onClick.RemoveListener(RequestClose);
        }

        public void Refresh()
        {
            displaySettingsUI.Refresh();
        }

        private void ApplySettings()
        {
            displaySettingsUI.Apply();
        }

        private void RequestClose()
        {
            onCloseRequested?.Invoke();
        }
    }
}