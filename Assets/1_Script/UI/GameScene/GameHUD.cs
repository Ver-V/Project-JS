using ProjectJS.Controller;
using ProjectJS.Manager;
using ProjectJS.UI.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectJS.UI.GameScene
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private PlayerStatusUI playerStatusUI;
        [SerializeField] private BossHPUI bossHPUI;
        [SerializeField] private PartyListUI partyListUI;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsUI settingsUI;
        [SerializeField] private bool isOpened;

        private void Start()
        {
            settingsUI.Init(CloseSettings);
            settingsButton.onClick.AddListener(ToggleSettings);
            Managers.PlayerInput.Player.OpenSettings.performed += OnOpenSettingsInput;

            isOpened = settingsUI.gameObject.activeSelf;
        }

        private void OnDestroy()
        {
            settingsButton.onClick.RemoveListener(ToggleSettings);
            Managers.PlayerInput.Player.OpenSettings.performed -= OnOpenSettingsInput;
        }

        public void BindLocalPlayer(Player player)
        {
            playerStatusUI.Bind(player);
        }

        public void UnbindLocalPlayer(Player player)
        {
            playerStatusUI.UnBind(player);
        }

        public void RegisterPartyPlayer(Player player)
        {
            partyListUI.RegisterPlayer(player);
        }

        public void UnregisterPartyPlayer(Player player)
        {
            partyListUI.UnregisterPlayer(player);
        }

        public void BindBoss(BossController boss)
        {
            bossHPUI.Bind(boss);
        }

        public void UnbindBoss(BossController boss)
        {
            bossHPUI.Bind(null);
        }

        private void OnOpenSettingsInput(InputAction.CallbackContext context)
        {
            ToggleSettings();
        }

        private void ToggleSettings()
        {
            if (isOpened)
            {
                CloseSettings();
                return;
            }

            OpenSettings();
        }

        private void OpenSettings()
        {
            if (isOpened) return;

            Debug.Log("[GameHUD] SettingsUI Opened");

            isOpened = true;
            settingsUI.gameObject.SetActive(isOpened);
            settingsUI.Refresh();
        }

        private void CloseSettings()
        {
            if (!isOpened) return;

            Debug.Log("[GameHUD] SettingsUI Closed");

            isOpened = false;
            settingsUI.gameObject.SetActive(isOpened);
        }
    }
}