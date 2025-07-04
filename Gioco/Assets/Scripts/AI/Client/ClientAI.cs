using UnityEngine;
using UnityEngine.AI;

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
    private bool orderShown = false;

    private float movingTimer = 0f;
    private const float movingTimeout = 15f;
    private float leavingTimer = 0f;
    private const float leavingTimeout = 10f;

    public ClientState state;
    public Order currentOrder { get; private set; }

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
        if (orderPoints == null || assignedOrderPoint < 0 || assignedOrderPoint >= orderPoints.Length)
        {
            return;
        }

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

        if (state == ClientState.Ordering && !orderShown)
        {
            float dist = Vector3.Distance(transform.position, orderPoints[assignedOrderPoint].position);
            if (dist <= 2.0f)
            {
                EventManager.ClientHasStartedOrdering(this);
                orderShown = true;
            }
        }
    }

    private void HandleMovingToOrder()
    {
        if (assignedOrderPoint >= 0 &&
            Vector3.Distance(transform.position, agent.destination) < 0.2f &&
            !agent.pathPending)
        {
            state = ClientState.Ordering;
            waitTimer = 0f;
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

        if ((!agent.pathPending && agent.remainingDistance <= despawnDistanceThreshold) || leavingTimer > leavingTimeout)
        {
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

        currentOrder = Order.GenerateRandomOrder();
        state = ClientState.Ordering;
        isOrderingNow = true;
    }

    public bool TryDeliverOrder(System.Collections.Generic.List<Ingredient> delivered)
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
            return false;
        }
    }

    public void CompleteOrder()
    {
        if (!isOrderingNow) return;

        isOrderingNow = false;
        currentOrder = null;
        ClientQueue.Instance.ReleasePoint(assignedOrderPoint);
        EventManager.ClientWasServed(this);
        state = ClientState.Leaving;
        agent.SetDestination(endRoute.position);
        leavingTimer = 0f;
    }

    private void GoAngry()
    {
        currentOrder = null;
        ClientQueue.Instance.ReleasePoint(assignedOrderPoint);
        EventManager.ClientGotAngry(this);
        state = ClientState.Leaving;
        agent.SetDestination(endRoute.position);
        leavingTimer = 0f;
    }

    public bool IsOrdering() => state == ClientState.Ordering && isOrderingNow;

    public string GetOrderDescription()
    {
        if (currentOrder == null) return "(Nessun ordine)";
        string burger = currentOrder.SelectedBurger.HasValue ? $"[{currentOrder.SelectedBurger}] " : "";
        return burger + string.Join(", ", currentOrder.Ingredients);
    }
}