using System;
using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace IronFlower
{
    public static class GameEvents
    {
        // 铁水被甩出事件
        public static UnityEvent<GameObject> ironLiquidThrownEvent = new UnityEvent<GameObject>();
        
        // 铁水被击中事件
        public static UnityEvent<Vector3, Vector3> ironLiquidHitEvent = new UnityEvent<Vector3, Vector3>();
        
        public static UnityEvent<HandType, Vector3, Vector3> handFanEvent = new UnityEvent<HandType, Vector3, Vector3>();
        
        public static UnityEvent sceneLoadedEvent = new UnityEvent();
        
        public static UnityEvent playNextGuideClipEvent = new UnityEvent();
        
        // 触发铁水被甩出事件
        public static void OnIronLiquidThrown(GameObject ironLiquid)
        {
            ironLiquidThrownEvent.Invoke(ironLiquid);
        }
        
        // 触发铁水被击中事件
        public static void OnIronLiquidHit(Vector3 hitPosition, Vector3 hitForce)
        {
            ironLiquidHitEvent.Invoke(hitPosition, hitForce);
        }

        public static void OnHandFan(HandType handType, Vector3 previousHandPosition, Vector3 movement)
        {
            handFanEvent.Invoke(handType, previousHandPosition, movement);
        }
        
        public static void OnSceneLoaded()
        {
            sceneLoadedEvent.Invoke();
        }
        
        public static void OnPlayNextGuideClip()
        {
            playNextGuideClipEvent.Invoke();
        }
    }
}