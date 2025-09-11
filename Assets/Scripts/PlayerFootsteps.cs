using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    // This allows us to define different surface types and their corresponding sounds in the Inspector.
    [System.Serializable]
    public class SurfaceType
    {
        public string tag;
        public AudioClip[] footstepSounds;
    }

    [Header("Footstep Settings")]
    [Tooltip("The list of surface types and the sounds they should play.")]
    [SerializeField] private List<SurfaceType> surfaceTypes = new List<SurfaceType>();

    [Tooltip("Default footstep sounds to play if the ground tag is not found in the list.")]
    [SerializeField] private AudioClip[] defaultFootstepSounds;

    [Header("Raycast Settings")]
    [Tooltip("How far down to check for the ground from the player's position.")]
    [SerializeField] private float groundCheckDistance = 1.0f;
    [Tooltip("The layer mask for objects that should be considered 'ground'.")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Audio Variation")]
    [Tooltip("The minimum pitch for the footstep sound.")]
    [Range(0.1f, 2f)]
    [SerializeField] private float minPitch = 0.8f;

    [Tooltip("The maximum pitch for the footstep sound.")]
    [Range(0.1f, 2f)]
    [SerializeField] private float maxPitch = 1.2f;

    private PlayerController playerController;

    [Tooltip("A reference to the root player object (the parent with the Rigidbody and Collider).")]
    [SerializeField] private Transform playerRoot;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    // This is the public function that you will call from your Animation Events.
    public void PlayFootstepSound()
    {
        // Perform a raycast straight down from the player's position.
        if (Physics.Raycast(playerRoot.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            // Get the tag of the object we hit.
            string groundTag = hit.collider.tag;

            // Find the surface type that matches the tag.
            SurfaceType currentSurface = surfaceTypes.Find(surface => surface.tag == groundTag);

            if (currentSurface != null && currentSurface.footstepSounds.Length > 0)
            {
                // If we found a matching surface with sounds, pick one at random and play it.
                AudioClip clip = GetRandomClip(currentSurface.footstepSounds);
                AudioManager.instance.PlayClipWithRandomPitch(clip, minPitch, maxPitch);
            }
            else
            {
                // If no specific surface was found, play a default sound.
                AudioClip clip = GetRandomClip(defaultFootstepSounds);
                AudioManager.instance.PlayClipWithRandomPitch(clip, minPitch, maxPitch);
            }
        }
    }

    // A helper function to pick a random audio clip from an array.
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            int randomIndex = Random.Range(0, clips.Length);
            return clips[randomIndex];
        }
        return null;
    }
}
