using UnityEngine;

// Targets for menu buttons (used through the button onClick events).
public class MenuActions : MonoBehaviour
{
    public void SelectEndless()   { GameSession.Mode = GameMode.Endless; }
    public void SelectTimeTrial() { GameSession.Mode = GameMode.TimeTrial; }
    public void StartRun()        { LoadingScreen.Load(GameFlow.GameSceneIndex); }
}
