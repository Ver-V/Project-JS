using ProjectJS.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.MainScene
{
	public class MainSceneUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;

		private void Start()
		{
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
		}

		private void OnDestroy()
		{
			hostButton.onClick.RemoveAllListeners();
		}
	}
}