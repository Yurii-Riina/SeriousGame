using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class ClientAI : MonoBehaviour
{
    [Header("Riferimenti")]
    public Transform spawnPoint;
    public Transform[] orderPoints;
    public Transform endRoute;

    [Header("Impostazioni ordine")]
    [Tooltip("Tempo massimo di attesa prima che il client si arrabbi.")]
    public float maxWaitTime = 240f;

    [Header("Impostazioni movimento")]
    [Tooltip("Velocità di movimento del client.")]
    public float moveSpeed = 3.5f;

    [Tooltip("Distanza per il despawn del client.")]
    public float despawnDistanceThreshold = 1.5f;

    public int assignedOrderPoint = -1;

    private NavMeshAgent agent;
    private float waitTimer = 0f;
    private bool isOrderingNow = false;

    public ClientState state;
    public Order currentOrder { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    private void Start()
    {
        if (spawnPoint == null)
            spawnPoint = ClientQueue.Instance.spawnPoint;

        if (orderPoints == null || orderPoints.Length == 0)
            orderPoints = ClientQueue.Instance.orderPoints;

        if (endRoute == null)
            endRoute = ClientQueue.Instance.endRoute;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        state = ClientState.MovingToOrder;
        ClientQueue.Instance.JoinQueue(this);
    }

    private void Update()
    {
        switch (state)
        {
            case ClientState.MovingToOrder:
                HandleMovingToOrder();
                break;

            case ClientState.WaitingInQueue:
                HandleWaitingInQueue();
                break;

            case ClientState.Leaving:
                HandleLeaving();
                break;
        }
    }

    private void HandleMovingToOrder()
    {
        if (assignedOrderPoint >= 0 &&
            Vector3.Distance(transform.position, agent.destination) < 0.2f &&
            !agent.pathPending)
        {
            state = ClientState.WaitingInQueue;
            waitTimer = 0f;
        }
    }

    private void HandleWaitingInQueue()
    {
        if (Vector3.Distance(transform.position, agent.destination) < 0.2f &&
            !agent.pathPending)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= maxWaitTime)
                GoAngry();
        }
    }

    private void HandleLeaving()
    {
        if (!agent.pathPending && agent.remainingDistance <= despawnDistanceThreshold)
        {
            Destroy(gameObject);
        }
    }

    public void AssignOrderPoint(int index, Vector3 queuePosition)
    {
        assignedOrderPoint = index;
        agent.SetDestination(queuePosition);
        state = ClientState.MovingToOrder;
    }

    public void StartOrdering()
    {
        if (state == ClientState.Leaving) return;

        currentOrder = Order.GenerateRandomOrder();
        state = ClientState.Ordering;
        isOrderingNow = true;
        Debug.Log($"{name} ha ordinato: {GetOrderDescription()}");

        // Notifica agli ascoltatori che il cliente ha iniziato un ordine
        EventManager.ClientHasStartedOrdering(this);
    }

    public bool TryDeliverOrder(List<Ingredient> delivered)
    {
        if (currentOrder == null || !isOrderingNow)
            return false;

        if (currentOrder.Matches(delivered))
        {
            CompleteOrder();
            return true;
        }
        else
        {
            // feedback: ordine errato
            return false;
        }
    }

    public void CompleteOrder()
    {
        if (!isOrderingNow) return;

        isOrderingNow = false;
        currentOrder = null; // Destroy the order
        Debug.Log($"{name} ha completato l'ordine e sta lasciando il ristorante.");
        ClientQueue.Instance.ReleasePoint(assignedOrderPoint);
        EventManager.ClientWasServed(this);
        state = ClientState.Leaving;
        agent.SetDestination(endRoute.position);
    }

    private void GoAngry()
    {
        Debug.Log($"{name} si è arrabbiato!");
        currentOrder = null; // Destroy the order
        ClientQueue.Instance.ReleasePoint(assignedOrderPoint);
        EventManager.ClientGotAngry(this);
        state = ClientState.Leaving;
        agent.SetDestination(endRoute.position);
    }

    public bool IsOrdering()
    {
        return state == ClientState.Ordering && isOrderingNow;
    }

    public string GetOrderDescription()
    {
        if (currentOrder == null) return "(Nessun ordine)";
        string burger = currentOrder.SelectedBurger.HasValue ? $"[{currentOrder.SelectedBurger}] " : "";
        return burger + string.Join(", ", currentOrder.Ingredients);
    }
}
