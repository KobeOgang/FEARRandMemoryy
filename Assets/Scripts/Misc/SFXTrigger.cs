using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXTrigger : MonoBehaviour
{
    [Header("Sound Effects")]
    [Tooltip("List of sound effects to play when triggered")]
    public AudioClip[] triggerSFX;

    [Header("Trigger Settings")]
    [Tooltip("Should the trigger be destroyed after activation?")]
    public bool destroyTriggerAfterActivation = false;

    [Tooltip("Optional delay before playing sounds (in seconds)")]
    public float playDelay = 0f;

    [Tooltip("Can this trigger be activated multiple times?")]
    public bool canTriggerMultipleTimes = false;

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
            Debug.LogError("SFXTrigger: No Collider found! Please add a Collider component and set it as trigger.");
            return;
        }

        if (!triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
            if (showDebugMessages)
                Debug.Log("SFXTrigger: Automatically set collider as trigger.");
        }

        // Validate SFX array
        if (triggerSFX == null || triggerSFX.Length == 0)
        {
            Debug.LogWarning("SFXTrigger: No SFX clips assigned!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player and we should trigger
        if (other.CompareTag("Player") && ShouldTrigger())
        {
            if (!canTriggerMultipleTimes)
            {
                hasTriggered = true;
            }

            if (showDebugMessages)
                Debug.Log($"SFXTrigger: Player entered trigger zone. Playing {triggerSFX.Length} sound effects.");

            if (playDelay > 0f)
            {
                StartCoroutine(PlaySFXWithDelay());
            }
            else
            {
                PlaySFXList();
            }
        }
    }

    private bool ShouldTrigger()
    {
        return canTriggerMultipleTimes || !hasTriggered;
    }

    private IEnumerator PlaySFXWithDelay()
    {
        yield return new WaitForSeconds(playDelay);
        PlaySFXList();
    }

    private void PlaySFXList()
    {
        // Check if AudioManager is available
        if (AudioManager.instance == null)
        {
            Debug.LogError("SFXTrigger: AudioManager instance not found!");
            return;
        }

        // Play all sound effects simultaneously
        int successCount = 0;
        for (int i = 0; i < triggerSFX.Length; i++)
        {
            if (triggerSFX[i] != null)
            {
                AudioManager.instance.PlayClip(triggerSFX[i]);
                successCount++;

                if (showDebugMessages)
                    Debug.Log($"SFXTrigger: Playing SFX {i + 1}/{triggerSFX.Length}: {triggerSFX[i].name}");
            }
            else
            {
                Debug.LogWarning($"SFXTrigger: SFX clip at index {i} is null!");
            }
        }

        if (showDebugMessages)
            Debug.Log($"SFXTrigger: Successfully played {successCount}/{triggerSFX.Length} sound effects.");

        // Destroy trigger if requested
        if (destroyTriggerAfterActivation)
        {
            if (showDebugMessages)
                Debug.Log("SFXTrigger: Destroying trigger after activation.");

            Destroy(gameObject);
        }
    }

    // Optional: Method to manually trigger the SFX (useful for testing)
    [ContextMenu("Test Play SFX")]
    public void TestPlaySFX()
    {
        if (Application.isPlaying)
        {
            if (!canTriggerMultipleTimes)
            {
                hasTriggered = true;
            }
            PlaySFXList();
        }
    }

    // Optional: Method to reset the trigger so it can be activated again
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (showDebugMessages)
            Debug.Log("SFXTrigger: Trigger has been reset.");
    }

    // Optional: Public method to enable/disable multiple triggering at runtime
    public void SetCanTriggerMultipleTimes(bool canTrigger)
    {
        canTriggerMultipleTimes = canTrigger;
        if (canTrigger)
        {
            hasTriggered = false; // Reset if we're now allowing multiple triggers
        }
    }
}
