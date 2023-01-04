using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MoveAgent : MonoBehaviour
{
    #region References to other Classes
    public Transform _target;

    private NavMeshAgent _navAgent;    
    private VisionCone _visionCone;
    private BaseState _baseState;
    private Rigidbody _enemyRigidbody;

    //private StateMachine _brain;
    //private Animator _animator;
    //private Text _stateNote;
    //private PlayerStateMachine _playerStateMachine;    
    #endregion

    #region Patrol Behaviour
    //Patrol Behaviour
    public Transform[] _points;
    private int _destPoint = 0;
    private bool _goingForward =  true;
    [SerializeField] private bool _backAndForth = false;
    #endregion

    #region Bools & Parameters
    //Locomotion
    public float _rotationSpeed = 15f;
    public float _moveSpeed = 3f;
    #endregion


    private void Awake()
    {
        //_animator = transform.GetChild(0).GetComponent<Animator>();
        //_brain = GetComponent<StateMachine>();
        //_playerStateMachine = FindObjectOfType<PlayerStateMachine>();
        
        _navAgent = GetComponent<NavMeshAgent>();
        _visionCone = GetComponentInChildren<VisionCone>();
        _baseState = FindObjectOfType<BaseState>();
        _enemyRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        //_navAgent.enabled = false;
        //_enemyRigidbody.isKinematic = false;

        _navAgent.autoBraking = false;

        // Start Patrol:
        GotoNextPoint();

    }


    private void Update()
    {
        //PatrolMovement();

    }

    private void FixedUpdate()
    {
        
    }


    #region Patrolling
    public void PatrolMovement()
    {
        if (_backAndForth)
        {
            if (_goingForward)
            { GotoNextPoint(); }

            else
            { GoToPreviousPoint(); }
        }

        else
        {

            GotoNextPoint();

        }
    }

    private void GotoNextPoint()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (!_navAgent.pathPending && _navAgent.remainingDistance < 0.5f)

        
        { // Returns if no points have been set up
            if (_points.Length == 0)
                return;

            _destPoint++;

            if(_destPoint >= _points.Length)
            {
                if(_backAndForth)
                {
                    _goingForward = false;
                    _destPoint = _points.Length - 1;
                }

                else
                {
                    _destPoint = 0;
                }
            }

            
            
            // Set the agent to go to the currently selected destination.
            _navAgent.destination = _points[_destPoint].position;

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            //_destPoint = (_destPoint + 1) % _points.Length;
        }
    }

    private void GoToPreviousPoint()
    {
        if (!_navAgent.pathPending && _navAgent.remainingDistance < 0.5f)
        {
            _destPoint--;

            if(_destPoint < 0)
            {
               if(_backAndForth)
                {
                    _goingForward = true;
                    _destPoint = 0;
                }
                
                else
                {
                 _destPoint = _points.Length - 1;
                }
            }

            _navAgent.destination = _points[_destPoint].position;

        }


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        for (int i = 0; i < _points.Length - 1; i++)
        {
            if (i == _points.Length) // at the last waypoint
            {
              Gizmos.DrawLine(_points[i].position, _points[0].position);
             
            }

            else
            {
              Gizmos.DrawLine(_points[i].position, _points[i + 1].position);
            }

        }
    }

    #endregion

    #region Targeting
    /*
    public void HandleMoveToTarget()
    {
        Vector3 targetDirection = _target.position - transform.position;
        float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

        // If we perform an action, stop movement
        if (_baseState._isPerformingAction)
        { 
            
        }

    }
    */
    #endregion















}
