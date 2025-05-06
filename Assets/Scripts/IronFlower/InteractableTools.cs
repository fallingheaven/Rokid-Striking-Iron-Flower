using UnityEngine;
using Rokid.UXR.Interaction;

namespace IronFlower
{
    public class InteractableTools : MonoBehaviour
    {
        [SerializeField] private GameObject spoonPrefab;  // 勺子预制体
        [SerializeField] private GameObject hammerPrefab; // 锤子预制体
        
        [SerializeField] private Transform leftHandAnchor;  // 左手锚点
        [SerializeField] private Transform rightHandAnchor; // 右手锚点
        
        private GrabInteractable spoonInteractable;
        private GrabInteractable hammerInteractable;
        
        void Start()
        {
            // 实例化勺子和锤子
            GameObject spoon = Instantiate(spoonPrefab, leftHandAnchor.position, Quaternion.identity);
            GameObject hammer = Instantiate(hammerPrefab, rightHandAnchor.position, Quaternion.identity);
            
            // 获取交互组件
            spoonInteractable = spoon.GetComponent<GrabInteractable>();
            hammerInteractable = hammer.GetComponent<GrabInteractable>();
            
            // 确保物体有交互组件
            if (spoonInteractable == null)
                spoonInteractable = spoon.AddComponent<GrabInteractable>();
                
            if (hammerInteractable == null)
                hammerInteractable = hammer.AddComponent<GrabInteractable>();
                
            // 配置交互属性
            ConfigureInteractable(spoonInteractable, true);  // 左手物体
            ConfigureInteractable(hammerInteractable, false); // 右手物体
        }
        
        private void ConfigureInteractable(GrabInteractable interactable, bool isLeftHand)
        {
            // 设置物体只能被特定手抓取
            interactable.useHandObjectAttachmentPoint = true;
            interactable.attachEaseIn = true;
            interactable.snapAttachEaseInTime = 0.15f;
            interactable.changeScaleOnHover = true;
            
            // 添加刚体组件（如果没有）
            Rigidbody rb = interactable.GetComponent<Rigidbody>();
            if (rb == null)
                rb = interactable.gameObject.AddComponent<Rigidbody>();
                
            rb.useGravity = true;
            rb.isKinematic = false;
            
            // 添加碰撞器（如果没有）
            if (interactable.GetComponent<Collider>() == null)
                interactable.gameObject.AddComponent<BoxCollider>();
        }
    }
}