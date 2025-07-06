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

    private Dictionary<string, List<string>> stackOnTrayDict;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (hand == null)
            Debug.LogError("Hand transform is not assigned in the inspector.");

        if (pickUpAndPlaceScript == null)
            pickUpAndPlaceScript = GetComponent<PickUpAndPlace>();

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

                // Rimuove eventuali suffissi "(Clone)" e numeri finali
                if (heldName.Contains("("))
                    heldName = heldName.Substring(0, heldName.IndexOf("(")).Trim();
                Debug.Log($"[PlaceOnTray] Nome normalizzato oggetto in mano: {heldName}");


                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                Collider col = heldObject.GetComponent<Collider>();
                if (rb != null && col != null)
                {
                    pickUpAndPlaceScript.SetHeldObject(rb, col);
                }

                string matchingStackPoint = stackOnTrayDict
                    .FirstOrDefault(kvp => kvp.Value.Contains(heldName))
                    .Key;

                if (!string.IsNullOrEmpty(matchingStackPoint))
                {
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
            AddChildrenRecursive(child, list);
        }
    }

    public Ingredient? IngredientFromGameObject(GameObject obj)
    {
        string name = obj.name;

        // Ignora contenitori
        if (name.StartsWith("CartonHamburger") || name.StartsWith("CartonTop"))
            return null;

        // Ignora semini sul bun
        if (name.Contains("Cube"))
            return null;

        // Ignora elementi decorativi dell'acqua
        if (name.StartsWith("aqua_05_element"))
            return null;

        // Panino
        if (name == "BottomCookedBread") return Ingredient.BottomBun;
        if (name == "TopCookedBread") return Ingredient.TopBun;
        if (name == "CookedMeat") return Ingredient.Hamburger;
        if (name == "CookedBacon") return Ingredient.Bacon;
        if (name == "Cheese") return Ingredient.Cheese;
        if (name == "MeltedCheese") return Ingredient.Cheese;
        if (name == "MeltedCheese_FInal(Clone)") return Ingredient.Cheese;
        if (name == "Onion") return Ingredient.Onion;
        if (name == "Lettuce") return Ingredient.Lettuce;
        if (name == "Tomato") return Ingredient.Tomato;

        // Bevande
        if (name == "CocaCola") return Ingredient.CocaCola;
        if (name == "Fanta") return Ingredient.Fanta;
        if (name == "Water") return Ingredient.Water;

        // Dolci
        if (name == "Donut") return Ingredient.Donut;

        // Patatine e Nuggets
        if (name.StartsWith("CookedFriesPack") || name.StartsWith("CartonFries"))
            return Ingredient.Fries;
        if (name.StartsWith("CookedNuggetsPack") || name.StartsWith("CartonNuggets"))
            return Ingredient.Nuggets;

        // Decorazioni Donut
        if (name.StartsWith("Topping") || name.Contains("Sprinkle"))
            return null;

        Debug.LogWarning($"❗Ingrediente non riconosciuto: {obj.name}");
        return null;
    }

    public Ingredient? IngredientFromGameObjectSafe(GameObject obj)
    {
        try
        {
            return IngredientFromGameObject(obj);
        }
        catch
        {
            return null;
        }
    }

    public List<Ingredient> ConvertObjectsToIngredients(List<GameObject> objects)
    {
        var ingredients = new List<Ingredient>();

        foreach (var obj in objects)
        {
            Ingredient? ingr = IngredientFromGameObjectSafe(obj);

            if (ingr.HasValue)
            {
                // Se è un pack riconosciuto (patatine, nuggets) o un bun, NON processare i figli
                if (
                    obj.name.StartsWith("CookedFriesPack")
                    || obj.name.StartsWith("CookedNuggetsPack")
                    || obj.name == "TopCookedBread"
                    || obj.name == "BottomCookedBread"
                )
                {
                    ingredients.Add(ingr.Value);
                    continue; // SALTA i figli
                }

                // Aggiunge l'ingrediente riconosciuto
                ingredients.Add(ingr.Value);
            }

            // Se non era un contenitore, controlla ricorsivamente i figli
            ingredients.AddRange(GetIngredientsFromChildren(obj.transform));
        }

        return ingredients;
    }

    public List<Ingredient> GetIngredientsFromChildren(Transform parent)
    {
        var list = new List<Ingredient>();

        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains("stackpoint"))
                continue;

            if (child.name.ToLower().Contains("topping") || child.name.ToLower().Contains("sprinkle"))
                continue;

            Ingredient? ingr = IngredientFromGameObjectSafe(child.gameObject);
            if (ingr.HasValue)
            {
                list.Add(ingr.Value);

                // Se è un pack, NON scendere nei figli
                if (ingr.Value == Ingredient.Fries || ingr.Value == Ingredient.Nuggets)
                    continue;
            }

            // Ricorsione nei figli
            list.AddRange(GetIngredientsFromChildren(child));
        }

        return list;
    }

    /// <summary>
    /// Rimuove il riferimento al vassoio attualmente selezionato.
    /// </summary>
    public void ClearCurrentTray()
    {
        currentTray = null;
    }
}