using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DeliverOrder : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private PlaceOnTray placeOnTrayScript;
    [SerializeField] private float interactionRange = 3f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryDeliver();
        }
    }

    private void TryDeliver()
    {
        if (hand.childCount == 0)
            return;

        GameObject heldObject = hand.GetChild(0).gameObject;

        if (!heldObject.name.Contains("Tray"))
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            ClientAI client = hit.collider.GetComponent<ClientAI>();
            if (client != null && client.IsOrdering())
            {
                var trayObjects = placeOnTrayScript.GetObjectsOnTray(heldObject.transform);
                var ingredients = placeOnTrayScript.ConvertObjectsToIngredients(trayObjects);

                bool correct = client.TryDeliverOrder(ingredients);

                if (correct)
                {
                    Debug.Log($"✅ Ordine corretto per {client.name}");
                    // TODO: Suono di successo, animazione, svuotare vassoio
                }
                else
                {
                    Debug.Log($"❌ Ordine ERRATO per {client.name}");
                    // TODO: Feedback errore
                }
            }
        }
    }
}