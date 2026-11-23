public enum GameMode { Endless, TimeTrial }

// Carries the mode picked on the Mode Select screen into the gameplay scene.
public static class GameSession
{
    public static GameMode Mode = GameMode.Endless;
}
