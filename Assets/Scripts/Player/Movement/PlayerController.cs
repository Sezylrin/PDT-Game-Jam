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
    WallRunning,
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
    private float playerSize;
    [field:SerializeField]
    public float gravity { get; private set; }
    [SerializeField]
    private float terminalVelocity;
    [HideInInspector]
    public float dragAmount { get; private set; }

    private Vector2 speed;

    [Header("Jump Setting")]
    [SerializeField]
    private float jumpHeight = 1;
    [SerializeField]
    private float maxHold = 1;


    [SerializeField][ReadOnly]
    private bool holding;
    [SerializeField] [ReadOnly]
    private float holdTimer;
    [SerializeField] [ReadOnly]
    private int timesJumped;
    [SerializeField] [ReadOnly]
    private bool hasDoubleJumped;

    [Header("Crouch Setting")]
    [SerializeField][ReadOnly]
    private bool isCrouch;
    [SerializeField][ReadOnly]
    private bool isCrouching;
    [SerializeField]
    private float crouchHeight;
    private Vector3 crouchPos;
    private Vector3 unCrouchPos;

    [field:SerializeField]
    public float crouchSpeed { get; private set; }
    [field:SerializeField]
    public float crouchTriggerSpeed { get; private set; }

    private RaycastHit slopeHit;

    [field: Header("Slope Setting")]
    [field:SerializeField][field:ReadOnly]
    public float slopeAngle { get; private set; }
    [field:SerializeField][field:ReadOnly]
    public bool isSlope { get; private set; }

    [Header("Player States")]
    [SerializeField] [ReadOnly]
    private bool isGrounded;

    [Header("Player Core")]
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private CapsuleCollider col;
    [SerializeField]
    private Transform camTr;

    [field:SerializeField][field:ReadOnly]
    public PlayerStates CurrentState { get; private set; }
    [SerializeField]
    private LayerMask notPlayer;

    [SerializeField]
    private Climbing climb;
    [SerializeField]
    private WallRunning wallRun;
    [SerializeField]
    private Sliding slide;

    [Header("Debug Values")]
    [SerializeField] [ReadOnly]
    private Vector2 inputDirection;
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
        speed = new Vector2(forwardSpeed, normalSpeed);
        unCrouchPos = camTr.localPosition;
        crouchPos = unCrouchPos + (Vector3.down * crouchHeight);
    }
    // Update is called once per frame
    void Update()
    {
        StateChecker();
        SetCrouching();
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
        if (isGrounded && (CurrentState == PlayerStates.Grounded || CurrentState == PlayerStates.Crouching))
        {
            Vector2 accel = inputDirection * acceleration * Time.fixedDeltaTime;
            accel *= dragAmount;
            Debug.DrawRay(transform.position, transform.forward * 10, Color.green);
            Vector3 horizontal = rb.velocity;
            horizontal.y = 0;
            Vector3 relative = transform.forward * accel.x + transform.right * accel.y;
            if (isSlope)
            {
                float relativeMag = relative.magnitude;
                relative = Vector3.ProjectOnPlane(relative, slopeHit.normal);
                relative.Normalize();
                relative *= relativeMag;
                if (accel != Vector2.zero)
                relative += Vector3.down;
            }
            if (accel.x > 0)
            {
                if ((horizontal + relative).magnitude <= speed.x)
                    rb.velocity += relative;
                else if (horizontal.magnitude < speed.x)
                {
                    Vector3 vert = rb.velocity;
                    vert.x = 0;
                    vert.z = 0;
                    rb.velocity = (horizontal + relative).normalized * speed.x + vert;
                }
                else if (horizontal.magnitude > speed.x)
                {
                    Vector3 vert = rb.velocity;
                    vert.x = 0;
                    vert.z = 0;
                    rb.velocity = (horizontal + relative).normalized * rb.velocity.magnitude + vert;
                }
            }
            else
            {
                if ((horizontal + relative).magnitude <= speed.y)
                    rb.velocity += relative;
                else if (horizontal.magnitude < speed.y)
                {
                    Vector3 vert = rb.velocity;
                    vert.x = 0;
                    vert.z = 0;
                    rb.velocity = (horizontal + relative).normalized * speed.y + vert;
                }
                else if (horizontal.magnitude > speed.y)
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
        Vector2 accel = inputDirection * acceleration * Time.fixedDeltaTime;
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
            rb.velocity += Vector3.up * gravity * Time.fixedDeltaTime;
            holdTimer += Time.fixedDeltaTime;
        }
    }

    public void TriggerJump(InputAction.CallbackContext context)
    {
        if (timesJumped < 1 && isGrounded)
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
        if (wallRun.CheckWall())
            return;
        if (climb.IsStartClimb())
            return;
        if (climb.GetClimbing())
            return;
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
        else if (wallRun.IsWallRunning())
        {
            CurrentState = PlayerStates.WallRunning;
        }
        else if (slide.GetSliding())
        {
            CurrentState = PlayerStates.Sliding;
        }
        else if (isCrouching)
        {
            CurrentState = PlayerStates.Crouching;
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
        if (Physics.Raycast(transform.position,Vector3.down, out slopeHit, playerSize, notPlayer))
        {
            if (!isGrounded)
            {
                climb.ResetWallClimb();
                /*Vector3 velocity = rb.velocity;
                velocity.y = 0;
                rb.velocity = velocity;*/
                if (CurrentState != PlayerStates.Sliding)
                    rb.drag = dragAmount;
                timesJumped = 0;
                hasDoubleJumped = false;
            }
            isGrounded = true;
            Vector3 forward = transform.forward;
            forward.y = 0;
            slopeAngle = 90 - Vector3.Angle(forward.normalized * -1f, slopeHit.normal);
            isSlope = Mathf.Abs(slopeAngle) > 1 ? true : false;
        }
        else
        {
            isGrounded = false;
            
            if (CurrentState == PlayerStates.InAir)
            {
                rb.drag = 0;
            }
            if (isCrouching && !isSlope)
            {
                CancelCrouch();
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
                rb.velocity += (Vector3.down * gravity * Time.fixedDeltaTime);
            }
            else if (dot < 0)
            {
                rb.velocity += (Vector3.down * gravity * Time.fixedDeltaTime);
            }

        }
    }
    

    private void DebugVelocity()
    {
        velocityVector.text = ("Velocity Vector " + rb.velocity.ToString());
        velocityVertical.text = ("Vertical Velocity " + rb.velocity.y.ToString("F2"));
        velocityHorizontal.text = ("Horizontal Velocity " + new Vector2(rb.velocity.x, rb.velocity.z).magnitude.ToString("F2"));
    }
    
    private void SetCrouching()
    {
        if (CurrentState == PlayerStates.Sliding)
            return; 
        
        if (isCrouch && HoriVelocity().magnitude < crouchTriggerSpeed && isGrounded && !isCrouching)
        {
            //shrink the hitbox
            //move the cam down
            //slow the movement speed
            col.height = col.height  - crouchHeight;
            col.center = new Vector3(0, (crouchHeight * 0.5f) * -1f, 0);
            camTr.localPosition = crouchPos;
            if (rb.velocity.magnitude > crouchSpeed)
            rb.velocity = rb.velocity.normalized * crouchSpeed;
            speed = new Vector2(crouchSpeed, crouchSpeed);
            isCrouching = true;
        }
    }
    public void CrouchPressed(InputAction.CallbackContext context)
    {
        isCrouch = true;
        /*if (HoriVelocity().magnitude < crouchSpeed && isGrounded)
        {
            //shrink the hitbox
            //move the cam down
            //slow the movement speed
            speed = new Vector2(crouchSpeed, crouchSpeed);
            isCrouching = true;
        }*/
    }

    public void CrouchCancelled(InputAction.CallbackContext context)
    {
        isCrouch = false;
        CancelCrouch();
    }

    private void CancelCrouch()
    {
        if (isCrouching)
        {
            //animate going up
            //set hitboxes going up
            //set cam back to default pos
            col.height = col.height + crouchHeight;
            col.center = new Vector3(0, 0, 0);
            camTr.localPosition = unCrouchPos;
        }
        isCrouching = false;
        speed = new Vector2(forwardSpeed, normalSpeed);
    }

    public void ToggleGravity(bool toggle)
    {
        isGravity = toggle;
    }

    public Vector3 HoriVelocity()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = 0;
        return velocity;
    }

    public float MaxSpeed()
    {
        return forwardSpeed;
    }

    public bool GetGrounded()
    {
        return isGrounded;
    }
    
    public RaycastHit GetSlopeHit()
    {
        return slopeHit;
    }
}
