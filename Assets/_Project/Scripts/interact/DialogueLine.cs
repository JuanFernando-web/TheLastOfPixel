using System;
using UnityEngine;

/// <summary>Una línea de diálogo: quién habla, qué dice y si dispara un evento al mostrarse.</summary>
[Serializable]
public struct DialogueLine
{
    [Tooltip("Nombre que se muestra arriba del cuadro. Déjalo vacío para texto narrativo.")]
    public string speaker;

    [TextArea(2, 4)]
    public string text;

    [Tooltip("Si está marcado, al mostrar esta línea se dispara el evento 'On Trigger Line' del objeto.")]
    public bool fireEvent;
}
