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
        // Mouse sinistro/destro
        pickUpAndPlaceScript.PickPlaceDrop();

        // E per prendere dagli imballaggi
        pickFromPackageScript.HandleEButtonClick();

        // F per mettere sul tray
        placeOnTrayScript.HandleFButtonClick();

        // C per mettere sulla griglia
        grillPlacingRaycast.HandleCButtonClick();

        // F per prelevare dai contenitori SOLO se la mano è vuota
        if (pickUpAndPlaceScript.GetHeldObject() == null)
        {
            foodContainerInteraction.HandleFButtonClick();
        }
    }
}
