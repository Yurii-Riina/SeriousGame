using UnityEngine;

public class GrillPlacingTest : MonoBehaviour
{
    [SerializeField] private GrillSlotPlacer slotPlacer;
    [SerializeField] private PickUpAndPlace pickUpAndPlace;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameObject held = pickUpAndPlace.GetHeldObject();
            if (held != null)
            {
                if (slotPlacer.TryPlaceObject(held))
                {
                    pickUpAndPlace.ClearHeldObject();
                }
            }
        }
    }
}
