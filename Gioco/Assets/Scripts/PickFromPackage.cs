using System.Collections.Generic;
using UnityEngine;

public class PickFromPackage : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;

    [System.Serializable]
    public class PackagePrefabPair
    {
        public string packageName;
        public GameObject EClickPrefab;   // default con singolo clic E
        public GameObject DoubleEClickPrefab;  // usato con doppio clic E per BreadPack
    }

    [SerializeField] private List<PackagePrefabPair> packagePrefabs;

    private Dictionary<string, PackagePrefabPair> packagePrefabDict;

    private float lastPressTime = 0f;
    private float doubleClickThreshold = 0.3f;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (hand == null)
            Debug.LogError("Hand transform is not assigned in the inspector.");

        if (pickUpAndPlaceScript == null)
            pickUpAndPlaceScript = GetComponent<PickUpAndPlace>();

        packagePrefabDict = new Dictionary<string, PackagePrefabPair>();
        foreach (var pair in packagePrefabs)
        {
            if (!packagePrefabDict.ContainsKey(pair.packageName))
                packagePrefabDict.Add(pair.packageName, pair);
        }
    }

    public void HandleEButtonClick()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float timeSinceLastPress = Time.time - lastPressTime;
            bool isDoubleClick = timeSinceLastPress <= doubleClickThreshold;

            TryPickFromPackage(isDoubleClick);
            lastPressTime = Time.time;
        }
    }

    private void TryPickFromPackage(bool isDoubleClick)
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

                    if (packageName == "BreadPack")
                    {
                        prefabToSpawn = isDoubleClick ? prefabPair.DoubleEClickPrefab : prefabPair.EClickPrefab;
                    }
                    else
                    {
                        prefabToSpawn = prefabPair.EClickPrefab;
                    }

                    if (prefabToSpawn == null)
                    {
                        Debug.LogWarning($"Nessun prefab assegnato per {packageName} (isDoubleClick: {isDoubleClick})");
                        return;
                    }

                    if (hand.childCount > 0)
                    {
                        Transform child = hand.GetChild(0);
                        if (child.CompareTag("Pickable"))
                        {
                            Destroy(child.gameObject);
                        }
                        else
                        {
                            Debug.LogWarning("Oggetto nella mano non è 'Pickable': " + child.name);
                        }
                    }

                    GameObject item = Instantiate(prefabToSpawn, hand.position, hand.rotation);

                    item.name = prefabToSpawn.name; //così evito che l'oggetto che mi spawna in mano abbia il suffisso "(Clone)" perchè avrebbe dato problemi con altri script

                    // Reset scala originale se presente componente OriginalScale
                    OriginalScale os = item.GetComponent<OriginalScale>();
                    if (os != null)
                    {
                        item.transform.localScale = os.originalScale;
                    }

                    Rigidbody rb = item.GetComponent<Rigidbody>();
                    Collider col = item.GetComponent<Collider>();

                    if (rb != null && col != null)
                    {
                        rb.isKinematic = true;
                        col.enabled = false;

                        pickUpAndPlaceScript.SetHeldObject(rb, col);
                        item.transform.SetParent(hand);

                        Debug.Log($"Preso oggetto da {packageName} {(isDoubleClick ? "[Doppio clic]" : "[Singolo clic]")}: {item.name}");
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
