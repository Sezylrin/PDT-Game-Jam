using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("Player Detection")]
    private GameObject player;
    [SerializeField] private float detectionDistance;
    private float distanceFromPlayer;

    [Header("Projectile Properties")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private Vector3 projectileSize;
    [SerializeField] private float projectileLifespan;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        ProcessDetection();
    }

    // Handles player detection and calling necessary methods
    private void ProcessDetection()
    {
        distanceFromPlayer = Vector3.Distance(player.transform.position, this.transform.position);

        if(distanceFromPlayer <= detectionDistance && IsPlayerVisible())
        {
            // shoot
            Debug.Log("Player detected");
        }
    }

    // Checks if player is within line of sight
    private bool IsPlayerVisible()
    {
        Vector3 raycastDirection = player.transform.position - this.transform.position;
        RaycastHit hit;
        if(Physics.Raycast(this.transform.position, raycastDirection, out hit))
        {
            if(hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
}