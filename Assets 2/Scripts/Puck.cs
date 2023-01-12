using System;
using System.Collections;
using UnityEngine;

public class Puck : MonoBehaviour
{
     private Vector3 startPostition;
     private Rigidbody rb;
     private bool hasBeenHitBefore = false;
       [SerializeField] private GameObject circle;
       //[SerializeField] private Transform lookAtPlayer;
       [SerializeField] private float trailCircleIntervalFactor = 0.3f;
       private bool rippleOnStartup = false;
     
    void Awake()
    {
        startPostition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rippleOnStartup)
        {
            StartCoroutine(CreateCircleTrail());
        }
        //   instance = this;
    }
    

    private void Update()
    {
        //rb.velocity = (Input.GetAxis("Horizontal") * Manager.lookAtPlayer.right + Input.GetAxis("Vertical") *  Manager.lookAtPlayer.forward).normalized*speed;
    }

    private void OnTriggerEnter(Collider other)
    {
  //      Debug.Log("HI");
        if (other.CompareTag("GoalRed"))
        {
            Manager.instance.ScoredGoal(false);
            ResetPuck();
            
        }else if (other.CompareTag("GoalBlue"))
        {
            Manager.instance.ScoredGoal(true);
            ResetPuck();
        }
        if(other.gameObject.layer == 7)
        {
            rb.velocity = Vector3.Reflect(rb.velocity, other.transform.right);
        }
        /*
        if (other.CompareTag("Striker"))
        {
            Vector3 force = Vector3.zero;
            Rigidbody ORB = other.GetComponent<Rigidbody>();
            if(ORB.velocity.magnitude > rb.velocity.magnitude)
            {
                force = (transform.position - other.transform.position).normalized * (ORB.velocity.magnitude / 1.2f);
            }
            if(rb.velocity.magnitude > ORB.velocity.magnitude)
            {
                force = Vector3.Reflect(rb.velocity / 1.4f,(other.transform.position - transform.position).normalized);
            } 
            rb.AddForce(force, ForceMode.Impulse);
        }
        */
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Striker"))
        {
            rb.AddForce((transform.position - other.transform.position) * 5f);
        }
    }
    private void ResetPuck()
    {
        transform.position = startPostition;
        rb.velocity = Vector3.zero;
    }

    private IEnumerator CreateCircleTrail()
    {
        while (true)
        {
            yield return new WaitForSeconds(1/(rb.velocity.magnitude*trailCircleIntervalFactor+trailCircleIntervalFactor));
            //Instantiate(circle, transform.position, Quaternion.identity);
            var g =  Instantiate(circle,transform.position,Quaternion.identity);
            g.transform.parent = transform.parent;
            g.name = "puckCircleAnimation";
        }
    }

     private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Striker"))
        {
         //  var g =  Instantiate(circle,transform.position,Quaternion.identity);
          // g.transform.parent = transform.parent;
         //  g.name = "puckCircleAnimation";
          // g.transform.LookAt(Manager.lookAtPlayer);
           if (hasBeenHitBefore == false && rippleOnStartup == false)
           {
               hasBeenHitBefore = true;
               StartCoroutine(CreateCircleTrail());
           }
        }
        if (collision.gameObject.CompareTag("Striker"))
        {
            Vector3 force = Vector3.zero;
            Rigidbody ORB = collision.gameObject.GetComponent<Rigidbody>();
            if (ORB.velocity.magnitude > rb.velocity.magnitude)
            {
                force = (transform.position - collision.transform.position).normalized * (ORB.velocity.magnitude / 1.2f);
            }
            if (rb.velocity.magnitude > ORB.velocity.magnitude)
            {
                force = Vector3.Reflect(rb.velocity / 1, (collision.transform.position - transform.position).normalized);
            }
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

}
