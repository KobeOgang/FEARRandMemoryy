using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Player's Location
    public string sceneName;
    public float[] playerPosition; // Stored as a float array [x, y, z]

    // Manager Data
    public List<ItemData> inventoryItems;
    public List<ItemData> codexDocuments;
    public List<string> collectedObjectIDs_LIST;
    public List<string> puzzleStates_KEYS;
    public List<int> puzzleStates_VALUES;

    // Save Metadata
    public string saveTimestamp;

    // Default constructor for a new game
    public GameData()
    {
        this.sceneName = "OutdoorScene"; // Or your game's starting scene name
        this.playerPosition = new float[] { 0, 0, 0 }; // A default starting position

        this.inventoryItems = new List<ItemData>();
        this.codexDocuments = new List<ItemData>();
        this.collectedObjectIDs_LIST = new List<string>();
        this.puzzleStates_KEYS = new List<string>();
        this.puzzleStates_VALUES = new List<int>();
    }
}
