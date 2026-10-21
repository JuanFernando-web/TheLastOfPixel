using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objeto que abre un diálogo al usarlo con E. Sirve para la cama, el teléfono, la foto y la TV.
/// </summary>
public class DialogueInteractable : Interactable
{
    [Tooltip("Custom = usa las líneas de abajo. Los demás cargan el texto de StoryLines.cs.")]
    [SerializeField] private StoryPreset preset = StoryPreset.Custom;

    [SerializeField] private DialogueLine[] lines;

    [Tooltip("Si está marcado, solo se puede usar una vez.")]
    [SerializeField] private bool onlyOnce = false;

    [Header("Eventos")]
    [Tooltip("Se dispara cuando se muestra una línea marcada con 'Fire Event'.")]
    [SerializeField] private UnityEvent onTriggerLine;

    [Tooltip("Se dispara cuando termina el diálogo.")]
    [SerializeField] private UnityEvent onFinished;

    private bool used;

    public override bool CanInteract => !(onlyOnce && used) && !DialogueManager.IsOpen;

    private void Awake()
    {
        if (preset != StoryPreset.Custom) lines = StoryLines.Get(preset);
    }

    public override void Interact(PlayerController player)
    {
        if (lines == null || lines.Length == 0) return;

        DialogueLine[] current = lines;
        DialogueManager.GetOrCreate().StartDialogue(
            current,
            () =>
            {
                used = true;
                onFinished?.Invoke();
            },
            lineIndex =>
            {
                if (current[lineIndex].fireEvent) onTriggerLine?.Invoke();
            });
    }
}
