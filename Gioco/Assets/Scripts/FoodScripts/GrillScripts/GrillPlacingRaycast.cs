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
                    Debug.LogWarning("L'oggetto non può essere piazzato sul grill.");
                    return;
                }

                // Cerchiamo il GrillCookingManager
                GrillCookingManager cookingManager = hit.collider.GetComponentInParent<GrillCookingManager>();
                if (cookingManager != null)
                {
                    if (cookingManager.TryPlaceObject(held))
                    {
                        pickUpAndPlace.ClearHeldObject();
                    }
                    else
                    {
                        Debug.LogWarning("Nessuno slot libero nel GrillCookingManager.");
                    }
                }
                else
                {
                    Debug.LogWarning("Nessun GrillCookingManager trovato sul target.");
                }
            }
            else
            {
                Debug.Log("Raycast non ha colpito nulla.");
            }
        }
    }
}
