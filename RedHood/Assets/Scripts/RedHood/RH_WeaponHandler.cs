using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RH_WeaponHandler : MonoBehaviour
{
    //WeaponLogic
    [SerializeField] private GameObject rightWeaponLogic;
    [SerializeField] private GameObject leftWeaponLogic;
    //WeaponTrails
    [SerializeField] private GameObject rightWeaponTrail;
    [SerializeField] private GameObject leftWeaponTrail;


    public void EnableRightWeapon()
    {

        rightWeaponLogic.SetActive(true);
        rightWeaponTrail.SetActive(true);
        //weaponLogic.GetComponentInChildren<GameObject>().SetActive(true);

    }

    public void EnableLeftWeapon()
    {

        leftWeaponLogic.SetActive(true);
        leftWeaponTrail.SetActive(true);
        //weaponLogic.GetComponentInChildren<GameObject>().SetActive(true);

    }

    public void DisableRightWeapon()
    {

        rightWeaponLogic.SetActive(false);
        rightWeaponTrail.SetActive(false);
        //weaponLogic.GetComponentInChildren<GameObject>().SetActive(false);

    }

    public void DisableLeftWeapon()
    {

        leftWeaponLogic.SetActive(false);
        leftWeaponTrail.SetActive(false);
        //weaponLogic.GetComponentInChildren<GameObject>().SetActive(false);

    }


}
