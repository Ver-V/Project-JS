using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.zTest
{
    public class TestAutoHost : MonoBehaviour
    {
        void Start()
        {
            // 네트워크 매니저가 켜져 있고 서버나 클라이언트가 아니라면 바로 호스트로 시작합니다.
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            {
                Debug.Log("[TestAutoHost] 테스트씬용 호스트 자동 시작!");
                NetworkManager.Singleton.StartHost();
            }
        }
    }
}