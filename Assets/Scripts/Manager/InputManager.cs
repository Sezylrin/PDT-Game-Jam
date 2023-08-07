using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerInput inputs;
    private Punch punch;
    [SerializeField]
    private PlayerController player;
    [SerializeField]
    private FirstPersonCamera Camera;
    [SerializeField]
    private Climbing climbing;
    [SerializeField]
    private WallRunning wallRun;
    [SerializeField]
    private PlayerAttack attack;
    [SerializeField]
    private Sliding slide;
    [SerializeField]
    private Swinging swing;
    private void Awake()
    {
        inputs = new PlayerInput();
        punch = new Punch();
    }
    private void OnEnable()
    {
        inputs.Player.Enable();
        inputs.Player.WASD.performed += player.SetDirection;
        inputs.Player.MouseX.performed += Camera.SetMouseX;
        inputs.Player.MouseY.performed += Camera.SetMouseY;
        inputs.Player.Jump.performed += player.TriggerJump;
        inputs.Player.Jump.canceled += player.CancelJump;
        inputs.Player.Forward.performed += climbing.IsForward;
        inputs.Player.Climbing.performed += climbing.IsSpace;
        inputs.Player.Jump.performed += climbing.WallJump;
        inputs.Player.WASD.performed += wallRun.CheckInputs;
        inputs.Player.Jump.performed += wallRun.JumpPressed;

        punch.Enable(); 
        punch.Gameplay.Attack.started += attack.OnAttack; 
        punch.Gameplay.Attack.canceled += attack.ReleaseAttack;
        punch.Gameplay.Attack.performed += attack.FullCharge;
        inputs.Player.Crouch.performed += player.CrouchPressed;
        inputs.Player.Crouch.canceled += player.CrouchCancelled;
        inputs.Player.Crouch.performed += slide.SetCrouch;
        inputs.Player.Crouch.canceled += slide.CancelCrouch;
        inputs.Player.WASD.performed += slide.SetDir;
        inputs.Player.Grapple.performed += swing.AttemptGrapple;
    }

    private void OnDisable()
    {
        inputs.Player.WASD.performed -= player.SetDirection;
        inputs.Player.MouseX.performed -= Camera.SetMouseX;
        inputs.Player.MouseY.performed -= Camera.SetMouseY;
        inputs.Player.Jump.performed -= player.TriggerJump;
        inputs.Player.Jump.canceled -= player.CancelJump;
        inputs.Player.Forward.performed -= climbing.IsForward;
        inputs.Player.Climbing.performed -= climbing.IsSpace;
        inputs.Player.Jump.performed -= climbing.WallJump;
        inputs.Player.WASD.performed -= wallRun.CheckInputs;
        inputs.Player.Jump.performed -= wallRun.JumpPressed;
        inputs.Player.Crouch.performed -= player.CrouchPressed;
        inputs.Player.Crouch.canceled -= player.CrouchCancelled;
        inputs.Player.Crouch.performed -= slide.SetCrouch;
        inputs.Player.Crouch.canceled -= slide.CancelCrouch;
        inputs.Player.WASD.performed -= slide.SetDir;
        inputs.Player.Grapple.performed -= swing.AttemptGrapple;
        inputs.Player.Disable();

        punch.Gameplay.Attack.started -= attack.OnAttack;
        punch.Gameplay.Attack.canceled -= attack.ReleaseAttack;
        punch.Gameplay.Attack.performed -= attack.FullCharge;
        punch.Enable();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
