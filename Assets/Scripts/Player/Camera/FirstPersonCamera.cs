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
        NormalLook();
        ClimbLook();
        WallRunLook();
    }
    //((n mod 360) + 360) mod 360
    public void SetMouseX(InputAction.CallbackContext context)
    {
        yRotation += context.ReadValue<float>() * sensX * Time.deltaTime;
        if (player.CurrentState == PlayerStates.WallRunning)
        {
            if (yRotation > wallRunForward + 180f || yRotation < wallRunForward - 180f)
            {
                SetYClamp();
            }
            yRotation = Mathf.Clamp(yRotation, wallRunForward - 90f, wallRunForward + 90f);
        }
        
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
        if (player.CurrentState == PlayerStates.Climbing || player.CurrentState == PlayerStates.WallRunning)
            return;
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
    public void ClimbLook()
    {
        if (player.CurrentState != PlayerStates.Climbing)
            return;
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    public void WallRunLook()
    {
        if (player.CurrentState != PlayerStates.WallRunning)
            return;
        wallRunForward = orientation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    public void SetYClamp()
    {
        yRotation = ((yRotation % 360) + 360) % 360;
    }

}
