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
    }
}