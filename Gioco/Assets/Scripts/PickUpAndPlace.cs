using UnityEngine;
//using static System.IO.Enumeration.FileSystemEnumerable<TResult>;

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

    public void PlaceObjectOnStack()
    {
        currentObjectRB.isKinematic = true;
        currentObjectCollider.enabled = true;
        currentObjectRB.transform.SetParent(null);
        currentObjectRB.transform.position = currentStackPoint.position;

        // Inizia con la rotazione base dello stack
        currentObjectRB.transform.rotation = currentStackPoint.rotation;

        // Aggiunge la rotazione personalizzata
        var rotComponent = currentObjectRB.GetComponent<OriginalRotation>();
        if (rotComponent != null && currentObjectRB.name.Contains("Tray")) //evitiamo la rotazione per il tray
        {
            currentObjectRB.transform.localRotation = Quaternion.Euler(rotComponent.originalEulerRotation);
        }
        else if (rotComponent != null) 
        {
            currentObjectRB.transform.rotation *= rotComponent.originalRotation;
        }

        var originalScaleComponent = currentObjectRB.GetComponent<OriginalScale>();
        currentObjectRB.transform.localScale = originalScaleComponent ? originalScaleComponent.originalScale : originalScale;

        Debug.Log("Posizionato su stack: " + currentStackPoint.name);

        // StackPoint aggiornato
        currentStackPoint = currentObjectRB.transform.Find("StackPoint");

        ClearState();
    }


    public void PlaceObjectAt(Transform stackPoint)
    {
        if (currentObjectRB == null) return;

        currentObjectRB.isKinematic = true;
        currentObjectRB.transform.SetParent(null);
        currentObjectRB.transform.position = stackPoint.position;
        currentObjectRB.transform.rotation = stackPoint.rotation;

        // Applica rotazione personalizzata se esiste
        var rotComponent = currentObjectRB.GetComponent<OriginalRotation>();
        if (rotComponent != null)
        {
            currentObjectRB.transform.localRotation *= Quaternion.Euler(rotComponent.originalEulerRotation);
        }

        var originalScaleComponent = currentObjectRB.GetComponent<OriginalScale>();
        currentObjectRB.transform.localScale = originalScaleComponent ? originalScaleComponent.originalScale : originalScale;

        currentObjectCollider.enabled = true;

        Debug.Log("Oggetto posizionato su stack: " + stackPoint.name);

        currentStackPoint = currentObjectRB.transform.Find("StackPoint");

        ClearState();
    }

    public void SetHeldObject(Rigidbody rb, Collider col)
    {
        currentObjectRB = rb;
        currentObjectCollider = col;

        if (currentObjectRB != null)
        {
            currentObjectRB.isKinematic = true;
        }

        rb.transform.SetParent(hand);
        rb.transform.localPosition = Vector3.zero;
        rb.transform.localRotation = Quaternion.identity;
    }

    private void AttachToHand()
    {
        currentObjectRB.isKinematic = true;
        currentObjectCollider.enabled = false;

        currentObjectRB.transform.SetParent(hand);
        currentObjectRB.transform.localPosition = Vector3.zero;

        // Applica la rotazione definita nello script OriginalRotation
        var rotComponent = currentObjectRB.GetComponent<OriginalRotation>();
        if (rotComponent != null)
        {
            currentObjectRB.transform.localRotation = Quaternion.Euler(rotComponent.originalEulerRotation);
        }
        else
        {
            currentObjectRB.transform.localRotation = Quaternion.identity;
        }

        currentObjectRB.transform.localScale = originalScale;
    }

    private void ClearState()
    {
        currentObjectRB = null;
        currentObjectCollider = null;
        if (currentStackPoint == null || !currentStackPoint.name.Contains("CartonHamburger")) // evitiamo che lo stack point venga eliminato quando costruiamo il panino sennò gli ingredienti si posizionerebbero uno dentro l'altro 
        {
            currentStackPoint = null;
        }

    }

    public GameObject GetHeldObject()
    {
        return currentObjectRB != null ? currentObjectRB.gameObject : null;
    }

    public void ClearHeldObject()
    {
        currentObjectRB = null;
        currentObjectCollider = null;
    }
}
