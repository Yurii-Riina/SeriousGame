/*using UnityEngine;

public class Client : MonoBehaviour
{
    [SerializeField] private ClientAI clientAI;

    void Start()
    {
        if (clientAI != null && clientAI.spawnPoint != null)
        {
            transform.position = clientAI.spawnPoint.position;
            transform.rotation = clientAI.spawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("ClientAI o SpawnPoint non assegnato.");
        }
    }
}*/