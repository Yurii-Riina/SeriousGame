using System.Collections.Generic;
using UnityEngine;

public class PickUpFromPackage : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;

    // Definisci una lista serializzabile di pacchi e prefab associati
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

        if (Physics.Raycast(ray, out RaycastHit hit, pickUpAndPlaceScript.pickUpRange))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Package"))
            {
                string packageName = hitObject.name;

                if (packagePrefabDict.TryGetValue(packageName, out GameObject prefabToSpawn))
                {
                    GameObject item = Instantiate(prefabToSpawn, hand.position, hand.rotation);
                    item.transform.SetParent(hand);
                }
                else
                {
                    Debug.LogWarning("Nessun prefab associato a: " + packageName);
                }
            }
        }
    }
}
