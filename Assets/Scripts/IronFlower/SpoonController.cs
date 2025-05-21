using UnityEngine;
using Rokid.UXR.Interaction;

namespace IronFlower
{
    public class SpoonController : MonoBehaviour
    {
        [SerializeField] private GameObject ironLiquidPrefab; // 铁水预制体
        [SerializeField] private Transform spoonTip; // 勺子尖端位置
        [SerializeField] private float throwVelocityThreshold = 5.0f; // 甩动速度阈值
        [SerializeField] private float throwForce = 1.5f; // 甩动时的力

        public float dropletSize = 0.02f;

        private GrabInteractable grabInteractable;
        private Throwable throwable;
        private GameObject currentIronLiquid; // 添加这个变量来跟踪当前舀起的铁水
        private bool hasIronLiquid = false;

        private Rigidbody rb;
        private VelocityEstimator velocityEstimator; // 添加速度估算器
        
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

        void Start()
        {
            grabInteractable = GetComponent<GrabInteractable>();
            throwable = GetComponent<Throwable>();
            rb = GetComponent<Rigidbody>();
            velocityEstimator = GetComponent<VelocityEstimator>(); // 获取速度估算器组件

            // 监听抓取事件
            if (throwable != null)
            {
                throwable.OnPickUp.AddListener(OnPickUp);
                throwable.OnHeldUpdate.AddListener(OnHeldUpdate);
            }
        }

        private void OnPickUp()
        {
            // 当勺子被抓起时的逻辑
            Debug.Log("勺子被抓起");

            // 开始估算速度
            if (velocityEstimator != null)
            {
                velocityEstimator.BeginEstimatingVelocity();
            }
        }
        
        private void OnHeldUpdate()
        {
            // Debug.Log(123);
            // // 当勺子被持有时持续更新
            // FollowHandOrientation();

            // 检测甩动动作
            // CheckThrowMotion();
        }

        private void FollowHandOrientation()
        {
            if (grabInteractable.grabbedToHand != null)
            {
                // 使用Rokid SDK提供的抓取点数据
                GrabbedObject? grabbedInfo = grabInteractable.grabbedToHand.currentGrabbedObjectInfo;

                if (grabbedInfo.HasValue)
                {
                    // 获取手部抓取点位置和旋转
                    Transform attachPoint = grabbedInfo.Value.handAttachmentPointTransform;

                    // 应用物理更新方式
                    rb.MovePosition(attachPoint.position);
                    rb.MoveRotation(attachPoint.rotation * Quaternion.Euler(30, 0, 0));
                }
            }
        }

        private void CheckThrowMotion()
        {
            // 检测甩动动作的逻辑
            if (grabInteractable.grabbedToHand != null && hasIronLiquid)
            {
                // 使用速度估算器获取速度
                Vector3 estimatedVelocity = velocityEstimator != null
                    ? velocityEstimator.GetVelocityEstimate()
                    : rb.velocity;
                
                // 检测快速向上甩动
                if (estimatedVelocity.magnitude > throwVelocityThreshold)
                {
                    ThrowIronLiquid();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 检测是否与铁水容器碰撞
            if (other.CompareTag("IronLiquidContainer") && !hasIronLiquid)
            {
                ScoopIronLiquid();
            }
        }

        public void ScoopIronLiquid()
        {
            // 舀起铁水的逻辑
            if (!hasIronLiquid)
            {
                hasIronLiquid = true;

                // 创建铁水视觉效果
                if (currentIronLiquid == null && ironLiquidPrefab != null)
                {
                    currentIronLiquid = Instantiate(ironLiquidPrefab, spoonTip.transform);
                    currentIronLiquid.transform.localPosition = Vector3.zero;
                    currentIronLiquid.GetComponent<Rigidbody>().isKinematic = true; // 暂时固定在勺子上
                    currentIronLiquid.transform.localScale *= dropletSize;

                    // 确保铁水对象有IronLiquidMaterial组件
                    if (currentIronLiquid.GetComponent<IronLiquidMaterial>() == null)
                    {
                        currentIronLiquid.AddComponent<IronLiquidMaterial>();
                    }

                    // 设置标签，方便后续碰撞检测
                    currentIronLiquid.tag = "IronLiquid";
                }

                Debug.Log("舀起铁水");
            }
        }

        private void ThrowIronLiquid()
        {
            if (hasIronLiquid && currentIronLiquid != null)
            {
                // 铁水被甩出的逻辑
                hasIronLiquid = false;

                // 分离铁水并添加物理效果
                currentIronLiquid.transform.parent = null;

                Rigidbody ironRb = currentIronLiquid.GetComponent<Rigidbody>();
                if (ironRb == null)
                    ironRb = currentIronLiquid.AddComponent<Rigidbody>();

                // 使用速度估算器获取速度
                Vector3 estimatedVelocity = velocityEstimator != null
                    ? velocityEstimator.GetVelocityEstimate()
                    : rb.velocity;

                // 应用甩出的力
                ironRb.isKinematic = false;
                // ironRb.velocity = estimatedVelocity * 1.5f;
                ironRb.AddForce(estimatedVelocity.normalized * throwForce, ForceMode.Impulse);

                // 发送铁水被甩出的事件
                GameEvents.OnIronLiquidThrown(currentIronLiquid);

                currentIronLiquid = null;

                Debug.Log("铁水被甩出");
            }
        }
        
        private void OnIronLiquidHit(Vector3 hitPosition, Vector3 hitForce)
        {
            hasIronLiquid = false;
            currentIronLiquid = null;
        }
    }
}