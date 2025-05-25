using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class GuideTextAudioSync : MonoBehaviour
{
    [Header("TextMeshPro 组件")]
    public TMP_Text guideText;           // 指向 GuideText
    [Header("语音设置")]
    public AudioClip guideClip;         // 语音文件

    private AudioSource audioSource;
    private string fullText;            // 完整文本
    private int totalChars;             // 字符总数

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (guideText == null || guideClip == null)
        {
            Debug.LogError("请在 Inspector 里绑定 guideText 和 guideClip");
            enabled = false;
            return;
        }

        // 读取完整文本并初始化
        fullText = guideText.text;
        totalChars = fullText.Length;
        guideText.maxVisibleCharacters = 0;

        // 开始同步流程
        StartCoroutine(PlayAndReveal());
    }

    IEnumerator PlayAndReveal()
    {
        // 播放语音
        audioSource.clip = guideClip;
        audioSource.Play();

        // 持续到音频播放完毕
        while (audioSource.isPlaying)
        {
            // 计算应显示的字符数
            float t = audioSource.time / guideClip.length;
            int charsToShow = Mathf.Clamp(
                Mathf.FloorToInt(t * totalChars),
                0, totalChars
            );
            guideText.maxVisibleCharacters = charsToShow;

            yield return null;
        }

        // 保证最后一帧全显
        guideText.maxVisibleCharacters = totalChars;
    }
}
