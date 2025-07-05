using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DeliverOrder : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;
    [SerializeField] private PlaceOnTray placeOnTrayScript;
    [SerializeField] private XManager xManager;

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

        if (xManager == null)
            xManager = FindFirstObjectByType<XManager>();
    }

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
        {
            Debug.Log("Non stai tenendo nulla.");
            return;
        }

        Transform held = hand.GetChild(0);
        if (!held.name.Contains("Tray"))
        {
            Debug.Log("L'oggetto in mano non è un vassoio.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, pickUpAndPlaceScript.pickUpRange))
        {
            Debug.Log("Nessun cliente di fronte.");
            return;
        }

        ClientAI client = hit.collider.GetComponent<ClientAI>();
        if (client == null)
        {
            Debug.Log("Non stai guardando un cliente.");
            return;
        }

        if (!client.IsOrdering())
        {
            Debug.Log("Il cliente non sta ordinando.");
            return;
        }

        // Ingredienti trovati nel vassoio
        List<Ingredient> trayIngredients = placeOnTrayScript.GetIngredientsFromChildren(held);
        Debug.Log($"🧾 Ingredienti trovati nel vassoio:\n- {string.Join("\n- ", trayIngredients)}");


        // Ingredienti richiesti dal cliente
        List<Ingredient> requiredIngredients = client.GetOrderIngredients();
        Debug.Log($"📋 Ingredienti richiesti dal cliente:\n- {string.Join("\n- ", requiredIngredients)}");
        List<Ingredient> required = client.GetOrderIngredients();
        Debug.Log($"📝 Ingredienti richiesti:\n- {string.Join("\n- ", required)}");


        // Controlla se tutti i richiesti sono presenti (indipendentemente dall'ordine)
        bool correct = requiredIngredients.All(req => trayIngredients.Contains(req)) &&
                       trayIngredients.Count == requiredIngredients.Count;

        if (correct)
        {
            Debug.Log($"✅ Ordine corretto per {client.name}");
            
            client.CompleteOrder(); // 🚀 Fai andare via il cliente

            Destroy(held.gameObject);
            placeOnTrayScript.ClearCurrentTray();
        }
        else
        {
            client.GoAngry(); // 😡 Fai arrabbiare il client
            xManager?.TurnNextXRed(); // 🔴 Segna un errore
            
            Destroy(held.gameObject);
            placeOnTrayScript.ClearCurrentTray();
            Debug.Log($"❌ Ordine ERRATO per {client.name}");
        }
    }
}