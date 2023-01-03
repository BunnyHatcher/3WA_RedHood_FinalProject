using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;


public class CharacterEffectsManager : MonoBehaviour
{
    // REFACTORING ADVICE: use underscore for naming variables inside a class
    //use capital letter for naming functions
    //parameters inside a function start with a minuscule letter

    [SerializeField]Health scriptHealth;
    
    // REFACTORING ADVICE: WHY NOT USE ACCESS MODIFIER "PROTECTED"?
    [Header("WeaponFX")]
    public WeaponEffects WeaponFX;

    public ParticleSystem weaponTrail;

    
    [Header("Impact FX")]
    public GameObject[] bloodFX;
    public GameObject shieldBlockFX;

    [Header("Impact Audio")]
    public AudioClip[] weaponImpactSFX;
    public AudioClip[] shieldBlockSFX;

    [Header("Pain Audio")]
    public AudioClip[] painSFX;
    public AudioClip[] deathSFX;



    private void Awake()
    {
        
    }

    

    public virtual void PlayWeaponTrail()
    {
        weaponTrail.Play();

    }

    public virtual void StopWeaponTrail()
    {
        weaponTrail.Stop();

    }

    /*
    public void ApplyDamage(Vector3 contact)
    {
     Debug.Log("ApplyDamage");

     //assign the direction, for example Physics.Raycast(ray, out hit); direction = hit.normal;
     var direction = contact.normalized;
        
     //calculate angle of blood splatter
     float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 180;

     // trigger BloodFX
     var instance = Instantiate(bloodFX, transform.position, Quaternion.Euler(0, angle + 90, 0));

    // set ground height to enable decals on ground
    instance.GetComponent<BFX_BloodSettings>().GroundHeight = 0f; 

    }
    */


    public void PlayBloodFX()
    {
        //Debug.Log("BloodFX played");
        
        // get bool isInvulnerable and use it to create if condition for shield block effects
        if (scriptHealth.isInvulnerable == true )
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
            GameObject weapon = GameObject.Find("WeaponLogic");
            // trigger BloodFX
            var instance = Instantiate(bloodFX[Random.Range(0, bloodFX.Length)], weapon.transform.position, Quaternion.identity);

        }
        
        


    }

    




        //play weapon impact audio
        public void PlayWeaponImpactAudio()
        {
            if (scriptHealth.isInvulnerable == true)
            {
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.clip = shieldBlockSFX[Random.Range(0, shieldBlockSFX.Length)];
            audioSource.PlayOneShot(audioSource.clip);

            }
        
            else
            {
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.clip = weaponImpactSFX[Random.Range(0, weaponImpactSFX.Length)];
            audioSource.PlayOneShot(audioSource.clip);
            }
        }

    public void PlayPainAudio()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = painSFX[Random.Range(0, painSFX.Length)];
        audioSource.PlayOneShot(audioSource.clip);
    }

    public void PlayDeathAudio()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = deathSFX[Random.Range(0, deathSFX.Length)];
        audioSource.PlayOneShot(audioSource.clip);
    }










}
