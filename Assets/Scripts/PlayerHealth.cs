using System;
using UnityEngine;

// Publisher. It only changes health and announces what happened; it knows nothing about
// the HUD, sounds, camera or particles. Those are listeners (HudListener, GameSfxListener,
// CameraShake, HitFlash, GameController).
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float invulnerableTime = 1f;

    public int Current { get; private set; }
    public bool IsDead { get { return Current <= 0; } }

    public event Action<int, int> Damaged;   // (current, max)
    public event Action<int, int> Healed;    // (current, max)
    public event Action Died;

    float invulnerableUntil;

    void Awake() { Current = maxHealth; }

    // returns true if the hit counted (false while dead or in the i-frames after a hit)
    public bool TakeDamage(int amount)
    {
        if (IsDead || Time.time < invulnerableUntil) return false;
        Current = Mathf.Max(0, Current - amount);
        invulnerableUntil = Time.time + invulnerableTime;
        Damaged?.Invoke(Current, maxHealth);
        if (Current == 0) Died?.Invoke();
        return true;
    }

    public void Heal(int amount)
    {
        if (IsDead || Current >= maxHealth) return;
        Current = Mathf.Min(maxHealth, Current + amount);
        Healed?.Invoke(Current, maxHealth);
    }
}
