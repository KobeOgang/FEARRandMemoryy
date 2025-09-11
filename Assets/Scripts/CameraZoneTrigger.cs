using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneTrigger : MonoBehaviour
{
    public GameObject fixedCamera; 
    public PlayerController playerController; 
    private Quaternion preservedCharacterRotation;
    private Vector3 lastKnownDirection;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController.PreserveCurrentOrientation();

            fixedCamera.SetActive(true);

            playerController.isUsingFixedCamera = true;

            playerController.worldReferenceOrientation = fixedCamera.transform; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fixedCamera.SetActive(false);

            CameraZoneTrigger[] otherZones = FindObjectsOfType<CameraZoneTrigger>();
            bool playerInAnotherZone = false;

            foreach (var zone in otherZones)
            {
                if (zone != this && zone.GetComponent<Collider>().bounds.Contains(playerController.transform.position))
                {
                    playerInAnotherZone = true;
                    break;
                }
            }

            if (!playerInAnotherZone)
            {
                // Before reverting to top-down, preserve the orientation of the fixed cam we are LEAVING.
                playerController.PreserveCurrentOrientation();

                //playerController.SyncOrientationToPlayerModel();
                // Revert to the top-down camera if no other zones are active
                playerController.isUsingFixedCamera = false;
            }
        }
    }
}
