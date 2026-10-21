using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Efectos que cubren toda la pantalla: destello blanco (explosión) y fundido a negro (final del juego).
/// Se crea solo la primera vez que se usa.
/// </summary>
public class ScreenFX : MonoBehaviour
{
    public static ScreenFX Instance { get; private set; }

    private Image overlay;
    private Coroutine routine;

    public static ScreenFX GetOrCreate()
    {
        if (Instance != null) return Instance;
        return new GameObject("ScreenFX").AddComponent<ScreenFX>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameObject canvasGO = new GameObject("ScreenFXCanvas");
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90; // debajo del cuadro de diálogo (100)

        GameObject imageGO = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
        imageGO.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = imageGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        overlay = imageGO.GetComponent<Image>();
        overlay.raycastTarget = false;
        overlay.color = Color.clear;
    }

    /// <summary>Destello que aparece de golpe y se desvanece. Ej.: Flash(Color.white, 0.9f, 0.7f).</summary>
    public void Flash(Color color, float peakAlpha, float duration)
    {
        Run(FlashRoutine(color, peakAlpha, duration));
    }

    /// <summary>Cambia suavemente el color y la opacidad de la pantalla. FadeTo(Color.black, 1f, 1.5f) deja todo negro.</summary>
    public void FadeTo(Color color, float alpha, float duration)
    {
        Run(FadeRoutine(color, alpha, duration));
    }

    /// <summary>Quita cualquier efecto de la pantalla.</summary>
    public void Clear(float duration = 0.3f)
    {
        FadeTo(overlay.color, 0f, duration);
    }

    private void Run(IEnumerator next)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(next);
    }

    private IEnumerator FlashRoutine(Color color, float peakAlpha, float duration)
    {
        Color c = color;
        c.a = peakAlpha;
        overlay.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Clamp01(t / duration);
            c.a = peakAlpha * k * k;
            overlay.color = c;
            yield return null;
        }

        c.a = 0f;
        overlay.color = c;
    }

    private IEnumerator FadeRoutine(Color color, float alpha, float duration)
    {
        Color from = overlay.color;
        Color to = color;
        to.a = alpha;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            overlay.color = Color.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        overlay.color = to;
    }
}
