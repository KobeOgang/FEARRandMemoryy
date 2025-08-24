using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Interaction Settings")]
    [Tooltip("The message displayed when the player first interacts with this locked object.")]
    [TextArea] public string lockedMessage = "Door is locked.";
    public GameObject interactPrompt;
    [Tooltip("The text to display on the interact prompt (e.g., '[E] Examine', '[E] Check Device').")]
    public string interactPromptText = "[E] Examine";


    private bool hasBeenNotified = false;

    private bool isPlayerNearby = false;
    private PersistentObjectID objectID;

    private void Start()
    {
        objectID = GetComponent<PersistentObjectID>();
        if (objectID == null) return; // Exit if there's no ID

        // On load, check the manager for a saved state
        int savedState;
        if (WorldStateManager.Instance.GetPuzzleState(objectID.uniqueID, out savedState))
        {
            // If a state was found, update this puzzle
            currentState = savedState;

            // IMPORTANT: We must also restore the world to its correct state.
            // For example, if a door was opened, it needs to be opened again on load.
            // This loop re-invokes the success events for all completed stages.
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
                // ...then open the inventory to solve the puzzle.
                InventoryManager.Instance.inventoryUI.OpenForPuzzle(this);
            }
        }
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
        }
        else
        {
            Debug.Log("Wrong item used for this stage.");
            // Play a "fail" sound.
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
        }
    }
}
