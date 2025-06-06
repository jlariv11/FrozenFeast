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
    public static Action<GameManager.ItemType> onStoreItem;
    
    private const int MaxStock = 4;
    
    private GameManager.ItemType _type;

    private SpriteRenderer _itemRenderer;
    private bool _isHovering;
    private int _itemID;

    private void Start()
    {
        Order.onSegmentComplete += RollRarity;
        Order.onSegmentComplete += DestroyStockOnComplete;
        _isHovering = false;
        _itemID = GameManager.nextItemIndex++;
        _itemRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        Order.onSegmentComplete -= RollRarity;
        Order.onSegmentComplete -= DestroyStockOnComplete;
    }

    // Make sure the proper item is getting re-rolled (called by onSegmentComplete event)
    private void RollRarity(int completingItem)
    {
        if(IsStored() || completingItem != _itemID)
            return;
        RollRarity();
    }

    // Re-Roll the rarity of the current item. Ignore if it is stored
    public void RollRarity()
    {
        if(IsStored())
            return;
        /*
         * Rarity Chances:
         * Common: 50%
         * Uncommon: 30%
         * Rare: 15%
         * Legendary: 5%
         */
        int randIndex = Random.Range(1, 101);
        if (randIndex <= 50)
        {
            _type = GameManager.ItemType.SEAWEED;
        }else if (randIndex <= 80)
        {
            _type = GameManager.ItemType.KRILL;
        }else if (randIndex <= 95)
        {
            _type = GameManager.ItemType.SARDINE;
        }
        else
        {
            _type = GameManager.ItemType.SQUID;
        }
        // Change the sprite of the item based on the type
        _itemRenderer.sprite = GameManager.GetItemSprite(_type);
    }
    

    // Set the rarity of the item and change its color
    private void SetType(GameManager.ItemType type)
    {
        _type = type;
        // Can occur when creating stored items
        if (_itemRenderer == null)
        {
            _itemRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
        _itemRenderer.sprite = GameManager.GetItemSprite(_type);
    }

    public GameManager.ItemType GetItemType()
    {
        return _type;
    }

    public int GetID()
    {
        return _itemID;
    }

    // Base Items will be ids 0-3 all others will be stored items
    public bool IsStored()
    {
        return _itemID > 3;
    }

    // Highlight the item currently hovered by the mouse
    private void OnMouseEnter()
    {
        Color highlighted = _itemRenderer.color;
        highlighted.a = 0.8f;
        _itemRenderer.color = highlighted;
        _isHovering = true;
    }

    // Stop highlighting when the mouse leaves
    private void OnMouseExit()
    {
        Color normal = _itemRenderer.color;
        normal.a = 1.0f;
        _itemRenderer.color = normal;
        _isHovering = false;
    }
    
    // Handle clicking on items
    public void MouseClick(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || GameManager.IsPaused()) return;
        if (_isHovering)
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
            if (orderToSend == -1 || (orderToSend >= 0 && orderToSend < GameManager.MaxOrders))
            {
                // Check if the player can afford the item, then process the order
                if (MoneyManager.canAffordItem?.Invoke(_type) == true || IsStored())
                {
                    onSegmentComplete?.Invoke(this, orderToSend);
                }
            }
        }
    }

    // Handles moving an item to the stock
    public void StockItem(InputAction.CallbackContext ctx)
    {
        if (ctx.started && _isHovering && !GameManager.IsPaused())
        {
            Transform stockHolder = GameObject.Find("Stock").transform.GetChild(1);
            if (stockHolder.childCount < MaxStock)
            {
                if (MoneyManager.canAffordItem?.Invoke(_type) == true)
                {
                    // Create a new Item to be put into the stock with the same rarity as this one
                    // Re-roll the rarity of this item
                    Item stockItem = Instantiate(gameObject, stockHolder).GetComponent<Item>();
                    stockItem.SetType(_type);
                    RollRarity();
                    onStoreItem?.Invoke(_type);
                }
            }
        }
    }

    // Destroy the Item in the stock upon completing a segment
    private void DestroyStockOnComplete(int completingItem)
    {
        if (IsStored() && completingItem == _itemID)
        {
            Destroy(gameObject);
        }
    }
}
