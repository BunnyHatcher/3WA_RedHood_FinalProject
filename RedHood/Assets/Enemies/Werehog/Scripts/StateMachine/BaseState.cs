using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public abstract class BaseState : StateMachineBehaviour
{
    #region References
    //private StateMachine _brain;
    protected GameObject _player;
    
    public GameObject _enemy;
    protected Rigidbody _enemyRigidbody;
    protected MoveAgent _moveAgent;
    protected NavMeshAgent _navAgent;
    protected AnimateAgent _enemyAnimations;
    protected Animator _animator;
    protected Animator _FSM;
    protected TMP_Text _stateNote;
    protected Target _target;

    //Vision Cone
    protected VisionCone _visionCone;
    #endregion

    #region Bools & Parameters
    
    public bool _isPerformingAction = false;
    public float _distanceFromTarget = Mathf.Infinity;    
    
    // Chasing
    protected float _endChaseDistance = 10f;
    
    // Suspicion
    public float _timeSinceLastSawPlayer = Mathf.Infinity;
    protected float _suspicionTime = 3f;

    // Attack
    public float _stoppingDistance = 1f;
    #endregion



    #region Awake & Start

    private void Awake()
    {
        //_player = GameObject.Find("Player");
        _player = GameObject.FindWithTag("Player");
        
        _enemy = GameObject.Find("Werehog");
        _enemyRigidbody = _enemy.GetComponent<Rigidbody>();
        _target = _enemy.GetComponent<Target>();

        // A.I.
        _navAgent = _enemy.GetComponent<NavMeshAgent>();
        _moveAgent = _enemy.GetComponent<MoveAgent>();

        // Animation
        _animator = _enemy.GetComponent<Animator>();
        _enemyAnimations = _enemy.GetComponent<AnimateAgent>();

        // State Machine
        _FSM = GameObject.Find("WerehogStateMachine").GetComponent<Animator>();
        _stateNote = _enemy.GetComponentInChildren<TMP_Text>();

        _visionCone = _enemy.GetComponentInChildren<VisionCone>();
    }

    private void Start()
    {
               

    }

    #endregion



    #region Methods

    public void HandleMoveToTarget()
    {
        Vector3 targetDirection = _player.transform.position - _enemy.transform.position;
        _distanceFromTarget = Vector3.Distance(_player.transform.position, _enemy.transform.position);
        float viewableAngle = Vector3.Angle(targetDirection, _enemy.transform.forward);

        // If we perform an action, stop movement
        if (_isPerformingAction)
        {
            _animator.SetFloat("velocityX", 0, 0.1f, Time.deltaTime);
            _navAgent.enabled = false;
        }
        // otherwise...
        else
        {
            // we move, when we are not in attack range
            if(_distanceFromTarget > _stoppingDistance /*or: _navAgent.stoppingDistance */)
            {
                _animator.SetFloat("velocityX", 1, 0.1f, Time.deltaTime);
            }
            // or we stand still when we are in attack range
            else if (_distanceFromTarget <= _stoppingDistance)
            {
                _animator.SetFloat("velocityX", 0, 0.1f, Time.deltaTime);
            }
        }

        HandleRotateTowardsTarget();
        _navAgent.transform.localPosition = Vector3.zero;
        _navAgent.transform.localRotation = Quaternion.identity;
    }

    public void HandleRotateTowardsTarget()
    {
        // Rotate manually
        if (_isPerformingAction)
        {
            Vector3 direction = _player.transform.position - _enemy.transform.position;
            direction.y = 0;
            direction.Normalize();

            if (direction == Vector3.zero)
            {
                direction = _enemy.transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, targetRotation, _moveAgent._rotationSpeed / Time.deltaTime);
        }
        //Rotate with Pathfinding
        else
        {
            Vector3 relativeDirection = _enemy.transform.InverseTransformDirection(_navAgent.desiredVelocity);
            Vector3 targetVelocity = _enemyRigidbody.velocity;

            _navAgent.enabled = true;
            _navAgent.SetDestination(_player.transform.position);
            _enemyRigidbody.velocity = targetVelocity;
            _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation,
                _navAgent.transform.rotation, _moveAgent._rotationSpeed / Time.deltaTime);
        }

        _navAgent.transform.localPosition = Vector3.zero;
        _navAgent.transform.localRotation = Quaternion.identity;
    }

    #endregion

}
