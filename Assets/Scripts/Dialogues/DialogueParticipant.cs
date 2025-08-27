using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueParticipant : MonoBehaviour
{
    [Tooltip("The unique ID for this character (e.g., 'Player', 'NPC_Guard_1').")]
    public string participantID;

    [Tooltip("(Optional) The name to display in the UI. If empty, the Participant ID will be used.")]
    public string displayName;
}
