using UnityEngine;
using ProjectJS.Manager;
using ProjectJS.ScriptableObjects;
using ProjectJS.Entities;

namespace ProjectJS.zTest
{
    public class TestComponent : MonoBehaviour
    {
        int id = 0;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GameObject projectile = Managers.Pool.Get(PoolingType.Projectile_Bullet);
                projectile.GetComponent<Projectile>().Init(id++, new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Managers.Pool.Get(PoolingType.VFX_Splash);
            }
        }
    }
}