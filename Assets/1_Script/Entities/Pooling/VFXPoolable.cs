using UnityEngine;
using System.Collections;

namespace ProjectJS.Entities
{
    [RequireComponent(typeof(ParticleSystem))]
    public class VFXPoolable : Poolable
    {
        private ParticleSystem ps = null;

        void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            ps.Play();
            StartCoroutine(CheckAlive());
        }

        private IEnumerator CheckAlive()
        {
            yield return new WaitUntil(() => !ps.IsAlive(true));
            Return();
        }
	}
}