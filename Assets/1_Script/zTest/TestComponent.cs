using UnityEngine;
using ProjectJS.Manager;
using ProjectJS.ScriptableObjects;

namespace ProjectJS.zTest
{
    public class TestComponent : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Managers.Pool.Get(PoolingType.Projectile_Bullet);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Managers.Pool.Get(PoolingType.VFX_Splash);
            }
        }
    }
}