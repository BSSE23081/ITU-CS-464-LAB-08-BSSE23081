using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Full-screen loading overlay that survives scene changes.
// LoadingScreen.Load(index) fades the overlay in, loads the scene asynchronously with
// allowSceneActivation = false, shows the real progress, and only switches scenes once
// the bar has reached 100%.
public class LoadingScreen : MonoBehaviour
{
    const float MinShowTime = 0.8f;   // so a very fast load doesn't just flash the screen

    static LoadingScreen instance;
    CanvasGroup group;
    RectTransform fill;
    TMP_Text percent;
    bool loading;

    public static void Load(int buildIndex)
    {
        if (instance == null)
        {
            var go = new GameObject("LoadingScreen");
            instance = go.AddComponent<LoadingScreen>();
            instance.Build();
            DontDestroyOnLoad(go);
        }
        instance.Begin(buildIndex);
    }

    void Begin(int buildIndex)
    {
        if (loading) return;
        StartCoroutine(Run(buildIndex));
    }

    IEnumerator Run(int buildIndex)
    {
        loading = true;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SetProgress(0f);
        group.blocksRaycasts = true;

        for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / 0.25f)
        {
            group.alpha = t;
            yield return null;
        }
        group.alpha = 1f;

        var op = SceneManager.LoadSceneAsync(buildIndex);
        op.allowSceneActivation = false;   // hold the new scene back until we say so

        float shown = 0f, elapsed = 0f;
        // async progress stops at 0.9 while activation is blocked, so 0.9 means "fully loaded"
        while (op.progress < 0.9f || shown < 0.999f || elapsed < MinShowTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float real = Mathf.Clamp01(op.progress / 0.9f);
            shown = Mathf.MoveTowards(shown, real, Time.unscaledDeltaTime * 1.2f);
            SetProgress(shown);
            yield return null;
        }

        SetProgress(1f);
        yield return new WaitForSecondsRealtime(0.2f);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
        yield return null;

        for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / 0.3f)
        {
            group.alpha = 1f - t;
            yield return null;
        }
        group.alpha = 0f;
        group.blocksRaycasts = false;
        loading = false;
    }

    void SetProgress(float p)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(p), 1f);
        percent.text = Mathf.RoundToInt(p * 100f) + "%";
    }

    // The whole overlay is built in code so it works from any scene without setup.
    void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        var bg = NewRect("Background", transform);
        Stretch(bg);
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = Neon.BG;

        Label(bg, "LOADING", 100, Neon.Cyan, new Vector2(0, 160), new Vector2(900, 160));

        var track = NewRect("BarFrame", bg);
        Anchor(track, new Vector2(0, -20), new Vector2(760, 50));
        track.gameObject.AddComponent<Image>().color = Neon.Cyan;
        var inner = NewRect("BarBack", track);
        Stretch(inner);
        inner.offsetMin = new Vector2(4, 4);
        inner.offsetMax = new Vector2(-4, -4);
        inner.gameObject.AddComponent<Image>().color = Color.Lerp(Neon.BG, Neon.Cyan, 0.12f);

        fill = NewRect("BarFill", inner);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        fill.gameObject.AddComponent<Image>().color = Neon.Cyan;

        percent = Label(bg, "0%", 52, Neon.White, new Vector2(0, -110), new Vector2(400, 90));
        Label(bg, "SWIPE UP TO JUMP  -  SWIPE DOWN TO SLAM", 30, Neon.Grey, new Vector2(0, -300), new Vector2(900, 80));
    }

    static RectTransform NewRect(string n, Transform parent)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    static void Anchor(RectTransform r, Vector2 pos, Vector2 size)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    static TMP_Text Label(Transform parent, string s, int size, Color col, Vector2 pos, Vector2 sz)
    {
        var r = NewRect("Text", parent);
        Anchor(r, pos, sz);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = s; t.fontSize = size; t.color = col;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }
}
