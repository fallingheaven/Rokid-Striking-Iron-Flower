using System.Collections.Generic;
using UnityEngine;

namespace IronFlower
{
    public class PhysicalDropletSystem : MonoBehaviour
    {
        [SerializeField] private GameObject dropletPrefab;
        [SerializeField] private int dropletCount = 50;
        [SerializeField] private float dropletRadius = 0.02f;
        [SerializeField] private float viscosityFactor = 0.8f;
        [SerializeField] private Material ironLiquidMaterial;
        
        private List<GameObject> droplets = new List<GameObject>();
        private List<Rigidbody> dropletRigidbodies = new List<Rigidbody>();
        private bool isContained = true;
        
        void Start()
        {
            CreateDroplets();
        }
        
        private void CreateDroplets()
        {
            for (int i = 0; i < dropletCount; i++)
            {
                // 在容器范围内随机生成位置
                Vector3 randomPos = transform.position + Random.insideUnitSphere * 0.1f;
                
                GameObject droplet = Instantiate(dropletPrefab, randomPos, Quaternion.identity, transform);
                droplet.transform.localScale = Vector3.one * dropletRadius;
                
                // 设置材质
                Renderer rendererComponent = droplet.GetComponent<Renderer>();
                if (rendererComponent != null && ironLiquidMaterial != null)
                    rendererComponent.material = ironLiquidMaterial;
                
                // 添加物理属性
                Rigidbody rb = droplet.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = droplet.AddComponent<Rigidbody>();
                
                rb.mass = 0.1f;
                rb.drag = viscosityFactor;
                rb.angularDrag = viscosityFactor;
                rb.useGravity = true;
                // rb.isKinematic = isContained; // 在容器中时是静态的
                
                // 添加碰撞器
                SphereCollider colliderComponent = droplet.GetComponent<SphereCollider>();
                if (colliderComponent == null)
                    colliderComponent = droplet.AddComponent<SphereCollider>();
                
                // colliderComponent.radius = dropletRadius;
                
                droplets.Add(droplet);
                dropletRigidbodies.Add(rb);
            }
        }
        
        // 当勺子舀起铁水时调用
        public void ScoopDroplets(Transform container, int amount)
        {
            int count = Mathf.Min(amount, droplets.Count);
            
            for (int i = 0; i < count; i++)
            {
                if (i < droplets.Count)
                {
                    droplets[i].transform.SetParent(container);
                    dropletRigidbodies[i].isKinematic = true; // 暂时固定在勺子上
                }
            }
        }
        
        // 当甩动时调用
        public void ThrowDroplets(Vector3 velocity)
        {
            foreach (var droplet in droplets)
            {
                if (droplet.transform.parent != transform) // 如果不在原容器中
                {
                    droplet.transform.SetParent(null);
                    Rigidbody rb = droplet.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.velocity = velocity + Random.insideUnitSphere * 0.5f; // 添加一些随机性
                    }
                }
            }
        }
    }
}