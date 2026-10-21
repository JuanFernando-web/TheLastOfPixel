using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

/// <summary>
/// La explosión de la introducción: sonido, temblor de cámara, destello blanco y el cambio de ambiente
/// (la noche azul pasa a un rojizo oscuro con el resplandor de un incendio lejano).
/// Se dispara llamando a Trigger() desde el evento "On Trigger Line" de la TV.
/// </summary>
public class ExplosionEvent : MonoBehaviour
{
    [Header("Sonido, cámara y pantalla")]
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private float shakeDuration = 1.4f;
    [SerializeField] private float shakeMagnitude = 0.25f;
    [Range(0f, 1f)]
    [SerializeField] private float flashAlpha = 0.9f;

    [Header("Luces")]
    [SerializeField] private Light2D globalLight;
    [Tooltip("Se apaga al explotar para que no pise el cambio de luz (arrastra el objeto Ambiente).")]
    [SerializeField] private LightningFlash lightning;
    [SerializeField] private Color afterColor = new Color(0.40f, 0.18f, 0.16f);
    [SerializeField] private float afterIntensity = 0.14f;
    [SerializeField] private float transitionTime = 2f;
    [Tooltip("Luces que se apagan con la explosión (lámparas de la casa).")]
    [SerializeField] private Light2D[] lightsToCut;
    [Tooltip("Objeto con una Light2D naranja (y LightFlicker) que empieza apagado y se enciende tras la explosión.")]
    [SerializeField] private GameObject glowObject;

    [Header("Evento")]
    [Tooltip("Se dispara cuando termina la transición de luz.")]
    [SerializeField] private UnityEvent onExploded;

    private AudioSource audioSource;
    private bool triggered;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (glowObject != null) glowObject.SetActive(false);
    }

    /// <summary>Lanza la explosión. Solo funciona una vez.</summary>
    public void Trigger()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        if (explosionClip != null) audioSource.PlayOneShot(explosionClip);
        if (cameraFollow != null) cameraFollow.Shake(shakeDuration, shakeMagnitude);
        ScreenFX.GetOrCreate().Flash(Color.white, flashAlpha, 0.7f);

        if (lightning != null) lightning.enabled = false;

        if (lightsToCut != null)
        {
            foreach (Light2D lamp in lightsToCut)
                if (lamp != null) lamp.enabled = false;
        }

        if (glowObject != null) glowObject.SetActive(true);

        if (globalLight != null)
        {
            Color startColor = globalLight.color;
            float startIntensity = globalLight.intensity;
            float t = 0f;

            while (t < transitionTime)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / transitionTime);
                globalLight.color = Color.Lerp(startColor, afterColor, k);
                globalLight.intensity = Mathf.Lerp(startIntensity, afterIntensity, k);
                yield return null;
            }

            globalLight.color = afterColor;
            globalLight.intensity = afterIntensity;
        }

        onExploded?.Invoke();
    }
}
