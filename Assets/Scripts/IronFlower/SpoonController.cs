using Audio;
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
        
        public AudioClip pickedSound;
        public AudioClip scoopUpSound;

        private GrabInteractable grabInteractable;
        private Throwable throwable;
        private GameObject currentIronLiquid; // 添加这个变量来跟踪当前舀起的铁水
        private bool hasIronLiquid = false;

        private Rigidbody rb;
        
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
        }
        
        private void OnHeldUpdate()
        {
            // Debug.Log(123);
            // // 当勺子被持有时持续更新
            // FollowHandOrientation();

            // 检测甩动动作
            // CheckThrowMotion();
        }

        private void OnTriggerEnter(Collider other)
        {
            // 检测是否与铁水容器碰撞
            if (other.CompareTag("IronLiquidContainer") && !hasIronLiquid)
            {
                ScoopIronLiquid();
                
                // 播放舀起铁水的音效
                if (pickedSound != null)
                {
                    AudioManager.Instance.PlayAudio(scoopUpSound, transform.position, 0.5f);
                }
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
        
        private void OnIronLiquidHit(Vector3 hitPosition, Vector3 hitForce)
        {
            hasIronLiquid = false;
            currentIronLiquid = null;
        }
        
        public void PlayPickedSound()
        {
            if (pickedSound != null)
            {
                AudioManager.Instance.PlayAudio(pickedSound, transform.position, 0.5f);
            }
        }
    }
}