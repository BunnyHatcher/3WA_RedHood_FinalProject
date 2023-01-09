using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(NavMeshAgent))]
//[RequireComponent(typeof(Animator))]

public class AnimateAgent : MonoBehaviour
{
    [SerializeField]
    GameObject _enemy;
    Rigidbody _enemyRigidbody;
    Animator _anim;
    NavMeshAgent _navAgent;
    MoveAgent _moveAgent;
    public Animator _werehogStateMachine;

    Vector2 _smoothDeltaPosition = Vector2.zero;
    Vector2 _velocity = Vector2.zero;


    private void Awake()
    {
        //_enemy = GameObject.Find("Werehog");
        _anim = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _enemyRigidbody = GetComponent<Rigidbody>();
        //_werehogStateMachine = GetComponentInChildren<Animator>();
    }
    void Start()
    {
        // turn off RootMotion
        _anim.applyRootMotion = false;

        // Don’t update position automatically
        _navAgent.updatePosition = false;
    }

    void Update()
    {
        Vector3 worldDeltaPosition = _navAgent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        _smoothDeltaPosition = Vector2.Lerp(_smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            _velocity = _smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = _velocity.magnitude > 0.5f && _navAgent.remainingDistance > _navAgent.radius;

        // Update animation parameters
        _anim.SetBool("isMoving", shouldMove);
        _anim.SetFloat("velocityX", _velocity.x);
        _anim.SetFloat("velocityY", _velocity.y);

        LookAt lookAt = GetComponent<LookAt>();
        if (lookAt)
            lookAt.lookAtTargetPosition = _navAgent.steeringTarget + transform.forward;
    }

    void OnAnimatorMove()
    {
        // Update position to agent position
        transform.position = _navAgent.nextPosition;

        // the following code centers our model back on our gameobject
        //when our animator plays an animation with root motion:
        float delta = Time.deltaTime;
        _enemyRigidbody.drag = 0;
        Vector3 deltaPosition = _anim.deltaPosition;
        deltaPosition.y = 0;
        Vector3 velocity = deltaPosition / delta;
        _enemyRigidbody.velocity = velocity /* * _moveAgent._moveSpeed*/;

    }

    #region Methods

    public void PlayTargetAnimation(string targetAnim, bool isInteracting)
    {
        //_anim.applyRootMotion = isInteracting;
        _anim.SetBool("isInteracting", isInteracting);
        _anim.CrossFade(targetAnim, 0.2f);

    }

    public void HandleEnemyDamage()
    {
        _anim.SetTrigger("ImpactReceived");
        _werehogStateMachine.SetBool("IMPACT", true);
    }

    public void HandleEnemyDeath()
    {
        //_anim.SetTrigger("ImpactReceived");
        _werehogStateMachine.SetBool("DEAD", true);
    }

    #endregion

}
