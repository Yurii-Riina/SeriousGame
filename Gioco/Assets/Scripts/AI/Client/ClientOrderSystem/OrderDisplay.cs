using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/// <summary>
/// Mostra e aggiorna gli ordini dei clienti nella UI.
/// </summary>
public class OrderDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Transform orderContainer;
    public GameObject orderUIPrefab;

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

    /// <summary>
    /// Aggiunge l'ordine alla UI quando il cliente arriva al banco.
    /// </summary>
    private void AddOrder(ClientAI client)
    {
        if (client == null || !client.IsOrdering() || client.currentOrder == null)
        {
            return;
        }

        if (activeOrders.Any(e => e.client == client))
        {
            return;
        }

        if (activeOrders.Count >= 4)
        {
            return;
        }

        GameObject go = Instantiate(orderUIPrefab, orderContainer);
        TMP_Text label = go.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            List<string> elements = new List<string>();

            if (client.currentOrder.SelectedBurger.HasValue)
                elements.Add(client.currentOrder.SelectedBurger.Value.ToString());

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

    /// <summary>
    /// Rimuove l'ordine dalla UI quando il cliente viene servito o si arrabbia.
    /// </summary>
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