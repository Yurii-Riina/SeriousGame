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
        public GameObject prefab;
    }

    [SerializeField] private List<PackagePrefabPair> packagePrefabs;

    private Dictionary<string, GameObject> packagePrefabDict;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (hand == null)
            Debug.LogError("Hand transform is not assigned in the inspector.");

        if (pickUpAndPlaceScript == null)
            pickUpAndPlaceScript = GetComponent<PickUpAndPlace>();

        // Costruisce il dizionario
        packagePrefabDict = new Dictionary<string, GameObject>();
        foreach (var pair in packagePrefabs)
        {
            if (!packagePrefabDict.ContainsKey(pair.packageName))
            {
                packagePrefabDict.Add(pair.packageName, pair.prefab);
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPickUpFromPackage();
        }
    }

    private void TryPickUpFromPackage()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if(Physics.Raycast(ray, out RaycastHit hit, pickUpAndPlaceScript.pickUpRange))
        {
            GameObject hitObject = hit.collider.gameObject;

            if(hitObject.CompareTag("Package"))
            {
                string packageName = hitObject.name;

                if (packagePrefabDict.TryGetValue(packageName, out GameObject prefabToSpawn))
                {
                    if (hand.transform.childCount != 0)
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

                        // Aggiorna il PickUpAndPlace per segnalare che l’oggetto è in mano
                        pickUpAndPlaceScript.SetHeldObject(rb, col);
                    }
                    else
                    {
                        Debug.LogWarning("Prefab senza Rigidbody o Collider: " + item.name);
                    }
                    item.transform.SetParent(hand);
                    Debug.Log("Preso oggetto dal pacchetto: " + prefabToSpawn+ " | Active: " + prefabToSpawn.activeSelf);
                }
                else
                {
                    Debug.LogWarning("Nessun prefab associato a: " + packageName);
                }
            }
        }
    }
}
