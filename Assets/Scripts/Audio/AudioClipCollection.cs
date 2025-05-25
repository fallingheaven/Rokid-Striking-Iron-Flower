// Assets/Scripts/AudioClipCollection.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Audio/AudioClip Collection")]
public class AudioClipCollection : ScriptableObject
{
    [Tooltip("按播放顺序排列的音频片段列表")]
    public List<AudioClip> clips = new List<AudioClip>();
    public List<string> texts = new List<string>();
}
