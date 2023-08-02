using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    public GameObject explosionParticles; // The gameobject to play the particle systems in the children during detonation
    public GameObject explosionSphere; // The gameobject to show the radius of the explosion

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

    // Create a explosion at "position" of size "radius", pushing out all rigidbodies at a force plus additional upwardForce.
    // "lifetime" affects how long Explosion FX stay active.
    public static void CastExplosion(Vector3 position, float radius, float force = 200.0f, float upwardForce = 50.0f, float lifetime = 1.2f)
    {
        ExplodeFX(position, radius, lifetime);
        Collider[] entities = Physics.OverlapSphere(position, radius);

        // Look for all rigidbodies and apply explosion force onto them
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
        CastExplosion(Vector3.zero, 10, 300);
    }

    // Method to call ExplosionFX statically
    static void ExplodeFX(Vector3 position, float radius, float lifetime)
    {
        manager.StartCoroutine(manager.ExplosionFX(position, lifetime));
        manager.StartCoroutine(manager.ExplosionSphere(position, radius, 0.2f, lifetime));
    }

    // Instantiate "explosionParitcles" gameobject and play all particle systems in children and destroy after "lifetime" seconds.
    IEnumerator ExplosionFX(Vector3 position, float lifetime)
    {
        if (explosionParticles)
        {
            GameObject temp = Instantiate(explosionParticles, position, new Quaternion(), gameObject.transform);
            ParticleSystem[] particles = temp.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Play();
            }

            yield return new WaitForSeconds(lifetime);
            Destroy(temp);
        }
    }

    // Instantiate a sphere to show the explosion radius. The sphere grows to full size within "growthTime" seconds and fades away within "lifetime" seconds.
    IEnumerator ExplosionSphere(Vector3 position, float radius, float growthTime, float lifetime)
    {
        float time = 0;

        GameObject temp = Instantiate(explosionSphere, position, new Quaternion(), gameObject.transform);
        Transform tempt = temp.transform;
        Material material = temp.GetComponent<Renderer>().material;
        Color color = material.GetColor("_Color");

        float alpha = color.a;
        tempt.localScale = Vector3.zero;

        while (time < lifetime)
        {
            if (time <= growthTime)
            {
                float growValue = Mathf.Lerp(0, radius * 2, 1 - Mathf.Pow(1 - (time / growthTime), 5));
                tempt.localScale = new Vector3(growValue, growValue, growValue);
            }
            else
            {
                float a = Mathf.Lerp(color.a, 0, (time - growthTime) / (lifetime - growthTime));
                material.SetColor("_Color", new Color(color.r, color.g, color.b, a));
            }

            time += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        Destroy(temp);
        yield return null;
    }
}
