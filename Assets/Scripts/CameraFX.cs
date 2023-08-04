using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraFX : MonoBehaviour
{
    public GameObject player;
    public PostProcessVolume postProcess;
    [Range(0.0f, 10.0f)]
    public float caDecayRate = 1.0f;
    [Range(0.0f, 1.0f)]
    public float maxVignetteIntensity = 0.5f;

    static CameraFX manager;
    Vector3 cameraLocalPos; // The camera local position from the parent
    private ChromaticAberration ca;
    private Vignette vg;
    float maxSpeedBenefit = 100.0f; // !!! will be removed when merged with HealthBar and references from it !!!
    float currentSpeed = 80.0f; // !!! will be removed when merged with CharacterMovement and refences from it !!!
    //HealthBar health;
    //PlayerMovement playerMove;

    // Start is called before the first frame update
    void Start()
    {
        manager = this;
        cameraLocalPos = transform.position;
        postProcess.profile.TryGetSettings<ChromaticAberration>(out ca);
        postProcess.profile.TryGetSettings<Vignette>(out vg);
    }

    // Update is called once per frame
    void Update()
    {
        SpeedVignette();
        if (ca)
        {
            ca.intensity.value -= caDecayRate * Time.deltaTime;
        }
    }

    public void SetChromaticAberation(float num)
    {
        if (ca)
        {
            ca.intensity.value = num;
        }
    }

    public void SpeedVignette()
    {
        if (vg)
        {
            vg.intensity.value = Mathf.Lerp(0, maxVignetteIntensity, currentSpeed / maxSpeedBenefit); // !!! Mathf.Lerp(0, maxVignetteIntensity, playerMove.velocity / health.maxSpeedBenefit)
        }
    }

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
