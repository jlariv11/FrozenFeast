using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Random = UnityEngine.Random;

public class Item : MonoBehaviour
{
    public static Action<Item, int> onSegmentComplete;
    public static Action<GameManager.ItemRarity> onStoreItem;
    
    private const int MaxStock = 4;
    
    private GameManager.ItemRarity _rarity;

    private SpriteRenderer _renderer;
    private bool _isHovering;
    private bool _isStored;
    private int _itemID;
    void Awake()
    {
        RollRarity();
        Order.onSegmentComplete += RollRarity;
        Order.onSegmentComplete += DestroyStockOnComplete;
        _isHovering = false;
        _isStored = false;
        _itemID = GameManager.nextItemIndex++;
    }

    // Make sure the proper item is getting re-rolled
    private void RollRarity(int completingItem)
    {
        if(_isStored || completingItem != _itemID)
            return;
        RollRarity();
    }

    // Re-Roll the rarity of the current item. Ignore if it is stored
    public void RollRarity()
    {
        if(_isStored)
            return;
        /*
         * Rarity Chances:
         * Common: 50%
         * Uncommon: 30%
         * Rare: 15%
         * Legendary: 5%
         */
        int randIndex = Random.Range(1, 101);
        if (randIndex < 51)
        {
            _rarity = GameManager.ItemRarity.COMMON;
        }else if (randIndex < 81)
        {
            _rarity = GameManager.ItemRarity.UNCOMMON;
        }else if (randIndex < 96)
        {
            _rarity = GameManager.ItemRarity.RARE;
        }
        else
        {
            _rarity = GameManager.ItemRarity.LEGENDARY;
        }
        // Change the color of the item based on the rarity
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = GameManager.GetRarityColor(_rarity);
    }

    // Set the rarity of the item and change its color
    private void SetRarity(GameManager.ItemRarity rarity)
    {
        _rarity = rarity;
        _renderer.color = GameManager.GetRarityColor(_rarity);
    }

    public GameManager.ItemRarity GetRarity()
    {
        return _rarity;
    }

    public int GetID()
    {
        return _itemID;
    }

    private void SetStored(bool value)
    {
        _isStored = value;
    }

    public bool IsStored()
    {
        return _isStored;
    }

    // Highlight the item currently hovered by the mouse
    private void OnMouseEnter()
    {
        Color highlighted = _renderer.color;
        highlighted.a = 0.8f;
        _renderer.color = highlighted;
        _isHovering = true;
    }

    // Stop highlighting when the mouse leaves
    private void OnMouseExit()
    {
        Color normal = _renderer.color;
        normal.a = 1.0f;
        _renderer.color = normal;
        _isHovering = false;
    }
    
    // Handle clicking on items
    public void MouseClick(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            // -2: Click was neither leftButton or number key should never happen
            // -1: Click was leftButton - needs to find the most urgent order
            // 1-9: Click was a number key - apply the item to the (1-9)th order
            int orderToSend = -2;
            if (ctx.control.name == "leftButton")
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
                if (hit)
                {
                    if (hit.transform.gameObject == this.gameObject)
                    {
                        orderToSend = -1;
                    }
                }
            }
            else
            {
                orderToSend = int.Parse(ctx.control.name) - 1;
            }

            if (orderToSend != -2)
            {
                // Check if the player can afford the item, then process the order
                if (MoneyManager.canAffordItem?.Invoke(_rarity) == true)
                {
                    onSegmentComplete?.Invoke(this, orderToSend);
                }
            }
        }
    }

    // Handles moving an item to the stock
    public void StockItem(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _isHovering)
        {
            Transform stockHolder = GameObject.Find("Stock").transform.GetChild(1);
            if (stockHolder.childCount < MaxStock)
            {
                if (MoneyManager.canAffordItem?.Invoke(_rarity) == true)
                {
                    // Create a new Item to be put into the stock witht the same rarity as this one
                    // Re-roll the rarity of this item
                    Item stockItem = Instantiate(gameObject, stockHolder).GetComponent<Item>();
                    stockItem.SetRarity(_rarity);
                    stockItem.SetStored(true);
                    RollRarity();
                    onStoreItem?.Invoke(_rarity);
                }
            }
        }
    }

    // Destroy the Item in the stock upon completing a segment
    private void DestroyStockOnComplete(int completingItem)
    {
        if (_isStored && completingItem == _itemID)
        {
            Destroy(gameObject);
        }
    }
}
