using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DeliverOrder : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;
    [SerializeField] private PlaceOnTray placeOnTrayScript;
    [SerializeField] private Order order;
    void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (hand == null)
            Debug.LogError("Hand transform is not assigned in the inspector.");

        if (pickUpAndPlaceScript == null)
            pickUpAndPlaceScript = GetComponent<PickUpAndPlace>();

        if (placeOnTrayScript == null)
            placeOnTrayScript = GetComponent<PlaceOnTray>(); 
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickUpAndPlaceScript.pickUpRange))
        {
            if (hit.collider.CompareTag("Client") && Input.GetMouseButtonDown(0) && hand.GetChild(0).name.Contains("Tray"))
            {
                bool isRight = TryDeliverOrder(hand.GetChild(0).gameObject);
                Debug.Log(isRight ? "Order delivered successfully!" : "Order is incorrect!");
            }
        }
    }

    private bool TryDeliverOrder(GameObject tray)
    {
        List<GameObject> trayObjects = placeOnTrayScript.GetObjectsOnTray(tray.transform);
        Debug.Log("Objects on tray: " + string.Join(", ", trayObjects.Select(go => go.name)));
        List<Ingredient> ingredientsOnTray = placeOnTrayScript.ConvertObjectsToIngredients(trayObjects);

        Debug.Log("Ingredients on tray: " + string.Join(", ", ingredientsOnTray));
        bool orderIsCorrect = order.Matches(ingredientsOnTray);

        return orderIsCorrect;
    }
}
