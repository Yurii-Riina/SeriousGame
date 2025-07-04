using UnityEngine;
using System.Collections.Generic;

public class FryerPackagingSpawner : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;

    [Header("Prefabs dei pacchetti pronti")]
    [SerializeField] private GameObject cookedFriesPackPrefab;
    [SerializeField] private GameObject cookedNuggetsPackPrefab;

    [Header("Slot di spawn disponibili (14 slot generici)")]
    [SerializeField] private List<Transform> allSpawnSlots;

    public List<Transform> AllSpawnSlots => allSpawnSlots;

    private const int MaxPackagesPerInteraction = 4;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log("Ho colpito: " + hit.collider.name + " su GameObject: " + hit.collider.gameObject.name);

                // Prima cerco un CookedMarker in tutti i possibili punti
                CookedMarker cookedMarker = hit.collider.GetComponent<CookedMarker>()
                    ?? hit.collider.GetComponentInParent<CookedMarker>()
                    ?? hit.collider.GetComponentInChildren<CookedMarker>();

                if (cookedMarker == null)
                {
                    Debug.LogError("ERRORE: Nessun CookedMarker trovato sull'oggetto colpito.");
                    return;
                }

                // Se ho trovato il CookedMarker, recupero il Grillable
                Grillable grillable = cookedMarker.GetComponent<Grillable>();
                if (grillable == null)
                {
                    Debug.LogError("ERRORE: L'oggetto colpito ha CookedMarker ma non Grillable.");
                    return;
                }

                // Genera i pacchetti
                SpawnPackages(grillable);

                // Distruggi l'oggetto originale
                Destroy(cookedMarker.gameObject);

                Debug.Log("Pacchetti creati e oggetto rimosso.");
            }
        }
    }



    private void SpawnPackages(Grillable grillable)
    {
        int packagesToSpawn = 0;

        // Determina quanti pacchetti creare
        if (grillable.type == GrillableType.Fries)
            packagesToSpawn = 5;
        else if (grillable.type == GrillableType.Nuggets)
            packagesToSpawn = 4;
        else
        {
            Debug.LogError("Tipo di Grillable non gestito: " + grillable.type);
            return;
        }

        int packagesSpawned = 0;

        foreach (var slot in allSpawnSlots)
        {
            if (slot.childCount > 0)
                continue;

            // Scegli il prefab in base al tipo
            GameObject prefabToSpawn = null;
            if (grillable.type == GrillableType.Fries)
                prefabToSpawn = cookedFriesPackPrefab;
            else if (grillable.type == GrillableType.Nuggets)
                prefabToSpawn = cookedNuggetsPackPrefab;

            GameObject spawned = Instantiate(prefabToSpawn, slot.position, slot.rotation);
            spawned.name = prefabToSpawn.name;
            spawned.transform.SetParent(slot, worldPositionStays: true);

            packagesSpawned++;

            if (packagesSpawned >= packagesToSpawn)
                break;
        }

        if (packagesSpawned == 0)
        {
            Debug.LogWarning("Nessuno slot libero trovato: nessun pacchetto creato.");
        }
        else if (packagesSpawned < packagesToSpawn)
        {
            Debug.LogWarning($"Solo {packagesSpawned} slot liberi trovati su {packagesToSpawn} previsti.");
        }
    }
}
