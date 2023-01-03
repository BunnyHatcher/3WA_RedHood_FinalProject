using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{

    [SerializeField] private GameObject weaponLogic;
    [SerializeField] private GameObject weaponTrail;
    
    public void EnableWeapon()
    {

        weaponLogic.SetActive(true);
        //weaponLogic.GetComponentInChildren<GameObject>().SetActive(true);

    }

    public void DisableWeapon()
    {

        weaponLogic.SetActive(false);
        //weaponLogic.GetComponentInChildren<GameObject>().SetActive(false);

    }


}
