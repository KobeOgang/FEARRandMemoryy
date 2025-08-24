using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;         // Reference to the ScriptableObject data
    public GameObject interactPrompt; // Text indicator prefab (assign in Inspector)
    /*public float pickupRange = 2f;*/

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

}
