using System;
using Audio;
using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace IronFlower
{
    public class FurnaceInteract : MonoBehaviour
    {
        public Slider progressSlider;
        public float progressSpeed = 0.05f; // 基础进度条速度
        public float speedBoostDuration = 1.0f; // 速度提升持续时间
        public float speedBoostMultiplier = 4.0f; // 速度提升倍率

        [SerializeField]
        private bool _ironFilled = false;

        private bool _finished = false;
        private float _currentBoostTime = 0f; // 当前速度提升剩余时间
        private float _currentSpeedMultiplier = 1.0f; // 当前速度倍率

        public AudioClip ironPickedSound;
        public AudioClip ironDroppedSound;
        public AudioClip furnaceSound;
        public AudioClip handFanSound;
        
        private AudioSource furnaceAudioSource;

        private void OnEnable()
        {
            GameEvents.handFanEvent.AddListener(OnHandFan);
            
            // 播放熔炉背景音效
            if (furnaceSound != null)
            {
                furnaceAudioSource = AudioManager.Instance.PlayAudio(furnaceSound, transform.position, 0.7f);
            }
            else
            {
                Debug.LogWarning("Furnace sound clip is not assigned.");
            }
        }

        private void OnDisable()
        {
            GameEvents.handFanEvent.RemoveListener(OnHandFan);
            
            furnaceAudioSource?.Stop();
        }

        private void Update()
        {
            if (_ironFilled)
            {
                // 更新速度提升计时
                if (_currentBoostTime > 0)
                {
                    _currentBoostTime -= Time.deltaTime;
                    _currentSpeedMultiplier = speedBoostMultiplier;
                    
                    // 可选：在提升快结束时平滑过渡
                    if (_currentBoostTime < 0.5f)
                    {
                        _currentSpeedMultiplier = Mathf.Lerp(1.0f, speedBoostMultiplier, _currentBoostTime * 2);
                    }
                }
                else
                {
                    _currentSpeedMultiplier = 1.0f;
                }

                // 应用当前速度乘数
                float currentSpeed = progressSpeed * _currentSpeedMultiplier;
                progressSlider.value += currentSpeed * Time.deltaTime;

                if (!_finished && progressSlider.value >= 1)
                {
                    SceneLoader.Instance.LoadSceneKeepPersistent("Scene02");
                    _finished = true;
                }
            }
        }
        
        private bool _nextStageTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Iron"))
            {
                Debug.Log("Iron block entered the furnace.");
                _ironFilled = true;
                
                AudioManager.Instance.PlayAudio(ironDroppedSound, other.transform.position, 0.15f);

                if (_nextStageTriggered) return;
                GameEvents.OnPlayNextGuideClip();
                _nextStageTriggered = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Iron"))
            {
                Debug.Log("Iron block exited the furnace.");
                _ironFilled = false;
            }
        }

        private void OnHandFan(HandType handType, Vector3 previousHandPosition, Vector3 movement)
        {
            Debug.Log($"Hand Fan Detected: {handType}, Movement: {movement}");

            if (!_ironFilled) return;

            // 根据扇动幅度增加速度提升时间
            float movementMagnitude = movement.magnitude;
            float boostIncrease = Mathf.Clamp(movementMagnitude * 0.5f, 0.5f, 2.0f);
            
            // 增加或刷新速度提升时间
            _currentBoostTime = Mathf.Max(_currentBoostTime, speedBoostDuration * boostIncrease);

            if (Camera.main != null)
                AudioManager.Instance.PlayAudio(handFanSound, Camera.main.transform.position, 0.5f);
        }
        
        public void PlayIronPickedSound(Transform ironTransform)
        {
            if (ironPickedSound != null)
            {
                AudioManager.Instance.PlayAudio(ironPickedSound, ironTransform.position, 0.5f);
            }
            else
            {
                Debug.LogWarning("Iron picked sound clip is not assigned.");
            }
        }
    }
}