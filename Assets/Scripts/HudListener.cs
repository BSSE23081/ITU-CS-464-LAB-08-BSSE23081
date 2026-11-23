using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Listener: keeps the HUD in sync with health and coins through events.
// Distance / time are read every frame because they change continuously.
public class HudListener : MonoBehaviour
{
    public PlayerHealth health;
    public CoinWallet wallet;
    public GameController game;
    public Image[] hearts;
    public TMP_Text coinsText, distanceText;
    public RectTransform distanceFill;

    void OnEnable()
    {
        if (health != null) { health.Damaged += OnHealthChanged; health.Healed += OnHealthChanged; }
        if (wallet != null) wallet.Collected += OnCoins;
    }

    void OnDisable()
    {
        if (health != null) { health.Damaged -= OnHealthChanged; health.Healed -= OnHealthChanged; }
        if (wallet != null) wallet.Collected -= OnCoins;
    }

    void Start()
    {
        OnHealthChanged(health.Current, health.maxHealth);
        OnCoins(wallet.Coins);
        // Endless has no goal, so no progress bar
        distanceFill.transform.parent.gameObject.SetActive(game.Mode == GameMode.TimeTrial);
    }

    void Update()
    {
        int meters = Mathf.FloorToInt(game.Distance);
        if (game.Mode == GameMode.TimeTrial)
        {
            distanceFill.anchorMax = new Vector2(Mathf.Clamp01(game.Elapsed / game.timeLimit), 1f);
            distanceText.text = "TIME " + Mathf.CeilToInt(game.TimeLeft) + "     " + meters + " M";
        }
        else
        {
            distanceText.text = meters + " M";
        }
    }

    void OnHealthChanged(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].color = i < current ? Neon.Pink : Neon.Dim;
    }

    void OnCoins(int total) { coinsText.text = "COINS " + total; }
}
