using UnityEngine;
using System.Collections;
using ProjectJS.Manager;

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

    public class DynamicVFXPoolable : MonoBehaviour
    {
        private GameObject sourcePrefab;
        private ParticleSystem[] particleSystems;
        private Animator[] animators;
        private UnityEngine.Animation[] legacyAnimations;
        private Coroutine returnCoroutine;

        public void Configure(GameObject prefab)
        {
            sourcePrefab = prefab;
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            animators = GetComponentsInChildren<Animator>(true);
            legacyAnimations = GetComponentsInChildren<UnityEngine.Animation>(true);
        }

        public void Play()
        {
            gameObject.SetActive(true);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.Clear(true);
                particleSystem.Play(true);
            }

            float animationDuration = RestartAnimations();

            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }

            returnCoroutine = StartCoroutine(ReturnWhenFinished(animationDuration));
        }

        private float RestartAnimations()
        {
            float duration = 0f;

            foreach (Animator animator in animators)
            {
                if (animator.runtimeAnimatorController == null) continue;

                animator.Rebind();
                animator.Update(0f);

                for (int layer = 0; layer < animator.layerCount; layer++)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                    float speed = Mathf.Abs(animator.speed * stateInfo.speed * stateInfo.speedMultiplier);
                    duration = Mathf.Max(duration, stateInfo.length / Mathf.Max(0.01f, speed));
                }
            }

            foreach (UnityEngine.Animation legacyAnimation in legacyAnimations)
            {
                legacyAnimation.Rewind();
                legacyAnimation.Play();

                foreach (UnityEngine.AnimationState state in legacyAnimation)
                {
                    duration = Mathf.Max(
                        duration,
                        state.length / Mathf.Max(0.01f, Mathf.Abs(state.speed)));
                }
            }

            return duration;
        }

        private IEnumerator ReturnWhenFinished(float animationDuration)
        {
            float elapsed = 0f;

            do
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
            while (elapsed < animationDuration || HasAliveParticles());

            returnCoroutine = null;
            Managers.Pool.ReturnVfx(sourcePrefab, gameObject);
        }

        private bool HasAliveParticles()
        {
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem.IsAlive(true))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDisable()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }
        }
    }
}
