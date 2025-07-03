using UnityEngine;
using System.Collections;

/// <summary>
/// Spawna i client a intervalli regolari, rispettando il limite massimo di attesa.
/// </summary>
public class ClientSpawner : MonoBehaviour
{
    [Header("Prefab e Spawn")]
    [Tooltip("Prefab del client da spawnare.")]
    public GameObject clientPrefab;

    [Tooltip("Punto di spawn del client.")]
    public Transform spawnPoint;

    [Header("Impostazioni Spawn")]
    [Tooltip("Ritardo tra uno spawn e l'altro (secondi).")]
    public float spawnDelay = 1f;

    [Tooltip("Numero massimo di client in attesa nella coda.")]
    public int maxQueueWaitingClients = 3;

    [Tooltip("Avvia automaticamente lo spawn all'avvio.")]
    public bool autoStart = true;

    private int spawnedCount = 0;

    private void Start()
    {
        if (autoStart)
        {
            StartCoroutine(SpawnClients());
        }
    }

    /// <summary>
    /// Coroutine che gestisce lo spawn dei client.
    /// </summary>
    public IEnumerator SpawnClients()
    {
        if (clientPrefab == null)
        {
            Debug.LogError("ClientSpawner: clientPrefab non è assegnato!");
            yield break;
        }

        while (true)
        {
            // Attende finché la coda ha raggiunto il limite massimo
            while (!ClientQueue.Instance.CanSpawnClient(maxQueueWaitingClients))
            {
                yield return new WaitForSeconds(1f);
            }

            var client = Instantiate(clientPrefab, spawnPoint.position, spawnPoint.rotation);
            client.name = $"Client_{++spawnedCount}";

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}