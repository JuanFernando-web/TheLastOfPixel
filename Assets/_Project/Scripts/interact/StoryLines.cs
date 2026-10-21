/// <summary>Opciones de texto ya escrito para los objetos de la introducción.</summary>
public enum StoryPreset
{
    Custom,   // usa las líneas que escribas en el Inspector
    Bed,
    Phone,
    Photo,
    TvNews
}

/// <summary>
/// Todos los textos de la introducción en un solo lugar.
/// Edítalos aquí o usa el preset "Custom" y escribe tus propias líneas en el Inspector.
/// </summary>
public static class StoryLines
{
    public static DialogueLine[] Get(StoryPreset preset)
    {
        switch (preset)
        {
            case StoryPreset.Bed:
                return new[]
                {
                    L("Sarah", "Todavía está tibia mi cama... ¿Qué hora será? Está tan oscuro afuera.")
                };

            case StoryPreset.Phone:
                return new[]
                {
                    L("Sarah", "Llamadas perdidas... todas de papá."),
                    L("Sarah", "Dijo que volvería temprano. ¿Dónde estará?")
                };

            case StoryPreset.Photo:
                return new[]
                {
                    L("Sarah", "La feria del año pasado. Papá prometió llevarme otra vez."),
                    L("Sarah", "Tengo que encontrarlo.")
                };

            case StoryPreset.TvNews:
                return new[]
                {
                    L("Reportera", "Interrumpimos la programación para informar sobre un brote de un virus desconocido en la ciudad."),
                    L("Reportera", "Las autoridades piden mantener la calma y permanecer en sus hogares. Los hospitales ya están desbordados."),
                    L("Reportera", "Testigos hablan de personas que atacan a otras sin razón. Repetimos: no salgan de..."),
                    L("", "(Una explosión sacude el estudio. La transmisión se corta.)", true),
                    L("Sarah", "¿Qué fue eso...? ¡Papá! ¡PAPÁ!")
                };

            default:
                return new DialogueLine[0];
        }
    }

    /// <summary>Antes de que Sarah se acerque a la puerta de cristal.</summary>
    public static DialogueLine[] GlassIntro()
    {
        return new[]
        {
            L("", "(Alguien golpea con fuerza la puerta de cristal, del otro lado.)"),
            L("Sarah", "¿Papá...? ¿Eres tú?")
        };
    }

    /// <summary>Después de que Joel dispara al infectado.</summary>
    public static DialogueLine[] GlassRescue()
    {
        return new[]
        {
            L("Joel", "¡Sarah! ¿Estás bien? ¿Te mordió?"),
            L("Sarah", "N-no... creo que no."),
            L("Joel", "Escúchame. Tenemos que salir de aquí. Ahora."),
            L("Joel", "Quédate pegada a mí.")
        };
    }

    private static DialogueLine L(string speaker, string text, bool fireEvent = false)
    {
        return new DialogueLine { speaker = speaker, text = text, fireEvent = fireEvent };
    }
}
