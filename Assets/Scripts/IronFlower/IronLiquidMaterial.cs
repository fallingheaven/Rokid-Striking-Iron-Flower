using UnityEngine;

namespace IronFlower
{
    public class IronLiquidMaterial : MonoBehaviour
    {
        [SerializeField] private Material ironLiquidMaterial;
        [SerializeField] private float glowIntensity = 1.5f;
        [SerializeField] private float coolingRate = 0.1f;
        
        private float temperature = 1.0f; // 归一化温度
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        
        void Start()
        {
            if (ironLiquidMaterial == null)
            {
                Renderer rendererComponent = GetComponent<Renderer>();
                if (rendererComponent != null)
                    ironLiquidMaterial = rendererComponent.material;
            }
            
            // 确保材质启用了发光
            if (ironLiquidMaterial != null)
            {
                ironLiquidMaterial.EnableKeyword("_EMISSION");
                UpdateGlow();
            }
        }
        
        void Update()
        {
            // 随时间冷却
            temperature = Mathf.Max(0.0f, temperature - coolingRate * Time.deltaTime);
            UpdateGlow();
        }
        
        private void UpdateGlow()
        {
            if (ironLiquidMaterial != null)
            {
                // 根据温度设置发光颜色
                Color baseColor = Color.Lerp(Color.red, Color.yellow, temperature);
                ironLiquidMaterial.SetColor(EmissionColor, baseColor * glowIntensity * temperature);
            }
        }
        
        // 当被锤击时调用
        public void OnHit()
        {
            // 被击中时温度略微上升
            temperature = Mathf.Min(1.0f, temperature + 0.2f);
        }
    }
}