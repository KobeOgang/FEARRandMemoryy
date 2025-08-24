using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [Header("Door Configuration")]
    public string sceneToLoad;
    public string targetSpawnPointID;

    [Header("Interaction UI")]
    public GameObject interactPrompt;

    private bool isPlayerNearby = false;

    private void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (SceneLoader.Instance != null)
            {
                // We no longer pass the spawn point tag
                SceneLoader.Instance.LoadScene(sceneToLoad, targetSpawnPointID);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }
}
