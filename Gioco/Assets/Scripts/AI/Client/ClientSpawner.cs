using UnityEngine;
using System.Collections;

public class ClientSpawner : MonoBehaviour
{
    [Header("Client Prefabs da spawnare")]
    public GameObject[] clientPrefabs; // Tutti i prefab NPC qui

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
        if (clientPrefabs == null || clientPrefabs.Length == 0)
        {
            Debug.LogError("⚠️ Nessun prefab assegnato in ClientSpawner.");
            yield break;
        }

        while (true)
        {
            while (!ClientQueue.Instance.CanSpawnClient(maxQueueWaitingClients))
            {
                yield return new WaitForSeconds(1f);
            }

            // Scegli un prefab casuale
            GameObject selectedPrefab = clientPrefabs[Random.Range(0, clientPrefabs.Length)];

            var client = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
            client.name = $"Client_{++spawnedCount}";

            // Assegna i riferimenti prima di attivarlo
            ClientAI ai = client.GetComponent<ClientAI>();
            ai.spawnPoint = ClientQueue.Instance.spawnPoint;
            ai.orderPoints = ClientQueue.Instance.orderPoints;
            ai.endRoute = ClientQueue.Instance.endRoute;

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
