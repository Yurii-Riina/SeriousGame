using UnityEngine;

public class OriginalScale : MonoBehaviour
{
    [HideInInspector] public Vector3 originalScale;

    void Awake()
    {
        // Salva la scala originale dell'oggetto
        originalScale = transform.localScale;
    }
}
