using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{

    //[SerializeField] private PlayerHealthbar playerHealthbar;

    protected override void Start()
    {
        // at the start - don't forget to set the player's healt as well as his healthbar to max value
        healthValue = maxHealth;
        playerHealthbar.SetMaxHealth(maxHealth);

    }

    public void DiminishHealthbar()
    {

        playerHealthbar.SetHealth(healthValue);

    }









}
