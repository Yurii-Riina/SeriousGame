using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerWithMouse : MonoBehaviour
{
    [SerializeField] private Transform playerCamera; 
    [SerializeField][Range(0.0f, 0.5f)] private float mouseSmoothTime = 0.03f;
    [SerializeField] private bool cursorLock = true;
    [SerializeField] private float mouseSensivity = 3.5f;
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private  float fallingSpeed = -5;
    private Transform myTransform;
    float cameraCap;
    Vector2 currentMouseDelta;
    Vector2 currentMouseDeltaVelocity;
    CharacterController controller;
 
    void Start()
    {
        controller = GetComponent<CharacterController>();
        myTransform = GetComponent<Transform>();

        if(cursorLock){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        UpdateMouse();
        UpdateMove();
    }

    void UpdateMouse(){
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime); //cambia gradualmente un vettore verso un altro desiderato in un certo periodo di tempo e con una certa e-velocità

        cameraCap -= currentMouseDelta.y * mouseSensivity; //cameracap la usiamo per settare i limiti di movimento della camera (angoli)
        cameraCap = Mathf.Clamp(cameraCap, -90f, 90f); //clamp va a serrare la cameracap (il range di movimento della camera) tra due angoli massimi

        playerCamera.localEulerAngles = Vector3.right * cameraCap; //così gestiamo la rotazione intorno ad x (lo facciamo separatamente perchè ha dei limiti)

        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensivity); //così gestiamo la rotazione intorno ad y
    }

    void UpdateMove(){
            
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if(movement.magnitude > 1f){// lo facciamo solo quando è maggiore di 1 perchè sul controller potrei muovere la levetta di "un po'" quindi il vettore non assumerebbe solo 1 o 0 o -1 come valori (infatti è un float)

            movement.Normalize(); //serve per normalizzare il vettore movement e portarlo ad 1
        
        }

        if(!controller.isGrounded){//se il personaggio è a terra è true

            movement.y = fallingSpeed;

        } 

        movement = myTransform.TransformDirection(movement); //trasforma la direzione da locale a gloabale

        controller.Move(movement * Time.deltaTime * speed);

    

    }
}
