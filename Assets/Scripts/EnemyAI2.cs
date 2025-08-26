using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI2 : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack, Taunt }

    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform objective;

    private int wpIndex = 0;
    private State currentState = State.Patrol;

    private Animator anim => GetComponentInChildren<Animator>();
    private NavMeshAgent agent => GetComponent<NavMeshAgent>();

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hitSfx;

    [SerializeField] private float viewRadius = 10f;
    [SerializeField] private float viewAngle = 90f;
    private float loseSightTimer = 0f;
    private float loseSightTime = 3f;

    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime = 0f;

    [SerializeField] private float tauntDuration = 1.2f;
    private float tauntTimer = 0f;

    void Start()
    {
        if (waypoints.Length > 0) agent.SetDestination(waypoints[wpIndex].position);
    }

    void Update()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);

        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase:  Chase();  break;
            case State.Attack: Attack(); break;
            case State.Taunt:  Taunt();  break;
        }
    }

    private void Taunt()
    {
        if (objective) transform.LookAt(objective);
        agent.isStopped = true;
        anim.SetBool("isTaunting", true);
        tauntTimer += Time.deltaTime;
        if (tauntTimer >= tauntDuration)
        {
            anim.SetBool("isTaunting", false);
            agent.isStopped = false;
            currentState = State.Chase;
        }
    }

    private void Attack()
    {
        if (objective) transform.LookAt(objective);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            if (objective) agent.SetDestination(objective.position);
            anim.SetBool("isAttacking", true);
            lastAttackTime = Time.time;
        }

        float d = objective ? Vector3.Distance(transform.position, objective.position) : Mathf.Infinity;

        if (d > attackRange)
        {
            currentState = State.Chase;
            anim.SetBool("isAttacking", false);
        }
        else if (!LookForObjective())
        {
            currentState = State.Patrol;
            anim.SetBool("isAttacking", false);
        }
    }

    private void Chase()
    {
        if (objective) agent.SetDestination(objective.position);
        agent.speed = 5f;

        float d = objective ? Vector3.Distance(transform.position, objective.position) : Mathf.Infinity;

        if (d <= attackRange)
        {
            currentState = State.Attack;
            anim.SetTrigger("isAttacking");
        }
        else if (d > viewRadius)
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseSightTime)
            {
                currentState = State.Patrol;
                loseSightTimer = 0f;
            }
        }
        else
        {
            loseSightTimer = 0f;
        }
    }

    private void Patrol()
    {
        agent.speed = 2.5f;

        if (agent.remainingDistance < 0.5f && waypoints.Length > 0)
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[wpIndex].position);
        }

        if (LookForObjective())
        {
            tauntTimer = 0f;
            currentState = State.Taunt;
            anim.SetBool("isAttacking", false);
        }
    }

    private bool LookForObjective()
    {
        if (!objective) return false;
        Vector3 to = objective.position - transform.position;
        float dist = to.magnitude;
        if (dist > viewRadius) return false;
        Vector3 dir = to.normalized;
        float ang = Vector3.Angle(transform.forward, dir);
        if (ang > viewAngle * 0.5f) return false;
        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, viewRadius))
            return hit.transform == objective || hit.transform.IsChildOf(objective);
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = LookForObjective() ? Color.green : Color.red;
        Vector3 L = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 R = Quaternion.Euler(0,  viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up, L * viewRadius);
        Gizmos.DrawRay(transform.position + Vector3.up, R * viewRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(1);
                if (sfxSource && hitSfx) sfxSource.PlayOneShot(hitSfx);
            }
        }
    }
}
