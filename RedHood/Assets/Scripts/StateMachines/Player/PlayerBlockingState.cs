using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlockingState : PlayerBaseState
{
    public PlayerBlockingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    private readonly int BlockHash = Animator.StringToHash("BlockIdle");

    private const float CrossFadeDuration = 0.1f;


    public override void Enter()
    {
        stateMachine.Health.SetInvulnerable(true);
        stateMachine.Animator.CrossFadeInFixedTime(BlockHash, CrossFadeDuration);

    }
    public override void Tick(float deltaTime)
    {
        Move(deltaTime); // enforce gravity

        if (!stateMachine.InputReader.IsBlocking) // if we're not blocking...
        {
            stateMachine.SwitchState(new PlayerTargetingState(stateMachine)); // ... we go back into the targeting state
            return;
        }
        
        if (stateMachine.Targeter.CurrentTarget == null)
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            return;
        }
    }

    public override void Exit()
    {
        stateMachine.Health.SetInvulnerable(false);
    }

}
