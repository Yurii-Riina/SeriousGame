using UnityEngine;
using System.Collections.Generic;

public class FryerCookedSpawner : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;

    [System.Serializable]
    public class CookedSpawnPoint
    {
        public GrillableType type;
        public Transform spawnPoint;
        public GameObject readyPrefab;
    }

    [Header("Mapping Tipo ➜ Spawn Point e Prefab Ready")]
    [SerializeField] private List<CookedSpawnPoint> spawnMappings;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                FryerBasketMover fryer = hit.collider.GetComponentInParent<FryerBasketMover>();
                if (fryer != null)
                {
                    TransferCookedObjects(fryer);
                }
                else
                {
                    Debug.Log("Non stai mirando a una friggitrice.");
                }
            }
        }
    }

    private void TransferCookedObjects(FryerBasketMover fryer)
    {
        bool anyTransferred = false;

        foreach (Transform slot in fryer.GetComponentsInChildren<Transform>())
        {
            if (slot.childCount == 0)
                continue;

            Transform cooked = slot.GetChild(0);
            if (cooked.GetComponent<CookedMarker>() == null)
                continue;

            var grillable = cooked.GetComponent<Grillable>();
            if (grillable == null)
            {
                Debug.LogError("Oggetto cotto senza componente Grillable.");
                continue;
            }

            // Trova TUTTI i mapping disponibili di questo tipo
            var availableMappings = spawnMappings.FindAll(m => m.type == grillable.type);

            // Trova il primo spawn point libero
            CookedSpawnPoint mapping = null;
            foreach (var m in availableMappings)
            {
                if (m.spawnPoint.childCount == 0)
                {
                    mapping = m;
                    break;
                }
            }

            if (mapping == null)
            {
                Debug.LogError("Nessun mapping configurato per tipo: " + grillable.type);
                continue;
            }

            // Elimina l'oggetto cotto dal cestello
            Destroy(cooked.gameObject);

            // Istanzia il prefab pronto e lo rende figlio dello spawn point
            GameObject spawned = Instantiate(mapping.readyPrefab, mapping.spawnPoint.position, mapping.spawnPoint.rotation);
            spawned.transform.SetParent(mapping.spawnPoint, worldPositionStays: true);

            Debug.Log($"Prefab {mapping.readyPrefab.name} spawnato in {mapping.spawnPoint.name} come figlio");

            anyTransferred = true;
        }

        if (!anyTransferred)
        {
            Debug.Log("Nessun oggetto cotto da trasferire.");
        }
    }
}
