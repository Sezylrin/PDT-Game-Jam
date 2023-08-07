using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KevinCastejon.MoreAttributes;

public class Climbing : MonoBehaviour
{
    [SerializeField][ReadOnly]
    private bool isW;
    [SerializeField]
    [ReadOnly]
    private bool isSpace;
    [SerializeField]
    [ReadOnly]
    private bool isClimbing;
    [SerializeField]
    [ReadOnly]
    private bool canClimb = true;
    [SerializeField]
    private PlayerController player;

    [Header("Core")]
    [SerializeField]
    private Transform camTr;
    [SerializeField]
    private Rigidbody rb;

    [Header("Climb Values")]
    [SerializeField]
    private float climbRate;
    [SerializeField]
    private float climbDuration;
    private float currentClimb;
    [SerializeField]
    private float scanDistance;
    [SerializeField]
    private LayerMask terrain;
    [SerializeField][Range(0,90)]
    private int lookAngles;

    [Header("Wall Jump")]
    [SerializeField]
    private float horiForce;
    [SerializeField]
    private float vertForce;
    private RaycastHit hit;


    void Start()
    {
    }

    void Update()
    {
        CheckClimbing();
    }

    private void FixedUpdate()
    {
        ClimbWall();
    }
    public void CheckClimbing()
    {
        if (isW)
            Physics.Raycast(transform.position, transform.forward, out hit, scanDistance, terrain);
        if (isW && isSpace && canClimb)
        {
            if (hit.collider != null && currentClimb < climbDuration)
            {
                Vector3 normDir = hit.normal;
                normDir.y = 0;
                normDir.Normalize();
                float angle = Vector3.Angle(transform.forward, normDir * -1f);
                if (angle < lookAngles)
                    isClimbing = true;
            }
        }
        if (isClimbing)
        {
            if ((!isW && !isSpace) || currentClimb > climbDuration || hit.collider == null)
            {
                StopClimb();
            }
            currentClimb += Time.deltaTime;
        }
    }

    private void StopClimb()
    {
        isClimbing = false;
        canClimb = false;
        currentClimb = 0;
        player.ToggleGravity(true);
    }

    public void ClimbWall()
    {
        if (!isClimbing)
            return;
        Vector3 forward = hit.normal;
        forward.y = 0;
        transform.forward = forward * -1f;
        player.ToggleGravity(false);
        rb.velocity = Vector3.up * climbRate + transform.forward * 1f;
    }

    public void WallJump(InputAction.CallbackContext context)
    {
        if (!isClimbing)
            return;
        float dot = Vector3.Dot(transform.forward, camTr.forward);
        if (dot >= 0)
            return;
        StopClimb();
        Vector3 dir = camTr.forward;
        dir.y = 0;
        dir.Normalize();
        dir *= horiForce;
        dir += Vector3.up * vertForce;
        rb.velocity = dir;
    }
    public void ResetWallClimb()
    {
        canClimb = true;
    }
    public bool IsStartClimb()
    {
        return (isW && hit.collider != null);
    }
    public bool GetClimbing()
    {
        return isClimbing;
    }
    public void IsForward(InputAction.CallbackContext context)
    {
        isW = context.ReadValue<float>().Equals(1f);
    }

    public void IsSpace(InputAction.CallbackContext context)
    {
        isSpace = context.ReadValue<float>().Equals(1f);
    }
}
