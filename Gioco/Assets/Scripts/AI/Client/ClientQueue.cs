using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class ClientQueue : MonoBehaviour
{
    public static ClientQueue Instance;

    public Transform spawnPoint;
    public Transform[] orderPoints;
    public Transform endRoute;

    public float queueSpacing = 3f;
    public int maxQueueLength = 7;

    private bool[] occupiedPoints;
    private Queue<ClientAI> queue = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        occupiedPoints = new bool[orderPoints.Length];
    }

    public bool CanJoinQueue()
    {
        return queue.Count < maxQueueLength;
    }

    public void JoinQueue(ClientAI client)
    {
        if (client.spawnPoint == null)
            client.spawnPoint = spawnPoint;
        if (client.orderPoints == null || client.orderPoints.Length == 0)
            client.orderPoints = orderPoints;
        if (client.endRoute == null)
            client.endRoute = endRoute;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(client.spawnPoint.position, out hit, 2f, NavMesh.AllAreas))
        {
            client.transform.position = hit.position;
            client.transform.rotation = spawnPoint.rotation;
        }

        if (!CanJoinQueue())
        {
            return;
        }

        queue.Enqueue(client);
        UpdateQueuePositions();
        TryServeNext();
    }

    private void UpdateQueuePositions()
    {
        Vector3 basePos = orderPoints[0].position;
        Vector3 forward = orderPoints[0].forward;

        int i = 0;
        foreach (var client in queue)
        {
            Vector3 pos = basePos - forward * queueSpacing * (i + 1);
            client.AssignOrderPoint(0, pos);
            i++;
        }
    }

    private void TryServeNext()
    {
        if (queue.Count == 0) return;

        for (int i = 0; i < orderPoints.Length; i++)
        {
            if (!occupiedPoints[i])
            {
                var client = queue.Dequeue();
                occupiedPoints[i] = true;

                client.AssignOrderPoint(i, orderPoints[i].position);
                client.StartOrdering();
                UpdateQueuePositions();
                break;
            }
        }
    }

    public void ReleasePoint(int pointIdx)
    {
        if (pointIdx >= 0 && pointIdx < occupiedPoints.Length)
        {
            occupiedPoints[pointIdx] = false;
        }

        TryServeNext();
    }

    public bool CanSpawnClient(int maxWaitingClients)
    {
        int count = 0;
        foreach (var client in queue)
        {
            if (client.state == ClientState.MovingToOrder || client.state == ClientState.WaitingInQueue || client.state == ClientState.Ordering)
                count++;
        }
        return count < maxWaitingClients;
    }
}