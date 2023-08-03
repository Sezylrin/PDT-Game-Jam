using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KevinCastejon.MoreAttributes;

public class WallRunning : MonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private bool isAorD;
    [SerializeField]
    [ReadOnly]
    private bool isLeft;
    [SerializeField]
    [ReadOnly]
    private bool isWallRunning;
    [SerializeField]
    [ReadOnly]
    private bool isWall;
    [Header("Core")]
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private PlayerController player;

    [SerializeField]
    private Transform camTr;

    [SerializeField]
    private FirstPersonCamera fpsCam;

    [Header("Wall Run Values")]
    [SerializeField]
    private float scanDist;
    [SerializeField]
    private float minVelocity;
    [SerializeField]
    private float minLookAngle;
    [SerializeField]
    private float vertVel;
    [SerializeField]
    private Vector2 lookAngles;

    [Header("Wall Jump Values")]
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private float jumpStrength;

    [SerializeField]
    private LayerMask terrain;


    private RaycastHit hit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        CheckForWall();
        WallRun();
    }

    private void CheckForWall()
    {
        Vector3 velocity = player.HoriVelocity();
        float dot = Vector3.Dot(transform.forward, velocity);
        if (!isAorD || dot <= 0)
        {
            StopWallRun();
            return;
        }
        Vector3 dir = (isLeft ? transform.right * -1f : transform.right);
        if (Physics.Raycast(transform.position, dir, out hit, scanDist, terrain))
        {
            float angle = Vector3.Angle(transform.forward, NormalDir() * -1);
            if (angle > lookAngles.y && angle < lookAngles.x)
                isWall = true;
        }
        else
        {
            StopWallRun();
        }
    }

    private void StopWallRun()
    {
        isWall = false;
        isWallRunning = false;
        player.ToggleGravity(true);
    }

    public void JumpPressed(InputAction.CallbackContext context)
    {
        if (isWallRunning)
        {
            float dot = Vector3.Dot(NormalDir(), camTr.forward);
            if (dot > 0)
                WallJump();
        }
        if (isWall)
        {
            Vector3 velocity = player.HoriVelocity();
            if (velocity.magnitude < minVelocity)
                return;
            isWallRunning = true;
            player.ToggleGravity(false);
        }
    }

    private void WallJump()
    {        
        Vector3 velocity = camTr.forward * player.HoriVelocity().magnitude + NormalDir() * jumpStrength + Vector3.up * jumpHeight;
        rb.velocity = velocity;
        StopWallRun();
    }
    private void WallRun()
    {
        if (!isWallRunning)
            return;
        Vector3 wallNormal = NormalDir();
        transform.right = isLeft ? wallNormal : wallNormal * -1;
        Vector3 velocity =transform.forward * player.HoriVelocity().magnitude;
        float lookAngle = camTr.eulerAngles.x;
        if (lookAngle > 180)
            lookAngle -= 360;
        if (Mathf.Abs(lookAngle) > minLookAngle)
        {
            Vector3 temp = Vector3.up * (lookAngle > 0 ? (vertVel * -1f) : vertVel);
            velocity += temp;
        }
        rb.velocity = velocity;
    }

    private Vector3 NormalDir()
    {
        Vector3 temp = hit.normal;
        temp.y = 0;
        temp.Normalize();
        return temp;
    }

    public void CheckInputs(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<Vector2>().y;
        isAorD = !input.Equals(0);
        if (isAorD)
            isLeft = input < 0;
    }

    public bool CheckWall()
    {
        return isWall;
    }

    public bool IsWallRunning()
    {
        return isWallRunning;
    }
}
