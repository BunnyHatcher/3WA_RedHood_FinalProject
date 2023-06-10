using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class CinematicControlRemover : MonoBehaviour
{
    private GameObject player;
    private PlayerInput input;
    private Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        input = player.GetComponent<PlayerInput>();
        anim = player.GetComponent<Animator>();

        GetComponent<PlayableDirector>().played += DisableControl;
        GetComponent<PlayableDirector>().stopped += EnableControl;

        void DisableControl(PlayableDirector playableDirector)
        {
            //Disable Movement
            player.GetComponent<RH_PlayerMovement>().enabled = false;
            player.GetComponent<Rigidbody>().velocity = Vector3.zero; // make character stand still
            
            //Disable Input
            input.enabled = false;

            //Play Animation
            anim.SetBool("cutsceneAnimation", true);
            
            
        }

        void EnableControl(PlayableDirector playableDirector)
        {
            //Play Animation
            anim.SetBool("cutsceneAnimation", false);

            //Reenable Movement
            player.GetComponent<RH_PlayerMovement>().enabled = true;

            //Reenable Input
            input.enabled = true;
        }
    }

    
}
