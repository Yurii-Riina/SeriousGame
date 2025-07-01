using UnityEngine;

public class FoodContainerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;
    [SerializeField] private CookedFoodContainerManager containerManager;
    [SerializeField] private PickUpAndPlace pickUpAndPlace;

    private bool fKeyHeld = false;

    public void HandleFButtonClick()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (fKeyHeld)
                return;

            fKeyHeld = true;

            Debug.Log("Premuto F, inizio raycast per prelevare cibo cotto.");

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log($"Raycast colpito: {hit.collider.name}");

                CookedFoodContainerManager manager = hit.collider.GetComponentInParent<CookedFoodContainerManager>();

                if (manager != null)
                {
                    GameObject taken = manager.TakeRandomFood();
                    if (taken != null)
                    {
                        Rigidbody rb = taken.GetComponent<Rigidbody>();
                        Collider col = taken.GetComponent<Collider>();

                        if (rb != null && col != null)
                        {
                            pickUpAndPlace.SetHeldObject(rb, col);
                            Debug.Log("Cibo cotto prelevato e messo in mano.");
                        }
                        else
                        {
                            Debug.LogWarning("Il cibo cotto non ha Rigidbody o Collider.");
                        }
                    }
                    else
                    {
                        Debug.Log("Nessun cibo cotto disponibile nel contenitore.");
                    }
                }
                else
                {
                    Debug.LogWarning("Nessun CookedFoodContainerManager trovato.");
                }
            }
            else
            {
                Debug.Log("Raycast non ha colpito nulla.");
            }
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            fKeyHeld = false;
        }
    }
}
