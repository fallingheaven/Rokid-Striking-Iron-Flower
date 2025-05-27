using Audio;
using UnityEngine;
using Rokid.UXR.Interaction;

namespace IronFlower
{
    public class HammerController : MonoBehaviour
    {
        [SerializeField] private float hitForceThreshold = 5.0f; // 触发击打效果的力阈值
        [SerializeField] private Transform hammerHead; // 锤子头部位置
        [SerializeField] private float strikeMultiplier = 2f;
        
        public AudioClip pickedSound; // 锤子被抓起时的音效
        public AudioClip hitSound; // 锤子击打铁水时的音效

        private GrabInteractable grabInteractable;
        private Throwable throwable;
        private Rigidbody rb;
        private VelocityEstimator velocityEstimator;

        void Start()
        {
            grabInteractable = GetComponent<GrabInteractable>();
            throwable = GetComponent<Throwable>();
            rb = GetComponent<Rigidbody>();
            velocityEstimator = GetComponent<VelocityEstimator>();

            // 监听 Throwable 的事件
            if (throwable != null)
            {
                throwable.OnPickUp.AddListener(OnPickUp);
                throwable.OnHeldUpdate.AddListener(OnHeldUpdate);
            }
            
            // 如果没有指定锤子头部，则使用当前对象
            if (hammerHead == null)
            {
                hammerHead = transform;
            }
        }

        private void OnPickUp()
        {
            Debug.Log("锤子被抓起");
            if (velocityEstimator != null)
            {
                velocityEstimator.BeginEstimatingVelocity();
            }
        }

        private void OnHeldUpdate()
        {
            // FollowHandOrientation();
        }

        private void FollowHandOrientation()
        {
            if (grabInteractable.grabbedToHand != null)
            {
                GrabbedObject? grabbedInfo = grabInteractable.grabbedToHand.currentGrabbedObjectInfo;
                if (grabbedInfo.HasValue)
                {
                    Transform attachPoint = grabbedInfo.Value.handAttachmentPointTransform;
                    rb.MovePosition(attachPoint.position);
                    rb.MoveRotation(attachPoint.rotation);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("IronLiquid"))
            {
                // 使用锤子自身的速度，而不是相对速度
                Vector3 hammerVelocity = velocityEstimator != null && grabInteractable.grabbedToHand != null
                    ? velocityEstimator.GetVelocityEstimate()
                    : rb.velocity;
                
                float impactForce = hammerVelocity.magnitude;
                
                Debug.Log($"锤子速度: {impactForce}, 阈值: {hitForceThreshold}");

                if (impactForce > hitForceThreshold)
                {
                    IronLiquidMaterial ironLiquidMat = other.GetComponent<IronLiquidMaterial>();
                    if (ironLiquidMat != null)
                    {
                        ironLiquidMat.OnHit();
                    }

                    // 使用碰撞点或锤子头部位置
                    Vector3 hitPosition = hammerHead.position;
                    TriggerIronFlowerEffect(hitPosition);
                    Debug.Log("锤子击中铁水，触发铁花效果！");
                    
                    AudioManager.Instance.PlayAudio(hitSound, hitPosition, 0.5f);
                    
                    Destroy(other.gameObject);
                }
            }
        }

        private void TriggerIronFlowerEffect(Vector3 hitPosition)
        {
            Vector3 hammerVelocity = velocityEstimator != null && grabInteractable.grabbedToHand != null
                ? velocityEstimator.GetVelocityEstimate()
                : rb.velocity;
            
            hammerVelocity = hammerVelocity.normalized * strikeMultiplier;
                
            GameEvents.OnIronLiquidHit(hitPosition, hammerVelocity);
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