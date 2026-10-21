using UnityEngine;

/// <summary>
/// Cualquier personaje que se pueda animar con SpriteDirectionalAnimator:
/// dice hacia dónde mira y si está caminando (PlayerController, NpcWalker, infectados...).
/// </summary>
public interface IMovementSource
{
    Vector2 Facing { get; }
    bool IsMoving { get; }
}
