using UnityEngine;

public class GrillPlacingRaycast : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;
    [SerializeField] private PickUpAndPlace pickUpAndPlace;

    public void HandleCButtonClick()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameObject held = pickUpAndPlace.GetHeldObject();
            if (held == null)
                return;

            // Raycast davanti al player
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                CanBePlacedOnGrill canBePlaced = held.GetComponent<CanBePlacedOnGrill>();
                if (canBePlaced == null || !canBePlaced.canBePlacedOnGrill)
                {
                    Debug.LogWarning("L'oggetto non può essere piazzato su questa macchina.");
                    return;
                }

                // 1️⃣ Verifica GrillCookingManager
                GrillCookingManager grillManager = hit.collider.GetComponentInParent<GrillCookingManager>();
                if (grillManager != null)
                {
                    if (grillManager.TryPlaceObject(held))
                    {
                        pickUpAndPlace.ClearHeldObject();
                    }
                    else
                    {
                        Debug.LogWarning("Nessuno slot libero nel GrillCookingManager.");
                    }
                    return;
                }

                // 2️⃣ Verifica FryerController
                FryerBasketMover fryerController = hit.collider.GetComponentInParent<FryerBasketMover>();
                if (fryerController != null)
                {
                    FryerSlotManager fryerSlots = fryerController.GetComponent<FryerSlotManager>();
                    if (fryerSlots != null)
                    {
                        if (fryerSlots.TryPlaceObject(held))
                        {
                            pickUpAndPlace.ClearHeldObject();
                        }
                        else
                        {
                            Debug.LogWarning("Nessuno slot libero nella Fryer.");
                        }
                    }
                    else
                    {
                        Debug.LogError("FryerController trovato ma manca FryerSlotManager.");
                    }
                    return;
                }

                Debug.LogWarning("Nessuna macchina di cottura trovata sul target.");
            }
            else
            {
                Debug.Log("Raycast non ha colpito nulla.");
            }
        }
    }
}
