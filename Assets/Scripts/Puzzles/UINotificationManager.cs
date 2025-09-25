using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UINotificationManager : MonoBehaviour
{
    public static UINotificationManager Instance;

    [Header("UI References")]
    public GameObject notificationPanel; 
    public TMP_Text notificationText;    

    [Header("Text Effect Settings")]
    [Tooltip("The time in seconds between each character appearing.")]
    public float typingSpeed = 0.02f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        notificationPanel.SetActive(false);
    }

    // A coroutine to show a message for a specific duration
    public void ShowNotificationForDuration(string message, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(NotificationSequence(message, duration));
    }

    public void CloseNotificationImmediately()
    {
        StopAllCoroutines();
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public bool IsNotificationActive()
    {
        return notificationPanel != null && notificationPanel.activeSelf;
    }

    private IEnumerator NotificationSequence(string message, float duration)
    {
        // Set the text and show the panel
        notificationText.text = message;
        notificationPanel.SetActive(true);

        yield return StartCoroutine(TypeLine(message));

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Hide the panel again
        notificationPanel.SetActive(false);
    }

    private IEnumerator TypeLine(string line)
    {
        notificationText.text = "";
        foreach (char c in line.ToCharArray())
        {
            notificationText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
