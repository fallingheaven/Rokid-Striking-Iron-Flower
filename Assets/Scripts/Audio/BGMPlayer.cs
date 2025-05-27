using UnityEngine;

namespace Audio
{
    public class BGMPlayer : MonoBehaviour
    {
        public AudioClip bgmClip; // 背景音乐音频剪辑
        public float volume = 0.5f; // 音量

        private AudioSource audioSource;

        private void Start()
        {
            audioSource = AudioManager.Instance.PlayAudio(bgmClip, Camera.main.transform.position, volume, true);
        }

        private void OnDestroy()
        {
            if (audioSource != null)
            {
                audioSource.Stop(); // 停止播放背景音乐
            }
        }
    }
}