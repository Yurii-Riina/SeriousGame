using UnityEngine;
using System.Collections;

public class GrillController : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform grillDoor;            // Lo sportello che ruota
    [SerializeField] private Transform grillAxis;            // L'asse di rotazione (barra bianca)
    [SerializeField] private Transform[] cookingSlots;       // Gli slot dove mettere hamburger crudi
    [SerializeField] private GameObject cookedHamburgerPrefab; // Prefab hamburger cotto
    [SerializeField] private Transform cookedContainer;      // Il contenitore che riceve gli hamburger
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cookingSound;
    [SerializeField] private AudioClip finishedSound;

    [Header("Parametri")]
    [SerializeField] private float rotationSpeed = 2f;      // Velocità apertura/chiusura
    [SerializeField] private float openAngle = 90f;         // Angolo di apertura
    [SerializeField] private float cookingTime = 10f;       // Durata cottura in secondi

    private bool isOpen = false;
    private bool isCooking = false;
    private Coroutine cookingCoroutine;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleGrill();
        }
    }

    public void ToggleGrill()
    {
        if (isOpen)
        {
            StartCoroutine(RotateGrillDoor(0f)); // chiudi
            isOpen = false;

            // Se ci sono hamburger crudi e non sta già cucinando, parte la cottura
            if (!isCooking && HasRawHamburgers())
            {
                cookingCoroutine = StartCoroutine(CookHamburgers());
            }
        }
        else
        {
            if (isCooking)
            {
                Debug.Log("Non puoi aprire durante la cottura!");
                return;
            }

            StartCoroutine(RotateGrillDoor(openAngle)); // apri
            isOpen = true;
        }
    }

    private IEnumerator RotateGrillDoor(float targetAngle)
    {
        float currentAngle = grillDoor.localEulerAngles.x;
        if (currentAngle > 180f) currentAngle -= 360f;

        while (Mathf.Abs(currentAngle - targetAngle) > 0.5f)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, rotationSpeed * Time.deltaTime * 100f);
            grillDoor.localEulerAngles = new Vector3(currentAngle, 0f, 0f);
            yield return null;
        }

        grillDoor.localEulerAngles = new Vector3(targetAngle, 0f, 0f);
    }

    private bool HasRawHamburgers()
    {
        foreach (Transform slot in cookingSlots)
        {
            if (slot.childCount > 0)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator CookHamburgers()
    {
        isCooking = true;

        if (audioSource && cookingSound)
            audioSource.PlayOneShot(cookingSound);

        Debug.Log("Inizio cottura...");

        yield return new WaitForSeconds(cookingTime);

        Debug.Log("Cottura terminata!");

        if (audioSource && finishedSound)
            audioSource.PlayOneShot(finishedSound);

        // Trasforma tutti gli hamburger crudi in cotti
        foreach (Transform slot in cookingSlots)
        {
            if (slot.childCount > 0)
            {
                Transform rawBurger = slot.GetChild(0);
                Destroy(rawBurger.gameObject);

                GameObject cookedBurger = Instantiate(
                    cookedHamburgerPrefab,
                    slot.position,
                    Quaternion.identity
                );

                cookedBurger.name = cookedHamburgerPrefab.name;
                cookedBurger.transform.SetParent(slot);
            }
        }

        isCooking = false;
    }
}
