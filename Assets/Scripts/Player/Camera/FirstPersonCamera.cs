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

    [SerializeField]
    private float xRotation;
    [SerializeField]
    private float yRotation;

    [SerializeField]
    private PlayerController player;

    private Vector2 clampAmount;
    [SerializeField]
    private float wallRunForward;
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
        switch ((int)player.CurrentState)
        {
            case (int)PlayerStates.Climbing:
                ClimbLook();
                break;
            case (int)PlayerStates.WallRunning:
                WallRunLook();
                break;
            case (int)PlayerStates.Sliding:
                SlideLook();
                break;
            default:
                NormalLook();
                break;
        }
    }
    //((n mod 360) + 360) mod 360
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

    public void NormalLook()
    {
        
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
    public void ClimbLook()
    {
        
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    public void SlideLook()
    {
        
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    public void WallRunLook()
    {
        
        wallRunForward = orientation.eulerAngles.y;
        if (yRotation + 360 > wallRunForward - 90f && yRotation + 360 < wallRunForward + 90f)
        {
            yRotation += 360;
        }
        else if (yRotation - 360 > wallRunForward - 90f && yRotation - 360 < wallRunForward + 90f)
        {
            yRotation -= 360;
        }
        else if (yRotation > wallRunForward + 180f || yRotation < wallRunForward - 180f)
        {
            SetYClamp();
        }
        yRotation = Mathf.Clamp(yRotation, wallRunForward - 90f, wallRunForward + 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    public void SetYClamp()
    {
        yRotation = ((yRotation % 360) + 360) % 360;
    }

}
