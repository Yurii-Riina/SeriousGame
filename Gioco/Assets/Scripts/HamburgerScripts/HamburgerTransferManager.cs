using UnityEngine;

public class HamburgerTransferManager : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;
    [SerializeField] private HamburgerContainerFixedManager containerManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Premuto Q, inizio raycast.");

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log($"Colpito: {hit.collider.name}");

                // Cerchiamo il GrillCookingManager partendo dal collider colpito o dal suo parent
                GrillCookingManager cookingManager = null;

                Transform current = hit.collider.transform;

                while (current != null)
                {
                    cookingManager = current.GetComponent<GrillCookingManager>();
                    if (cookingManager != null)
                        break;

                    current = current.parent;
                }

                if (cookingManager != null)
                {
                    Debug.Log("Trovato GrillCookingManager.");
                    TransferCookedHamburgers(cookingManager);
                }
                else
                {
                    Debug.LogWarning("Nessun GrillCookingManager trovato. Assicurati di mirare la griglia stessa, non l'hamburger.");
                }
            }
            else
            {
                Debug.Log("Raycast non ha colpito nulla.");
            }
        }
    }

    private void TransferCookedHamburgers(GrillCookingManager grillManager)
    {
        foreach (Transform slot in grillManager.GetSlots())
        {
            if (slot.childCount > 0)
            {
                Transform candidate = slot.GetChild(0);

                if (candidate.GetComponent<CookedMarker>() != null)
                {
                    containerManager.TryPlaceCookedHamburger(candidate.gameObject);
                }
                else
                {
                    Debug.Log($"Slot {slot.name}: Oggetto senza CookedMarker, ignorato.");
                }
            }
        }
    }
}
