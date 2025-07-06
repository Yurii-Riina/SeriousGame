using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class ClientAI : MonoBehaviour
{
    #region Public Fields

    public Transform spawnPoint;
    public Transform[] orderPoints;
    public Transform endRoute;

    public float maxWaitTime = 90f;
    public float maxOrderingTime = 90f;
    public float moveSpeed = 3.5f;
    public float despawnDistanceThreshold = 1.5f;

    [HideInInspector] public int assignedOrderPoint = -1;
    public ClientState state;
    public Order currentOrder { get; private set; }

    #endregion

    #region Private Fields

    private NavMeshAgent agent;
    private Coroutine orderingTimeoutCoroutine;
    [SerializeField] private XManager xManager;

    private float waitTimer = 0f;
    private float leavingTimer = 0f;

    private bool isOrderingNow = false;
    private bool orderShown = false;

    #endregion

    #region Unity Events

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        if (spawnPoint != null && NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        ClientQueue.Instance.JoinQueue(this);

        if (xManager == null)
            xManager = FindFirstObjectByType<XManager>();
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

            case ClientState.Ordering:
                HandleOrdering();
                break;
        }
    }

    #endregion

    #region State Handlers

    private void HandleMovingToOrder()
    {
        if (assignedOrderPoint >= 0 && !agent.pathPending && agent.remainingDistance < 0.2f)
        {
            Debug.Log($"🟢 Client {name} reached order point.");
            state = ClientState.Ordering;
            waitTimer = 0f;
        }
    }

    private void HandleWaitingInQueue()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            if (!isOrderingNow)
                StartOrdering();

            waitTimer += Time.deltaTime;
            if (waitTimer >= maxWaitTime)
                GoAngry();
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

    private void HandleOrdering()
    {
        if (orderShown) return;

        float dist = Vector3.Distance(transform.position, orderPoints[assignedOrderPoint].position);
        if (dist <= 2f)
        {
            EventManager.ClientHasStartedOrdering(this);
            Debug.Log($"🟢 Client {name} started ordering.");
            orderShown = true;

            StartOrderingTimer();
        }
    }

    #endregion

    #region Public Methods

    public void AssignOrderPoint(int index, Vector3 queuePosition)
    {
        assignedOrderPoint = index;

        if (NavMesh.SamplePosition(queuePosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            StartCoroutine(SetDestinationNextFrame(hit.position));

        state = ClientState.MovingToOrder;
        ResetTimers();
    }

    public void StartOrdering()
    {
        if (state == ClientState.Leaving) return;

        currentOrder = Order.GenerateRandomOrder();
        Debug.Log($"🟢 Client {name} created order: {currentOrder.GetReadableDescription()}");

        state = ClientState.Ordering;
        isOrderingNow = true;
    }

    public bool TryDeliverOrder(List<Ingredient> delivered)
    {
        if (currentOrder == null || !isOrderingNow)
        {
            Debug.Log("🔴 Cannot deliver: either no order or not ordering.");
            return false;
        }

        Debug.Log($"📦 Delivered: {string.Join(", ", delivered)}");
        Debug.Log($"📋 Expected: {currentOrder.GetReadableDescription()}");

        if (currentOrder.Matches(delivered))
        {
            CompleteOrder();
            return true;
        }

        Debug.Log("❌ Wrong delivery.");
        return false;
    }

    public void CompleteOrder()
    {
        if (!isOrderingNow)
        {
            Debug.Log("⚠️ Cannot complete: not ordering.");
            return;
        }

        Debug.Log("✅ Order completed.");

        isOrderingNow = false;
        currentOrder = null;

        StopOrderingTimer();

        ClientQueue.Instance.ReleasePoint(assignedOrderPoint);
        EventManager.ClientWasServed(this);

        MoveToEndRoute();
    }

    public void GoAngry()
    {
        Debug.Log("😡 Client left angry.");

        currentOrder = null;
        StopOrderingTimer();

        ClientQueue.Instance.ReleasePoint(assignedOrderPoint);
        EventManager.ClientGotAngry(this);

        xManager?.TurnNextXRed(); // 🔴 Segna un errore
        MoveToEndRoute();
    }

    public bool IsOrdering() => state == ClientState.Ordering && isOrderingNow;

    public string GetOrderDescription() => currentOrder?.GetReadableDescription() ?? "(Nessun ordine)";
    public List<Ingredient> GetOrderIngredients() => currentOrder?.Ingredients ?? new List<Ingredient>();

    #endregion

    #region Private Helpers

    private void ResetTimers()
    {
        waitTimer = 0f;
        leavingTimer = 0f;
        orderShown = false;
        isOrderingNow = false;
    }

    private void MoveToEndRoute()
    {
        state = ClientState.Leaving;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(endRoute.position);
        }
        else
        {
            Debug.LogWarning("⚠️ Agent is not on NavMesh.");
        }

        leavingTimer = 0f;
    }

    private void StartOrderingTimer()
    {
        if (orderingTimeoutCoroutine != null) return;
        orderingTimeoutCoroutine = StartCoroutine(OrderingTimeoutCoroutine());
    }

    private void StopOrderingTimer()
    {
        if (orderingTimeoutCoroutine != null)
        {
            StopCoroutine(orderingTimeoutCoroutine);
            orderingTimeoutCoroutine = null;
        }
    }

    private IEnumerator SetDestinationNextFrame(Vector3 pos)
    {
        yield return null;
        agent.SetDestination(pos);
    }

    private IEnumerator OrderingTimeoutCoroutine()
    {
        float timer = 0f;
        while (timer < maxOrderingTime)
        {
            if (!isOrderingNow || state != ClientState.Ordering)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"⏰ Client {name} ordering time expired.");
        GoAngry();
    }

    #endregion
}