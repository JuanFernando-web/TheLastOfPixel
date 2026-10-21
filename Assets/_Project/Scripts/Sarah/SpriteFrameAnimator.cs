using UnityEngine;

/// <summary>
/// Anima un objeto cambiando entre varios sprites en bucle (la lluvia en la ventana, una lámpara, etc.).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 4f;
    [Tooltip("Cada objeto empieza en un cuadro distinto para que dos ventanas no se vean idénticas.")]
    [SerializeField] private bool randomStart = true;

    private SpriteRenderer sr;
    private float timer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (randomStart) timer = Random.value * 10f;
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime * framesPerSecond;
        sr.sprite = frames[Mathf.FloorToInt(timer) % frames.Length];
    }
}
