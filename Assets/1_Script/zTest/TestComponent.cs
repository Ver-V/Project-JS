using ProjectJS.Controller;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.zTest
{
	public class TestComponent : MonoBehaviour
    {
        [SerializeField] private GameObject bossPrefab;
        int id = 0;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                var obj = Instantiate(bossPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                obj.GetComponent<NetworkObject>().Spawn();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                var obj = FindObjectOfType<BossController>();
                obj.RequestTakeDamageServerRpc(10);
			}
        }
    }
}