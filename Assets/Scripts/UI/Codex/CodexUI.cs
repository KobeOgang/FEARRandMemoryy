using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CodexUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject codexUIParent; // The codex UI canvas/panel
    public GameObject fullTextPanel;   //panel that shows the full itemText

    [Header("List Elements")]
    public Transform listContentParent; // vertical layout group
    public GameObject documentListItemPrefab; // prefab for each item in the list

    [Header("Display Elements")]
    public TMP_Text displayNameText;
    public TMP_Text displayDescriptionText;
    public TMP_Text displayFullText;
    public Image displayIconImage;
    public GameObject readTextPrompt;

    private ItemData currentSelectedDocument;

    void Start()
    {
        codexUIParent.SetActive(false);
        fullTextPanel.SetActive(false);
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.J))
        {
            // If another menu is already open, do nothing.
            if (GameUIManager.isMenuOpen && !codexUIParent.activeSelf)
            {
                return;
            }

            bool isActive = !codexUIParent.activeSelf;
            codexUIParent.SetActive(isActive);

            // Update the global state flag
            GameUIManager.isMenuOpen = isActive;

            if (isActive)
            {
                Time.timeScale = 0f; // Pause the game
                Cursor.lockState = CursorLockMode.None; // Unlock the cursor
                Cursor.visible = true; // Make the cursor visible
                PopulateDocumentList();
            }
            else
            {
                Time.timeScale = 1f; // Resume the game
                Cursor.lockState = CursorLockMode.Locked; // Lock the cursor again
                Cursor.visible = false; // Hide the cursor again
            }
        }*/

        // showing the full text with F
        if (codexUIParent.activeSelf && currentSelectedDocument != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                fullTextPanel.SetActive(true);
            }
            if (Input.GetKeyUp(KeyCode.F))
            {
                fullTextPanel.SetActive(false);
            }
        }
    }

    void PopulateDocumentList()
    {
        // Clear old list items
        foreach (Transform child in listContentParent)
        {
            Destroy(child.gameObject);
        }

        // Check if any documents have been collected
        if (CodexManager.Instance.collectedDocuments.Count == 0)
        {
            displayIconImage.gameObject.SetActive(false);
            displayNameText.text = "";
            displayDescriptionText.text = "No documents collected.";

            if (fullTextPanel != null)
                readTextPrompt.SetActive(false);

            return; 
        }

        
        for (int i = CodexManager.Instance.collectedDocuments.Count - 1; i >= 0; i--)
        {
            ItemData doc = CodexManager.Instance.collectedDocuments[i];

            GameObject listItem = Instantiate(documentListItemPrefab, listContentParent);
            listItem.GetComponentInChildren<TMP_Text>().text = doc.itemName;
            listItem.GetComponent<Button>().onClick.AddListener(() => DisplayDocument(doc));
        }

        DisplayDocument(CodexManager.Instance.collectedDocuments[CodexManager.Instance.collectedDocuments.Count - 1]);

        if (readTextPrompt != null)
            readTextPrompt.SetActive(true);
    }

    public void DisplayDocument(ItemData data)
    {
        currentSelectedDocument = data;

        displayNameText.text = data.itemName;
        displayDescriptionText.text = data.description;
        displayFullText.text = data.itemText;

        // Display the item's icon
        if (data.icon != null)
        {
            displayIconImage.sprite = data.icon;
            displayIconImage.gameObject.SetActive(true);
        }
        else
        {
            displayIconImage.gameObject.SetActive(false);
        }

        fullTextPanel.SetActive(false);
    }

    public void OpenCodex()
    {
        codexUIParent.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PopulateDocumentList();

        // Register with GameUIManager
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.RegisterUIOpened(GameUIManager.UIType.Codex);
        }
    }

    public void CloseCodex()
    {
        codexUIParent.SetActive(false);
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        fullTextPanel.SetActive(false);

        // Register with GameUIManager
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.RegisterUIClosed(GameUIManager.UIType.Codex);
        }
    }
}
