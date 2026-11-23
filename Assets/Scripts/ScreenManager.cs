using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [System.Serializable]
    public class ScreenEntry { public string id; public CanvasGroup group; }

    public List<ScreenEntry> screens = new List<ScreenEntry>();
    public string startScreen = "Title";
    public float duration = 0.35f;
    public float slide = 120f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    readonly Dictionary<string, CanvasGroup> map = new Dictionary<string, CanvasGroup>();
    readonly Stack<string> history = new Stack<string>();
    string current;
    bool busy;

    public bool Busy { get { return busy; } }
    public string Current { get { return current; } }

    void Awake()
    {
        foreach (var s in screens) { map[s.id] = s.group; Hide(s.group); }
        current = startScreen;
        var g = map[startScreen];
        g.alpha = 1; g.interactable = true; g.blocksRaycasts = true;
    }

    static void Hide(CanvasGroup g) { g.alpha = 0; g.interactable = false; g.blocksRaycasts = false; }

    // forward: remembers where you came from
    public void Go(string id)
    {
        if (busy || id == current) return;
        history.Push(current);
        AudioManager.PlayUI(UISound.Open);
        Switch(id, 1);
    }

    // forward and forget history (Main Menu, Retry, etc.)
    public void GoRoot(string id)
    {
        if (busy || id == current) return;
        history.Clear();
        AudioManager.PlayUI(UISound.Open);
        Switch(id, 1);
    }

    public void Back()
    {
        if (busy || history.Count == 0) return;
        Switch(history.Pop(), -1);
    }

    void Switch(string id, int dir)
    {
        var from = map[current];
        var to = map[id];
        current = id;
        StartCoroutine(Transition(from, to, dir));
    }

    IEnumerator Transition(CanvasGroup from, CanvasGroup to, int dir)
    {
        busy = true;
        from.interactable = false; from.blocksRaycasts = false;
        to.blocksRaycasts = true; to.interactable = false;
        var toRt = (RectTransform)to.transform;
        var fromRt = (RectTransform)from.transform;

        for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / duration)
        {
            float e = ease.Evaluate(t);
            to.alpha = e;
            toRt.anchoredPosition = new Vector2(Mathf.Lerp(slide * dir, 0, e), 0);
            from.alpha = 1 - e;
            fromRt.anchoredPosition = new Vector2(Mathf.Lerp(0, -slide * dir, e), 0);
            yield return null;
        }

        to.alpha = 1; toRt.anchoredPosition = Vector2.zero; to.interactable = true;
        Hide(from); fromRt.anchoredPosition = Vector2.zero;
        busy = false;
    }
}
