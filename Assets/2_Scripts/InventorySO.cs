using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class InventorySO : ScriptableObject
{
    public int NumberOfCoins;
    public int NumberOfKeys;
    public UnityAction OnNumberChange;
    private void OnEnable()
    {
        NumberOfCoins = 0;
        NumberOfKeys = 0;
    }
    public void AddCoin(int value)
    {
        NumberOfCoins += value;
        OnNumberChange?.Invoke();
    }
    public void AddKey(int value)
    {
        NumberOfKeys += value;
        OnNumberChange?.Invoke();
    }
    public int PayKey(int value)
    {
        int paidValue = 0;
        if (NumberOfKeys > value)
        {
            paidValue = value;
            AddKey(-value);
        }
        else
        {
            paidValue = NumberOfKeys;
            AddKey(-NumberOfKeys);
        }

        return paidValue;
    }
}
