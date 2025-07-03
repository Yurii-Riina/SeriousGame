using UnityEngine;
using System.Collections.Generic;

public class FryerSlotManager : MonoBehaviour
{
    [SerializeField] private Transform[] fryerSlots;

    [System.Serializable]
    public class FryableSpawn
    {
        public GrillableType type;
        public GameObject rawPrefab;
        public int slotIndex; // aggiunto: indice slot dedicato
    }

    [Header("Prefab crudi da spawnare")]
    [SerializeField] private List<FryableSpawn> rawPrefabsToSpawn;

    public bool TryPlaceObject(GameObject heldObject)
    {
        if (heldObject == null)
        {
            Debug.LogError("Nessun oggetto in mano.");
            return false;
        }

        Grillable grillable = heldObject.GetComponent<Grillable>();
        if (grillable == null)
        {
            Debug.LogError("Oggetto non contiene Grillable.");
            return false;
        }

        // Trova configurazione per questo tipo
        var prefabEntry = rawPrefabsToSpawn.Find(p => p.type == grillable.type);
        if (prefabEntry == null || prefabEntry.rawPrefab == null)
        {
            Debug.LogError("Prefab crudo da spawnare non configurato per " + grillable.type);
            return false;
        }

        // Verifica indice valido
        if (prefabEntry.slotIndex < 0 || prefabEntry.slotIndex >= fryerSlots.Length)
        {
            Debug.LogError("Indice slot non valido per " + grillable.type);
            return false;
        }

        Transform slot = fryerSlots[prefabEntry.slotIndex];

        // Verifica se lo slot è vuoto
        if (slot.childCount > 0)
        {
            Debug.LogWarning("Slot già occupato per " + grillable.type);
            return false;
        }

        // Distrugge quello in mano
        Destroy(heldObject);

        // Istanzia prefab corretto
        GameObject spawned = Instantiate(prefabEntry.rawPrefab, slot.position, slot.rotation);
        spawned.transform.SetParent(slot);
        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localRotation = Quaternion.identity;

        Debug.Log($"Spawnato prefab {prefabEntry.rawPrefab.name} in {slot.name}");
        return true;
    }
}
