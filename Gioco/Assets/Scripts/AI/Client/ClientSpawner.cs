using UnityEngine;
using System.Collections;

public class ClientSpawner : MonoBehaviour
{
    public GameObject clientPrefab;
    public Transform spawnPoint;

    public float spawnDelay = 1f;
    public int maxQueueWaitingClients = 3;
    public bool autoStart = true;

    private int spawnedCount = 0;

    private void Start()
    {
        if (autoStart)
            StartCoroutine(SpawnClients());
    }

    public IEnumerator SpawnClients()
    {
        if (clientPrefab == null)
        {
            yield break;
        }

        while (true)
        {
            while (!ClientQueue.Instance.CanSpawnClient(maxQueueWaitingClients))
            {
                yield return new WaitForSeconds(1f);
            }

            var client = Instantiate(clientPrefab);
            client.name = $"Client_{++spawnedCount}";

            // Assegna i riferimenti PRIMA di attivarlo
            ClientAI ai = client.GetComponent<ClientAI>();
            ai.spawnPoint = ClientQueue.Instance.spawnPoint;
            ai.orderPoints = ClientQueue.Instance.orderPoints;
            ai.endRoute = ClientQueue.Instance.endRoute;

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}