using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpAndPlace : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    public float pickUpRange = 3f;
    [SerializeField] private Transform hand;

    private Rigidbody currentObjectRB;
    private Collider currentObjectCollider;
    private Quaternion originalRotation;

    // StackPoint attuale dove verrà posizionato il prossimo ingrediente/oggetto
    private Transform currentStackPoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange))
            {
                GameObject target = hit.collider.gameObject;

                // Se clicchi su un oggetto con StackPoint (es. pane), aggiorna lo stackPoint attivo
                Transform targetStack = target.transform.Find("StackPoint");
                if (targetStack != null)
                {
                    currentStackPoint = targetStack;
                    Debug.Log("StackPoint aggiornato su: " + target.name);
                }

                // Se hai in mano un ingrediente E hai un punto di appoggio, piazzalo
                if (currentObjectRB != null && currentStackPoint != null)
                {
                    PlaceObjectOnStack();
                    return;
                }

                // Se NON hai niente in mano e clicchi su un pickable, lo raccogli
                if (currentObjectRB == null && target.CompareTag("Pickable"))
                {
                    PickUpObject(hit);
                    return;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            // Se hai un oggetto in mano, lo lasci cadere
            if (currentObjectRB != null)
            {
                DropObject();
            }
        }
    }

    void PickUpObject(RaycastHit hitInfo)
    {
        currentObjectRB = hitInfo.rigidbody;
        currentObjectCollider = hitInfo.collider;

        originalRotation = currentObjectRB.transform.rotation;

        currentObjectRB.isKinematic = true;
        currentObjectCollider.enabled = false;

        currentObjectRB.transform.SetParent(hand);
        currentObjectRB.transform.localPosition = Vector3.zero;
        currentObjectRB.transform.localRotation = Quaternion.identity;

        Debug.Log("Raccolto: " + currentObjectRB.name);
    }

    void DropObject()
    {
        currentObjectRB.isKinematic = false;
        currentObjectRB.useGravity = true;
        currentObjectCollider.enabled = true;
        currentObjectRB.transform.SetParent(null);

        Debug.Log("Droppato: " + currentObjectRB.name);

        currentObjectRB = null;
        currentObjectCollider = null;
        currentStackPoint = null;

    }

    void PlaceObjectOnStack()
    {
        currentObjectRB.isKinematic = true;
        currentObjectCollider.enabled = true;

        currentObjectRB.transform.SetParent(null);
        currentObjectRB.transform.position = currentStackPoint.position;
        currentObjectRB.transform.rotation = originalRotation;

        Debug.Log("Posizionato su stack: " + currentStackPoint.name);

        // Aggiorna lo stack point con quello del nuovo ingrediente
        Transform nextStackPoint = currentObjectRB.transform.Find("StackPoint");
        if (nextStackPoint != null)
        {
            currentStackPoint = nextStackPoint;
        }
        else
        {
            currentStackPoint = null;
        }

        currentObjectRB = null;
        currentObjectCollider = null;
    }

    public void SetHeldObject(Rigidbody rb, Collider col)
    {
        currentObjectRB = rb;
        currentObjectCollider = col;
        originalRotation = rb.transform.rotation;
    }
}
