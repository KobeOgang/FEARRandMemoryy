using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public InventoryUI inventoryUI;

    [Header("Inventory Settings")]
    public int maxSlots = 15;
    public List<ItemData> inventory = new List<ItemData>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Add this line
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // NOTE: The Update method with the Tab key logic has been REMOVED from this script.

    public bool AddItem(ItemData item)
    {
        if (inventory.Count >= maxSlots)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        inventory.Add(item);
        Debug.Log("Added item: " + item.itemName);

        if (inventoryUI != null && inventoryUI.inventoryUIParent.activeSelf)
        {
            inventoryUI.PopulateItemList();
        }

        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        if (inventory.Contains(item))
        {
            inventory.Remove(item);
            Debug.Log("Removed item: " + item.itemName);

            // Refresh UI if it's currently open
            if (inventoryUI != null && inventoryUI.inventoryUIParent.activeSelf)
            {
                inventoryUI.PopulateItemList();
            }

            return true;
        }

        Debug.LogWarning("Attempted to remove item not in inventory: " + item.itemName);
        return false;
    }

    public bool HasItem(ItemData item)
    {
        return inventory.Contains(item);
    }

    public int GetItemCount(ItemData item)
    {
        int count = 0;
        foreach (ItemData inventoryItem in inventory)
        {
            if (inventoryItem == item)
            {
                count++;
            }
        }
        return count;
    }

    public void ApplyLoadedData(GameData data)
    {
        this.inventory = data.inventoryItems;
    }
}
