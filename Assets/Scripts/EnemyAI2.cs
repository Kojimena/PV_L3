using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI2 : MonoBehaviour
{
    private enum State { Patrol, Chase }

    [Header("Refs")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private string playerTag = "Player";

    [Header("Percepción")]
    [SerializeField] private float viewRadius = 12f;        // radio de detección
    [SerializeField] private float viewAngle = 110f;        // ángulo de detección
    [SerializeField] private LayerMask obstacleMask;        // capas que bloquean la vista

    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed = 2.2f;
    [SerializeField] private float chaseSpeed = 3.8f;
    [SerializeField] private float waypointTolerance = 0.6f; 
    [SerializeField] private float waitAtWaypoint = 0.5f;    

    [Header("Persecución")]
    [SerializeField] private float loseTargetAfter = 3.0f;   // intervalo para volver a patrullar

    private State state = State.Patrol;
    private int currentWaypoint = 0;
    private float waitTimer = 0f;

    private Transform player;
    private float timeSinceLastSight = Mathf.Infinity;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        GameObject playerGO = GameObject.FindGameObjectWithTag(playerTag);
        if (playerGO != null) player = playerGO.transform;

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"{name}: No hay waypoints asignados.");
        }

        GoToNextWaypoint();
        agent.speed = patrolSpeed;
        state = State.Patrol;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Chase:
                ChaseUpdate();
                break;
        }
    }

    private void PatrolUpdate()
    {
        agent.stoppingDistance = 0f;
        agent.speed = patrolSpeed;

        if (waypoints != null && waypoints.Length > 0)
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitAtWaypoint)
                {
                    waitTimer = 0f;
                    GoToNextWaypoint();
                }
            }
        }

        if (PlayerInSight())
        {
            state = State.Chase;
            agent.speed = chaseSpeed;
            timeSinceLastSight = 0f;
        }
    }

    private void ChaseUpdate()
    {
        if (player == null)
        {
            state = State.Patrol;
            return;
        }

        agent.stoppingDistance = 0f;
        agent.speed = chaseSpeed;

        // Perseguir jugador siempre
        agent.SetDestination(player.position);

        bool canSee = PlayerInSight();
        if (canSee) 
            timeSinceLastSight = 0f; 
        else 
            timeSinceLastSight += Time.deltaTime;

        if (timeSinceLastSight >= loseTargetAfter)
        {
            state = State.Patrol;
            GoToNextWaypoint();
        }
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        if (waypoints[currentWaypoint] != null)
        {
            agent.stoppingDistance = 0f;
            agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    private bool PlayerInSight()
    {
        if (player == null) return false;

        Vector3 toTarget = (player.position - transform.position);
        float distance = toTarget.magnitude;
        if (distance > viewRadius) return false;

        Vector3 toTargetDir = toTarget.normalized;
        float angleToTarget = Vector3.Angle(transform.forward, toTargetDir);
        if (angleToTarget > viewAngle * 0.5f) return false;

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2f, true);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2f, true);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        if (waypoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }
}
