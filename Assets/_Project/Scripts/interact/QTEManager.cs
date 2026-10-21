using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Evento rápido (quick time event): el jugador debe pulsar una secuencia de botones en orden y a tiempo.
/// Teclado: Q W E R. Mando: X Y B A (en ese orden). Un error o quedarse sin tiempo termina la secuencia como fallida.
/// La interfaz se crea sola la primera vez que se usa.
/// </summary>
public class QTEManager : MonoBehaviour
{
    public static QTEManager Instance { get; private set; }
    public static bool IsRunning => Instance != null && Instance.running;

    [Tooltip("Segundos al inicio en los que se ignoran las pulsaciones (para no contar la tecla que abrió la escena).")]
    [SerializeField] private float graceTime = 0.4f;

    private static readonly string[] KeyLabels = { "Q", "W", "E", "R" };
    private static readonly string[] PadLabels = { "X", "Y", "B", "A" };

    private static readonly Color PendingColor = new Color(0.22f, 0.24f, 0.32f);
    private static readonly Color CurrentColor = new Color(0.96f, 0.78f, 0.25f);
    private static readonly Color GoodColor = new Color(0.25f, 0.70f, 0.35f);
    private static readonly Color BadColor = new Color(0.80f, 0.20f, 0.20f);

    private GameObject root;
    private RectTransform keyRow;
    private RectTransform timerFill;
    private Image timerFillImage;
    private TMP_Text titleText;
    private AudioSource audioSource;
    private readonly List<Image> boxes = new List<Image>();
    private bool running;

    public static QTEManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        return new GameObject("QTEManager").AddComponent<QTEManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        BuildUI();
        root.SetActive(false);
    }

    /// <summary>
    /// Empieza una secuencia de botones al azar. onFinished recibe true si el jugador la completó.
    /// secondsPerKey es el tiempo para cada botón (1.5 es un valor cómodo, 0.8 es difícil).
    /// </summary>
    public void StartSequence(int length, float secondsPerKey, Action<bool> onFinished,
        string title = "¡Defiéndete!", AudioClip hitClip = null, AudioClip failClip = null)
    {
        if (running) return;
        StartCoroutine(Run(length, secondsPerKey, onFinished, title, hitClip, failClip));
    }

    private IEnumerator Run(int length, float secondsPerKey, Action<bool> onFinished,
        string title, AudioClip hitClip, AudioClip failClip)
    {
        running = true;

        int[] sequence = new int[length];
        for (int i = 0; i < length; i++)
        {
            sequence[i] = UnityEngine.Random.Range(0, 4);
            if (i > 0 && sequence[i] == sequence[i - 1])
                sequence[i] = (sequence[i] + 1 + UnityEngine.Random.Range(0, 3)) % 4;
        }

        BuildKeys(sequence);
        titleText.text = title;
        root.SetActive(true);

        bool success = true;
        for (int i = 0; i < length && success; i++)
        {
            Highlight(i);
            float elapsed = 0f;
            bool done = false;

            while (!done)
            {
                elapsed += Time.unscaledDeltaTime;
                SetTimer(1f - elapsed / secondsPerKey);

                int pressed = ReadPressed();
                bool ignored = i == 0 && elapsed < graceTime;

                if (pressed >= 0 && !ignored)
                {
                    if (pressed == sequence[i])
                    {
                        boxes[i].color = GoodColor;
                        if (hitClip != null) audioSource.PlayOneShot(hitClip);
                    }
                    else
                    {
                        boxes[i].color = BadColor;
                        success = false;
                    }
                    done = true;
                }
                else if (elapsed >= secondsPerKey)
                {
                    boxes[i].color = BadColor;
                    success = false;
                    done = true;
                }

                yield return null;
            }

            boxes[i].rectTransform.localScale = Vector3.one;
        }

        if (!success && failClip != null) audioSource.PlayOneShot(failClip);

        yield return new WaitForSecondsRealtime(0.4f);

        root.SetActive(false);
        running = false;
        onFinished?.Invoke(success);
    }

    private static int ReadPressed()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.qKey.wasPressedThisFrame) return 0;
            if (kb.wKey.wasPressedThisFrame) return 1;
            if (kb.eKey.wasPressedThisFrame) return 2;
            if (kb.rKey.wasPressedThisFrame) return 3;
        }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.buttonWest.wasPressedThisFrame) return 0;
            if (pad.buttonNorth.wasPressedThisFrame) return 1;
            if (pad.buttonEast.wasPressedThisFrame) return 2;
            if (pad.buttonSouth.wasPressedThisFrame) return 3;
        }
        return -1;
    }

    // ------------------------------------------------------------ interfaz
    private void Highlight(int index)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (i == index)
            {
                boxes[i].color = CurrentColor;
                boxes[i].rectTransform.localScale = Vector3.one * 1.15f;
            }
            else if (i > index)
            {
                boxes[i].color = PendingColor;
            }
        }
    }

    private void SetTimer(float fraction)
    {
        fraction = Mathf.Clamp01(fraction);
        timerFill.anchorMax = new Vector2(fraction, 1f);
        timerFillImage.color = Color.Lerp(BadColor, GoodColor, fraction);
    }

    private void BuildKeys(int[] sequence)
    {
        for (int i = keyRow.childCount - 1; i >= 0; i--)
            Destroy(keyRow.GetChild(i).gameObject);
        boxes.Clear();

        foreach (int code in sequence)
        {
            GameObject box = new GameObject("Key", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(keyRow, false);
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(96f, 96f);
            Image image = box.GetComponent<Image>();
            image.color = PendingColor;
            boxes.Add(image);

            TMP_Text label = MakeText("Label", box.transform, Vector2.zero, Vector2.one, 46f, FontStyles.Bold, Color.white);
            label.text = KeyLabels[code] + "\n<size=55%>" + PadLabels[code] + "</size>";
        }
    }

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("QTECanvas");
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        root = canvasGO;

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.18f, 0.30f);
        panelRect.anchorMax = new Vector2(0.82f, 0.62f);
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        titleText = MakeText("Title", panel.transform, new Vector2(0.02f, 0.72f), new Vector2(0.98f, 0.98f),
            34f, FontStyles.Bold, Color.white);

        TMP_Text hint = MakeText("Hint", panel.transform, new Vector2(0.02f, 0.00f), new Vector2(0.98f, 0.15f),
            20f, FontStyles.Italic, new Color(0.7f, 0.72f, 0.8f));
        hint.text = "Teclado: Q W E R     Mando: X Y B A";

        GameObject row = new GameObject("Keys", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(panel.transform, false);
        keyRow = row.GetComponent<RectTransform>();
        keyRow.anchorMin = new Vector2(0.02f, 0.30f);
        keyRow.anchorMax = new Vector2(0.98f, 0.72f);
        keyRow.offsetMin = keyRow.offsetMax = Vector2.zero;
        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        GameObject bar = new GameObject("TimerBar", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(panel.transform, false);
        RectTransform barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.06f, 0.17f);
        barRect.anchorMax = new Vector2(0.94f, 0.25f);
        barRect.offsetMin = barRect.offsetMax = Vector2.zero;
        bar.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(bar.transform, false);
        timerFill = fill.GetComponent<RectTransform>();
        timerFill.anchorMin = Vector2.zero;
        timerFill.anchorMax = Vector2.one;
        timerFill.offsetMin = timerFill.offsetMax = Vector2.zero;
        timerFillImage = fill.GetComponent<Image>();
    }

    private static TMP_Text MakeText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        float size, FontStyles style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }
}
