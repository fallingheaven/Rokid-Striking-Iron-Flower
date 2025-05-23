using System;
using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace IronFlower
{
    public class FurnaceInteract : MonoBehaviour
    {
        public Slider progressSlider;
        
        [SerializeField]
        private bool _ironFilled = false;
        
        private bool _finished = false;
        
        private void OnEnable()
        {
            GameEvents.handFanEvent.AddListener(OnHandFan);
        }

        private void OnDisable()
        {
            GameEvents.handFanEvent.RemoveListener(OnHandFan);
        }

        private void Update()
        {
            if (_ironFilled)
            {
                progressSlider.value += 0.2f * Time.deltaTime;
                
                if (!_finished && progressSlider.value >= 1)
                {
                    SceneLoader.Instance.LoadSceneKeepPersistent("Scene02");
                    _finished = true;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Iron"))
            {
                // 处理铁块进入炉子
                Debug.Log("Iron block entered the furnace.");

                _ironFilled = true;

                // 禁止交互
                // Destroy(other.GetComponent<RayInteractable>());
            }
        }

        private void OnHandFan(HandType handType, Vector3 previousHandPosition, Vector3 movement)
        {
            // 处理手扇事件
            Debug.Log($"Hand Fan Detected: {handType}, Movement: {movement}");

            if (!_ironFilled) return;
            
            // 最少扇五次
            progressSlider.value += Mathf.Max(2f, movement.magnitude) * 0.1f;

            if (!_finished && progressSlider.value >= 1)
            {
                SceneLoader.Instance.LoadSceneKeepPersistent("Scene02");
                _finished = true;
            }
        }
    }
}