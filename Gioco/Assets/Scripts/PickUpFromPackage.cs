using System.Collections.Generic;
using UnityEngine;

public class PickUpFromPackage : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;

    [System.Serializable]
    public class PackagePrefabPair
    {
        public string packageName;
        public GameObject leftClickPrefab;   // per tasto sinistro
        public GameObject rightClickPrefab;  // per tasto destro (opzionale)
    }

    [SerializeField] private List<PackagePrefabPair> packagePrefabs;

    private Dictionary<string, PackagePrefabPair> packagePrefabDict;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (hand == null)
            Debug.LogError("Hand transform is not assigned in the inspector.");

        if (pickUpAndPlaceScript == null)
            pickUpAndPlaceScript = GetComponent<PickUpAndPlace>();

        // Costruisce il dizionario
        packagePrefabDict = new Dictionary<string, PackagePrefabPair>();
        foreach (var pair in packagePrefabs)
        {
            if (!packagePrefabDict.ContainsKey(pair.packageName))
            {
                packagePrefabDict.Add(pair.packageName, pair);
            }
        }
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPickUpFromPackage(0); // tasto sinistro
        }
        else if (Input.GetMouseButtonDown(1))
        {
            TryPickUpFromPackage(1); // tasto destro
        }
    }

    private void TryPickUpFromPackage(int mouseButton)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickUpAndPlaceScript.pickUpRange))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Package"))
            {
                string packageName = hitObject.name;

                if (packagePrefabDict.TryGetValue(packageName, out PackagePrefabPair prefabPair))
                {
                    GameObject prefabToSpawn = null;

                    // Logica speciale per BreadPack
                    if (packageName == "BreadPack")
                    {
                        prefabToSpawn = mouseButton == 0 ? prefabPair.leftClickPrefab : prefabPair.rightClickPrefab;
                    }
                    else
                    {
                        prefabToSpawn = prefabPair.leftClickPrefab;
                    }

                    if (prefabToSpawn == null)
                    {
                        Debug.LogWarning($"Nessun prefab assegnato per il pulsante {mouseButton} su {packageName}");
                        return;
                    }

                    // Rimuovi oggetto precedente dalla mano
                    if (hand.childCount > 0)
                    {
                        Destroy(hand.GetChild(0).gameObject);
                    }

                    GameObject item = Instantiate(prefabToSpawn, hand.position, hand.rotation);
                    Rigidbody rb = item.GetComponent<Rigidbody>();
                    Collider col = item.GetComponent<Collider>();

                    if (rb != null && col != null)
                    {
                        rb.isKinematic = true;
                        col.enabled = false;

                        pickUpAndPlaceScript.SetHeldObject(rb, col);
                        item.transform.SetParent(hand);

                        Debug.Log($"Preso oggetto da {packageName} con pulsante {(mouseButton == 0 ? "sinistro" : "destro")}: {prefabToSpawn.name}");
                    }
                    else
                    {
                        Debug.LogWarning("Prefab mancante di Rigidbody o Collider: " + item.name);
                    }
                }
                else
                {
                    Debug.LogWarning("Nessun prefab registrato per: " + packageName);
                }
            }
        }
    }
}
