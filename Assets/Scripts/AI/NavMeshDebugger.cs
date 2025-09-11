using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebugger : MonoBehaviour
{
    private NavMeshAgent agent;
    private EnemyAI enemyAI;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();
    }

    void Update()
    {
        if (agent != null)
        {
            Debug.Log($"=== NavMesh Debug Info ===");
            Debug.Log($"IsOnNavMesh: {agent.isOnNavMesh}");
            Debug.Log($"HasPath: {agent.hasPath}");
            Debug.Log($"PathStatus: {agent.pathStatus}");
            Debug.Log($"PathPending: {agent.pathPending}");
            Debug.Log($"RemainingDistance: {agent.remainingDistance}");
            Debug.Log($"IsStopped: {agent.isStopped}");
            Debug.Log($"Velocity: {agent.velocity}");
            Debug.Log($"DesiredVelocity: {agent.desiredVelocity}");
            Debug.Log($"Current State: {enemyAI.currentState}");
            Debug.Log($"============================");
        }
    }
}
