using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.VFX;

namespace IronFlower
{
    public class IronFlowerEffectManager : MonoBehaviour
    {
        [SerializeField] private VisualEffect ironFlowerVFX;
        public List<VisualEffect> ironFlowerVFXList;
        private bool _firstTime = true;
        
        public AudioClip ironFlowerFinishSound;
        public AudioClip firstIronFlowerSound;
        
        
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

        private void OnIronLiquidHit(Vector3 hitPosition, Vector3 hitForce)
        {
            // 设置参数
            ironFlowerVFX.SetVector3("Position", hitPosition - transform.position);
            ironFlowerVFX.SetVector3("MainDirection", hitForce.normalized * 12f);
            
            // 开始播放
            // ironFlowerVFX.Play();
            ironFlowerVFX.SendEvent("OnPlay");

            StartCoroutine(DelayPlayFinishSound());

            if (_firstTime)
            {
                _firstTime = false;

                foreach (var effect in ironFlowerVFXList)
                {
                    effect.SendEvent("OnPlay");
                }

                AudioManager.Instance.PlayAudio(firstIronFlowerSound, Camera.main.transform.position, 0.1f);
            }
            else
            {
                foreach (var effect in ironFlowerVFXList)
                {
                    effect.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator DelayPlayFinishSound()
        {
            yield return new WaitForSeconds(3f);
            AudioManager.Instance.PlayAudio(ironFlowerFinishSound, Camera.main.transform.position, 0.05f);
        }
    }
}