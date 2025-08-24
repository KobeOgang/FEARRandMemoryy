using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueData dialogueToPlay;

    [Header("Trigger Settings")]
    [Tooltip("If true, dialogue starts on enter. If false, requires Q key press.")]
    public bool triggerOnEnter = false;

    [Header("Interaction UI")]
    [Tooltip("Assign the 'Press E to talk' UI element here.")]
    public GameObject interactPrompt;

    private bool isPlayerNearby = false;
    private bool isTalked = false;
    private PersistentObjectID objectID;

    private void Awake()
    {
        objectID = GetComponent<PersistentObjectID>();

        // Check the WorldStateManager on load
        if (objectID != null && WorldStateManager.Instance.IsObjectCollected(objectID.uniqueID))
        {
            // If this dialogue has already been triggered, mark it as talked.
            this.isTalked = true;
        }
    }

    private void Start()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (!triggerOnEnter && isPlayerNearby && !isTalked && Input.GetKeyDown(KeyCode.E))
        {
            StartTriggeredDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (!triggerOnEnter && !isTalked && interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }

            if (triggerOnEnter && !isTalked)
            {
                StartTriggeredDialogue();
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

    private void StartTriggeredDialogue()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        DialogueManager.Instance.StartDialogue(dialogueToPlay);
        isTalked = true;

        if (objectID != null)
        {
            WorldStateManager.Instance.RecordObjectAsCollected(objectID.uniqueID);
        }
    }
}
