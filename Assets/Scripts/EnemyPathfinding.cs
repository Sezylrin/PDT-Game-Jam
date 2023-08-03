using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    private EnemyShooting EnemyShooting;
    private Pathfinding.AIDestinationSetter AIDestinationSetter;
    private Pathfinding.AIPath AIPath;
    private GameObject player;
    [SerializeField] private GameObject patrolPoint;
    [SerializeField] float patrolAreaRadius;
    private GameObject patrolTarget;

    private void Awake()
    {
        player = GameObject.FindWithTag(Tags.T_Player);
        EnemyShooting = this.GetComponentInChildren<EnemyShooting>();
        AIDestinationSetter = this.GetComponent<Pathfinding.AIDestinationSetter>();
        AIPath = this.GetComponent<Pathfinding.AIPath>();
    }

    private void Start()
    {
        patrolPoint = new GameObject("AIPatrolPoint");
        patrolPoint.transform.position = this.transform.position;

        patrolTarget = new GameObject("PatrolTarget");
        patrolTarget.transform.position = patrolPoint.transform.position;
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

        if(AIPath == null)
        {
            Debug.Log("AIPath not found. Ensure the script is attached to the enemy AI");
            return;
        }

        switch(EnemyShooting.enemyState)
        {
            // Set destination to a random point close to patrolPoint
            case EnemyShooting.EnemyState.Patrolling:
                if(AIPath.reachedDestination)
                {
                    MovePatrolTarget();
                }
                AIDestinationSetter.target = patrolTarget.transform;
                break;

            // Set destination to the player when the player is in range and line of sight
            case EnemyShooting.EnemyState.Shooting:
                AIDestinationSetter.target = player.transform;
                break;
            
            default:
                break;
        }
    }

    // Randomises the position of patrolTarget within a circular area with patrolPoint as the origin
    // The size of the circular area is dependant on the patrolAreaRadius float
    private void MovePatrolTarget()
    {
        Vector2 rand = Random.insideUnitCircle * patrolAreaRadius;
        patrolTarget.transform.position = new Vector3(patrolPoint.transform.position.x + rand.x, patrolPoint.transform.position.y, patrolPoint.transform.position.z + rand.y);
    }
}