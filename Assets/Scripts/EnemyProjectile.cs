using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float lifespan;
    private float timeRemaining;

    private void Update()
    {
        if(timeRemaining <= 0f)
        {
            Destroy(this.gameObject);
        }

        timeRemaining -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) 
    {
        switch(other.tag)
        {
            case Tags.T_Player:
                // Do damage to the player
                Destroy(this.gameObject);
                break;
            case Tags.T_Enemy:
                // Ignores enemies
                break;
            default:
                Destroy(this.gameObject);
                break;
        }
    }

    public void SetValues(float projectileLifespan)
    {
        lifespan = projectileLifespan;
        timeRemaining = lifespan;
    }
}