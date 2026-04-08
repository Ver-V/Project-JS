using ProjectJS.Structs;
using Steamworks;
using TMPro;
using UnityEngine;

namespace ProjectJS.UI.LobbyScene.Items
{
    public class PlayerCard : MonoBehaviour
	{
		[SerializeField] private TMP_Text playerName;
		[SerializeField] private bool isHost;
		[SerializeField] private GameObject hostMark;

        public GameObject readyImage;
		public ulong steamId;
		public ulong clientId;
		

		public void SetPlayerCard(PlayerInfo playerInfo, bool isHost=false)
		{
			playerName.text = playerInfo.steamName;
			steamId = playerInfo.steamId;
			this.isHost = isHost;

			if (this.isHost)
			{
				hostMark.SetActive(true);
			}
			if (steamId == SteamClient.SteamId)
			{
				playerName.color = Color.blue;
			}
        }

		private void Start()
		{
			readyImage.SetActive(false);
			if(!isHost) hostMark.SetActive(false);
		}
	}
}