using System.Collections;
using System.Collections.Generic;
using Fusion.XR.Host.Rig;
using UnityEngine;

public class MyPositionChanger : MonoBehaviour
{
    public Transform player1StartTransform,player2StartTransform;
    // Start is called before the first frame update
    void Start()
    {
        var playerCount = FindObjectsOfType<MyPositionChanger>().Length;
            
        Debug.Log(playerCount);
        Vector3 newPosition;
        Quaternion newRotation;
                
        if (playerCount%2!=0)
        {
            newPosition = player1StartTransform.position;
            newRotation = player1StartTransform.rotation;
        }
        else
        {
            newPosition = player2StartTransform.position;
            newRotation = player2StartTransform.rotation;
        }
              
                Debug.Log(playerCount+": "+newPosition);
        if (GetComponent<NetworkRig>().hardwareRig != null)
        {
            var vrRigTransform = GetComponent<NetworkRig>().hardwareRig.transform;// networkPlayerObject.gameObject.GetComponent<HardwareRig>();
            vrRigTransform.position = newPosition;
            vrRigTransform.rotation = newRotation;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
