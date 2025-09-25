using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject inventoryUIParent;

    [Header("List Elements")]
    public Transform itemListContentParent;
    public GameObject itemListItemPrefab;

    [Header("Display Elements")]
    public Image displayItemIcon;
    public TMP_Text displayItemName;
    public TMP_Text displayItemDescription;
    public Button useItemButton;

    private PuzzleObject currentPuzzleObject;
    private ItemData selectedItemData;

    void Start()
    {
        inventoryUIParent.SetActive(false);
        if (useItemButton != null)
            useItemButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (inventoryUIParent.activeSelf && currentPuzzleObject != null && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
            return;
        }
    }

    public void OpenForPuzzle(PuzzleObject puzzleObject)
    {
        currentPuzzleObject = puzzleObject;
        OpenMenu();

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.RegisterUIOpened(GameUIManager.UIType.PuzzleInventory);
        }
    }

    private void OpenMenu()
    {
        inventoryUIParent.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PopulateItemList();

        // Register with GameUIManager
        if (GameUIManager.Instance != null && currentPuzzleObject == null)
        {
            GameUIManager.Instance.RegisterUIOpened(GameUIManager.UIType.Inventory);
        }
    }

    private void CloseMenu()
    {
        inventoryUIParent.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentPuzzleObject != null)
        {
            currentPuzzleObject.OnInventoryClosed();
        }

        currentPuzzleObject = null;

        // Register with GameUIManager
        if (GameUIManager.Instance != null)
        {
            if (currentPuzzleObject != null)
            {
                GameUIManager.Instance.RegisterUIClosed(GameUIManager.UIType.PuzzleInventory);
            }
            else
            {
                GameUIManager.Instance.RegisterUIClosed(GameUIManager.UIType.Inventory);
            }
        }
    }

    public void PopulateItemList()
    {
        //Clear old list items
        foreach (Transform child in itemListContentParent)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance.inventory.Count == 0)
        {
            displayItemIcon.gameObject.SetActive(false);
            displayItemName.text = "";
            displayItemDescription.text = "Inventory is empty.";

            // Hide use button when inventory is empty
            if (useItemButton != null)
                useItemButton.gameObject.SetActive(false);

            return;
        }

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
        selectedItemData = data;

        displayItemName.text = data.itemName;
        displayItemDescription.text = data.description;
        displayItemIcon.sprite = data.icon;
        displayItemIcon.gameObject.SetActive(true);

        // Show use button for puzzle interactions OR consumable items
        bool shouldShowUseButton = false;

        if (currentPuzzleObject != null)
        {
            // Puzzle mode - show for all items
            shouldShowUseButton = true;
        }
        else if (data.itemType == ItemData.ItemType.Consumable)
        {
            // Normal inventory mode - show only for consumables
            shouldShowUseButton = true;
        }

        if (shouldShowUseButton)
        {
            useItemButton.gameObject.SetActive(true);
            useItemButton.onClick.RemoveAllListeners();
            useItemButton.onClick.AddListener(OnUseButtonPressed);

            // Update button text based on context
            TMP_Text buttonText = useItemButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                if (currentPuzzleObject != null)
                {
                    buttonText.text = "Use Item";
                }
                else if (data.itemType == ItemData.ItemType.Consumable)
                {
                    buttonText.text = "Consume";
                }
            }
        }
        else
        {
            useItemButton.gameObject.SetActive(false);
        }
    }

    private void OnUseButtonPressed()
    {
        if (selectedItemData == null) return;

        if (currentPuzzleObject != null)
        {
            // Handle puzzle interaction
            currentPuzzleObject.UseItemOnPuzzle(selectedItemData);
            PopulateItemList();

            if (currentPuzzleObject.enabled == false)
            {
                CloseMenu();
            }
        }
        else if (selectedItemData.itemType == ItemData.ItemType.Consumable)
        {
            // Handle consumable item
            ConsumeItem(selectedItemData);
        }
    }

    private void ConsumeItem(ItemData consumableItem)
    {
        // Find the PlayerHealth component
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("InventoryUI: PlayerHealth component not found! Cannot consume item.");
            return;
        }

        // Check if player is already at full health
        if (playerHealth.GetCurrentHealth() >= playerHealth.maxHealth)
        {
            Debug.Log("Player is already at full health. Cannot use health pack.");
            // Optionally show a message to the player
            ShowConsumableMessage("I don't think I need that right now...");
            return;
        }

        // Heal the player
        playerHealth.Heal(consumableItem.healAmount);

        // Remove the item from inventory
        bool itemRemoved = InventoryManager.Instance.RemoveItem(consumableItem);

        if (itemRemoved)
        {
            Debug.Log($"Consumed {consumableItem.itemName}, healed for {consumableItem.healAmount} health.");

            // Refresh the inventory display
            PopulateItemList();
        }
        else
        {
            Debug.LogError("Failed to remove consumed item from inventory!");
        }
    }

    private void ShowConsumableMessage(string message)
    {
        // Close the inventory UI first
        CloseMenu();

        // Show the notification using UINotificationManager
        if (UINotificationManager.Instance != null)
        {
            UINotificationManager.Instance.ShowNotificationForDuration(message, 2.5f);
        }
        else
        {
            // Fallback if UINotificationManager not found
            Debug.Log($"Consumable Message: {message}");
        }
    }

    public void OpenInventory()
    {
        OpenMenu();
    }

    public void CloseInventory()
    {
        CloseMenu();
    }
}
