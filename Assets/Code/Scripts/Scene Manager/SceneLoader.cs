using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using HUB;

public class SceneLoader : Singleton<SceneLoader>
{
    [Title("UI Settings")]
    [SerializeField, Required] private Image blackScreen;
    [SerializeField, MinValue(0f)] private float fadeDuration = 1f;

    [Title("Loading Settings")]
    [SerializeField, Range(1f, 10f)] private float maxLoadingTime = 5f; // Maximum loading time before showing loading screen
    [SerializeField, Required] private GameObject loadingScreen; // Loading screen GameObject

    public static Action<string> OnSwitchScene;
    public static Action OnSceneLoaded;
    public static Action OnSceneLoadComplete;

    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#else
    Application.targetFrameRate = 20;
#endif
    }

    private void OnEnable()
    {
        OnSwitchScene += LoadScene;
        DataManager.OnDataLoaded += DataLoaded;
        HUBManager.OnDataLoaded += DataLoaded;
    }

    private void OnDisable()
    {
        OnSwitchScene -= LoadScene;
        DataManager.OnDataLoaded -= DataLoaded;
        HUBManager.OnDataLoaded -= DataLoaded;

    }

    private void Start()
    {
        blackScreen.color = new Color(0, 0, 0, 0);
    }

    /// <summary>
    /// Loads a scene with fade-in and fade-out transition.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName));
    }

    /// <summary>
    /// Reloads the current scene with fade-in and fade-out transition.
    /// </summary>
    /// <param name="sceneName">Name of the scene to reload.</param>
    public void ReLoadScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName, false));
    }

    private void DataLoaded()
    {
        StartCoroutine(EndTransition());
    }

    private IEnumerator TransitionToScene(string sceneName, bool isFadeIn = true)
    {
        if (isFadeIn) yield return StartCoroutine(FadeIn());
        else blackScreen.color = new Color(0, 0, 0, 1);

        // Start loading screen if it takes more than a set amount of time
        loadingScreen?.SetActive(true);

        // Asynchronous scene loading
        yield return StartCoroutine(LoadSceneAsync(sceneName));

        yield return StartCoroutine(FadeOut());

        loadingScreen?.SetActive(false);
    }

    private IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        blackScreen.color = new Color(0, 0, 0, 1);
    }

    private IEnumerator FadeOut()
    {
        float timerOut = 0f;
        while (timerOut < fadeDuration)
        {
            timerOut += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1, 0, timerOut / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        blackScreen.color = new Color(0, 0, 0, 0);
    }

    private IEnumerator EndTransition()
    {
        yield return StartCoroutine(FadeOut());

        loadingScreen?.SetActive(false);

        OnSceneLoadComplete?.Invoke();
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return StartCoroutine(FadeIn());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        // Timer to check if loading exceeds the max loading time
        float loadStartTime = Time.unscaledTime;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f && Time.unscaledTime - loadStartTime >= maxLoadingTime)
            {
                // Show loading screen if loading time exceeds the max limit
                loadingScreen?.SetActive(true);
            }

            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        OnSceneLoaded?.Invoke();
    }

    /// <summary>
    /// Loads multiple scenes in additive mode, such as the main scene and UI.
    /// </summary>
    /// <param name="sceneNames">Array of scene names to load.</param>
    public void LoadMultipleScenes(string[] sceneNames)
    {
        StartCoroutine(LoadMultipleScenesAsync(sceneNames));
    }

    private IEnumerator LoadMultipleScenesAsync(string[] sceneNames)
    {
        bool isFirstScene = true;

        yield return StartCoroutine(FadeIn());

        foreach (string sceneName in sceneNames)
        {
            LoadSceneMode loadMode = isFirstScene ? LoadSceneMode.Single : LoadSceneMode.Additive;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadMode);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }

            isFirstScene = false;
        }

        OnSceneLoaded?.Invoke();
    }
}
