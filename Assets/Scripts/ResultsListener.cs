using System.Collections;
using TMPro;
using UnityEngine;

// Listener: when the run ends, fills in the Results screen and shows it.
public class ResultsListener : MonoBehaviour
{
    public GameController game;
    public CoinWallet wallet;
    public ScreenManager sm;
    public TMP_Text title, distance, coins, best;

    void OnEnable()
    {
        if (game == null) return;
        game.GameWon += OnWon;
        game.GameLost += OnLost;
    }

    void OnDisable()
    {
        if (game == null) return;
        game.GameWon -= OnWon;
        game.GameLost -= OnLost;
    }

    void OnWon()  { StartCoroutine(Show(true)); }
    void OnLost() { StartCoroutine(Show(false)); }

    IEnumerator Show(bool won)
    {
        yield return new WaitForSeconds(won ? 0.6f : 1.0f);
        title.text = won ? "YOU WIN" : "RUN OVER";
        title.color = won ? Neon.Gold : Neon.Red;
        distance.text = Mathf.FloorToInt(game.Distance) + " M";
        coins.text = wallet.Coins.ToString();
        best.text = Mathf.FloorToInt(game.BestDistance) + " M";
        sm.GoRoot("Results");
    }
}
