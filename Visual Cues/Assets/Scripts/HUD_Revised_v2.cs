using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;
using Microsoft.MixedReality.Toolkit.Audio;



/// <summary>
/// Displays HUD indicators leading towards desired Obsts.
/// Revised design features a limited number of bars along the side of the viewing area 
/// that grow or shrink in response to distance and direction to Obst.
/// Revised v2 replaces raycast system with calculations based on rectangular digital obstacles.
/// </summary>

public class HUD_Revised_v2 : MonoBehaviour
{
    public GameObject HUDFrame; //Frame containing HUD cues
    public TextToSpeech textToSpeech;
    public ObstacleManager obstacleManager;
    public Experiment_Logger experimentLogger;
    public GameObject collocatedCuesParent;
 

    [HideInInspector]
    public List<GameObject> HUDCues; //All HUD cue objects

    //List of obstacles and their relevant information for HUD cues
    private List<ObstInfo> ObstInfos = new List<ObstInfo>();

    //Maximum multiplier for cue width - will scale to it as distance to obstacle shrinks
    public float cueWidthMaxMultiplier = 2f;

    //Minimum and maximum Obst distance at which to show cues
    public float minDist = 0f;
    public float maxDist = 2.5f;

    [Tooltip("The threshold for HUD cue activation. <=0 = always on; >=1 = never on.")]
    public float HUDThreshold = 0.5f;

    [Tooltip("The maximum angle for an obstacle to be considered 'in front of' the user.")]
    public float frontAngle = 75f;

    //Minimum angle. Any object which extends closer to the user's gaze will not trigger cues.
    public float minAngle = 43f;

    [Tooltip("Number, in angles, to skip between raycasts. Smaller = more precise but higher processing load.")]
    public float angleInterval = 15f;

    [Tooltip("Size of sphere to spherecast with.")]
    public float sphereRadius = 0.2f;

    public GameObject debugText;
    public bool DebugRays = false;
    public bool HUDCalibration;

    private LayerMask obstacleMask;

    [Tooltip("Time in seconds for a cue to stay on after being triggered.")]
    public float fadeTime = 0.25f;

    //Time each HUD cue was last triggered
    private float northTrigger = 0;
    private float eastTrigger = 0;
    private float westTrigger = 0;
    private float southTrigger = 0;

