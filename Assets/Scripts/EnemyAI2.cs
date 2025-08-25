using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI4 : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack }
    [SerializeField] private Transform[] waypoints;
    
    private int wpIndex = 0;
    private State currentState = State.Patrol;
    
    private Animator anim => GetComponentInChildren<Animator>(); 
    private NavMeshAgent agent => GetComponent<NavMeshAgent>();
    
    // Chase
    private float viewRadius = 10.0f;
    private float viewAngle = 90.0f;
    [SerializeField] private Transform objective;
    private float loseSightTimer = 0.0f; // Tiempo para perder el objetivo si no está a la vista
    private float loseSightTime = 3.0f; // Tiempo para perder el objetivo si no está a la vista
    
    // Attack 
    private float attackRange = 2.0f; // Distancia para iniciar ataque
    private float attackCooldown = 1.0f; // Tiempo entre ataques
    private float lastAttackTime = 0.0f; // Tiempo del último ataque
    
    void Start()
    {
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[wpIndex].position);
        }
        
    }

    void Update()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);
        
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
            default:
                break;
        }
        
    }
    
    private void Attack()
    {
        transform.LookAt(objective);
        
        if (Time.time > lastAttackTime + attackCooldown)
        {
            agent.SetDestination(objective.position);
            anim.SetBool("isAttacking", true);
            lastAttackTime = Time.time;
            // Lógica de daño
        }
        
        float distanceToObjective = Vector3.Distance(transform.position, objective.position);
        
        if (distanceToObjective > attackRange)
        {
            currentState = State.Chase; // Volver a perseguir si el objetivo está fuera de rango
            anim.SetBool("isAttacking", false);
        }
        else if (!LookForObjective())
        {
            currentState = State.Patrol; // Volver a patrullar si el objetivo no está a la vista
            anim.SetBool("isAttacking", false);
        }
        
    }
    
    private void Chase()
    {
        agent.SetDestination(objective.position);
        agent.speed = 5.0f;
        
        float distanceToObjective = Vector3.Distance(transform.position, objective.position);
        if (distanceToObjective <= attackRange)
        {
            currentState = State.Attack;
            anim.SetTrigger("isAttacking");
        }
        else if (distanceToObjective > viewRadius)
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseSightTime)
            {
                currentState = State.Patrol;
                loseSightTimer = 0.0f;
            }
        }
        else
        {
            loseSightTimer = 0.0f; // Resetear el temporizador si el objetivo está a la vista
        }
        
    }
    
    private void Patrol()
    {
        agent.speed = 2.5f;
        if (agent.remainingDistance < 0.5f)
        {
            wpIndex++;
            wpIndex %= waypoints.Length;
            agent.SetDestination(waypoints[wpIndex].position);
        }
        if (LookForObjective())
        {
            currentState = State.Chase;
        }
    }

    private bool LookForObjective()
    {
        if (objective == null) return false;
        Vector3 directionToObjective = (objective.position - transform.position).normalized;
        if (directionToObjective.magnitude > viewRadius) return false;
        
        float angleToObjective = Vector3.Angle(transform.forward, directionToObjective);
        if (angleToObjective > viewAngle/2.0f) return false;
        
        if (Physics.Raycast(transform.position + Vector3.up, directionToObjective, out RaycastHit hit, viewRadius))
        {
            if (hit.transform == objective)
            {
                return true; // Encontramos el objetivo
            }
        }
        return false; // No hay línea de visión clara al objetivo
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = LookForObjective() ? Color.green : Color.red;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2.0f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2.0f, 0) * transform.forward;
        
        Gizmos.DrawRay(transform.position + Vector3.up, leftBoundary * viewRadius);
        Gizmos.DrawRay(transform.position + Vector3.up, rightBoundary * viewRadius);
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(1); // resta 1 vida por golpe
            }
        }
    }

}
