using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChasingState : EnemyBaseState
{
    private readonly int EnemyLocomotionHash = Animator.StringToHash("Locomotion");

    private readonly int EnemySpeedHash = Animator.StringToHash("Speed");

    private const float CrossFadeDuration = 0.1f;

    private const float AnimatorDampTime = 0.1f;

    //Constructor
    public EnemyChasingState(EnemyStateMachine stateMachine) : base(stateMachine) { }


    public override void Enter()
    {
        //Play BlendTree Animation via Hash
        stateMachine.Animator.CrossFadeInFixedTime(EnemyLocomotionHash, CrossFadeDuration);
    }


    public override void Tick(float deltaTime)
    {
        // if player is not in attack range, switch to idle
        if (!IsInChaseRange()) 
        {
            stateMachine.SwitchState(new EnemyIdleState(stateMachine));
            return;
        }

        else if(IsInAttackRange())
        {

            stateMachine.SwitchState(new EnemyAttackingState(stateMachine));
            return;

        }



        MoveToPlayer(deltaTime);

        FacePlayer();

        stateMachine.Animator.SetFloat(EnemySpeedHash, 1f, AnimatorDampTime, deltaTime);

    }


    public override void Exit() //whenever enemy is not in chasing range anymore
    {
        stateMachine.Agent.ResetPath();//agent tries no longer to move towards player
        stateMachine.Agent.velocity = Vector3.zero; //agent stops

    }
    private void MoveToPlayer(float deltaTime)
    {
        //check if agent is enabled and they're on the NavMesh

        if (stateMachine.Agent.isOnNavMesh)
        {
            // move agent to where player is
            stateMachine.Agent.destination = stateMachine.Player.transform.position;

            // normalize the movement speed of the agent to prevent it from influencing our movement, we calculate it ourselves
            Move(stateMachine.Agent.desiredVelocity.normalized * stateMachine.MovementSpeed, deltaTime);
        }

        // velocity is not always same as desired velocity, there might be an obstacle in the way
        // that is why we always have to update the velocity with our actual velocity        
        stateMachine.Agent.velocity = stateMachine.EnemyController.velocity; //Agent and Character Controller need to be in sync
    }

    //could put method also into base state if we want to use it for other enemies as well

    private bool IsInAttackRange()
    {

        if(stateMachine.Player.IsDead) { return false; }

        float playerDistanceSqr = (stateMachine.Player.transform.position - stateMachine.transform.position).sqrMagnitude;

        return playerDistanceSqr <= stateMachine.AttackRange * stateMachine.AttackRange;// don't forget to square it

    }

}

