using ProjectJS.Controller;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.GameScene
{
    public struct GameResultInfo
    {
        public bool isCleared;
        public string bossName;
        public float combatTime;
        public int combatPlayerCount;
        public int alivePlayerCount;
        public float bossHPRatio;
    }

    public class GameResultUI : MonoBehaviour
    {
        private const string _bossClearDescMessage = "파티가 보스 토벌에 성공했습니다.";
        private const string _bossFailDescMessage = "파티가 전멸했습니다.";
        private const string _clientWaitMessage = "호스트의 선택을 기다리는 중...";

        private TestBossFlow testBossFlow;

        [SerializeField] private TMP_Text bossClearMainText;
        [SerializeField] private TMP_Text bossClearSubText;
        [SerializeField] private TMP_Text bossClearDescText;

        [SerializeField] private TMP_Text bossNameLabelText;
        [SerializeField] private TMP_Text bossNameText;
        [SerializeField] private TMP_Text combatTimeText;
        [SerializeField] private TMP_Text alivePlayersCountText;


        [SerializeField] private TMP_Text clientWaitText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button lobbyButton;

        private void Start()
        {
            testBossFlow = FindObjectOfType<TestBossFlow>();
            if (NetworkManager.Singleton.IsHost)
            {
                retryButton.onClick.AddListener(OnClickRestartBoss);
                lobbyButton.onClick.AddListener(OnClickReturnToLobby);
            }
        }

        public void ShowGameResult(GameResultInfo gameResultInfo, bool isHost)
        {
            if (gameResultInfo.isCleared)
            {
                bossClearMainText.text = "Boss Clear";
                bossClearSubText.text = "처치 성공";
                bossClearDescText.text = _bossClearDescMessage;

                bossNameLabelText.text = "처치한 보스";
            }
            else
            {
                bossClearMainText.text = "Boss Failed";
                bossClearSubText.text = "처치 실패";
                bossClearDescText.text = _bossFailDescMessage;

                bossNameLabelText.text = "보스 이름";
            }

            int minute = (int)(gameResultInfo.combatTime / 60);
            int second = (int)(gameResultInfo.combatTime % 60);

            bossNameText.text = gameResultInfo.bossName;
            combatTimeText.text = $"{minute} : {second}";
            alivePlayersCountText.text = $"{gameResultInfo.alivePlayerCount} / {gameResultInfo.combatPlayerCount}";

            retryButton.gameObject.SetActive(isHost);
            lobbyButton.gameObject.SetActive(isHost);
            
            clientWaitText.gameObject.SetActive(!isHost);
            
            

            if (!isHost)
            {
                clientWaitText.text = _clientWaitMessage;
            }
        }

        private void OnClickRestartBoss()
        {
            if (NetworkManager.Singleton.IsHost) 
            { 
                testBossFlow.RestartBoss(); 
            }
            this.gameObject.SetActive(false);
        }
        private void OnClickReturnToLobby()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GameSceneUI.Instance.ReturnToLobby();
            }
            this.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            lobbyButton.onClick.RemoveAllListeners();
            retryButton.onClick.RemoveAllListeners();
        }

    }

}

