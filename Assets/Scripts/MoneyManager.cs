using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{

    public static Func<GameManager.ItemRarity, bool> canAffordItem;
    public static Func<int, bool> canAffordPurchase;
    public static int[] costRarityTable = { 5, 10, 15, 20 };
    
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private int _startingMoney = 100;
    private int _money;
    void Start()
    {
        _money = _startingMoney;
        _moneyText.text = "$" + _money;
        GameManager.purchaseItem += Purchase;
        Order.onOrderComplete += SuccessfulOrder;
        Refresh.onRefresh += RemoveMoney;
        Item.onStoreItem += Purchase;
        GameManager.addMoney += AddMoney;

        canAffordItem += CanAfford;
        canAffordPurchase += CanAfford;

    }
    
    // Purchase an Item.
    // Cost is determined by the costRarityTable
    private void Purchase(GameManager.ItemRarity rarity)
    {
        int toSpend = costRarityTable[(int)rarity];
        RemoveMoney(toSpend);
    }
    // Add money upon successful order based on the rarities needed to complete and the time remaining
    private void SuccessfulOrder(Order order)
    {
        float timePercentRemaining = order.GetTimeRemaining() / order.GetMaxTime();
        int toAdd = Mathf.CeilToInt(order.GetOrderSum() * GameStatsManager.GetTimeBonus(timePercentRemaining));
        _money += toAdd;
        _moneyText.text = "$" + _money;
        GameStatsManager.onMoneyEarn?.Invoke(toAdd);
    }
    // Add money to the player's total and update the text
    private void AddMoney(int amount)
    {
        _money += amount;
        _moneyText.text = "$" + _money;
        GameStatsManager.onMoneyEarn?.Invoke(amount);
    }
    // Remove money to the player's total and update the text
    private void RemoveMoney(int amount)
    {
        _money -= amount;
        _moneyText.text = "$" + _money;
        GameStatsManager.onMoneySpend?.Invoke(amount);
    }
    
    // Check if the player can afford to purchase something
    private bool CanAfford(int amount)
    {
        return _money >= amount;
    }
    // Check if the player can afford to purchase an Item
    private bool CanAfford(GameManager.ItemRarity rarity)
    {
        return CanAfford(costRarityTable[(int) rarity]);
    }
    
    
    
}
