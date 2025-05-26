using System;
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Iron"))
            {
                Debug.Log("Iron block entered the furnace.");
                _ironFilled = true;
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
            
            // 可视化反馈（可选）
            // TODO: 添加粒子效果或变色效果表示加速
        }
    }
}