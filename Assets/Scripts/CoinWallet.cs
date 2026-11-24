using System;
using UnityEngine;

public class CoinWallet : MonoBehaviour
{
    public int Coins { get; private set; }

    public event Action<int> Collected;   // total coins after the pickup

    public void Add(int amount)
    {
        Coins += amount;          // coins in this run
        PlayerData.Coins += amount;   // and in the saved total
        Collected?.Invoke(Coins);
    }
}
