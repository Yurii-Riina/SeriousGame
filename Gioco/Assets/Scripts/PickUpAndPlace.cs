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
            string objName = currentObjectRB.name.ToLower();

            if (objName.Contains("cookedfriespack") || objName.Contains("cookednuggetspack"))
            {
                Debug.Log($"🟢 Oggetto {currentObjectRB.name} distrutto perché non droppabile.");
                Destroy(currentObjectRB.gameObject);
                ClearState();
                return;
            }

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
        if (currentObjectRB == null) return;

        currentObjectRB.isKinematic = false;
        currentObjectRB.useGravity = true;
        currentObjectCollider.enabled = true;

        // Controlla se è un pacchetto di patatine o nuggets
        string name = currentObjectRB.name.ToLower();
        if (name.Contains("fries") || name.Contains("nugget"))
        {
            // Blocca le rotazioni per sempre
            currentObjectRB.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            // Sblocca tutto per gli altri oggetti
            currentObjectRB.constraints = RigidbodyConstraints.None;
        }

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

        if (!currentObjectRB.name.Contains("Tray"))
        {
            Transform parent = currentStackPoint.parent;
            currentObjectRB.transform.SetParent(parent);
        }

        // StackPoint aggiornato
        currentStackPoint = currentObjectRB.transform.Find("StackPoint");

        // Debug: Inizio blocco formaggio
        Debug.Log("INIZIO VERIFICA FUSIONE FORMAGGIO");

        // Verifica se l'oggetto posizionato è un formaggio crudo
        MeltableCheese meltable = currentObjectRB.GetComponent<MeltableCheese>();
        if (meltable != null)
        {
            Debug.Log("Oggetto posizionato è un formaggio crudo.");

            Vector3 checkPosition = currentObjectRB.transform.position + Vector3.down * 0.01f;
            float radius = 0.1f;

            Collider[] hits = Physics.OverlapSphere(checkPosition, radius);

            if (hits.Length == 0)
            {
                Debug.Log("Nessun oggetto trovato sotto il formaggio.");
            }
            else
            {
                foreach (Collider col in hits)
                {
                    Debug.Log("Trovato collider sotto il formaggio: " + col.name);

                    if (col.GetComponent<CookedMarker>() != null)
                    {
                        Debug.Log("TROVATO HAMBURGER COTTO sotto. Procedo con sostituzione.");

                        // Salviamo posizione e rotazione
                        Vector3 pos = currentObjectRB.transform.position;
                        Quaternion rot = currentObjectRB.transform.rotation;

                        // Distruggiamo il formaggio crudo
                        Destroy(currentObjectRB.gameObject);

                        // Istanziamo il formaggio fuso
                        GameObject melted = Instantiate(meltable.meltedPrefab, pos, rot);

                        // Se vuoi piazzarlo nello StackPoint
                        melted.transform.position = currentStackPoint.position;

                        // Imposta la scala originale se serve
                        OriginalScale os = melted.GetComponent<OriginalScale>();
                        if (os != null)
                            melted.transform.localScale = os.originalScale;

                        // Mettiamo nello stesso parent dell'hamburger
                        melted.transform.SetParent(col.transform.parent);

                        Debug.Log("Formaggio CRUDO distrutto e sostituito con prefab fuso: " + meltable.meltedPrefab.name);

                        break;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Oggetto NON è formaggio crudo, nessuna sostituzione.");
        }

        ClearState();
    }


    public void PlaceObjectAt(Transform stackPoint)
    {
        if (currentObjectRB == null) return;

        currentObjectRB.isKinematic = true;
        currentObjectRB.transform.SetParent(null);
        currentObjectRB.transform.position = stackPoint.position + new Vector3(0f, 0.02f, 0f);

        Quaternion rotationOffset = Quaternion.identity;
        string objName = currentObjectRB.name.ToLower();

        Debug.Log("Posiziono oggetto con nome: " + objName);

        if (objName.Contains("cartonhamburger"))
        {
            // Cartone panino ruotato di 180° sull'asse Y
            rotationOffset = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (objName.Contains("cartonfries"))
        {
            // Carton Fries disteso come prima
            rotationOffset = Quaternion.Euler(-90f, 180f, 0f);
        }
        else if (objName.Contains("cookedfriespack"))
        {
            // Cooked Fries Pack disteso in modo diverso (prova così)
            rotationOffset = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (objName.Contains("cookednuggetspack"))
        {
            // Cooked Nuggets Pack disteso in modo chiaro
            rotationOffset = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (objName.Contains("cocacola") || objName.Contains("fanta") || objName.Contains("water"))
        {
            // Bibite in piedi
            rotationOffset = Quaternion.Euler(-90f, 0f, 0f);
        }
        else if (objName.Contains("donut"))
        {
            // Donut distesa con glassa verso l'alto
            rotationOffset = Quaternion.Euler(-90f, 0f, 0f);
        }
        else
        {
            // Default: ruotato di 180° Y
            rotationOffset = Quaternion.Euler(0f, 180f, 0f);
        }

        currentObjectRB.transform.rotation = rotationOffset;

        var originalScaleComponent = currentObjectRB.GetComponent<OriginalScale>();
        currentObjectRB.transform.localScale = originalScaleComponent ? originalScaleComponent.originalScale : originalScale;

        currentObjectCollider.enabled = true;

        Debug.Log("Oggetto posizionato su stack: " + stackPoint.name);

        currentStackPoint = currentObjectRB.transform.Find("StackPoint");

        Debug.Log("INIZIO VERIFICA FUSIONE FORMAGGIO");

        MeltableCheese meltable = currentObjectRB.GetComponent<MeltableCheese>();
        if (meltable != null)
        {
            Debug.Log("Oggetto posizionato è un formaggio crudo.");

            if (currentStackPoint != null)
            {
                Debug.Log("StackPoint presente: " + currentStackPoint.name);

                Transform parent = currentStackPoint.parent;
                if (parent != null)
                {
                    Debug.Log("Parent dello StackPoint: " + parent.name);

                    foreach (Transform child in parent)
                    {
                        Debug.Log("Analizzo child: " + child.name);

                        if (child.GetComponent<CookedMarker>() != null)
                        {
                            Debug.Log("TROVATO HAMBURGER COTTO nello stack. Procedo con sostituzione formaggio.");

                            Vector3 pos = currentObjectRB.transform.position;
                            Quaternion rot = currentObjectRB.transform.rotation;

                            Destroy(currentObjectRB.gameObject);

                            GameObject melted = Instantiate(meltable.meltedPrefab, pos, rot);
                            melted.name = meltable.meltedPrefab.name;

                            melted.transform.position = currentStackPoint.position;

                            OriginalScale os = melted.GetComponent<OriginalScale>();
                            if (os != null)
                                melted.transform.localScale = os.originalScale;

                            melted.transform.SetParent(parent);

                            Debug.Log("Formaggio CRUDO distrutto e sostituito con prefab fuso: " + meltable.meltedPrefab.name);

                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("Oggetto NON è formaggio crudo, nessuna sostituzione.");
        }

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

        // Solo se NON è il vassoio, disabilita il collider
        if (!currentObjectRB.name.Contains("Tray"))
        {
            currentObjectCollider.enabled = false;
        }

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
