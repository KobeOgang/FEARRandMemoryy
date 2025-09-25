using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class PuzzleObject : MonoBehaviour
{
    [Header("Puzzle State")]
    [Tooltip("The current stage of the puzzle. Starts at 0.")]
    private int currentState = 0;

    [Header("Puzzle Settings")]
    [Tooltip("The list of items required, in order. Item 0 for State 0, etc.")]
    public ItemData[] requiredItems;

    [Header("Success Events")]
    [Tooltip("The list of events to trigger for each stage.")]
    public UnityEvent[] onSuccessEvents;

    [Header("Close-Up Camera")]
    [Tooltip("Virtual camera for close-up view of the puzzle")]
    public CinemachineVirtualCamera closeUpCamera;

    [Tooltip("How long to show close-up view before opening inventory")]
    [Range(1f, 5f)]
    public float closeUpDuration = 2.5f;

    [Tooltip("Camera priority when active (should be higher than main camera)")]
    public int activeCameraPriority = 20;

    [Header("Interaction Settings")]
    [Tooltip("The message displayed when the player first interacts with this locked object.")]
    [TextArea] public string lockedMessage = "Door is locked.";
    public GameObject interactPrompt;
    [Tooltip("The text to display on the interact prompt (e.g., '[E] Examine', '[E] Check Device').")]
    public string interactPromptText = "[E] Examine";

    [Header("Wrong Item Messages")]
    [Tooltip("Random messages to display when the player uses the wrong item")]
    [TextArea]
    public string[] wrongItemMessages = {
    "That's not it...",
    "I don't think that would work...",
    "This doesn't seem right.",
    "Maybe I need something else.",
    "That won't help here.",
    "I should try a different approach.",
    "This isn't what I need.",
    "I need to think about this more...",
    "That doesn't fit.",
    "Wrong tool for the job."
};

    private bool hasBeenNotified = false;

    private bool isPlayerNearby = false;
    private PersistentObjectID objectID;

    private bool isShowingCloseUp = false;
    private int originalCameraPriority = 0;

    private void Start()
    {
        objectID = GetComponent<PersistentObjectID>();
        if (objectID == null) return; // Exit if there's no ID

        if (closeUpCamera != null)
        {
            originalCameraPriority = closeUpCamera.Priority;
            closeUpCamera.Priority = 0; // Set to 0 by default
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No close-up camera assigned to PuzzleObject!");
        }

        // On load, check the manager for a saved state
        int savedState;
        if (WorldStateManager.Instance.GetPuzzleState(objectID.uniqueID, out savedState))
        {
            // If a state was found, update this puzzle
            currentState = savedState;


            for (int i = 0; i < currentState; i++)
            {
                if (i < onSuccessEvents.Length)
                {
                    onSuccessEvents[i].Invoke();
                }
            }
        }
    }

    private void Update()
    {
        // First, check if the player is nearby and presses the interact key
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            // If this is the VERY FIRST time the player interacts...
            if (hasBeenNotified == false)
            {
                // Mark that the player has now been notified.
                hasBeenNotified = true;

                Debug.Log(lockedMessage);
                UINotificationManager.Instance.ShowNotificationForDuration(lockedMessage, 3f);
            }
            else
            {
                // MODIFIED: Handle subsequent interactions with camera sequence
                if (UINotificationManager.Instance.IsNotificationActive())
                {
                    UINotificationManager.Instance.CloseNotificationImmediately();
                }

                // NEW: Start close-up camera sequence instead of opening inventory directly
                if (!isShowingCloseUp)
                {
                    StartCloseUpSequence();
                }
            }
        }
    }

    private void StartCloseUpSequence()
    {
        if (closeUpCamera == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No close-up camera assigned! Opening inventory directly.");
            OpenPuzzleInventory();
            return;
        }

        isShowingCloseUp = true;

        // Activate close-up camera
        closeUpCamera.Priority = activeCameraPriority;

        // Start coroutine to open inventory after delay
        StartCoroutine(CloseUpSequence());
    }

    private IEnumerator CloseUpSequence()
    {
        // Wait for the close-up duration
        yield return new WaitForSeconds(closeUpDuration);

        // Open puzzle inventory
        OpenPuzzleInventory();
    }

    private void OpenPuzzleInventory()
    {
        InventoryManager.Instance.inventoryUI.OpenForPuzzle(this);
    }

    public void OnInventoryClosed()
    {
        // Reset camera when inventory is closed
        ResetCamera();
    }

    private void ResetCamera()
    {
        if (closeUpCamera != null)
        {
            closeUpCamera.Priority = 0; // Reset to default priority
        }

        isShowingCloseUp = false;
    }

    // This method is called by the UI when the player tries to use an item.
    public void UseItemOnPuzzle(ItemData playerItem)
    {
        // First, check if the puzzle is already complete.
        if (currentState >= requiredItems.Length)
        {
            Debug.Log("Puzzle is already solved.");
            return;
        }

        // Check if the item used is correct FOR THE CURRENT STAGE.
        if (playerItem == requiredItems[currentState])
        {
            Debug.Log("Correct item for stage " + currentState);

            if (currentState < onSuccessEvents.Length)
            {
                onSuccessEvents[currentState].Invoke();
            }

            // Remove the used item from inventory.
            InventoryManager.Instance.inventory.Remove(playerItem);

            currentState++;

            // Report the NEW state of the puzzle to the manager
            if (objectID != null)
            {
                WorldStateManager.Instance.RecordPuzzleState(objectID.uniqueID, currentState);
            }

            ResetCamera();
        }
        else
        {
            Debug.Log("Wrong item used for this stage.");

            if (wrongItemMessages.Length > 0 && UINotificationManager.Instance != null)
            {
                string randomMessage = wrongItemMessages[Random.Range(0, wrongItemMessages.Length)];
                UINotificationManager.Instance.ShowNotificationForDuration(randomMessage, 3f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (interactPrompt != null && currentState < requiredItems.Length)
            {
                TMPro.TMP_Text promptTextComponent = interactPrompt.GetComponent<TMPro.TMP_Text>();
                if (promptTextComponent != null)
                {
                    promptTextComponent.text = interactPromptText;
                }

                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            if (isShowingCloseUp)
            {
                StopAllCoroutines();
                ResetCamera();
            }
        }
    }
}
