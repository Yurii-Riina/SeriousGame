using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OrderDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Transform orderContainer;
    public GameObject orderUIPrefab;

    // Ogni indice corrisponde allo slot di ClientQueue
    private TMP_Text[] slotTexts;
    private ClientAI[] slotClients;

    private void Start()
    {
        // Crea 4 slot fissi
        int slots = 4;
        slotTexts = new TMP_Text[slots];
        slotClients = new ClientAI[slots];

        for (int i = 0; i < slots; i++)
        {
            GameObject go = Instantiate(orderUIPrefab, orderContainer);
            TMP_Text label = go.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = ""; // Vuoto all'inizio

            slotTexts[i] = label;
            slotClients[i] = null;
        }
    }

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
    /// Aggiunge o aggiorna lo slot quando il cliente arriva.
    /// </summary>
    private void AddOrder(ClientAI client)
    {
        if (client == null || !client.IsOrdering() || client.currentOrder == null)
            return;

        int slotIdx = client.assignedOrderPoint;
        if (slotIdx < 0 || slotIdx >= slotTexts.Length)
        {
            Debug.LogWarning($"❗ Slot index {slotIdx} fuori range per {client.name}");
            return;
        }

        // Associa il client allo slot
        slotClients[slotIdx] = client;

        // Prepara testo
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

        string finalText = string.Join("\n", elements);
        slotTexts[slotIdx].text = finalText;
    }

    /// <summary>
    /// Svuota lo slot quando il cliente viene servito o si arrabbia.
    /// </summary>
    private void RemoveOrder(ClientAI client)
    {
        if (client == null) return;

        int slotIdx = client.assignedOrderPoint;
        if (slotIdx < 0 || slotIdx >= slotTexts.Length)
            return;

        slotTexts[slotIdx].text = "";
        slotClients[slotIdx] = null;
    }
}