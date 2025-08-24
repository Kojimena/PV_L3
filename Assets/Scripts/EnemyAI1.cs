using UnityEngine;
using UnityEngine.AI;

public class EnemyAI1 : MonoBehaviour
{
    private enum State { Idle, Patrol }

    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float walkSpeed = 2f;

    private int wpIndex = 0;
    private State currentState = State.Idle;

    private Animator anim;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        agent.isStopped = false;
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[wpIndex].position);
            currentState = State.Patrol;
        }
        else
        {
            Debug.LogWarning("No waypoints assigned to the enemy AI.");
        }
    }

    private void Update()
    {
        // Usa desiredVelocity para animar
        float speed = agent.desiredVelocity.magnitude;
        // Suaviza un poco para evitar parpadeos
        anim.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrol:
                Patrol();
                break;
        }
    }

    private void Idle()
    {
        agent.speed = 0f;
    }

    private void Patrol()
    {
        agent.speed = walkSpeed;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[wpIndex].position);
        }
    }
}