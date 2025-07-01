using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaceOnTray : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;
    public Transform currentTray { get; private set; }

    [System.Serializable]
    public class StackOnTray
    {
        public string stackPointOnTrayName;
        public GameObject placeableObject;
    }

    [SerializeField] private List<StackOnTray> stackOnTrayList;

    // Mappa tra nome stack point e oggetti associati
    private Dictionary<string, List<string>> stackOnTrayDict;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (hand == null)
            Debug.LogError("Hand transform is not assigned in the inspector.");

        if (pickUpAndPlaceScript == null)
            pickUpAndPlaceScript = GetComponent<PickUpAndPlace>();

        // Costruzione del dizionario
        stackOnTrayDict = new Dictionary<string, List<string>>();
        foreach (var stack in stackOnTrayList)
        {
            string objectName = stack.placeableObject.name;

            if (!stackOnTrayDict.ContainsKey(stack.stackPointOnTrayName))
                stackOnTrayDict[stack.stackPointOnTrayName] = new List<string>();

            stackOnTrayDict[stack.stackPointOnTrayName].Add(objectName);
        }
    }

    public void HandleFButtonClick()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPlaceOnTray();
        }
    }

    private void TryPlaceOnTray()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickUpAndPlaceScript.pickUpRange))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.name == "Tray" && hand.childCount > 0)
            {
                currentTray = hitObject.transform;

                GameObject heldObject = hand.GetChild(0).gameObject;
                string heldName = heldObject.name;

                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                Collider col = heldObject.GetComponent<Collider>();
                if (rb != null && col != null)
                {
                    pickUpAndPlaceScript.SetHeldObject(rb, col);
                }

                // Trova il nome del punto di stack corrispondente
                string matchingStackPoint = stackOnTrayDict
                    .FirstOrDefault(kvp => kvp.Value.Contains(heldName))
                    .Key;

                if (!string.IsNullOrEmpty(matchingStackPoint))
                {
                    // Cerca lo stack point come figlio del Tray effettivamente cliccato
                    Transform stackPointTransform = currentTray.Find(matchingStackPoint);

                    if (stackPointTransform != null)
                    {
                        Debug.Log($"[PlaceOnTray] Posizionamento '{heldName}' su '{matchingStackPoint}' del tray: {currentTray.name}");
                        pickUpAndPlaceScript.PlaceObjectAt(stackPointTransform);
                        heldObject.transform.SetParent(currentTray); 
                    }
                    else
                    {
                        Debug.LogWarning($"[PlaceOnTray] Stack point '{matchingStackPoint}' non trovato sotto il Tray {currentTray.name}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[PlaceOnTray] Nessun matching stack point per oggetto '{heldName}'.");
                }
            }
        }
    }
}
