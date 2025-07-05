using UnityEngine;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    private List<ClientAI> waitingClients = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Aggiunge un cliente in attesa ordine.
    /// </summary>
    public void AddWaitingClient(ClientAI client)
    {
        if (!waitingClients.Contains(client))
        {
            waitingClients.Add(client);
            Debug.Log($"🟢 Aggiunto {client.name} alla lista in attesa ordine.");
        }
    }

    /// <summary>
    /// Rimuove un cliente dalla lista.
    /// </summary>
    public void RemoveWaitingClient(ClientAI client)
    {
        if (waitingClients.Contains(client))
        {
            waitingClients.Remove(client);
        }
    }

    /// <summary>
    /// Consegna un ordine al primo cliente in coda.
    /// </summary>
    public void DeliverOrder(List<Ingredient> deliveredIngredients)
    {
        if (waitingClients.Count == 0)
        {
            Debug.LogWarning("⚠️ Nessun cliente in attesa ordine.");
            return;
        }

        var client = waitingClients[0];
        bool success = client.TryDeliverOrder(deliveredIngredients);

        if (success)
        {
            Debug.Log("✅ Ordine consegnato correttamente.");
            RemoveWaitingClient(client);
        }
        else
        {
            Debug.Log("❌ Ordine errato.");
        }
    }
}
