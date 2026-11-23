using UnityEngine;

// Saved totals that carry across runs and scenes (PlayerPrefs).
public static class PlayerData
{
    const string CoinsKey = "TotalCoins";
    const string BestKey = "BestDistanceOverall";

    public static int Coins
    {
        get { return PlayerPrefs.GetInt(CoinsKey, 0); }
        set { PlayerPrefs.SetInt(CoinsKey, Mathf.Max(0, value)); }
    }

    public static int BestDistance
    {
        get { return Mathf.FloorToInt(PlayerPrefs.GetFloat(BestKey, 0f)); }
    }

    public static void SubmitDistance(float meters)
    {
        if (meters > PlayerPrefs.GetFloat(BestKey, 0f)) PlayerPrefs.SetFloat(BestKey, meters);
    }

    public static void Save() { PlayerPrefs.Save(); }
}
