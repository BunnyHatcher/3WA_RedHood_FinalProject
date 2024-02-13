using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // added to be able to use the Action keyword

public class AIState
{
    // the Delegates for our 3 FSM states 
    #region Delegates
    public Action ActiveAction; //While we do sth,
    public Action OnEnterAction; //when we start doing sth.
    public Action OnExitAction; //when we finished doing sth.
    #endregion

    // StateConstructor, which contains the methods for our 3 states
    #region StateConstructor
    public AIState(Action active, Action onEnter, Action onExit)
    {
        ActiveAction = active;
        OnEnterAction = onEnter;
        OnExitAction = onExit;
    }
    #endregion

    // The Methods for our 3 states
    #region Methods
    public void Execute()
    {
        if (ActiveAction != null)
            ActiveAction.Invoke();
    }

    public void OnEnter()
    {
        if (OnEnterAction != null)
            OnEnterAction.Invoke();
    }

    public void OnExit()
    {
        if (OnExitAction != null)
            OnExitAction.Invoke();
    }
    #endregion
}
