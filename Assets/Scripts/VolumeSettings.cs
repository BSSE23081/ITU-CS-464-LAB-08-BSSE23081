using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Mixer volumes: slider (0..1) -> decibels, saved in PlayerPrefs.
// The strings must match the exposed parameter names in the mixer.
public static class VolumeSettings
{
    public const string Master = "MasterVolume";
    public const string Music = "MusicVolume";
    public const string Sfx = "SFXVolume";
    public const string Ui = "UIVolume";
    public static readonly string[] All = { Master, Music, Sfx, Ui };

    const float MinDb = -80f;   // the mixer's floor, treated as "muted"
    static AudioMixer mixer;
    static readonly HashSet<string> missing = new HashSet<string>();

    // raised when a mixer parameter can't be set, so AudioManager can change the volume directly instead
    public static event Action<string, float> FallbackVolume;

    public static void Init(AudioMixer m)
    {
        mixer = m;
        missing.Clear();
        if (mixer == null)
            Debug.LogWarning("[Audio] Resources/GameMixer.mixer was not found. Create it as described in the README. Until then the sliders change the volume directly instead of through the mixer.");
        foreach (var key in All) Apply(key, Get(key));
        if (mixer != null && missing.Count > 0)
            Debug.LogWarning("[Audio] These mixer parameters are not exposed (or are misspelled): " + string.Join(", ", missing)
                + ". Expose each group's Volume and rename it exactly. Using direct volume for them meanwhile.");
    }

    public static float Get(string key)
    {
        return PlayerPrefs.GetFloat(key, key == Music ? 0.7f : 1f);
    }

    public static void Set(string key, float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(key, value);
        Apply(key, value);
    }

    public static void Save() { PlayerPrefs.Save(); }

    // Perceived loudness is logarithmic: dB = 20 * log10(v). Half the slider is about -6 dB.
    // log10(0) is -infinity, so anything at or near zero is clamped to -80 dB (silence).
    public static float ToDecibels(float value)
    {
        if (value <= 0.0001f) return MinDb;
        return Mathf.Clamp(Mathf.Log10(value) * 20f, MinDb, 0f);
    }

    static void Apply(string key, float value)
    {
        if (mixer != null && mixer.SetFloat(key, ToDecibels(value))) return;
        missing.Add(key);
        FallbackVolume?.Invoke(key, value);
    }
}
