using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance;

    private HashSet<string> collectedObjectIDs = new HashSet<string>();
    private Dictionary<string, int> puzzleStates = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    
    public void RecordObjectAsCollected(string uniqueID)
    {
        if (!collectedObjectIDs.Contains(uniqueID))
        {
            collectedObjectIDs.Add(uniqueID);
        }
    }

    
    public bool IsObjectCollected(string uniqueID)
    {
        return collectedObjectIDs.Contains(uniqueID);
    }

    public void RecordPuzzleState(string uniqueID, int state)
    {
        // If the puzzle is already in the dictionary, update its state.
        if (puzzleStates.ContainsKey(uniqueID))
        {
            puzzleStates[uniqueID] = state;
        }
        // Otherwise, add it.
        else
        {
            puzzleStates.Add(uniqueID, state);
        }
    }

    public bool GetPuzzleState(string uniqueID, out int state)
    {
        // Try to get the value. This returns true if the key exists, false otherwise.
        return puzzleStates.TryGetValue(uniqueID, out state);
    }

    public HashSet<string> GetAllCollectedIDs()
    {
        return collectedObjectIDs;
    }

    public Dictionary<string, int> GetAllPuzzleStates()
    {
        return puzzleStates;
    }

    public void ApplyLoadedData(GameData data)
    {
        this.collectedObjectIDs = new HashSet<string>(data.collectedObjectIDs_LIST);

        this.puzzleStates = new Dictionary<string, int>();
        for (int i = 0; i < data.puzzleStates_KEYS.Count; i++)
        {
            this.puzzleStates.Add(data.puzzleStates_KEYS[i], data.puzzleStates_VALUES[i]);
        }
    }

    [ContextMenu("Debug Print Stored Data")]
    public void DebugPrintStoredData()
    {
        Debug.Log("--- WorldStateManager Live Data ---");
        Debug.Log($"There are {collectedObjectIDs.Count} collected IDs:");
        foreach (string id in collectedObjectIDs)
        {
            Debug.Log("- " + id);
        }

        Debug.Log($"There are {puzzleStates.Count} puzzle states:");
        foreach (var puzzle in puzzleStates)
        {
            Debug.Log($"- ID: {puzzle.Key}, State: {puzzle.Value}");
        }
        Debug.Log("---------------------------------");
    }
}
