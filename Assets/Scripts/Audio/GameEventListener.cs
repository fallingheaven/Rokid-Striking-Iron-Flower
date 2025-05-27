using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [Tooltip("拖入你在 Project 创建的 GameEvent 资产")]
    public GameEvent gameEvent;

    [Tooltip("事件触发时要调用的函数（Inspector 可绑定）")]
    public UnityEvent response;

    private void OnEnable()
    {
        if (gameEvent != null)
            gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (gameEvent != null)
            gameEvent.UnregisterListener(this);
    }

    /// <summary>由 GameEvent.Raise() 调用</summary>
    public void OnEventRaised()
    {
        response.Invoke();
    }
}
