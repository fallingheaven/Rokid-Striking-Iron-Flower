using UnityEngine;

public class CollisionRaiser : MonoBehaviour
{
    [Tooltip("同一个事件资产：PlayNextAudioEvent")]
    public GameEvent playNextAudioEvent;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cube"))
        {
            if (playNextAudioEvent != null)
                playNextAudioEvent.Raise();
            else
                Debug.LogWarning("playNextAudioEvent 没有在 Inspector 里赋值！");
        }
    }
}
