using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class SneakPeekTrigger : MonoBehaviour
{
    public CinemachineVirtualCamera sneakPeekCamera; // Assign the sneak peek virtual camera in the Inspector

    public float sneakPeekDuration = 3f; // Duration of the sneak peek
    public bool sneakPeeked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !sneakPeeked)
        {
            sneakPeeked=true;
            // Start the sneak peek sequence
            StartCoroutine(SneakPeekSequence());
        }
    }

    private IEnumerator SneakPeekSequence()
    {
        // Activate the sneak peek camera by raising its priority
        sneakPeekCamera.Priority = 20; // Ensure it's higher than the original camera

        yield return new WaitForSeconds(sneakPeekDuration);

        // Revert to the original camera by lowering the sneak peek camera's priority
        sneakPeekCamera.Priority = 0;
    }
}
