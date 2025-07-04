using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FryerBasketMover : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform basket;
    [SerializeField] private Transform[] cookingSlots;

    [System.Serializable]
    public class FryablePrefab
    {
        public GrillableType type;
        public GameObject cookedPrefab;
    }

    [Header("Prefab cotti")]
    [SerializeField] private List<FryablePrefab> cookedPrefabs;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cookingSound;
    [SerializeField] private AudioClip finishedSound;

    [Header("Parametri")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float loweredY = -0.2f;
    [SerializeField] private float raisedY = 0f;
    public float cookingTime = 8f;

    private bool isLowered = false;
    private bool isCooking = false;
    public bool IsCooking => isCooking;
    private Coroutine cookingCoroutine;

    public void ToggleBasket()
    {
        if (isLowered)
        {
            if (isCooking)
            {
                Debug.Log("Non puoi alzare durante la cottura!");
                return;
            }
            StartCoroutine(MoveBasket(raisedY));
            isLowered = false;
        }
        else
        {
            StartCoroutine(MoveBasket(loweredY));
            isLowered = true;

            if (HasRawItems() && !isCooking)
            {
                cookingCoroutine = StartCoroutine(CookItems());
            }
        }
    }

    private IEnumerator MoveBasket(float targetY)
    {
        Vector3 currentPos = basket.localPosition;

        while (Mathf.Abs(currentPos.y - targetY) > 0.01f)
        {
            currentPos.y = Mathf.MoveTowards(currentPos.y, targetY, moveSpeed * Time.deltaTime);
            basket.localPosition = currentPos;
            yield return null;
        }

        currentPos.y = targetY;
        basket.localPosition = currentPos;
    }

    private bool HasRawItems()
    {
        foreach (Transform slot in cookingSlots)
        {
            if (slot.childCount > 0)
                return true;
        }
        return false;
    }

    private IEnumerator CookItems()
    {
        isCooking = true;

        if (audioSource && cookingSound)
            audioSource.PlayOneShot(cookingSound);

        Debug.Log("Inizio cottura...");

        yield return new WaitForSeconds(cookingTime);

        Debug.Log("Cottura terminata!");

        foreach (Transform slot in cookingSlots)
        {
            if (slot.childCount > 0)
            {
                Transform rawItem = slot.GetChild(0);
                var grillable = rawItem.GetComponent<Grillable>();
                if (grillable == null)
                {
                    Debug.LogWarning("Oggetto nello slot non ha Grillable.");
                    continue;
                }

                GameObject prefab = cookedPrefabs.Find(p => p.type == grillable.type)?.cookedPrefab;
                if (prefab == null)
                {
                    Debug.LogError("Prefab cotto mancante per " + grillable.type);
                    continue;
                }

                // Salva posizione, rotazione e scala
                Vector3 pos = rawItem.localPosition;
                Quaternion rot = rawItem.localRotation;
                Vector3 scale = rawItem.localScale;

                Destroy(rawItem.gameObject);

                // Istanzia prefab cotto
                GameObject cooked = Instantiate(prefab);
                cooked.name = prefab.name;
                cooked.transform.SetParent(slot);

                // FORZA posizione, rotazione, scala
                cooked.transform.localPosition = pos;
                cooked.transform.localRotation = rot;
                cooked.transform.localScale = scale;

                Debug.Log($"Prefab cotto {prefab.name} spawnato e allineato.");
            }
        }

        if (audioSource && finishedSound)
            audioSource.PlayOneShot(finishedSound);

        isCooking = false;
    }
}
