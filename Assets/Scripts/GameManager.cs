using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public enum ItemType
    {
        SEAWEED,
        KRILL,
        SARDINE,
        SQUID
    }
    
    public static Action<ItemType> purchaseItem;
    public static Action<int> addMoney;
    public static Action onOrderCreated;
    public static int nextItemIndex = 0;
    public static int MaxOrders = 6;
    
    [SerializeField] private TextMeshProUGUI _timerText;

    [SerializeField] private Image _timerBar;

    [SerializeField] private GameObject _orderHolder;
    [SerializeField] private GameObject _orderPrefab;
    
    [SerializeField] private float _maxGameTime = 300; // Seconds
    [SerializeField] private Transform _itemHolder;

    private float _currentGameTime;
    private List<Order> _orders;
    private int _nextOrderIndex;
    private static Sprite[] _itemSprites;
    private bool _gameStarted;
    private static bool _paused;
    [SerializeField] private GameObject _pauseMenu;

    private IEnumerator _createOrderPassive;
    private IEnumerator _createMoneyPassive;

    private void Awake()
    {
        _gameStarted = false;
        _paused = false;
    }

    void Start()
    {
        _currentGameTime = _maxGameTime;
        _nextOrderIndex = 0;
        _orders = new List<Order>();
        _createOrderPassive = CreateOrderPassive();
        _createMoneyPassive = MoneyGenerationPassive();
        StartCoroutine(PrepareGame());
    }

    private IEnumerator PrepareGame()
    {
        // Make sure assets are loaded before the game starts
        yield return LoadResources();
        StartGame();
    }
    private void StartGame()
    {
        Item.onSegmentComplete += TryCompleteOrder;
        Order.onOrderElapse += ElapseOrder;
        Order.onOrderComplete += RemoveAndRenewOrder;
        // Create 3 orders for the start of the game
        for (int i = 0; i < 3; i++)
        {
            CreateOrder();
        }
        // Initialize the items
        for (int i = 0; i < _itemHolder.childCount; i++)
        {
            _itemHolder.GetChild(i).gameObject.GetComponent<Item>().RollRarity();
        }
        StartCoroutine(_createOrderPassive);
        StartCoroutine(_createMoneyPassive);
        _gameStarted = true;
    }
    
    private IEnumerator LoadResources()
    {
        _itemSprites = new Sprite[Enum.GetNames(typeof(ItemType)).Length];
        // Load the item sprites
        for (int i = 0; i < _itemSprites.Length; i++)
        {
            _itemSprites[i] = Resources.Load<Sprite>("Sprites/" + Enum.GetName(typeof(ItemType), i));
        }

        yield return null;
    }
    
    // Update the game clock and end the game when time is up
    void Update()
    {
        if (!_gameStarted || _paused)
            return;
        _currentGameTime -= Time.deltaTime;
        _timerBar.fillAmount = _currentGameTime / _maxGameTime;
        _timerText.text = GameTimeToFormattedString();
        if (_currentGameTime <= 0)
        {
            GameOver();
        }
    }
    
    private void GameOver()
    {
        SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
    }


    // Unsubscribe from all events and reset parameters
    private void CleanUpGame()
    {
        Item.onSegmentComplete -= TryCompleteOrder;
        Order.onOrderComplete -= RemoveAndRenewOrder;
        Order.onOrderElapse -= ElapseOrder;
        _gameStarted = false;
        nextItemIndex = 0;
        StopCoroutine(_createOrderPassive);
        StopCoroutine(_createMoneyPassive);
        foreach (Order o in _orders)
        {
            if (o != null)
            {
                Destroy(o.gameObject);
            }
        }
    }
    
    // Handle when the player quits during the game or the game ends
    private void OnDestroy()
    {
        CleanUpGame();
    }

    private void ElapseOrder(Order order)
    {
        _orders.Remove(order);
    }

    public void OnPause()
    {
        if (!_gameStarted)
            return;
        _paused = !_paused;
        _pauseMenu.SetActive(_paused);
        if (_paused)
        {
            Item.onSegmentComplete -= TryCompleteOrder;
            Order.onOrderComplete -= RemoveAndRenewOrder;
            Order.onOrderElapse -= (order) => _orders.Remove(order);
            StopCoroutine(_createOrderPassive);
            StopCoroutine(_createMoneyPassive);
        }
        else
        {
            Item.onSegmentComplete += TryCompleteOrder;
            Order.onOrderElapse += (order) => _orders.Remove(order);
            Order.onOrderComplete += RemoveAndRenewOrder;
            StartCoroutine(_createOrderPassive);
            StartCoroutine(_createMoneyPassive);
        }
    }

    public static bool IsPaused()
    {
        return _paused;
    }

    public bool GameStarted()
    {
        return _gameStarted;
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
        if (_orders.Count < MaxOrders)
        {
            Order order = Instantiate(_orderPrefab, _orderHolder.transform).GetComponent<Order>();
            order._orderID = _nextOrderIndex++;
            _orders.Add(order);
            onOrderCreated?.Invoke();
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
                if (order.GetSegments().Contains(item.GetItemType()))
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
        else if(orderToComplete >= 0 && orderToComplete < _orders.Count)
        {
            if (_orders[orderToComplete].GetSegments().Contains(item.GetItemType()))
            {
                closestOrder = _orders[orderToComplete];
            }
        }

        // If found, check if the player can afford the item and attempt to complete the segment
        if (closestOrder != null)
        {
            if (MoneyManager.canAffordItem?.Invoke(item.GetItemType()) == true || item.IsStored())
            {
                closestOrder.CompleteSegment(item.GetItemType(), item.GetID());
                // Stored items are already paid for, so make sure the player doesn't pay twice
                if (!item.IsStored())
                {
                    purchaseItem?.Invoke(item.GetItemType());
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
    
    // Get the sprite an item should be based on it's rarity
    public static Sprite GetItemSprite(ItemType type)
    {
        switch (type)
        {
            case ItemType.SEAWEED:
                return _itemSprites[0];
            case ItemType.KRILL:
                return _itemSprites[1];
            case ItemType.SARDINE:
                return _itemSprites[2];
            case ItemType.SQUID:
                return _itemSprites[3];
            default:
                return _itemSprites[0];
        }
    }
}
