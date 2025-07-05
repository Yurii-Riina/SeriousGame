using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class ClientAI : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform[] orderPoints;
    public Transform endRoute;

    public float maxWaitTime = 240f;
    public float moveSpeed = 3.5f;
    public float despawnDistanceThreshold = 1.5f;

    [HideInInspector] public int assignedOrderPoint = -1;

    private NavMeshAgent agent;
    private float waitTimer = 0f;
    private bool isOrderingNow = false;
    //private bool orderShown = false;

    private float movingTimer = 0f;
    private const float movingTimeout = 15f;
    private float leavingTimer = 0f;

    public ClientState state;
    public Order currentOrder { get; private set; }
    public bool IsOrderingNow
    {
        get => isOrderingNow;
        set => isOrderingNow = value;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        if (spawnPoint != null)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPoint.position, out hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

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
            //case ClientState.Ordering:
            //    if (!orderShown)
            //    {
            //        float dist = Vector3.Distance(transform.position, orderPoints[assignedOrderPoint].position);
            //        if (dist <= 2f)
            //        {
            //            EventManager.ClientHasStartedOrdering(this);
            //            Debug.Log($"🟢 Client {name} started ordering.");
            //            orderShown = true;
            //        }
            //    }
            //    break;
        }   
    }

    private void HandleMovingToOrder()
    {
        if (assignedOrderPoint >= 0 &&
            Vector3.Distance(transform.position, agent.destination) < 0.2f &&
            !agent.pathPending)
        {
            Debug.Log($"🟢 Client {name} reached order point.");

            StartOrdering();
        }
    }

    private void HandleWaitingInQueue()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            if (!isOrderingNow)
            {
                StartOrdering();
            }

            waitTimer += Time.deltaTime;
            if (waitTimer >= maxWaitTime)
            {
                GoAngry();
            }
        }
    }

    private void HandleLeaving()
    {
        leavingTimer += Time.deltaTime;
        if ((!agent.pathPending && agent.remainingDistance <= despawnDistanceThreshold) || leavingTimer > 10f)
        {
            Debug.Log($"🟢 Client {name} despawned.");
            Destroy(gameObject);
        }
    }

    public void AssignOrderPoint(int index, Vector3 queuePosition)
    {
        assignedOrderPoint = index;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(queuePosition, out hit, 2f, NavMesh.AllAreas))
        {
            StartCoroutine(SetDestinationNextFrame(hit.position));
        }

        state = ClientState.MovingToOrder;
        movingTimer = waitTimer = leavingTimer = 0f;
    }

    private System.Collections.IEnumerator SetDestinationNextFrame(Vector3 pos)
    {
        yield return null;
        agent.SetDestination(pos);
    }

    public void StartOrdering()
    {
        if (state == ClientState.Leaving) return;

        Debug.Log($"🟢 Client {name} started ordering: {currentOrder.GetReadableDescription()}");

        if (OrderManager.Instance.HasAvailableSlot())
        {
            OrderManager.Instance.AddWaitingClient(this);
            isOrderingNow = true;
        }
        else
        {
            OrderManager.Instance.waitingQueue.Add(this);
            isOrderingNow = false;
            Debug.Log($"🟡 Client {name} messo in coda in attesa di slot libero.");
        }

        state = ClientState.Ordering;

        EventManager.ClientHasStartedOrdering(this);
    }

    public bool TryDeliverOrder(List<Ingredient> delivered)
    {
        Debug.Log($"🔹 Client {name} TryDeliverOrder invoked.");
        if (currentOrder == null || !isOrderingNow)
        {
            Debug.Log("🔴 Cannot deliver: either no order or not ordering.");
            return false;
        }

        Debug.Log($"📦 Delivered ingredients: {string.Join(", ", delivered)}");
        Debug.Log($"📋 Required ingredients: {currentOrder.GetReadableDescription()}");

        if (currentOrder.Matches(delivered))
        {
            CompleteOrder();
            return true;
        }
        else
        {
            Debug.Log("❌ Delivered items do not match the order.");
            return false;
        }
    }

    public void CompleteOrder()
    {
        Debug.Log("✅ CompleteOrder called.");

        if (!isOrderingNow)
        {
            Debug.Log("⚠️ CompleteOrder ignored: isOrderingNow is false.");
            return;
        }

        isOrderingNow = false;
        currentOrder = null;
        ClientQueue.Instance.ReleasePoint(assignedOrderPoint);
        EventManager.ClientWasServed(this);

        Debug.Log($"🚶 Client {name} is leaving towards {endRoute.position}");
        state = ClientState.Leaving;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(endRoute.position);
        }
        else
        {
            Debug.LogWarning("⚠️ Client NavMeshAgent is not on NavMesh!");
        }

        leavingTimer = 0f;
    }

    public void GoAngry()
    {
        currentOrder = null;
        ClientQueue.Instance.ReleasePoint(assignedOrderPoint);
        EventManager.ClientGotAngry(this);
        Debug.Log("😡 Client left angry!");
        state = ClientState.Leaving;
        agent.SetDestination(endRoute.position);
        leavingTimer = 0f;
    }

    public bool IsOrdering() => state == ClientState.Ordering && isOrderingNow;

    public string GetOrderDescription()
    {
        if (currentOrder == null) return "(Nessun ordine)";
        return currentOrder.GetReadableDescription();
    }

    /// <summary>
    /// Restituisce una lista degli ingredienti dell'ordine corrente, oppure una lista vuota.
    /// </summary>
    public List<Ingredient> GetOrderIngredients()
    {
        if (currentOrder != null)
            return currentOrder.Ingredients;
        return new List<Ingredient>();
    }

    public void InitializeOrder()
    {
        if (currentOrder == null)
        {
            currentOrder = Order.GenerateRandomOrder();
            Debug.Log($"🟢 Client {name} inizializza ordine: {currentOrder.GetReadableDescription()}");
        }
    }
}