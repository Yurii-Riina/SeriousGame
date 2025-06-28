using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;
    [SerializeField] private PickFromPackage pickFromPackageScript;
    [SerializeField] private PlaceOnTray placeOnTrayScript;

    void Awake()
    {
        if (pickUpAndPlaceScript == null)
        {
            pickUpAndPlaceScript = GetComponent<PickUpAndPlace>();
            if (pickUpAndPlaceScript == null)
            {
                Debug.LogError("PickUpAndPlace script is not assigned or found on this GameObject.");
            }
        }

        if (pickFromPackageScript == null)
        {
            pickFromPackageScript = GetComponent<PickFromPackage>();
            if (pickFromPackageScript == null)
            {
                Debug.LogError("PickUpFromPackage script is not assigned or found on this GameObject.");
            }
        }

        if (placeOnTrayScript == null)
        {
            placeOnTrayScript = GetComponent<PlaceOnTray>();
            if (placeOnTrayScript == null)
            {
                Debug.LogError("PlaceOnTray script is not assigned or found on this GameObject.");
            }
        }
    }

    void Update()
    {
        pickUpAndPlaceScript.PickPlaceDrop();
        pickFromPackageScript.HandleEButtonClick();
        placeOnTrayScript.HandleFButtonClick();
    }
}
