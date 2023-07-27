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
    [SerializeField] private float projectileLifespan;
    [SerializeField] private float fireRate; // Projectiles per minute
    private float fireRateCountdown;
    private float fireRateTime;
    private Vector3 projectileSpawnPosition;

    private void Start()
    {
        player = GameObject.FindWithTag(Tags.T_Player);
        projectileSpawnPosition = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

        fireRateCountdown = fireRate / 60f;
    }

    private void Update()
    {
        ProcessDetection();
    }

    private void FixedUpdate() 
    {
        ProcessShooting();
    }

    // Handles player detection and calling necessary methods
    private void ProcessDetection()
    {
        distanceFromPlayer = Vector3.Distance(player.transform.position, this.transform.position);

        if(distanceFromPlayer <= detectionDistance && IsPlayerVisible())
        {
            this.transform.LookAt(player.transform);
            enemyState = EnemyState.Shooting;

            Debug.Log("Player detected");
        }
        else
        {
            enemyState = EnemyState.Patrolling;
        }
    }

    // Checks if player is within line of sight
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

    private void ProcessShooting()
    {
        Debug.Log("Shooting");

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

    private void ShootProjectile()
    {
        GameObject projectile = Instantiate(this.projectile, projectileSpawnPosition, this.transform.rotation);
        projectile.GetComponent<Rigidbody>().AddForce(transform.forward * projectileSpeed);
    }
}