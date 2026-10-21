using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Hace parpadear una Light2D (la luz de la TV, el fuego de una explosión lejana, una lámpara dañada).
/// </summary>
[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [SerializeField] private float minIntensity = 0.7f;
    [SerializeField] private float maxIntensity = 1.2f;
    [Tooltip("Qué tan rápido cambia la luz. 3 = suave, 25 = nervioso.")]
    [SerializeField] private float speed = 6f;

    private Light2D target;
    private float seed;

    private void Awake()
    {
        target = GetComponent<Light2D>();
        seed = Random.value * 100f;
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(seed, Time.time * speed);
        target.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;

    public void SetRange(float min, float max)
    {
        minIntensity = min;
        maxIntensity = max;
    }
}
