using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(InteractableItem))]
public class InteractableItemEditor : Editor
{
    // Serialized properties
    private SerializedProperty itemDataProp;
    private SerializedProperty interactPromptProp;
    private SerializedProperty responseTypeProp;
    private SerializedProperty customNotificationTextProp;
    private SerializedProperty notificationDurationProp;
    private SerializedProperty pickupDialogueProp;

    private void OnEnable()
    {
        // Find serialized properties
        itemDataProp = serializedObject.FindProperty("itemData");
        interactPromptProp = serializedObject.FindProperty("interactPrompt");
        responseTypeProp = serializedObject.FindProperty("responseType");
        customNotificationTextProp = serializedObject.FindProperty("customNotificationText");
        notificationDurationProp = serializedObject.FindProperty("notificationDuration");
        pickupDialogueProp = serializedObject.FindProperty("pickupDialogue");
    }

    public override void OnInspectorGUI()
    {
        InteractableItem item = (InteractableItem)target;

        // Start checking for changes
        serializedObject.Update();

        // Draw basic item properties
        DrawBasicProperties();

        EditorGUILayout.Space(10);

        // Draw pickup response type
        DrawResponseTypeSelection();

        EditorGUILayout.Space(10);

        // Draw conditional sections based on response type
        DrawConditionalSections(item);

        EditorGUILayout.Space(10);

        // Draw configuration status
        DrawConfigurationStatus(item);

        // Apply property modifications
        if (serializedObject.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(item);
        }
    }

