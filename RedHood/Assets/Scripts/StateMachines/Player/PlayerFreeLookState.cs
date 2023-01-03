using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{


    private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed");

    private readonly int FreeLookBlendTreeHash = Animator.StringToHash("FreeLookBlendTree"); //Hash to be used in BlendTree Animation method


    private const float AnimatorDampTime = 0.1f;

    private const float CrossFadeDuration = 0.1f;

    public PlayerFreeLookState(PlayerStateMachine stateMachine) : base(stateMachine){ }

    public override void Enter()
    {
        //Targeting
        stateMachine.InputReader.TargetEvent += OnTarget;

        //Dodge
        base.Enter(); // call OnDodge Enter from the PlayerBaseState

        //Jumping
        stateMachine.InputReader.JumpEvent += OnJump;

        //Play BlendTree Animation via Hash
        stateMachine.Animator.CrossFadeInFixedTime(FreeLookBlendTreeHash, CrossFadeDuration);

    }

    //----------------------------------------------------------------------------------------------------------------------

    public override void Tick(float deltaTime)
    {
        
        // for attack
        if (stateMachine.InputReader.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackingState(stateMachine, 0));
            return;
        }


        //Movement Events
        Vector3 movement = CalculateMovement(deltaTime);
        this.currentMovement = movement;

        Move(movement * stateMachine.FreeLookMovementSpeed, deltaTime); //new method that calls movement method from the PlayerBaseState
        
        
        //old method: stateMachine.CharacterController.Move(movement * stateMachine.FreeLookMovementSpeed * deltaTime);

        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0, AnimatorDampTime, deltaTime);
            return;
        }

        //Moving into direction the player is facing
        stateMachine.Animator.SetFloat(FreeLookSpeedHash, 1, AnimatorDampTime, deltaTime);
        FaceMoveDirection(movement, deltaTime);

    }

    //----------------------------------------------------------------------------------------------------------------------

    public override void Exit()
    {
        // stop targeting
        stateMachine.InputReader.TargetEvent -= OnTarget;

        //Dodge
        base.Exit(); // call OnDodge Exit from the PlayerBaseState

        //stop jumping
        stateMachine.InputReader.JumpEvent -= OnJump;
    }


    //----------------------------------------------------------------------------------------------------------------------

    private void OnTarget()
    {
        if(!stateMachine.Targeter.SelectTarget()) { return; }
        //switch target state
        stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
    }

    private void OnDodge()
    {
        stateMachine.SwitchState(new PlayerDodgingState(stateMachine, stateMachine.InputReader.MovementValue));

    }

    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpingState(stateMachine));

    }


    //----------------------------------------------------------------------------------------------------------------------
    
    
    //Calculating the movement method
    public override Vector3 CalculateMovement(float deltaTime)
    {
        Vector3 movement = new Vector3();
        if (remainingDodgeTime > 0f)
        {
            movement += CalculateDodge(deltaTime);

        }

        else
        {
            Vector3 forward = stateMachine.MainCameraTransform.forward;
            Vector3 right = stateMachine.MainCameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            movement = forward * stateMachine.InputReader.MovementValue.y +
                       right * stateMachine.InputReader.MovementValue.x;

            
        }

        return movement;

    }

    //method to make player turn into direction they are facing
    private void FaceMoveDirection(Vector3 movement, float deltaTime) 
    {
        stateMachine.transform.rotation = Quaternion.Lerp(
            stateMachine.transform.rotation,
            Quaternion.LookRotation(movement),
            deltaTime * stateMachine.RotationDamping);
    }


    protected override Vector3 CalculateDodge(float deltaTime)
    {
        Vector3 movement = new Vector3();// creates an instance of a vector 3 type structure named movement

        // movement with dodge

        // Direction of Dodge                                       // Distance of Dodge
        movement += stateMachine.transform.right * dodgingDirectionInput.x * stateMachine.DodgeLength / stateMachine.DodgeDuration; // if we don't divide, the whole dodge distance would be executed in one frame
        movement += stateMachine.transform.forward * dodgingDirectionInput.y * stateMachine.DodgeLength / stateMachine.DodgeDuration;// same thing as above, only for forward

        remainingDodgeTime = Mathf.Max(remainingDodgeTime - deltaTime, 0f); // sets value to 0, if the calculation would go negative


        return movement;
    }

}
