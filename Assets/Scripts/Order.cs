using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private GameManager.ItemRarity _orderRarity;
    private List<GameManager.ItemRarity> _orderSegments;
    private int _orderSum;

    private int[] _timesByRarity = { 5, 10, 15, 20};
    public int _orderID { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _orderSegments = new List<GameManager.ItemRarity>();
        int rarityNum = Random.Range(1, 5);
        for (int i = 0; i < rarityNum; i++)
        {
            GameManager.ItemRarity orderRarity = ((GameManager.ItemRarity[])Enum.GetValues(typeof(GameManager.ItemRarity)))[Random.Range(0, 4)];
            transform.GetChild(i).GetComponent<Image>().color = GameManager.GetRarityColor(orderRarity);
            _orderSegments.Add(orderRarity);
        }

        for (int i = rarityNum; i < 4; i++)
        {
            transform.GetChild(i).GetComponent<Image>().color = Color.gray;
        }

        _orderSum = _orderSegments.Select(rarity => MoneyManager.costRarityTable[(int)rarity]).Sum();
        _maxOrderTime = _orderSegments.Select(rarity => _timesByRarity[(int)rarity] + Random.Range(-3, 4)).Sum();
        _currentOrderTime = _maxOrderTime;
    }
    void Update()
    {
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

    public List<GameManager.ItemRarity> GetSegments()
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
    public void CompleteSegment(GameManager.ItemRarity rarity, int completingItem)
    {
        if (_orderSegments.Remove(rarity))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Image child = transform.GetChild(i).GetComponent<Image>();
                if (child.color == GameManager.GetRarityColor(rarity))
                {
                    child.color = Color.gray;
                    onSegmentComplete?.Invoke(completingItem);
                    break;
                }
            }
        }
        // When there are no more segments the order is completed
        if (_orderSegments.Count == 0)
        {
            onOrderComplete?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
