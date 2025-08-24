using UnityEngine;
using UnityEngine.AI;

public class EnemyAI1 : MonoBehaviour
{
    private enum State { Idle, Patrol }

    [Header("Patrulla")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float walkSpeed = 2f;

    [Header("Salto en OffMeshLink")]
    [SerializeField] private string jumpTriggerName = "Jump"; // Trigger del Animator
    [SerializeField] private float jumpDuration = 0.6f;        // Segundos que dura el salto
    [SerializeField] private float jumpHeight = 1.2f;          // Altura máxima de la parábola

    private int wpIndex = 0;
    private State currentState = State.Idle;

    private Animator anim;
    private NavMeshAgent agent;
    private int HashSpeed;
    private int HashJump;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponentInChildren<Animator>();
        HashSpeed = Animator.StringToHash("Speed");
        HashJump  = Animator.StringToHash(jumpTriggerName);
    }

    private void Start()
    {
        agent.autoTraverseOffMeshLink = false;

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

        StartCoroutine(HandleOffMeshLinks());
    }

    private void Update()
    {
        float speed = agent.desiredVelocity.magnitude;
        anim.SetFloat(HashSpeed, speed, 0.1f, Time.deltaTime);

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
    
    private System.Collections.IEnumerator HandleOffMeshLinks()
    {
        while (true)
        {
            while (!agent.isOnOffMeshLink)
                yield return null;

            OffMeshLinkData link = agent.currentOffMeshLinkData;
            Vector3 startPos = agent.transform.position;
            Vector3 endPos = link.endPos;
            endPos.y = startPos.y; 

            if (anim != null && HashJump != 0)
                anim.SetTrigger(HashJump);
            
            {
                float t = 0f;
                Vector3 horizontalStart = startPos;
                Vector3 horizontalEnd   = endPos;

                while (t < 1f)
                {
                    t += Time.deltaTime / Mathf.Max(0.01f, jumpDuration);
                    Vector3 pos = Vector3.Lerp(horizontalStart, horizontalEnd, t);
                    // Arco parabólico
                    float heightOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                    pos.y = startPos.y + heightOffset;

                    Vector3 dir = (horizontalEnd - horizontalStart);
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);

                    agent.Warp(pos);
                    yield return null;
                }

                agent.Warp(endPos);
            }

            agent.CompleteOffMeshLink();

            yield return null;
        }
    }
}
