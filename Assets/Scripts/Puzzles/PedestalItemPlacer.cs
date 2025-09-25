using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestalItemPlacer : MonoBehaviour
{
    [Header("Pedestal Settings")]
    [Tooltip("The empty GameObject where items will be placed")]
    public Transform itemSlot;

    [Tooltip("Items that correspond to each puzzle stage")]
    public GameObject[] itemPrefabs;

    [Header("Placement Settings")]
    [Tooltip("Local position offset for placed items")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("Local rotation offset for placed items")]
    public Vector3 rotationOffset = Vector3.zero;

    [Tooltip("Scale multiplier for placed items")]
    public float scaleMultiplier = 1f;

    // Track placed items for cleanup
    private GameObject[] placedItems;

    private void Start()
    {
        // Initialize array to track placed items
        if (itemPrefabs != null)
        {
            placedItems = new GameObject[itemPrefabs.Length];
        }

        // Validate setup
        if (itemSlot == null)
        {
            Debug.LogError($"[{gameObject.name}] PedestalItemPlacer: No item slot assigned!");
        }
    }

   
    public void PlaceItemForStage(int stageIndex)
    {
        Debug.Log($"[PedestalItemPlacer] PlaceItemForStage called with index: {stageIndex}");

        if (itemSlot == null)
        {
            Debug.LogError($"[{gameObject.name}] Cannot place item: No item slot assigned!");
            return;
        }

        Debug.Log($"[PedestalItemPlacer] Item slot found: {itemSlot.name}");
        Debug.Log($"[PedestalItemPlacer] Item prefabs array length: {(itemPrefabs?.Length ?? 0)}");

        if (itemPrefabs == null || stageIndex >= itemPrefabs.Length || stageIndex < 0)
        {
            Debug.LogError($"[{gameObject.name}] Invalid stage index {stageIndex} or no item prefabs assigned!");
            return;
        }

        if (itemPrefabs[stageIndex] == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No prefab assigned for stage {stageIndex}");
            return;
        }

        Debug.Log($"[PedestalItemPlacer] About to instantiate: {itemPrefabs[stageIndex].name}");

        // Remove existing item for this stage if any
        if (placedItems[stageIndex] != null)
        {
            DestroyImmediate(placedItems[stageIndex]);
        }

        // Instantiate new item
        GameObject newItem = Instantiate(itemPrefabs[stageIndex], itemSlot);

        Debug.Log($"[PedestalItemPlacer] Item instantiated: {newItem.name} at position {newItem.transform.position}");
        Debug.Log($"[PedestalItemPlacer] NEW ITEM CREATED:");
        Debug.Log($"  - Name: {newItem.name}");
        Debug.Log($"  - Parent: {(newItem.transform.parent ? newItem.transform.parent.name : "NO PARENT")}");
        Debug.Log($"  - World Position: {newItem.transform.position}");
        Debug.Log($"  - Local Position: {newItem.transform.localPosition}");
        Debug.Log($"  - Active in Hierarchy: {newItem.activeInHierarchy}");
        Debug.Log($"  - Active Self: {newItem.activeSelf}");

        // Apply positioning
        newItem.transform.localPosition = positionOffset;
        newItem.transform.localRotation = Quaternion.Euler(rotationOffset);
        newItem.transform.localScale = newItem.transform.localScale * scaleMultiplier;

        newItem.transform.localPosition = Vector3.up * 2f; // 2 units above ObjectLockPoint
        newItem.transform.localScale = Vector3.one * 5f; // Make it HUGE
        newItem.SetActive(true);

        Debug.Log($"[PedestalItemPlacer] AFTER POSITIONING:");
        Debug.Log($"  - World Position: {newItem.transform.position}");
        Debug.Log($"  - Local Position: {newItem.transform.localPosition}");

        // Disable collider to prevent interaction issues
        Collider itemCollider = newItem.GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        // Store reference
        placedItems[stageIndex] = newItem;

        Debug.Log($"[{gameObject.name}] Placed item '{itemPrefabs[stageIndex].name}' for stage {stageIndex}");
    }

    public void PlaceItemStage0() => PlaceItemForStage(0);
    public void PlaceItemStage1() => PlaceItemForStage(1);
    public void PlaceItemStage2() => PlaceItemForStage(2);
    public void PlaceItemStage3() => PlaceItemForStage(3);
    public void PlaceItemStage4() => PlaceItemForStage(4);

    public void RemoveItemForStage(int stageIndex)
    {
        if (placedItems != null && stageIndex >= 0 && stageIndex < placedItems.Length)
        {
            if (placedItems[stageIndex] != null)
            {
                DestroyImmediate(placedItems[stageIndex]);
                placedItems[stageIndex] = null;
            }
        }
    }

    public void ClearAllItems()
    {
        if (placedItems != null)
        {
            for (int i = 0; i < placedItems.Length; i++)
            {
                if (placedItems[i] != null)
                {
                    DestroyImmediate(placedItems[i]);
                    placedItems[i] = null;
                }
            }
        }
    }
}
