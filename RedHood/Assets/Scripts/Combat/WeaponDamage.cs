using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;


public class WeaponDamage : MonoBehaviour
{
    #region References

    [SerializeField]
    private Collider myCollider; //get Player's collider    
    private List<Collider> alreadyCollidedWith = new List<Collider>();// List where we store all the objects our weapon collided with
    [HideInInspector]
    public GameObject Player;    
    public UnityEvent weaponHitEvent;
    //public GameObject bloodFX;

    #endregion

    #region Bools & Ints
    [SerializeField]
    private int damage;
    private float knockback;

    #endregion

    


    private void Awake()
    {
        //Player = GameObject.Find("Player");
        Player = GameObject.Find("RH_V3");
    }


    #region Methods

    private void OnEnable()
    {
        //everytime the script gets enabled to deal damage,
        //the list gets reset:
        
        alreadyCollidedWith.Clear();//clears list
    }
        
    private void OnTriggerEnter(Collider other)
    {
        if (other == myCollider) { return; } // to prevent player from hurting himself with his own weapon

        if (alreadyCollidedWith.Contains(other)){ return; }// if the object our weapon collides with is in the list already, do nothing...
        alreadyCollidedWith.Add(other);// ... otherwise: add object to list

        if(other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damage);

            #region Raycast Hit (Not Used)
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

            #endregion

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

    #endregion

}
