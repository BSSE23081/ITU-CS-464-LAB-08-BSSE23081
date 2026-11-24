using System;
using UnityEngine;

// Owns the state of a run and publishes GameStarted / GameWon / GameLost.
// It also listens to PlayerHealth.Died to turn a death into GameLost.
//   Endless:    no goal, the run ends when you die.
//   Time Trial: survive for timeLimit seconds to win.
public class GameController : MonoBehaviour
{
    public PlayerHealth health;
    public float timeLimit = 60f;
    public float rampDistance = 250f;    // distance over which the speed rises from start to end
    public float startSpeed = 6f, endSpeed = 9.5f;

    public GameMode Mode { get { return GameSession.Mode; } }
    public float Distance { get; private set; }
    public float Elapsed { get; private set; }
    public bool Running { get; private set; }
    public float TimeLeft { get { return Mathf.Max(0f, timeLimit - Elapsed); } }
    public float Speed { get { return Mathf.Lerp(startSpeed, endSpeed, Mathf.Clamp01(Distance / rampDistance)); } }
    string BestKey { get { return "BestDistance_" + Mode; } }
    public float BestDistance { get { return PlayerPrefs.GetFloat(BestKey, 0f); } }

    public event Action GameStarted;
    public event Action GameWon;
    public event Action GameLost;

    void OnEnable()  { if (health != null) health.Died += OnPlayerDied; }
    void OnDisable() { if (health != null) health.Died -= OnPlayerDied; }

    System.Collections.IEnumerator Start()
    {
        yield return null;   // let every listener finish subscribing
        Running = true;
        GameStarted?.Invoke();
    }

    void Update()
    {
        if (!Running) return;
        Elapsed += Time.deltaTime;
        Distance += Speed * Time.deltaTime;
        if (Mode == GameMode.TimeTrial && Elapsed >= timeLimit)
        {
            Elapsed = timeLimit;
            Finish();
            GameWon?.Invoke();
        }
    }

    void OnPlayerDied()
    {
        if (!Running) return;
        Finish();
        GameLost?.Invoke();
    }

    void Finish()
    {
        Running = false;
        PlayerData.SubmitDistance(Distance);
        PlayerData.Save();
        if (Distance > BestDistance)
        {
            PlayerPrefs.SetFloat(BestKey, Distance);
            PlayerPrefs.Save();
        }
    }
}
