using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFX : MonoBehaviour
{
    static CameraFX manager;
    Vector3 cameraLocalPos; // The camera local position from the parent

    // Start is called before the first frame update
    void Start()
    {
        manager = this;
        cameraLocalPos = transform.position;
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    public static void CameraShake(float intensity = 1.0f, float duration = 1.0f)
    {
        manager.StartCoroutine(manager.Shake(intensity, duration));
    }

    IEnumerator Shake(float intensity, float duration)
    {
        float timer = 0.0f;
        float distance = intensity;

        while (timer < duration)
        {
            transform.position = cameraLocalPos + (Vector3)Random.insideUnitCircle * distance;
            distance = Mathf.Lerp(intensity, 0, 1 - Mathf.Pow(1 - timer / duration, 3));
            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        transform.position = cameraLocalPos;
        yield return null;
    }

    public void TestShake()
    {
        CameraShake(0.8f, 0.2f);
    }
}
