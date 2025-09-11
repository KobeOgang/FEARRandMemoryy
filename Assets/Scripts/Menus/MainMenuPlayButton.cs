using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the first scene to load when starting new game")]
    public string firstGameScene = "SlmsB1";

    public void OnPlayClicked()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadSceneFromMenu(firstGameScene);
        }
        else
        {
            Debug.LogError("SceneLoader not found! Loading scene directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(firstGameScene);
        }
    }
}
