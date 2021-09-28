using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays HUD indicators leading towards desired Obsts.
/// Revised design features a limited number of bars along the side of the viewing area 
/// that grow or shrink in response to distance and direction to Obst.
/// </summary>

public class HUD_Revised : MonoBehaviour
{
    public GameObject HUDFrame; //Frame containing HUD cues

    [HideInInspector]
    public List<GameObject> HUDCues; //All HUD cue objects

    //List of obstacles and their relevant information for HUD cues
    private List<ObstInfo> ObstInfos = new List<ObstInfo>();

    //Minimum and maximum bar width
    public float minWidth = 0.1f;
    public float maxWidth = 0.2f;

    //Minimum and maximum Obst distance at which to show cues
    public float minDist = 0f;
    public float maxDist = 2.5f;

    //Minimum angle. Any object which extends closer to the user's gaze will not trigger cues.
    public float minAngle = 43f;

    [Tooltip("Number, in angles, to skip between raycasts. Smaller = more precise but higher processing load.")]
    public float angleInterval = 15f;

    [Tooltip("Size of sphere to spherecast with.")]
    public float sphereRadius = 0.05f;




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



        //Cast at intervals from -90 degrees to positive 90 degrees X and Y
        for (float xAngle = -90; xAngle <= 90; xAngle += angleInterval)
        {
            for (float yAngle = -90; yAngle <= 90; yAngle += angleInterval)
            {
                RaycastHit hit;

                //Determine new ray direction based on a certain angle off of the gaze direction
                var rayDirection = gazeDirection;
                rayDirection = Quaternion.AngleAxis(xAngle, Camera.main.transform.right) * rayDirection;
                rayDirection = Quaternion.AngleAxis(yAngle, Camera.main.transform.up) * rayDirection;
                



                //Debug rays
                if (Mathf.Abs(xAngle) <= minAngle && Mathf.Abs(yAngle) <= minAngle)
                    Debug.DrawRay(headPosition, rayDirection * maxDist, Color.red);
                else
                    Debug.DrawRay(headPosition, rayDirection * maxDist, Color.green);

                Physics.SphereCast(headPosition, sphereRadius, rayDirection, out hit, maxDist);



                if (hit.transform != null)
                {
                    Debug.Log("Hit " + hit.transform.parent.gameObject.ToString());

                    //If it hit an obstacle, update its information in the obstacle info list
                    bool newObst = true;
                    foreach (ObstInfo obst in ObstInfos)
                    {
                        if (obst.ObstObject == hit.transform.parent.gameObject)
                        {
                            newObst = false;
                            //Check if the hit has a shorter or larger distance and angle than recorded
                            if (hit.distance > obst.ObstMaxDist)
                                obst.ObstMaxDist = hit.distance;

                            if (hit.distance < obst.ObstMinDist)
                                obst.ObstMinDist = hit.distance;

                            //TODO: Write angle overrides

                        }

                    }

                    //If the obstacle was not found, create a new one
                    if (newObst)
                    {
                        ObstInfos.Add(new ObstInfo(hit.transform.parent.gameObject.ToString(), hit.transform.parent.gameObject, hit.distance, hit.distance, 0, 0)); //TODO: Replace 0s with Angle
                        Debug.Log("New obstacle found: " + hit.transform.parent.gameObject.ToString());
                    }


                }

            }
        }


    }

    public class ObstInfo
    {
        public string ObstName { get; set; }
        public GameObject ObstObject { get; set; }
        public float ObstMinDist { get; set; }
        public float ObstMaxDist { get; set; }
        public float ObstMinAngle { get; set; }
        public float ObstMaxAngle { get; set; }
        public ObstInfo()
        {
            ObstName = "unknown";
            ObstObject = null;
            ObstMinDist = 0;
            ObstMaxDist = 0;
            ObstMinAngle = 0;
            ObstMaxAngle = 0;
        }

        public ObstInfo (string obstName, GameObject obstObject, float minDist, float maxDist, float minAngle, float maxAngle)
        {
            ObstName = obstName;
            ObstObject = obstObject;
            ObstMinDist = minDist;
            ObstMaxDist = maxDist;
            ObstMinAngle = minAngle;
            ObstMaxAngle = maxAngle;
        }


    }
}
