using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutosaveTrigger : MonoBehaviour
{
    [Tooltip("A unique ID for this trigger so it only fires once. e.g., 'Entered_Alleyway_Autosave'")]
    [SerializeField] private string uniqueTriggerID;

    [Tooltip("Check this if you want the trigger to be a physical box in the scene.")]
    [SerializeField] private bool isZoneTrigger = true;

    private void Start()
    {
        // If this is a zone trigger, ensure the collider is set to be a trigger.
        if (isZoneTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
            else
            {
                Debug.LogError("AutosaveTrigger is set as a Zone Trigger, but no Collider is attached!", this.gameObject);
            }
        }
    }

    // This method will be called by zone triggers.
    private void OnTriggerEnter(Collider other)
    {
        if (isZoneTrigger && other.CompareTag("Player"))
        {
            DoAutosave();
        }
    }

    // This is the public method you can call from anywhere (like a UnityEvent).
    public void DoAutosave()
    {
        // First, check with the WorldStateManager to see if this autosave has already happened.
        if (WorldStateManager.Instance.IsObjectCollected(uniqueTriggerID))
        {
            // If it has happened, do nothing.
            return;
        }

        // If it's a new autosave, call the SaveSystem.
        Debug.Log("AUTOSAVING: " + uniqueTriggerID);
        SaveSystem.AutosaveGame();

        // Immediately record that this autosave has occurred so it doesn't happen again.
        WorldStateManager.Instance.RecordObjectAsCollected(uniqueTriggerID);
    }
}
