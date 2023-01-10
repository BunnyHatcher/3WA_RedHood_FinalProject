using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class Target : MonoBehaviour
{
    public event Action<Target> OnDestroyed;
    //public UnityEvent targetDestroyedEvent;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
        //targetDestroyedEvent.Invoke();
    }
}
