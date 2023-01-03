using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    protected PlayerStateMachine stateMachine;

    public float remainingDodgeTime;
    protected Vector2 dodgingDirectionInput;

    //declare current movement to be able to use it later
    public Vector3 currentMovement { get; protected set; } // we want to be able to access the variable, but not to manipulate it outside its class
    


    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        // always set default values for your methods in constructor
        remainingDodgeTime = 0f;
        dodgingDirectionInput = new Vector2();
    }

    //----------------------------------------------------------------------------------------------------------------------

    public override void Enter()
    {
        stateMachine.InputReader.DodgeEvent += OnDodge;
        Debug.Log("bind input reader onDodge");

    }


    public override void Tick(float deltaTime)
    {
     
    }

    public override void Exit()
    {
        stateMachine.InputReader.DodgeEvent -= OnDodge;
        Debug.Log("unbind input reader onDodge");
    }
    //----------------------------------------------------------------------------------------------------------------------

    //method for basic movement
    public virtual Vector3 CalculateMovement(float deltaTime)
    { 
        
        return new Vector3();
    }

    //method for dodge
    protected virtual Vector3 CalculateDodge(float deltaTime)
    { return new Vector3(); } // <-- Default Constructor of Vector3!!

    private void OnDodge()
    {
        if (Time.time - stateMachine.PreviousDodgeTime < stateMachine.DodgeCooldown) { return; }

        stateMachine.SetDodgeTime(Time.time);
        dodgingDirectionInput = stateMachine.InputReader.MovementValue; // calculate the direction we want to dodge to
        remainingDodgeTime = stateMachine.DodgeDuration; //set remaining dodge time equal to duration of dodge

    }

    //----------------------------------------------------------------------------------------------------------------------


    //method to calculate movement without player input (e.g. knockback, falling etc.)
    protected void Move(float deltaTime)
    {
        Move(Vector3.zero, deltaTime);

    }

    //method to calculate active movement influenced by player input
    protected void Move(Vector3 motion, float deltaTime)
    {
        stateMachine.CharacterController.Move((motion + stateMachine.ForceReceiver.Movement) * deltaTime);

    }

    //----------------------------------------------------------------------------------------------------------------------


    protected void FaceTarget()//method to be used in Player Targeting State, but also on other classes
    {

        //when no more target is in reach, we switch back to the FreeLook State
        if (stateMachine.Targeter.CurrentTarget == null) { return; }

        //make targeting method:
        // first try: Vector3 lookPos.x = targetPosition - our.position
        Vector3 lookPos = stateMachine.Targeter.CurrentTarget.transform.position - stateMachine.transform.position;
        // player should not tilt up and down towards the target, so we don't want any movement on the y axis
        lookPos.y = 0f;

        //We still need to implement the rotation: turn vector into a Quaternion
        stateMachine.transform.rotation = Quaternion.LookRotation(lookPos);
    }


        //----------------------------------------------------------------------------------------------------------------------

    protected void ReturnToLocomotion()
        {
            if(stateMachine.Targeter.CurrentTarget != null)//if there is still a target...
            {

                stateMachine.SwitchState(new PlayerTargetingState(stateMachine));//switch to TargetingState
            }

            else
            {
                stateMachine.SwitchState(new PlayerFreeLookState(stateMachine)); //... if not, switch to Freelook

            }

        }
        

    
}
