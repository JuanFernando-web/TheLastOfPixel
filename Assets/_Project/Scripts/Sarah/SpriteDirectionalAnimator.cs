using System;
using UnityEngine;

/// <summary>
/// Anima a un personaje top-down de 4 direcciones usando un sprite sheet (sin Animator).
/// Orden esperado del sheet (filas): 0 = abajo, 1 = izquierda, 2 = derecha, 3 = arriba.
/// Cada fila tiene 4 cuadros: quieto, paso, quieto, paso.
/// Ponlo en el mismo objeto que el SpriteRenderer y un componente que implemente IMovementSource
/// (PlayerController para Sarah, NpcWalker para Joel).
/// (Versión del paso 5: funciona con cualquier IMovementSource.)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteDirectionalAnimator : MonoBehaviour
{
    [Tooltip("Los 16 sprites cortados del sheet. Selecciónalos todos en la ventana Project y arrástralos aquí.")]
    [SerializeField] private Sprite[] sheetSprites;
    [SerializeField] private int framesPerRow = 4;
    [SerializeField] private float framesPerSecond = 8f;

    private enum Row { Down = 0, Left = 1, Right = 2, Up = 3 }

    private SpriteRenderer sr;
    private IMovementSource source;
    private float timer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        source = GetComponent<IMovementSource>();

        // Unity nombra los cortes nombre_0, nombre_1... pero el arrastre puede desordenarlos
        if (sheetSprites != null)
            Array.Sort(sheetSprites, (a, b) => ExtractIndex(a).CompareTo(ExtractIndex(b)));
    }

    private void Update()
    {
        if (source == null || sheetSprites == null || sheetSprites.Length < framesPerRow * 4) return;

        Row row = RowFromFacing(source.Facing);

        int frame = 0;
        if (source.IsMoving)
        {
            timer += Time.deltaTime;
            frame = Mathf.FloorToInt(timer * framesPerSecond) % framesPerRow;
        }
        else
        {
            timer = 0f;
        }

        sr.sprite = sheetSprites[(int)row * framesPerRow + frame];
    }

    private static Row RowFromFacing(Vector2 facing)
    {
        if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
            return facing.x > 0f ? Row.Right : Row.Left;
        return facing.y > 0f ? Row.Up : Row.Down;
    }

    private static int ExtractIndex(Sprite s)
    {
        if (s == null) return int.MaxValue;
        int i = s.name.LastIndexOf('_');
        return (i >= 0 && int.TryParse(s.name.Substring(i + 1), out int n)) ? n : 0;
    }
}
