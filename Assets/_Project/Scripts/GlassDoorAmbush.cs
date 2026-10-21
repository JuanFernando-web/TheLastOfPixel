using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// La escena de la puerta de cristal: algo golpea del otro lado, Sarah cree que es su papá, la puerta revienta,
/// un infectado la tira al suelo, el jugador resiste con un evento rápido y Joel llega a salvarla.
/// Se pone en una zona (BoxCollider2D marcado como Trigger) delante de la puerta.
/// La escena solo se activa cuando está "armada" (después de la explosión de la TV) y se juega una vez.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GlassDoorAmbush : MonoBehaviour
{
    [Header("Armado")]
    [Tooltip("Si no está marcado, la escena espera a que se llame a Arm() (desde ExplosionEvent → On Exploded).")]
    [SerializeField] private bool armed = false;

    [Header("Sarah")]
    [SerializeField] private PlayerController sarah;
    [SerializeField] private SpriteRenderer sarahRenderer;
    [Tooltip("Punto delante de la puerta donde Sarah se detiene y es atacada.")]
    [SerializeField] private Transform sarahStandPoint;

    [Header("Joel")]
    [SerializeField] private NpcWalker joel;
    [SerializeField] private Transform joelStartPoint;
    [SerializeField] private Transform joelArrivePoint;
    [Tooltip("Dónde se para Joel respecto a Sarah cuando la ayuda.")]
    [SerializeField] private Vector2 joelHelpOffset = new Vector2(-1f, -0.1f);

    [Header("Puerta de cristal")]
    [SerializeField] private SpriteRenderer doorRenderer;
    [Tooltip("Orden: 0 intacta, 1 golpeada, 2 agrietada, 3 abierta y rota.")]
    [SerializeField] private Sprite[] doorSprites;
    [Tooltip("Collider sólido de la puerta; se apaga cuando se rompe.")]
    [SerializeField] private Collider2D doorBlocker;

    [Header("Ataque")]
    [SerializeField] private SpriteRenderer attackRenderer;
    [Tooltip("Orden: 0 ataque A, 1 ataque B, 2 después del disparo, 3 solo el cuerpo.")]
    [SerializeField] private Sprite[] attackSprites;
    [SerializeField] private int qteLength = 5;
    [SerializeField] private float qteSecondsPerKey = 1.5f;

    [Header("Sonido y cámara")]
    [SerializeField] private AudioClip bangClip;
    [SerializeField] private AudioClip glassBreakClip;
    [SerializeField] private AudioClip growlClip;
    [SerializeField] private AudioClip gunshotClip;
    [SerializeField] private AudioClip qteHitClip;
    [SerializeField] private AudioClip qteFailClip;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Evento")]
    [Tooltip("Se dispara cuando termina la escena y Sarah recupera el control.")]
    [SerializeField] private UnityEvent onFinished;

    private AudioSource sfx;
    private AudioSource growl;
    private Transform doorTransform;
    private Vector3 doorHome;
    private Coroutine bangRoutine;
    private Coroutine struggleRoutine;
    private float bangInterval = 0.9f;
    private bool started;

    public void Arm() => armed = true;

    private void Awake()
    {
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;

        growl = gameObject.AddComponent<AudioSource>();
        growl.playOnAwake = false;
        growl.loop = true;
        growl.clip = growlClip;
        growl.volume = 0.8f;

        if (doorRenderer != null)
        {
            doorTransform = doorRenderer.transform;
            doorHome = doorTransform.position;
        }
        if (attackRenderer != null) attackRenderer.enabled = false;
        if (joel != null) joel.gameObject.SetActive(false);

        SetDoor(0);
    }

    // Stay (y no Enter) para que funcione aunque Sarah ya esté dentro de la zona cuando se arma la escena
    private void OnTriggerStay2D(Collider2D other)
    {
        if (started || !armed || !other.CompareTag("Player")) return;
        started = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        sarah.LockInput();

        bangRoutine = StartCoroutine(BangLoop());
        yield return Say(StoryLines.GlassIntro());

        // Sarah camina hacia la puerta creyendo que es su papá
        yield return WalkSarahTo(sarahStandPoint.position, 4f);
        sarah.Face(Vector2.up);
        bangInterval = 0.45f;
        yield return new WaitForSeconds(0.9f);

        // La puerta revienta
        StopBang();
        SetDoor(2);
        yield return new WaitForSeconds(0.15f);
        SetDoor(3);
        if (doorBlocker != null) doorBlocker.enabled = false;
        Play(glassBreakClip);
        Shake(0.5f, 0.22f);
        ScreenFX.GetOrCreate().Flash(Color.white, 0.35f, 0.25f);

        // El infectado derriba a Sarah
        sarahRenderer.enabled = false;
        attackRenderer.transform.position = sarah.transform.position;
        attackRenderer.sprite = attackSprites[0];
        attackRenderer.enabled = true;
        if (growlClip != null) growl.Play();
        struggleRoutine = StartCoroutine(StruggleLoop());

        // Evento rápido: si falla, se repite desde justo antes del ataque
        bool success = false;
        while (!success)
        {
            bool finished = false;
            QTEManager.GetOrCreate().StartSequence(qteLength, qteSecondsPerKey,
                ok => { success = ok; finished = true; },
                "¡Quítatelo de encima!", qteHitClip, qteFailClip);
            yield return new WaitUntil(() => finished);

            if (!success)
            {
                Shake(0.4f, 0.12f);
                ScreenFX.GetOrCreate().FadeTo(Color.black, 1f, 0.4f);
                yield return new WaitForSeconds(0.9f);
                ScreenFX.GetOrCreate().FadeTo(Color.black, 0f, 0.4f);
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Joel llega corriendo y dispara
        joel.gameObject.SetActive(true);
        joel.Teleport(joelStartPoint.position);
        yield return joel.WalkTo(joelArrivePoint.position, 5.5f);
        joel.Face(Vector2.up);
        yield return new WaitForSeconds(0.25f);

        Play(gunshotClip);
        Shake(0.3f, 0.12f);
        ScreenFX.GetOrCreate().Flash(Color.white, 0.5f, 0.25f);
        growl.Stop();
        if (struggleRoutine != null) StopCoroutine(struggleRoutine);
        attackRenderer.sprite = attackSprites[2];
        yield return new WaitForSeconds(1f);

        // Joel se acerca a Sarah
        yield return joel.WalkTo((Vector2)sarah.transform.position + joelHelpOffset, 2.4f);
        joel.Face(Vector2.right);
        yield return Say(StoryLines.GlassRescue());

        // Sarah se levanta y recupera el control
        attackRenderer.sprite = attackSprites[3];
        sarahRenderer.enabled = true;
        sarah.Face(Vector2.left);
        sarah.UnlockInput();
        onFinished?.Invoke();
    }

    // ------------------------------------------------------------ ayudas de la escena
    private IEnumerator Say(DialogueLine[] lines)
    {
        yield return new WaitUntil(() => !DialogueManager.IsOpen);

        bool done = false;
        DialogueManager.GetOrCreate().StartDialogue(lines, () => done = true);
        yield return new WaitUntil(() => done);
    }

    private IEnumerator WalkSarahTo(Vector2 target, float timeout)
    {
        float time = 0f;
        while (time < timeout)
        {
            Vector2 delta = target - (Vector2)sarah.transform.position;
            if (delta.magnitude < 0.08f) break;

            sarah.ScriptedMove(delta.normalized);
            time += Time.deltaTime;
            yield return null;
        }
        sarah.StopScriptedMove();
    }

    private IEnumerator BangLoop()
    {
        while (true)
        {
            Play(bangClip);
            SetDoor(1);
            Shake(0.15f, 0.04f);

            for (float t = 0f; t < 0.12f; t += Time.deltaTime)
            {
                if (doorTransform != null)
                    doorTransform.position = doorHome + (Vector3)(Random.insideUnitCircle * 0.04f);
                yield return null;
            }

            if (doorTransform != null) doorTransform.position = doorHome;
            SetDoor(0);
            yield return new WaitForSeconds(bangInterval);
        }
    }

    private void StopBang()
    {
        if (bangRoutine != null) StopCoroutine(bangRoutine);
        if (doorTransform != null) doorTransform.position = doorHome;
    }

    private IEnumerator StruggleLoop()
    {
        bool alternate = false;
        while (true)
        {
            alternate = !alternate;
            attackRenderer.sprite = attackSprites[alternate ? 1 : 0];
            yield return new WaitForSeconds(0.16f);
        }
    }

    private void SetDoor(int index)
    {
        if (doorRenderer != null && doorSprites != null && index < doorSprites.Length)
            doorRenderer.sprite = doorSprites[index];
    }

    private void Play(AudioClip clip)
    {
        if (clip != null) sfx.PlayOneShot(clip);
    }

    private void Shake(float duration, float magnitude)
    {
        if (cameraFollow != null) cameraFollow.Shake(duration, magnitude);
    }
}
