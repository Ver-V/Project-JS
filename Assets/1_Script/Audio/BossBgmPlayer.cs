using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace ProjectJS.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class BossBgmPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip bossBgm;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField, Min(0f)] private float fadeInDuration = 1f;
        [SerializeField, Range(0f, 1f)] private float maxVolume = 1f;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
            audioSource.outputAudioMixerGroup = musicMixerGroup;
        }

        private void Start()
        {
            if (bossBgm == null) return;

            audioSource.clip = bossBgm;
            audioSource.volume = fadeInDuration > 0f ? 0f : maxVolume;
            audioSource.Play();

            if (fadeInDuration > 0f)
            {
                StartCoroutine(FadeIn());
            }
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(0f, maxVolume, elapsed / fadeInDuration);
                yield return null;
            }

            audioSource.volume = maxVolume;
        }
    }
}
