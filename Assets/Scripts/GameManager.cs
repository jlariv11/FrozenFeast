using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public const int ItemCount = 4;
    
    public enum ItemRarity
    {
        COMMON,
        UNCOMMON,
        RARE,
        LEGENDARY
    }
    
    public static Action<ItemRarity> purchaseItem;
    public static Action<int> addMoney;
    public static int nextItemIndex = 0;
    
    [SerializeField] private TextMeshProUGUI _timerText;

    [SerializeField] private Image _timerBar;

    [SerializeField] private GameObject _orderHolder;
    [SerializeField] private GameObject _orderPrefab;
    
    [SerializeField] private float _maxGameTime = 300; // Seconds
    [SerializeField] private int _maxOrders = 6;

    private float _currentGameTime;
    private List<Order> _orders;
    private int _nextOrderIndex;
    
    void Start()
    {
        _currentGameTime = _maxGameTime;
        _nextOrderIndex = 0;
        _orders = new List<Order>();
        Item.onSegmentComplete += TryCompleteOrder;
        Order.onOrderElapse += (order) => _orders.Remove(order);
        Order.onOrderComplete += RemoveAndRenewOrder;
        // Create 3 orders for the start of the game
        for (int i = 0; i < 3; i++)
        {
            CreateOrder();
        }
        StartCoroutine(CreateOrderPassive());
        StartCoroutine(MoneyGenerationPassive());
    }
    
    // Update the game clock and end the game when time is up
    void Update()
    {
        _currentGameTime -= Time.deltaTime;
        _timerBar.fillAmount = _currentGameTime / _maxGameTime;
        _timerText.text = GameTimeToFormattedString();
        if (_currentGameTime <= 0)
        {
            GameOver();
        }
    }

    // Unsubscribe from game events and load Game Over scene
    private void GameOver()
    {
        Item.onSegmentComplete -= TryCompleteOrder;
        Order.onOrderComplete -= RemoveAndRenewOrder;
        Order.onOrderElapse -= (order) => _orders.Remove(order);
        SceneManager.LoadScene("GameOver", LoadSceneMode.Single);

    }

    // Passively create new orders every 10 seconds
    private IEnumerator CreateOrderPassive()
    {
        while (true)
        {
            CreateOrder();
            yield return new WaitForSeconds(10.0f);
        }
    }

    // Passively generate $5 every 5 seconds
    private IEnumerator MoneyGenerationPassive()
    {
        while (true)
        {
            addMoney?.Invoke(5);
            yield return new WaitForSeconds(5.0f);
        }
    }

    // Add an order to the order bar as long as it isn't full
    private void CreateOrder()
    {
        if (_orders.Count < _maxOrders)
        {
            Order order = Instantiate(_orderPrefab, _orderHolder.transform).GetComponent<Order>();
            order._orderID = _nextOrderIndex++;
            _orders.Add(order);
        }
    }
    // Format the game time to mins:secs format
    private string GameTimeToFormattedString()
    {
        int mins = Mathf.FloorToInt(_currentGameTime / 60);
        int secs = (int)_currentGameTime % 60;
        return string.Format("{0:0}:{1:00}", mins, secs);
    }

    // Handle the purchasing of items to complete orders
    private void TryCompleteOrder(Item item, int orderToComplete)
    {
        // Find the order with the least amount of time remaining
        Order closestOrder = null;
        float closestTime = float.MaxValue;
        // Need to find the order with the least time remaining that needs the given item
        if (orderToComplete == -1)
        {
            foreach (Order order in _orders)
            {
                if (order.GetSegments().Contains(item.GetRarity()))
                {
                    if (order.GetTimeRemaining() < closestTime)
                    {
                        closestOrder = order;
                        closestTime = order.GetTimeRemaining();
                    }
                }
            }
        }
        // Make sure the specified order needs the item and if so, apply it
        else
        {
            if (_orders[orderToComplete].GetSegments().Contains(item.GetRarity()))
            {
                closestOrder = _orders[orderToComplete];
            }
        }

        // If found, check if the player can afford the item and attempt to complete the segment
        if (closestOrder != null)
        {
            if (MoneyManager.canAffordItem?.Invoke(item.GetRarity()) == true)
            {
                closestOrder.CompleteSegment(item.GetRarity(), item.GetID());
                // Stored items are already paid for, so make sure the player doesn't pay twice
                if (!item.IsStored())
                {
                    purchaseItem?.Invoke(item.GetRarity());
                }

                if (_orders.Count == 0)
                {
                    CreateOrder();
                }
            }
        }
    }

    // Remove the order from the list and have a 50% chance for a new order to replace it
    private void RemoveAndRenewOrder(Order order)
    {
        _orders.Remove(order);
        if (Random.Range(0, 2) == 0)
        {
            CreateOrder();
        }
    }
    
    // Get the color an item should be based on it's rarity
    public static Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case GameManager.ItemRarity.COMMON:
                return Color.blue;
            case GameManager.ItemRarity.UNCOMMON:
                return Color.green;
            case GameManager.ItemRarity.RARE:
                return Color.yellow;
            case GameManager.ItemRarity.LEGENDARY:
                return Color.magenta;
            default:
                return Color.blue;
        }
    }
}
