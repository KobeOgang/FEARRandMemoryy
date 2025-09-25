using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRagdollDeath : MonoBehaviour
{
    [Header("Ragdoll Components")]
    [Tooltip("The skeleton root (should be 'treehead_th_tree head_skeleton')")]
    public Transform skeletonRoot;

    [Header("Death Settings")]
    [Tooltip("Force applied on death (simulates impact)")]
    public float deathForce = 500f;
    [Tooltip("How long ragdoll stays before cleanup")]
    public float ragdollLifetime = 10f;
    [Tooltip("Fade out duration before destruction")]
    public float fadeOutDuration = 2f;

    [Header("Audio")]
    [Tooltip("Sound to play when enemy dies")]
    public AudioClip deathSound;

    // Component references
    private Animator animator;
    private NavMeshAgent navAgent;
    private Rigidbody mainRigidbody;
    private Collider mainCollider;
    private EnemyAI enemyAI;

    // Ragdoll components
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;

    private bool isDead = false;

    private void Start()
    {
        // Get components - use GetComponentInChildren for Animator since it's on a child object
        animator = GetComponentInChildren<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        mainRigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
        enemyAI = GetComponent<EnemyAI>();

        // Auto-find skeleton if not assigned
        if (skeletonRoot == null)
        {
            skeletonRoot = FindDeepChild(transform, "treehead_th_tree head_skeleton");
            if (skeletonRoot == null)
            {
                Debug.LogError($"EnemyRagdollDeath: Cannot find skeleton root in {gameObject.name}");
                return;
            }
        }

        // Get all ragdoll components from skeleton
        ragdollRigidbodies = skeletonRoot.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = skeletonRoot.GetComponentsInChildren<Collider>();

        // Start with ragdoll disabled
        SetRagdollEnabled(false);

        // Debug info
        Debug.Log($"EnemyRagdollDeath: Found {ragdollRigidbodies.Length} ragdoll rigidbodies");
        Debug.Log($"EnemyRagdollDeath: Animator found: {animator != null}");
        if (animator != null)
        {
            Debug.Log($"EnemyRagdollDeath: Animator is on: {animator.gameObject.name}");
        }
    }

    public void TriggerDeath(Vector3 impactPoint = default, Vector3 impactForce = default)
    {
        if (isDead) return;

        isDead = true;

        // Disable AI and movement
        if (enemyAI != null) enemyAI.enabled = false;
        if (navAgent != null) navAgent.enabled = false;
        if (animator != null) animator.enabled = false;

        // Enable ragdoll
        SetRagdollEnabled(true);

        // Apply death force
        ApplyDeathForce(impactPoint, impactForce);

        // Play death sound
        PlayDeathSound();

        // Start cleanup coroutine
        StartCoroutine(CleanupRagdoll());

        Debug.Log($"Enemy {gameObject.name} died with ragdoll physics");
    }

    private void SetRagdollEnabled(bool enabled)
    {
        // Toggle main character physics
        if (mainRigidbody != null)
            mainRigidbody.isKinematic = enabled;
        if (mainCollider != null)
            mainCollider.enabled = !enabled;

        // Toggle ragdoll physics
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
        }

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = enabled;
        }
    }

    private void ApplyDeathForce(Vector3 impactPoint, Vector3 impactForce)
    {
        if (ragdollRigidbodies.Length == 0) return;

        // If no specific impact provided, apply random force
        if (impactForce == Vector3.zero)
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
            impactForce = randomDirection * deathForce;
        }

        // Apply force to spine (main body) for realistic death
        Transform spineTransform = FindDeepChild(skeletonRoot, "spine");
        if (spineTransform != null)
        {
            Rigidbody spineRb = spineTransform.GetComponent<Rigidbody>();
            if (spineRb != null)
            {
                spineRb.AddForce(impactForce, ForceMode.Impulse);

                // Add slight torque for more dynamic death
                Vector3 torque = new Vector3(
                    Random.Range(-50f, 50f),
                    Random.Range(-50f, 50f),
                    Random.Range(-50f, 50f)
                );
                spineRb.AddTorque(torque, ForceMode.Impulse);
            }
        }
    }

    private void PlayDeathSound()
    {
        if (deathSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlayClip(deathSound);
        }
    }

    private IEnumerator CleanupRagdoll()
    {
        // Wait for ragdoll lifetime
        yield return new WaitForSeconds(ragdollLifetime);

        // Optional: Fade out effect here
        yield return StartCoroutine(FadeOutRagdoll());

        // Destroy the enemy
        Destroy(gameObject);
    }

    private IEnumerator FadeOutRagdoll()
    {
        // Get all renderers for fade effect
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        float fadeTimer = 0f;
        List<Material> originalMaterials = new List<Material>();

        // Store original materials
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                originalMaterials.Add(mat);
            }
        }

        // Fade out
        while (fadeTimer < fadeOutDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutDuration);

            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = alpha;
                        mat.color = color;
                    }
                }
            }

            yield return null;
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
