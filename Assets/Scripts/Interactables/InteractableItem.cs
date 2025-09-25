using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;
    public GameObject interactPrompt;

    [Header("Pickup Response Type")]
    [Tooltip("Select how the player responds when picking up this item")]
    public PickupResponseType responseType = PickupResponseType.None;

    [Header("One-Liner Notification")]
    [Tooltip("Custom notification text when picked up. Leave empty for default messages.")]
    [TextArea(2, 4)]
    public string customNotificationText = "";

    [Tooltip("How long the notification stays on screen (in seconds)")]
    public float notificationDuration = 2.5f;

    [Header("Dialogue Sequence")]
    [Tooltip("Dialogue sequence to play when this item is picked up")]
    public DialogueData pickupDialogue;

    public enum PickupResponseType
    {
        None,
        OneLineNotification,
        DialogueSequence
    }

    private bool isPlayerNearby = false;
    private Outline outline;
    private PersistentObjectID objectID;

    private void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    private void Start()
    {
        // Ensure the prompt is hidden at the start
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        objectID = GetComponent<PersistentObjectID>();

        // This is the "check on load" part.
        if (objectID != null && WorldStateManager.Instance.IsObjectCollected(objectID.uniqueID))
        {
            Destroy(gameObject); // Destroy self if already collected.
        }

        // Validate configuration at start
        ValidatePickupConfiguration();
    }

    void Update()
    {
        if (InspectionManager.IsInspecting) return;

        // Start inspection when interacting
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Hide the prompt when interaction starts
            if (interactPrompt != null)
                interactPrompt.SetActive(false);

            InspectionManager.Instance.StartInspection(itemData, gameObject);
        }
    }

   
    public void TriggerPickupResponse()
    {
        switch (responseType)
        {
            case PickupResponseType.None:
                // No response
                break;

            case PickupResponseType.OneLineNotification:
                TriggerPickupNotification();
                break;

            case PickupResponseType.DialogueSequence:
                TriggerPickupDialogue();
                break;
        }
    }

    private void TriggerPickupNotification()
    {
        if (UINotificationManager.Instance == null)
        {
            Debug.LogWarning($"UINotificationManager.Instance is null! Cannot show notification for {itemData?.itemName}");
            return;
        }

        string notificationMessage = GetNotificationText();

        if (!string.IsNullOrEmpty(notificationMessage))
        {
            UINotificationManager.Instance.ShowNotificationForDuration(notificationMessage, notificationDuration);
        }
    }

    private void TriggerPickupDialogue()
    {
        if (pickupDialogue == null)
        {
            Debug.LogWarning($"Pickup dialogue is null for {itemData?.itemName}! Cannot play dialogue sequence.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning($"DialogueManager.Instance is null! Cannot play dialogue for {itemData?.itemName}");
            return;
        }

        // Check if dialogue is already active to prevent conflicts
        if (DialogueManager.IsDialogueActive || DialogueManager.IsNormalDialogueActive)
        {
            Debug.LogWarning($"Dialogue is already active! Skipping pickup dialogue for {itemData?.itemName}");
            return;
        }

        DialogueManager.Instance.StartDialogue(pickupDialogue);
        Debug.Log($"Started pickup dialogue sequence for {itemData?.itemName}");
    }

    
    private string GetNotificationText()
    {
        // Use custom text if provided
        if (!string.IsNullOrEmpty(customNotificationText))
        {
            return customNotificationText;
        }

        // Generate default messages based on item type
        if (itemData == null) return "";

        switch (itemData.itemType)
        {
            case ItemData.ItemType.Document:
                return GenerateDocumentNotification();

            case ItemData.ItemType.Key:
                return GenerateKeyNotification();

            default:
                return GenerateGenericNotification();
        }
    }

    private string GenerateDocumentNotification()
    {
        string[] documentMessages = new string[]
        {
            "Another piece of the puzzle...",
            "This document might contain important information.",
            "Let me add this to my notes.",
            "This could be useful later.",
            "I should read this carefully."
        };

        return documentMessages[Random.Range(0, documentMessages.Length)];
    }

    private string GenerateKeyNotification()
    {
        string[] keyMessages = new string[]
        {
            "This key might open something important.",
            "I wonder what this unlocks...",
            "Another key for my collection.",
            "This looks like it fits somewhere.",
            "Better keep this safe."
        };

        return keyMessages[Random.Range(0, keyMessages.Length)];
    }

    private string GenerateGenericNotification()
    {
        string[] genericMessages = new string[]
        {
            "This might come in handy.",
            "I should hold onto this.",
            "Interesting... I'll keep this.",
            "This could be important.",
            "Better take this with me."
        };

        return genericMessages[Random.Range(0, genericMessages.Length)];
    }

    private void ValidatePickupConfiguration()
    {
        if (responseType == PickupResponseType.DialogueSequence && pickupDialogue == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Response type is set to DialogueSequence but no DialogueData is assigned!", this);
        }

        if (responseType == PickupResponseType.OneLineNotification && string.IsNullOrEmpty(customNotificationText) && itemData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Response type is set to OneLineNotification but no custom text is provided and itemData is null!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // Show prompt when player is nearby
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }

            if (outline != null)
            {
                outline.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            // Hide prompt when player leaves
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }

    #region Inspector Validation
    private void OnValidate()
    {
        // This runs in the editor to help prevent configuration errors
        ValidatePickupConfiguration();
    }
    #endregion
}
