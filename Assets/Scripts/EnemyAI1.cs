using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI1 : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack }
    [SerializeField] private Transform[] waypoints;

    [Header("OffMeshLink / Jump")]
    [SerializeField] private string jumpTriggerName = "Jump"; // trigger de la animación
    [SerializeField] private string landTriggerName = "";      
    [SerializeField] private float jumpDuration = 0.6f;        // segundos
    [SerializeField] private float jumpHeight   = 1.2f;        // altura de la parábola
    [SerializeField] private bool useParabolaMovement = true;  
    
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hitSfx;


    private int wpIndex = 0;
    private State currentState = State.Patrol;

    private Animator anim => GetComponentInChildren<Animator>();
    private NavMeshAgent agent => GetComponent<NavMeshAgent>();

    // Chase
    [SerializeField] private float viewRadius = 10.0f;
    [SerializeField] private float viewAngle = 90.0f;
    [SerializeField] private Transform objective;
    private float loseSightTimer = 0.0f;
    [SerializeField] private float loseSightTime = 3.0f;

    // Attack 
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackCooldown = 1.0f;
    private float lastAttackTime = 0.0f;

    // Animator hashes
    private int HashSpeed;
    private int HashJump;
    private int HashLand;

    private bool isJumping;

    void Start()
    {
        HashSpeed = Animator.StringToHash("Speed");
        HashJump  = Animator.StringToHash(jumpTriggerName);
        HashLand  = string.IsNullOrEmpty(landTriggerName) ? 0 : Animator.StringToHash(landTriggerName);

        agent.autoTraverseOffMeshLink = false;

        if (waypoints != null && waypoints.Length > 0)
            agent.SetDestination(waypoints[wpIndex].position);
    }

    void Update()
    {
        if (anim) anim.SetFloat(HashSpeed, agent.velocity.magnitude);

        // link para salto
        if (!isJumping && agent.isOnOffMeshLink)
            StartCoroutine(TraverseOffMeshLinkWithJump());

        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase:  Chase();  break;
            case State.Attack: Attack(); break;
        }
    }

    private void Attack()
    {
        if (!objective) return;
        transform.LookAt(objective);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            agent.SetDestination(objective.position);
            anim.SetBool("isAttacking", true);
            lastAttackTime = Time.time;
        }

        float d = Vector3.Distance(transform.position, objective.position);
        if (d > attackRange) { currentState = State.Chase; anim.SetBool("isAttacking", false); }
        else if (!LookForObjective()) { currentState = State.Patrol; anim.SetBool("isAttacking", false); }
    }

    private void Chase()
    {
        if (!objective) { currentState = State.Patrol; return; }

        agent.speed = 5.0f;
        agent.SetDestination(objective.position);

        float d = Vector3.Distance(transform.position, objective.position);
        if (d <= attackRange)
        {
            currentState = State.Attack;
            anim.SetTrigger("isAttacking");
        }
        else if (d > viewRadius)
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseSightTime) { currentState = State.Patrol; loseSightTimer = 0.0f; }
        }
        else loseSightTimer = 0.0f;
    }

    private void Patrol()
    {
        agent.speed = 2.5f;
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !agent.isOnOffMeshLink)
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[wpIndex].position);
        }
        if (LookForObjective()) currentState = State.Chase;
    }

    private bool LookForObjective()
    {
        if (!objective) return false;
        Vector3 to = (objective.position - transform.position);
        if (to.magnitude > viewRadius) return false;

        float angle = Vector3.Angle(transform.forward, to.normalized);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, to.normalized, out RaycastHit hit, viewRadius))
            return hit.transform == objective;
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = LookForObjective() ? Color.green : Color.red;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0,  viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up, leftBoundary  * viewRadius);
        Gizmos.DrawRay(transform.position + Vector3.up, rightBoundary * viewRadius);
    }
    

    private IEnumerator TraverseOffMeshLinkWithJump()
    {
        isJumping = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 start = data.startPos;
        Vector3 end   = data.endPos;

        FaceHorizontally(end, 18f);

        if (HashJump != 0) anim.SetTrigger(HashJump);

        if (!useParabolaMovement)
        {
            agent.updatePosition = false;

            float timer = 0f;
            while (timer < jumpDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            agent.updatePosition = true;
        }
        else
        {
            agent.updatePosition = false;

            float t = 0f;
            Vector3 origin = start;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.001f, jumpDuration);
                Vector3 pos = Parabola(origin, end, jumpHeight, Mathf.Clamp01(t));
                transform.position = pos;

                Vector3 dir = (end - transform.position); dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 12f);

                yield return null;
            }

            agent.updatePosition = true;
        }

        if (HashLand != 0) anim.SetTrigger(HashLand);

        agent.CompleteOffMeshLink();
        isJumping = false;
    }

    private Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Vector3 m = Vector3.Lerp(start, end, t);
        float y = -4f * height * (t - 0.5f) * (t - 0.5f) + height; // pico en t=0.5
        m.y += y;
        return m;
    }

    private void FaceHorizontally(Vector3 target, float turnSpeed)
    {
        Vector3 dir = target - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(1); // resta 1 vida por golpe
                if (sfxSource != null && hitSfx != null) sfxSource.PlayOneShot(hitSfx);
            }
        }
    }
}
