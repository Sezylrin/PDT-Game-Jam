using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    private EnemyShooting EnemyShooting;
    private Pathfinding.AIDestinationSetter AIDestinationSetter;
    private GameObject player;
    [SerializeField] private GameObject patrolPoint;
    [SerializeField] float patrolAreaRadius;

    private void Awake()
    {
        player = GameObject.FindWithTag(Tags.T_Player);
        EnemyShooting = this.GetComponentInChildren<EnemyShooting>();
        AIDestinationSetter = this.GetComponent<Pathfinding.AIDestinationSetter>();
    }

    private void Start()
    {
        patrolPoint = new GameObject("AIPatrolPoint");
        patrolPoint.transform.position = this.transform.position;
    }

    private void Update()
    {
        if(EnemyShooting == null)
        {
            Debug.Log("EnemyShooting.cs not found. Ensure the script is attached to the enemy.");
            return;
        }

        if(AIDestinationSetter == null)
        {
            Debug.Log("AIDestinationSetter not found. Ensure the script is attached to the enemy AI");
            return;
        }

        switch(EnemyShooting.enemyState)
        {
            case EnemyShooting.EnemyState.Patrolling:
                AIDestinationSetter.target = patrolPoint.transform;
                break;

            // Set destination to the player when the player is in range and line of sight
            case EnemyShooting.EnemyState.Shooting:
                AIDestinationSetter.target = player.transform;
                break;
            
            default:
                break;
        }
    }
}