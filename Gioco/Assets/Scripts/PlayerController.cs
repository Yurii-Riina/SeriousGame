using System.Collections;
using UnityEngine;

public class PlayerControllerWithMouse : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField][Range(0.0f, 0.5f)] private float mouseSmoothTime = 0.03f;
    [SerializeField] private bool cursorLock = true;
    [SerializeField] private float mouseSensivity = 3.5f;
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float fallingSpeed = -5f;

    private Transform myTransform;
    private float cameraCap;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myTransform = GetComponent<Transform>();

        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused)
        {
            // Blocca il movimento e la camera in pausa
            return;
        }

        UpdateMouse();
        UpdateMove();
    }

    void UpdateMouse()
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraCap -= currentMouseDelta.y * mouseSensivity;
        cameraCap = Mathf.Clamp(cameraCap, -90f, 90f);

        playerCamera.localEulerAngles = Vector3.right * cameraCap;
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensivity);
    }

    void UpdateMove()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (movement.magnitude > 1f)
            movement.Normalize();

        if (!controller.isGrounded)
            movement.y = fallingSpeed;

        movement = myTransform.TransformDirection(movement);
        controller.Move(movement * Time.deltaTime * speed);
    }
}
