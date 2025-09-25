using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveView : MonoBehaviour
{
    [SerializeField] private GameObject savePanel;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private SaveSlotUI autosaveSlotUI;

    [Header("Confirmation Popup")]
    [SerializeField] private GameObject confirmationPopup;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private int selectedSlot;
    private bool isInitialized = false;

    private void Awake()
    {
        // Ensure initialization happens before any other component tries to use this
        InitializeComponent();
    }

    private void Start()
    {
        if (!isInitialized)
        {
            InitializeComponent();
        }

        // Hook up confirmation button listeners
        confirmYesButton.onClick.AddListener(OnConfirmOverwrite);
        confirmNoButton.onClick.AddListener(OnCancelOverwrite);
        savePanel.SetActive(false); // Start hidden
        confirmationPopup.SetActive(false);
    }

    private void InitializeComponent()
    {
        if (isInitialized) return;

        // Validate all required components
        if (savePanel == null)
        {
            Debug.LogError("SaveView: savePanel is not assigned in the inspector!", this);
            return;
        }

        if (slotContainer == null)
        {
            Debug.LogError("SaveView: slotContainer is not assigned in the inspector!", this);
            return;
        }

        if (saveSlotPrefab == null)
        {
            Debug.LogError("SaveView: saveSlotPrefab is not assigned in the inspector!", this);
            return;
        }

        isInitialized = true;
        Debug.Log("SaveView: Component initialized successfully");
    }

    public void Open()
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.OpenUI(GameUIManager.UIType.Save);
        }
        else
        {
            OpenInternal();
        }
    }

    public void Close()
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.CloseUI(GameUIManager.UIType.Save);
        }
        else
        {
            CloseInternal();
        }
    }

    public void OpenInternal()
    {
        if (!isInitialized)
        {
            InitializeComponent();
        }

        if (!isInitialized)
        {
            Debug.LogError("SaveView: Cannot open - component not properly initialized!");
            return;
        }

        Debug.Log("SaveView: Opening save panel");
        savePanel.SetActive(true);
        PopulateSlots();
        PopulateAutosaveSlot();
        Time.timeScale = 0f; // Pause game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInternal()
    {
        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }
        Time.timeScale = 1f; // Resume game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Allow closing with Escape key
        /*if (savePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }*/
    }

    private void PopulateAutosaveSlot()
    {
        if (autosaveSlotUI != null)
        {
            GameData autosaveData = SaveSystem.GetSaveInfo(0);
            autosaveSlotUI.Initialize(0, this);
            autosaveSlotUI.UpdateDisplay(autosaveData);

            // Disable the button component to prevent clicks
            autosaveSlotUI.GetComponent<Button>().interactable = false;
        }
    }

    private void PopulateSlots()
    {
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i <= 5; i++)
        {
            GameObject slotGO = Instantiate(saveSlotPrefab, slotContainer);
            SaveSlotUI slotUI = slotGO.GetComponent<SaveSlotUI>();

            slotUI.Initialize(i, this);
            slotUI.UpdateDisplay(SaveSystem.GetSaveInfo(i));
        }
    }

    public void OnSlotSelected(int slotNumber)
    {
        if (slotNumber == 0) return;

        this.selectedSlot = slotNumber;
        if (SaveSystem.GetSaveInfo(slotNumber) != null)
        {
            confirmationPopup.SetActive(true);
        }
        else
        {
            SaveGameToSlot();
        }
    }

    private void OnConfirmOverwrite()
    {
        SaveGameToSlot();
        confirmationPopup.SetActive(false);
    }

    private void OnCancelOverwrite()
    {
        confirmationPopup.SetActive(false);
    }

    private void SaveGameToSlot()
    {
        SaveSystem.SaveGame(selectedSlot);
        PopulateSlots(); // Refresh UI to show new save data
    }
}
