using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage; 
    private float maxHealth = 100f; 
    private float currentHealth;
    public ShakeEffect shakeEffect;  


    void Start()
    {
        // At start of the game, the health is max
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (fillImage != null) 
        {
            float fillValue = currentHealth / maxHealth;
            fillImage.fillAmount = fillValue;
        } 
        else 
        {
            Debug.LogError("Fill Image is not assigned in the Inspector");
        }
    }

    public void DecreaseHealthButton()
    {
        // reduce health by 10
        currentHealth -= 10;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        UpdateHealthBar();
        shakeEffect.TriggerShake();
    }
}
