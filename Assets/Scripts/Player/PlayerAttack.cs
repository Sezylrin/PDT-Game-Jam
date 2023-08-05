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
            Debug.Log("Starting Attack");
        }
        else
        {
            Debug.Log("On Cooldown");
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
                // A large attack that damages enemies in front of the player with more range, damage, knockback. 
                StartCoroutine(PrimaryAttack(chargedRadius, chargedKnockback, chargedDamage));
            }
            else
            {
                // Attack that damages enemies in front of the player
                StartCoroutine(PrimaryAttack(standardRadius, standardKnockback, standardDamage));
            }
        }
    }

    // Allows charge attack when input is held long enough
    public void FullCharge(InputAction.CallbackContext context)
    {
        if (isCooldown)
        {
            //Debug.Log("Full Charge");
            isCharged = true;
        }
    }

    private IEnumerator PrimaryAttack(float radius, float knockback, float damage)
    {
        yield return new WaitForSeconds(attackDelay);

        Vector3 position = transform.position + Vector3.forward * radius;
        Collider[] collisions = Physics.OverlapSphere(position, radius, 256); // 256 = enemy layer

        float currentKnockback;

        if (enableDebugSphere)
        {
            StartCoroutine(DebugSphere(position, radius));
        }

        for (int i = 0; i < collisions.Length; i++)
        {
            collisions[i].attachedRigidbody.velocity = Vector3.zero;
            currentKnockback = Mathf.Lerp(knockback, 1, (Vector3.Distance(transform.position, collisions[i].transform.position) - distanceThreshold) / (radius * 2));
            //Debug.Log(knockback);
            collisions[i].attachedRigidbody.AddForce(transform.forward * currentKnockback, ForceMode.VelocityChange);
            // collisions[i].gameObject.EnemyHealth.TakeDamage(standardDamage); //!!! for when enemy health is created which should theoretically work !!!
        }
    }

    // Pushes enemies to other nearby enemies
    private bool ReboundEntity()
    {
        return true;
    }

    private IEnumerator DebugSphere(Vector3 position, float radius)
    {
        GameObject temp = Instantiate(hitboxSphere, position, new Quaternion());
        temp.transform.localScale = Vector3.one * radius * 2;
        yield return new WaitForSeconds(1);
        Destroy(temp);
    }
}
