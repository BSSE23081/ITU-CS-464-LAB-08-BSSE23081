using TMPro;
using UnityEngine;

// A menu label that shows saved player data instead of fixed text.
public class LiveText : MonoBehaviour
{
    public enum Kind { HighAndCoins, ShopCoins, Distance, Coins }
    public Kind kind;

    void OnEnable()
    {
        var t = GetComponent<TMP_Text>();
        if (t == null) return;
        switch (kind)
        {
            case Kind.HighAndCoins: t.text = "HIGH " + PlayerData.BestDistance + "M   |   COINS " + PlayerData.Coins; break;
            case Kind.ShopCoins:    t.text = "COINS " + PlayerData.Coins; break;
            case Kind.Distance:     t.text = PlayerData.BestDistance + "M"; break;
            case Kind.Coins:        t.text = PlayerData.Coins.ToString(); break;
        }
    }
}
