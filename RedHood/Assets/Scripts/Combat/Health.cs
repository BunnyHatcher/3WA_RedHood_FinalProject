using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 100;

    public int healthValue;

    //---------------GETTERS / SETTERS-----------------------------------------------------------
    
    //Getter for healthValue
    public int GetHealthValue() { return healthValue; }
    

    // Setter for healthValue
    public void SetHealthValue(int value) { healthValue = value; }
       
    
    //---------------------------------------------------------------------------------------------
    
    
    public PlayerHealthbar playerHealthbar;

    public bool isInvulnerable = false;

    public event Action OnTakeDamage; //event to be invoked whenever player or enemy takes damage
    public event Action OnDie; // event to be invoked whenever player or enemy dies

    public bool IsDead => healthValue == 0; //short way to check anywhere else if IsDead is true and returning if health is 0 

    //---------E V E N T S----------------------------------------------------------------------------------------

    public UnityEvent damageTakenEvent;

    public UnityEvent whenKilledEvent;


    //-----------------------------------------------------------------------------------------------------


    protected virtual void Start()
    {
        // at the start - don't forget to set the player's healt as well as his healthbar to max value
        healthValue = maxHealth;
        playerHealthbar.SetMaxHealth(maxHealth);

    }

    public void SetInvulnerable(bool isInvulnerable)
    {
        this.isInvulnerable = isInvulnerable;

    }

    public void DealDamage(int damage)
    {
        if(healthValue <= 0) { return; }

        if(isInvulnerable)  // cancel calculation if invulnerability is turned on
        {
            return;
        }

        healthValue = Mathf.Max(healthValue - damage, 0); // returns either current healthValue value or 0 if it goes below 0

        //playerHealthbar.SetHealth(healthValue);

        OnTakeDamage?.Invoke(); // invoke event when damage is dealt

        //Invoke Events when taking damage -  why not using the OnTakeDamage Invoke from above? Well, it seems like that is another kind of event, an Action, not a Unity event
        damageTakenEvent.Invoke();

        
        
        
        if(healthValue == 0)
        {

            OnDie?.Invoke(); // invoke event when death has taken his toll

            //Invoke Unity Events on death
            whenKilledEvent.Invoke();

        }

        //Debug.Log(healthValue);
    }

    
}
