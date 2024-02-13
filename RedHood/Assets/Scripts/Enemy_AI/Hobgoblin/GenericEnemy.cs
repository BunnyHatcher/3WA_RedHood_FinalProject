using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class GenericEnemy : MonoBehaviour
{
    private AIStateMachine brain;
    private Animator animator;
    [SerializeField]
    private TMP_Text stateNote;
    private NavMeshAgent navAgent;
    private GameObject player;
    private Rigidbody aiRigidbody;

    #region Bools & Parameters
    public bool isPerformingAction = false;
    public float distanceFromTarget = Mathf.Infinity;

    //Locomotion
    public float rotationSpeed = 15f;
    //public float moveSpeed = 3f; //might not be necessary, we can try and use the move Speed from the NavAgent, no?

    // Idle
    bool playerIsNear;

    // Chasing
    public float endChaseDistance = 10f;
    // Suspicion
    public float timeSinceLastSawPlayer = Mathf.Infinity;
    protected float _suspicionTime = 3f;
    // Attack
    public float stoppingDistance = 1f;
    #endregion


    //-----INITIALIZATIONS-------------------------------
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        animator = transform.GetComponent<Animator>();
        brain = GetComponent<AIStateMachine>();
        aiRigidbody = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();

        playerIsNear = false;
        brain.PushState(Idle, OnIdleEnter, OnIdleExit);
    }

    // Update is called once per frame
    void Update()
    {
        playerIsNear = Vector3.Distance(transform.position, player.transform.position) < 5;
    }

    //-----S T A T E S-------------------------------    
    #region IDLE

    // IDLE STATE    
     void OnIdleEnter()
         {
             stateNote.text = "Idle";
             navAgent.ResetPath();
         }
     void Idle()
     {
        // Calculate distance from target
        distanceFromTarget = Vector3.Distance(transform.position, player.transform.position);
        playerIsNear = distanceFromTarget < 5;

        if (playerIsNear)
         {
            brain.PushState(Chase, OnChaseEnter, OnChaseExit);
         }
         else if (distanceFromTarget <= navAgent.stoppingDistance)
         {
            brain.PushState(Attack, OnEnterAttack, null);
            
         }        
     }

     void OnIdleExit()
     {
        animator.SetBool("Idle", false);
     }

     

    #endregion

    #region CHASE

    // CHASE STATE
    void OnChaseEnter()
    {
        stateNote.text = "Chase";
        animator.SetBool("Chase", true);
    }

    void Chase()
    {
        // Safety Check to make sure, that Actions interrupt locomotion
        if (isPerformingAction)
            return;

        // AI chases player's position
        navAgent.SetDestination(player.transform.position);

        // Calculate distance from target
        distanceFromTarget = Vector3.Distance(transform.position, player.transform.position);

        if (distanceFromTarget > endChaseDistance)
        {
            brain.PopState();
            brain.PushState(Idle, OnIdleEnter, OnIdleExit);
        }

        if (distanceFromTarget <= navAgent.stoppingDistance)
        {
            timeSinceLastSawPlayer = 0;
            brain.PushState(Attack, OnEnterAttack, null);
        }


        timeSinceLastSawPlayer += Time.deltaTime;
    }

    void OnChaseExit()
    {
        animator.SetBool("Chase", false);
    }

    #endregion

    #region ATTACK

    // ATTACK STATE
    void OnEnterAttack()
    {
        navAgent.ResetPath();
        stateNote.text = "Attack";
    }
    void Attack()
    {
        
    }

    #endregion




    #region Methods
    public void HandleMoveToTarget()
    {
        Vector3 targetDirection = player.transform.position - transform.position;
        distanceFromTarget = Vector3.Distance(player.transform.position, transform.position);
        float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

        // If we perform an action, stop movement
        if (isPerformingAction)
        {
            animator.SetFloat("velocityX", 0, 0.1f, Time.deltaTime);
            navAgent.enabled = false;
        }
        // otherwise...
        else
        {
            // we move, when we are not in attack range
            if (distanceFromTarget > stoppingDistance /*or: _navAgent.stoppingDistance */)
            {
                animator.SetFloat("velocityX", 1, 0.1f, Time.deltaTime);
            }
            // or we stand still when we are in attack range
            else if (distanceFromTarget <= stoppingDistance)
            {
                animator.SetFloat("velocityX", 0, 0.1f, Time.deltaTime);
            }
        }

        HandleRotateTowardsTarget();
        navAgent.transform.localPosition = Vector3.zero;
        navAgent.transform.localRotation = Quaternion.identity;
    }

    public void HandleRotateTowardsTarget()
    {
        // Rotate manually
        if (isPerformingAction)
        {
            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0;
            direction.Normalize();

            if (direction == Vector3.zero)
            {
                direction = transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed / Time.deltaTime);
        }
        //Rotate with Pathfinding
        else
        {
            Vector3 relativeDirection = transform.InverseTransformDirection(navAgent.desiredVelocity);
            Vector3 targetVelocity = aiRigidbody.velocity;

            navAgent.enabled = true;
            navAgent.SetDestination(player.transform.position);
            aiRigidbody.velocity = targetVelocity;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                navAgent.transform.rotation, rotationSpeed / Time.deltaTime);
        }

        navAgent.transform.localPosition = Vector3.zero;
        navAgent.transform.localRotation = Quaternion.identity;
    }
    #endregion
}
