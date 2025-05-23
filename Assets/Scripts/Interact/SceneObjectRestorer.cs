using UnityEngine;
using UnityEngine.SceneManagement;
using Rokid.UXR.Interaction;

namespace IronFlower
{
    [RequireComponent(typeof(Throwable))]
    public class SceneObjectRestorer : MonoBehaviour
    {
        private string originalSceneName;
        private Throwable throwable;

        private void Awake()
        {
            // 记录物体的初始场景
            Scene currentScene = gameObject.scene;
            originalSceneName = currentScene.name;
            
            // 获取Throwable组件
            throwable = GetComponent<Throwable>();
        }

        private void OnEnable()
        {
            // 订阅Throwable的OnDropDown事件
            if (throwable != null)
            {
                throwable.OnDropDown.AddListener(RestoreToOriginalScene);
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            if (throwable != null)
            {
                throwable.OnDropDown.RemoveListener(RestoreToOriginalScene);
            }
        }

        private void RestoreToOriginalScene()
        {
            // 如果在Persistent场景中并且记录了原始场景名称
            if (gameObject.scene.name == "Persistent" && !string.IsNullOrEmpty(originalSceneName))
            {
                // 确保原始场景正在加载
                Scene originalScene = SceneManager.GetSceneByName(originalSceneName);
                if (originalScene.IsValid() && originalScene.isLoaded)
                {
                    Debug.Log($"将{gameObject.name}恢复到原始场景: {originalSceneName}");
                    
                    // 保存当前位置和旋转
                    Vector3 currentPosition = transform.position;
                    Quaternion currentRotation = transform.rotation;
                    
                    // 将物体移回原始场景
                    SceneManager.MoveGameObjectToScene(gameObject, originalScene);
                    
                    // 恢复位置和旋转
                    transform.position = currentPosition;
                    transform.rotation = currentRotation;
                }
                else
                {
                    Debug.LogWarning($"无法恢复{gameObject.name}到场景{originalSceneName}，该场景未加载");
                }
            }
        }
    }
}