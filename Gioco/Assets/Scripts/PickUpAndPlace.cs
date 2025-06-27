using UnityEngine;

public class PickUpAndPlace : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    public float pickUpRange = 3f;
    [SerializeField] private Transform hand;

    private Rigidbody currentObjectRB;
    private Collider currentObjectCollider;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Transform currentStackPoint;

    public void PickPlaceDrop()
    {
        HandleLeftClick();
        HandleRightClick();
    }

    private void HandleLeftClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, pickUpRange)) return;

        GameObject target = hit.collider.gameObject;

        // StackPoint detection
        Transform targetStack = target.transform.Find("StackPoint");
        if (targetStack != null)
        {
            currentStackPoint = targetStack;
            Debug.Log("StackPoint aggiornato su: " + target.name);
        }

        // Place object
        if (currentObjectRB != null && currentStackPoint != null)
        {
            PlaceObjectOnStack();
            return;
        }

        // Pick up object
        if (currentObjectRB == null && target.CompareTag("Pickable"))
        {
            PickUpObject(hit);
        }
    }

    private void HandleRightClick()
    {
        if (Input.GetMouseButtonDown(1) && currentObjectRB != null)
        {
            Debug.Log("Droppo l'oggetto: " + currentObjectRB.name);
            DropObject();
        }
    }

    private void PickUpObject(RaycastHit hitInfo)
    {
        if (hitInfo.rigidbody == null) return;

        currentObjectRB = hitInfo.rigidbody;
        currentObjectCollider = hitInfo.collider;
        originalRotation = currentObjectRB.transform.rotation;

        var originalScaleComponent = currentObjectRB.GetComponent<OriginalScale>();
        originalScale = originalScaleComponent ? originalScaleComponent.originalScale : currentObjectRB.transform.localScale;

        AttachToHand();

        Debug.Log("Raccolto: " + currentObjectRB.name);
    }

    private void DropObject()
    {
        currentObjectRB.isKinematic = false;
        currentObjectRB.useGravity = true;
        currentObjectCollider.enabled = true;
        currentObjectRB.transform.SetParent(null);

        Debug.Log("Droppato: " + currentObjectRB.name);

        ClearState();
    }

    private void PlaceObjectOnStack()
    {
        currentObjectRB.isKinematic = true;
        currentObjectCollider.enabled = true;
        currentObjectRB.transform.SetParent(null);
        currentObjectRB.transform.position = currentStackPoint.position;
        currentObjectRB.transform.rotation = currentStackPoint.rotation;

        var originalScaleComponent = currentObjectRB.GetComponent<OriginalScale>();
        currentObjectRB.transform.localScale = originalScaleComponent ? originalScaleComponent.originalScale : originalScale;

        Debug.Log("Posizionato su stack: " + currentStackPoint.name);

        // Stack sul nuovo oggetto (se esiste)
        currentStackPoint = currentObjectRB.transform.Find("StackPoint");

        ClearState();
    }

    //serve per permettere ad altri script di comunicare che hanno fatto raccogliere un oggetto al player
    public void SetHeldObject(Rigidbody rb, Collider col)
    {
        currentObjectRB = rb;
        currentObjectCollider = col;
        originalRotation = rb.transform.rotation;

        var originalScaleComponent = rb.GetComponent<OriginalScale>();
        originalScale = originalScaleComponent ? originalScaleComponent.originalScale : rb.transform.localScale;

        AttachToHand();
    }

    private void AttachToHand()
    {
        currentObjectRB.isKinematic = true;
        currentObjectCollider.enabled = false;

        currentObjectRB.transform.SetParent(hand);
        currentObjectRB.transform.localPosition = Vector3.zero;
        currentObjectRB.transform.localRotation = Quaternion.identity;
        currentObjectRB.transform.localScale = originalScale;
    }

    private void ClearState()
    {
        currentObjectRB = null;
        currentObjectCollider = null;
        currentStackPoint = null;
    }
}
