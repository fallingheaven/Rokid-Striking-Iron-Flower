using UnityEngine;

namespace IronFlower
{
    public class FaceCamera : MonoBehaviour
    {
        [Tooltip("目标摄像机，如果为null则使用主摄像机")]
        public Camera targetCamera;

        [Tooltip("是否只在Y轴旋转（适用于只需要水平朝向的UI）")]
        public bool onlyYRotation = false;

        [Tooltip("是否保持原有的上方向")]
        public bool preserveUpDirection = false;

        [Tooltip("Y轴位置偏移量")]
        public float heightOffset = 0f;

        private void Start()
        {
            // 如果没有指定摄像机，使用主摄像机
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
                return;

            if (onlyYRotation)
            {
                // 只在Y轴上旋转，保持UI垂直
                Vector3 targetPosition = new Vector3(targetCamera.transform.position.x, 
                    transform.position.y + heightOffset, 
                    targetCamera.transform.position.z);
                transform.LookAt(targetPosition);
                
                // 锁定X和Z轴的旋转
                Vector3 eulerAngles = transform.eulerAngles;
                transform.eulerAngles = new Vector3(0, eulerAngles.y, 0);
            }
            else
            {
                // 完全面向摄像机
                transform.LookAt(targetCamera.transform.position + new Vector3(0, heightOffset, 0));
                
                if (preserveUpDirection)
                {
                    // 保持Y轴朝上
                    transform.rotation = Quaternion.LookRotation(
                        targetCamera.transform.position - transform.position, 
                        Vector3.up);
                }
            }
        }
    }
}