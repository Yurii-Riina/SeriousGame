using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays and manages the list of active client orders on the order board UI.
/// </summary>
public class OrderDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent transform that holds all order UI entries.")]
    public Transform orderContainer;

    [Tooltip("Prefab for displaying a single client order.")]
    public GameObject orderUIPrefab;

    // Track active order UI entries and their associated clients
    private List<(ClientAI client, GameObject uiObject)> activeOrders = new();

    private void OnEnable()
    {
        EventManager.OnClientStartedOrdering += AddOrder;
        EventManager.OnClientWasServed += RemoveOrder;
        EventManager.OnClientGotAngry += RemoveOrder;
    }

    private void OnDisable()
    {
        EventManager.OnClientStartedOrdering -= AddOrder;
        EventManager.OnClientWasServed -= RemoveOrder;
        EventManager.OnClientGotAngry -= RemoveOrder;
    }

    private void AddOrder(ClientAI client)
    {
        if (!client.IsOrdering() || client.currentOrder == null)
            return;

        if (activeOrders.Count >= 4)
        {
            var oldest = activeOrders[0];
            Destroy(oldest.uiObject);
            activeOrders.RemoveAt(0);
        }

        GameObject go = Instantiate(orderUIPrefab);
        go.transform.SetParent(orderContainer, false);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        TMP_Text label = go.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            List<string> elements = new List<string>();

            // Mostra solo nome del panino
            if (client.currentOrder.SelectedBurger.HasValue)
                elements.Add(client.currentOrder.SelectedBurger.Value.ToString());

            // Aggiungi dolci, fritti, bevande (non mostrare ingredienti interni del panino)
            foreach (var item in client.currentOrder.Ingredients)
            {
                switch (item)
                {
                    case Ingredient.Donut:
                    case Ingredient.Fries:
                    case Ingredient.Nuggets:
                    case Ingredient.Water:
                    case Ingredient.CocaCola:
                    case Ingredient.Fanta:
                        elements.Add(item.ToString());
                        break;
                }
            }

            label.text = string.Join("\n", elements);
        }

        activeOrders.Add((client, go));
    }

    private void RemoveOrder(ClientAI client)
    {
        var entry = activeOrders.FirstOrDefault(e => e.client == client);
        if (entry.uiObject != null)
        {
            Destroy(entry.uiObject);
            activeOrders.Remove(entry);
        }
    }
}
