using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI3 : MonoBehaviour
{
    private enum State { Angry, Rescue }

    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform rescueTarget;
    [SerializeField] private Transform objective;
    [SerializeField] private float viewRadius = 10f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float angryDuration = 3.5f;
    

    private int wpIndex = 0;
    private State currentState = State.Angry;
    private float angryTimer = 0f;

    private Animator anim => GetComponentInChildren<Animator>();
    private NavMeshAgent agent => GetComponent<NavMeshAgent>();

    void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
            agent.SetDestination(waypoints[wpIndex].position);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Angry:
                Angry();
                break;
            case State.Rescue:
                Rescue();
                break;
        }
    }

    private void Angry()
    {
        bool sees = LookForObjective();
        if (sees)
        {
            transform.LookAt(objective);
            anim.SetBool("IsAngry", true);
            anim.SetBool("IsWalking", false);
            angryTimer += Time.deltaTime;
            if (angryTimer >= angryDuration)
            {
                currentState = State.Rescue;
                anim.SetBool("IsAngry", false);
                angryTimer = 0f;
            }
            return;
        }

        if (waypoints != null && waypoints.Length > 0)
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[wpIndex].position);
            if (distanceToWaypoint < 1.5f)
            {
                wpIndex = (wpIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[wpIndex].position);
            }
            agent.speed = 1.0f;
            anim.SetBool("IsWalking", true);
        }
        anim.SetBool("IsAngry", false);
        angryTimer = 0f;
    }

    private void Rescue()
    {
        if (rescueTarget == null) return;
        agent.speed = 2.0f;
        agent.isStopped = false;
        agent.SetDestination(rescueTarget.position);
        float distanceToObjective = Vector3.Distance(transform.position, rescueTarget.position);
        if (distanceToObjective < 1.5f)
        {
            anim.SetBool("IsWalking", false);
            agent.isStopped = true;
        }
        else
        {
            anim.SetBool("IsWalking", true);
        }
    }

    private bool LookForObjective()
    {
        if (objective == null) return false;
        Vector3 toObjective = objective.position - transform.position;
        float distance = toObjective.magnitude;
        if (distance > viewRadius) return false;
        Vector3 dir = toObjective.normalized;
        float angleToObjective = Vector3.Angle(transform.forward, dir);
        if (angleToObjective > viewAngle * 0.5f) return false;
        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, viewRadius))
            return hit.transform == objective || hit.transform.IsChildOf(objective);
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = LookForObjective() ? Color.green : Color.red;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up, leftBoundary * viewRadius);
        Gizmos.DrawRay(transform.position + Vector3.up, rightBoundary * viewRadius);
    }
}