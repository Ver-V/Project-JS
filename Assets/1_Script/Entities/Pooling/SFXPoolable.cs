using UnityEngine;
using System.Collections;

namespace ProjectJS.Entities
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXPoolable : Poolable
	{
        private AudioSource audioSource = null;
        private Coroutine returnCoroutine;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
            {
                Return();
                return;
            }

            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }

            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();
            returnCoroutine = StartCoroutine(ReturnAfterSound());
        }

        private IEnumerator ReturnAfterSound()
        {
            yield return new WaitForSecondsRealtime(audioSource.clip.length / Mathf.Max(0.01f, Mathf.Abs(audioSource.pitch)));
            returnCoroutine = null;
            Return();
        }

        public override void OnDespawn()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }

            audioSource.Stop();
            audioSource.clip = null;
            base.OnDespawn();
        }
    }
}
