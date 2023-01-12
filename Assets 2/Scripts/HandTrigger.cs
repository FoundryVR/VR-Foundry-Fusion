using System;
using UnityEngine;

public class HandTrigger : MonoBehaviour
{
    [SerializeField] private bool isLeftHand;

    private void OnTriggerStay(Collider other)
    {
        if (isLeftHand && Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            Check(other,true);

        }else if (isLeftHand == false && Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            Check(other,true);
        }
        
        
        else if (isLeftHand && Input.GetKeyUp(KeyCode.JoystickButton4))
        {
            Check(other,false);
        }
        else if (isLeftHand == false && Input.GetKeyUp(KeyCode.JoystickButton5))
        {
            Check(other,false);
        }
    }

    private void Check(Collider other,bool isGrab)
    {
        if (other.CompareTag("Striker"))
        {
            if (isGrab)
            {
                other.transform.parent = transform;
            }
            else
            {
                other.transform.parent = null;
            }
           
        }
        
    }
}
