using System.Collections;
using UnityEngine;

// Listener: turns gameplay events into sound effects.
public class GameSfxListener : MonoBehaviour
{
    public PlayerController player;
    public PlayerHealth health;
    public CoinWallet wallet;
    public GameController game;

    void OnEnable()
    {
        if (player != null) player.Jumped += OnJumped;
        if (wallet != null) wallet.Collected += OnCollected;
        if (health != null) { health.Damaged += OnDamaged; health.Healed += OnHealed; health.Died += OnDied; }
        if (game != null) { game.GameWon += OnWon; game.GameLost += OnLost; }
    }

    void OnDisable()
    {
        if (player != null) player.Jumped -= OnJumped;
        if (wallet != null) wallet.Collected -= OnCollected;
        if (health != null) { health.Damaged -= OnDamaged; health.Healed -= OnHealed; health.Died -= OnDied; }
        if (game != null) { game.GameWon -= OnWon; game.GameLost -= OnLost; }
    }

    void OnJumped()                      { AudioManager.PlaySfx(Sfx.Jump, 0.8f); }
    void OnCollected(int total)          { AudioManager.PlaySfx(Sfx.Coin); }
    void OnDamaged(int cur, int max)     { AudioManager.PlaySfx(Sfx.Hit); }
    void OnHealed(int cur, int max)      { AudioManager.PlaySfx(Sfx.Coin, 0.9f); }
    void OnDied()                        { AudioManager.PlaySfx(Sfx.Death); }
    void OnWon()                         { AudioManager.PlaySfx(Sfx.Win); }
    void OnLost()                        { StartCoroutine(LoseAfterDeath()); }

    IEnumerator LoseAfterDeath()
    {
        yield return new WaitForSeconds(0.7f);   // let the death sound breathe first
        AudioManager.PlaySfx(Sfx.Lose);
    }
}
