using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatsManager : MonoBehaviour
{
    public static Action<int> onMoneySpend; 
    public static Action<int> onMoneyEarn;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private int[] rarityScoreTable = { 100, 200, 300, 400};
    
    private int _score;
    private int _ordersCompleted;
    private int _ordersMissed;
    private int[] _itemsSubmitted;
    private int _moneySpent;
    private int _moneyEarned;
    
    private static GameStatsManager _smSingleton;
    
    // Make sure this GO persists between scenes so the stat data can be accessed on the Game Over Screen
    private void Awake()
    {
        if (_smSingleton != null && _smSingleton != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += HandleSceneChange;

    }

    // Handle when switching scenes
    private void HandleSceneChange(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe from all events
        Order.onOrderComplete -= UpdateScore;
        Order.onOrderComplete -= UpdateOrdersCompleted;
        Order.onOrderElapse -= UpdateOrdersMissed;
        onMoneyEarn -= UpdateMoneyEarned;
        onMoneySpend -= UpdateMoneySpent;
        
        // If in the Game Over scene, display the player's stats
        if (scene.name == "GameOver")
        {
            Transform statsHolder = GameObject.Find("StatsHolder").transform;
            statsHolder.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Score: " + _score;
            statsHolder.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Orders Completed: " + _ordersCompleted;
            statsHolder.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Orders Missed: " + _ordersMissed;
            statsHolder.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Money Earned: " + _moneyEarned;
            statsHolder.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Money Spent: " + _moneySpent;
            statsHolder.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Profit: " + (_moneyEarned - (_moneySpent + 100));
        }
        // If switching to the Game Scene, reset all stats
        else if(scene.name == "GameScene")
        {
            Start();
        }
    }

    void Start()
    {
        _score = 0;
        _ordersCompleted = 0;
        _ordersMissed = 0;
        _itemsSubmitted = new int[]{0, 0, 0, 0};
        Order.onOrderComplete += UpdateScore;
        Order.onOrderComplete += UpdateOrdersCompleted;
        Order.onOrderElapse += UpdateOrdersMissed;
        onMoneyEarn += UpdateMoneyEarned;
        onMoneySpend += UpdateMoneySpent;
    }

    private void UpdateMoneyEarned(int amt)
    {
        _moneyEarned += amt;
    }
    private void UpdateMoneySpent(int amt)
    {
        _moneySpent += amt;
    }

    private void UpdateOrdersMissed(Order order)
    {
        _ordersMissed++;
    }

    private void UpdateOrdersCompleted(Order order)
    {
        _ordersCompleted++;
    }

    // Update the score based on the sum value of all segments and modified by a time bonus
    private void UpdateScore(Order order)
    {
        float timePercentRemaining = order.GetTimeRemaining() / order.GetMaxTime();
        _score += Mathf.CeilToInt(order.GetOrderSum() * GetTimeBonus(timePercentRemaining));
        _scoreText.text = "Score: " + _score;
    }
    
    // Get the time bonus based on the remaining percent of time left
    public static float GetTimeBonus(float remainingPercent)
    {
        if (remainingPercent >= 0.75)
            return 1.5f;
        if (remainingPercent >= 0.50)
            return 1.25f;
        return 1.0f;
    }
}
