using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecta el objeto interactuable que Sarah tiene enfrente y lo activa con E (o el botón A del mando).
/// Ponlo en el mismo objeto que PlayerController.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerInteractor : MonoBehaviour
{
    [Tooltip("Qué tan adelante del personaje se busca (en unidades del mundo).")]
    [SerializeField] private float reach = 0.6f;
    [SerializeField] private float radius = 0.6f;
    [Tooltip("Punto de partida de la búsqueda, a la altura del torso (el pivote está en los pies).")]
    [SerializeField] private Vector2 originOffset = new Vector2(0f, 0.4f);

    private PlayerController player;
    private Interactable current;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!player.InputEnabled || DialogueManager.IsOpen)
        {
            SetCurrent(null);
            return;
        }

        SetCurrent(FindBest());

        if (current != null
            && InteractPressed()
            && Time.frameCount != DialogueManager.LastCloseFrame)
        {
            current.Interact(player);
        }
    }

    private Interactable FindBest()
    {
        Vector2 origin = (Vector2)transform.position + originOffset + player.Facing * reach;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);

        Interactable best = null;
        float bestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Interactable candidate = hit.GetComponentInParent<Interactable>();
            if (candidate == null || !candidate.CanInteract) continue;

            float distance = ((Vector2)candidate.transform.position - origin).sqrMagnitude;
            if (distance < bestDistance)
            {
                best = candidate;
                bestDistance = distance;
            }
        }
        return best;
    }

    private void SetCurrent(Interactable target)
    {
        current = target;

        if (current != null)
            DialogueManager.GetOrCreate().ShowPrompt(current.PromptWorldPosition, current.Prompt);
        else if (DialogueManager.Instance != null)
            DialogueManager.Instance.HidePrompt();
    }

    private static bool InteractPressed()
    {
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        return (kb != null && kb.eKey.wasPressedThisFrame)
            || (pad != null && pad.buttonSouth.wasPressedThisFrame);
    }

    private void OnDrawGizmosSelected()
    {
        PlayerController p = GetComponent<PlayerController>();
        Vector2 facing = (p != null && Application.isPlaying) ? p.Facing : Vector2.down;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + originOffset + facing * reach, radius);
    }
}
