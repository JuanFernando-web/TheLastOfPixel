using UnityEngine;

/// <summary>
/// Puerta que se abre sola cuando el jugador se acerca (el sistema de interacción con E llega en el paso 3).
/// Requiere: SpriteRenderer, un collider sólido (Blocker) y un segundo collider marcado como Trigger.
/// El jugador debe tener el tag "Player".
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Door : MonoBehaviour
{
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [Tooltip("Collider sólido (NO trigger) que bloquea el paso mientras la puerta está cerrada.")]
    [SerializeField] private Collider2D blocker;
    [Tooltip("Una puerta bloqueada no se abre aunque el jugador se acerque.")]
    [SerializeField] private bool locked = false;

    private SpriteRenderer sr;
    private int playersNearby;

    public bool IsLocked => locked;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        SetOpen(false);
    }

    public void Lock()
    {
        locked = true;
        SetOpen(false);
    }

    public void Unlock()
    {
        locked = false;
        if (playersNearby > 0) SetOpen(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playersNearby++;
        if (!locked) SetOpen(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playersNearby = Mathf.Max(0, playersNearby - 1);
        if (playersNearby == 0) SetOpen(false);
    }

    private void SetOpen(bool open)
    {
        sr.sprite = open ? openSprite : closedSprite;
        if (blocker != null) blocker.enabled = !open;
    }
}
