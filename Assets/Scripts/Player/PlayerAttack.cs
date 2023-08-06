using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator; // left public in case the attack animation is a child of the player
    public Camera camera; // left public in case camera is not within the player

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

    // Start is called before the first frame update
    void Start()
    {
        
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

        float currentKnockback;
        Vector3 position = camera.transform.position + camera.transform.forward * radius;
        Vector3 direction;

        if (enableDebugSphere)
        {
            StartCoroutine(DebugSphere(position, radius));
        }

        Collider[] enemies = Physics.OverlapSphere(position, radius, 256); // 256 = Enemy layer

        // Knockback enemies within the sphere
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].attachedRigidbody.velocity = Vector3.zero;
            currentKnockback = Mathf.Lerp(knockback, 1, (Vector3.Distance(transform.position, enemies[i].transform.position) - distanceThreshold) / (radius * 2));
            direction = Vector3.Normalize(enemies[i].transform.position - transform.position);
            enemies[i].attachedRigidbody.AddForce(direction * currentKnockback, ForceMode.VelocityChange);
            // collisions[i].gameObject.EnemyHealth.TakeDamage(standardDamage); //!!! for when enemy health is created which should theoretically work !!!
        }

        // Charged Attacks reflect bullets within the sphere
        if (isCharged)
        {
            Collider[] bullets = Physics.OverlapSphere(position, radius, 4); // 4 = Ignore Raycast layer, which the bullet is using. However, the bullet should be moved to its own layer.

            for (int i = 0; i < bullets.Length; i++)
            {
                bullets[i].attachedRigidbody.velocity *= -1;
            }

            isCharged = false;
        }
    }

    // Visualizes the hitbox
    private IEnumerator DebugSphere(Vector3 position, float radius)
    {
        GameObject temp = Instantiate(hitboxSphere, position, new Quaternion());
        temp.transform.localScale = Vector3.one * radius * 2;
        yield return new WaitForSeconds(1);
        Destroy(temp);
    }
}
