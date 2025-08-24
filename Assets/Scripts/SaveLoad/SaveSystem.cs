using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public static class SaveSystem
{
    // This will hold the data from a loaded game right before we load the new scene
    public static GameData dataToLoad = null;

    private static string GetSaveFilePath(int slotNumber)
    {
        string fileName = slotNumber == 0 ? "autosave.json" : "save_slot_" + slotNumber + ".json";
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public static void SaveGame(int slotNumber)
    {
        GameData data = new GameData();

        // --- GATHER DATA (SAME AS BEFORE) ---
        PlayerController player = Object.FindObjectOfType<PlayerController>();
        data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        data.playerPosition = new float[] { player.transform.position.x, player.transform.position.y, player.transform.position.z };
        data.inventoryItems = InventoryManager.Instance.inventory;
        data.codexDocuments = CodexManager.Instance.collectedDocuments;

        // --- CONVERT AND STORE WORLD STATE ---
        data.collectedObjectIDs_LIST = WorldStateManager.Instance.GetAllCollectedIDs().ToList();
        data.puzzleStates_KEYS = WorldStateManager.Instance.GetAllPuzzleStates().Keys.ToList();
        data.puzzleStates_VALUES = WorldStateManager.Instance.GetAllPuzzleStates().Values.ToList();

        data.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // --- SERIALIZE AND WRITE (SAME AS BEFORE) ---
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSaveFilePath(slotNumber), json);
        Debug.Log("Game saved to slot " + slotNumber);
    }

    public static void LoadGame(int slotNumber)
    {
        string path = GetSaveFilePath(slotNumber);
        if (File.Exists(path))
        {
            // Read the file and deserialize it
            string json = File.ReadAllText(path);
            dataToLoad = JsonUtility.FromJson<GameData>(json);

            // Load the scene specified in the save data
            UnityEngine.SceneManagement.SceneManager.LoadScene(dataToLoad.sceneName);
        }
        else
        {
            Debug.LogError("Save file not found for slot " + slotNumber);
        }
    }

    // A helper method to just read the data without loading a scene (for the UI)
    public static GameData GetSaveInfo(int slotNumber)
    {
        string path = GetSaveFilePath(slotNumber);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        return null;
    }

    public static void AutosaveGame()
    {
        // We use slot 0 as the dedicated autosave slot.
        SaveGame(0);
        Debug.Log("Game progress has been autosaved.");
    }
}
