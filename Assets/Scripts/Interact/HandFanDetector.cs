using System;
using UnityEngine;
using Rokid.UXR.Interaction;

namespace IronFlower
{
    public class HandFanDetector : MonoBehaviour
    {
        [SerializeField] private HandType handType;
        [SerializeField] private float minSwipeSpeed = 1f; // 最小扇动速度阈值
        [SerializeField] private float cooldownTime = 1.0f; // 检测冷却时间，防止连续多次触发
        
        private Vector3 previousHandPosition;
        private float lastFanTime = -10f;
        private bool isTracking = false;
        private bool IsPalm => GesEventInput.Instance.GetGestureType(handType) == GestureType.Palm;

        private void OnEnable()
        {
            // 默认使用右手
            GesEventInput.Instance.SetActiveHand(HandType.RightHand);
        }

        private void OnHandTrackingSuccess(HandType hand)
        {
            if (hand == handType)
            {
                isTracking = true;
                // 初始化手部位置
                previousHandPosition = GesEventInput.Instance.GetHandPos(handType);
            }
        }
        
        private void OnHandTrackingFailed(HandType hand)
        {
            if (hand == handType || hand == HandType.None)
            {
                isTracking = false;
            }
        }
        
        private void Update()
        {
            handType = GesEventInput.Instance.GetActiveHand();
            if (handType != HandType.None)
            {
                OnHandTrackingSuccess(handType);
            }
            else
            {
                OnHandTrackingFailed(handType);
            }
            
            Debug.Log($"{isTracking} {handType} {GesEventInput.Instance.GetGestureType(handType)}");
            if (!isTracking || Time.time - lastFanTime < cooldownTime || !IsPalm)
                return;
                
            // 获取当前手部位置
            Vector3 currentHandPosition = GesEventInput.Instance.GetHandPos(handType);
            
            // 计算手移动向量
            Vector3 movement = GesEventInput.Instance.GetHandDeltaPos(handType);
            
            // 只关注y轴方向的移动
            // 计算移动速度和距离
            float speed = movement.y / Time.deltaTime;
            
            // Debug.Log($"位置：{currentHandPosition} 速度：{speed}，方向：{movement.normalized}");
            
            // 检测是否满足扇动条件
            if (speed >= minSwipeSpeed)
            {
                // 触发扇动事件
                GameEvents.OnHandFan(handType, previousHandPosition, movement);
                
                // 更新冷却时间
                lastFanTime = Time.time;
                
                // Debug.Log($"检测到手部扇动！速度：{speed}，方向：{movement.normalized}");
            }
            
            // 更新上一帧手部位置
            previousHandPosition = currentHandPosition;
        }
        
        // 公共方法，允许外部强制重置冷却时间
        public void ResetCooldown()
        {
            lastFanTime = -10f;
        }
    }
}