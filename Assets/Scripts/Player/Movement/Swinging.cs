using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KevinCastejon.MoreAttributes;
using UnityEngine.InputSystem.Interactions;
using System;

public class Swinging : MonoBehaviour
{
    [SerializeField][ReadOnly]
    private bool isSwinging;
    [SerializeField][ReadOnly]
    private bool isHold;
    [SerializeField][ReadOnly]
    private bool attemptSwing = false;

    [Header("Line Variables")]
    [SerializeField]
    private LineRenderer lr;
    [SerializeField]
    private Transform gunTip, camTr;
    [SerializeField]
    private LayerMask grappleMask;

    [SerializeField]
    private float spring;
    [SerializeField]
    private float damper;
    [SerializeField]
    private float massScale;
    [SerializeField]
    [ReadOnly]
    private Vector3 currentGrapplePoint;

    [Header("Swing Variables")]
    [SerializeField]
    private float GrappleDist;
    [SerializeField]
    private float pullSpeed;

    [SerializeField][ReadOnly]
    private Vector3 swingPoint;
    private SpringJoint joint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pullLine();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        if (!joint)
            return;

        currentGrapplePoint = Vector3.Lerp(currentGrapplePoint, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePoint);
    }
    
    private void StartSwinging()
    {
        RaycastHit hit;
        if (Physics.Raycast(camTr.position,camTr.forward, out hit, GrappleDist, grappleMask))
        {
            isSwinging = true;

            swingPoint = hit.point;
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint + 2;
            joint.minDistance = distanceFromPoint * 0.5f;

            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;

            lr.positionCount = 2;
            currentGrapplePoint = gunTip.position;
        }
    }

    private void StopSwinging()
    {
        isSwinging = false;
        lr.positionCount = 0;
        Destroy(joint);
    }

    private void pullLine()
    {
        if (!joint)
            return;
        joint.minDistance -= pullSpeed * Time.deltaTime;
        joint.maxDistance -= pullSpeed * Time.deltaTime;
    }
    public void AttemptGrapple(InputAction.CallbackContext context)
    {
        if (context.interaction is PressInteraction)
        {
            attemptSwing = !attemptSwing;
            if(attemptSwing)
            {
                StartSwinging();
            }
            else
            {
                StopSwinging();
            }
        }
        else if (context.interaction is HoldInteraction)
        {
            Debug.Log("Hold");
            isHold = true;
        }
    }

    public void GrappleCanceled(InputAction.CallbackContext context)
    {
        if (isHold)
        {
            isHold = false;
        }
    }

    public bool GetSwinging()
    {
        return isSwinging;
    }

}
