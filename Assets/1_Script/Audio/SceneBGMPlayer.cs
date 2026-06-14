using UnityEngine;

namespace ProjectJS.Manager
{
    public class SceneBGMPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip bgmClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            audioSource.clip = bgmClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = volume;

            // BGM은 위치 영향 받지 않게 2D 사운드로 설정
            audioSource.spatialBlend = 0f;
        }

        private void Start()
        {
            if (bgmClip == null)
            {
                Debug.LogWarning($"{gameObject.name}에 BGM Clip이 없습니다.");
                return;
            }

            audioSource.Play();
            Debug.Log($"Play BGM : {bgmClip}");
        }

        private void OnDestroy()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log($"Stop BGM : {bgmClip}");
            }
        }
    }
}