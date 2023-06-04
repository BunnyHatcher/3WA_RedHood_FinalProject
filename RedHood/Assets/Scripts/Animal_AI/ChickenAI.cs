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
    public float _wanderTimer = 5f;
    public float _wanderSphereMultiplier = 4f;
    public float _walkDistance = 4f;

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

        _wanderTimer = 3f;
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
                break;
            case AgentState.WALKING:
                break;
            case AgentState.EATING:
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
                break;
            case AgentState.WALKING:
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
                break;
            case AgentState.WALKING:
                break;
            case AgentState.EATING:
                break;
            default:
                break;
        }
    }

    //---------------------------| S T A T E  M A C H I N E  M E T H O D S |----------------------------------------------------------------------------------------------------------------------

    public void TransitionToState(AgentState ToState)
    {
        OnStateExit();
        _currentState = ToState;
        OnStateEnter();

    }

    public void ChickenWanderMethod()

     {// when chicken has wandered long enough, it will pick a new target
         if (_timer >= _wanderTimer)
            {
                _animator.SetBool("isWandering", true);

                Vector3 wanderDirection = (Random.insideUnitSphere * _wanderSphereMultiplier) + transform.position;

                // We need to make sure the AI only walks to a point on the NavMesh
                NavMeshHit navMeshHit;
                NavMesh.SamplePosition(wanderDirection, out navMeshHit, _walkDistance, NavMesh.AllAreas);

                // Send AI on their way
                Vector3 destination = navMeshHit.position;
                _agent.SetDestination(destination);

                // Set timer back to 0 and reshuffle the wander timer
                _timer = 0f;
                _wanderTimer = Random.Range(0.5f, 5.0f);
            }

         else
            {
                _animator.SetBool("isEating", true);
            }
     }

   
}
