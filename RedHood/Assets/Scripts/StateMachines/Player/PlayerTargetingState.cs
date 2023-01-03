using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetingState : PlayerBaseState
{
    
    
    // Declaring Targetting BlendTree with a hash
    private readonly int TargetingBlendTreeHash = Animator.StringToHash("TargetingBlendTree"); //Hash to be used in BlendTree Animation method

    private readonly int TargetingForwardHash = Animator.StringToHash("ForwardTargeting"); //Hash to be used in BlendTree Animation method

    private readonly int TargetingRightHash = Animator.StringToHash("RightTargeting"); //Hash to be used in BlendTree Animation method

    private const float CrossFadeDuration = 0.1f;

    
    public PlayerTargetingState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        //subscribe OnTarget method to TargetEvent of InputReader
        stateMachine.InputReader.TargetEvent += OnTarget;

        //Dodge
        base.Enter(); // call OnDodge Enter from the PlayerBaseState

        //initiate Jump
        stateMachine.InputReader.JumpEvent += OnJump;

        //Play BlendTree Animation via Hash
        stateMachine.Animator.CrossFadeInFixedTime(TargetingBlendTreeHash, CrossFadeDuration);//use crossFade to make transition smoother
    }


    public override void Tick(float deltaTime)
    {
        Debug.Log(stateMachine.Targeter.CurrentTarget.name);
        
        if(stateMachine.InputReader.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackingState(stateMachine, 0));
            return;
        }
        
        
        //BLOCKING
        if(stateMachine.InputReader.IsBlocking)
        {

            stateMachine.SwitchState(new PlayerBlockingState(stateMachine));
            return;

        }
        
        //TARGETING
        //when no more target is in reach, we switch back to the FreeLook State
        if(stateMachine.Targeter.CurrentTarget == null)
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            return;
        }

        //MOVEMENT
        Vector3 movement = CalculateMovement(deltaTime); // create new vector3 for movement
        Move(movement * stateMachine.TargetingMovementSpeed, deltaTime);//then multiply this vector3 with the Movement Speed value set up in the PlayerStateMachine

        UpdateAnimator(deltaTime);

        FaceTarget(); // implemented method from PlayerBaseState
    }

    private void UpdateAnimator(float deltaTime)
    {
        
        //FORWARD MOVEMENT

        //idle state
        if (stateMachine.InputReader.MovementValue.y == 0)
        {
            //idle: player is not moving, hence the float is 0
            stateMachine.Animator.SetFloat(TargetingForwardHash, 0, 0.1f, deltaTime); //add damping time (smoothing) and deltaTime
        }

        //forward movement from input
        else
        {
            
            float value = stateMachine.InputReader.MovementValue.y > 0 ? 1f : -1f; // short method doing a boolean check: is y > 0 or not? 
                                                                                  //If yes (=true)--> use first value (1f) if not, use second value (-1f)
            
            stateMachine.Animator.SetFloat(TargetingForwardHash, value, 0.1f, deltaTime); // trigger forward/backward animation
        }

        //LATERAL MOVEMENT

        //idle state
        if (stateMachine.InputReader.MovementValue.x == 0)
        {
            stateMachine.Animator.SetFloat(TargetingRightHash, 0, 0.1f, deltaTime); //because player not moving 
        }

        //movement from input
        else
        {

            float value = stateMachine.InputReader.MovementValue.x > 0 ? 1f : -1f; // short method doing a boolean check: is x > 0 or not? 
                                                                                   //If yes, go right, if not, go left

            stateMachine.Animator.SetFloat(TargetingRightHash, value, 0.1f, deltaTime); // trigger right/left animation
        }
    }

    public override void Exit()
    {
        stateMachine.InputReader.TargetEvent -= OnTarget;

        base.Exit(); // call OnDodge Exit from the PlayerBaseState

        stateMachine.InputReader.JumpEvent -= OnJump;
    }

    private void OnTarget()
    {
        stateMachine.Targeter.Cancel();
        stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
    }

    private void OnDodge()
    {
        stateMachine.SwitchState(new PlayerDodgingState(stateMachine, stateMachine.InputReader.MovementValue));

    }

    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpingState(stateMachine));

    }

    public override Vector3 CalculateMovement(float deltaTime) // maybe set back to protected?
    {
        Vector3 movement = new Vector3();
        if (remainingDodgeTime > 0f)
        {
            movement += CalculateDodge(deltaTime);
        }

        else
        {
            // to calculate laterate movement, multiply right transform with the Input for movement on x-axis (left or right arrow) and add it to the movement vector
            movement += stateMachine.transform.right * stateMachine.InputReader.MovementValue.x;
            movement += stateMachine.transform.forward * stateMachine.InputReader.MovementValue.y; // do same thing with forward movement and the y axis
        }

        return movement;
    
    }

    protected override Vector3 CalculateDodge(float deltaTime)
    {
        Vector3 movement = new Vector3();// creates an instance of a vector 3 type structure named movement

        // movement with dodge
        
                                // Direction of Dodge                                       // Distance of Dodge
        movement += stateMachine.transform.right * dodgingDirectionInput.x * stateMachine.DodgeLength / stateMachine.DodgeDuration; // if we don't divide, the whole dodge distance would be executed in one frame
        movement += stateMachine.transform.forward * dodgingDirectionInput.y * stateMachine.DodgeLength / stateMachine.DodgeDuration;// same thing as above, only for forward

        //remainingDodgeTime = Mathf.Max(remainingDodgeTime - deltaTime, 0f); // sets value to 0, if the calculation would go negative


        return movement;
    }
}
