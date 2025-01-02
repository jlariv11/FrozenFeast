using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Refresh : MonoBehaviour
{
    public static Action<int> onRefresh;
    [SerializeField] private GameObject _itemHolder;
    [SerializeField] private int _refreshCost = 5;
    
    // Process pressing the R key to refresh the items
    public void RPress(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            RefreshItems();
        }
    }

    // Re-Roll the rarities of every item in the containter if the player can afford the re-roll
    private void RefreshItems()
    {
        if (MoneyManager.canAffordPurchase?.Invoke(_refreshCost) == true)
        {
            for (var i = 0; i < _itemHolder.transform.childCount; i++)
            {
                _itemHolder.transform.GetChild(i).gameObject.GetComponent<Item>().RollRarity();
            }
            onRefresh?.Invoke(_refreshCost);
        }
    }
    // Handle clicking on the refresh button to refresh the items
    public void MouseClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.down);
            if (hit)
            {
                if (hit.transform.gameObject == this.gameObject)
                {
                    RefreshItems();
                }
            }
        }
    }
}