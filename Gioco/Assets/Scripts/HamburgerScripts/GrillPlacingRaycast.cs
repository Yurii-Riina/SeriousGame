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
                // Cerchiamo un oggetto che in gerarchia abbia GrillSlotPlacer
                Transform current = hit.collider.transform;
                GrillSlotPlacer placer = null;

                CanBePlacedOnGrill canBePlaced = held.GetComponent<CanBePlacedOnGrill>();
                if(canBePlaced == null || !canBePlaced.canBePlacedOnGrill)
                {
                    Debug.LogWarning("L'oggetto non può essere piazzato sul grill.");
                    return;
                }

                while (current != null)
                {
                    placer = current.GetComponent<GrillSlotPlacer>();
                    if (placer != null)
                        break;

                    current = current.parent;
                }

                if (placer != null)
                {
                    if (placer.TryPlaceObject(held))
                    {
                        pickUpAndPlace.ClearHeldObject();
                    }
                }
                else
                {
                    Debug.LogWarning("Nessun GrillSlotPlacer trovato nel target colpito.");
                }
            }
            else
            {
                Debug.Log("Raycast non ha colpito nulla.");
            }
        }
    }
}
