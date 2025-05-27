using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    // 运行时注册的所有监听器
    private readonly List<GameEventListener> listeners = new List<GameEventListener>();

    /// <summary>在游戏逻辑中调用这个方法来触发事件。</summary>
    public void Raise()
    {
        // 倒序遍历以防监听器在响应中被移除
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised();
    }

    internal void RegisterListener(GameEventListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    internal void UnregisterListener(GameEventListener listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
}
