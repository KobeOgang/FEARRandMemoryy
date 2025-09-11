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
    }

    private void Start()
    {
        if (SaveSystem.dataToLoad == null)
        {
            Debug.Log("No save data was loaded. Checking for a spawn point ID...");
            string spawnPointID = SceneLoader.GetAndClearNextSpawnPointID();
            if (!string.IsNullOrEmpty(spawnPointID))
            {
                PlayerSpawnPoint[] spawnPoints = FindObjectsOfType<PlayerSpawnPoint>();
                Debug.Log($"Found {spawnPoints.Length} spawn points in the scene. Looking for ID: {spawnPointID}");

                bool foundSpawn = false;
                foreach (var spawnPoint in spawnPoints)
                {
                    if (spawnPoint.spawnPointID == spawnPointID)
                    {
                        rb.position = spawnPoint.transform.position;
                        rb.velocity = Vector3.zero;
                        transform.rotation = spawnPoint.transform.rotation;
                        Debug.Log("Player spawned at: " + spawnPointID);
                        foundSpawn = true;
                        break;
                    }
                }
                if (!foundSpawn)
                {
                    Debug.LogWarning("Could not find spawn point with ID: " + spawnPointID);
                }
            }
        }
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
            RotatePlayerWithMouse(); 
        }
        else
        {
            SmoothFaceCamera(); 
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
            //animator.SetBool("IsSprinting", true);
        }
        else
        {
            moveSpeed = normalMoveSpeed;
            // --- WALKING ANIMATION  ---
            //animator.SetBool("IsSprinting", false);
        }
    }

    private void MovePlayer()
    {
        bool isInputHeld = (hInput != 0 || vInput != 0);

        if (!isInputHeld)
        {
            preservedRotation = null;
        }

        if (isUsingFixedCamera && useTankControls)
        {
            // Tank Controls
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
            Transform reference = isUsingFixedCamera ? worldReferenceOrientation : this.orientation;
            moveDirection = reference.forward * vInput + reference.right * hInput;
        }

        moveDirection.y = 0;

        if (isGrounded)
        {
            if (Time.frameCount < 5) // Log only for the first few frames
            {
                Debug.Log($"Frame {Time.frameCount}: Applying force with input (h:{hInput}, v:{vInput})");
            }
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        //Handle Locomotion Blend Tree
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
        animator.SetFloat("y", 0f);
        animator.SetFloat("x", 0f);

        animY = 0f;
        animX = 0f;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }
}
