// Assets/Scripts/SequentialAudioPlayer.cs
using UnityEngine;
using TMPro;
using System.Collections;
using System;

[RequireComponent(typeof(AudioSource))]
public class SequentialAudioPlayer : MonoBehaviour
{
    [Header("数据源：音频 & 文本（数量需一一对应）")]
    public AudioClipCollection clipCollection;

    [Header("播放组件")]
    public AudioSource audioSource;

    [Header("文本同步组件")]
    public TMP_Text guideText;

    [Header("打字机速度倍数")]
    [Tooltip("1 = 与音频同步；>1 = 在音频结束前完成；<1 = 更慢")]
    public float revealSpeedMultiplier = 5.0f;

    private int currentIndex = 0;
    private Coroutine revealCoroutine;
    private bool isPlayingAudio = false;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        PlayClipsUntilNextStage();
    }

    private void PlayNextClip()
    {
        if (clipCollection == null ||
            clipCollection.clips == null ||
            clipCollection.texts == null ||
            clipCollection.clips.Count != clipCollection.texts.Count ||
            clipCollection.clips.Count == 0)
        {
            Debug.LogWarning("请检查 clipCollection 是否赋值，且 clips 与 texts 数量一致且不为空！");
            return;
        }

        if (currentIndex >= clipCollection.clips.Count)
        {
            Debug.Log("所有条目已播完，已重置。");
            ResetPlayback();
            return;
        }

        AudioClip clip = clipCollection.clips[currentIndex];
        string text = clipCollection.texts[currentIndex];

        // 准备文本
        guideText.text = text;
        guideText.maxVisibleCharacters = 0;

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();

            if (revealCoroutine != null)
                StopCoroutine(revealCoroutine);
            revealCoroutine = StartCoroutine(RevealTextCoroutine(clip.length, text.Length));
        }
        else
        {
            Debug.LogWarning($"第 {currentIndex} 条 AudioClip 为空，跳过文本同步。");
        }

        currentIndex++;
    }

    private IEnumerator PlayClipsUntilNextStageCoroutine()
    {
        int tmpIndex = currentIndex;
        if (!isPlayingAudio)
        {
            isPlayingAudio = true;
            if (tmpIndex < 2)
            {
                for (int i = tmpIndex; i < Math.Min(clipCollection.clips.Count, tmpIndex + 2); i++)
                {
                    PlayNextClip();
                    yield return new WaitForSeconds(audioSource.clip.length);
                }
            }
            else if (tmpIndex == 2)
            {
                PlayNextClip();
            }
            else if (tmpIndex < 5)
            {
                for (int i = tmpIndex; i < Math.Min(clipCollection.clips.Count, tmpIndex + 2); i++)
                {
                    PlayNextClip();
                    yield return new WaitForSeconds(audioSource.clip.length);
                }
            }
            else if (tmpIndex == 5)
            {
                PlayNextClip();
            }
            else if (tmpIndex < 8)
            {
                for (int i = tmpIndex; i < Math.Min(clipCollection.clips.Count, tmpIndex + 2); i++)
                {
                    PlayNextClip();
                    yield return new WaitForSeconds(audioSource.clip.length);
                }
            }
            else if (tmpIndex == 8)
            {
                PlayNextClip();
            }
            isPlayingAudio = false;
        }

        // 进入下一阶段
        OnReachedNextStage();
    }

    private void OnReachedNextStage()
    {
        Debug.Log("当前阶段引导已播完，进入下一阶段，currentIndex = " + currentIndex);
    }

    public void PlayClipsUntilNextStage()
    {
        StartCoroutine(PlayClipsUntilNextStageCoroutine());
    }

    private IEnumerator RevealTextCoroutine(float clipLength, int totalChars)
    {
        while (audioSource.isPlaying)
        {
            // 归一化播放进度，再乘以速度倍数
            float normalized = (audioSource.time / clipLength) * revealSpeedMultiplier;
            // 限制在 [0,1]
            normalized = Mathf.Clamp01(normalized);

            int charsToShow = Mathf.FloorToInt(normalized * totalChars);
            guideText.maxVisibleCharacters = charsToShow;

            yield return null;
        }
        // 播放结束，确保文字全显
        guideText.maxVisibleCharacters = totalChars;
    }

    public void ResetPlayback()
    {
        currentIndex = 0;
        audioSource.Stop();
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }
        guideText.maxVisibleCharacters = 0;
    }
}
