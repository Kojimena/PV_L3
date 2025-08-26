using UnityEngine;
using TMPro;
using System.Collections;

public class EnemySpeech : MonoBehaviour
{
    [SerializeField] private TMP_Text bubbleText;
    [SerializeField] [TextArea] private string line = "¡Aparta, voy a rescatar a la princesa!";
    [SerializeField] private float showSeconds = 3f;
    [SerializeField] private bool playOnStart = true;

    private void Start()
    {
        if (playOnStart) Speak();
    }

    public void Speak()
    {
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        if (bubbleText == null) yield break;
        bubbleText.gameObject.SetActive(true);
        bubbleText.text = line;
        yield return new WaitForSeconds(showSeconds);
        bubbleText.gameObject.SetActive(false);
    }
}