    //Cue width multipliers
    private float northMultiplier = 1.0f;
    private float eastMultiplier = 1.0f;
    private float southMultiplier = 1.0f;
    private float westMultiplier = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Transform Cue in HUDFrame.transform)
        {
            HUDCues.Add(Cue.gameObject);
        }

        obstacleMask = LayerMask.GetMask("Obstacles");

        if (collocatedCuesParent.transform.childCount >0)
        {
            foreach (Transform Obst in collocatedCuesParent.transform)
            {
                //Add each obstacle in the list to ObstInfos
                //ObstInfos.Add(new ObstInfo(Obst.gameObject.ToString(), Obst.gameObject, 0, 0, 0));
                ObstInfos.Add(new ObstInfo(Obst.gameObject.ToString(), Obst.gameObject));
            }
        }

        else
        {
            Debug.Log("No obstacles found.");
        }

    }

    // Update is called once per frame
    void Update()
    {
        CalculatePositions();
        ActivateHUD();

    }


    
    public void CalculatePositions()
    {
        //Calculates positions of obstacles relative to user

        //Capture camera's location and orientation
        var headPosition = Camera.main.transform.position;
        var headPositionCorrected = headPosition - new Vector3(0f, 0.5f, 0f); //Shoot ray from mid height to treat upper and lower obstacles more equitably
        var gazeDirection = Camera.main.transform.forward;

        //Debug.Log("Gaze direction: " + gazeDirection);


        //Update information in ObstInfos
        foreach (ObstInfo obst in ObstInfos)
        {

            //Calculate angle from gaze direction to obstacle
            Vector3 camToObstacle = obst.ObstObject.transform.position - headPositionCorrected;
            float obstAngle = Vector3.Angle(gazeDirection, camToObstacle);

            //float obstAngle = Vector3.SignedAngle(gazeDirection, info.ObstObject.transform.position, Camera.main.transform.right);
            //Debug.DrawRay(headPositionCorrected, gazeDirection, Color.blue);
            //Debug.DrawRay(headPositionCorrected, camToObstacle, Color.green);
            //Debug.Log("Distance to " + obst.ObstName + " = " + obst.ObstMinDist);
            //Debug.Log("Angle of " + obst.ObstName + " = " + obstAngle.ToString());
            //Debug.Log("Position of " + info.ObstName + " = " + info.ObstObject.transform.position);

            float xAngle = Vector3.SignedAngle(Camera.main.transform.right, camToObstacle, Camera.main.transform.up);
            float yAngle = Vector3.SignedAngle(Camera.main.transform.up, camToObstacle, Camera.main.transform.right);

            //Assign values to the ObstInfo object
            obst.ObstMinDist = (headPositionCorrected - obst.ObstObject.transform.position).magnitude; //Distance from head to center of object
            obst.ObstMinAngle = obstAngle;
            obst.obstX = Mathf.Cos(xAngle * Mathf.Deg2Rad);
            obst.obstY = Mathf.Cos(yAngle * Mathf.Deg2Rad);

            if (obstAngle <= frontAngle)
                obst.isFront = true;
            else
                obst.isFront = false;

            //Debug.Log("X Angle of " + info.ObstName + " = " + xAngle + "; cosine " + info.obstX + "; acos " +  Mathf.Acos(info.obstX) * Mathf.Rad2Deg);
            //Debug.Log("Y Angle of " + info.ObstName + " = " + yAngle + "; cosine " + Mathf.Cos(yAngle * Mathf.Deg2Rad));




        }

    }
    public void ActivateHUD ()
    {
        //Assesses which cues should be illuminated and their appropriate size


        //For each cue, determine if it should be on for time/calibration reasons
        foreach (GameObject Cue in HUDCues)
        {
            //Debug.Log("Cue name: " + Cue.name);
            bool resetTrigger = false;

            //If cue is on from a previous frame and hasn't passed the fade time threshold yet, keep it on; otherwise, reset it
            if (Cue.name == "HUD Cue East")
            {
                if (Time.time <= eastTrigger + fadeTime)
                {
                    //Do nothing
                }

                else
                {
                    Cue.SetActive(false);
                    eastMultiplier = 1.0f;
                    Cue.transform.localScale = Vector3.one;
                    resetTrigger = true;
                }
            }

            else if (Cue.name == "HUD Cue South")
            {
                if (Time.time <= southTrigger + fadeTime)
                {
                    //Do nothing
                }

                else
                {
                    Cue.SetActive(false);
                    southMultiplier = 1.0f;
                    Cue.transform.localScale = Vector3.one;
                    resetTrigger = true;
                    //Debug.Log("South cue reset.");
                }
            }

            else if (Cue.name == "HUD Cue West")
            {
                if (Time.time <= westTrigger + fadeTime)
                {
                    //Do nothing
                }

                else
                {
                    Cue.SetActive(false);
                    westMultiplier = 1.0f;
                    Cue.transform.localScale = Vector3.one;
                    resetTrigger = true;
                }
            }

            else if (Cue.name == "HUD Cue North")
            {
                if (Time.time <= northTrigger + fadeTime)
                {
                    //Do nothing
                }

                else
                {
                    Cue.SetActive(false);
                    northMultiplier = 1.0f;
                    Cue.transform.localScale = Vector3.one;
                    resetTrigger = true;
                }
            }

            else
            {
                Debug.Log("Strange item found in Cues list.");
            }


            if (HUDCalibration)
            {
                //Keep all cues visible and reset to default positions
                Cue.SetActive(true);
                if (Cue.name == "HUD Cue East")
                    Cue.transform.localPosition = new Vector3(0.5f - 0.05f, Cue.transform.localPosition.y, Cue.transform.localPosition.z);
                else if (Cue.name == "HUD Cue South")
                    Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, -0.5f + 0.05f, Cue.transform.localPosition.z);
                else if (Cue.name == "HUD Cue West")
                    Cue.transform.localPosition = new Vector3(-0.5f + 0.05f, Cue.transform.localPosition.y, Cue.transform.localPosition.z);
                else if (Cue.name == "HUD Cue North")
                    Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, 0.5f - 0.05f, Cue.transform.localPosition.z);
            }
        }


        //Determine closest obstacle within accesptable distance in front of the user as the target obstacle
        float closestDist = 1000f;
        ObstInfo targetObst = null;

        foreach (ObstInfo obst in ObstInfos)
        {
            if (obst.isFront)
            {
                if (obst.ObstMinDist < closestDist && obst.ObstMinDist <= maxDist)
                {
                    targetObst = obst;
                    closestDist = obst.ObstMinDist;
                }
            }
        }

        //Adjust the HUD cues based on the characteristics of the target obstacle
        if (targetObst == null)
        {
            Debug.Log("No valid targets.");
        }

        else
        {
            Debug.Log("Target obstacle: " + targetObst.ObstName);
            if (debugText.activeSelf)
            {
                debugText.GetComponent<TextMeshProUGUI>().text +=
                    "\nTarget obstacle: " + targetObst.ObstName +
                    "\nDistance: " + Mathf.Round(100*targetObst.ObstMinDist)/100 +
                    "\nAngle: " + Mathf.Round(targetObst.ObstMinAngle) +
                    "\nX factor: " + Mathf.Round(100*targetObst.obstX)/100 +
                    "\nY factor: " + Mathf.Round(100*targetObst.obstY)/100;
            }



            if (targetObst.obstX >= HUDThreshold) //Turn on right HUD
            {
                GameObject Cue = HUDCues.Find(obj => obj.name == "HUD Cue East");
                Cue.SetActive(true);
                eastMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, targetObst.ObstMinDist);
                Cue.transform.localScale = new Vector3(Vector3.one.x * eastMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(0.5f - 0.05f * eastMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);

                if (debugText.activeSelf)
                    debugText.GetComponent<TextMeshProUGUI>().text += "\nEast Cue width multiplier: " + Mathf.Round(eastMultiplier * 100) / 100;
            }

            else if (targetObst.obstX <= -1 * HUDThreshold) //Turn on left HUD 
            {
                GameObject Cue = HUDCues.Find(obj => obj.name == "HUD Cue West");
                Cue.SetActive(true);
                westMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, targetObst.ObstMinDist);
                Cue.transform.localScale = new Vector3(Vector3.one.x * westMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(-0.5f + 0.05f * westMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);

                if (debugText.activeSelf)
                    debugText.GetComponent<TextMeshProUGUI>().text += "\nWest Cue width multiplier: " + Mathf.Round(westMultiplier * 100) / 100;
            }

            if (targetObst.obstY >= HUDThreshold) //Turn on top HUD
            {
                GameObject Cue = HUDCues.Find(obj => obj.name == "HUD Cue North");
                Cue.SetActive(true);
                northMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, targetObst.ObstMinDist);
                Cue.transform.localScale = new Vector3(Vector3.one.x * northMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, 0.5f - 0.05f * northMultiplier, Cue.transform.localPosition.z);

                if (debugText.activeSelf)
                    debugText.GetComponent<TextMeshProUGUI>().text += "\nNorth Cue width multiplier: " + Mathf.Round(northMultiplier * 100) / 100;
            }

            else if (targetObst.obstY <= -1* HUDThreshold) //Turn on bottom HUD
            {
                GameObject Cue = HUDCues.Find(obj => obj.name == "HUD Cue South");
                Cue.SetActive(true);
                southMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, targetObst.ObstMinDist);
                Cue.transform.localScale = new Vector3(Vector3.one.x * southMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, -0.5f + 0.05f * southMultiplier, Cue.transform.localPosition.z);

                if (debugText.activeSelf)
                    debugText.GetComponent<TextMeshProUGUI>().text += "\nSouth Cue width multiplier: " + Mathf.Round(southMultiplier * 100) / 100;
            }
        }


    }

    public float CalculateCueMultiplier (float MaxMultiplier, float minDist, float maxDist, float distance)
    {
        float cueMultiplier = 1 + (MaxMultiplier - 1) * (1 - (distance - minDist) / (maxDist - minDist));
        return cueMultiplier;
    }

    public void shiftHUDRight (float xShift)
    {
        var curPos = HUDFrame.transform.localPosition;
        HUDFrame.transform.localPosition = new Vector3(curPos.x + xShift, curPos.y, curPos.z);
    }

    public void shiftHUDUp (float yShift)
    {
        var curPos = HUDFrame.transform.localPosition;
        HUDFrame.transform.localPosition = new Vector3(curPos.x, curPos.y + yShift, curPos.z);
    }

    public void scaleHUD (float multiplier)
    {
        var currentScale = HUDFrame.transform.localScale;
        HUDFrame.transform.localScale = new Vector3(currentScale.x * multiplier, currentScale.y, currentScale.z);
    }

    public void HUDCalibrationToggle ()
    {
        if (HUDCalibration)
        {
            HUDCalibration = false;
            Debug.Log("HUD calibration off.");
            textToSpeech.StartSpeaking("HUD Calibration off.");
        }

        else
        {
            HUDCalibration = true;
            Debug.Log("HUD calibration on.");
            textToSpeech.StartSpeaking("HUD Calibration on.");
        }
    }

    public void AdjustMinAngle (float x)
    {
        minAngle += x;
        if (minAngle < 0)
            minAngle = 0;
        if (minAngle > 90)
            minAngle = 90;
        Debug.Log("Min angle now " + minAngle);
        textToSpeech.StartSpeaking("Min angle now " + minAngle);
    }

    public void AdjustSphereRadius (float x)
    {
        sphereRadius += x;
        if (sphereRadius < 0)
            sphereRadius = 0;

        Debug.Log("Sphere radius now " + sphereRadius);
        textToSpeech.StartSpeaking("Sphere radius now " + sphereRadius);
    }

    public void AdjustAngleInterval (float x)
    {
        angleInterval += x;
        if (angleInterval < 5)
            angleInterval = 5;
        Debug.Log("Angle interval now " + angleInterval);
        textToSpeech.StartSpeaking("Angle interval now " + angleInterval);
    }

    /*
    public class ObstInfo
    {
        public string ObstName { get; set; }
        public GameObject ObstObject { get; set; }
        public float ObstMinDist { get; set; }
        public List<float> ObstXAngles { get; set; }
        public List<float> ObstYAngles { get; set; }
        
        public float ObstMinX { get; set; }
        public float ObstMaxX { get; set; }
        public float ObstMinY { get; set; }
        public float ObstMaxY { get; set; }

        public float ObstMinAbsX { get; set; }
        public float ObstMinAbsY { get; set; }
        
        public ObstInfo()
        {
            ObstName = "unknown";
            ObstObject = null;
            ObstMinDist = 0;
            ObstXAngles = null;
            ObstYAngles = null;
        }

        public ObstInfo (string obstName, GameObject obstObject, float minDist,  float angleX, float angleY) 

        {
            ObstName = obstName;
            ObstObject = obstObject;
            ObstMinDist = minDist;
            ObstXAngles = new List<float>();
            ObstYAngles = new List<float>();
            ObstXAngles.Add(angleX);
            ObstYAngles.Add(angleY);
            ObstMinX = angleX;
            ObstMaxX = angleX;
            ObstMinY = angleY;
            ObstMaxY = angleY;
            ObstMinAbsX = Mathf.Abs(angleX);
            ObstMinAbsY = Mathf.Abs(angleY);
        }
    }
    */

    
    public class ObstInfo
    {
        //Redone for HUD_Revised_v2 
        public string ObstName { get; set; }
        public GameObject ObstObject { get; set; }
        public float ObstMinDist { get; set; }
        //public List<float> ObstXValues { get; set; }
        //public List<float> ObstYValues { get; set; }
        public float obstX { get; set; } //for now, use just one value - that of the center
        public float obstY { get; set; }
        public float ObstMinAngle { get; set; }
        public float ObstMaxAngle { get; set; }
        public bool isFront { get; set; } //true if the obstacle is "in front of" the user


        public ObstInfo()
        {
            ObstName = "unknown";
            ObstObject = null;
            ObstMinDist = 0;

        }

        public ObstInfo(string obstName, GameObject obstObject)

        {
            ObstName = obstName;
            ObstObject = obstObject;
            //ObstMinDist = minDist;
            //ObstAngles = new List<float>();
            //ObstAngles.Add(angle);
            //ObstMinAngle = angle;
            //ObstMaxAngle = angle;
            //ObstMinAngleAbsolute = Mathf.Abs(angle);
        }
    }
    
}
