using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

//Setting up all parameters necessary for an Attack ability
public class Attack
{
    
    //PLAYER ATTACKS & COMBOS

    // Animation
    [field: SerializeField] public string AnimationName { get; private set; }


    // set length of transition
    [field: SerializeField] public float TransitionDuration { get; private set; }

    //Can our Attack combo into another attack
    [field: SerializeField] public int ComboStateIndex { get; private set; } = -1; // by default, there is no combo, so -1 
    
    //How far do you have to be in the attack animation before you can combo to the next attack?
    [field: SerializeField] public float ComboAttackTime { get; private set; }

    // FOR KNOCKBACK AND OTHER COMBAT PHYSICS   
    [field: SerializeField] public float ForceTime { get; private set; }

    [field: SerializeField] public float Force { get; private set; }


    [field: SerializeField] public int Damage { get; private set; }

    [field: SerializeField] public float Knockback { get; private set; }
}
