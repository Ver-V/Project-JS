using UnityEngine;

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
            CancelInvoke(nameof(Return));
        }

        void OnCollisionEnter(Collision collision)
        {
            Return();
        }
    }
}