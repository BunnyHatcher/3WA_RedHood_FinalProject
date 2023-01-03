using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup cineTargetGroup;

    private Camera mainCamera;
    
    public List<Target> targets = new List<Target>();

    public Target CurrentTarget { get; private set; }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    #region Manage Triggers
    // Add Target to Target list, if Targeter collider overlaps with collider box of target
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Target>(out Target target))
        {
            targets.Add(target);
            
            target.OnDestroyed += RemoveTarget; // if target is destroyed: subscribe to RemoveTarget method 
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Target>(out Target target))
        {
            RemoveTarget(target); // if target is out of range: subscribe to RemoveTarget method as well
        }
    }

    #endregion

    #region Select & Remove Targets

    public bool SelectTarget() //return a boolean that determines if there is a target or not
    {
        if (targets.Count == 0) { return false; }

        
        // calculate if there is a target in sight (not just there, but in the player's view)
        // check which target is closest if you have more than one target in reach
        
        Target closestTarget = null;//declare a variable for holding the closest target (default to null)
        
        //distance from the centre of the screen to closest target
        float closestTargetDistance = Mathf.Infinity; //default to largest number possible 
        
        foreach (Target target in targets) //search through all the targets in the Target list
        {
            //... check the target's position            
            Vector2 viewPos = mainCamera.WorldToViewportPoint(target.transform.position);// if the target is indeed on the screen, viewPos will be between 0|0 and 1|1

            //old version: 
            //if  (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
            
            //new version, also more reliable
            if (!target.GetComponentInChildren<Renderer>().isVisible) // if not visible, continue 
            {
                continue;
            }

            // if visible, check how far away from the centre of the screen
            Vector2 toCenter = viewPos - new Vector2(0.5f, 0.5f); // how far away is target from centre of screen

            
            if (toCenter.sqrMagnitude < closestTargetDistance) //if it is closer than the previously selected target ...
            {
                
                closestTarget = target; // ... this target becomes the new closest target
                closestTargetDistance = toCenter.sqrMagnitude; // then set its distance to the actual value

            }
        }

        if (closestTarget == null) { return false; } // if there is no closest target, stop calculating

        CurrentTarget = closestTarget; // if yes, make it the current target
        cineTargetGroup.AddMember(CurrentTarget.transform, 1f, 2f);

        return true;
    }

    //method for cancelling targeting
    public void Cancel()
    {
        if (CurrentTarget == null) { return; }

        cineTargetGroup.RemoveMember(CurrentTarget.transform);
        CurrentTarget = null;
    }
    
    //method to remove current target from list of targets
    private void RemoveTarget(Target target)
    {
        if(CurrentTarget == target)
        {
            cineTargetGroup.RemoveMember(CurrentTarget.transform);
            CurrentTarget = null;
        }

        target.OnDestroyed -= RemoveTarget;
        targets.Remove(target);
    }

    #endregion


}
