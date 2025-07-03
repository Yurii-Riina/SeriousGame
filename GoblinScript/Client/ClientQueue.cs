using System.Collections.Generic;
using UnityEngine;

public class ClientQueue : MonoBehaviour
{
    public static ClientQueue Instance;

    [Header("Punti di riferimento")]
    public Transform spawnPoint;
    public Transform[] orderPoints;
    public Transform endRoute;

    [Header("Impostazioni coda")]
    public float queueSpacing = 3f;
    public int maxQueueLength = 7;

    private bool[] occupiedPoints;
    private Queue<ClientAI> queue = new Queue<ClientAI>();
    private bool isBusy = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        occupiedPoints = new bool[orderPoints.Length];
    }

    /// <summary>
    /// Verifica se un nuovo client può unirsi alla coda.
    /// </summary>
    public bool CanJoinQueue()
    {
        int total = queue.Count + (isBusy ? 1 : 0);
        return total < maxQueueLength;
    }

    /// <summary>
    /// Aggiunge un client alla coda.
    /// </summary>
    public void JoinQueue(ClientAI client)
    {
        if (!CanJoinQueue())
        {
            Debug.Log($"{client.name} non può entrare: coda piena.");
            return;
        }

        queue.Enqueue(client);
        UpdateQueuePositions();
        TryServeNext();
    }

    /// <summary>
    /// Aggiorna le posizioni dei client in coda.
    /// </summary>
    private void UpdateQueuePositions()
    {
        Vector3 basePos = orderPoints[0].position;
        Vector3 forward = -orderPoints[0].right; // verso sinistra

        int i = 0;
        foreach (var client in queue)
        {
            Vector3 pos = basePos - forward * queueSpacing * (i + 1);
            client.AssignOrderPoint(0, pos);
            i++;
        }
    }

    /// <summary>
    /// Prova a servire il prossimo client nella coda.
    /// </summary>
    private void TryServeNext()
    {
        if (queue.Count == 0) return;

        for (int i = 0; i < orderPoints.Length; i++)
        {
            if (!occupiedPoints[i])
            {
                ClientAI client = queue.Dequeue();
                occupiedPoints[i] = true;

                client.AssignOrderPoint(i, orderPoints[i].position);
                client.StartOrdering();
                UpdateQueuePositions();
                break;
            }
        }
    }

    /// <summary>
    /// Libera un punto d'ordine e serve il prossimo client.
    /// </summary>
    public void ReleasePoint(int pointIdx)
    {
        if (pointIdx >= 0 && pointIdx < occupiedPoints.Length)
        {
            occupiedPoints[pointIdx] = false;
        }

        TryServeNext();
    }

    /// <summary>
    /// Verifica se è possibile spawnare un nuovo client in base ai client in attesa.
    /// </summary>
    public bool CanSpawnClient(int maxWaitingClients)
    {
        int count = 0;
        foreach (var client in queue)
        {
            if (client.state == ClientState.MovingToOrder || client.state == ClientState.WaitingInQueue)
                count++;
        }
        return count < maxWaitingClients;
    }

    // Metodo temporaneo per testare il completamento dell'ordine
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("N");
            GameObject[] allClients = GameObject.FindGameObjectsWithTag("Client");
            foreach (var obj in allClients)
            {
                ClientAI ai = obj.GetComponent<ClientAI>();
                if (ai == null || !ai.IsOrdering())
                    continue;

                // Controlla se è vicino al suo orderPoint
                Vector3 orderPointPos = ClientQueue.Instance.orderPoints[ai.assignedOrderPoint].position;
                float distance = Vector3.Distance(ai.transform.position, orderPointPos);

                if (distance <= 2f) // margine di tolleranza 
                {
                    ai.CompleteOrder();
                }
            }
        }
    }
}
