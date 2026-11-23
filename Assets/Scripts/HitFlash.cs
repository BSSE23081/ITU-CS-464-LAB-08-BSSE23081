using System.Collections;
using UnityEngine;

// Listener: flashes the player white, then blinks through the invulnerability window.
public class HitFlash : MonoBehaviour
{
    public PlayerHealth health;
    SpriteRenderer[] renderers;   // the player's sprites, built at runtime by PlayerController
    Color[] original;
    Coroutine running;

    void Start()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        original = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) original[i] = renderers[i].color;
    }

    void OnEnable()  { if (health != null) health.Damaged += OnDamaged; }
    void OnDisable() { if (health != null) health.Damaged -= OnDamaged; }

    void OnDamaged(int cur, int max)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        Tint(Color.white, 1f);
        yield return new WaitForSeconds(0.08f);
        float end = Time.time + health.invulnerableTime;
        bool faded = true;
        while (Time.time < end)
        {
            Restore(faded ? 0.3f : 1f);
            faded = !faded;
            yield return new WaitForSeconds(0.08f);
        }
        Restore(1f);
        running = null;
    }

    void Tint(Color c, float alpha)
    {
        foreach (var r in renderers) r.color = new Color(c.r, c.g, c.b, alpha);
    }

    void Restore(float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var c = original[i];
            renderers[i].color = new Color(c.r, c.g, c.b, c.a * alpha);
        }
    }
}
