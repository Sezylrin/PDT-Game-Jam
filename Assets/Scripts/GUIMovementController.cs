using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIMovementController : MonoBehaviour
{
    [Tooltip("The speed which the GUI moves")]
    public float moveModifier = 0.5f;

    [Tooltip("The speed which the GUI returns to its original position")]
    public float returnSpeed = 0.1f;

    [Tooltip("The sensitivity for when the GUI moves")]
    public float mouseDistanceThreshold = 50.0f;

    [Tooltip("The max distance the GUI is dragged by the mouse")]
    public float maxDragDistance = 20.0f;

    [Tooltip("The max distance the GUI is shaking")]
    public float maxShakeDistance = 30.0f;

    [Tooltip("The time which the GUI shakes")]
    public float shakeTime = 1.0f;

    public static GUIMovementController controller;

    RectTransform GUI;

    float distance;
    Vector2 originalPos;
    Vector2 mouseVector;
    Vector2 mousePos;
    Vector2 previousMousePos;

    bool isShaking;
    float shake;

    // Start is called before the first frame update
    void Start()
    {
        controller = this;
        isShaking = false;
        shake = 0.0f;
        GUI = GetComponent<RectTransform>();
        originalPos = GUI.position;
        previousMousePos = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        CheckMouse();
        Shake();
        GUIMove();
    }

    void CheckMouse()
    {
        mousePos = Input.mousePosition;
        mouseVector = mousePos - previousMousePos;
        distance = Vector2.Distance(mouseVector, Vector2.zero);
        
        if (distance <= mouseDistanceThreshold)
        {
            mouseVector = Vector2.zero;
        }
    }

    void GUIMove()
    {
        if (Vector2.Distance(originalPos, GUI.position) <= maxDragDistance)
        {
            if (mouseVector != Vector2.zero)
            {
                GUI.position += (Vector3)(mouseVector * Time.deltaTime / moveModifier);
            }
            else
            {
                GUI.position += ((Vector3)originalPos - GUI.position) * Time.deltaTime / returnSpeed;
            }
        }
        else
        {
            GUI.position += ((Vector3)originalPos - GUI.position) * Time.deltaTime / returnSpeed;
        }
        previousMousePos = mousePos;
    }

    void Shake()
    {
        if (isShaking)
        {
            GUI.position = originalPos + Random.insideUnitCircle * shake;

            if (shake > 0)
            {
                shake -= maxShakeDistance * Time.deltaTime / shakeTime;
            }
            else
            {
                shake = 0;
                isShaking = false;
            }
        }
    }

    public static void StartShake()
    {
        controller.InitShake();
    }

    public void InitShake()
    {
        isShaking = true;
        shake = maxShakeDistance;
    }

    /*public static void SetMoveSpeed(float num)
    {
        moveModifier = num;
    }

    public static void SetSensitivity(float num)
    {
        mouseDistanceThreshold = num;
    }

    public static void SetDragDistance(float num)
    {
        maxDragDistance = num;
    }

    public static void SetShakeDistance(float num)
    {
        maxShakeDistance = num;
    }*/
}
