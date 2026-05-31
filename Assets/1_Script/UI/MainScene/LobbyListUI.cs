using NUnit.Framework;
using ProjectJS.Manager;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.MainScene
{

    public class LobbyListUI : MonoBehaviour
    {
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_Text statusText;

        private List<Steamworks.Data.Lobby> lobbyList;
        [SerializeField] private ScrollRect scrollRect;

        public void OnRefreshButtonClicked()
        {
            GameNetworkManager.Instance.FindLobbiesWithCallback(FindLobbyCallback);
        }

        public void FindLobbyCallback(Steamworks.Data.Lobby[] lobbies)
        {
            lobbyList = new List<Steamworks.Data.Lobby>(lobbies);
            RefreshLobbyList();
        }

        public void RefreshLobbyList()
        {
            
        }

    }

}