using UnityEngine;

/// <summary>
/// Cámara que sigue suavemente al jugador en vista top-down y puede temblar (explosiones, golpes).
/// Ponla en la Main Camera y arrastra a Sarah al campo Target.
/// (Versión del paso 4: agrega Shake.)
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector2 offset = Vector2.zero;

    private Vector3 basePosition;   // posición sin temblor
    private Vector3 velocity;
    private float shakeTime;
    private float shakeDuration = 1f;
    private float shakeMagnitude;

    public void SetTarget(Transform newTarget) => target = newTarget;

    /// <summary>Hace temblar la cámara. magnitude está en unidades del mundo (0.2 es un temblor fuerte).</summary>
    public void Shake(float duration, float magnitude)
    {
        shakeDuration = Mathf.Max(0.01f, duration);
        shakeTime = shakeDuration;
        shakeMagnitude = magnitude;
    }

    private void Start()
    {
        basePosition = transform.position;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                basePosition.z); // conserva la profundidad de la cámara

            basePosition = Vector3.SmoothDamp(basePosition, desired, ref velocity, smoothTime);
        }

        Vector3 shake = Vector3.zero;
        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            float strength = Mathf.Clamp01(shakeTime / shakeDuration);
            Vector2 random = Random.insideUnitCircle * shakeMagnitude * strength;
            shake = new Vector3(random.x, random.y, 0f);
        }

        transform.position = basePosition + shake;
    }
}
