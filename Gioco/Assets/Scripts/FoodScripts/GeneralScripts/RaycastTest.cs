using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 10f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log("HIT: " + hit.collider.name);
            }
            else
            {
                Debug.Log("NO HIT");
            }
        }
    }
}
