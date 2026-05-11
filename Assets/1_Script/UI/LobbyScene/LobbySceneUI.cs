using ProjectJS.Manager;
using ProjectJS.Structs;
using ProjectJS.UI.LobbyScene.Items;
using ProjectJS.Utils;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.LobbyScene
{
	public class LobbySceneUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button notreadyButton;
        [SerializeField] private Button startButton;

        [Tooltip("초대 버튼")] [SerializeField] private Button inviteButton;

        [Space(20)]
        [Header("Chating System")]
		[SerializeField] private int maxMessages = 20;
		[SerializeField] private Transform chatPanel;
        [SerializeField] private TMP_InputField chatInputField;
        [SerializeField] private GameObject chatPrefab;

        [Space(20)]
        [Header("Player Cards")]
        [SerializeField] private Transform playerFieldBox;
        [SerializeField] private GameObject playerCardPrefab;


		private List<Message> messageList = new List<Message>();
		private List<PlayerCard> cardList = new List<PlayerCard>();

		public class Message
		{
			public string text;
			public TMP_Text textObject;
		}

		private void Start()
		{
			GameManagerEx.Instance.OnSendMessageAction += SendMessage;
			GameManagerEx.Instance.OnAddPlayerAction += OnAddPlayerToDictionary;
			GameManagerEx.Instance.OnRemovePlayerAction += OnRemovePlayerFromDictionary;
			GameManagerEx.Instance.OnUpdatePlayerReadyAction += OnUpdatePlayerReady;

            foreach (var pair in GameManagerEx.Instance.playerInfo)
            {
                OnAddPlayerToDictionary(pair.Value);
            }

            disconnectButton.onClick.AddListener(GameNetworkManager.Instance.Disconnected);
			readyButton.onClick.AddListener(() => {
				NetworkTransmission.instance.IsTheClientReadyServerRPC(true, GameManagerEx.Instance.MyClientId);
			}); 

			notreadyButton.onClick.AddListener(() => {
				NetworkTransmission.instance.IsTheClientReadyServerRPC(false, GameManagerEx.Instance.MyClientId);
			});

			startButton.onClick.AddListener(() =>
			{
				Managers.Scene.ChangeScene(SceneEnum.Game);
			});

            inviteButton.onClick.AddListener(() =>
            {
                Debug.Log("초대 버튼 클릭됨");

                Debug.Log($"SteamClient.IsValid = {SteamClient.IsValid}");
                Debug.Log($"Overlay Enabled = {SteamUtils.IsOverlayEnabled}");
                Debug.Log($"CurrentLobby HasValue = {GameNetworkManager.Instance.currentLobby.HasValue}");

                if (!SteamClient.IsValid)
                {
                    Debug.LogWarning("SteamClient가 유효하지 않음");
                    return;
                }

                if (!SteamUtils.IsOverlayEnabled)
                {
                    Debug.LogWarning("Steam Overlay가 아직 활성화되지 않았음");
                    return;
                }

                if (!GameNetworkManager.Instance.currentLobby.HasValue)
                {
                    Debug.LogWarning("현재 로비가 없음");
                    return;
                }

                GameNetworkManager.Instance.OpenInviteWindow();
            });

        }

		private void OnDestroy()
		{
			GameManagerEx.Instance.OnSendMessageAction -= SendMessage;
			GameManagerEx.Instance.OnAddPlayerAction -= OnAddPlayerToDictionary;
			GameManagerEx.Instance.OnRemovePlayerAction -= OnRemovePlayerFromDictionary;
			GameManagerEx.Instance.OnUpdatePlayerReadyAction -= OnUpdatePlayerReady;
		}


		private void Update()
		{
			if (chatInputField.text != "")
			{
				if (Input.GetKeyDown(KeyCode.Return))
				{
					if (chatInputField.text == " ")
					{
						chatInputField.text = "";
						chatInputField.DeactivateInputField();
						return;
					}
					NetworkTransmission.instance.IWishToSendAChatServerRPC(chatInputField.text, NetworkManager.Singleton.LocalClientId);
					chatInputField.text = "";
				}
			}
			else
			{
				if (Input.GetKeyDown(KeyCode.Return))
				{
					chatInputField.ActivateInputField();
					chatInputField.text = " ";
				}
			}
		}

		private void SendMessage(string name, string text)
        {
			if (messageList.Count >= maxMessages)
			{
				Destroy(messageList[0].textObject.gameObject);
				messageList.Remove(messageList[0]);
			}
			Message newMessage = new Message();

			newMessage.text = name + ": " + text;
			GameObject newText = Instantiate(chatPrefab, chatPanel.transform);
			newMessage.textObject = newText.GetComponent<TMP_Text>();
			newMessage.textObject.text = newMessage.text;

			messageList.Add(newMessage);
		}

		private void ClearChat()
		{
			messageList.Clear();
			GameObject[] chat = GameObject.FindGameObjectsWithTag(Constants.TAG_CHAT);
			foreach (GameObject chit in chat)
			{
				Destroy(chit);
			}
		}

		private void OnAddPlayerToDictionary(PlayerInfo pi)
		{
            for(int i=0;i<cardList.Count; i++) 
            {
                if (cardList[i].steamId == pi.steamId)
                    return;
            }

            PlayerCard pc = Instantiate(playerCardPrefab, playerFieldBox).GetComponent<PlayerCard>();


            bool isHostCard = false;
            if (GameNetworkManager.Instance.currentLobby.HasValue)
            {
                isHostCard = pi.steamId == GameNetworkManager.Instance.currentLobby.Value.Owner.Id;
            }

			pc.SetPlayerCard(pi, isHostCard);

			cardList.Add(pc);
		}

		private void OnRemovePlayerFromDictionary(PlayerInfo pi)
		{
			for(int i=cardList.Count-1; i>=0; i--)
			{
				if (cardList[i].steamId == pi.steamId)
				{
					Destroy(cardList[i].gameObject);
					cardList.RemoveAt(i);
				}
			}
		}

		private void OnUpdatePlayerReady(bool isReady, ulong steamId)
		{
			bool isAllReady = true;
			foreach (PlayerCard card in cardList)
			{
				if (card.steamId == steamId)
				{
					card.readyImage.SetActive(isReady);
				}

				if (!card.readyImage.activeSelf) isAllReady = false;
			}

			if (SteamClient.SteamId == steamId)
			{
				readyButton.gameObject.SetActive(!isReady);
				notreadyButton.gameObject.SetActive(isReady);
			}

			// Host�� ���ӽ��۹�ư ����ߵ�
			if (NetworkManager.Singleton.IsHost)				
			{
				startButton.gameObject.SetActive(isAllReady);
			}
		}
	}
}