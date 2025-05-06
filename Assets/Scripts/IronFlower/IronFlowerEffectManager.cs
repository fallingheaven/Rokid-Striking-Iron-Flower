using UnityEngine;
using UnityEngine.VFX;

namespace IronFlower
{
    public class IronFlowerEffectManager : MonoBehaviour
    {
        [SerializeField] private VisualEffect ironFlowerVFX;
        public static Vector3 directionOffset = new Vector3(-0.6f, -0.6f, -0.6f);
        
        private void OnEnable()
        {
            // 订阅铁水击中事件
            GameEvents.ironLiquidHitEvent.AddListener(OnIronLiquidHit);
        }
        
        private void OnDisable()
        {
            // 取消订阅事件
            GameEvents.ironLiquidHitEvent.RemoveListener(OnIronLiquidHit);
        }
        
        private void OnIronLiquidHit(Vector3 hitPosition, Vector3 hitForce)
        {
            // 向上方向为基础方向
            Vector3 direction = directionOffset + hitForce;
            
            // 设置参数
            ironFlowerVFX.SetVector3("Position", hitPosition - transform.position);
            ironFlowerVFX.SetVector3("MainDirection", direction);
            
            // 开始播放
            // ironFlowerVFX.Play();
            ironFlowerVFX.SendEvent("OnPlay");
        }
    }
}