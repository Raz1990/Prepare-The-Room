using System.Collections;
using UnityEngine;

public class WhiteboardCleaner : MonoBehaviour
{
    [Header("Target Canvas Group")]
    [SerializeField] private CanvasGroup textCanvasGroup;

    [Header("Wipe Settings")]
    [SerializeField] private float wipeDuration = 4.0f;

    void OnEnable()
    {
        WhiteboardEvents.OnWhiteboardCleaningStarted += StartCleaningProcess;
    }

    void OnDisable()
    {
        WhiteboardEvents.OnWhiteboardCleaningStarted -= StartCleaningProcess;
    }

    private void StartCleaningProcess()
    {
        if (textCanvasGroup != null)
        {
            StartCoroutine(FadeAndDestroyCanvasRoutine());
        }
    }

    private IEnumerator FadeAndDestroyCanvasRoutine()
    {
        float elapsed = 0f;
        float startAlpha = textCanvasGroup.alpha;

        while (elapsed < wipeDuration)
        {
            elapsed += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / wipeDuration);
            yield return null;
        }

        textCanvasGroup.alpha = 0f;

        // Destroy the text container canvas/panel entirely once transparent
        Destroy(textCanvasGroup.gameObject);
    }
}