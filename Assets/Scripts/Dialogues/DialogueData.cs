using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Game/Dialogue")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Check this if the dialogue is an inner monologue. Player will not be frozen and lines will advance automatically.")]
    public bool isMonologue;

    [Tooltip("Check this to make the main player character invisible during this dialogue sequence.")]
    public bool hidePlayerDuringDialogue;

    [Header("Audio")]
    [Tooltip("(Optional) Custom background music to play during this dialogue sequence")]
    public AudioClip customBackgroundMusic;

    public DialogueLine[] conversationLines;

}
