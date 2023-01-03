using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectsManager : CharacterEffectsManager
{

    [Header("Healthbar")]
    [SerializeField] private Health health;
    [SerializeField] private PlayerHealthbar playerHealthbar;

    // Set HealthBar to max health on start
    private void Start()
    {
        //playerHealth.GetHealthValue();
        //playerHealth.SetHealthValue(100);

        // at the start - don't forget to set the player's healt as well as his healthbar to max value
        //healthValue = maxHealth;
        //playerHealthbar.SetMaxHealth(maxHealth);


        



    }

    public void DiminishHealthbar()
    {
            
        playerHealthbar.SetHealth(health.healthValue);

    }
    



}
