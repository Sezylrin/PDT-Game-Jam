using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public enum EnemyState { Patrolling, Shooting };
    public EnemyState enemyState;
    [Tooltip("The mesh of the enemy that rotates to the player")] public Transform mesh;
    //public GameObject turret; !!! when the enemy gun has a separate model
    [Tooltip("The speed which the turret turns")] public float lookSpeed;

    [Header("Player Detection")]
    private Transform player;
    [SerializeField] private float detectionDistance;
    private float distanceFromPlayer;

    [Header("Projectile Properties")]
    [SerializeField] private GameObject projectile;
    [SerializeField, Tooltip("The required angle between \"where it wants to look at\" VS \"where it is currently looking at\" before it start shooting")] private float requiredAngle;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private Vector3 projectileSize;
    [SerializeField, Range(0, 60), Tooltip("Time in seconds before projectile despawns.")] public float projectileLifespan;
    [SerializeField, Tooltip("Projectiles fired per minute.")] private float fireRate;
    [SerializeField, Tooltip("The offset position from parent where the projectile spawns")] private Transform nuzzlePosition;
    private Vector3 spawnPosition;
    private float fireRateCountdown;
    private float fireRateTime;

    private void Start()
    {
        player = GameObject.FindWithTag(Tags.T_Player).transform;
    }

    private void Update()
    {
        ProcessDetection();
        ProcessFireRate();
    }

    private void FixedUpdate() 
    {
        ProcessShooting();
    }

    // Handles player detection and calling necessary methods
    private void ProcessDetection()
    {
        // Gets distance from player every update frame
        distanceFromPlayer = Vector3.Distance(player.position, this.transform.position);

        // Set state depending on conditions
        if(distanceFromPlayer <= detectionDistance && IsPlayerVisible())
        {
            if (Physics.Raycast(transform.position, player.position - transform.position, detectionDistance, ~8)) // 8 = Player Layer
            {
                enemyState = EnemyState.Shooting;
                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0;
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                mesh.rotation = Quaternion.Lerp(mesh.rotation, lookRot, Time.deltaTime * lookSpeed);
            }
        }
        else
        {
            enemyState = EnemyState.Patrolling;
        }
    }

    // Checks if player is within line of sight with raycast
    private bool IsPlayerVisible()
    {
        Vector3 raycastDirection = player.position - this.transform.position;
        RaycastHit hit;
        if(Physics.Raycast(this.transform.position, raycastDirection, out hit))
        {
            if(hit.transform.CompareTag(Tags.T_Player))
            {
                return true;
            }
        }
        return false;
    }

    // Handles rate of fire and shooting projectiles
    private void ProcessShooting()
    {
        // Only run rest of method if enemy is in shooting state
        if(enemyState != EnemyState.Shooting)
        {
            fireRateTime = fireRateCountdown;
            return;
        }
        
        if(fireRateTime > 0f)
        {
            fireRateTime -= Time.deltaTime;
        }
        else 
        {
            // Check if enemy is alligned enough before shooting
            Vector3 dir = (player.position - transform.position).normalized;
            if (Vector3.Angle(nuzzlePosition.forward, dir) < requiredAngle)
            {
                ShootProjectile();
                fireRateTime = fireRateCountdown;
            }
        }
    }

    // Spawns a projectile and applies a force to it
    private void ShootProjectile()
    {
        if (nuzzlePosition)
        {
            spawnPosition = nuzzlePosition.position;
        }
        else
        {
            spawnPosition = transform.position;
        }

        Vector3 dir = (player.position - spawnPosition).normalized;
        GameObject projectile = Instantiate(this.projectile, spawnPosition, Quaternion.LookRotation(dir, Vector3.up));
        projectile.GetComponent<Rigidbody>().AddForce(dir * projectileSpeed);
        projectile.GetComponent<EnemyProjectile>().SetValues(projectileLifespan);
    }

    // Calculates time between shots based on rate of fire
    private void ProcessFireRate()
    {
        fireRateCountdown = 60f / fireRate;
    }
}