using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class KissTrigger : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private Transform player;        
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float radius = 1.5f;    
    [SerializeField] private float fovAngle = 60f;    

    [Header("Anim")]
    [SerializeField] private Animator girlAnimator;   
    [SerializeField] private string kissTrigger = "Kiss";
    [SerializeField] private float endDelay = 5.0f;   

    [Header("Final")]
    [SerializeField] private string winSceneName = "";   
    [SerializeField] private bool quitIfNoScene = true;  
    [SerializeField] private bool freezeIfEditor = true; 

    private bool done;

    void Awake()
    {
        if (!girlAnimator) girlAnimator = GetComponentInChildren<Animator>();
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (done || !player || !girlAnimator) return;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > radius) return;

        toPlayer.y = 0f;
        Vector3 forward = transform.forward; forward.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float angle = Vector3.Angle(forward.normalized, toPlayer.normalized);
        if (angle > fovAngle * 0.5f) return;

        StartCoroutine(DoKissAndEnd());
    }

    private IEnumerator DoKissAndEnd()
    {
        done = true;

        if (TryGetComponent(out NavMeshAgent girlAgent))
        {
            girlAgent.isStopped = true;
            girlAgent.ResetPath();
        }

        Vector3 look = player.position - transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(look);

        girlAnimator.ResetTrigger(kissTrigger);
        girlAnimator.SetTrigger(kissTrigger);

        if (endDelay > 0f) yield return new WaitForSeconds(endDelay);

        EndGame();
    }

    private void EndGame()
    {
        if (!string.IsNullOrEmpty(winSceneName))
        {
            SceneManager.LoadScene(winSceneName);
            return;
        }

#if UNITY_EDITOR
        if (freezeIfEditor)
        {
            Time.timeScale = 0f;
            Debug.Log("Juego finalizado.");
            return;
        }
#endif

        if (quitIfNoScene)
        {
            Application.Quit();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.5f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radius);

        Vector3 f = transform.forward;
        Quaternion leftRot  = Quaternion.AngleAxis(-fovAngle * 0.5f, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis( fovAngle * 0.5f, Vector3.up);
        Gizmos.color = new Color(1f, 0.2f, 0.5f, 0.6f);
        Gizmos.DrawRay(transform.position, leftRot  * f * radius);
        Gizmos.DrawRay(transform.position, rightRot * f * radius);
    }
}
