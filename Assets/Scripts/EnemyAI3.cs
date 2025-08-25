using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI3 : MonoBehaviour
{
    private enum State { Idle, Angry, Rescue }

    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform rescueTarget; 

    [Header("Percepción")]
    [SerializeField] private float viewRadius = 12f;
    [SerializeField] private float viewAngle  = 110f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Comportamiento")]
    [SerializeField] private float loseTargetAfter = 3.0f;
    [SerializeField] private float rotateTowardsPlayerSpeed = 9f;

    private static readonly int HashIsAngry = Animator.StringToHash("IsAngry");
    private static readonly int HashIsWalking = Animator.StringToHash("IsWalking"); 

    private State state = State.Idle;
    private Transform player;
    private float timeSinceLastSight = Mathf.Infinity;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        GameObject playerGO = GameObject.FindGameObjectWithTag(playerTag);
        if (playerGO != null) player = playerGO.transform;

        agent.isStopped = true;
        agent.ResetPath();
        agent.updatePosition = true;   
        agent.updateRotation = false;  

        if (animator != null) animator.applyRootMotion = false;

        SetIsAngry(false);
        SetIsWalking(false);
        state = State.Idle;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                IdleUpdate();
                break;
            case State.Angry:
                AngryUpdate();
                break;
            case State.Rescue:
                RescueUpdate();
                break;
        }
    }

    private void IdleUpdate()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (PlayerInSight())
        {
            timeSinceLastSight = 0f;
            EnterAngry();
        }
    }

    private void AngryUpdate()
    {
        if (player == null) { EnterIdle(); return; }

        FaceTargetHorizontally(player.position, rotateTowardsPlayerSpeed);

        bool canSee = PlayerInSight();
        timeSinceLastSight = canSee ? 0f : timeSinceLastSight + Time.deltaTime;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (timeSinceLastSight >= loseTargetAfter)
        {
            EnterRescue();
        }
    }

    private void RescueUpdate()
    {
        if (rescueTarget == null) { EnterIdle(); return; }

        agent.isStopped = false;
        agent.SetDestination(rescueTarget.position);
        SetIsWalking(true);

        // cuando llega a destino
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;
            SetIsWalking(false);
            // evento de "rescatar"
            agent.updateRotation = false;
        }
    }

    private void EnterAngry()
    {
        state = State.Angry;
        SetIsAngry(true);
        SetIsWalking(false);
    }

    private void EnterIdle()
    {
        state = State.Idle;
        SetIsAngry(false);
        SetIsWalking(false);
    }

    private void EnterRescue()
    {
        state = State.Rescue;
        SetIsAngry(false);
        SetIsWalking(true);
        
        agent.updateRotation = true;
    }

    private bool PlayerInSight()
    {
        if (player == null) return false;

        Vector3 toTarget = player.position - transform.position;
        float distance = toTarget.magnitude;
        if (distance > viewRadius) return false;

        Vector3 toTargetDir = toTarget.normalized;
        float angleToTarget = Vector3.Angle(transform.forward, toTargetDir);
        if (angleToTarget > viewAngle * 0.5f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.7f;
        if (Physics.Raycast(eyePos, toTargetDir, distance, obstacleMask))
            return false;

        return true;
    }

    private void FaceTargetHorizontally(Vector3 targetPos, float turnSpeed)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
    }

    private void SetIsAngry(bool value)
    {
        if (animator != null) animator.SetBool(HashIsAngry, value);
    }

    private void SetIsWalking(bool value)
    {
        if (animator != null) animator.SetBool(HashIsWalking, value);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
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
    }
}