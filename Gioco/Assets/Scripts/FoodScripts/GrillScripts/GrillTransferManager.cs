using UnityEngine;
using System.Collections.Generic;

public class GrillTransferManager : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;

    [System.Serializable]
    public class GrillContainer
    {
        public GrillableType type;
        public CookedFoodContainerManager container;
    }

    [SerializeField] private List<GrillContainer> containerMappings;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Premuto Q, inizio raycast per trasferimento.");

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                GrillCookingManager cookingManager = hit.collider.GetComponentInParent<GrillCookingManager>();

                if (cookingManager != null)
                {
                    TransferCookedObjects(cookingManager);
                }
                else
                {
                    Debug.LogWarning("Nessun GrillCookingManager trovato.");
                }
            }
            else
            {
                Debug.Log("Raycast non ha colpito nulla.");
            }
        }
    }

    private void TransferCookedObjects(GrillCookingManager grillManager)
    {
        foreach (Transform slot in grillManager.GetSlots())
        {
            if (slot.childCount > 0)
            {
                Transform candidate = slot.GetChild(0);

                if (candidate.GetComponent<CookedMarker>() == null)
                {
                    Debug.Log($"Oggetto {candidate.name} non cotto, ignorato.");
                    continue;
                }

                var grillable = candidate.GetComponent<Grillable>();
                if (grillable == null)
                {
                    Debug.Log("Oggetto senza Grillable, ignorato.");
                    continue;
                }

                var containerEntry = containerMappings.Find(c => c.type == grillable.type);
                if (containerEntry == null || containerEntry.container == null)
                {
                    Debug.LogError("Nessun contenitore trovato per " + grillable.type);
                    continue;
                }

                candidate.SetParent(null);

                containerEntry.container.TryPlaceCookedFood(candidate.gameObject);

                Debug.Log($"Oggetto {grillable.type} trasferito nel contenitore.");
            }
        }
    }
}
