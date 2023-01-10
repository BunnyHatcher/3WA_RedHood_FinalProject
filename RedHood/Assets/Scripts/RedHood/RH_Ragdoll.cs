using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RH_Ragdoll : MonoBehaviour
{

    [SerializeField] private Animator animator;

    //[SerializeField] private CharacterController controller;

    [SerializeField] private Collider mainCollider;

    [SerializeField] private RH_PlayerMovement playerController;

    private Collider[] allColliders;

    private Rigidbody[] allRigidbodies;


    private void Start()
    {
        allColliders = GetComponentsInChildren<Collider>(true);
        allRigidbodies = GetComponentsInChildren<Rigidbody>(true);

        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool isRagdoll)
    {

        foreach(Collider collider in allColliders) // go through all the colliders in the allCollider array ...
        {
            if (collider.gameObject.CompareTag("Ragdoll")) // if they have the tag Ragdoll...
            {
                collider.enabled = isRagdoll;   // ... activate ragdoll for them
            }

        }

        foreach (Rigidbody rigidbody in allRigidbodies) // go through all the rigidbodies array ...
        {
            if (rigidbody.gameObject.CompareTag("Ragdoll")) // if they have the tag Ragdoll...
            {
                rigidbody.isKinematic = !isRagdoll;   // ... if it's not in ragdoll, activate kinematics
                rigidbody.useGravity = isRagdoll; // if it's ragdoll, turn on gravity to make the body drop to the floor
            }

        }

        //playerController.enabled = !isRagdoll; // when Ragdoll is enabled, we can't controll the character anymore
        animator.enabled = !isRagdoll; // when Ragdoll is enabled, the animator doesn't play anymore
        mainCollider.enabled = !isRagdoll; // turn off main collider after death
    }


}
