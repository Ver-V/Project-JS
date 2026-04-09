using ProjectJS.Manager;
using UnityEngine;
using ProjectJS.ScriptableObjects;

namespace ProjectJS.Entities
{
	public class ProjectilePoolable : Poolable
	{
        [SerializeField] private float lifeTime;

        public override void OnSpawn()
        {
            base.OnSpawn();
            Invoke(nameof(Return), lifeTime);
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

			Managers.Pool.Get(PoolingType.VFX_Explode, transform.position);
			CancelInvoke(nameof(Return));
        }

        void OnCollisionEnter(Collision collision)
        {
            // TODO - 이건 Projectile에서 호출 필요
            Managers.Pool.Get(PoolingType.VFX_Explode, transform.position);
            Return();
        }
    }
}