using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento top-down de Sarah (y luego de Joel) con teclado o mando.
/// Usa el paquete Input System, que viene activado en la plantilla 2D de Unity 6.
/// (Versión del paso 5: movimiento por script para escenas y IMovementSource.)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IMovementSource
{
    [Header("Velocidad")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 4.5f;
    [Tooltip("Sarah camina en la casa. Activa esto en las escenas de persecución.")]
    [SerializeField] private bool canRun = false;
    [Tooltip("Multiplicador de velocidad cuando el personaje está asustado (SetPanic).")]
    [SerializeField] private float panicMultiplier = 1.4f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool runHeld;
    private int lockCount;
    private bool panic;
    private bool scripted;
    private Vector2 scriptedInput;

    /// <summary>Última dirección hacia la que miró el personaje (se usa para animar, interactuar y apuntar).</summary>
    public Vector2 Facing { get; private set; } = Vector2.down;

    /// <summary>False mientras haya un diálogo, cinemática o evento rápido que bloquee el control.</summary>
    public bool InputEnabled => lockCount == 0;

    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

    /// <summary>Bloquea el control. Cada LockInput() debe tener su UnlockInput().</summary>
    public void LockInput() => lockCount++;

    public void UnlockInput() => lockCount = Mathf.Max(0, lockCount - 1);

    /// <summary>Activa o quita el miedo: con miedo se camina más rápido. Se puede llamar desde un UnityEvent.</summary>
    public void SetPanic(bool on) => panic = on;

    /// <summary>Hace caminar al personaje en esa dirección aunque el control esté bloqueado (cinemáticas). Llamar cada frame.</summary>
    public void ScriptedMove(Vector2 direction)
    {
        scripted = true;
        scriptedInput = Vector2.ClampMagnitude(direction, 1f);
    }

    public void StopScriptedMove()
    {
        scripted = false;
        scriptedInput = Vector2.zero;
    }

    /// <summary>Gira al personaje hacia esa dirección sin moverlo.</summary>
    public void Face(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.001f) Facing = direction.normalized;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;      // vista top-down: no hay gravedad
        rb.freezeRotation = true;  // el personaje no rueda al chocar
    }

    private void OnEnable() => DialogueManager.OpenChanged += OnDialogueChanged;

    private void OnDisable() => DialogueManager.OpenChanged -= OnDialogueChanged;

    private void OnDialogueChanged(bool open)
    {
        if (open) LockInput();
        else UnlockInput();
    }

    private void Update()
    {
        moveInput = Vector2.zero;
        runHeld = false;

        if (scripted)
        {
            moveInput = scriptedInput;
            if (IsMoving) Facing = moveInput.normalized;
            return;
        }

        if (!InputEnabled) return;

        Vector2 input = Vector2.zero;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
            runHeld = kb.leftShiftKey.isPressed;
        }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            input += pad.leftStick.ReadValue();
            runHeld |= pad.leftStickButton.isPressed;
        }

        // Evita que la diagonal sea más rápida que ir recto
        moveInput = Vector2.ClampMagnitude(input, 1f);

        if (IsMoving) Facing = moveInput.normalized;
    }

    private void FixedUpdate()
    {
        float speed = (canRun && runHeld) ? runSpeed : walkSpeed;
        if (panic) speed *= panicMultiplier;
        Vector2 velocity = moveInput * speed;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
        rb.velocity = velocity;
#endif
    }
}
