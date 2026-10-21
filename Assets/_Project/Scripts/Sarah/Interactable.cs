using UnityEngine;

/// <summary>
/// Base de todo lo que el jugador puede usar con la tecla E (objetos, la TV, puertas con llave...).
/// El objeto necesita un Collider2D para que el jugador lo detecte.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Texto del aviso: aparece como [E] + este texto.")]
    [SerializeField] private string prompt = "Examinar";

    [Tooltip("Dónde aparece el aviso respecto al centro del objeto (en unidades del mundo).")]
    [SerializeField] private Vector2 promptOffset = new Vector2(0f, 0.9f);

    public string Prompt => prompt;
    public Vector3 PromptWorldPosition => transform.position + (Vector3)promptOffset;

    /// <summary>Si es false, el objeto no muestra aviso ni responde a E.</summary>
    public virtual bool CanInteract => true;

    public abstract void Interact(PlayerController player);
}
