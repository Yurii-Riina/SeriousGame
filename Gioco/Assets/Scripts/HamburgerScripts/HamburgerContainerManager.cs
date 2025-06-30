using UnityEngine;
using System.Collections.Generic;

public class HamburgerContainerFixedManager : MonoBehaviour
{
    [Header("Slot di appoggio nel contenitore")]
    [SerializeField] private Transform[] containerSlots;

    private bool[] isSlotOccupied;
    private List<GameObject> storedHamburgers = new List<GameObject>();

    private void Awake()
    {
        isSlotOccupied = new bool[containerSlots.Length];
    }

    /// <summary>
    /// Piazza un hamburger cotto nello slot libero.
    /// </summary>
    public bool TryPlaceCookedHamburger(GameObject obj)
    {
        for (int i = 0; i < containerSlots.Length; i++)
        {
            if (!isSlotOccupied[i])
            {
                obj.transform.SetParent(containerSlots[i]);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f); // imposta dimensione corretta

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                isSlotOccupied[i] = true;
                storedHamburgers.Add(obj);

                Debug.Log($"Hamburger spostato nello slot contenitore {i}");
                return true;
            }
        }

        Debug.LogWarning("Tutti gli slot del contenitore sono occupati!");
        return false;
    }

    /// <summary>
    /// Rimuove un hamburger casuale dal contenitore e lo restituisce.
    /// </summary>
    public GameObject TakeRandomHamburger()
    {
        if (storedHamburgers.Count == 0)
        {
            Debug.Log("Nessun hamburger nel contenitore.");
            return null;
        }

        int index = Random.Range(0, storedHamburgers.Count);
        GameObject selected = storedHamburgers[index];

        storedHamburgers.RemoveAt(index);

        // Trova a quale slot apparteneva e libera lo slot
        for (int i = 0; i < containerSlots.Length; i++)
        {
            if (selected.transform.parent == containerSlots[i])
            {
                isSlotOccupied[i] = false;
                break;
            }
        }

        selected.transform.SetParent(null);

        Debug.Log($"Prelevato hamburger {selected.name} dal contenitore.");
        return selected;
    }
}
