using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodexManager : MonoBehaviour
{
    public static CodexManager Instance;

    public List<ItemData> collectedDocuments = new List<ItemData>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddDocument(ItemData document)
    {
        if (!collectedDocuments.Contains(document))
        {
            collectedDocuments.Add(document);
            Debug.Log("Added document to Codex: " + document.itemName);
        }
    }

    public void ApplyLoadedData(GameData data)
    {
        this.collectedDocuments = data.codexDocuments;
    }
}
