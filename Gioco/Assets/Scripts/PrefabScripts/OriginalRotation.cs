using UnityEngine;

public class OriginalRotation : MonoBehaviour
{
    [Tooltip("Rotazione da applicare quando l'oggetto è in mano.")]
    public Vector3 originalEulerRotation;

    public Quaternion originalRotation => Quaternion.Euler(originalEulerRotation);

    private void Awake()
    {
        SetRotationByName();
    }

    private void SetRotationByName()
    {
        if (gameObject.name.Contains("CartonHamburger"))
        {
            originalEulerRotation = new Vector3(90f, 0f, 0f);
        }
        else if (gameObject.name.Contains("Tray"))
        {
            originalEulerRotation = new Vector3(-90f, 180f, 0f);
        }
        else if (gameObject.name.Contains("BottomUncookedBread"))
        {
            originalEulerRotation = new Vector3(-90, 0, 0);
        }
        else if (gameObject.name.Contains("TopUncookedBread"))
        {
            originalEulerRotation = new Vector3(90, 0, 0);
        }
    }
}

