using UnityEngine;

namespace IronFlower
{
    public class SceneLoadTest : MonoBehaviour
    {
        public string sceneName;
        
        private void OnEnable()
        {
            SceneLoader.Instance.LoadSceneKeepPersistent(sceneName);
        }
    }
}