using UnityEngine;

/// <summary>
/// Controla la pantalla de la TV: noticia, estática parpadeante (con sonido y luz nerviosa) o apagada.
/// Se conecta a DialogueInteractable desde el Inspector (evento On Trigger Line → ShowStatic).
/// (Versión del paso 4: sonido de estática y luz.)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class TVScreen : MonoBehaviour
{
    [Header("Imagen")]
    [SerializeField] private Sprite newsSprite;
    [SerializeField] private Sprite[] staticSprites;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private float staticFramesPerSecond = 12f;

    [Header("Sonido y luz (opcional)")]
    [SerializeField] private AudioClip staticClip;
    [Range(0f, 1f)]
    [SerializeField] private float staticVolume = 0.5f;
    [Tooltip("Luz de la TV con el componente LightFlicker.")]
    [SerializeField] private LightFlicker tvLight;
    [SerializeField] private float staticFlickerSpeed = 25f;

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private bool flickering;
    private float timer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (staticClip != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.clip = staticClip;
            audioSource.volume = staticVolume;
        }

        ShowNews();
    }

    public void ShowNews()
    {
        flickering = false;
        StopStaticAudio();
        if (newsSprite != null) sr.sprite = newsSprite;
    }

    public void ShowStatic()
    {
        flickering = true;
        timer = 0f;

        if (audioSource != null && !audioSource.isPlaying) audioSource.Play();
        if (tvLight != null) tvLight.SetSpeed(staticFlickerSpeed);
    }

    public void TurnOff()
    {
        flickering = false;
        StopStaticAudio();
        if (offSprite != null) sr.sprite = offSprite;
        if (tvLight != null) tvLight.gameObject.SetActive(false);
    }

    private void StopStaticAudio()
    {
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
    }

    private void Update()
    {
        if (!flickering || staticSprites == null || staticSprites.Length == 0) return;

        timer += Time.deltaTime * staticFramesPerSecond;
        sr.sprite = staticSprites[Mathf.FloorToInt(timer) % staticSprites.Length];
    }
}
