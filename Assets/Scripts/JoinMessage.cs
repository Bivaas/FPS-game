using UnityEngine;
using TMPro;
using System.Collections;

public class JoinMessage : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI messageText;

    [Header("Timing")]
    public float displayDuration = 4f;
    public float fadeDuration = 1.5f;

    [Header("Message")]
    [TextArea(2, 4)]
    public string message = "In the centerpiece lies the ultimate firepower...";

    void Start()
    {
        if (messageText == null) return;

        messageText.text = message;
        StartCoroutine(ShowAndFade());
    }

    IEnumerator ShowAndFade()
    {
        SetAlpha(1f);

        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f);
        messageText.gameObject.SetActive(false);
    }

    void SetAlpha(float a)
    {
        Color c = messageText.color;
        c.a = a;
        messageText.color = c;
    }
}