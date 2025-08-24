using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text slotInfoText;
    [SerializeField] private TMP_Text emptySlotText;

    private int slotNumber;
    private Button thisButton;
    private LoadView parentView;

    private void Awake()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(OnSlotClicked);
    }

    public void Initialize(int number, LoadView view)
    {
        this.slotNumber = number;
        this.parentView = view;
    }

    public void UpdateDisplay(GameData data)
    {
        if (data == null)
        {
            slotInfoText.gameObject.SetActive(false);
            emptySlotText.gameObject.SetActive(true);
            thisButton.interactable = false; // Make empty slots unclickable
        }
        else
        {
            emptySlotText.gameObject.SetActive(false);
            slotInfoText.gameObject.SetActive(true);
            slotInfoText.text = $"Scene: {data.sceneName}\nSaved: {data.saveTimestamp}";
            thisButton.interactable = true;
        }
    }

    private void OnSlotClicked()
    {
        parentView.OnSlotSelected(slotNumber);
    }
}
