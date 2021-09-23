using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays HUD indicators leading towards desired obstacles.
/// Revised design features a limited number of bars along the side of the viewing area 
/// that grow or shrink in response to distance and direction to obstacle.
/// </summary>

public class HUD_Revised : MonoBehaviour
{
    public GameObject HUDFrame; //Frame containing HUD cues

    [HideInInspector]
    public List<GameObject> HUDCues; //All HUD cue objects

    //Minimum and maximum bar width
    public float minWidth = 0.1f;
    public float maxWidth = 0.2f;

    //Minimum and maximum obstacle distance at which to show cues
    public float minDist = 0f;
    public float maxDist = 2.5f;

    //Minimum angle. Any object which extends closer to the user's gaze will not trigger cues.
    public float minAngle = 43f;



    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform Cue in HUDFrame.transform)
        {
            HUDCues.Add(Cue.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CueCast();
    }

    public void CueCast()
    {
        //Assesses locations of obstacles using raycasts

        //Capture camera's location and orientation
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        //Uses spherecast
        float sphereRadius = 0.05f;

        List<RaycastHit> cueCastHits = new List<RaycastHit>();

        RaycastHit hit;

        Physics.SphereCast(headPosition, sphereRadius, gazeDirection, out hit, maxDist);

        if(hit.transform != null)
        {
            Debug.Log("Hit " + hit.transform.parent.gameObject.ToString());
        }

    }
}
