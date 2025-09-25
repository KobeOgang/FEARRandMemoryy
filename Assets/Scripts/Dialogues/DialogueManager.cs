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
    public TMP_Text speakerNameText; 

    [Header("Character References")]
    public PlayerController playerController;

    [Header("Text Effect Settings")]
    [Tooltip("The time in seconds between each character appearing.")]
    public float typingSpeed = 0.02f;
    [Tooltip("The sound to play for each character typed.")]
    public AudioClip typingSFX;
    [Tooltip("The minimum pitch for the typing sound.")] 
    [Range(0.1f, 3f)]                                  
    public float minTypingPitch = 0.9f;                  
    [Tooltip("The maximum pitch for the typing sound.")] 
    [Range(0.1f, 3f)]                                  
    public float maxTypingPitch = 1.1f;

    // --- Private State Variables ---
    private Queue<DialogueLine> lines;
    private Dictionary<string, DialogueParticipant> participants = new Dictionary<string, DialogueParticipant>();
    private Dictionary<string, DialogueCamera> cameras = new Dictionary<string, DialogueCamera>();
    public static bool IsDialogueActive = false;
    public static bool IsNormalDialogueActive = false;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentFullLine;

    private List<string> lastVisibleParticipants = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        lines = new Queue<DialogueLine>();
    }

    void Start()
    {
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
        lastVisibleParticipants.Clear();
        IsDialogueActive = true;

        // Only handle BGM if there's actually custom background music assigned
        if (AudioManager.instance != null && data.customBackgroundMusic != null)
        {
            AudioManager.instance.StartDialogueBGM(data.customBackgroundMusic);
        }

        if (data.hidePlayerDuringDialogue && participants.ContainsKey("Dante"))
        {
            participants["Dante"].gameObject.SetActive(false);
        }

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
        AudioManager.instance.PlayClip(line.lineSFX);
        currentFullLine = line.dialogueText;

        // First, hide only the participants that were visible on the PREVIOUS line.
        foreach (string participantID in lastVisibleParticipants)
        {
            if (participants.ContainsKey(participantID))
            {
                participants[participantID].gameObject.SetActive(false);
            }
        }
        lastVisibleParticipants.Clear(); // Clear the list for the new line

        // Now, show only the participants specified for this current line.
        if (line.participantsToSetVisible != null)
        {
            foreach (string participantID in line.participantsToSetVisible)
            {
                if (participants.ContainsKey(participantID))
                {
                    participants[participantID].gameObject.SetActive(true);
                    lastVisibleParticipants.Add(participantID); // Remember them for the next line
                }
            }
        }

        // Direct the Camera
        foreach (var cam in cameras.Values) cam.SetActive(false);
        if (!string.IsNullOrEmpty(line.cameraShotID) && cameras.ContainsKey(line.cameraShotID))
        {
            cameras[line.cameraShotID].SetActive(true);
        }

        // Set Speaker Name and Trigger Animation
        if (participants.ContainsKey(line.speakerID))
        {
            DialogueParticipant speaker = participants[line.speakerID];
            speakerNameText.text = string.IsNullOrEmpty(speaker.displayName) ? speaker.participantID : speaker.displayName;

            // Now that we are SURE the speaker is visible, we can get their Animator.
            Animator speakerAnimator = speaker.GetComponent<Animator>();
            if (speakerAnimator != null && !string.IsNullOrEmpty(line.animationTrigger))
            {
                // This should now work reliably.
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

            if (typingSFX != null && !char.IsWhiteSpace(c))
            {
                AudioManager.instance.PlayClipWithRandomPitch(typingSFX, minTypingPitch, maxTypingPitch);
            }

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
        if (AudioManager.instance != null && AudioManager.instance.isPlayingDialogueMusic)
        {
            AudioManager.instance.EndDialogueBGM();
        }

        StopAllCoroutines();
        IsDialogueActive = false;
        IsNormalDialogueActive = false;
        dialoguePanel.SetActive(false);

        // Deactivate all dialogue cameras to return to the main gameplay camera
        foreach (var cam in cameras.Values) cam.SetActive(false);

        // Hide only the participants from the very last line of dialogue.
        foreach (string participantID in lastVisibleParticipants)
        {
            if (participants.ContainsKey(participantID))
            {
                participants[participantID].gameObject.SetActive(false);
            }
        }
        lastVisibleParticipants.Clear();

        // Now, find and re-activate the main player character.
        if (participants.ContainsKey("Dante"))
        {
            participants["Dante"].gameObject.SetActive(true);
        }
    }

    // --- Helper methods to find and store scene objects ---
    private void RegisterAllParticipants()
    {
        participants.Clear();
        foreach (var p in FindObjectsOfType<DialogueParticipant>(true))
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
