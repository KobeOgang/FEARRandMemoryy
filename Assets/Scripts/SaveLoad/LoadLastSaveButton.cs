using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class LoadLastSaveButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Optional text to show when no saves are available")]
    public TextMeshProUGUI statusText;
    [Tooltip("Optional popup GameObject to show when no saves exist")]
    public GameObject noSavesPopup;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        // Optionally disable the button if no saves exist
        if (button != null)
        {
            button.interactable = HasAnySaves();
        }

        UpdateStatusText();
    }

    public void OnLoadLastSaveClicked()
    {
        int mostRecentSlot = FindMostRecentSave();

        if (mostRecentSlot != -1)
        {
            GameData saveData = SaveSystem.GetSaveInfo(mostRecentSlot);
            string slotType = mostRecentSlot == 0 ? "Autosave" : $"Save Slot {mostRecentSlot}";
            Debug.Log($"Loading most recent save: {slotType} from {saveData.saveTimestamp}");

            SaveSystem.LoadGame(mostRecentSlot);
        }
        else
        {
            Debug.LogWarning("No save files found!");
            ShowNoSavesMessage();
        }
    }

    private int FindMostRecentSave()
    {
        int mostRecentSlot = -1;
        DateTime mostRecentDate = DateTime.MinValue;

        // Check all save slots (0 = autosave, 1-5 = manual saves)
        for (int i = 0; i <= 5; i++)
        {
            GameData saveData = SaveSystem.GetSaveInfo(i);

            if (saveData != null && !string.IsNullOrEmpty(saveData.saveTimestamp))
            {
                // Parse the timestamp using the same format as SaveSystem
                if (DateTime.TryParseExact(saveData.saveTimestamp, "yyyy-MM-dd HH:mm:ss",
                    null, System.Globalization.DateTimeStyles.None, out DateTime saveDate))
                {
                    if (saveDate > mostRecentDate)
                    {
                        mostRecentDate = saveDate;
                        mostRecentSlot = i;
                    }
                }
            }
        }

        return mostRecentSlot;
    }

    private void ShowNoSavesMessage()
    {
        if (noSavesPopup != null)
        {
            noSavesPopup.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.text = "No save files available";
            statusText.color = Color.red;
        }
    }

    private void UpdateStatusText()
    {
        if (statusText == null) return;

        int mostRecentSlot = FindMostRecentSave();
        if (mostRecentSlot != -1)
        {
            GameData saveData = SaveSystem.GetSaveInfo(mostRecentSlot);
            string slotType = mostRecentSlot == 0 ? "Autosave" : $"Save Slot {mostRecentSlot}";
            statusText.text = $"Most Recent: {slotType}\n{saveData.saveTimestamp}";
            statusText.color = Color.white;
        }
        else
        {
            statusText.text = "No saves available";
            statusText.color = Color.gray;
        }
    }

    public bool HasAnySaves()
    {
        for (int i = 0; i <= 5; i++)
        {
            if (SaveSystem.GetSaveInfo(i) != null)
            {
                return true;
            }
        }
        return false;
    }
}
