using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    private bool justClosedUI = false;

    // UI State Management
    public enum UIType
    {
        None,
        Pause,
        Inventory,
        PuzzleInventory,
        Codex,
        Save,
        Load,
        Inspection,
        Dialogue
    }

    private UIType currentActiveUI = UIType.None;
    private Stack<UIType> uiStack = new Stack<UIType>();

    // UI System References
    [Header("UI System References")]
    public PauseManager pauseManager;
    public InventoryManager inventoryManager;
    public CodexManager codexManager;
    public SaveView saveView;
    public InspectionManager inspectionManager;
    public DialogueManager dialogueManager;

    // UI Input Settings
    [Header("Input Keys")]
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode inventoryKey = KeyCode.Tab;
    public KeyCode codexKey = KeyCode.J;

    // Delegates for UI state changes
    public System.Action<UIType> OnUIOpened;
    public System.Action<UIType> OnUIClosed;

    private void Awake()
    {
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

    private void Start()
    {
        // Auto-find UI components if not assigned
        if (pauseManager == null) pauseManager = FindObjectOfType<PauseManager>();
        if (inventoryManager == null) inventoryManager = FindObjectOfType<InventoryManager>();
        if (codexManager == null) codexManager = FindObjectOfType<CodexManager>();
        if (saveView == null) saveView = FindObjectOfType<SaveView>();
        if (inspectionManager == null) inspectionManager = FindObjectOfType<InspectionManager>();
        if (dialogueManager == null) dialogueManager = FindObjectOfType<DialogueManager>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Handle ESC key based on current UI state
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }

        // Handle other UI keys only if no blocking UI is open
        if (!IsBlockingUIActive())
        {
            if (Input.GetKeyDown(inventoryKey))
            {
                ToggleInventory();
            }

            if (Input.GetKeyDown(codexKey))
            {
                ToggleCodex();
            }
        }
    }

    private void HandleEscapeKey()
    {

        switch (currentActiveUI)
        {
            case UIType.None:
                // No UI open, show pause menu
                OpenUI(UIType.Pause);
                break;

            case UIType.Inventory:
                // Close inventory
                CloseUI(UIType.Inventory);
                break;

            case UIType.PuzzleInventory:
                CloseUI(UIType.PuzzleInventory);
                break;

            case UIType.Codex:
                // Close codex
                CloseUI(UIType.Codex);
                break;

            case UIType.Save:
                // Close save menu
                CloseUI(UIType.Save);
                break;

            case UIType.Load:
                // Close load menu
                CloseUI(UIType.Load);
                break;

            case UIType.Pause:
                // Close pause menu
                CloseUI(UIType.Pause);
                break;

            case UIType.Inspection:
                // ESC during inspection - end inspection
                if (inspectionManager != null)
                {
                    inspectionManager.EndInspection();
                    CloseUI(UIType.Inspection);
                }
                break;

            case UIType.Dialogue:
                // ESC during dialogue - do nothing (dialogues should complete naturally)
                break;
        }
    }

    // Public methods for opening/closing UIs
    public void OpenUI(UIType uiType)
    {
        if (currentActiveUI == uiType) return; // Already open

        // Close current UI if it exists and can be interrupted
        if (currentActiveUI != UIType.None && CanInterruptUI(currentActiveUI))
        {
            CloseCurrentUIInternal();
        }

        // Push current UI to stack for restoration
        if (currentActiveUI != UIType.None)
        {
            uiStack.Push(currentActiveUI);
        }

        // Open new UI
        currentActiveUI = uiType;
        OpenUIInternal(uiType);

        OnUIOpened?.Invoke(uiType);
    }

    public void CloseUI(UIType uiType)
    {
        if (currentActiveUI != uiType) return; // Not currently active

        CloseCurrentUIInternal();

        // Restore previous UI from stack
        if (uiStack.Count > 0)
        {
            currentActiveUI = uiStack.Pop();
        }
        else
        {
            currentActiveUI = UIType.None;
        }

        OnUIClosed?.Invoke(uiType);
    }

    public void CloseCurrentUI()
    {
        if (currentActiveUI != UIType.None)
        {
            CloseUI(currentActiveUI);
        }
    }

    // Toggle methods
    public void TogglePause()
    {
        if (currentActiveUI == UIType.Pause)
        {
            CloseUI(UIType.Pause);
        }
        else if (CanOpenUI(UIType.Pause))
        {
            OpenUI(UIType.Pause);
        }
    }

    public void ToggleInventory()
    {
        if (currentActiveUI == UIType.Inventory)
        {
            CloseUI(UIType.Inventory);
        }
        else if (CanOpenUI(UIType.Inventory))
        {
            OpenUI(UIType.Inventory);
        }
    }

    public void ToggleCodex()
    {
        if (currentActiveUI == UIType.Codex)
        {
            CloseUI(UIType.Codex);
        }
        else if (CanOpenUI(UIType.Codex))
        {
            OpenUI(UIType.Codex);
        }
    }

    // State queries
    public bool IsUIActive(UIType uiType)
    {
        return currentActiveUI == uiType;
    }

    public bool IsAnyUIActive()
    {
        return currentActiveUI != UIType.None;
    }

    public bool IsBlockingUIActive()
    {
        return currentActiveUI == UIType.Dialogue ||
               currentActiveUI == UIType.Inspection;
    }

    public UIType GetCurrentUI()
    {
        return currentActiveUI;
    }

    // Helper methods
    private bool CanOpenUI(UIType uiType)
    {
        // Can't open UI during dialogue or inspection
        if (IsBlockingUIActive() && uiType != UIType.Pause)
        {
            return false;
        }

        return true;
    }

    private bool CanInterruptUI(UIType uiType)
    {
        // Dialogue and inspection cannot be interrupted
        return uiType != UIType.Dialogue && uiType != UIType.Inspection;
    }

    private void OpenUIInternal(UIType uiType)
    {
        switch (uiType)
        {
            case UIType.Pause:
                if (pauseManager != null)
                {
                    pauseManager.OpenPauseMenu();
                }
                break;

            case UIType.Inventory:
                if (inventoryManager != null && inventoryManager.inventoryUI != null)
                {
                    inventoryManager.inventoryUI.OpenInventory();
                }
                break;

            case UIType.PuzzleInventory: // NEW: Handle puzzle inventory
                
                break;

            case UIType.Codex:
                // Assuming CodexUI has similar method
                var codexUI = FindObjectOfType<CodexUI>();
                if (codexUI != null)
                {
                    codexUI.OpenCodex();
                }
                break;

            case UIType.Save:
                if (saveView != null)
                {
                    saveView.Open();
                }
                break;

            case UIType.Inspection:
                // Inspection is opened by InspectionManager.StartInspection()
                break;

            case UIType.Dialogue:
                // Dialogue is opened by DialogueManager.StartDialogue()
                break;
        }
    }

    private void CloseCurrentUIInternal()
    {
        switch (currentActiveUI)
        {
            case UIType.Pause:
                if (pauseManager != null)
                {
                    pauseManager.ClosePauseMenu();
                }
                break;

            case UIType.Inventory:
                if (inventoryManager != null && inventoryManager.inventoryUI != null)
                {
                    inventoryManager.inventoryUI.CloseInventory();
                }
                break;

            case UIType.PuzzleInventory: 
                if (inventoryManager != null && inventoryManager.inventoryUI != null)
                {
                    inventoryManager.inventoryUI.CloseInventory();
                }
                break;

            case UIType.Codex:
                var codexUI = FindObjectOfType<CodexUI>();
                if (codexUI != null)
                {
                    codexUI.CloseCodex();
                }
                break;

            case UIType.Save:
                if (saveView != null)
                {
                    saveView.Close();
                }
                break;

            case UIType.Inspection:
                if (inspectionManager != null)
                {
                    inspectionManager.EndInspection();
                }
                break;
        }
    }

    // Methods for other systems to register UI state changes
    public void RegisterUIOpened(UIType uiType)
    {
        currentActiveUI = uiType;
        OnUIOpened?.Invoke(uiType);
    }

    public void RegisterUIClosed(UIType uiType)
    {
        if (currentActiveUI == uiType)
        {
            currentActiveUI = UIType.None;
            OnUIClosed?.Invoke(uiType);
        }
    }

    // Legacy support
    public static bool isMenuOpen
    {
        get { return Instance != null && Instance.IsAnyUIActive(); }
    }
}
