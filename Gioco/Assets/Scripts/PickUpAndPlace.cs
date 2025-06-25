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
    private Vector3 originalScale;

    private Transform currentStackPoint;

    void Update()
    {
        // TASTO SINISTRO DEL MOUSE - raccoglie o piazza
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange))
            {
                GameObject target = hit.collider.gameObject;

                // Aggiorna lo StackPoint se clicchi su un oggetto che lo contiene
                Transform targetStack = target.transform.Find("StackPoint");
                if (targetStack != null)
                {
                    currentStackPoint = targetStack;
                    Debug.Log("StackPoint aggiornato su: " + target.name);
                }

                // Se hai in mano un oggetto e hai uno stack valido, prova a piazzarlo
                if (currentObjectRB != null && currentStackPoint != null)
                {
                    StackPointRestrictions restriction = currentStackPoint.GetComponent<StackPointRestrictions>();

                    if (restriction != null && !restriction.IsTagAllowed(currentObjectRB.tag))
                    {
                        Debug.Log($"Non puoi posizionare '{currentObjectRB.name}' con tag '{currentObjectRB.tag}' su '{currentStackPoint.name}'");
                        return;
                    }

                    PlaceObjectOnStack();
                    return;
                }

                // Se non hai nulla in mano e clicchi su un oggetto pickable, raccoglilo
                if (currentObjectRB == null && target.CompareTag("Pickable"))
                {
                    PickUpObject(hit);
                    return;
                }
            }
        }

        // TASTO DESTRO DEL MOUSE - droppa oggetto
        if (Input.GetMouseButtonDown(1))
        {
            if (currentObjectRB != null)
            {
                Debug.Log("Droppo l'oggetto: " + currentObjectRB.name);
                DropObject();
            }
        }
    }

    void PickUpObject(RaycastHit hitInfo)
    {
        currentObjectRB = hitInfo.rigidbody;
        currentObjectCollider = hitInfo.collider;

        originalRotation = currentObjectRB.transform.rotation;
        originalScale = currentObjectRB.transform.localScale;

        currentObjectRB.isKinematic = true;
        currentObjectCollider.enabled = false;

        currentObjectRB.transform.SetParent(hand);
        currentObjectRB.transform.localPosition = Vector3.zero;
        currentObjectRB.transform.localRotation = Quaternion.identity;
        currentObjectRB.transform.localScale = originalScale;

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
        currentObjectRB.transform.rotation = currentStackPoint.rotation;

        Debug.Log("Posizionato su stack: " + currentStackPoint.name);

        // Aggiorna il nuovo punto di stack se presente
        Transform nextStackPoint = currentObjectRB.transform.Find("StackPoint");
        currentStackPoint = nextStackPoint != null ? nextStackPoint : null;

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
