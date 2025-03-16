using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Order : MonoBehaviour
{
    public static Action<Order> onOrderElapse;
    public static Action<int> onSegmentComplete;
    public static Action<Order> onOrderComplete;

    [SerializeField] private Image _orderTimerBar;

    private float _maxOrderTime;

    private float _currentOrderTime;

    private GameManager.ItemType _orderType;
    private List<GameManager.ItemType> _orderSegments;
    private int _orderSum;
    private Transform _plate;
    private TextMeshProUGUI _orderNumberText;

    private int[] _timesByRarity = { 5, 10, 15, 20};
    public int _orderID { get; set; }


    private void Awake()
    {
        _orderNumberText = transform.GetChild(2).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _orderSegments = new List<GameManager.ItemType>();
        _plate = transform.GetChild(0);
        int rarityNum = Random.Range(1, 5);
        GameManager.ItemType orderType;
        /*
         * Rarity Chances:
         * Common: 40%
         * Uncommon: 30%
         * Rare: 15%
         * Legendary: 15%
         */
        for (int i = 0; i < rarityNum; i++)
        {
            int randIndex = Random.Range(1, 101);
            if (randIndex <= 40)
            {
                orderType = GameManager.ItemType.SEAWEED;
            }else if (randIndex <= 70)
            {
                orderType = GameManager.ItemType.KRILL;
            }else if (randIndex <= 85)
            {
                orderType = GameManager.ItemType.SARDINE;
            }
            else
            {
                orderType = GameManager.ItemType.SQUID;
            }
            _plate.GetChild(i).GetComponent<Image>().sprite = GameManager.GetItemSprite(orderType);
            _plate.GetChild(i).gameObject.SetActive(true);
            _orderSegments.Add(orderType);
        }

        for (int i = rarityNum; i < 4; i++)
        {
            _plate.GetChild(i).gameObject.SetActive(false);
        }

        _orderSum = _orderSegments.Select(rarity => MoneyManager.costRarityTable[(int)rarity]).Sum();
        _maxOrderTime = _orderSegments.Select(rarity => _timesByRarity[(int)rarity] + Random.Range(-3, 4)).Sum();
        _currentOrderTime = _maxOrderTime;
    }

    void Update()
    {
        if (GameManager.IsPaused())
            return;
        // Update the time an order has been active
        // Update progress bar and delete when time reaches 0
        _currentOrderTime -= Time.deltaTime;
        _orderTimerBar.fillAmount = _currentOrderTime / _maxOrderTime;
        if (_currentOrderTime <= 0)
        {
            Destroy(gameObject);
            onOrderElapse?.Invoke(this);
        }
    }

    public void SetOrderNumber(int orderNumber)
    {
        _orderNumberText.text = orderNumber.ToString();
    }

    public List<GameManager.ItemType> GetSegments()
    {
        return _orderSegments;
    }

    public float GetTimeRemaining()
    {
        return _currentOrderTime;
    }

    public float GetMaxTime()
    {
        return _maxOrderTime;
    }

    public int GetOrderSum()
    {
        return _orderSum;
    }

    // Remove the appropriate segment from the list and update its color when an order is processed
    public void CompleteSegment(GameManager.ItemType type, int completingItem)
    {
        if (_orderSegments.Remove(type))
        {
            for (int i = 0; i < _plate.childCount; i++)
            {
                Image child = _plate.GetChild(i).GetComponent<Image>();
                if (child.gameObject.activeSelf && child.sprite == GameManager.GetItemSprite(type))
                {
                    child.gameObject.SetActive(false);
                    onSegmentComplete?.Invoke(completingItem);
                    break;
                }
            }
        }
        // When there are no more segments the order is completed
        if (_orderSegments.Count == 0)
        {
            Destroy(gameObject);
            onOrderComplete?.Invoke(this);
        }
    }
}
