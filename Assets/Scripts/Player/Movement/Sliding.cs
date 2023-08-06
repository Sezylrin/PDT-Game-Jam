using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KevinCastejon.MoreAttributes;

public class Sliding : MonoBehaviour
{
    [SerializeField][ReadOnly]
    private bool isCrouch;
    [SerializeField][ReadOnly]
    private bool isSliding;
    [SerializeField]
    [ReadOnly]
    private Vector2 dir;

    [Header("Slide Stats")]
    [SerializeField]
    private float slideAcceleration;
    [SerializeField]
    private float slideSpeedBoost;
    [SerializeField]
    private float sidewardsSpeed;
    private Vector3 slideCamPos;
    private Vector3 normalCamPos;

    [Header("Core")]
    [SerializeField]
    private PlayerController player;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private CapsuleCollider col;
    [SerializeField]
    private Transform camTr;
    [SerializeField]
    private float camShiftDist;
    // Start is called before the first frame update
    void Start()
    {
        normalCamPos = camTr.localPosition;
        slideCamPos = normalCamPos + (Vector3.down * camShiftDist);
    }

    // Update is called once per frame
    void Update()
    {
        StartSliding();
        StopSliding();
    }

    private void FixedUpdate()
    {
        SlideVelocity();
    }
    private void SlideVelocity()
    {
        if (!isSliding)
            return;
        if (dir.y != 0)
        {
            rb.velocity = (rb.velocity + transform.right * dir.y * sidewardsSpeed * Time.fixedDeltaTime).normalized * rb.velocity.magnitude;
        }
        if (player.isSlope)
        {
            rb.drag = 0;
            RaycastHit hit = player.GetSlopeHit();
            Vector3 temp = Vector3.ProjectOnPlane(transform.forward, hit.normal);
            transform.LookAt(transform.position + temp, Vector3.up);

            float ratio = Mathf.Abs(player.slopeAngle) / 90;
            if (player.slopeAngle < 0)
            {
                rb.velocity += rb.velocity.normalized * (ratio * slideAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                rb.velocity -= rb.velocity.normalized * (ratio * slideAcceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            rb.drag = 1;
        }
    }
    private void StartSliding()
    {
        if (isSliding || player.CurrentState == PlayerStates.Crouching)
            return;
        if (isCrouch && dir.x > 0 && player.GetGrounded() && rb.velocity.magnitude > player.crouchTriggerSpeed)
        {
            isSliding = true;
            col.direction = 2;
            col.center = new Vector3(0, -0.5f, 0.5f);
            camTr.localPosition = slideCamPos;
            rb.drag = 1;
            rb.velocity = rb.velocity.normalized * (rb.velocity.magnitude + slideSpeedBoost);
            rb.constraints = (int)rb.constraints + RigidbodyConstraints.FreezeRotationY;
        }
    }

    private void StopSliding()
    {
        if (!isSliding)
            return;
        if (!player.GetGrounded() || !isCrouch || rb.velocity.magnitude < player.crouchSpeed)
        {
            Vector3 forward = transform.forward;
            forward.y = 0;
            isSliding = false;
            col.direction = 1;
            col.center = Vector3.zero;
            camTr.localPosition = normalCamPos;
            rb.drag = player.dragAmount;
            rb.constraints = (int)rb.constraints - RigidbodyConstraints.FreezeRotationY;
        }
    }

    public void SetCrouch(InputAction.CallbackContext context)
    {
        isCrouch = true;
    }

    public void CancelCrouch(InputAction.CallbackContext context)
    {
        isCrouch = false;
    }

    public void SetDir(InputAction.CallbackContext context)
    {
        dir = context.ReadValue<Vector2>();
    }

    public bool GetSliding()
    {
        return isSliding;
    }
}
