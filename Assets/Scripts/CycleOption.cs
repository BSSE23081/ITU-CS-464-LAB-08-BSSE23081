using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Loadout card: tap to cycle options, value text pops to confirm the change.
public class CycleOption : MonoBehaviour
{
    public Button button;
    public TMP_Text value;
    public string[] options;
    public Image tint;
    public Color[] tints;
    int index;
    Coroutine co;

    void Awake()
    {
        button.onClick.AddListener(Next);
        Apply();
    }

    void Next()
    {
        index = (index + 1) % options.Length;
        AudioManager.PlayUI(UISound.Select);
        Apply();
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Pop());
    }

    void Apply()
    {
        value.text = options[index];
        if (tint != null && tints != null && tints.Length > index) tint.color = tints[index];
    }

    IEnumerator Pop()
    {
        Transform t = value.transform;
        for (float x = 0; x < 1f; x += Time.unscaledDeltaTime / 0.25f)
        {
            t.localScale = Vector3.one * (1f + 0.25f * Mathf.Sin(x * Mathf.PI) * (1f - x));
            yield return null;
        }
        t.localScale = Vector3.one;
    }
}
