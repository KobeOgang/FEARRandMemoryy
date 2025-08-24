using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject inventoryUIParent; // inventory UI panel

    [Header("List Elements")]
    public Transform itemListContentParent;   // parent object for the vertical list
    public GameObject itemListItemPrefab;     // prefab for each item in the list

    [Header("Display Elements")]
    public Image displayItemIcon;
    public TMP_Text displayItemName;
    public TMP_Text displayItemDescription;
    public Button useItemButton;

    private PuzzleObject currentPuzzleObject; // Stores which puzzle opened the inventory
    private ItemData selectedItemData; // Stores which item is currently selected

    void Start()
    {
        // Start with the inventory hidden
        inventoryUIParent.SetActive(false);
        if (useItemButton != null)
            useItemButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (inventoryUIParent.activeSelf && currentPuzzleObject != null && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
            return; // Optional: exit the Update method early after closing.
        }

        // Toggle inventory
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // If another menu is already open, do nothing.
            // Don't allow normal toggling if a puzzle is active or another menu is open
            if (currentPuzzleObject != null || (GameUIManager.isMenuOpen && !inventoryUIParent.activeSelf))
            {
                return;
            }


            bool isActive = !inventoryUIParent.activeSelf;
            if (isActive)
            {
                OpenMenu(); // Use helper function
            }
            else
            {
                CloseMenu(); // Use helper function
            }
        }
    }

    public void OpenForPuzzle(PuzzleObject puzzleObject)
    {
        currentPuzzleObject = puzzleObject; // Remember which puzzle we're solving
        OpenMenu();
    }

    private void OpenMenu()
    {
        inventoryUIParent.SetActive(true);
        GameUIManager.isMenuOpen = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PopulateItemList();
    }

    private void CloseMenu()
    {
        inventoryUIParent.SetActive(false);
        GameUIManager.isMenuOpen = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentPuzzleObject = null; // IMPORTANT: Forget the puzzle when we close
    }

    public void PopulateItemList()
    {
        // Clear old list items before repopulating
        foreach (Transform child in itemListContentParent)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance.inventory.Count == 0)
        {
            // Clear the display panel if no items are left
            displayItemIcon.gameObject.SetActive(false);
            displayItemName.text = "";
            displayItemDescription.text = "Inventory is empty.";
            return;
        }

        // Create a new list item for each item in the inventory
        for (int i = InventoryManager.Instance.inventory.Count - 1; i >= 0; i--)
        {
            ItemData item = InventoryManager.Instance.inventory[i];

            GameObject listItem = Instantiate(itemListItemPrefab, itemListContentParent);
            listItem.GetComponentInChildren<TMP_Text>().text = item.itemName;
            listItem.GetComponent<Button>().onClick.AddListener(() => DisplayItem(item));
        }

        DisplayItem(InventoryManager.Instance.inventory[InventoryManager.Instance.inventory.Count - 1]);
    }

    public void DisplayItem(ItemData data)
    {
        selectedItemData = data; // Keep track of the selected item

        displayItemName.text = data.itemName;
        displayItemDescription.text = data.description;
        displayItemIcon.sprite = data.icon;
        displayItemIcon.gameObject.SetActive(true);

        // Only show the "Use" button if the inventory was opened by a puzzle
        if (currentPuzzleObject != null)
        {
            useItemButton.gameObject.SetActive(true);
            // Hook up the OnClick event for the button
            useItemButton.onClick.RemoveAllListeners(); // Clear previous listeners
            useItemButton.onClick.AddListener(OnUseButtonPressed);
        }
    }

    private void OnUseButtonPressed()
    {
        if (currentPuzzleObject != null && selectedItemData != null)
        {
            // Tell the puzzle object that we are using the selected item on it
            currentPuzzleObject.UseItemOnPuzzle(selectedItemData);

            // Re-populate the list in case the item was consumed
            PopulateItemList();

            // Check if the puzzle was solved and disabled itself
            if (currentPuzzleObject.enabled == false)
            {
                CloseMenu();
            }
        }
    }
}
