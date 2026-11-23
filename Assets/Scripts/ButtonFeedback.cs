using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Press: shrink (ease-out). Release: spring back with overshoot (ease-out-back).
public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    public UISound pressSound = UISound.Click;   // back buttons use UISound.Back

    public float pressedScale = 0.94f;
    public float downTime = 0.08f;
    public float upTime = 0.2f;
    Coroutine co;
    Selectable sel;

    void Awake() { sel = GetComponent<Selectable>(); }
    bool Active { get { return sel == null || sel.IsInteractable(); } }

    // hover only exists for a mouse (pointerId < 0); touches would give a double sound
    public void OnPointerEnter(PointerEventData e)
    {
        if (e.pointerId < 0 && Active) AudioManager.PlayUI(UISound.Hover);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (Active) AudioManager.PlayUI(pressSound);
        Tween(pressedScale, downTime, EaseOut);
    }
    public void OnPointerUp(PointerEventData e) { Tween(1f, upTime, EaseOutBack); }

    void Tween(float target, float dur, Func<float, float> ease)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Run(target, dur, ease));
    }

    IEnumerator Run(float target, float dur, Func<float, float> ease)
    {
        float start = transform.localScale.x;
        for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / dur)
        {
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(start, target, ease(t));
            yield return null;
        }
        transform.localScale = Vector3.one * target;
    }

    static float EaseOut(float t) { return 1f - (1f - t) * (1f - t) * (1f - t); }
    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
    }
}
