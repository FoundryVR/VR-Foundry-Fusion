using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    private Transform vrCam;
    void Start()
    {
        vrCam = FindObjectOfType<CameraOffset>().transform;
        vrCam.transform.position = transform.position;
        vrCam.transform.rotation = transform.rotation;
        Debug.Log("set pos" +transform.position);
    }

    // Update is called once per frame
    void Update()
    {
    
    }
}
