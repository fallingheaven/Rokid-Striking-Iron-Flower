using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 尝试在场景中找到AudioManager
                    _instance = FindObjectOfType<AudioManager>();
                    
                    // 如果场景中不存在，则创建一个
                    if (_instance == null)
                    {
                        GameObject audioManagerObject = new GameObject("AudioManager");
                        _instance = audioManagerObject.AddComponent<AudioManager>();
                        DontDestroyOnLoad(audioManagerObject);
                    }
                }
                return _instance;
            }
        }

        public int maxAudioSources = 10; // 最大音频源数量
        private List<AudioSource> audioSources = new List<AudioSource>();
        
        private void Awake()
        {
            // 确保只有一个AudioManager实例
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 初始化音频源
            InitializeAudioSources();
        }
        
        private void InitializeAudioSources()
        {
            for (int i = 0; i < maxAudioSources; i++)
            {
                GameObject audioObject = new GameObject("AudioSource_" + i);
                audioObject.transform.SetParent(transform);
                AudioSource audioSource = audioObject.AddComponent<AudioSource>();
                audioSources.Add(audioSource);
            }
        }

        public AudioSource PlayAudio(AudioClip clip, Vector3 position, float volume = 1.0f, bool loop = false)
        {
            if (clip == null)
            {
                Debug.LogWarning("尝试播放空音频片段");
                return null;
            }
            
            AudioSource availableSource = GetAvailableAudioSource();
            if (availableSource != null)
            {
                availableSource.transform.position = position;
                availableSource.clip = clip;
                availableSource.volume = volume;
                availableSource.loop = loop;
                availableSource.Play();
                return availableSource;
            }
            else
            {
                Debug.LogWarning("没有可用的音频源来播放片段");
                return null;
            }
        }

        private AudioSource GetAvailableAudioSource()
        {
            foreach (var source in audioSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return null;
        }
    }
}