using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Text progressText;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        // 场景数据字典，用于场景间传递数据
        private static Dictionary<string, object> sceneData = new Dictionary<string, object>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 如果在Inspector中没有指定加载界面，则动态创建一个简单的加载界面
            if (loadingScreen == null)
            {
                CreateBasicLoadingScreen();
            }
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

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// 异步加载场景，带加载界面和进度条
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="onSceneLoaded">场景加载完成的回调</param>
        public void LoadSceneAsync(string sceneName, Action onSceneLoaded = null)
        {
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onSceneLoaded));
        }

        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, Action onSceneLoaded)
        {
            // 显示加载界面
            loadingScreen.SetActive(true);

            // 淡入效果
            yield return StartCoroutine(FadeIn());

            // 开始异步加载场景
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;

            // 更新进度条
            while (!asyncOperation.isDone)
            {
                // 进度值在0-0.9之间，加载完成时为0.9
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                
                if (progressBar != null)
                    progressBar.value = progress;
                
                if (progressText != null)
                    progressText.text = $"{(int)(progress * 100)}%";

                // 当加载进度到达0.9（即100%）时，允许场景激活
                if (asyncOperation.progress >= 0.9f)
                {
                    // 等待一小段时间，让用户看到100%的进度
                    yield return new WaitForSeconds(0.2f);
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            // 淡出效果
            yield return StartCoroutine(FadeOut());

            // 隐藏加载界面
            loadingScreen.SetActive(false);

            // 调用回调
            onSceneLoaded?.Invoke();
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

        private void CreateBasicLoadingScreen()
        {
            // 创建基本的加载UI
            GameObject canvasGO = new GameObject("LoadingCanvas");
            canvasGO.transform.SetParent(transform);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            // 创建黑色背景
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvas.transform, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);
            RectTransform bgRect = bgImage.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

            // 创建进度条
            GameObject sliderGO = new GameObject("ProgressBar");
            sliderGO.transform.SetParent(canvas.transform, false);
            progressBar = sliderGO.AddComponent<Slider>();
            RectTransform sliderRect = progressBar.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.1f, 0.4f);
            sliderRect.anchorMax = new Vector2(0.9f, 0.5f);
            sliderRect.offsetMin = sliderRect.offsetMax = Vector2.zero;

            // 创建进度文本
            GameObject textGO = new GameObject("ProgressText");
            textGO.transform.SetParent(canvas.transform, false);
            progressText = textGO.AddComponent<Text>();
            progressText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            progressText.alignment = TextAnchor.MiddleCenter;
            progressText.color = Color.white;
            progressText.fontSize = 24;
            RectTransform textRect = progressText.rectTransform;
            textRect.anchorMin = new Vector2(0.1f, 0.55f);
            textRect.anchorMax = new Vector2(0.9f, 0.65f);
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;

            // 设置淡入淡出效果的CanvasGroup
            fadeCanvasGroup = canvasGO.AddComponent<CanvasGroup>();

            // 保存加载界面并默认隐藏
            loadingScreen = canvasGO;
            loadingScreen.SetActive(false);
        }
    }
}