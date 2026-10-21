using System.Collections;
using UnityEngine;

/// <summary>
/// Personaje no jugable que camina por orden de un script (Joel en la emboscada, luego como acompañante).
/// Se anima con SpriteDirectionalAnimator igual que Sarah.
/// </summary>
public class NpcWalker : MonoBehaviour, IMovementSource
{
    [SerializeField] private float walkSpeed = 2.2f;

    public Vector2 Facing { get; private set; } = Vector2.down;
    public bool IsMoving { get; private set; }

    public void Face(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.001f) Facing = direction.normalized;
    }

    public void Teleport(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    /// <summary>Camina hasta el punto. Uso: yield return npc.WalkTo(punto); (speed menor a 0 usa la velocidad normal).</summary>
    public IEnumerator WalkTo(Vector2 target, float speed = -1f)
    {
        float moveSpeed = speed > 0f ? speed : walkSpeed;

        while (true)
        {
            Vector2 position = transform.position;
            Vector2 delta = target - position;
            if (delta.magnitude <= 0.05f) break;

            IsMoving = true;
            Facing = delta.normalized;

            Vector2 next = Vector2.MoveTowards(position, target, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            yield return null;
        }

        IsMoving = false;
    }
}
