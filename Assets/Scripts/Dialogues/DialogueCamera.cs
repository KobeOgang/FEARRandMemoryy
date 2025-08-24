using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class DialogueCamera : MonoBehaviour
{
    [Tooltip("The unique ID for this camera shot (e.g., 'Player_Closeup', 'NPC_Wide').")]
    public string cameraID;

    private CinemachineVirtualCamera vcam;

    private void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    public void SetActive(bool isActive)
    {
        // We control cameras by setting their priority. High priority is active.
        vcam.Priority = isActive ? 100 : 0;
    }
}
