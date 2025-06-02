using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Rokid.UXR.Interaction;

public class CustomGrabManager : MonoBehaviour
{
    private Hand lastHoldingHand;
    private bool wasGrabbed = false;
    private CustomDraggable draggable;
    
    // 控制参数
    public bool keepGrabbed = true; // 是否保持抓取状态
    
    private void Awake()
    {
        draggable = GetComponent<CustomDraggable>();
    }
    
    private void OnEnable()
    {
        // 订阅释放和悬停事件
        Hand.OnGrabbedToHand += OnObjectGrabbed;
        Hand.OnReleasedFromHand += OnObjectReleased;
        // Hand.OnHandHoverBegin += OnHandHoverBegin;

        GesEventInput.OnTrackedSuccess += OnTrackSuccess;
    }
    
    private void OnDisable()
    {
        // 取消订阅事件
        Hand.OnGrabbedToHand -= OnObjectGrabbed;
        Hand.OnReleasedFromHand -= OnObjectReleased;
        // Hand.OnHandHoverBegin -= OnHandHoverBegin;
        
        GesEventInput.OnTrackedSuccess -= OnTrackSuccess;
    }

    private void Update()
    {
        if (lastHoldingHand != null)
            Debug.Log($"之前的抓取手 {lastHoldingHand.handType} {gameObject.name}");
    }

    // 当物体被抓取时记录抓取它的手
    private void OnObjectGrabbed(HandType handType, GameObject grabbedObject)
    {
        if (grabbedObject != this.gameObject) return;
        
        wasGrabbed = true;
        
        // 找到对应的手对象
        Hand hand = null;
        if (handType == HandType.LeftHand)
        {
            hand = GameObject.FindObjectsOfType<Hand>().FirstOrDefault(h => h.handType == HandType.LeftHand);
        }
        else if (handType == HandType.RightHand)
        {
            hand = GameObject.FindObjectsOfType<Hand>().FirstOrDefault(h => h.handType == HandType.RightHand);
        }
        
        if (hand != null)
        {
            lastHoldingHand = hand;
        }
    }
    
    // 当物体被释放时，如果设置了保持抓取，则暂存位置但不真正释放
    private void OnObjectReleased(HandType handType)
    {
        if (!wasGrabbed || !keepGrabbed) return;

        wasGrabbed = false;
        
        Debug.Log($"{gameObject.name} 被释放 {transform.position}");

        // 这里可以添加额外的代码来处理释放时的行为
        // 例如，你可能想让物体停留在当前位置，或者跟随手部移动但不完全依附
    }
    
    public void OnHandHoverBegin(Hand hand)
    {
        if (hand == null) return;
    
        // 仅在我们需要的情况下尝试抓取
        if (wasGrabbed) return;
    
        Debug.Log($"OnHandHoverBegin 被 SendMessage 调用: {hand.handType} {gameObject.name}");
        
        transform.position = hand.transform.position;
        transform.position += draggable.grabbedOffset.localPosition;

        hand.GrabObject(gameObject, GrabTypes.Pinch,
            GrabFlags.DetachOthers | GrabFlags.ParentToHand | GrabFlags.TurnOffGravity | GrabFlags.TurnOnKinematic);
        
        wasGrabbed = true;
        lastHoldingHand = hand;
    }

    private void OnTrackSuccess(HandType handType)
    {
        if (lastHoldingHand == null || wasGrabbed) return;
        
        if (handType == lastHoldingHand.handType && keepGrabbed)
        {
            Debug.Log($"手重新追踪成功 {lastHoldingHand.handType} {handType} {gameObject.name} {transform.position}");
            
            // 延迟一帧执行抓取，确保手的位置已经稳定
            StartCoroutine(DelayedRegrab());
        }
    }
    
    private IEnumerator DelayedRegrab()
    {
        // 等待一帧，让手的位置更新
        yield return null;

        transform.position = lastHoldingHand.transform.position;
        transform.position += draggable.grabbedOffset.localPosition;
    
        // 进行抓取
        lastHoldingHand.GrabObject(gameObject, GrabTypes.Pinch,
            GrabFlags.DetachOthers | GrabFlags.ParentToHand | GrabFlags.TurnOffGravity | GrabFlags.TurnOnKinematic);
        
    
        wasGrabbed = true;
    
        // 记录此时的位置，方便调试
        Debug.Log($"重新抓取物体 {gameObject.name} 位置: {transform.position} 局部位置: {transform.localPosition}");
    }
    
}