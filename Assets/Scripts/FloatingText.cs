using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class FloatingText : MonoBehaviour
{
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private Vector2 riseOffset = new Vector2(0, 60f);
    [SerializeField] private bool useUnscaledTime = false;

    private TMP_Text text;
    private RectTransform rect;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float timer = 0f;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + riseOffset;
        Color startColor = text.color;

        while (timer < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            timer += dt;

            float t = timer / duration;

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            text.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}