using UnityEngine;
using System.Collections;

public class GrillCookingManager : MonoBehaviour
{
    [Header("Slot di appoggio")]
    [SerializeField] private Transform[] grillSlots;

    [Header("Prefab")]
    [SerializeField] private GameObject cookedHamburgerPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip grillOpenSound;
    [SerializeField] private AudioClip grillCloseSound;
    [SerializeField] private AudioClip cookingSound;
    [SerializeField] private AudioClip finishedSound;

    [Header("Parametri")]
    public float cookingTime = 5f; //messo publico per il tutorial

    [Header("Gestione contenitore")]
    [SerializeField] private HamburgerContainerFixedManager containerManager;

    private bool[] isSlotOccupied;
    [HideInInspector] public bool[] isCooking; //pubblico perchè ci serve per il tutorial

    private void Awake()
    {
        isSlotOccupied = new bool[grillSlots.Length];
        isCooking = new bool[grillSlots.Length];
    }

    /// <summary>
    /// Piazza un hamburger nello slot libero.
    /// </summary>
    public bool TryPlaceObject(GameObject obj)
    {
        for (int i = 0; i < grillSlots.Length; i++)
        {
            if (!isSlotOccupied[i])
            {
                obj.transform.SetParent(grillSlots[i]);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;

                isSlotOccupied[i] = true;
                Debug.Log($"Hamburger piazzato nello slot {i}");
                return true;
            }
        }

        Debug.LogWarning("Tutti gli slot occupati.");
        return false;
    }

    /// <summary>
    /// Avvia la cottura quando chiudi la griglia.
    /// </summary>
    public void StartCooking()
    {
        for (int i = 0; i < grillSlots.Length; i++)
        {
            bool slotHasObject = grillSlots[i].childCount > 0;

            if (slotHasObject && !isCooking[i])
            {
                StartCoroutine(CookSlot(i));
            }
        }

        if (audioSource && grillCloseSound)
            audioSource.PlayOneShot(grillCloseSound);

        Debug.Log("Cottura iniziata.");
    }

    /// <summary>
    /// Suono di apertura griglia.
    /// </summary>
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
        Transform rawBurger = slot.childCount > 0 ? slot.GetChild(0) : null;

        if (rawBurger != null)
        {
            Destroy(rawBurger.gameObject);

            GameObject cookedBurger = Instantiate(
                cookedHamburgerPrefab,
                slot.position,
                slot.rotation,
                slot
            );

            cookedBurger.name = "CookedMeat";

            Debug.Log($"Slot {index}: Cottura completata!");

            if (audioSource && finishedSound)
                audioSource.PlayOneShot(finishedSound);
        }

        isCooking[index] = false;
        isSlotOccupied[index] = false;
    }

    public Transform[] GetSlots()
    {
        return grillSlots;
    }
}
