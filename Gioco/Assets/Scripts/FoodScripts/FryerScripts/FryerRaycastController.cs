using UnityEngine;

public class FryerRaycastController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                FryerBasketMover fryer = hit.collider.GetComponentInParent<FryerBasketMover>();
                if (fryer != null)
                {
                    fryer.ToggleBasket();
                }
                else
                {
                    Debug.Log("Non stai guardando una friggitrice.");
                }
            }
        }
    }
}
