using ProjectJS.Controller;
using ProjectJS.Manager;
using ProjectJS.UI.Settings;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectJS.UI.GameScene
{
    public class GameSceneUI : MonoBehaviour
    {
        public static GameSceneUI Instance { get; private set; }

        [SerializeField] private GameHUD gameHUD;

        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsUI settingsUI;
        [SerializeField] private bool isOpened;

        [SerializeField] private Button returnToLobbyButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            settingsUI.Init(CloseSettings);
            settingsButton.onClick.AddListener(ToggleSettings);
            returnToLobbyButton.onClick.AddListener(ReturnToLobby);
            Managers.PlayerInput.Player.OpenSettings.performed += OnOpenSettingsInput;

            isOpened = settingsUI.gameObject.activeSelf;

        }

        private void OnDestroy()
        {
            settingsButton.onClick.RemoveListener(ToggleSettings);
            Managers.PlayerInput.Player.OpenSettings.performed -= OnOpenSettingsInput;

            if (Instance == this)
                Instance = null;
        }

        public void RegisterPlayer(Player player)
        {
            if (player == null) return;

            if (player.IsOwner)
            {
                gameHUD.BindLocalPlayer(player);
                return;
            }

            gameHUD.RegisterPartyPlayer(player);
        }

        public void UnregisterPlayer(Player player)
        {
            if (player == null) return;

            if (player.IsOwner)
            {
                gameHUD.UnbindLocalPlayer(player);
                return;
            }

            gameHUD.UnregisterPartyPlayer(player);
        }

        public void RegisterBoss(BossController boss)
        {
            if (boss == null) return;
        
            gameHUD.BindBoss(boss);
        }
        
        public void UnregisterBoss(BossController boss)
        {
            if (boss == null) return;
        
            gameHUD.UnbindBoss(null);
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

        private void ReturnToLobby()
        {
            if (NetworkManager.Singleton == null) return;
            if (!NetworkManager.Singleton.IsHost) return;
            if (GameNetworkManager.Instance == null) return;

            GameNetworkManager.Instance.ReturnToLobbyFromGame();
        }
    }
}
