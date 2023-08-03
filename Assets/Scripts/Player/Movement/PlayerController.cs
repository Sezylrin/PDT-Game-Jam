using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KevinCastejon.MoreAttributes;
using TMPro;

public enum PlayerStates
{
    Grounded,
    Crouching,
    Sliding,
    Climbing,
    InAir
}
public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float forwardSpeed;
    [SerializeField]
    private float normalSpeed;
    [SerializeField]
    private float crouchFactor;
    [SerializeField]
    private float playerSize;
    [SerializeField]
    private float gravity;
    [SerializeField]
    private float terminalVelocity;
    private float dragAmount;

    [Header("Jump Setting")]
    [SerializeField]
    private float jumpHeight = 1;
    [SerializeField]
    private float maxHold = 1;

    [SerializeField] [ReadOnly]
    private bool holding;
    [SerializeField] [ReadOnly]
    private float holdTimer;
    [SerializeField] [ReadOnly]
    private int timesJumped;
    [SerializeField] [ReadOnly]
    private bool hasDoubleJumped;

    [Header("Wall Climb")]
    [SerializeField]
    [ReadOnly]
    private bool isClimbing;
    [SerializeField]
    private float climbSpeed;

    [Header("Player States")]
    [SerializeField] [ReadOnly]
    private bool isGrounded;

    [Header("Player Core")]
    [SerializeField]
    private Rigidbody rb;
    [field:SerializeField][field:ReadOnly]
    public PlayerStates CurrentState { get; private set; }
    [SerializeField]
    private LayerMask notPlayer;

    [SerializeField]
    private Climbing climb;
    [SerializeField]
    private WallRunning wallRun;

    [Header("Debug Values")]
    [SerializeField] [ReadOnly]
    private Vector2 inputDirection;
    [SerializeField] [ReadOnly]
    private Vector2 speed;
    public TMP_Text velocityVector;
    public TMP_Text velocityVertical;
    public TMP_Text velocityHorizontal;

    private bool isGravity = true;
    private void Awake()
    {

    }
    void Start()
    {
        dragAmount = rb.drag;
        ResetSpeed();
    }
    // Update is called once per frame
    void Update()
    {
        StateChecker();
    }

    private void FixedUpdate()
    {
        CheckOnGround();
        Gravity();
        Move();
        AerialMove();
        Jump();
        DebugVelocity();
    }

    public void SetDirection(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>().normalized;
    }

    private void Move()
    {
        if (isGrounded && CurrentState == PlayerStates.Grounded)
        {
            Vector2 accel = inputDirection * acceleration * 0.02f;
            accel *= dragAmount;
            Debug.DrawRay(transform.position, transform.forward * 10, Color.green);
            Vector3 horizontal = rb.velocity;
            horizontal.y = 0;
            Vector3 relative = transform.forward * accel.x + transform.right * accel.y;
            if (accel.x > 0)
            {
                if ((horizontal + relative).magnitude <= forwardSpeed)
                    rb.velocity += relative;
                else if (horizontal.magnitude < forwardSpeed)
                {
                    Vector3 vert = rb.velocity;
                    vert.x = 0;
                    vert.z = 0;
                    rb.velocity = (horizontal + relative).normalized * forwardSpeed + vert;
                }
                else if (horizontal.magnitude > forwardSpeed)
                {
                    Vector3 vert = rb.velocity;
                    vert.x = 0;
                    vert.z = 0;
                    rb.velocity = (horizontal + relative).normalized * rb.velocity.magnitude + vert;
                }
            }
            else
            {
                if ((horizontal + relative).magnitude <= normalSpeed)
                    rb.velocity += relative;
                else if (horizontal.magnitude < normalSpeed)
                {
                    Vector3 vert = rb.velocity;
                    vert.x = 0;
                    vert.z = 0;
                    rb.velocity = (horizontal + relative).normalized * normalSpeed + vert;
                }
                else if (horizontal.magnitude > normalSpeed)
                {
                    Vector3 vert = rb.velocity;
                    vert.x = 0;
                    vert.z = 0;
                    rb.velocity = (horizontal + relative).normalized * rb.velocity.magnitude + vert;
                }
            }
        }
    }

    private void AerialMove()
    {
        if (isGrounded || CurrentState != PlayerStates.InAir)
            return;
        Vector2 accel = inputDirection * acceleration * 0.02f;
        Vector3 horizontal = rb.velocity;
        horizontal.y = 0;
        Vector3 vert = rb.velocity;
        vert.x = 0;
        vert.z = 0;
        Vector3 relative = transform.forward * accel.x + transform.right * accel.y;
        if ((horizontal + relative).magnitude > forwardSpeed)
        {
            if ((horizontal + relative).magnitude > horizontal.magnitude)
                rb.velocity = (horizontal + relative).normalized * horizontal.magnitude + vert;
            else
                rb.velocity = horizontal + relative + vert;
        }
        else
        {
            rb.velocity = horizontal + relative + vert;
        }
    }
    [ContextMenu("suddenboost")]
    private void AddForce()
    {
        if (rb.velocity.magnitude < 20)
        {
            rb.AddForce(transform.forward * 300, ForceMode.Acceleration);
        }
    }

    private void Jump()
    {
        if (holding && holdTimer < maxHold && rb.velocity.y >= 0)
        {
            rb.velocity += Vector3.up * gravity * 0.02f;
            holdTimer += Time.fixedDeltaTime;
        }
    }

    public void TriggerJump(InputAction.CallbackContext context)
    {
        if (timesJumped < 1 && CurrentState != PlayerStates.InAir)
        {
            // Putting it here prevents holding after a release on the second jump.
            rb.drag = 0;
            holding = true;
            Vector3 velocity = rb.velocity;
            velocity.y = 0;
            rb.velocity = velocity;
            rb.velocity += Vector3.up * Mathf.Sqrt(2f * gravity * jumpHeight);
        }
        timesJumped++;
        if (!hasDoubleJumped && !isGrounded && CurrentState == PlayerStates.InAir)
        {
            hasDoubleJumped = true;
            holding = true;
            Vector3 velocity = rb.velocity;
            velocity.y = 0;
            rb.velocity = velocity;
            rb.velocity += Vector3.up * Mathf.Sqrt(2f * gravity * jumpHeight);
        }
    }

    public void CancelJump(InputAction.CallbackContext context)
    {
        holding = false;
        holdTimer = 0;
    }

    private void StateChecker()
    {
        if (climb.GetClimbing())
        {
            CurrentState = PlayerStates.Climbing;
        }
        else if (isGrounded)
        {
            CurrentState = PlayerStates.Grounded;
        }
        else if (!isGrounded)
        {
            CurrentState = PlayerStates.InAir;
        }
    }
    private void CheckOnGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position,Vector3.down, out hit, playerSize, notPlayer))
        {
            isGrounded = true;
            climb.ResetWallClimb();
            if (CurrentState != PlayerStates.Grounded)
            {
                Vector3 velocity = rb.velocity;
                velocity.y = 0;
                rb.velocity = velocity;
                rb.drag = dragAmount;
                timesJumped = 0;
                hasDoubleJumped = false;
            }
        }
        else
        {
            isGrounded = false;
            
            if (CurrentState != PlayerStates.InAir)
            {
                rb.drag = 0;
            }
        }
    }
    private void Gravity()
    {
        if (!isGravity)
            return;
        if (!isGrounded)
        {
            Vector3 proj = Vector3.Project(rb.velocity, Vector3.down);
            float dot = Vector3.Dot(proj, Vector3.down);
            if (dot >= 0 && proj.magnitude <= terminalVelocity)
            {
                rb.velocity += (Vector3.down * gravity * 0.02f);
            }
            else if (dot < 0)
            {
                rb.velocity += (Vector3.down * gravity * 0.02f);
            }

        }
    }
    

    private void DebugVelocity()
    {
        velocityVector.text = ("Velocity Vector " + rb.velocity.ToString());
        velocityVertical.text = ("Vertical Velocity " + rb.velocity.y.ToString("F2"));
        velocityHorizontal.text = ("Horizontal Velocity " + new Vector2(rb.velocity.x, rb.velocity.z).magnitude.ToString("F2"));
    }

    private void ResetSpeed()
    {
        speed.x = forwardSpeed;
        speed.y = normalSpeed;
    }

    public void SetState(PlayerStates stateToSet)
    {
        CurrentState = stateToSet;
    }

    public void ToggleGravity(bool toggle)
    {
        isGravity = toggle;
    }
}
