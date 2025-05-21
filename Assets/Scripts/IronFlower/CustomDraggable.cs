using UnityEngine;

namespace Rokid.UXR.Interaction
{
    [RequireComponent(typeof(GrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class CustomDraggable : Throwable
    {
        // 是否允许放下物体
        [Tooltip("设置为true时物体可以放下，设置为false时物体拿起后无法放下")]
        public bool allowRelease = false;
        
        // 重写抓取更新方法，阻止释放
        public new void OnGrabbedUpdate(Hand hand)
        {
            // 如果允许释放，则调用原始行为
            if (allowRelease && hand.IsGrabEnding(this.gameObject))
            {
                hand.ReleaseObject(gameObject, restoreOriginalParent);
            }
            
            OnHeldUpdate?.Invoke();
        }
        
        public new void OnReleasedFromHand(Hand hand)
        {
            if (allowRelease)
            {
                base.OnReleasedFromHand(hand);
            }
        }
    }
}