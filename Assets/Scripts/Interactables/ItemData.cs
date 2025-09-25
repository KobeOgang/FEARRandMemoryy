using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Key,
        Document,
        Consumable,
        Miscellaneous
    }

    [Header("Item Properties")]
    public ItemType itemType;   // The type of the item
    public string itemName;     // The name of the item
    [TextArea]
    public string description;  // A description of the item

    [Header("Document Content")]
    [TextArea(10, 20)] // Makes the text box bigger in the inspector
    public string itemText;

    [Header("Consumable Properties")]
    [Tooltip("Amount of health this consumable restores when used")]
    public float healAmount = 25f;

    [Header("Visual Representation")]
    public Sprite icon;            // Icon for inventory UI display
    public GameObject prefab;
}

