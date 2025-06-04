// Assets/Scripts/SequentialAudioPlayer.cs
using UnityEngine;
using TMPro;
using System.Collections;
using System;
using IronFlower;

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

    public GameObject finalMenu;

    private int currentIndex = 0;
    private Coroutine revealCoroutine;
    private Coroutine clipCoroutine;
    private bool isPlayingAudio = false;
    
    private bool isFinished = false;

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
        
    }

    private void OnEnable()
    {
        GameEvents.sceneLoadedEvent.AddListener(PlayClipsUntilNextStage);
        GameEvents.playNextGuideClipEvent.AddListener(PlayClipsUntilNextStage);
    }

    private void OnDisable()
    {
        GameEvents.sceneLoadedEvent.RemoveListener(PlayClipsUntilNextStage);
        GameEvents.playNextGuideClipEvent.RemoveListener(PlayClipsUntilNextStage);
    }
    
    public void SkipCurrentStage()
    {
        // 停止当前播放
        audioSource.Stop();
        if (clipCoroutine != null)
        {
            StopCoroutine(clipCoroutine);
        }
        
        Debug.Log("跳过当前阶段，currentIndex = " + currentIndex);
    
        // 根据当前索引确定下一阶段的起始索引
        if (currentIndex < 2)
        {
            // 从阶段1跳到阶段2
            currentIndex = 2;
        }
        else if (currentIndex == 2)
        {
            // 从阶段2跳到阶段3
            currentIndex = 3;
        }
        else if (currentIndex <= 5)
        {
            // 从阶段3跳到阶段4
            currentIndex = 6;
        }
        else if (currentIndex == 6)
        {
            // 从阶段4跳到阶段5
            currentIndex = 7;
        }
        else if (currentIndex <= 8)
        {
            // 从阶段6跳到阶段7
            currentIndex = 9;
        }
        else if (currentIndex == 9)
        {
            // 阶段7结束，重置
            ResetPlayback();
        }
    
        // 重置播放状态
        isPlayingAudio = false;
        
        // 通知阶段已更改
        OnReachedNextStage();
        
        // 播放新阶段的内容
        clipCoroutine = StartCoroutine(PlayClipsUntilNextStageCoroutine());
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
                    yield return new WaitUntil(() => !audioSource.isPlaying);
                    currentIndex++;
                }
            }
            else if (tmpIndex == 2)
            {
                PlayNextClip();
                yield return new WaitUntil(() => !audioSource.isPlaying);
                currentIndex++;
            }
            else if (tmpIndex == 3)
            {
                for (int i = tmpIndex; i < Math.Min(clipCollection.clips.Count, tmpIndex + 3); i++)
                {
                    PlayNextClip();
                    yield return new WaitUntil(() => !audioSource.isPlaying);
                    currentIndex++;
                }
            }
            else if (tmpIndex == 6)
            {
                PlayNextClip();
                yield return new WaitUntil(() => !audioSource.isPlaying);
                currentIndex++;
            }
            else if (tmpIndex == 7)
            {
                for (int i = tmpIndex; i < Math.Min(clipCollection.clips.Count, tmpIndex + 2); i++)
                {
                    PlayNextClip();
                    yield return new WaitUntil(() => !audioSource.isPlaying);
                    currentIndex++;
                }
            }
            else
            {
                PlayNextClip();
                
                if (tmpIndex == 9)
                {
                    finalMenu.SetActive(true);
                }
                
                yield return new WaitUntil(() => !audioSource.isPlaying);
                currentIndex++;
                isFinished = true;
                ResetPlayback();
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
        if (isFinished) return;
        
        if (isPlayingAudio)
        {
            SkipCurrentStage();    
        }
        else
        {
            clipCoroutine = StartCoroutine(PlayClipsUntilNextStageCoroutine());
        }
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
        // // 播放结束，确保文字全显
        // guideText.maxVisibleCharacters = totalChars;

        guideText.text = "";
    }

    public void ResetPlayback()
    {
        isFinished = true;
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
