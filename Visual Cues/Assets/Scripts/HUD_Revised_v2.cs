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
    private GameObject HUD_East;
    private GameObject HUD_North;
    private GameObject HUD_South;
    private GameObject HUD_West;

    //List of obstacles and their relevant information for HUD cues
    private List<ObstInfo> ObstInfos = new List<ObstInfo>();

    //Maximum multiplier for cue width - will scale to it as distance to obstacle shrinks
    public float cueWidthMaxMultiplier = 4f;

    //Minimum and maximum Obst distance at which to show cues
    public float minDist = 0f;
    public float maxDist = 2.5f;

    [Tooltip("The threshold for HUD cue activation. 0 = always on; 1 = never on.")]
    public float HUDThreshold = 0.25f;

    [Tooltip("The amount to multiply the upper cue HUD Threshold by. Smaller = easier activation.")]
    public float HUDTopMultiplier = 0.5f;

    [Tooltip("The maximum angle for an obstacle to be considered 'in front of' the user.")]
    public float frontAngle = 75f;



    public GameObject debugText;
    public bool HUDCalibration;

    private LayerMask obstacleMask;

    //Cue width multiplier
    private float cueSizeMultiplier = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Transform Cue in HUDFrame.transform)
        {
            HUDCues.Add(Cue.gameObject);

            if (Cue.name == "HUD Cue East")
                HUD_East = Cue.gameObject;
            else if (Cue.name == "HUD Cue South")
                HUD_South = Cue.gameObject;
            else if (Cue.name == "HUD Cue North")
                HUD_North = Cue.gameObject;
            else if (Cue.name == "HUD Cue West")
                HUD_West = Cue.gameObject;


        }

        obstacleMask = LayerMask.GetMask("Obstacles");

        if (collocatedCuesParent.transform.childCount >0)
        {
            foreach (Transform Obst in collocatedCuesParent.transform)
            {
                //Add each obstacle in the list to ObstInfos
                ObstInfos.Add(new ObstInfo(Obst.gameObject.name, Obst.gameObject));
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
        var gazeDirection = Camera.main.transform.forward;

        //Negate X rotation (vertical) for determining whether an obstacle is in front or not
        Vector3 gazeFlat = new Vector3(gazeDirection.x, 0, gazeDirection.z);

        //Debug.Log("Gaze direction: " + gazeDirection.ToString() + "; Flat gaze: " + gazeFlat.ToString());


        //Update information in ObstInfos
        foreach (ObstInfo obst in ObstInfos)
        {
            //Clear recorded  angle, X and Y values; reset bounds
            obst.ObstXValues.Clear();
            obst.ObstYValues.Clear();
            obst.ObstAngles.Clear();
            obst.ObstXmin = 1;
            obst.ObstXmax = -1;
            obst.ObstYmin = 1;
            obst.ObstYmax = -1;
            obst.ObstAngleMin = 180;
            obst.ObstAngleMax = 0;
            obst.ObstBounds = obst.ObstCollider.bounds;


            //Calculate distance to obstacle center
            Vector3 camToObstacle = obst.ObstObject.transform.position - headPosition;
            obst.ObstMinDist = camToObstacle.magnitude; //Distance from head to center of object


            //Calculate angles to obstacle center, min and max bounds
            //Center
            float xAngleCenter = Vector3.SignedAngle(Camera.main.transform.right, camToObstacle, Camera.main.transform.up);
            float yAngleCenter = Vector3.SignedAngle(Camera.main.transform.up, camToObstacle, Camera.main.transform.right);
            float obstAngleCenter = Vector3.Angle(gazeFlat, camToObstacle);

            obst.ObstXValues.Add(Mathf.Cos(xAngleCenter * Mathf.Deg2Rad));
            obst.ObstYValues.Add(Mathf.Cos(yAngleCenter * Mathf.Deg2Rad));
            obst.ObstAngles.Add(obstAngleCenter);

            //Minimum bounds
            Vector3 camToObstacleMin = obst.ObstBounds.min - headPosition;
            float xAngleMin = Vector3.SignedAngle(Camera.main.transform.right, camToObstacleMin, Camera.main.transform.up);
            float yAngleMin = Vector3.SignedAngle(Camera.main.transform.up, camToObstacleMin, Camera.main.transform.right);
            float obstAngleUpperBounds = Vector3.Angle(gazeFlat, camToObstacleMin);

            obst.ObstXValues.Add(Mathf.Cos(xAngleMin * Mathf.Deg2Rad));
            obst.ObstYValues.Add(Mathf.Cos(yAngleMin * Mathf.Deg2Rad));
            obst.ObstAngles.Add(obstAngleUpperBounds);

            //Maximum bounds
            Vector3 camToObstacleMax = obst.ObstBounds.max - headPosition;
            float xAngleMax = Vector3.SignedAngle(Camera.main.transform.right, camToObstacleMax, Camera.main.transform.up);
            float yAngleMax = Vector3.SignedAngle(Camera.main.transform.up, camToObstacleMax, Camera.main.transform.right);
            float obstAngleLowerBounds = Vector3.Angle(gazeFlat, camToObstacleMax);

            obst.ObstXValues.Add(Mathf.Cos(xAngleMax * Mathf.Deg2Rad));
            obst.ObstYValues.Add(Mathf.Cos(yAngleMax * Mathf.Deg2Rad));
            obst.ObstAngles.Add(obstAngleLowerBounds);

            





            //Set minimum and maximum angles, X values, and Y values for each obstacle.
            foreach (float x in obst.ObstXValues)
            {
                if (obst.ObstXmin > x)
                    obst.ObstXmin = x;

                if (obst.ObstXmax < x)
                    obst.ObstXmax = x;
            }

            foreach (float y in obst.ObstYValues)
            {
                if (obst.ObstYmin > y)
                    obst.ObstYmin = y;

                if (obst.ObstYmax < y)
                    obst.ObstYmax = y;
            }

            foreach (float angle in obst.ObstAngles)
            {
                if (obst.ObstAngleMin > angle)
                    obst.ObstAngleMin = angle;

                if (obst.ObstAngleMax < angle)
                    obst.ObstAngleMax = angle;
            }

            //Check if obstacle is in front
            if (obst.ObstAngleMin <= frontAngle)
                obst.IsFront = true;
            else
                obst.IsFront = false;

        }

    }
    public void ActivateHUD ()
    {
        //Assesses which cues should be illuminated and their appropriate size


        //Determine closest obstacle within acceptable distance in front of the user as the target obstacle
        float closestDist = 1000f;
        ObstInfo targetObst = null;

        foreach (ObstInfo obst in ObstInfos)
        {
            if (obst.IsFront)
            {
                if (obst.ObstMinDist < closestDist && obst.ObstMinDist <= maxDist)
                {
                    targetObst = obst;
                    closestDist = obst.ObstMinDist;
                }
            }
        }


        //If calirbation mode is on, keep all cues visible and reset to default positions and sizes
        if (HUDCalibration)
        {
            cueSizeMultiplier = 1.0f;
            foreach (GameObject Cue in HUDCues)
            {
                Cue.SetActive(true);
                Cue.transform.localScale = Vector3.one;
            }
            HUD_East.transform.localPosition = new Vector3(0.5f - 0.05f, HUD_East.transform.localPosition.y, HUD_East.transform.localPosition.z);
            HUD_South.transform.localPosition = new Vector3(HUD_South.transform.localPosition.x, -0.5f + 0.05f, HUD_South.transform.localPosition.z);
            HUD_West.transform.localPosition = new Vector3(-0.5f + 0.05f, HUD_West.transform.localPosition.y, HUD_West.transform.localPosition.z);
            HUD_North.transform.localPosition = new Vector3(HUD_North.transform.localPosition.x, 0.5f - 0.05f, HUD_North.transform.localPosition.z);
        }

        //Reset and turn off all cues
        else
        {
            cueSizeMultiplier = 1.0f;
            foreach (GameObject Cue in HUDCues)
            {
                Cue.SetActive(false);
                Cue.transform.localScale = Vector3.one;
            }
            HUD_East.transform.localPosition = new Vector3(0.5f - 0.05f, HUD_East.transform.localPosition.y, HUD_East.transform.localPosition.z);
            HUD_South.transform.localPosition = new Vector3(HUD_South.transform.localPosition.x, -0.5f + 0.05f, HUD_South.transform.localPosition.z);
            HUD_West.transform.localPosition = new Vector3(-0.5f + 0.05f, HUD_West.transform.localPosition.y, HUD_West.transform.localPosition.z);
            HUD_North.transform.localPosition = new Vector3(HUD_North.transform.localPosition.x, 0.5f - 0.05f, HUD_North.transform.localPosition.z);
        }
        
        //If no target obstacle, do nothing
        if (targetObst == null)            
        {
            Debug.Log("No valid targets.");
        }

        else //Turn on and resize cues for current obstacle
        {
            cueSizeMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, targetObst.ObstMinDist);

            if (targetObst.ObstXmin >= HUDThreshold) //Turn on right HUD
            {
                GameObject Cue = HUD_East;
                Cue.SetActive(true);
                Cue.transform.localScale = new Vector3(Vector3.one.x * cueSizeMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(0.5f - 0.05f * cueSizeMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);
            }

            if (targetObst.ObstXmax <= -1 * HUDThreshold) //Turn on left HUD 
            {
                GameObject Cue = HUD_West;
                Cue.SetActive(true);
                Cue.transform.localScale = new Vector3(Vector3.one.x * cueSizeMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(-0.5f + 0.05f * cueSizeMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);
            }

            if (targetObst.ObstYmin >= HUDThreshold * HUDTopMultiplier) //Turn on top HUD. Modified by HUD Top Multiplier.
            {
                GameObject Cue = HUD_North;
                Cue.SetActive(true);
                Cue.transform.localScale = new Vector3(Vector3.one.x * cueSizeMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, 0.5f - 0.05f * cueSizeMultiplier, Cue.transform.localPosition.z);
            }

            else if (targetObst.ObstYmax <= -1* HUDThreshold) //Turn on bottom HUD
            {
                GameObject Cue = HUD_South;
                Cue.SetActive(true);
                Cue.transform.localScale = new Vector3(Vector3.one.x * cueSizeMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, -0.5f + 0.05f * cueSizeMultiplier, Cue.transform.localPosition.z);
            }

            //Debug.Log("Target obstacle: " + targetObst.ObstName);
            if (debugText.activeSelf)
            {
                debugText.GetComponent<TextMeshProUGUI>().text +=
                    "\nTarget obstacle: " + targetObst.ObstName.ToString() +
                    "\nDistance: " + Mathf.Round(100 * targetObst.ObstMinDist) / 100 +
                    "\nCue Size Multiplier: " + Mathf.Round(100 * cueSizeMultiplier) / 100 +
                    "\nMin and max angle: " + Mathf.Round(targetObst.ObstAngleMin) + ", " + Mathf.Round(targetObst.ObstAngleMax) +
                    "\nMin and max X factor: " + Mathf.Round(100 * targetObst.ObstXmin) / 100 + ", " + Mathf.Round(100 * targetObst.ObstXmax) / 100 +
                    "\nMin and max Y factor: " + Mathf.Round(100 * targetObst.ObstYmin) / 100 + ", " + Mathf.Round(100 * targetObst.ObstYmax) / 100;
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

            //Also adjust calibration obstacle
            obstacleManager.calibrationObstacle.SetActive(false);
        }

        else
        {
            HUDCalibration = true;
            Debug.Log("HUD calibration on.");
            textToSpeech.StartSpeaking("HUD Calibration on.");
            obstacleManager.calibrationObstacle.SetActive(true);

        }
    }

  
    public void AdjustFrontAngle(float x)
    {
        //Adjust width of cone to consider an obstacle "in front of" the user
        frontAngle += x;
        if (frontAngle < 0)
            frontAngle = 0;
        else if (frontAngle > 90)
            frontAngle = 90;
        Debug.Log("Front angle now " + frontAngle);
        textToSpeech.StartSpeaking("Front angle now " + frontAngle);
    }

    public void AdjustHUDThreshold (float x)
    {
        //Adjust HUD threshold. Higher = more extreme angle required to trigger HUD.
        HUDThreshold += x;
        if (HUDThreshold < 0)
            HUDThreshold = 0;
        if (HUDThreshold > 1)
            HUDThreshold = 1;
        Debug.Log("HUD Threshold now " + HUDThreshold);
        textToSpeech.StartSpeaking("HUD threshold now " + HUDThreshold);
    }


    public class ObstInfo
    {
        //Redone for HUD_Revised_v2 
        public string ObstName { get; set; }
        public GameObject ObstObject { get; set; }
        public float ObstMinDist { get; set; }
        public List<float> ObstXValues { get; set; }
        public List<float> ObstYValues { get; set; }
        public float ObstXmax { get; set; }
        public float ObstXmin { get; set; }
        public float ObstYmax { get; set; }
        public float ObstYmin { get; set; }
        public List<float> ObstAngles { get; set; }
        public float ObstAngleMin { get; set; }
        public float ObstAngleMax { get; set; }
        public bool IsFront { get; set; } //true if the obstacle is "in front of" the user
        public Collider ObstCollider { get; set; }
        public Bounds ObstBounds { get; set; }


        public ObstInfo()
        {
            ObstName = "unknown";
            ObstObject = null;
            ObstMinDist = 0;
            ObstXValues = new List<float>();
            ObstYValues = new List<float>();
            ObstAngles = new List<float>();
        }

        public ObstInfo(string obstName, GameObject obstObject)

        {
            ObstName = obstName;
            ObstObject = obstObject;
            ObstCollider = obstObject.GetComponent<Collider>();
            ObstBounds = ObstCollider.bounds;
            ObstXValues = new List<float>();
            ObstYValues = new List<float>();
            ObstAngles = new List<float>();

        }
    }
    
}
