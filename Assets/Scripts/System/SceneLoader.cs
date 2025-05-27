using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IronFlower
{
    public class SceneLoader : MonoBehaviour
    {
        #region 单例实现
        private static SceneLoader _instance;
        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneLoader");
                    _instance = go.AddComponent<SceneLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion

        [Header("过渡界面设置")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        [Header("场景设置")]
        [SerializeField] private string persistentSceneName = "Persistent"; // 常驻场景名称
        // [SerializeField] private string initialSceneName = "Scene01"; // 初始场景名称
        
        // 场景数据字典，用于场景间传递数据
        private static Dictionary<string, object> sceneData = new Dictionary<string, object>();
        
        // 当前活动的场景名（除了常驻场景）
        [SerializeField] private string _currentSceneName;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            // LoadSceneKeepPersistent(initialSceneName);
        }

        /// <summary>
        /// 设置场景间传递的数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">数据值</param>
        public void SetSceneData(string key, object value)
        {
            if (sceneData.ContainsKey(key))
                sceneData[key] = value;
            else
                sceneData.Add(key, value);
        }

        /// <summary>
        /// 获取场景间传递的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值（如果键不存在）</param>
        /// <returns>数据值</returns>
        public T GetSceneData<T>(string key, T defaultValue = default)
        {
            if (sceneData.TryGetValue(key, out object value) && value is T result)
                return result;
            return defaultValue;
        }

        /// <summary>
        /// 清除所有场景数据
        /// </summary>
        public void ClearSceneData()
        {
            sceneData.Clear();
        }

        // /// <summary>
        // /// 同步加载场景（单场景模式，会卸载其他所有场景包括常驻场景）
        // /// </summary>
        // /// <param name="sceneName">场景名称</param>
        // public void LoadScene(string sceneName)
        // {
        //     SceneManager.LoadScene(sceneName);
        //     _currentSceneName = sceneName;
        // }
        
        public void LoadSceneKeepPersistent(string sceneName)
        {
            LoadSceneKeepPersistent(sceneName, true);
        }
        
        /// <summary>
        /// 加载场景并保持常驻场景不变
        /// </summary>
        /// <param name="sceneName">要加载的场景名称</param>
        /// <param name="unloadCurrent">是否卸载当前场景（不包括常驻场景）</param>
        public void LoadSceneKeepPersistent(string sceneName, bool unloadCurrent)
        {
            if (progressBar != null)
                progressBar.value = 0f;
            if (progressText != null)
                progressText.text = $"Loading: 0%";
            
            StartCoroutine(LoadSceneKeepPersistentCoroutine(sceneName, unloadCurrent));
        }
        
        private IEnumerator LoadSceneKeepPersistentCoroutine(string sceneName, bool unloadCurrent)
        {
            // 显示加载界面
            if (loadingScreen != null)
                loadingScreen.SetActive(true);

            // 淡入效果
            yield return StartCoroutine(FadeIn());

            // 安全卸载当前场景（如果需要）
            if (unloadCurrent && !string.IsNullOrEmpty(_currentSceneName) && 
                _currentSceneName != persistentSceneName)
            {
                // 检查场景是否有效且已加载
                bool sceneExists = false;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene checkScene = SceneManager.GetSceneAt(i);
                    if (checkScene.name == _currentSceneName && checkScene.isLoaded)
                    {
                        sceneExists = true;
                        break;
                    }
                }

                if (sceneExists)
                {
                    AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentSceneName);
                    // 确保unloadOperation不为null再继续
                    if (unloadOperation != null)
                    {
                        while (!unloadOperation.isDone)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"场景 {_currentSceneName} 不存在或未加载，跳过卸载");
                }
            }

            // 检查常驻场景是否已加载
            bool isPersistentLoaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == persistentSceneName)
                {
                    isPersistentLoaded = true;
                    break;
                }
            }

            // 如果常驻场景未加载，先加载它
            if (!isPersistentLoaded)
            {
                AsyncOperation persistentLoadOperation = SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);
                while (!persistentLoadOperation.isDone)
                {
                    yield return null;
                }
            }
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    Debug.Log($"场景 {sceneName} 已加载，将设为活动场景");
                    Scene loadedScene = SceneManager.GetSceneAt(i);
                    yield return null;
                    SceneManager.SetActiveScene(loadedScene);
                    _currentSceneName = sceneName;
                    yield return StartCoroutine(FadeOut());
                    loadingScreen?.SetActive(false);
                    yield break;
                }
            }

            // 加载新场景（如果不是常驻场景）
            if (sceneName != persistentSceneName)
            {
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                
                // 更新进度条
                while (!asyncOperation.isDone)
                {
                    // 进度值在0-0.9之间
                    float progress = Mathf.Clamp01(asyncOperation.progress);

                    if (progressBar != null)
                        progressBar.value = progress;

                    if (progressText != null)
                        progressText.text = $"Loading: {(int)(progress * 100)}%";

                    yield return null;
                }

                // 等待一帧以确保场景完全加载
                yield return null;

                try
                {
                    // 获取新加载的场景并设置为活动场景
                    Scene newScene = SceneManager.GetSceneByName(sceneName);
                    if (newScene.IsValid() && newScene.isLoaded)
                    {
                        SceneManager.SetActiveScene(newScene);
                        _currentSceneName = sceneName;
                        Debug.Log($"成功设置场景 {sceneName} 为活动场景");
                    }
                    else
                    {
                        Debug.LogError($"无法设置场景 {sceneName} 为活动场景：场景无效或未加载");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"设置活动场景时出错: {e.Message}");
                }
            }

            // 淡出效果
            yield return StartCoroutine(FadeOut());

            // 隐藏加载界面
            loadingScreen?.SetActive(false);
            
            GameEvents.OnSceneLoaded();
        }

        // /// <summary>
        // /// 异步加载场景，带加载界面和进度条
        // /// </summary>
        // /// <param name="sceneName">场景名称</param>
        // /// <param name="onSceneLoaded">场景加载完成的回调</param>
        // public void LoadSceneAsync(string sceneName, Action onSceneLoaded = null)
        // {
        //     if (progressBar != null)
        //         progressBar.value = 0f;
        //     if (progressText != null)
        //         progressText.text = $"Loading: 0%";
        //     
        //     StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onSceneLoaded));
        // }
        //
        // private IEnumerator LoadSceneAsyncCoroutine(string sceneName, Action onSceneLoaded)
        // {
        //     // 显示加载界面
        //     if (loadingScreen != null)
        //         loadingScreen.SetActive(true);
        //
        //     // 淡入效果
        //     yield return StartCoroutine(FadeIn());
        //     
        //     for (int i = 0; i < SceneManager.sceneCount; i++)
        //     {
        //         if (SceneManager.GetSceneAt(i).name == sceneName)
        //         {
        //             Debug.Log($"场景 {sceneName} 已加载，将设为活动场景");
        //             Scene loadedScene = SceneManager.GetSceneAt(i);
        //             yield return null;
        //             SceneManager.SetActiveScene(loadedScene);
        //             _currentSceneName = sceneName;
        //             yield return StartCoroutine(FadeOut());
        //             loadingScreen?.SetActive(false);
        //             yield break;
        //         }
        //     }
        //
        //     // 开始异步加载场景
        //     AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        //     asyncOperation.allowSceneActivation = false;
        //
        //     // 更新进度条
        //     while (!asyncOperation.isDone)
        //     {
        //         // 进度值在0-0.9之间，加载完成时为0.9
        //         float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
        //
        //         if (progressBar != null)
        //             progressBar.value = progress;
        //
        //         if (progressText != null)
        //             progressText.text = $"加载中：{(int)(progress * 100)}%";
        //
        //         // 当加载进度到达0.9（即100%）时，允许场景激活
        //         if (asyncOperation.progress >= 0.9f)
        //         {
        //             // 等待一小段时间，让用户看到100%的进度
        //             yield return new WaitForSeconds(0.2f);
        //             asyncOperation.allowSceneActivation = true;
        //         }
        //
        //         yield return null;
        //     }
        //     
        //     _currentSceneName = sceneName;
        //
        //     // 淡出效果
        //     yield return StartCoroutine(FadeOut());
        //
        //     // 隐藏加载界面
        //     if (loadingScreen != null)
        //         loadingScreen.SetActive(false);
        //
        //     // 调用回调
        //     onSceneLoaded?.Invoke();
        // }
        
        /// <summary>
        /// 卸载指定场景（但不会卸载常驻场景）
        /// </summary>
        /// <param name="sceneName">要卸载的场景名称</param>
        public AsyncOperation UnloadScene(string sceneName)
        {
            if (sceneName == persistentSceneName)
            {
                Debug.LogWarning("尝试卸载常驻场景，操作已被忽略");
                return null;
            }
            
            if (_currentSceneName == sceneName)
                _currentSceneName = persistentSceneName;
                
            return SceneManager.UnloadSceneAsync(sceneName);
        }
        
        public void EndGame()
        {
            // 退出游戏
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0;
                float elapsedTime = 0;
                while (elapsedTime < fadeDuration)
                {
                    fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                fadeCanvasGroup.alpha = 1;
            }
        }

        private IEnumerator FadeOut()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 1;
                float elapsedTime = 0;
                while (elapsedTime < fadeDuration)
                {
                    fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                fadeCanvasGroup.alpha = 0;
            }
        }
    }
}