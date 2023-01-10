using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseState
{
    public EnemyAttackAction[] _enemyAttacks;
    public EnemyAttackAction _currentAttack;
    public float _currentRecoveryTime = 0;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _stateNote.text = "Attacking";
        _navAgent.ResetPath();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HandleCurrentAction();
        //AttackTarget();
        HandleRecoveryTimer();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _FSM.SetBool("ATTACKING", false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}


    private void HandleCurrentAction()
    {
        _distanceFromTarget = Vector3.Distance(_player.transform.position, _enemy.transform.position);
        if (_distanceFromTarget > _stoppingDistance)
        {
            //HandleMoveToTarget();
            _FSM.SetBool("CHASING", true);
        }
        else if (_distanceFromTarget <= _stoppingDistance)
        {
            AttackTarget();
        }
    }

    #region Attacks
    private void AttackTarget()
    {
        if (_isPerformingAction)
            return;
       
        if(_currentAttack == null)
        {
            GetNewAttack();
        }
        else
        {
             _isPerformingAction = true;
            _currentRecoveryTime = _currentAttack._recoveryTime; // _currentAttack = EnemyAttackAction
            _enemyAnimations.PlayTargetAnimation(_currentAttack._actionAnimation, true);
            _currentAttack = null;
        }
    }
    private void GetNewAttack()
    {
        Vector3 targetsDirection = _player.transform.position - _enemy.transform.position;
        float viewableAngle = Vector3.Angle(targetsDirection, _enemy.transform.forward);
        _distanceFromTarget = Vector3.Distance(_enemy.transform.position, _player.transform.position);

        int maxScore = 0;

        for (int i = 0; i < _enemyAttacks.Length; i++)
        {
            EnemyAttackAction enemyAttackAction = _enemyAttacks[i];

            if(_distanceFromTarget <= enemyAttackAction.maxAttackDistance
                && _distanceFromTarget >= enemyAttackAction.minAttackDistance)
            {
                if(viewableAngle <= enemyAttackAction.maxAttackAngle
                    && viewableAngle >= enemyAttackAction.minAttackAngle)
                {
                    maxScore += enemyAttackAction._attackScore;
                }
            }
        }

        int randomValue = Random.Range(0, maxScore);
        int temporaryScore = 0;

        for (int i = 0; i < _enemyAttacks.Length; i++)
        {
            EnemyAttackAction enemyAttackAction = _enemyAttacks[i];

            if (_distanceFromTarget <= enemyAttackAction.maxAttackDistance
                && _distanceFromTarget >= enemyAttackAction.minAttackDistance)
            {
                if (viewableAngle <= enemyAttackAction.maxAttackAngle
                    && viewableAngle >= enemyAttackAction.minAttackAngle)
                {
                    if (_currentAttack != null)
                        return;

                    temporaryScore += enemyAttackAction._attackScore;

                    if (temporaryScore > randomValue)
                    {
                        _currentAttack = enemyAttackAction;
                    }

                }
            }
        }
    }
    private void HandleRecoveryTimer()
    {
        if (_currentRecoveryTime > 0)
        {
            _currentRecoveryTime -= Time.deltaTime;
        }

        if (_isPerformingAction)
        {
            if (_currentRecoveryTime <= 0)
            {
                _isPerformingAction = false;
            }
        }
    }
    #endregion


}
