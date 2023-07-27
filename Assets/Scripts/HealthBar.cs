using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider tempSlider; //!!! will be removed later when player movement to completed !!!
    public Text tempText; //!!! will be removed later when player movement to completed !!!

    [Header("Heath Bars")]
    public Image background;
    public Image foregroundLeftBar;
    public Image foregroundRightBar;



    [Header("Health Stats")]
    [Tooltip("The starting percentage of health when the player starts a level")]
    [Range(0, 1)] public float startingPercentage = 0.5f;

    [Tooltip("The minimum speed before player's health will start decreasing")] 
    public float speedThreshold = 20.0f;

    [Tooltip("The base rate the player loses health per second when under the speed threshold")]
    [Range(0, 1)] public float baseDrainRate = 0.1f;

    [Tooltip("The maximum drain rate when player reaches 0 speed")]
    public float maxDrainMultiplier = 3.0f;

    [Tooltip("The base rate the player gains health per second when above the speed threshold")]
    [Range(0, 1)] public float baseRegenRate = 0.1f;

    [Tooltip("The maximum regen rate when player reaches full speed benefit")]
    public float maxRegenMultiplier = 5.0f;

    [Tooltip("The time it takes for health to change when adding or subtracting a fixed amount of health")]
    public float healthChangeTime = 0.2f;

    [Tooltip("The speed for reaching maximum regen rate")]
    public float maxSpeedBenefit = 100.0f;



    [Header("Colours")]
    // Colours to represent low to high speed
    public Gradient backgroundColor;

    // Colours to represent zero to max health
    public Gradient foregroundColor;

    [Tooltip("Minimum health to reach start of gradient colour")]
    public float minHealthColor = 0.2f;



    public static HealthBar healthBar;
    [Range(0, 1)] float currentHealth; // Current health in percentage
    float previousHealth;
    float currentChange; // the current percentage remaining to add unto health
    float timer;
    bool isChange; // bool for when not to naturally regen or drain



    float tempSpeed; //!!! will be removed later when player movement to completed !!!

    // Start is called before the first frame update
    void Start()
    {
        healthBar = this;
        currentHealth = startingPercentage;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBar();
        UpdateHealth();

        tempSpeed = tempSlider.value;
        tempText.text = "Speed: " + tempSpeed;
    }

    // Updates the current health with lerp
    void UpdateHealth()
    {
        if (currentHealth > 1)
        {
            currentHealth = 1;
        }

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // Regen or drain health based on current speed
        if (!isChange)
        {
            float multiplier;

            if (tempSpeed >= speedThreshold)
            {
                multiplier = Mathf.Lerp(1, maxRegenMultiplier, (tempSpeed - speedThreshold) / (maxSpeedBenefit - speedThreshold));
                currentHealth += baseRegenRate * multiplier * Time.deltaTime;
            }
            else
            {
                multiplier = Mathf.Lerp(1, maxDrainMultiplier, 1 - (tempSpeed / speedThreshold));
                currentHealth -= baseDrainRate * multiplier * Time.deltaTime;
            }

            Debug.Log(multiplier);
        }
        // Rapid change in health using quart lerp
        else
        {
            timer += Time.deltaTime;
            currentHealth = Mathf.Lerp(previousHealth, previousHealth + currentChange, 1 - Mathf.Pow(1 - (timer / healthChangeTime), 4));

            if (timer >= healthChangeTime)
            {
                isChange = false;
            }
        }
    }

    // Updates the colours and fill rate of the health bar
    void UpdateBar()
    {
        background.color = backgroundColor.Evaluate(tempSpeed / maxSpeedBenefit);
        foregroundLeftBar.color = foregroundColor.Evaluate((currentHealth - minHealthColor) / (1 - minHealthColor));
        foregroundRightBar.color = foregroundColor.Evaluate((currentHealth - minHealthColor) / (1 - minHealthColor));

        foregroundLeftBar.fillAmount = currentHealth;
        foregroundRightBar.fillAmount = currentHealth;
    }

    [Tooltip("Modify health values in fixed amount")]
    public static void AddHealth(float amount)
    {
        healthBar.ChangeHealth(amount);
    }

    void ChangeHealth(float amount)
    {
        isChange = true;
        timer = 0.0f;
        previousHealth = currentHealth;
        currentChange = amount;
    }

    //!!! will be removed later when player movement to completed !!!
    public static void STATICTEMPCHANGESPEED(float amount)
    {
        healthBar.TEMPCHANGESPEED(amount);
    }

    void TEMPCHANGESPEED(float amount)
    {
        tempSpeed += amount;
        tempSlider.value = tempSpeed;
    }
}
