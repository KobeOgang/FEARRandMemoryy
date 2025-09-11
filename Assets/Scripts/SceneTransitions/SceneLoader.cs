using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Slider loadingSlider;

    [Header("Fade Effects")]
    [Tooltip("Black overlay for fade effects")]
    public Image fadeOverlay;
    [Tooltip("Time to fade to black before loading starts")]
    public float fadeInDuration = 1f;
    [Tooltip("Time to fade from black after loading completes")]
    public float fadeOutDuration = 1f;

    [Header("Settings")]
    public float minimumLoadTime = 6f;

    [Header("Main Menu Integration")]
    public GameObject mainMenuPanel;

    private static string nextSpawnPointID;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize fade overlay
            InitializeFadeOverlay();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFadeOverlay()
    {
        if (fadeOverlay == null)
        {
            // Auto-create fade overlay if not assigned
            CreateFadeOverlay();
        }

        // Start with fade overlay invisible and disabled
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(false);
        }
    }

    private void CreateFadeOverlay()
    {
        // Find or create a canvas for the fade overlay
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(this.transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Ensure it's on top
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create the fade overlay image
        GameObject overlayGO = new GameObject("FadeOverlay");
        overlayGO.transform.SetParent(canvas.transform, false);

        fadeOverlay = overlayGO.AddComponent<Image>();
        fadeOverlay.color = Color.black;

        // CRITICAL FIX: Disable raycast target so it doesn't block interactions when transparent
        fadeOverlay.raycastTarget = false;

        // Make it cover the entire screen
        RectTransform rectTransform = fadeOverlay.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        SetFadeAlpha(0f);

        // Start with the overlay GameObject disabled
        overlayGO.SetActive(false);
    }

    // Subscribe to the event when this object is enabled
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Unsubscribe when it's disabled to prevent errors
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName, string spawnPointID)
    {
        // Store the ID so we can use it after the new scene loads
        nextSpawnPointID = spawnPointID;
        StartCoroutine(LoadSceneWithFade(sceneName));
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        // Phase 1: Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Phase 2: Temporarily hide fade overlay to show loading screen
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(false);
        }

        // Phase 3: Show loading screen (now visible) and load the scene
        loadingScreen.SetActive(true);
        yield return StartCoroutine(LoadSceneAsync(sceneName));

        // Phase 4: Re-enable fade overlay at full black for fade out
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            SetFadeAlpha(1f); // Ensure it's fully black before fade out
        }

    }

    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(1f); // Ensure it's fully black
    }

    private IEnumerator FadeFromBlack()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(0f);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay != null)
        {
            // Enable/disable the overlay GameObject based on alpha
            if (alpha > 0f && !fadeOverlay.gameObject.activeSelf)
            {
                fadeOverlay.gameObject.SetActive(true);
            }
            else if (alpha <= 0f && fadeOverlay.gameObject.activeSelf)
            {
                fadeOverlay.gameObject.SetActive(false);
            }

            Color color = fadeOverlay.color;
            color.a = alpha;
            fadeOverlay.color = color;
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        float elapsedTime = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // Loop continues until the REAL load is ready AND our minimum time has passed
        while (elapsedTime < minimumLoadTime || operation.progress < 0.9f)
        {
            // Increment timer
            elapsedTime += Time.deltaTime;

            // Calculate progress based on timer
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);

            // Calculate progress based on the actual scene load
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // The progress bar will show the progress of whichever is SLOWER
            float displayProgress = Mathf.Min(timeProgress, loadProgress);
            if (loadingSlider != null)
            {
                loadingSlider.value = displayProgress;
            }

            yield return null;
        }

        // Allow the scene to activate
        operation.allowSceneActivation = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(CompleteSceneTransition());
    }

    private IEnumerator CompleteSceneTransition()
    {
        // Hide loading screen
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        // Fade from black to reveal the new scene
        yield return StartCoroutine(FadeFromBlack());
    }

    public static string GetAndClearNextSpawnPointID()
    {
        string id = nextSpawnPointID;
        nextSpawnPointID = null; // Clear the ID after it's been retrieved
        return id;
    }

    public void LoadSceneFromMenu(string sceneName)
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        LoadScene(sceneName, null);
    }

    public void StartNewGame()
    {
        LoadSceneFromMenu("Slums");
    }

    // Public methods to adjust fade timings at runtime
    public void SetFadeInDuration(float duration)
    {
        fadeInDuration = Mathf.Max(0f, duration);
    }

    public void SetFadeOutDuration(float duration)
    {
        fadeOutDuration = Mathf.Max(0f, duration);
    }

    public void SetFadeDurations(float fadeIn, float fadeOut)
    {
        fadeInDuration = Mathf.Max(0f, fadeIn);
        fadeOutDuration = Mathf.Max(0f, fadeOut);
    }
}
