using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;


public class WeaponDamage : MonoBehaviour
{
    private int damage;

    private float knockback;

    //public GameObject bloodFX;

    public GameObject Player;

    
    [SerializeField] private Collider myCollider; //get Player's collider

    // List where we store all the objects our weapon collided with
    private List<Collider> alreadyCollidedWith = new List<Collider>();

    
    //---------E V E N T S----------------------------------------------------------------------------------------

    public UnityEvent weaponHitEvent;


    //-----------------------------------------------------------------------------------------------------



    private void Awake()
    {
        Player = GameObject.Find("Player");
    }

    //everytime the script gets enabled to deal damage, the list gets reset
    private void OnEnable()
    {
        alreadyCollidedWith.Clear();//clears list
    }

    
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (other == myCollider) { return; } // to prevent player from hurting himself with his own weapon

        if (alreadyCollidedWith.Contains(other)){ return; }// if the object our weapon collides with is in the list already, do nothing

        alreadyCollidedWith.Add(other);// adds object to list

        if(other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damage);

            /*
                RaycastHit hit;

                Vector3 directionNormal = other.transform.position - transform.position;

                Vector3 contactPoint = new Vector3(0, 0, 0);

                if (Physics.Raycast(transform.position, directionNormal, out hit))
                {
                 contactPoint = hit.point;
                }


                    //trigger message
                    Player.SendMessage("ApplyDamage", contactPoint);

            */

            // Invoke Hit Event
            weaponHitEvent.Invoke();

        }

        // trigger knockback
        if(other.TryGetComponent<ForceReceiver>(out ForceReceiver forceReceiver))
        {
            Vector3 direction = (other.transform.position - myCollider.transform.position).normalized; //calculate force direction by subtracting my own position from the position of the otehr object
            forceReceiver.AddForce(direction * knockback); // fill in direction and multiply it with knockback force

        }
    }

    public void SetAttack (int damage, float knockback)
    {

        this.damage = damage;
        this.knockback = knockback;
        
    }

}
