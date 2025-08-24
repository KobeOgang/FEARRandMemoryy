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

    [Header("Settings")]
    public float minimumLoadTime = 6f;

    private static string nextSpawnPointID;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // subscribe to the event when this object is enabled
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // unsubscribe when it's disabled to prevent errors
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName, string spawnPointID)
    {
        // Store the ID so we can use it after the new scene loads
        nextSpawnPointID = spawnPointID;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreen.SetActive(true);

        float elapsedTime = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // loop continues until the REAL load is ready AND our minimum time has passed
        while (elapsedTime < minimumLoadTime || operation.progress < 0.9f)
        {
            // Increment timer
            elapsedTime += Time.deltaTime;

            // Calculate progress based on timer
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);

            // Calculate progress based on the actual scene load
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // The progress bar will show the progress of whichever is SLOWER.
            float displayProgress = Mathf.Min(timeProgress, loadProgress);
            loadingSlider.value = displayProgress;

            yield return null;
        }

        // allow the scene to activate.
        operation.allowSceneActivation = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // new scene is loaded, safely hide the loading screen.
        loadingScreen.SetActive(false);
        
    }

    public static string GetAndClearNextSpawnPointID()
    {
        string id = nextSpawnPointID;
        nextSpawnPointID = null; // Clear the ID after it's been retrieved
        return id;
    }
}
