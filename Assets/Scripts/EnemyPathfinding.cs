using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    private EnemyShooting EnemyShooting;
    private Pathfinding.AIDestinationSetter AIDestinationSetter;
    private Pathfinding.AIPath AIPath;
    private GameObject player;

    private enum PatrolBehaviour {PatrolArea, PatrolPath, Stationary};
    [SerializeField] private PatrolBehaviour patrolBehaviour;
    [SerializeField, Tooltip("Follow the player when the player is detected. When false, become stationary when the player is detected.")] private bool followPlayer;

    [Header("Patrol Area")]
    [SerializeField, Tooltip("If null then a patrolPoint will be generated at the enemy's position on Start.")] private GameObject patrolPoint;
    [SerializeField] float patrolAreaRadius;
    private GameObject patrolTarget;

    [Header("Patrol Path")]
    [SerializeField, Tooltip("Go back to the first patrolPoint upon reaching the final patrolPoint. When false, go through each patrolPoint in reverse.")] private bool useCircularPath;
    [SerializeField, Tooltip("Points that the enemy will follow along a path.")] private Transform[] patrolPoints;
    private bool patrolReversed;
    private int nextPatrolPoint;

    private void Awake()
    {
        player = GameObject.FindWithTag(Tags.T_Player);
        EnemyShooting = this.GetComponentInChildren<EnemyShooting>();
        AIDestinationSetter = this.GetComponent<Pathfinding.AIDestinationSetter>();
        AIPath = this.GetComponent<Pathfinding.AIPath>();
    }

    private void Start()
    {
        if(patrolPoint == null)
        {
            patrolPoint = new GameObject("AIPatrolPoint");
        }
        patrolPoint.transform.position = this.transform.position;

        patrolTarget = new GameObject("PatrolTarget");
        patrolTarget.transform.position = patrolPoint.transform.position;

        nextPatrolPoint = 0;
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
            // Pathfinding behaviour when the player is not detected
            case EnemyShooting.EnemyState.Patrolling:
                ProcessPatrolBehaviour();
                break;

            // Pathfinding behaviour when the player is in range and line of sight
            case EnemyShooting.EnemyState.Shooting:
                // Stay stationary
                if(!followPlayer)
                {
                    AIDestinationSetter.target = this.transform;
                    break;
                }
                // Follow the player player
                AIDestinationSetter.target = player.transform;
                break;
            
            default:
                break;
        }
    }

    private void ProcessPatrolBehaviour()
    {
        switch(patrolBehaviour)
        {
            // Set destination to a random point close to patrolPoint
            case PatrolBehaviour.PatrolArea:
                if(AIPath.reachedDestination)
                {
                    MovePatrolTarget();
                }
                AIDestinationSetter.target = patrolTarget.transform;
                break;

            // Follow a predetermined path made of several points
            case PatrolBehaviour.PatrolPath:
                if(AIPath.reachedDestination)
                {
                    FindNextPatrolPoint();
                }
                AIDestinationSetter.target = patrolPoints[nextPatrolPoint];
                break;
            
            // Stays stationary
            case PatrolBehaviour.Stationary:
                AIDestinationSetter.target = this.transform;
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

    // Finds next the next patrol point on the path
    private void FindNextPatrolPoint()
    {
        // The path will loop in a circle
        if(useCircularPath)
        {
            if(nextPatrolPoint == (patrolPoints.Length - 1))
            {
                nextPatrolPoint = 0;
            }
            else
            {
                nextPatrolPoint++;
            }
        }
        // The enemy will go back and forth along the path
        else
        {
            if(patrolReversed)
            {
                if(nextPatrolPoint == 0)
                {
                    patrolReversed = false;
                    nextPatrolPoint++;
                }
                else
                {
                    nextPatrolPoint--;
                }
            }
            else
            {
                if(nextPatrolPoint == (patrolPoints.Length - 1))
                {
                    patrolReversed = true;
                    nextPatrolPoint--;
                }
                else
                {
                    nextPatrolPoint++;
                }
            }
        }
    }
}