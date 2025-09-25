using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Settings")]
    [Tooltip("The monster GameObject to activate when player enters trigger")]
    public GameObject monsterToActivate;

    [Header("Trigger Settings")]
    [Tooltip("Should the trigger be destroyed after activation?")]
    public bool destroyTriggerAfterActivation = true;

    [Tooltip("Optional delay before activating monster (in seconds)")]
    public float activationDelay = 0f;

    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = true;

    private bool hasTriggered = false;

    private void Start()
    {
        // Ensure this GameObject has a trigger collider
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError("MonsterTrigger: No Collider found! Please add a Collider component and set it as trigger.");
            return;
        }

        if (!triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
            if (showDebugMessages)
                Debug.Log("MonsterTrigger: Automatically set collider as trigger.");
        }

        // Validate monster reference
        if (monsterToActivate == null)
        {
            Debug.LogError("MonsterTrigger: Monster To Activate is not assigned!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player and we haven't triggered yet
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (showDebugMessages)
                Debug.Log($"MonsterTrigger: Player entered trigger zone. Activating {monsterToActivate.name}");

            if (activationDelay > 0f)
            {
                Invoke(nameof(ActivateMonster), activationDelay);
            }
            else
            {
                ActivateMonster();
            }
        }
    }

    private void ActivateMonster()
    {
        if (monsterToActivate != null)
        {
            monsterToActivate.SetActive(true);

            if (showDebugMessages)
                Debug.Log($"MonsterTrigger: {monsterToActivate.name} has been activated!");
        }

        if (destroyTriggerAfterActivation)
        {
            if (showDebugMessages)
                Debug.Log("MonsterTrigger: Destroying trigger after activation.");

            Destroy(gameObject);
        }
    }

    // Optional: Method to manually activate the monster (useful for testing)
    [ContextMenu("Test Activate Monster")]
    public void TestActivateMonster()
    {
        if (Application.isPlaying)
        {
            hasTriggered = true;
            ActivateMonster();
        }
    }
}
