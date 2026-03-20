using ProjectJS.Controller;
using ProjectJS.Manager;
using Steamworks.Data;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{
	public class NetworkTransmission : NetworkBehaviour
	{
		[SerializeField] GameObject playerPrefab;
		private Dictionary<ulong, PlayerController> playerDict = new();

		#region Singleton
		public static NetworkTransmission instance;

		private void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
			}
			else
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
		}
		#endregion

		#region Heartbeat
		public float pingInterval = 2.0f;
		public float timeoutThreshold = 5.0f;

		private float lastPingTime;     // 가장 최근 받은 ping응답 시간
		private float pingTimer;        // ping 보내기 까지 남은 시간
		private float pingSentTime;     // 가장 최근 ping 보낸 시간
		private bool isDisconnected = true;

		private bool isHeartbeating = false;

		public float LastPingMs { get; private set; } = -1;

		private void Update()
		{
			if (!isHeartbeating) return;
			if (!IsClient || IsHost) return;

			pingTimer -= Time.deltaTime;

			if (pingTimer <= 0f)
			{
				if (NetworkManager.Singleton.IsConnectedClient)
				{
					SendPingServerRpc();
					pingSentTime = Time.time;
				}
				pingTimer = pingInterval;
			}

			if (!isDisconnected && (Time.time - lastPingTime) > timeoutThreshold)
			{
				isDisconnected = true;
				Debug.LogWarning("[HeartbeatChecker] : Host Disconnected");

				GameNetworkManager.Instance.Disconnected();
			}
		}

		public void StartHeartbeat()
		{
			return;

			if (IsClient && !IsHost)
			{
				lastPingTime = Time.time;
				pingTimer = pingInterval;
				isHeartbeating = true;
			}
		}

		public void EndHeartbeat()
		{
			return;

			isHeartbeating = false;
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendPingServerRpc(ServerRpcParams rpcParams = default)
		{
			if (!NetworkManager.Singleton.IsHost)
				return;

			ReceivePingClientRpc(rpcParams.Receive.SenderClientId);
		}
		[ClientRpc]
		private void ReceivePingClientRpc(ulong clientId)
		{
			if (clientId != NetworkManager.Singleton.LocalClientId)
				return;

			lastPingTime = Time.time;
			if (isDisconnected)
			{
				Debug.Log("[HeartbeatChecker] 서버 응답 복구됨!");
				isDisconnected = false;
			}

			// 핑 계산 (초 -> 밀리초 변환)
			LastPingMs = (Time.time - pingSentTime) * 1000.0f;
			Debug.Log($"[HeartbeatChecker] Ping: {LastPingMs:F0} ms");
		}


		#endregion

		[ServerRpc(RequireOwnership = false)]
		public void IWishToSendAChatServerRPC(string message, ulong fromwho)
		{
			ChatFromServerClientRPC(message, fromwho);
		}

		[ClientRpc]
		private void ChatFromServerClientRPC(string message, ulong fromwho)
		{
			GameManagerEx.Instance.SendMessageToChat(message, fromwho, false);
		}

		[ServerRpc(RequireOwnership = false)]
		public void AddMeToDictionayServerRPC(ulong steamId, string steamName, ulong clientId)
		{
			GameManagerEx.Instance.SendMessageToChat($"{steamName} has joined", clientId, true);
			GameManagerEx.Instance.AddPlayerToDictionary(clientId, steamName, steamId);
			GameManagerEx.Instance.UpdateClients();
		}

		[ClientRpc]
		public void UpdateClientsPlayerInfoClientRPC(ulong steamId, string steamName, ulong clientId)
		{
			GameManagerEx.Instance.AddPlayerToDictionary(clientId, steamName, steamId);
		}

		[ServerRpc(RequireOwnership = false)]
		public void RemoveMeFromDictionaryServerRPC(ulong steamId)
		{
			ulong clientId = GameManagerEx.Instance.GetClientIDBySteamID(steamId);
			DespawnPlayer(clientId);
			RemovePlayerFromDictionaryClientRPC(steamId);
		}

		[ClientRpc]
		public void RemovePlayerFromDictionaryClientRPC(ulong steamId)
		{
			Debug.Log("removing client");
			GameManagerEx.Instance.RemovePlayerFromDictionary(steamId);
		}

		[ServerRpc(RequireOwnership = false)]
		public void IsTheClientReadyServerRPC(bool ready, ulong clientId)
		{
			AClientMightBeReadyClientRPC(ready, clientId);
		}

		[ClientRpc]
		private void AClientMightBeReadyClientRPC(bool ready, ulong clientId)
		{
			GameManagerEx.Instance.UpdatePlayerIsReady(ready, clientId);
		}

		[ClientRpc]
		public void DisconnectAllClientRPC()
		{
			if (!IsHost)
			{
				return;
			}
			Debug.LogWarning("PlayerDict Clear");
			playerDict.Clear();
		}

		public bool isInGame = false;

		[ServerRpc]
		public void StartGameServerRPC()
		{
			if (!isInGame)
			{
				isInGame = true;
				StartGameClientRPC();
			}
		}
		[ClientRpc]
		public void StartGameClientRPC()
		{
			isInGame = true;
		}

		public void OnEndGame()
		{
			if (isInGame)
			{
				GameNetworkManager.Instance.UnlockLobby();
				isInGame = false;
			}
		}

		public void SpawnPlayer(ulong clientId, Vector3 position)
		{
			if (!IsHost) return;

			GameObject playerOb = Instantiate(playerPrefab, position, Quaternion.identity);
			playerDict.Add(clientId, playerOb.GetComponent<PlayerController>());
			NetworkObject networkOb = playerOb.GetComponent<NetworkObject>();

			networkOb.SpawnAsPlayerObject(clientId, true);
		}
		public void DespawnPlayer(ulong clientId)
		{
			if (!IsHost) return;
			if (!playerDict.ContainsKey(clientId)) return;

			NetworkObject networkOb = playerDict[clientId].GetComponent<NetworkObject>();
			networkOb.Despawn();
			Destroy(networkOb);
			playerDict.Remove(clientId);
			Debug.LogWarning("Plyaer Dict remvd : " + clientId);
		}

		public PlayerController GetLocalPlayerController()
		{
			return NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<PlayerController>();
			//return playerDict[GameManagerEx.Instance.MyClientId];
		}
	}
}