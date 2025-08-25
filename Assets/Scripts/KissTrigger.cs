using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class KissTrigger : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private Transform player;        // Si lo dejas vacío, busca por Tag
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float radius = 1.5f;     // Distancia máxima para activar
    [SerializeField] private float fovAngle = 60f;    // Ángulo del cono (grados, total)

    [Header("Animación de la chica")]
    [SerializeField] private Animator girlAnimator;   // Animator de la chica (si vacío, busca en hijos)
    [SerializeField] private string kissTrigger = "Kiss";
    [SerializeField] private float endDelay = 5.0f;   // Espera para que se vea el beso

    [Header("Final del juego")]
    [SerializeField] private string winSceneName = "";   // Si lo pones, carga esta escena
    [SerializeField] private bool quitIfNoScene = true;  // Si no hay escena, cerrar juego
    [SerializeField] private bool freezeIfEditor = true; // En editor, pausar TimeScale

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

        // ¿El jugador está dentro del cono frente a la chica?
        toPlayer.y = 0f;
        Vector3 forward = transform.forward; forward.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float angle = Vector3.Angle(forward.normalized, toPlayer.normalized);
        if (angle > fovAngle * 0.5f) return;

        // ¡Condiciones cumplidas! Disparar beso de la chica y terminar.
        StartCoroutine(DoKissAndEnd());
    }

    private IEnumerator DoKissAndEnd()
    {
        done = true;

        // Opcional: parar su NavMeshAgent para que no se mueva durante el beso
        if (TryGetComponent(out NavMeshAgent girlAgent))
        {
            girlAgent.isStopped = true;
            girlAgent.ResetPath();
        }

        // Mirar suavemente al jugador
        Vector3 look = player.position - transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(look);

        // Disparar animación de la chica
        girlAnimator.ResetTrigger(kissTrigger);
        girlAnimator.SetTrigger(kissTrigger);

        // Esperar para que se vea la animación
        if (endDelay > 0f) yield return new WaitForSeconds(endDelay);

        // Final del juego
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
            Debug.Log("Juego finalizado (Editor): beso completado.");
            return;
        }
#endif

        if (quitIfNoScene)
        {
            Application.Quit();
        }
    }

    // Gizmos para ver el radio y el cono en escena
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
