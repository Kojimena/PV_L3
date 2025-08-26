using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 10;
    private int currentLives;
    [SerializeField] private float fallThreshold = -10f;

    [Header("UI")]
    public TMP_Text livesText; 

    void Start()
    {
        currentLives = maxLives;
        UpdateLivesUI();
    }
    
    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            RestartScene();
        }
    }
    
    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TakeDamage(int amount)
    {
        currentLives -= amount;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            Die();
        }
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "Vidas: " + currentLives;
        }
    }

    private void Die()
    {
        Debug.Log("¡Jugador muerto!");
        gameObject.SetActive(false); 
        UnityEngine.SceneManagement.SceneManager.LoadScene("LostMenu");
    }
}