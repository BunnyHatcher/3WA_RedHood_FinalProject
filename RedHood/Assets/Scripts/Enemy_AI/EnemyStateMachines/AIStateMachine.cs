using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachine : MonoBehaviour
{
    public Stack<AIState> States { get; set; } // use get, set to turn into a property

    private void Awake()
    {
        // Initialize a fresh new stack on Awake
        States = new Stack<AIState>();
    }

    private void Update()
    {
        // Execute current state every single frame
        if (GetCurrentState() != null) // check if not null
        {
            GetCurrentState().ActiveAction.Invoke(); // ... then invoke Active Action on it
        }
    }
    
    // Method to replace the old state on top of the stack with a new state
    public void PushState(System.Action active, System.Action onEnter, System.Action onExit)
    {
        if (GetCurrentState() != null) // check that not null
            GetCurrentState().OnExit(); //... then exit current state
        AIState state = new AIState(active, onEnter, onExit); //use State constructor to create the state and save it in the variable
        States.Push(state); //Push the new state into the States Stack
        GetCurrentState().OnEnter(); // run OnEnter method on new Current State

    }

    // Method to remove the state currently on top of the stack
    public void PopState()
    {
        if (GetCurrentState() != null) // check if there is an active state
        {
            GetCurrentState().OnExit(); // ... then exit it
            GetCurrentState().ActiveAction = null; // ... and delete currently active action from state
            States.Pop(); // ... remove state from top of stack 
            GetCurrentState().OnEnter(); // run OnEnter method for new Current State
        }
    }


    // Get the current state on top of the stack
    private AIState GetCurrentState()
    {
        // check if State count is 0,
        // if yes, use Peek function to look at states on top of stack
        // else, return null
        return States.Count > 0 ? States.Peek() : null; 
    }
}
