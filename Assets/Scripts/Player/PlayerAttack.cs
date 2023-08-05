using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator; // left public in case the attack animation is a child of the player

    [Header("Main Stats")]
    public float attackCooldown = 0.5f;
    public float attackDelay = 0.0f; // time in seconds before the player starts the attack. used to help synchronise with animation
    public float distanceThreshold = 3.0f; // minimum distance between the player and enemy to reach full knockback strength
    public Vector3 hitboxOffset;

    [Header("Standard Attack")]
    public float standardDamage = 10.0f;
    public float standardRadius = 3.0f;
    public float standardKnockback = 150.0f;

    [Header("Charged Attack")]
    public float chargeTime = 1.0f;
    public float chargedDamage = 40.0f;
    public float chargedRadius = 5.0f;
    public float chargedKnockback = 450.0f;

    [Header("Debug")]
    public bool enableDebugSphere;
    public GameObject hitboxSphere; // sphere used to show the hitbox size for debugging
    public float debugLifetime; // how long the debug sphere will appear

    private bool isCharged;
    private bool isCooldown;
    private float timer;

    private Punch inputs; // !!! must be moved and implemented in input manager when merged !!!

    void Awake()
    {
        inputs = new Punch(); // !!! must be moved and implemented in input manager when merged !!!
    }

    // Start is called before the first frame update
    void Start()
    {
        inputs.Enable(); // !!! must be moved and implemented in input manager when merged !!!
        inputs.Gameplay.Attack.started += OnAttack; // !!! must be moved and implemented in input manager when merged !!!
        inputs.Gameplay.Attack.canceled += ReleaseAttack; // !!! must be moved and implemented in input manager when merged !!!
        inputs.Gameplay.Attack.performed += FullCharge; // !!! must be moved and implemented in input manager when merged !!!
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
    }

    // Start up attack charging on input press
    public void OnAttack(InputAction.CallbackContext context)
    {
        isCooldown = timer >= attackCooldown;

        /*if (isCooldown)
        {
            Debug.Log("1. Starting Attack");
        }
        else
        {
            Debug.Log("X. On Cooldown");
        }*/
    }

    // Upon input release, do charge or standard attack depending on the charge time
    public void ReleaseAttack(InputAction.CallbackContext context)
    {
        if (isCooldown)
        {
            timer = 0.0f;

            if (isCharged)
            {
                StartCoroutine(ChargeAttack());
            }
            else
            {
                StartCoroutine(StandardAttack());
            }
        }
    }

    // Attack that damages enemies in front of the player
    private IEnumerator StandardAttack()
    {
        //Debug.Log("2. Standard Attack");
        yield return new WaitForSeconds(attackDelay);
        Vector3 position = transform.position + Vector3.forward * standardRadius;
        Collider[] collisions = Physics.OverlapSphere(position, standardRadius, 256); // 256 = enemy layer
        float knockback;

        if (enableDebugSphere)
        {
            StartCoroutine(DebugSphere(position, standardRadius));
        }

        for (int i = 0; i < collisions.Length; i++)
        {
            collisions[i].attachedRigidbody.velocity = Vector3.zero;
            knockback = Mathf.Lerp(standardKnockback, 1, (Vector3.Distance(transform.position, collisions[i].transform.position) - distanceThreshold)/ (standardRadius * 2));
            //Debug.Log(knockback);
            collisions[i].attachedRigidbody.AddForce(transform.forward * knockback, ForceMode.VelocityChange);
            // collisions[i].gameObject.EnemyHealth.TakeDamage(standardDamage); //!!! for when enemy health is created which should theoretically work !!!
        }
    }

    // Allows charge attack when input is held long enough
    public void FullCharge(InputAction.CallbackContext context)
    {
        if (isCooldown)
        {
            //Debug.Log("3. Full Charge");
            isCharged = true;
        }
    }

    // A large attack that damages enemies in front of the player with more range, damage, knockback. 
    private IEnumerator ChargeAttack()
    {
        //Debug.Log("4. Charged Attack");
        yield return new WaitForSeconds(attackDelay);
        Vector3 position = transform.position + Vector3.forward * chargedRadius;
        Collider[] collisions = Physics.OverlapSphere(position, chargedRadius, 256); // 256 = enemy layer
        float knockback;

        if (enableDebugSphere)
        {
            StartCoroutine(DebugSphere(position, chargedRadius));
        }

        for (int i = 0; i < collisions.Length; i++)
        {
            collisions[i].attachedRigidbody.velocity = Vector3.zero;
            knockback = Mathf.Lerp(chargedKnockback, 1, (Vector3.Distance(transform.position, collisions[i].transform.position) - distanceThreshold) / (chargedRadius * 2));
            //Debug.Log(knockback);
            collisions[i].attachedRigidbody.AddForce(transform.forward * knockback, ForceMode.VelocityChange);
            // collisions[i].gameObject.EnemyHealth.TakeDamage(chargedDamage); //!!! for when enemy health is created which should theoretically work !!!
        }

        isCharged = false;
    }

    private IEnumerator DebugSphere(Vector3 position, float radius)
    {
        GameObject temp = Instantiate(hitboxSphere, position, new Quaternion());
        temp.transform.localScale = Vector3.one * radius * 2;
        yield return new WaitForSeconds(1);
        Destroy(temp);
    }
}
