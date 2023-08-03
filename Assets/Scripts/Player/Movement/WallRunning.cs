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
    [SerializeField]
    private PlayerController player;

    [Header("Wall Run Values")]
    [SerializeField]
    private float scanDist;

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

    public void CheckForWall()
    {
        if (!isAorD || player.CurrentState != PlayerStates.InAir)
            return;
        Vector3 dir = (isLeft ? transform.right * -1f : transform.right);
        if (Physics.Raycast(transform.position, dir, out hit, scanDist, terrain))
        {
            isWallRunning = true;
        }
    }

    public void CheckInputs(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<Vector2>().y;
        isAorD = !input.Equals(0);
        if (isAorD)
            isLeft = input < 0;
    }
}
