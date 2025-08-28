using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [System.NonSerialized]
    public bool isMonologueData = false;

    [Tooltip("(Optional) A list of ALL participant IDs that should be VISIBLE during this line. Others will be hidden.")]
    public string[] participantsToSetVisible;

    [Tooltip("The ID of the character speaking. Must match a Dialogue Participant in the scene.")]
    public string speakerID;

    [TextArea(3, 10)]
    public string dialogueText;

    [Tooltip("(Optional) The ID of the Cinemachine camera to cut to for this line.")]
    public string cameraShotID;

    [Tooltip("(Optional) The name of the trigger to fire on the speaker's Animator for this line.")]
    public string animationTrigger;

    [Tooltip("(Optional) A sound effect to play when this line is displayed.")]
    public AudioClip lineSFX;

    [Tooltip("How long this line should be displayed in monologue mode before automatically advancing. (in seconds)")]
    public float displayDuration = 3f;
}


