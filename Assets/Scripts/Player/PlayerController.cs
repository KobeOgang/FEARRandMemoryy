using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float normalMoveSpeed;
    public float sprintSpeed = 10f;
    public bool useTankControls = false;

    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("GroundCheck")]
    public float playerHeight;
    public LayerMask WhatIsGround;
    bool isGrounded;

    [Header("Animation Smoothing")]
    [Tooltip("How quickly the animation blends. Smaller values are faster.")]
    public float animationSmoothTime = 0.1f;
    private float animX = 0f;
    private float animY = 0f;


    public Transform playerModel; 
    public Transform orientation; 
    public Transform worldReferenceOrientation; 

    public float rotationSpeed = 5f; 
    public bool isUsingFixedCamera = false; 

    float hInput;
    float vInput;

    Vector3 moveDirection;

    Rigidbody rb;
    private Animator animator;

    private Quaternion? preservedRotation = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        animator = GetComponentInChildren<Animator>();

        // --- LOGIC TO APPLY LOADED DATA ---
        // Check if the SaveSystem has data waiting to be loaded
        if (SaveSystem.dataToLoad != null)
        {
            Debug.Log("--- STARTING DATA LOAD PROCESS ---");

            Debug.Log("1. Applying Inventory data...");
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ApplyLoadedData(SaveSystem.dataToLoad);
                Debug.Log("...Inventory data APPLIED successfully.");
            }
            else { Debug.LogError("InventoryManager.Instance is NULL!"); }

            Debug.Log("2. Applying Codex data...");
            if (CodexManager.Instance != null)
            {
                CodexManager.Instance.ApplyLoadedData(SaveSystem.dataToLoad);
                Debug.Log("...Codex data APPLIED successfully.");
            }
            else { Debug.LogError("CodexManager.Instance is NULL!"); }

            Debug.Log("3. Applying World State data...");
            if (WorldStateManager.Instance != null)
            {
                WorldStateManager.Instance.ApplyLoadedData(SaveSystem.dataToLoad);
                Debug.Log("...World State data APPLIED successfully.");
            }
            else { Debug.LogError("WorldStateManager.Instance is NULL!"); }

            Debug.Log("4. Applying Player Position...");
            float[] pos = SaveSystem.dataToLoad.playerPosition;
            rb.position = new Vector3(pos[0], pos[1], pos[2]);
            Debug.Log("...Player Position APPLIED. New position: " + rb.position);

            Debug.Log("5. Clearing dataToLoad...");
            SaveSystem.dataToLoad = null;
            Debug.Log("--- DATA LOAD COMPLETE ---");
        }
        else
        {
            Debug.Log("No save data to load, using SceneLoader for spawn point.");
            // Your existing spawn point logic for normal scene transitions
            string spawnPointID = SceneLoader.GetAndClearNextSpawnPointID();
            if (!string.IsNullOrEmpty(spawnPointID))
            {
                PlayerSpawnPoint[] spawnPoints = FindObjectsOfType<PlayerSpawnPoint>();
                foreach (var spawnPoint in spawnPoints)
                {
                    if (spawnPoint.spawnPointID == spawnPointID)
                    {
                        transform.position = spawnPoint.transform.position;
                        transform.rotation = spawnPoint.transform.rotation;
                        break;
                    }
                }
            }
        }
    }

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (InspectionManager.IsInspecting || DialogueManager.IsNormalDialogueActive)
        {
            rb.velocity = Vector3.zero;
            return; 
        }

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, WhatIsGround);
        PlayerInput();
        SpeedControl();

        // Handle ground drag
        rb.drag = isGrounded ? groundDrag : 0f;

        // Rotate player based on active camera mode
        if (isUsingFixedCamera)
        {
            RotatePlayerWithMouse(); // Mouse-based rotation for fixed cameras
        }
        else
        {
            SmoothFaceCamera(); // Smooth rotation relative to the top-down camera
        }

        MovePlayer();
    }


    private void PlayerInput()
    {
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKey(sprintKey) && isGrounded)
        {
            moveSpeed = sprintSpeed;
            // --- RUNNING ANIMATION  ---
            // Tell the animator that the character IS sprinting.
            //animator.SetBool("IsSprinting", true);
        }
        else
        {
            moveSpeed = normalMoveSpeed;
            // --- WALKING ANIMATION  ---
            // Tell the animator that the character IS NOT sprinting.
            //animator.SetBool("IsSprinting", false);
        }
    }

    private void MovePlayer()
    {
        bool isInputHeld = (hInput != 0 || vInput != 0);

        // If the player lets go of the keys, clear the preserved rotation memory.
        if (!isInputHeld)
        {
            preservedRotation = null;
        }

        if (isUsingFixedCamera && useTankControls)
        {
            // Tank Controls: Movement is relative to the player model's orientation
            moveDirection = playerModel.forward * vInput + playerModel.right * hInput;
        }
        else if (preservedRotation.HasValue && isInputHeld)
        {
            Vector3 forward = preservedRotation.Value * Vector3.forward;
            Vector3 right = preservedRotation.Value * Vector3.right;
            moveDirection = forward * vInput + right * hInput;
        }
        else
        {
            // If we have no preserved rotation, use the live camera as normal.
            Transform reference = isUsingFixedCamera ? worldReferenceOrientation : this.orientation;
            moveDirection = reference.forward * vInput + reference.right * hInput;
        }

        moveDirection.y = 0;

        if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // --- NEW ANIMATION LOGIC ---
        // Part A: Handle Locomotion Blend Tree
        float intensity = Input.GetKey(sprintKey) ? 1.0f : 0.5f;
        if (hInput == 0 && vInput == 0)
        {
            intensity = 0f;
        }
        Vector3 localMoveDirection = playerModel.transform.InverseTransformDirection(moveDirection.normalized);
        float targetY = localMoveDirection.z * intensity;
        float targetX = localMoveDirection.x * intensity;
        animY = Mathf.Lerp(animY, targetY, Time.deltaTime / animationSmoothTime);
        animX = Mathf.Lerp(animX, targetX, Time.deltaTime / animationSmoothTime);
        animator.SetFloat("y", animY);
        animator.SetFloat("x", animX);

        // Part B: Handle Turning-In-Place Triggers
        // This only runs when the player is idle.
        if (intensity == 0f)
        {
            float angularSpeed = rb.angularVelocity.y;
            float turnThreshold = 0.2f; 

            if (angularSpeed > turnThreshold)
            {
                animator.SetTrigger("TurnRight");
            }
            else if (angularSpeed < -turnThreshold)
            {
                animator.SetTrigger("TurnLeft");
            }
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    public void PreserveCurrentOrientation()
    {
        if (preservedRotation.HasValue)
        {
            return;
        }

        if (isUsingFixedCamera)
        {
            // Save the fixed camera's rotation VALUE
            preservedRotation = worldReferenceOrientation.rotation;
        }
        else
        {
            // Save the top-down camera's rotation VALUE
            preservedRotation = this.orientation.rotation;
        }
    }

    private void SmoothFaceCamera()
    {
        if (!isUsingFixedCamera)
        {
            // Smooth rotation to face the active camera's orientation
            float targetAngle = orientation.eulerAngles.y;
            float smoothedAngle = Mathf.LerpAngle(playerModel.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
            playerModel.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
        }
    }

    private void RotatePlayerWithMouse()
    {
        // Allow player rotation with mouse input
        float mouseX = Input.GetAxis("Mouse X");
        playerModel.Rotate(Vector3.up, mouseX * rotationSpeed);

    }
    public void SyncOrientationToPlayerModel()
    {
        orientation.rotation = playerModel.rotation;
    }

    public void ForceIdle()
    {
        // Reset the animator's parameters to idle values
        animator.SetFloat("y", 0f);
        animator.SetFloat("x", 0f);

        // Also reset the internal smoothing variables to prevent a sudden jump when dialogue ends
        animY = 0f;
        animX = 0f;

        // Instantly stop any physical movement as well
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }
}
