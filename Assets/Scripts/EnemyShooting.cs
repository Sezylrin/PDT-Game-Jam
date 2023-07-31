using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    private enum EnemyState {Patrolling, Shooting};
    private EnemyState enemyState;

    [Header("Player Detection")]
    private GameObject player;
    [SerializeField] private float detectionDistance;
    private float distanceFromPlayer;

    [Header("Projectile Properties")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private Vector3 projectileSize;
    [SerializeField, Range(0, 60), Tooltip("Time in seconds before projectile despawns")] public float projectileLifespan;
    [SerializeField, Tooltip("Projectiles fired per minute")] private float fireRate;
    private float fireRateCountdown;
    private float fireRateTime;
    private Vector3 projectileSpawnPosition;

    private void Start()
    {
        player = GameObject.FindWithTag(Tags.T_Player);
    }

    private void Update()
    {
        projectileSpawnPosition = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
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
        distanceFromPlayer = Vector3.Distance(player.transform.position, this.transform.position);

        // Set state depending on conditions
        if(distanceFromPlayer <= detectionDistance && IsPlayerVisible())
        {
            this.transform.LookAt(player.transform);
            enemyState = EnemyState.Shooting;
        }
        else
        {
            enemyState = EnemyState.Patrolling;
        }
    }

    // Checks if player is within line of sight with raycast
    private bool IsPlayerVisible()
    {
        Vector3 raycastDirection = player.transform.position - this.transform.position;
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
            ShootProjectile();
            fireRateTime = fireRateCountdown;
        }
    }

    // Spawns a projectile and applies a force to it
    private void ShootProjectile()
    {
        GameObject projectile = Instantiate(this.projectile, projectileSpawnPosition, this.transform.rotation);
        projectile.GetComponent<Rigidbody>().AddForce(transform.forward * projectileSpeed);
        projectile.GetComponent<EnemyProjectile>().SetValues(projectileLifespan);
    }

    // Calculates time between shots based on rate of fire
    private void ProcessFireRate()
    {
        fireRateCountdown = 60f / fireRate;
    }
}