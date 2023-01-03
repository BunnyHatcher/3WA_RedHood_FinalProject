using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{


   


    // Declaring Dodging BlendTree with a hash

    private readonly int DodgeBlendTreeHash = Animator.StringToHash("DodgeBlendTree"); //Hash to be used in BlendTree Animation method

    private readonly int DodgeForwardHash = Animator.StringToHash("DodgeForward"); //Hash to be used in BlendTree Animation method

    private readonly int DodgeRightHash = Animator.StringToHash("DodgeRight"); //Hash to be used in BlendTree Animation method

    //private float remainingDodgeTime;
    //private Vector3 dodgingDirectionInput;

    private const float CrossFadeDuration = 0.1f;


    public PlayerDodgingState(PlayerStateMachine stateMachine, Vector3 dodgingDirectionInput) : base(stateMachine) 
    {
        this.dodgingDirectionInput = dodgingDirectionInput;
    
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void Enter()
    {
        remainingDodgeTime = stateMachine.DodgeDuration;

        //setting up animation
        stateMachine.Animator.SetFloat(DodgeForwardHash, dodgingDirectionInput.y);
        stateMachine.Animator.SetFloat(DodgeRightHash, dodgingDirectionInput.x);
        stateMachine.Animator.CrossFadeInFixedTime(DodgeBlendTreeHash, CrossFadeDuration);

        // setting up invulnerability when dodging
        stateMachine.Health.SetInvulnerable(true);
        
        
    }


    public override void Tick(float deltaTime)
    {
        Vector3 movement = new Vector3();

        movement += stateMachine.transform.right * dodgingDirectionInput.x * stateMachine.DodgeLength / stateMachine.DodgeDuration; // if we don't divide, the whole dodge distance would be executed in one frame
        movement += stateMachine.transform.forward * dodgingDirectionInput.y * stateMachine.DodgeLength / stateMachine.DodgeDuration;// same thing as above, only for forward

        Move(movement, deltaTime);

        FaceTarget();

        remainingDodgeTime -= deltaTime;

        if(remainingDodgeTime <= 0f)
        {
            stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
        }

    }

    public override void Exit()
    {
        // cancelling up invulnerability when dodge is over
        stateMachine.Health.SetInvulnerable(false);
    }


    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    

}


