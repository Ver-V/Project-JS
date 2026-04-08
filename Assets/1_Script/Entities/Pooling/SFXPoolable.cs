using UnityEngine;
using System.Collections;

namespace ProjectJS.Entities
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXPoolable : Poolable
	{
        private AudioSource audioSource = null;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            audioSource.Play();
            StartCoroutine(ReturnAfterSound());
        }

        private IEnumerator ReturnAfterSound()
        {
            yield return new WaitForSeconds(audioSource.clip.length);
            Return();
        }
    }
}