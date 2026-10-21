using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Relámpagos de la tormenta: suben un instante la luz global y después suena el trueno.
/// Al desactivar este componente la luz vuelve a su valor normal.
/// </summary>
public class LightningFlash : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private AudioClip thunderClip;
    [Range(0f, 1f)]
    [SerializeField] private float thunderVolume = 0.8f;

    [Tooltip("Segundos mínimo y máximo entre un relámpago y el siguiente.")]
    [SerializeField] private Vector2 intervalRange = new Vector2(7f, 16f);
    [Tooltip("Cuánto sube la intensidad de la luz global durante el relámpago.")]
    [SerializeField] private float flashBoost = 0.55f;
    [Tooltip("Segundos que tarda en llegar el trueno después del relámpago.")]
    [SerializeField] private Vector2 thunderDelayRange = new Vector2(0.4f, 2f);

    private AudioSource audioSource;
    private float baseIntensity;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (globalLight == null) return;
        baseIntensity = globalLight.intensity;
        StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (globalLight != null) globalLight.intensity = baseIntensity;
    }

    private IEnumerator Loop()
    {
        yield return new WaitForSeconds(Random.Range(intervalRange.x, intervalRange.y) * 0.5f);

        while (true)
        {
            yield return Strike();
            yield return new WaitForSeconds(Random.Range(intervalRange.x, intervalRange.y));
        }
    }

    private IEnumerator Strike()
    {
        int pulses = Random.Range(1, 3);
        for (int i = 0; i < pulses; i++)
        {
            globalLight.intensity = baseIntensity + flashBoost;
            yield return new WaitForSeconds(0.07f);
            globalLight.intensity = baseIntensity;
            yield return new WaitForSeconds(0.09f);
        }

        StartCoroutine(Thunder());
    }

    private IEnumerator Thunder()
    {
        yield return new WaitForSeconds(Random.Range(thunderDelayRange.x, thunderDelayRange.y));
        if (thunderClip != null) audioSource.PlayOneShot(thunderClip, thunderVolume);
    }
}
