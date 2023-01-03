using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine stateMachine;

    public EnemyBaseState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    //method to calculate involuntary movement like knockback, falling etc.
    protected void Move(float deltaTime)
    {
        Move(Vector3.zero, deltaTime);

    }

    //method to calculate active movement
    protected void Move(Vector3 motion, float deltaTime)
    {
        stateMachine.EnemyController.Move((motion + stateMachine.ForceReceiver.Movement) * deltaTime);

    }

    protected void FacePlayer()
    {

        if (stateMachine.Player == null) { return; }//checking if player available as target

        Vector3 lookPos = stateMachine.Player.transform.position - stateMachine.transform.position;
        
        lookPos.y = 0f;// We don't want any movement on the y axis

        //We still need to implement the rotation: turn vector into a Quaternion
        stateMachine.transform.rotation = Quaternion.LookRotation(lookPos);

    }

    protected bool IsInChaseRange()
    {

        if(stateMachine.Player.IsDead) { return false; } // to make sure enemy stops chasing or attacking when player is already dead
        
        // old Method:
        /*Vector3 toPlayer = stateMachine.Player.transform.position - stateMachine.transform.position;
           
        return toPlayer.magnitude <= stateMachine.PlayerChasingRange;*/

        //new Method:

        float playerDistanceSqr = (stateMachine.Player.transform.position - stateMachine.transform.position).sqrMagnitude; //a simple Vector3 would work as well, but for optimization it's better to use the square magnitude

        return playerDistanceSqr <= stateMachine.PlayerChasingRange * stateMachine.PlayerChasingRange; // because we work with squared, we need to multiply the chasing range with itself


    }

    
}
