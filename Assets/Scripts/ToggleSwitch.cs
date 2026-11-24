using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Settings toggle: knob slides (smoothstep), plus ON/OFF text so state isn't colour-only.
public class ToggleSwitch : MonoBehaviour
{
    public Button button;
    public RectTransform knob;
    public Image track;
    public TMP_Text label;
    public float travel = 50f;
    public Color onColor = Color.cyan, offColor = Color.gray;
    public bool isOn = true;
    Coroutine co;

    void Awake()
    {
        button.onClick.AddListener(Flip);
        Apply(true);
    }

    void Flip() { isOn = !isOn; AudioManager.PlayUI(UISound.Select); Apply(false); }

    void Apply(bool instant)
    {
        label.text = isOn ? "ON" : "OFF";
        track.color = isOn ? onColor : offColor;
        label.color = isOn ? onColor : offColor;
        float target = isOn ? travel : -travel;
        if (instant) { knob.anchoredPosition = new Vector2(target, 0); return; }
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Slide(target));
    }

    IEnumerator Slide(float target)
    {
        float start = knob.anchoredPosition.x;
        for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / 0.2f)
        {
            float e = t * t * (3f - 2f * t);
            knob.anchoredPosition = new Vector2(Mathf.Lerp(start, target, e), 0);
            yield return null;
        }
        knob.anchoredPosition = new Vector2(target, 0);
    }
}
