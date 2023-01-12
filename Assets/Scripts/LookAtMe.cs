using System;
using UnityEngine;

public class LookAtMe : MonoBehaviour
{
     //  public static LookAtMe instance;
     //[SerializeField]
    
    void Awake()
    {
    //   instance = this;
    }

    private void Update()
    {
        transform.LookAt(Manager.lookAtPlayer);
    }
}
