using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Muestra los diálogos (cuadro abajo, texto que aparece letra por letra) y el aviso "[E] Examinar".
/// Se crea solo la primera vez que se usa: no hace falta armar ningún Canvas a mano.
/// Requiere TextMeshPro (Window > TextMeshPro > Import TMP Essential Resources, solo la primera vez).
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    /// <summary>Avisa cuando se abre (true) o se cierra (false) un diálogo. El jugador se bloquea con esto.</summary>
    public static event Action<bool> OpenChanged;

    /// <summary>Frame en el que se cerró el último diálogo (evita reabrirlo con la misma pulsación de E).</summary>
    public static int LastCloseFrame { get; private set; } = -1;

    public static bool IsOpen => Instance != null && Instance.isOpen;

    [Header("Texto")]
    [SerializeField] private float charsPerSecond = 45f;
    [SerializeField] private float bodyFontSize = 30f;
    [SerializeField] private float speakerFontSize = 26f;

    private RectTransform canvasRect;
    private GameObject dialogueRoot;
    private TMP_Text speakerText, bodyText, hintText;
    private GameObject promptRoot;
    private RectTransform promptRect;
    private TMP_Text promptLabel;

    private DialogueLine[] lines;
    private int index;
    private bool isOpen, typing;
    private Coroutine typeRoutine;
    private int startFrame;
    private Action onComplete;
    private Action<int> onLineShown;

    public static DialogueManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        return new GameObject("DialogueManager").AddComponent<DialogueManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        BuildUI();
        dialogueRoot.SetActive(false);
        promptRoot.SetActive(false);
    }

    // ------------------------------------------------------------ diálogo
    public void StartDialogue(DialogueLine[] newLines, Action complete = null, Action<int> lineShown = null)
    {
        if (newLines == null || newLines.Length == 0 || isOpen) return;

        lines = newLines;
        index = 0;
        onComplete = complete;
        onLineShown = lineShown;
        isOpen = true;
        startFrame = Time.frameCount;

        HidePrompt();
        dialogueRoot.SetActive(true);
        OpenChanged?.Invoke(true);
        ShowLine();
    }

    private void Update()
    {
        // Ignora la pulsación de E que acaba de abrir el diálogo
        if (!isOpen || Time.frameCount == startFrame) return;
        if (!AdvancePressed()) return;

        if (typing) FinishTyping();
        else NextLine();
    }

    private static bool AdvancePressed()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        Gamepad pad = Gamepad.current;

        if (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) return true;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;
        if (pad != null && pad.buttonSouth.wasPressedThisFrame) return true;
        return false;
    }

    private void ShowLine()
    {
        DialogueLine line = lines[index];

        speakerText.gameObject.SetActive(!string.IsNullOrEmpty(line.speaker));
        speakerText.text = line.speaker;
        bodyText.text = line.text;
        bodyText.maxVisibleCharacters = 0;
        hintText.gameObject.SetActive(false);

        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeLine());

        onLineShown?.Invoke(index);
    }

    private IEnumerator TypeLine()
    {
        typing = true;
        bodyText.ForceMeshUpdate();
        int total = bodyText.textInfo.characterCount;
        float shown = 0f;

        while (bodyText.maxVisibleCharacters < total)
        {
            shown += Time.unscaledDeltaTime * charsPerSecond;
            bodyText.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));
            yield return null;
        }

        typing = false;
        hintText.gameObject.SetActive(true);
    }

    private void FinishTyping()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        bodyText.maxVisibleCharacters = int.MaxValue;
        typing = false;
        hintText.gameObject.SetActive(true);
    }

    private void NextLine()
    {
        index++;
        if (index >= lines.Length) Close();
        else ShowLine();
    }

    private void Close()
    {
        isOpen = false;
        typing = false;
        LastCloseFrame = Time.frameCount;
        dialogueRoot.SetActive(false);
        OpenChanged?.Invoke(false);

        Action callback = onComplete;
        onComplete = null;
        onLineShown = null;
        callback?.Invoke();
    }

    // ------------------------------------------------------------ aviso "[E] ..."
    public void ShowPrompt(Vector3 worldPosition, string label)
    {
        Camera cam = Camera.main;
        if (cam == null || isOpen) return;

        promptRoot.SetActive(true);
        promptLabel.text = "[E] " + label;

        Vector2 screen = cam.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
        promptRect.anchoredPosition = local;
    }

    public void HidePrompt()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    // ------------------------------------------------------------ construcción de la interfaz
    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("GameUI");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasRect = canvasGO.GetComponent<RectTransform>();

        // Cuadro de diálogo
        dialogueRoot = MakeImage("DialogueBox", canvasGO.transform, new Color(0.05f, 0.05f, 0.09f, 0.92f));
        RectTransform box = dialogueRoot.GetComponent<RectTransform>();
        box.anchorMin = new Vector2(0.05f, 0.04f);
        box.anchorMax = new Vector2(0.95f, 0.30f);
        box.offsetMin = box.offsetMax = Vector2.zero;
        Outline border = dialogueRoot.AddComponent<Outline>();
        border.effectColor = new Color(0.85f, 0.85f, 0.95f, 0.9f);
        border.effectDistance = new Vector2(3f, -3f);

        speakerText = MakeText("Speaker", dialogueRoot.transform, new Vector2(0.03f, 0.70f), new Vector2(0.97f, 0.96f),
            speakerFontSize, FontStyles.Bold, new Color(0.96f, 0.76f, 0.44f), TextAlignmentOptions.TopLeft);
        bodyText = MakeText("Body", dialogueRoot.transform, new Vector2(0.03f, 0.18f), new Vector2(0.97f, 0.72f),
            bodyFontSize, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft);
        hintText = MakeText("Hint", dialogueRoot.transform, new Vector2(0.60f, 0.02f), new Vector2(0.98f, 0.18f),
            22f, FontStyles.Italic, new Color(0.7f, 0.72f, 0.8f), TextAlignmentOptions.BottomRight);
        hintText.text = "E / Espacio  >";

        // Aviso de interacción
        promptRoot = MakeImage("Prompt", canvasGO.transform, new Color(0.05f, 0.05f, 0.09f, 0.85f));
        promptRect = promptRoot.GetComponent<RectTransform>();
        promptRect.anchorMin = promptRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptRect.sizeDelta = new Vector2(280f, 46f);
        promptLabel = MakeText("Label", promptRoot.transform, Vector2.zero, Vector2.one,
            24f, FontStyles.Normal, Color.white, TextAlignmentOptions.Center);
    }

    private static GameObject MakeImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static TMP_Text MakeText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        float size, FontStyles style, Color color, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
        return t;
    }
}
