using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum UISound { Hover, Click, Select, Back, Open, Close, Pause, Resume, Success, Error }
public enum Sfx { Jump, Coin, Hit, Death, Win, Lose }
public enum MusicTrack { Menu, Gameplay, Results }

// One persistent audio hub. It creates itself before the first scene loads, so no scene
// needs an AudioManager object. Every AudioSource it owns is routed to a mixer group:
//   music (x2, crossfade) -> Master/Music, sfx pool -> Master/SFX, ui -> Master/UI.
// Needs Resources/GameMixer.mixer and the clips in Resources/Audio.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const int SfxVoices = 6;
    const float PausedMusicLevel = 0.35f;

    AudioMixer mixer;
    AudioSource uiSource;
    AudioSource[] sfxSources;
    AudioSource[] musicSources;
    readonly float[] fadeLevel = new float[2];
    readonly float[] fadeTarget = new float[2];
    readonly float[] fadeSpeed = new float[2];
    int activeMusic;
    int sfxIndex;
    string currentTrack;
    float duck = 1f, duckTarget = 1f;
    float fallbackMusic = 1f;   // only used if the mixer parameter is missing

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (Instance != null) return;
        new GameObject("AudioManager").AddComponent<AudioManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        mixer = Resources.Load<AudioMixer>("GameMixer");
        if (mixer == null)
            Debug.LogWarning("AudioManager: Resources/GameMixer.mixer not found. Audio will play but the volume sliders won't do anything.");

        var music = Group("Master/Music");
        var sfx = Group("Master/SFX");
        var ui = Group("Master/UI");

        // Music and UI ignore AudioListener.pause, so they keep playing while the game is paused.
        // Gameplay SFX do not ignore it, so they freeze when the game pauses.
        musicSources = new[] { Make("Music A", music, true, true), Make("Music B", music, true, true) };
        uiSource = Make("UI", ui, true, false);
        sfxSources = new AudioSource[SfxVoices];
        for (int i = 0; i < SfxVoices; i++) sfxSources[i] = Make("SFX " + i, sfx, false, false);
    }

    void Start()
    {
        VolumeSettings.Init(mixer);   // applied in Start: setting mixer params in Awake can be ignored
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameFlow.PauseChanged += OnPauseChanged;
        VolumeSettings.FallbackVolume += OnFallbackVolume;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameFlow.PauseChanged -= OnPauseChanged;
        VolumeSettings.FallbackVolume -= OnFallbackVolume;
    }

    // Used only when the mixer (or one of its exposed parameters) is missing.
    void OnFallbackVolume(string key, float value)
    {
        if (key == VolumeSettings.Master) AudioListener.volume = value;
        else if (key == VolumeSettings.Music) fallbackMusic = value;
        else if (key == VolumeSettings.Sfx) { foreach (var s in sfxSources) s.volume = value; }
        else if (key == VolumeSettings.Ui) uiSource.volume = value;
    }

    AudioMixerGroup Group(string path)
    {
        if (mixer == null) return null;
        var found = mixer.FindMatchingGroups(path);
        return found != null && found.Length > 0 ? found[0] : null;
    }

    AudioSource Make(string n, AudioMixerGroup group, bool ignoreListenerPause, bool loop)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform);
        var s = go.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.spatialBlend = 0f;
        s.outputAudioMixerGroup = group;
        s.ignoreListenerPause = ignoreListenerPause;
        return s;
    }

    AudioClip Load(string clipName)
    {
        var clip = Resources.Load<AudioClip>("Audio/" + clipName);
        if (clip == null) Debug.LogWarning("AudioManager: missing clip Resources/Audio/" + clipName);
        return clip;
    }

    // ---------------------------------------------------------------- public API

    public static void PlayUI(UISound sound)
    {
        if (Instance != null) Instance.DoUI(sound);
    }

    public static void PlaySfx(Sfx sfx, float volume = 1f)
    {
        if (Instance != null) Instance.DoSfx(sfx, volume);
    }

    public static void PlayMusic(MusicTrack track, float fadeTime = 1f)
    {
        if (Instance != null) Instance.DoMusic(track, fadeTime);
    }

    // ---------------------------------------------------------------- implementation

    void DoUI(UISound sound)
    {
        var clip = Load("ui_" + sound.ToString().ToLower());
        if (clip == null) return;
        float vol = sound == UISound.Hover ? 0.5f : sound == UISound.Open ? 0.6f : 1f;
        uiSource.PlayOneShot(clip, vol);
    }

    void DoSfx(Sfx sfx, float volume)
    {
        var clip = Load("sfx_" + sfx.ToString().ToLower());
        if (clip == null) return;
        // small random pitch so repeated sounds (coins, hits) don't feel machine-gunned.
        // The pitch belongs to the source, so a round-robin pool keeps overlaps mostly independent.
        var src = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxSources.Length;
        src.pitch = 1f + Random.Range(-0.06f, 0.06f);
        src.PlayOneShot(clip, volume);
    }

    void DoMusic(MusicTrack track, float fadeTime)
    {
        string clipName = "music_" + track.ToString().ToLower();
        if (clipName == currentTrack) return;
        var clip = Load(clipName);
        if (clip == null) return;
        currentTrack = clipName;
        fadeTime = Mathf.Max(0.05f, fadeTime);

        int old = activeMusic;
        activeMusic = 1 - activeMusic;

        fadeTarget[old] = 0f;
        fadeSpeed[old] = 1f / fadeTime;

        var s = musicSources[activeMusic];
        s.clip = clip;
        s.volume = 0f;
        s.Play();
        fadeLevel[activeMusic] = 0f;
        fadeTarget[activeMusic] = 1f;
        fadeSpeed[activeMusic] = 1f / fadeTime;
    }

    void Update()
    {
        // crossfade + pause ducking, all on unscaled time so it still works with timeScale = 0
        float dt = Time.unscaledDeltaTime;
        duck = Mathf.MoveTowards(duck, duckTarget, dt * 3f);
        for (int i = 0; i < 2; i++)
        {
            fadeLevel[i] = Mathf.MoveTowards(fadeLevel[i], fadeTarget[i], fadeSpeed[i] * dt);
            musicSources[i].volume = fadeLevel[i] * duck * fallbackMusic;
            if (fadeTarget[i] <= 0f && fadeLevel[i] <= 0f && musicSources[i].isPlaying)
                musicSources[i].Stop();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioListener.pause = false;
        duckTarget = 1f;
        if (scene.buildIndex == 0) DoMusic(MusicTrack.Menu, 1f);
    }

    void OnPauseChanged(bool paused)
    {
        AudioListener.pause = paused;
        duckTarget = paused ? PausedMusicLevel : 1f;
        DoUI(paused ? UISound.Pause : UISound.Resume);
    }
}