    private void DrawBasicProperties()
    {
        EditorGUILayout.LabelField("Item Data", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(itemDataProp, new GUIContent("Item Data", "The ScriptableObject containing item information"));
        EditorGUILayout.PropertyField(interactPromptProp, new GUIContent("Interact Prompt", "UI GameObject shown when player can interact"));

        EditorGUI.indentLevel--;
    }

    private void DrawResponseTypeSelection()
    {
        EditorGUILayout.LabelField("Pickup Response Type", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(responseTypeProp, new GUIContent("Response Type", "Select how the player responds when picking up this item"));

        // Add helpful description for each type
        InteractableItem.PickupResponseType currentType = (InteractableItem.PickupResponseType)responseTypeProp.enumValueIndex;
        string description = GetResponseTypeDescription(currentType);

        if (!string.IsNullOrEmpty(description))
        {
            EditorGUILayout.HelpBox(description, MessageType.Info);
        }

        EditorGUI.indentLevel--;
    }

    private void DrawConditionalSections(InteractableItem item)
    {
        InteractableItem.PickupResponseType currentType = (InteractableItem.PickupResponseType)responseTypeProp.enumValueIndex;

        switch (currentType)
        {
            case InteractableItem.PickupResponseType.None:
                // Show nothing additional
                EditorGUILayout.HelpBox("No additional configuration needed for silent pickup.", MessageType.Info);
                break;

            case InteractableItem.PickupResponseType.OneLineNotification:
                DrawOneLineNotificationSection();
                break;

            case InteractableItem.PickupResponseType.DialogueSequence:
                DrawDialogueSequenceSection();
                break;
        }
    }

    private void DrawOneLineNotificationSection()
    {
        EditorGUILayout.LabelField("One-Liner Notification", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(customNotificationTextProp, new GUIContent("Custom Notification Text", "Custom text to display. Leave empty for auto-generated messages based on item type."));

        EditorGUILayout.PropertyField(notificationDurationProp, new GUIContent("Duration (seconds)", "How long the notification stays on screen"));

        // Clamp duration to reasonable values
        if (notificationDurationProp.floatValue < 0.5f)
            notificationDurationProp.floatValue = 0.5f;
        if (notificationDurationProp.floatValue > 10f)
            notificationDurationProp.floatValue = 10f;

        EditorGUI.indentLevel--;

        // Preview section
        EditorGUILayout.Space(5);
        DrawNotificationPreview();
    }

    private void DrawDialogueSequenceSection()
    {
        EditorGUILayout.LabelField("Dialogue Sequence", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(pickupDialogueProp, new GUIContent("Pickup Dialogue", "DialogueData asset to play when this item is picked up"));

        EditorGUI.indentLevel--;

        // Preview section
        EditorGUILayout.Space(5);
        DrawDialoguePreview();
    }

    private void DrawNotificationPreview()
    {
        InteractableItem item = (InteractableItem)target;

        EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;

        string previewText = "";

        if (!string.IsNullOrEmpty(customNotificationTextProp.stringValue))
        {
            previewText = customNotificationTextProp.stringValue;
            EditorGUILayout.LabelField("Custom Message:", EditorStyles.miniLabel);
        }
        else if (item.itemData != null)
        {
            previewText = $"[Auto-generated based on {item.itemData.itemType}]";
            EditorGUILayout.LabelField("Auto-Generated:", EditorStyles.miniLabel);
        }
        else
        {
            previewText = "[No ItemData - message generation may fail]";
            EditorGUILayout.LabelField("Warning:", EditorStyles.miniLabel);
        }

        EditorGUILayout.SelectableLabel(previewText, EditorStyles.textField, GUILayout.Height(20));

        EditorGUI.indentLevel--;
    }

    private void DrawDialoguePreview()
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;

        if (pickupDialogueProp.objectReferenceValue != null)
        {
            DialogueData dialogue = pickupDialogueProp.objectReferenceValue as DialogueData;

            if (dialogue != null)
            {
                EditorGUILayout.LabelField($"Dialogue: {dialogue.name}");

                if (dialogue.conversationLines != null && dialogue.conversationLines.Length > 0)
                {
                    EditorGUILayout.LabelField($"Lines: {dialogue.conversationLines.Length}");
                    EditorGUILayout.LabelField($"Is Monologue: {dialogue.isMonologue}");

                    // Show first line as preview
                    if (!string.IsNullOrEmpty(dialogue.conversationLines[0].dialogueText))
                    {
                        string firstLine = dialogue.conversationLines[0].dialogueText;
                        if (firstLine.Length > 60)
                            firstLine = firstLine.Substring(0, 57) + "...";

                        EditorGUILayout.LabelField("First Line:", EditorStyles.miniLabel);
                        EditorGUILayout.SelectableLabel($"\"{firstLine}\"", EditorStyles.textField, GUILayout.Height(20));
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Dialogue has no lines!", MessageType.Warning);
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("No dialogue assigned", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUI.indentLevel--;
    }

    private void DrawConfigurationStatus(InteractableItem item)
    {
        EditorGUILayout.LabelField("Configuration Status", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        InteractableItem.PickupResponseType currentType = (InteractableItem.PickupResponseType)responseTypeProp.enumValueIndex;

        switch (currentType)
        {
            case InteractableItem.PickupResponseType.None:
                EditorGUILayout.HelpBox("✓ Configuration complete. Item will be picked up silently.", MessageType.Info);
                break;

            case InteractableItem.PickupResponseType.OneLineNotification:
                ValidateNotificationConfiguration(item);
                break;

            case InteractableItem.PickupResponseType.DialogueSequence:
                ValidateDialogueConfiguration(item);
                break;
        }

        EditorGUI.indentLevel--;
    }

    private void ValidateNotificationConfiguration(InteractableItem item)
    {
        bool hasCustomText = !string.IsNullOrEmpty(customNotificationTextProp.stringValue);
        bool hasItemData = item.itemData != null;

        if (hasCustomText)
        {
            EditorGUILayout.HelpBox("✓ Configuration complete. Will display custom message.", MessageType.Info);
        }
        else if (hasItemData)
        {
            EditorGUILayout.HelpBox($"✓ Configuration complete. Will auto-generate message for {item.itemData.itemType}.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ Warning: No custom text and no ItemData. Message generation may fail.", MessageType.Warning);
        }

        if (notificationDurationProp.floatValue < 1f)
        {
            EditorGUILayout.HelpBox("⚠ Duration is very short. Consider increasing for better readability.", MessageType.Warning);
        }
    }

    private void ValidateDialogueConfiguration(InteractableItem item)
    {
        if (pickupDialogueProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("❌ Error: No DialogueData assigned! Dialogue sequence cannot play.", MessageType.Error);
        }
        else
        {
            DialogueData dialogue = pickupDialogueProp.objectReferenceValue as DialogueData;
            if (dialogue != null && dialogue.conversationLines != null && dialogue.conversationLines.Length > 0)
            {
                EditorGUILayout.HelpBox("✓ Configuration complete. Dialogue sequence ready.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠ Warning: Assigned DialogueData has no conversation lines.", MessageType.Warning);
            }
        }
    }

    private string GetResponseTypeDescription(InteractableItem.PickupResponseType type)
    {
        switch (type)
        {
            case InteractableItem.PickupResponseType.None:
                return "Item will be picked up without any player response.";
            case InteractableItem.PickupResponseType.OneLineNotification:
                return "Shows a single-line typed notification when picked up.";
            case InteractableItem.PickupResponseType.DialogueSequence:
                return "Plays a full dialogue sequence when picked up.";
            default:
                return "";
        }
    }
}
#endif
