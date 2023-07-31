using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public float sensX;
    public float sensY;

    public Transform orientation;

    private float xRotation;
    private float yRotation;

    private Vector2 clampAmount;

    private void Awake()
    {
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        clampAmount = new Vector2(-90f, 90f);
    }

    private void Update()
    {
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
    public void SetMouseX(InputAction.CallbackContext context)
    {
        yRotation += context.ReadValue<float>() * sensX * Time.deltaTime;
    }

    public void SetMouseY(InputAction.CallbackContext context)
    {
        xRotation -= context.ReadValue<float>() * sensY * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, clampAmount.x, clampAmount.y);
    }

    public void SetClamp(float a, float b)
    {
        clampAmount.x = a;
        clampAmount.y = b;
    }
}
