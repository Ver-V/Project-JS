using ProjectJS.Manager;
using ProjectJS.UI.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.MainScene
{
	public class MainSceneUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
		[SerializeField] private Button settingsButton;
		[SerializeField] private Button exitGameButton;
		[SerializeField] private SettingsUI settingsUI;

		private void Start()
		{
            settingsUI?.gameObject.SetActive(false);
			settingsUI.Init(() => { settingsUI.gameObject.SetActive(false); });

			hostButton.onClick.AddListener(() =>
			{
				GameNetworkManager.Instance.CreateLobby();
			});

			clientButton.onClick.AddListener(() =>
			{
				GameNetworkManager.Instance.FindLobbiesWithCallback((lobbies) =>
				{
					GameNetworkManager.Instance.JoinLobby(lobbies[0]);
				});
			});

            settingsButton.onClick.AddListener( () => 
            {
                settingsUI?.gameObject.SetActive(true);
            });

            exitGameButton.onClick.AddListener(() =>
            {
				
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
            });
            
        }

		private void OnDestroy()
		{
			hostButton.onClick.RemoveAllListeners();
            clientButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
        }
	}
}