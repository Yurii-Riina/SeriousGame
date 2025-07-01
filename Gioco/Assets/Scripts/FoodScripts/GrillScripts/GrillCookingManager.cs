using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrillCookingManager : MonoBehaviour
{
    [Header("Slot di appoggio")]
    [SerializeField] private Transform[] grillSlots;

    [System.Serializable]
    public class GrillablePrefab
    {
        public GrillableType type;
        public GameObject cookedPrefab;
    }

    [Header("Prefab cotti")]
    [SerializeField] private List<GrillablePrefab> cookedPrefabs;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip grillOpenSound;
    [SerializeField] private AudioClip grillCloseSound;
    [SerializeField] private AudioClip cookingSound;
    [SerializeField] private AudioClip finishedSound;

    [Header("Parametri")]
    [SerializeField] private float cookingTime = 5f;

    private bool[] isSlotOccupied;
    private bool[] isCooking;

    public bool[] IsCooking => isCooking;
    public float CookingTime => cookingTime;

    private void Awake()
    {
        isSlotOccupied = new bool[grillSlots.Length];
        isCooking = new bool[grillSlots.Length];
    }

    private void Update()
    {
        // Ogni frame verifica se gli slot si sono svuotati
        for (int i = 0; i < grillSlots.Length; i++)
        {
            if (grillSlots[i].childCount == 0)
            {
                isSlotOccupied[i] = false;
            }
        }
    }

    public bool TryPlaceObject(GameObject obj)
    {
        for (int i = 0; i < grillSlots.Length; i++)
        {
            if (!isSlotOccupied[i])
            {
                PlaceInSlot(obj, i);
                return true;
            }
        }

        Debug.LogWarning("Tutti gli slot occupati.");
        return false;
    }

    /// <summary>
    /// Prova a piazzare l'oggetto nello slot indicato.
    /// </summary>
    public bool TryPlaceObjectInSlot(GameObject obj, int index)
    {
        if (index < 0 || index >= grillSlots.Length)
        {
            Debug.LogWarning("Indice slot non valido.");
            return false;
        }

        if (isSlotOccupied[index])
        {
            Debug.LogWarning($"Slot {index} già occupato.");
            return false;
        }

        PlaceInSlot(obj, index);
        return true;
    }

    private void PlaceInSlot(GameObject obj, int index)
    {
        obj.transform.SetParent(grillSlots[index]);

        var grillable = obj.GetComponent<Grillable>();
        if (grillable != null && grillable.type == GrillableType.Bacon)
        {
            obj.transform.localPosition = new Vector3(0, 0f, 0.015f);
        }
        else
        {
            obj.transform.localPosition = Vector3.zero;
        }

        obj.transform.localRotation = Quaternion.identity;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        isSlotOccupied[index] = true;
        Debug.Log($"Oggetto piazzato nello slot {index}");
    }

    public void StartCooking()
    {
        for (int i = 0; i < grillSlots.Length; i++)
        {
            if (grillSlots[i].childCount > 0 && !isCooking[i])
            {
                StartCoroutine(CookSlot(i));
            }
        }

        if (audioSource && grillCloseSound)
            audioSource.PlayOneShot(grillCloseSound);

        Debug.Log("Cottura iniziata.");
    }

    public void PlayOpenSound()
    {
        if (audioSource && grillOpenSound)
            audioSource.PlayOneShot(grillOpenSound);
    }

    private IEnumerator CookSlot(int index)
    {
        isCooking[index] = true;

        if (audioSource && cookingSound)
            audioSource.PlayOneShot(cookingSound);

        yield return new WaitForSeconds(cookingTime);

        Transform slot = grillSlots[index];
        Transform rawItem = slot.childCount > 0 ? slot.GetChild(0) : null;

        if (rawItem != null)
        {
            var grillable = rawItem.GetComponent<Grillable>();
            if (grillable == null)
            {
                Debug.LogWarning("Oggetto nello slot non ha Grillable.");
                yield break;
            }

            GameObject prefab = cookedPrefabs.Find(p => p.type == grillable.type)?.cookedPrefab;
            if (prefab == null)
            {
                Debug.LogError("Prefab cotto mancante per " + grillable.type);
                yield break;
            }

            Destroy(rawItem.gameObject);

            GameObject cooked = Instantiate(
                prefab,
                slot.position,
                slot.rotation,
                slot
            );

            // Offset per bacon
            var grillableCooked = cooked.GetComponent<Grillable>();
            if (grillableCooked != null && grillableCooked.type == GrillableType.Bacon)
            {
                cooked.transform.localPosition = new Vector3(0, 0f, 0.015f);
            }
            else
            {
                cooked.transform.localPosition = Vector3.zero;
            }

            Debug.Log($"Slot {index}: Cottura completata per {grillable.type}.");

            if (audioSource && finishedSound)
                audioSource.PlayOneShot(finishedSound);
        }

        isCooking[index] = false;
        // Qui non azzero isSlotOccupied perché il nuovo oggetto cotto resta nello slot
        // Sarà l'Update() a liberarlo quando lo togli
    }

    public Transform[] GetSlots()
    {
        return grillSlots;
    }
}
