using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    public GameObject explosion;

    static ExplosionManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = this;
    }

    // Update is called once per frame
    /*void Update()
    {

    }*/

    public static void CastExplosion(Vector3 position, float radius, float force = 200.0f, float upwardForce = 50.0f, float lifetime = 1.2f)
    {
        ExplodeFX(position, lifetime);
        Collider[] entities = Physics.OverlapSphere(position, radius);

        for (int i = 0; i < entities.Length; i++)
        {
            if (entities[i].attachedRigidbody)
            {
                entities[i].attachedRigidbody.AddExplosionForce(force, position, radius);
                //Debug.Log(entities[i].gameObject.name);
            }
        }
    }

    public void TestExplosion()
    {
        CastExplosion(Vector3.zero, 20, 300);
    }

    static void ExplodeFX(Vector3 position, float lifetime)
    {
        manager.StartCoroutine(manager.ExplosionFX(position, lifetime));
    }

    IEnumerator ExplosionFX(Vector3 position, float lifetime)
    {
        if (explosion)
        {
            GameObject temp = Instantiate(explosion, position, new Quaternion(), gameObject.transform);
            ParticleSystem[] particles = temp.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Play();
            }

            yield return new WaitForSeconds(lifetime);
            Destroy(temp);
        }
    }
}
