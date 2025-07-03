using UnityEngine;
using TMPro;
using System.Linq;

public class OrderBoardUI : MonoBehaviour
{
    [Header("Prefab and Container")]
    public GameObject orderUIPrefab;
    public Transform orderContainer;

    public void AddOrderToBoard(Order order)
    {
        GameObject orderGO = Instantiate(orderUIPrefab, orderContainer);
        OrderUI orderUI = orderGO.GetComponent<OrderUI>();

        if (orderUI != null)
        {
            orderUI.SetOrder(order);
        }
        else
        {
            Debug.LogWarning("OrderUI prefab is missing the OrderUI script.");
        }
    }

}
