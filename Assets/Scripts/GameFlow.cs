using System;
using UnityEngine;

// Pause / resume / retry / back to menu for the gameplay scene. Raises PauseChanged,
// which AudioManager listens to (ducks the music, pauses SFX, plays the pause sounds).
public class GameFlow : MonoBehaviour
{
    public const int MenuSceneIndex = 0;
    public const int GameSceneIndex = 1;

    public static event Action<bool> PauseChanged;

    public ScreenManager sm;
    public GameController game;

    bool paused;

    void Update()
    {
        if (!GameInput.BackPressed()) return;
        if (!paused) Pause();
        else if (sm.Current == "Pause") Resume();
        else sm.Back();   // from Settings back to the pause menu
    }

    public void Pause()
    {
        if (paused || sm.Busy || game == null || !game.Running) return;
        SetPaused(true);
        sm.Go("Pause");
    }

    public void Resume()
    {
        if (!paused || sm.Busy) return;
        SetPaused(false);
        sm.Back();
    }

    public void Retry()  { Leave(); LoadingScreen.Load(GameSceneIndex); }
    public void ToMenu() { Leave(); LoadingScreen.Load(MenuSceneIndex); }

    void Leave()
    {
        PlayerData.Save();
        if (paused) SetPaused(false);
    }

    void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = value ? 0f : 1f;
        PauseChanged?.Invoke(value);
    }
}
