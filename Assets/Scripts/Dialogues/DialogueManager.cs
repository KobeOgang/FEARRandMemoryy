using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text speakerNameText; // You can use the speakerID directly or have a separate name field

    [Header("Character References")]
    public PlayerController playerController;

    [Header("Text Effect Settings")]
    [Tooltip("The time in seconds between each character appearing.")]
    public float typingSpeed = 0.02f;

    // --- Private State Variables ---
    private Queue<DialogueLine> lines;
    private Dictionary<string, DialogueParticipant> participants = new Dictionary<string, DialogueParticipant>();
    private Dictionary<string, DialogueCamera> cameras = new Dictionary<string, DialogueCamera>();
    public static bool IsDialogueActive = false;
    public static bool IsNormalDialogueActive = false;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentFullLine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        lines = new Queue<DialogueLine>();
    }

    void Start()
    {
        // Find all participants and cameras in the scene at the start
        RegisterAllParticipants();
        RegisterAllCameras();
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        // This logic handles NORMAL dialogues.
        if (IsDialogueActive && !IsMonologueActive() && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                CompleteLine();
            }
            else
            {
                if (lines.Count == 0)
                {
                    EndDialogue();
                }
                else
                {
                    DisplayNextLine();
                }
            }
        }
    }

    public void StartDialogue(DialogueData data)
    {
        IsDialogueActive = true;



        // Check the new isMonologue flag
        if (!data.isMonologue)
        {
            IsNormalDialogueActive = true;

            // If it's a normal dialogue, force the player to idle.
            if (playerController != null)
            {
                playerController.ForceIdle();
            }
        }

        dialoguePanel.SetActive(true);
        lines.Clear();

        foreach (DialogueLine line in data.conversationLines)
        {
            line.isMonologueData = data.isMonologue;
            lines.Enqueue(line);
        }

        // Check if we should start the automatic monologue coroutine
        if (data.isMonologue)
        {
            StartCoroutine(MonologueCoroutine());
        }
        else // Otherwise, start the normal interactive dialogue
        {
            DisplayNextLine();
        }
    }

    private IEnumerator MonologueCoroutine()
    {
        // Loop while there are still lines in the queue.
        while (lines.Count > 0)
        {
            // Dequeue the line and immediately start displaying it.
            DialogueLine currentLine = lines.Dequeue();
            DisplayLine(currentLine);

            // Wait for the typewriter to finish.
            yield return new WaitUntil(() => !isTyping);

            // Then, wait for that specific line's display duration.
            yield return new WaitForSeconds(currentLine.displayDuration);
        }

        // After the loop finishes (meaning all lines have been shown), end the dialogue.
        EndDialogue();
    }

    public void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine nextLine = lines.Dequeue();
        DisplayLine(nextLine);
    }

    private void DisplayLine(DialogueLine line)
    {
        currentFullLine = line.dialogueText;

        // Direct the Camera
        foreach (var cam in cameras.Values) cam.SetActive(false);
        if (cameras.ContainsKey(line.cameraShotID))
        {
            cameras[line.cameraShotID].SetActive(true);
        }

        // Trigger the Animation
        if (participants.ContainsKey(line.speakerID))
        {
            speakerNameText.text = line.speakerID;
            Animator speakerAnimator = participants[line.speakerID].GetComponent<Animator>();
            if (speakerAnimator != null && !string.IsNullOrEmpty(line.animationTrigger))
            {
                speakerAnimator.SetTrigger(line.animationTrigger);
            }
        }

        // Start the typewriter effect
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine(currentFullLine));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        typingCoroutine = null;
    }

    private void CompleteLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        dialogueText.text = currentFullLine;
        isTyping = false;
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        StopAllCoroutines();
        IsDialogueActive = false;
        IsNormalDialogueActive = false;
        dialoguePanel.SetActive(false);

        // Deactivate all dialogue cameras to return to the main gameplay camera
        foreach (var cam in cameras.Values) cam.SetActive(false);
    }

    // --- Helper methods to find and store scene objects ---
    private void RegisterAllParticipants()
    {
        participants.Clear();
        foreach (var p in FindObjectsOfType<DialogueParticipant>())
        {
            participants[p.participantID] = p;
        }
    }

    private void RegisterAllCameras()
    {
        cameras.Clear();
        foreach (var c in FindObjectsOfType<DialogueCamera>())
        {
            cameras[c.cameraID] = c;
        }
    }

    private bool IsMonologueActive()
    {
        if (lines.Count > 0)
            return lines.Peek().isMonologueData;
        return false;
    }


}
