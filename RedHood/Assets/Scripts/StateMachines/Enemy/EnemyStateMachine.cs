using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : StateMachine
{
    
    [field: SerializeField] public Animator Animator { get; private set; }

    [field: SerializeField] public CharacterController EnemyController { get; private set; }

    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }

    [field: SerializeField] public  NavMeshAgent Agent { get; private set; }

    [field: SerializeField] public WeaponDamage Weapon { get; private set; }

    [field: SerializeField] public Health Health { get; private set; }

    [field: SerializeField] public Target Target { get; private set; }

    [field: SerializeField] public Ragdoll Ragdoll { get; private set; }

    [field: SerializeField] public float MovementSpeed { get; private set; }
    [field: SerializeField] public float PlayerChasingRange { get; private set; }

    [field: SerializeField] public float AttackRange { get; private set; }

    [field: SerializeField] public int AttackDamage { get; private set; }

    [field: SerializeField] public int AttackKnockback { get; private set; }

    
    public Health Player { get; private set; }  // in order to chase the player, the enemy needs a reference to the player to check the distance

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();

        
        // Code to disable movement created by NavMesh Agent

        Agent.updatePosition = false; //we don't want the object to be moved by the agent
        Agent.updateRotation = false; //we don't want the object to be rotated by the agent either



        SwitchState(new EnemyIdleState(this));
    }


    private void OnEnable()
    {
        Health.OnTakeDamage += HandleTakeDamage;
        Health.OnDie += HandleDie;
    }

    private void OnDisable()
    {
        Health.OnTakeDamage -= HandleTakeDamage;
        Health.OnDie -= HandleDie;
    }

    private void HandleTakeDamage()
    {
        SwitchState(new EnemyImpactState(this));
    }

    private void HandleDie()
    {
        SwitchState(new EnemyDeadState(this));
    }

    //Method for drawing chasing range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, PlayerChasingRange);
    }

}
