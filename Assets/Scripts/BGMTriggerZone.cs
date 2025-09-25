using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMTriggerZone : MonoBehaviour
{
    [Header("BGM Settings")]
    [Tooltip("The BGM clip to change to when triggered")]
    public AudioClip newBGM;

    [Header("Trigger Settings")]
    [Tooltip("Whether this trigger can be activated multiple times")]
    public bool canTriggerMultipleTimes = false;

    [Tooltip("Whether to trigger on player enter or exit")]
    public bool triggerOnEnter = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnEnter && other.CompareTag("Player"))
        {
            TriggerBGMChange();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!triggerOnEnter && other.CompareTag("Player"))
        {
            TriggerBGMChange();
        }
    }

    private void TriggerBGMChange()
    {
        if (!canTriggerMultipleTimes && hasTriggered)
        {
            return;
        }

        if (newBGM == null)
        {
            Debug.LogWarning($"BGMTriggerZone '{gameObject.name}' has no BGM clip assigned!");
            return;
        }

        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager instance not found!");
            return;
        }

        AudioManager.instance.ChangeBGMWithFade(newBGM);
        hasTriggered = true;

        Debug.Log($"BGM changed to: {newBGM.name}");
    }

    private void OnValidate()
    {
        if (newBGM == null)
        {
            Debug.LogWarning($"BGMTriggerZone '{gameObject.name}' needs a BGM clip assigned!");
        }
    }
}
