using UnityEngine;

public class ToasterManager : MonoBehaviour
{
    [Header("Slot di carico (dove il pane crudo viene messo)")]
    [SerializeField] private Transform[] loadSlots;

    [Header("Slot di spawn (dove il pane cotto appare)")]
    [SerializeField] private Transform[] spawnSlots;

    [Header("Prefabs del pane cotto")]
    [SerializeField] private GameObject cookedBottomBreadPrefab;
    [SerializeField] private GameObject cookedTopBreadPrefab;

    [Header("Tempo di cottura")]
    public float cookingTime = 5f;

    [Header("Riferimenti")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;
    [SerializeField] private PickUpAndPlace pickUpAndPlace;

    private bool[] isSlotOccupied;
    public bool[] IsCooking => isSlotOccupied;
    private float[] cookingTimers;
    private GameObject[] currentRawBread;
    private GameObject[] correspondingCookedPrefab;

    private void Awake()
    {
        isSlotOccupied = new bool[loadSlots.Length];
        cookingTimers = new float[loadSlots.Length];
        currentRawBread = new GameObject[loadSlots.Length];
        correspondingCookedPrefab = new GameObject[loadSlots.Length];
    }

    private void Update()
    {
        // Gestione tasto C per inserimento pane
        if (Input.GetKeyDown(KeyCode.C))
        {
            HandleBreadInsertion();
        }

        // Aggiornamento cottura per ciascun slot
        for (int i = 0; i < loadSlots.Length; i++)
        {
            if (isSlotOccupied[i])
            {
                cookingTimers[i] -= Time.deltaTime;
                if (cookingTimers[i] <= 0f)
                {
                    CompleteCooking(i);
                }
            }
        }
    }

    private void HandleBreadInsertion()
    {
        GameObject held = pickUpAndPlace.GetHeldObject();
        if (held == null)
        {
            Debug.Log("Nessun oggetto in mano.");
            return;
        }

        // Raycast per verificare se stai guardando il tostapane
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.collider.transform != this.transform && hit.collider.transform.parent != this.transform)
            {
                Debug.Log("Non stai guardando il tostapane.");
                return;
            }

            // Identifica il tipo di pane con BreadType
            BreadType breadType = held.GetComponent<BreadType>();
            if (breadType == null)
            {
                Debug.LogWarning("Questo oggetto non ha BreadType.");
                return;
            }

            GameObject cookedPrefab = null;
            if (breadType.kind == BreadKind.Bottom)
            {
                cookedPrefab = cookedBottomBreadPrefab;
            }
            else if (breadType.kind == BreadKind.Top)
            {
                cookedPrefab = cookedTopBreadPrefab;
            }

            // Determina l'indice dello slot corretto in base al tipo di pane
            int targetSlotIndex = -1;

            if (breadType.kind == BreadKind.Bottom)
                targetSlotIndex = 0;
            else if (breadType.kind == BreadKind.Top)
                targetSlotIndex = 1;

            // Controlla se lo slot è libero
            if (targetSlotIndex >= 0 && targetSlotIndex < loadSlots.Length)
            {
                if (!isSlotOccupied[targetSlotIndex] && loadSlots[targetSlotIndex].childCount == 0)
                {
                    PlaceBreadInSlot(held, cookedPrefab, targetSlotIndex);
                    pickUpAndPlace.ClearHeldObject();
                    return;
                }
                else
                {
                    Debug.LogWarning($"Lo slot {targetSlotIndex} per il pane {breadType.kind} � occupato.");
                    return;
                }
            }

            Debug.LogWarning("Tipo di pane non riconosciuto o indice slot non valido.");

            Debug.Log("Tutti gli slot del tostapane sono occupati.");
        }
        else
        {
            Debug.Log("Raycast non ha colpito nulla.");
        }
    }

    private void PlaceBreadInSlot(GameObject bread, GameObject cookedPrefab, int index)
    {
        isSlotOccupied[index] = true; // Importante: marcare subito occupato

        bread.transform.SetParent(loadSlots[index]);
        bread.transform.localPosition = Vector3.zero;
        bread.transform.localRotation = Quaternion.identity;

        Rigidbody rb = bread.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        cookingTimers[index] = cookingTime;
        currentRawBread[index] = bread;
        correspondingCookedPrefab[index] = cookedPrefab;

        Debug.Log($"Pane inserito nello slot {index} e cottura avviata.");
    }

    private void CompleteCooking(int index)
    {
        // Distruggi il pane crudo
        if (currentRawBread[index] != null)
        {
            Destroy(currentRawBread[index]);
        }

        // Instanzia il pane cotto come figlio dello spawn slot
        GameObject cooked = Instantiate(
            correspondingCookedPrefab[index],
            spawnSlots[index]);

        cooked.transform.localPosition = Vector3.zero;
        cooked.transform.localRotation = Quaternion.identity;

        cooked.name = correspondingCookedPrefab[index].name; //teniamo il nome del prefab per tutorial e placing

        isSlotOccupied[index] = false;
        cookingTimers[index] = 0f;
        currentRawBread[index] = null;
        correspondingCookedPrefab[index] = null;

        Debug.Log($"Cottura completata nello slot {index}.");
    }
}
