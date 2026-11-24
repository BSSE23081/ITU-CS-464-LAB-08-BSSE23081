using UnityEngine;

// Listener: music follows the state of the run.
//   menu -> gameplay when the run starts, gameplay -> results when it ends.
public class GameMusicListener : MonoBehaviour
{
    public GameController game;

    void OnEnable()
    {
        if (game == null) return;
        game.GameStarted += OnStarted;
        game.GameWon += OnEnded;
        game.GameLost += OnEnded;
    }

    void OnDisable()
    {
        if (game == null) return;
        game.GameStarted -= OnStarted;
        game.GameWon -= OnEnded;
        game.GameLost -= OnEnded;
    }

    void OnStarted() { AudioManager.PlayMusic(MusicTrack.Gameplay, 1.2f); }
    void OnEnded()   { AudioManager.PlayMusic(MusicTrack.Results, 1.5f); }
}
