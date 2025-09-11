using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class InspectionManager : MonoBehaviour
{
    public static InspectionManager Instance;
    public static bool IsInspecting = false;

    [Header("UI Elements")]
    public GameObject inspectionUI;      
    public TMP_Text itemNameText;        
    public TMP_Text itemDescriptionText; 

    [Header("Inspection Settings")]
    public Transform inspectionTransform;
    public Light inspectionSpotlight;

    [Header("Document Reading UI")]
    public GameObject inspectionFullTextPanel;  
    public TMP_Text inspectionFullTextDisplay;  
    public TMP_Text readTextPrompt;


    private GameObject currentItem;  
    private bool isInspecting = false;

    private GameObject originalItem;
    private bool isReadingDocument = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (inspectionUI != null)
        {
            inspectionUI.SetActive(false);
        }

        if (inspectionSpotlight != null)
        {
            inspectionSpotlight.enabled = false;
        }
    }

    void Update()
    {
        if (isInspecting)
        {
            if (currentItem.GetComponent<InteractableItem>().itemData.itemType == ItemData.ItemType.Document)
            {
                // toggle the full text view
                if (Input.GetKeyDown(KeyCode.F))
                {
                    isReadingDocument = !isReadingDocument;
                    inspectionFullTextPanel.SetActive(isReadingDocument);

                    currentItem.SetActive(!isReadingDocument);
                }
            }

            if (isReadingDocument) return;

            // Rotate the item based on mouse movement
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            currentItem.transform.Rotate(Vector3.up, -mouseX * 5f, Space.World);
            currentItem.transform.Rotate(Vector3.right, mouseY * 5f, Space.World);

            // Add item to inventory and exit inspection mode
            if (Input.GetMouseButtonDown(0))
            {
                // Get the item data from the inspected item
                ItemData dataOfInspectedItem = currentItem.GetComponent<InteractableItem>().itemData;

                // Get the Unique ID component from the original object in the scene
                PersistentObjectID objectID = originalItem.GetComponent<PersistentObjectID>();
                if (objectID != null)
                {
                    WorldStateManager.Instance.RecordObjectAsCollected(objectID.uniqueID);
                }

                // if t's a document, add it to the Codex
                if (dataOfInspectedItem.itemType == ItemData.ItemType.Document)
                {
                    CodexManager.Instance.AddDocument(dataOfInspectedItem);

                    Destroy(originalItem);
                    Destroy(currentItem);
                    EndInspection();
                }
                else
                {
                    bool added = InventoryManager.Instance.AddItem(currentItem.GetComponent<InteractableItem>().itemData);
                    if (added)
                    {
                        Destroy(originalItem); 
                        Destroy(currentItem);  
                        EndInspection();      
                    }
                    else
                    {
                        Debug.LogWarning("Item could not be added to inventory (likely inventory is full).");
                    }
                }
   
            }

            if (Input.GetMouseButtonDown(1))
            {
                Destroy(currentItem);
                EndInspection();
            }
        }

    }

    public void StartInspection(ItemData itemData, GameObject itemObject)
    {

        IsInspecting = true;

        if (inspectionSpotlight != null)
        {
            inspectionSpotlight.enabled = true;
        }

        originalItem = itemObject;

        Time.timeScale = 0;

        inspectionUI.SetActive(true);
        itemNameText.text = itemData.itemName;
        itemDescriptionText.text = itemData.description;

        if (itemData.itemType == ItemData.ItemType.Document)
        {
            readTextPrompt.gameObject.SetActive(true);
            inspectionFullTextDisplay.text = itemData.itemText;
        }

        currentItem = Instantiate(itemObject, inspectionTransform.position, inspectionTransform.rotation);
        currentItem.GetComponent<Collider>().enabled = false;
        currentItem.transform.SetParent(inspectionTransform);

        isInspecting = true;
    }


    public void EndInspection()
    {
        if (inspectionSpotlight != null)
        {
            inspectionSpotlight.enabled = false;
        }

        isReadingDocument = false;
        if (inspectionFullTextPanel != null) inspectionFullTextPanel.SetActive(false);
        if (readTextPrompt != null) readTextPrompt.gameObject.SetActive(false);

        IsInspecting = false;
        Time.timeScale = 1;

        inspectionUI.SetActive(false);

        Destroy(currentItem);
        currentItem = null;

        isInspecting = false;
    }

}
