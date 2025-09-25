using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{

    [Tooltip("Assign the '[E] Save Game' prompt UI element here.")]
    [SerializeField] private GameObject interactPrompt;

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
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.OpenUI(GameUIManager.UIType.Save);
            }
            else
            {
                Debug.LogError("SavePoint: GameUIManager not found!");
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
