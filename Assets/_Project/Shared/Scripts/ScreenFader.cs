using System;
using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Timings")]
    [SerializeField] private float fadeDuration = 0.25f; // 0.25s down + 0.25s up = 0.5s total blink

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Fades screen to black, executes action at peak blackness, then fades back to clear.
    /// </summary>
    public void TriggerBlink(Action onPeakBlack, float fadeDuration = -1f)
    {
        float duration = fadeDuration > 0f ? fadeDuration : this.fadeDuration;
        StartCoroutine(BlinkRoutine(duration, onPeakBlack));
    }

    private IEnumerator BlinkRoutine(float duration, Action onPeakBlack)
    {
        if (fadeCanvasGroup == null)
        {
            onPeakBlack?.Invoke();
            yield break;
        }

        // 1. Fade out to black
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // 2. Execute callback at peak black (spawn teacher, enable timeline, etc.)
        onPeakBlack?.Invoke();

        // 3. Fade back to clear
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
    }
}