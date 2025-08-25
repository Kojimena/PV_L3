using UnityEngine;
using TMPro; 

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 10;
    private int currentLives;

    [Header("UI")]
    public TMP_Text livesText; 

    void Start()
    {
        currentLives = maxLives;
        UpdateLivesUI();
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
        // o UnityEngine.SceneManagement.SceneManager.LoadScene("NombreDeLaEscena");
    }
}