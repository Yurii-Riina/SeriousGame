using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestisce il ciclo di vita di un cliente nella coda.
/// - Si muove verso il punto d'ordine.
/// - Mostra e genera un ordine.
/// - Attende di essere servito o va via se aspetta troppo.
/// - Gestisce la consegna dell'ordine e la reazione in caso di errore.
/// - Esce dalla scena dopo essere stato servito o se si arrabbia.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ClientAI : MonoBehaviour
{
    #region Public Fields

    public Transform spawnPoint;
    public Transform[] orderPoints;
    public Transform endRoute;

    /// <summary>Tempo massimo di attesa per essere servito.</summary>
    public float maxWaitTime = 180f;
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

    /// <summary>
    /// Inizializza il cliente e lo posiziona sul NavMesh.
    /// </summary>
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        // Posiziona il cliente sul NavMesh allo spawn
        if (spawnPoint != null && NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        // Entra nella coda
        ClientQueue.Instance.JoinQueue(this);

        if (xManager == null)
            xManager = FindFirstObjectByType<XManager>();
    }

    /// <summary>
    /// Gestisce il ciclo degli stati del cliente.
    /// </summary>
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

    /// <summary>
    /// Gestisce il movimento verso il punto d'ordine.
    /// </summary>
    private void HandleMovingToOrder()
    {
        if (assignedOrderPoint >= 0 && !agent.pathPending && agent.remainingDistance < 0.2f)
        {
            Debug.Log($"🟢 Client {name} reached order point.");
            state = ClientState.Ordering;
            waitTimer = 0f;
        }
    }

    /// <summary>
    /// Gestisce l'attesa dopo aver ordinato.
    /// </summary>
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

    /// <summary>
    /// Gestisce la fase di uscita del cliente.
    /// </summary>
    private void HandleLeaving()
    {
        leavingTimer += Time.deltaTime;

        if ((!agent.pathPending && agent.remainingDistance <= despawnDistanceThreshold) || leavingTimer > 10f)
        {
            Debug.Log($"🟢 Client {name} despawned.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gestisce la fase in cui il cliente mostra l'ordine.
    /// </summary>
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

    /// <summary>
    /// Assegna il punto d'ordine e muove il cliente verso la posizione in coda.
    /// </summary>
    public void AssignOrderPoint(int index, Vector3 queuePosition)
    {
        assignedOrderPoint = index;

        if (NavMesh.SamplePosition(queuePosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            StartCoroutine(SetDestinationNextFrame(hit.position));

        state = ClientState.MovingToOrder;
        ResetTimers();
    }

    /// <summary>
    /// Genera un nuovo ordine per il cliente.
    /// </summary>
    public void StartOrdering()
    {
        if (state == ClientState.Leaving) return;

        currentOrder = Order.GenerateRandomOrder();
        Debug.Log($"🟢 Client {name} created order: {currentOrder.GetReadableDescription()}");

        state = ClientState.Ordering;
        isOrderingNow = true;
    }

    /// <summary>
    /// Tenta di consegnare un ordine al cliente.
    /// </summary>
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

    /// <summary>
    /// Completa l'ordine e fa uscire il cliente.
    /// </summary>
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

    /// <summary>
    /// Il cliente si arrabbia e va via.
    /// </summary>
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

    /// <summary>
    /// Ritorna true se il cliente sta ordinando.
    /// </summary>
    public bool IsOrdering() => state == ClientState.Ordering && isOrderingNow;

    /// <summary>
    /// Restituisce la descrizione dell'ordine attuale.
    /// </summary>
    public string GetOrderDescription() => currentOrder?.GetReadableDescription() ?? "(Nessun ordine)";

    /// <summary>
    /// Restituisce la lista degli ingredienti dell'ordine attuale.
    /// </summary>
    public List<Ingredient> GetOrderIngredients() => currentOrder?.Ingredients ?? new List<Ingredient>();

    #endregion

    #region Private Helpers

    /// <summary>
    /// Reset di tutti i timer e flag.
    /// </summary>
    private void ResetTimers()
    {
        waitTimer = 0f;
        leavingTimer = 0f;
        orderShown = false;
        isOrderingNow = false;
    }

    /// <summary>
    /// Muove il cliente verso l'uscita.
    /// </summary>
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

    /// <summary>
    /// Avvia il timer per il timeout dell'ordine.
    /// </summary>
    private void StartOrderingTimer()
    {
        if (orderingTimeoutCoroutine != null) return;
        orderingTimeoutCoroutine = StartCoroutine(OrderingTimeoutCoroutine());
    }

    /// <summary>
    /// Ferma il timer per il timeout dell'ordine.
    /// </summary>
    private void StopOrderingTimer()
    {
        if (orderingTimeoutCoroutine != null)
        {
            StopCoroutine(orderingTimeoutCoroutine);
            orderingTimeoutCoroutine = null;
        }
    }

    /// <summary>
    /// Imposta la destinazione del NavMeshAgent al prossimo frame.
    /// </summary>
    private IEnumerator SetDestinationNextFrame(Vector3 pos)
    {
        yield return null;
        agent.SetDestination(pos);
    }

    /// <summary>
    /// Coroutine per il timeout dell'ordine.
    /// </summary>
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