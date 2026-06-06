using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.GameScene
{
    public class GameResultUI : MonoBehaviour
    {
        private const string _bossClearDescMessage = "파티가 보스 토벌에 성공했습니다.";
        private const string _bossFailDescMessage = "파티가 전멸했습니다.";
        private const string _clientWaitMessage = "호스트의 선택을 기다리는 중...";

        [SerializeField] private TMP_Text bossClearMainText;
        [SerializeField] private TMP_Text bossClearSubText;
        [SerializeField] private TMP_Text bossClearDescText;

        [SerializeField] private TMP_Text clientWaitText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button lobbyButton;

        private void Start()
        {
            //retryButton.onClick.AddListener();
            lobbyButton.onClick.AddListener(GameSceneUI.Instance.ReturnToLobby);
        }

        public void ShowGameResult(bool isClear, bool isHost)
        {
            if (isClear)
            {
                bossClearMainText.text = "Boss Clear";
                bossClearSubText.text = "처치 성공";
                bossClearDescText.text = _bossClearDescMessage;
            }
            else
            {
                bossClearMainText.text = "Boss Failed";
                bossClearSubText.text = "처치 실패";
                bossClearDescText.text = _bossFailDescMessage;
            }

            clientWaitText.gameObject.SetActive(!isHost);
            retryButton.gameObject.SetActive(isHost);
            lobbyButton.gameObject.SetActive(isHost);

            if (!isHost)
            {
                clientWaitText.text = _clientWaitMessage;
            }
        }

        private void OnDestroy()
        {
            lobbyButton.onClick.RemoveAllListeners();
            retryButton.onClick.RemoveAllListeners();
        }
    }

}

