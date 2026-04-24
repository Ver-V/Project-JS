using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;

namespace ProjectJS.Manager
{
    public class SteamManager : MonoBehaviour
    {
        public uint appId = 480; // spacewar AppId

        #region Singleton
        public static SteamManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitalizeSteam();
            }
            else
            {
                Destroy(gameObject);
            }

        }
        #endregion

        private void InitalizeSteam()
        {
            try
            {
                //steam client initalize
                SteamClient.Init(appId);
                Debug.Log($"<color=green>Steamworks initalize success! User : {SteamClient.Name}</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>Steamworks initalize fail! : {e.Message}</color>");

            }
        }

        private void Update()
        { // Call RunCallbacks per frame > Events operate successfully
            if (SteamClient.IsValid)
            {
                SteamClient.RunCallbacks();
            }
        }

        private void OnApplicationQuit()
        {
            if (SteamClient.IsValid)
            {
                SteamClient.Shutdown();
            }
        }

    }
}
