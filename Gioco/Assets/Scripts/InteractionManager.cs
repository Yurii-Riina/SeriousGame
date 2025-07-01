using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;
    [SerializeField] private PickFromPackage pickFromPackageScript;
    [SerializeField] private PlaceOnTray placeOnTrayScript;
    [SerializeField] private GrillPlacingRaycast grillPlacingRaycast;
    [SerializeField] private FoodContainerInteraction foodContainerInteraction;

    void Awake()
    {
        if (pickUpAndPlaceScript == null)
            pickUpAndPlaceScript = GetComponent<PickUpAndPlace>();

        if (pickFromPackageScript == null)
            pickFromPackageScript = GetComponent<PickFromPackage>();

        if (placeOnTrayScript == null)
            placeOnTrayScript = GetComponent<PlaceOnTray>();

        if (grillPlacingRaycast == null)
            grillPlacingRaycast = GetComponent<GrillPlacingRaycast>();

        if (foodContainerInteraction == null)
            foodContainerInteraction = GetComponent<FoodContainerInteraction>();
    }

    void Update()
    {
        pickUpAndPlaceScript.PickPlaceDrop();
        pickFromPackageScript.HandleEButtonClick();
        placeOnTrayScript.HandleFButtonClick();
        grillPlacingRaycast.HandleCButtonClick();
        foodContainerInteraction.HandleFButtonClick();
    }
}
