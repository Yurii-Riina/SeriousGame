using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
            if (stack.placeableObject == null)
            {
                Debug.LogWarning($"[PlaceOnTray] Uno stack ha placeableObject nullo (stackPoint: {stack.stackPointOnTrayName})");
                continue;
            }

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

    public List<GameObject> GetObjectsOnTray(Transform tray)
    {
        List<GameObject> placedObjects = new List<GameObject>();

        if (tray == null) return placedObjects;

        AddChildrenRecursive(tray, placedObjects);

        return placedObjects;
    }

    private void AddChildrenRecursive(Transform parent, List<GameObject> list)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains("stackpoint")) continue;

            list.Add(child.gameObject);

            // Ricorsione: controlla anche i figli del figlio
            AddChildrenRecursive(child, list);
        }
    }

    public Ingredient IngredientFromGameObject(GameObject obj) //la usiamo perchè per controllare se l'ordine è giusto ci serve una lista di ingredienti non di gameobjects
    {
        string name = obj.name;

        if (name == "BottomCookedBread") return Ingredient.BottomBun;
        if (name == "TopCookedBread") return Ingredient.TopBun;
        if (name == "CookedMeat") return Ingredient.Hamburger;
        if (name == "CookedBacon") return Ingredient.Bacon;
        if (name == "Cheese") return Ingredient.Cheese;
        if (name == "MeltedCheese") return Ingredient.Cheese;
        if (name == "MeltedCheese_FInal(Clone)") return Ingredient.Cheese; //non riuscivamo a correggerlo via codice quindi abbiamo optato per questa soluzione
        if (name == "Onion") return Ingredient.Onion;
        if (name == "Lettuce") return Ingredient.Lettuce;
        if (name == "Pickle") return Ingredient.Pickles;
        if (name == "Tomato") return Ingredient.Tomato;
        if (name == "CocaCola") return Ingredient.CocaCola;
        if (name == "Fanta") return Ingredient.Fanta;
        if (name == "Water") return Ingredient.Water;
        if (name == "Donut") return Ingredient.Donut;
        if (name == "CookedFriesPack") return Ingredient.Fries;
        if (name == "CookedNuggetsPack 1") return Ingredient.Nuggets;

        // Se non riconosciuto
        throw new Exception($"Ingrediente non riconosciuto: {obj.name}");
    }

    public List<Ingredient> ConvertObjectsToIngredients(List<GameObject> objects)
    {
        List<Ingredient> ingredients = new List<Ingredient>();

        foreach (var obj in objects)
        {
            try
            {
                Ingredient ingr = IngredientFromGameObject(obj);
                ingredients.Add(ingr);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        return ingredients;
    }

}
