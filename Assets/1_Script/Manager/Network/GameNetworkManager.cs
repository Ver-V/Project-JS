using Netcode.Transports.Facepunch;
using ProjectJS.Utils;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJS.Manager
{
	public class GameNetworkManager : MonoBehaviour
	{
		private FacepunchTransport transport = null;

		public Lobby? currentLobby { get; private set; } = null;

		#region Singleton
		public static GameNetworkManager Instance { get => instance; }
		private static GameNetworkManager instance = null;
		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);

				// Fix: SteamManager Init이 중복 실행되서 주석처리 했습니다.
				//if (GetComponent<SteamManager>() == null)
				//{
				//	gameObject.AddComponent<SteamManager>();
				//}
			}
			else
			{
				Destroy(gameObject);
				return;
			}
		}
		#endregion
		private void Update()
		{
			if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient)
			{
				ulong ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
				Debug.Log("PingRtt: " + ping + "ms");
			}
		}

		private void Start()
		{
			transport = GetComponent<FacepunchTransport>();

			SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
			SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
			SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyJoined;
			SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyLeaved;
			SteamMatchmaking.OnLobbyInvite += SteamMatchMaking_OnLobbyInvite;
			SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
			SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
		}
		private void OnDestroy()
		{
			SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
			SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
			SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyJoined;
			SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyLeaved;
			SteamMatchmaking.OnLobbyInvite -= SteamMatchMaking_OnLobbyInvite;
			SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmaking_OnLobbyGameCreated;
			SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;
			NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadedInNetwork;

			if (NetworkManager.Singleton == null) return;

			NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
			NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
			NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectedCallback;
		}
		private void OnApplicationQuit()
		{
			Disconnected();
		}

		#region Lobby Callbacks
		private void SteamMatchmaking_OnLobbyCreated(Result result, Lobby lobby)
		{
			if (result != Result.OK)
			{
				Debug.Log("Lobby was not created, result: " + result);
				return;
			}

			// Init Lobby data
			lobby.SetPublic();
			lobby.SetJoinable(true);
			Debug.Log($"Lobby created : {lobby.Owner.Name}");

			// Start Host
			StartHost();
		}
		private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
		{
			Debug.Log("Lobby entered");
			if (lobby.Owner.Id == SteamClient.SteamId) return;

			currentLobby = lobby;
			StartClient(lobby.Owner.Id);
		}
		private void SteamMatchmaking_OnLobbyJoined(Lobby lobby, Friend friend)
		{
			Debug.Log("member join");
		}

		// Only Lobby Owner
		private void SteamMatchmaking_OnLobbyLeaved(Lobby lobby, Friend friend)
		{
			Debug.Log("member leave");
			if (friend.Id == lobby.Owner.Id)
			{
				Debug.Log("HOST LEAVED");
			}
			GameManagerEx.Instance.SendMessageToChat($"{friend.Name} has left", friend.Id, true);
			NetworkTransmission.instance.RemoveMeFromDictionaryServerRPC(friend.Id);
		}
		private void SteamMatchMaking_OnLobbyInvite(Friend friend, Lobby lobby)
		{
			Debug.Log($"Invite from {friend.Name}");
		}
		private void SteamMatchmaking_OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
		{
			Debug.Log("LobbyGame created");
			GameManagerEx.Instance.SendMessageToChat($"LobbyGame created : ", NetworkManager.Singleton.LocalClientId, true);
		}

		// Accept the invice or join on a friend
		private async void SteamFriends_OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
		{
			RoomEnter joinedLobby = await lobby.Join();

			if (joinedLobby != RoomEnter.Success)
			{
				Debug.Log("Failed to create lobby");
			}
			else
			{
				Debug.Log("Joined Lobby");
			}
		}
		#endregion

		#region FromLobbyToGame Sequences
		public void StartHost()
		{
			Debug.Log("START HOST...");
			NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
			NetworkManager.Singleton.StartHost();
			NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectedCallback;
			GameManagerEx.Instance.MyClientId = NetworkManager.Singleton.LocalClientId;
		}
		public async void CreateLobby()
		{
			Debug.Log("Create lobby...");
			currentLobby = await SteamMatchmaking.CreateLobbyAsync(Constants.MAX_PLAYERS);
			currentLobby.Value.SetData(Constants.KEY_LOBBYNAME, $"{SteamClient.Name}'s lobby");
			currentLobby.Value.SetData(Constants.KEY_GAMENAME, Constants.VALUE_GAMENAME);
		}
		public void StartClient(SteamId steamId)
		{
			Debug.Log("Start client...");
			NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
			NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectedCallback;
			transport.targetSteamId = steamId.Value;
			GameManagerEx.Instance.MyClientId = NetworkManager.Singleton.LocalClientId;

			if (NetworkManager.Singleton.StartClient())
			{
				Debug.Log("StartClient...");
			}
		}
		public void StartGameInLobby()
		{
			if (!NetworkManager.Singleton.IsHost) return;
			if (!GameManagerEx.Instance.IsAllPlayerReady()) return;

			NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += OnSceneUnloaded;
			NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneloaded;
			
			// 이 부분이 플레이어 스폰을 담당하는 핵심 이벤트 연결입니다.
			NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadedInNetwork;

			currentLobby.Value.SetGameServer(currentLobby.Value.Owner.Id);
			Debug.Log("Start Game in lobby...");
			LockLobby();

			NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
		}
		private void OnSceneLoadedInNetwork(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
			Debug.Log($"Scene loaded by Server {sceneName}");
			if (sceneName == "GameScene" || sceneName == "testScene")
			{
				if (!NetworkManager.Singleton.IsHost) return;

				Debug.Log($"{sceneName} loaded on all clients. Spawning players...");

				foreach (ulong clientId in GameManagerEx.Instance.playerInfo.Keys)
				{
					NetworkTransmission.instance.SpawnPlayer(clientId, GetRandomSpawnPosition());
				}

				NetworkTransmission.instance.StartGameServerRPC();
			}
		}

		private Vector3 GetRandomSpawnPosition()
		{
			Vector3 spawnPos = Vector3.zero;
			spawnPos.x = UnityEngine.Random.Range(-1f, 1f);
			spawnPos.y = UnityEngine.Random.Range(-1f, 1f);
			spawnPos.z = 0f;

			return spawnPos;
		}

		public async void Disconnected()
		{
			Debug.Log("DISCONNECTED");

			NetworkTransmission.instance.OnEndGame();
			if (NetworkManager.Singleton.IsHost)
			{
				NetworkTransmission.instance.DisconnectAllClientRPC();
			}

			if (currentLobby == null) return;
			currentLobby?.Leave();
			currentLobby = null;

			if (NetworkManager.Singleton == null) return;
			if (NetworkManager.Singleton.IsHost)
			{
				NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
			}
			else
			{
				NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
			}

			GameManagerEx.Instance.Disconnected();
			NetworkManager.Singleton.Shutdown(true);
			Debug.Log("Shutdown.");
		}
		public async void FindLobbiesWithCallback(System.Action<Lobby[]> callback)
		{
			// Filtering
			var query = SteamMatchmaking.LobbyList
				.WithKeyValue(Constants.KEY_GAMENAME, Constants.VALUE_GAMENAME)
				.FilterDistanceClose();

			var lobbies = await query.RequestAsync();

			Debug.Log("LOBBY BEFORE");
			if (lobbies == null) return;
			Debug.Log("LOBBY AFTER");

			callback.Invoke(lobbies);
			return;
		}
		public async void JoinLobby(Lobby lobby)
		{
			try
			{
				await lobby.Join();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning($"Lobby enter failed : {e.Message}");
			}
		}
		public void LockLobby()
		{
			currentLobby.Value.SetJoinable(false);
		}
		public void UnlockLobby()
		{
			currentLobby.Value.SetJoinable(true);
		}
		#endregion

		// Both Server and Client
		// 근데 이거 Client에서는 실행이 안되네
		private void Singleton_OnClientDisconnectedCallback(ulong clientId)
		{
			Debug.Log("Client Disconnected, ClientID: " + clientId);
			if (clientId == NetworkManager.Singleton.LocalClientId)
			{
				Disconnected();
				NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectedCallback;
			}
		}

		public void OpenInviteWindow()
		{
			SteamFriends.OpenGameInviteOverlay(currentLobby.Value.Id);
		}

		// Both Server and Client
		private void Singleton_OnClientConnectedCallback(ulong clientId)
		{
			if (NetworkManager.Singleton.IsHost) return;
			GameManagerEx.Instance.ConnectedAsClient();
			Managers.Scene.UnloadCurrentScene();

			NetworkTransmission.instance.AddMeToDictionayServerRPC(SteamClient.SteamId, SteamClient.Name, clientId);
			GameManagerEx.Instance.MyClientId = clientId;

			NetworkTransmission.instance.IsTheClientReadyServerRPC(false, clientId);
			Debug.Log($"Client has connected : {clientId}");

			NetworkTransmission.instance.StartHeartbeat();
		}
		private void Singleton_OnServerStarted()
		{
			Debug.Log("OnServerStarted Callback...");
			GameManagerEx.Instance.HostCreated();

			NetworkTransmission.instance.AddMeToDictionayServerRPC(SteamClient.SteamId, SteamClient.Name, NetworkManager.Singleton.LocalClientId);
		}

		private void OnSceneUnloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
			Debug.Log("Unload Complete! Curscene: " + Managers.Scene.CurrentScene);
		}
		private void OnSceneloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
			Debug.Log("Load Complete! Curscene: " + Managers.Scene.CurrentScene);
		}
	}

}