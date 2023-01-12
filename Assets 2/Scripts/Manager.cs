using System;
using UnityEngine;
using TMPro;

public class Manager : MonoBehaviour
{
     public static Manager instance;
   //  [SerializeField] private SphereCollider handLeftCollider,handRightCollider;
     //[SerializeField] private Transform testStriker;
     [SerializeField] private TextMeshProUGUI scoreText;
     [SerializeField] private int winningScore = 8;
     [SerializeField] private Transform lookAtPlayerTransform;
     public static Transform lookAtPlayer;
   //  private string scoreTextStructure;
     private int redScore, blueScore;

    // private Camera camera;
    
    void Awake()
    {
        lookAtPlayer = lookAtPlayerTransform;
        //camera = FindObjectOfType<Camera>();
    //    scoreTextStructure = scoreText.text;
        DisplayScore();
           instance = this;
    }
    //
    // private void Update()
    // {
    //     
    //     Ray ray = camera.ScreenPointToRay(Input.mousePosition);
    //     RaycastHit hit;
    //     if(Physics.Raycast(ray, out hit))
    //     {
    //         var hitPoint = hit.point;
    //         Debug.DrawLine(transform.position, hitPoint);
    //         hitPoint.y = testStriker.position.y;//handLeftCollider.transform.position.y;
    //         testStriker.position = hitPoint;
    //     }
    // }

    public void ScoredGoal(bool hitGoalFromBlue)
    {
        if (hitGoalFromBlue)
        {
            redScore++;
        }
        else
        {
            blueScore++;
        }

        DisplayScore();
        if (redScore>=winningScore || blueScore >= winningScore)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
    }

    public void DisplayScore()
    {
        scoreText.text = redScore+" : "+blueScore;//scoreTextStructure.Replace("{0}", redScore.ToString()).Replace("{1}", blueScore.ToString());
    }
}
