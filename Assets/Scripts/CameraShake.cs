using System.Collections;
using UnityEngine;

// Listener: shakes the camera when the player is hurt. Uses unscaled time.
public class CameraShake : MonoBehaviour
{
    public PlayerHealth health;

    Vector3 home;
    Coroutine running;

    void Awake() { home = transform.localPosition; }

    void OnEnable()
    {
        if (health == null) return;
        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    void OnDisable()
    {
        if (health == null) return;
        health.Damaged -= OnDamaged;
        health.Died -= OnDied;
    }

    void OnDamaged(int cur, int max) { Shake(0.18f, 0.25f); }
    void OnDied()                    { Shake(0.4f, 0.45f); }

    void Shake(float magnitude, float duration)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Run(magnitude, duration));
    }

    IEnumerator Run(float magnitude, float duration)
    {
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            float fall = 1f - t / duration;
            Vector2 o = Random.insideUnitCircle * magnitude * fall;
            transform.localPosition = home + new Vector3(o.x, o.y, 0f);
            yield return null;
        }
        transform.localPosition = home;
        running = null;
    }
}
