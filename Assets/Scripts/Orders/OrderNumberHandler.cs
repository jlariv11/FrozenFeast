using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderNumberHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Update all order numbers when an order is created, completed or elapsed
        InitializeOrderNumbers();
        GameManager.onOrderCreated += InitializeOrderNumbers;
        Order.onOrderElapse += UpdateOrderNumbers;
        Order.onOrderComplete += UpdateOrderNumbers;
    }
    

    private void InitializeOrderNumbers()
    {
        // Give the Orders their initial numbers
        for (int i = 0; i < transform.childCount; i++)
        {
            // The actual index of the orders counts 1-9 not 0-9
            transform.GetChild(i).gameObject.GetComponent<Order>().SetOrderNumber(i + 1);
        }
    }
    
    private void UpdateOrderNumbers(Order order)
    {
        // The actual index of the orders counts 1-9 not 0-9
        int idx = 1;
        for (int i = 0; i < transform.childCount; i++)
        {
            Order currentOrder = transform.GetChild(i).gameObject.GetComponent<Order>();
            // Make sure we don't give the order to be destroyed a number
            if (currentOrder._orderID == order._orderID)
            {
                continue;
            }
            currentOrder.SetOrderNumber(idx++);
        }
    }
    
}
