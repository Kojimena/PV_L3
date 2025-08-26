using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GrilBehaviour : MonoBehaviour
{
    private enum State { Idle, Patrol }

    [Header("Patrulla")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float walkSpeed = 2f;

    [Header("Rescate")]
    [SerializeField] private string enemyTag = "boldor";
    [SerializeField] private string sceneToLoad = "LostMenu";
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask detectionMask = ~0;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip screamSfx;

    private int wpIndex = 0;
    private State currentState = State.Idle;

    private Animator anim;
    private NavMeshAgent agent;
    private int HashSpeed;
    private bool sceneTriggered = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponentInChildren<Animator>();
        HashSpeed = Animator.StringToHash("Speed");
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

        CheckEnemyProximity();
    }

    private void Idle()
    {
        agent.speed = 0f;
    }

    private void Patrol()
    {
        agent.speed = walkSpeed;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[wpIndex].position);
        }
    }

    private void CheckEnemyProximity()
    {
        if (sceneTriggered || string.IsNullOrEmpty(sceneToLoad)) return;

        var hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag(enemyTag))
            {
                sceneTriggered = true;
                if (screamSfx) PlaySfxPersist(screamSfx, 1f);
                SceneManager.LoadScene(sceneToLoad);
                break;
            }
        }
    }

    private void PlaySfxPersist(AudioClip clip, float volume = 1f)
    {
        var go = new GameObject("OneShotSFX_Persistent");
        DontDestroyOnLoad(go);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        src.volume = volume;
        src.clip = clip;
        src.Play();
        Destroy(go, clip.length);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
