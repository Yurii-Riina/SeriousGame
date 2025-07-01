using UnityEngine;
using System.Collections.Generic;

public class CookedFoodContainerManager : MonoBehaviour
{
    [Header("Slot di appoggio nel contenitore")]
    [SerializeField] private Transform[] containerSlots;

    private bool[] isSlotOccupied;
    private List<GameObject> storedItems = new List<GameObject>();

    private void Awake()
    {
        isSlotOccupied = new bool[containerSlots.Length];
    }

    /// <summary>
    /// Piazza un alimento cotto nello slot libero.
    /// </summary>
    public bool TryPlaceCookedFood(GameObject obj)
    {
        for (int i = 0; i < containerSlots.Length; i++)
        {
            if (!isSlotOccupied[i])
            {
                obj.transform.SetParent(containerSlots[i]);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = new Vector3(2.25f, 2.25f, 2.25f);

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                isSlotOccupied[i] = true;
                storedItems.Add(obj);

                Debug.Log($"Cibo cotto spostato nello slot contenitore {i}");
                return true;
            }
        }

        Debug.LogWarning("Tutti gli slot del contenitore sono occupati!");
        return false;
    }

    /// <summary>
    /// Rimuove un alimento cotto casuale e lo restituisce.
    /// </summary>
    public GameObject TakeRandomFood()
    {
        if (storedItems.Count == 0)
        {
            Debug.Log("Nessun cibo cotto nel contenitore.");
            return null;
        }

        int index = Random.Range(0, storedItems.Count);
        GameObject selected = storedItems[index];
        storedItems.RemoveAt(index);

        // Libera lo slot
        for (int i = 0; i < containerSlots.Length; i++)
        {
            if (selected.transform.parent == containerSlots[i])
            {
                isSlotOccupied[i] = false;
                break;
            }
        }

        selected.transform.SetParent(null);

        Debug.Log($"Prelevato cibo cotto {selected.name} dal contenitore.");
        return selected;
    }
}
