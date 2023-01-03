using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_AnimatorHandler : MonoBehaviour
{
    RH_InputManager _input;
    Animator _animator;
    int horizontal;
    int vertical;

    public float _moveSpeed;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
        //Convert int strings to Animator Values
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");        
    }

    
    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement)
    {
        _animator.SetFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
        _animator.SetFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
    }
}
