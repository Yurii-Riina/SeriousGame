using UnityEngine;

public class HamburgerContainerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;
    [SerializeField] private HamburgerContainerFixedManager containerManager;
    [SerializeField] private PickUpAndPlace pickUpAndPlace;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Premuto F, inizio raycast per prelevare hamburger.");

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log($"Raycast colpito: {hit.collider.name}");

                // Risali i genitori per trovare HamburgerContainerFixedManager
                HamburgerContainerFixedManager manager = hit.collider.GetComponentInParent<HamburgerContainerFixedManager>();

                if (manager != null)
                {
                    GameObject taken = manager.TakeRandomHamburger();
                    if (taken != null)
                    {
                        Rigidbody rb = taken.GetComponent<Rigidbody>();
                        Collider col = taken.GetComponent<Collider>();

                        if (rb != null && col != null)
                        {
                            pickUpAndPlace.SetHeldObject(rb, col);
                            Debug.Log("Hamburger prelevato e messo in mano.");
                        }
                        else
                        {
                            Debug.LogWarning("L'hamburger non ha Rigidbody o Collider.");
                        }
                    }
                    else
                    {
                        Debug.Log("Nessun hamburger disponibile nel contenitore.");
                    }
                }
                else
                {
                    Debug.LogWarning("Nessun HamburgerContainerFixedManager trovato. Controlla i collider del contenitore.");
                }
            }
            else
            {
                Debug.Log("Raycast non ha colpito nulla.");
            }
        }
    }
}
