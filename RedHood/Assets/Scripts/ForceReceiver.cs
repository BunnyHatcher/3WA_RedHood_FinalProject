using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ForceReceiver : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float drag = 0.1f;

    private Vector3 dampingVelocity;
    private Vector3 impact;
    
    private float verticalVelocity;

    public Vector3 Movement => impact + Vector3.up * verticalVelocity;

    private void Update()
    {

    
     // ------------GRAVITY-----------------------------------------------------------------------
     
        // If we are not falling and are standing on the ground...
        if(verticalVelocity < 0f && controller.isGrounded)
        {
            verticalVelocity = Physics.gravity.y * Time.deltaTime;

        }

        else
        {
            // simulates acceleration of falling speed when falling
            verticalVelocity += Physics.gravity.y * Time.deltaTime;

        }


        // calculates the slowing down of force over time
        impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dampingVelocity, drag);

        // when impact force has played out, reactivate NavMesh Agent system
        if (agent != null)
        {       // old method                 // new method
            if /*(impact == Vector3.zero)*/ (impact.sqrMagnitude < 0.2f * 0.2f) // use square magnitude for more performance-friendly calculation
            {
                impact = Vector3.zero; // with the new approach, we set impact to zero within the if statement 
                agent.enabled = true; // ... then reenable the agent
            }
        }
    }

    public void AddForce(Vector3 force)
    {

        impact += force;
        
        // if script is used for AI --> disable NavMesh Agent so it doesn't interfere with knockback effect
        if(agent != null)
        {
            agent.enabled = false;
        }
    }

    public void Jump(float jumpForce)
    {
        verticalVelocity += jumpForce;

    }


}
