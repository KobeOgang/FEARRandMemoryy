using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    [SerializeField] private Transform player;
    public Transform orientation;

    [Header("Movement Speeds")]
    public float walkSpeed = 3f;
    public float chaseSpeed = 6f;

    [Header("Waypoint System")]
    [Tooltip("List of waypoints for patrolling. Enemy will move between these points.")]
    public List<Transform> waypoints = new List<Transform>();
    [Tooltip("Time to wait at each waypoint before moving to the next one.")]
    public float waitTimeAtWaypoint = 2f;
    [Tooltip("Should the enemy patrol waypoints in order or randomly?")]
    public bool patrolInOrder = true;

    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaitingAtWaypoint = false;

    public LayerMask whatIsGround, whatIsPlayer;

    // States
    public float sightRange = 10f;
    public bool playerInSightRange;

    // Search behavior
    public float searchTime = 5f;
    private float currentSearchTimer;
    private Vector3 lastKnownPosition;

    public enum EnemyState
    {
        Patrolling,
        Chasing,
        SearchingLastKnown,
        Idle
    }

    public EnemyState currentState = EnemyState.Patrolling;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.speed = walkSpeed;

        // Validate waypoints
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints assigned to " + gameObject.name + ". Enemy will stay idle.");
            currentState = EnemyState.Idle;
        }
        else
        {
            // Start patrolling from the first waypoint
            currentWaypointIndex = 0;
            MoveToCurrentWaypoint();
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrolling();
                break;

            case EnemyState.Chasing:
                ChasePlayer();
                break;

            case EnemyState.SearchingLastKnown:
                SearchLastKnownPosition();
                break;

            case EnemyState.Idle:
                // Idle logic - enemy stays in place
                break;
        }
    }

    private void Patrolling()
    {
        agent.speed = walkSpeed;

        // If we have no waypoints, go idle
        if (waypoints.Count == 0)
        {
            currentState = EnemyState.Idle;
            return;
        }

        // Check if we're waiting at a waypoint
        if (isWaitingAtWaypoint)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                // Finished waiting, move to next waypoint
                isWaitingAtWaypoint = false;
                waitTimer = 0f;
                SelectNextWaypoint();
                MoveToCurrentWaypoint();
            }
        }
        else
        {
            // Check if we've reached our current waypoint
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // Arrived at waypoint, start waiting
                isWaitingAtWaypoint = true;
                waitTimer = 0f;
            }
        }

        // Check for the player while patrolling
        CheckForPlayer();
    }

    private void SelectNextWaypoint()
    {
        if (waypoints.Count == 0) return;

        if (patrolInOrder)
        {
            // Move to next waypoint in sequence
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
        else
        {
            // Select random waypoint (but not the current one if we have multiple)
            if (waypoints.Count > 1)
            {
                int newIndex;
                do
                {
                    newIndex = Random.Range(0, waypoints.Count);
                } while (newIndex == currentWaypointIndex);
                currentWaypointIndex = newIndex;
            }
        }
    }

    private void MoveToCurrentWaypoint()
    {
        if (waypoints.Count > 0 && currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void ChasePlayer()
    {
        agent.speed = chaseSpeed;

        // Check if we can still see the player
        bool canSeePlayer = false;
        if (Physics.CheckSphere(transform.position, sightRange, whatIsPlayer))
        {
            Vector3 directionToPlayer = player.position - orientation.position;
            if (Physics.Raycast(orientation.position, directionToPlayer, out RaycastHit hit, sightRange))
            {
                if (hit.transform == player)
                {
                    canSeePlayer = true;
                }
            }
        }

        if (canSeePlayer)
        {
            // Chase the player and update last known position
            agent.SetDestination(player.position);
            lastKnownPosition = player.position;
        }
        else
        {
            // Lost sight, switch to searching
            currentState = EnemyState.SearchingLastKnown;
        }
    }

    private void SearchLastKnownPosition()
    {
        agent.speed = walkSpeed;
        agent.SetDestination(lastKnownPosition);

        // If we reach the last known position, return to patrolling
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            ReturnToPatrolling();
        }

        // While searching, if we spot the player again, immediately go back to chasing
        CheckForPlayer();
    }

    private void ReturnToPatrolling()
    {
        currentState = EnemyState.Patrolling;
        isWaitingAtWaypoint = false;
        waitTimer = 0f;

        // Find the closest waypoint to resume patrolling
        FindClosestWaypoint();
        MoveToCurrentWaypoint();
    }

    private void FindClosestWaypoint()
    {
        if (waypoints.Count == 0) return;

        float closestDistance = float.MaxValue;
        int closestWaypointIndex = 0;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                float distance = Vector3.Distance(transform.position, waypoints[i].position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWaypointIndex = i;
                }
            }
        }

        currentWaypointIndex = closestWaypointIndex;
    }

    private void CheckForPlayer()
    {
        // This method is used to INITIATE a chase from a non-chasing state
        if (Physics.CheckSphere(transform.position, sightRange, whatIsPlayer))
        {
            Vector3 directionToPlayer = player.position - orientation.position;
            if (Physics.Raycast(orientation.position, directionToPlayer, out RaycastHit hit, sightRange))
            {
                if (hit.transform == player)
                {
                    currentState = EnemyState.Chasing;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draw waypoints and connections
        if (waypoints != null && waypoints.Count > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    // Draw waypoint
                    Gizmos.DrawWireSphere(waypoints[i].position, 1f);

                    // Draw path lines
                    if (patrolInOrder)
                    {
                        // Draw lines between sequential waypoints
                        int nextIndex = (i + 1) % waypoints.Count;
                        if (waypoints[nextIndex] != null)
                        {
                            Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                        }
                    }
                }
            }

            // Highlight current waypoint
            if (currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].position, 1.2f);
            }
        }
    }
}
