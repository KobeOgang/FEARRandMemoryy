using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadView : View
{
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject loadSlotPrefab;
    [SerializeField] private LoadSlotUI autosaveSlotUI;

    // This is called automatically when the view is shown by the ViewManager
    public override void Show()
    {
        base.Show(); // Activates the panel GameObject
        PopulateSlots();
        PopulateAutosaveSlot();
    }

    private void PopulateAutosaveSlot()
    {
        if (autosaveSlotUI != null)
        {
            // Slot 0 is our dedicated autosave slot
            GameData autosaveData = SaveSystem.GetSaveInfo(0);
            autosaveSlotUI.Initialize(0, this);
            autosaveSlotUI.UpdateDisplay(autosaveData);
        }
    }

    private void PopulateSlots()
    {
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i <= 5; i++)
        {
            GameObject slotGO = Instantiate(loadSlotPrefab, slotContainer);
            LoadSlotUI slotUI = slotGO.GetComponent<LoadSlotUI>();

            // Initialize the slot and update its display
            slotUI.Initialize(i, this);
            slotUI.UpdateDisplay(SaveSystem.GetSaveInfo(i));
        }
    }

    // This is called by the LoadSlotUI button when it's clicked
    public void OnSlotSelected(int slotNumber)
    {
        // Only try to load if the slot actually contains save data
        if (SaveSystem.GetSaveInfo(slotNumber) != null)
        {
            SaveSystem.LoadGame(slotNumber);
        }
    }

    // You need to implement the abstract Initialize method, even if it's empty
    public override void Initialize() { }
}
