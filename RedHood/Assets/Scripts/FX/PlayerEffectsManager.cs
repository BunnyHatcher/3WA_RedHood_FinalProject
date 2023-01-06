using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectsManager : CharacterEffectsManager
{
    [Header("WeaponLogic")]
    [SerializeField] GameObject weaponLogic_L;
    [SerializeField] GameObject weaponLogic_R;

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

    public void SetHealthbar()
    {

        playerHealthbar.SetHealth(health.healthValue);

    }


    public override void PlayFX()
    {
        //Debug.Log("BloodFX played");

        // get bool isInvulnerable and use it to create if condition for shield block effects
        if (scriptHealth.isInvulnerable == true)
        {
            Debug.Log("ShieldFX played");

            //GameObject rig = GameObject.Find("mixamorig:Hips");
            GameObject shieldImpact = GameObject.Find("mixamorig:Shield_joint");

            // trigger ShieldBlockFX
            GameObject instance = Instantiate(shieldBlockFX, shieldImpact.transform.position, Quaternion.identity);
            instance.transform.SetParent(shieldImpact.transform);// to link effect to parent
        }

        else
        {
            // trigger BloodFX
            var instance = Instantiate(bloodFX[Random.Range(0, bloodFX.Length)], weaponLogic_L.transform.position, Quaternion.identity);

        }


    }
}
