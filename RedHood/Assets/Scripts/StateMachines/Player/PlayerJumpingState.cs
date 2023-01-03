using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpingState : PlayerBaseState
{

    private readonly int JumpHash = Animator.StringToHash("JumpUp");

    private Vector3 momentum;

    private const float CrossFadeDuration = 0.1f;
    
    public PlayerJumpingState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    

    public override void Enter()
    {

        stateMachine.ForceReceiver.Jump(stateMachine.JumpForce); //vertical Jump Force

        momentum = stateMachine.CharacterController.velocity; // set momentum to velocity of Character
        momentum.y = 0f; // ignore y movement as we overwrite it anyway with our vertical Jump Force
        
        stateMachine.Animator.CrossFadeInFixedTime(JumpHash, CrossFadeDuration);
        
    }
    public override void Tick(float deltaTime)
    {
        // Jumping up
        Move(momentum, deltaTime); // use Move method from BasePlaye State and fill in the momentum we calculated in Enter as the Vector3 motion argument

        // Falling down
        if(stateMachine.CharacterController.velocity.y <= 0) // check if we are still moving up, if we stopped or are moving down, then ...
        {
            stateMachine.SwitchState(new PlayerFallingState(stateMachine)); // .. initiate falling state
            return;

        }

        FaceTarget(); // make sure that when we jump while targeting, we still face the target
    }

    public override void Exit()
    {
        
    }


    
}
