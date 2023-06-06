using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AgentState
{
    IDLE,
    WALKING,
    EATING,
}

public class ChickenAI : MonoBehaviour
{
    #region DECLARATIONS

    public float _wanderRadius = 5f;
    public float _wanderSphereMultiplier = 4f;
    public float _walkDistance = 4f;

    public float _idleTimer = 5f;
    public float _minIdleTime = 1f;
    public float _maxIdleTime = 5f;

    private Transform _target; // the chicken's current target
    private float _timer; // the amount of time the chicken has been wandering

    private Animator _animator;
    private MoveAgent _moveAgent;
    private NavMeshAgent _agent;

    private AgentState _currentState;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _moveAgent = GetComponent<MoveAgent>();
        _agent = GetComponent<NavMeshAgent>();

        // set chicken's starting position as its target
        _target = transform;
        // ... and set IDLE as starting state
        _currentState = AgentState.IDLE;

        _idleTimer = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        // Update timer
        _timer += Time.deltaTime;

        //Initiate State Updates
        OnStateUpdate();

    }



    //---------------------------| S T A T E  M A C H I N E S |----------------------

    private void OnStateEnter()
    {
        switch (_currentState)
        {
            case AgentState.IDLE:
                _animator.SetBool("isIdling", true);
                _agent.ResetPath();

                Debug.Log("Entered IDLE state");
                break;

            case AgentState.WALKING:
                _animator.SetBool("isWandering", true);
                ChickenWanderMethod();

                Debug.Log("Entered WALKING state");
                break;

            case AgentState.EATING:
                _animator.SetBool("isEating", true);
                break;
            default:
                break;
        }
    }

    private void OnStateUpdate()
    {
        switch (_currentState)
        {
            case AgentState.IDLE:
                // Update timer
                _timer += Time.deltaTime;

                // when chicken has idled long enough, it will pick a new target
                if (_timer >= _idleTimer)
                {
                    //Wait for current animation to finish playing
                    /*
                    if (!_animator || _animator.GetCurrentAnimatorStateInfo(0).normalizedTime
                    - Mathf.Floor(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime) > 0.99f)
                    */
                    //... then transition to the Walking State
                    //{
                        TransitionToState(AgentState.WALKING);
                        _animator.SetBool("isIdling", false);
                    //}                        
                }
                break;

            case AgentState.WALKING:
                if (_agent.remainingDistance <= .25f) // distance left before reaching the point they were looking for
                {
                    _agent.ResetPath();
                    TransitionToState(AgentState.IDLE);
                    _animator.SetBool("isWalking", false);
                }
                break;

            case AgentState.EATING:
                break;

            default:
                break;
        }
    }

    private void OnStateExit()
    {
        switch (_currentState)
        {
            case AgentState.IDLE:
                // On exit, set timer back to 0 and reshuffle the wander timer
                _timer = 0f;
                _idleTimer = Random.Range(_minIdleTime, _maxIdleTime);
                //...also exit Idling Animation State
                _animator.SetBool("isIdling", false);
                break;

            case AgentState.WALKING:
                _animator.SetBool("isWalking", false);
                break;

            case AgentState.EATING:
                _animator.SetBool("isEating", false);
                break;

            default:
                break;
        }
    }

    //---------------------------| M E T H O D S |----------------------------------------------------------------------------------------------------------------------

    public void TransitionToState(AgentState ToState)
    {
        OnStateExit();
        _currentState = ToState;
        OnStateEnter();

    }

    public void ChickenWanderMethod()

     {
          Vector3 wanderDirection = (Random.insideUnitSphere * _wanderSphereMultiplier) + transform.position;

          // We need to make sure the AI only walks to a point on the NavMesh
          NavMeshHit navMeshHit;
          NavMesh.SamplePosition(wanderDirection, out navMeshHit, _walkDistance, NavMesh.AllAreas);

          // Send AI on their way
          Vector3 destination = navMeshHit.position;
          _agent.SetDestination(destination);
    }

    private void FinishAnimation()
    {
        //Wait for current animation to finish playing
        if (!_animator || _animator.GetCurrentAnimatorStateInfo(0).normalizedTime
        - Mathf.Floor(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime) > 0.99f);

    } 

       


   
}
