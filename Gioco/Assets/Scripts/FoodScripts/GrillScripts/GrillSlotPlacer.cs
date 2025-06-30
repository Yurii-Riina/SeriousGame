using UnityEngine;

public class GrillSlotPlacer : MonoBehaviour
{
    [Header("Slot di appoggio")]
    [SerializeField] private Transform[] slots;

    private bool[] isSlotOccupied;

    private void Awake()
    {
        isSlotOccupied = new bool[slots.Length];
    }

    /// <summary>
    /// Prova a piazzare l'oggetto nello slot libero più vicino.
    /// </summary>
    public bool TryPlaceObject(GameObject obj)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!isSlotOccupied[i])
            {
                // Blocca posizione
                obj.transform.SetParent(slots[i]);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                // Disattiva fisica
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;

                isSlotOccupied[i] = true;

                Debug.Log($"Oggetto piazzato nello slot {i}");
                return true;
            }
        }

        Debug.LogWarning("Tutti gli slot occupati.");
        return false;
    }
}
