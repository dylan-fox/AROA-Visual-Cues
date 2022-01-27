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
                //ObstInfos.Add(new ObstInfo(Obst.gameObject.ToString(), Obst.gameObject, 0, 0, 0));
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
        //var headPositionCorrected = headPosition - new Vector3(0f, 0.5f, 0f); //Shoot ray from mid height to treat upper and lower obstacles more equitably
        var gazeDirection = Camera.main.transform.forward;

        //Debug.Log("Gaze direction: " + gazeDirection);


        //Update information in ObstInfos
        foreach (ObstInfo obst in ObstInfos)
        {
            //Clear recorded  angle, X and Y values
            obst.ObstXValues.Clear();
            obst.ObstYValues.Clear();
            obst.ObstAngles.Clear();
            obst.ObstXmin = 1;
            obst.ObstXmax = -1;
            obst.ObstYmin = 1;
            obst.ObstYmax = -1;
            obst.ObstAngleMin = 180;
            obst.ObstAngleMax = 0;


            //Calculate distance to obstacle center
            Vector3 camToObstacle = obst.ObstObject.transform.position - headPosition;
            obst.ObstMinDist = camToObstacle.magnitude; //Distance from head to center of object
            //obst.ObstMinAngle = obstAngle;



            //Calculate angles to obstacle center, min and max bounds
            float xAngleCenter = Vector3.SignedAngle(Camera.main.transform.right, camToObstacle, Camera.main.transform.up);
            float yAngleCenter = Vector3.SignedAngle(Camera.main.transform.up, camToObstacle, Camera.main.transform.right);
            float obstAngleCenter = Vector3.Angle(gazeDirection, camToObstacle);

            obst.ObstXValues.Add(Mathf.Cos(xAngleCenter * Mathf.Deg2Rad));
            obst.ObstYValues.Add(Mathf.Cos(yAngleCenter * Mathf.Deg2Rad));
            obst.ObstAngles.Add(obstAngleCenter);

            Vector3 camToObstacleMin = obst.ObstBounds.min - headPosition;
            float xAngleMin = Vector3.SignedAngle(Camera.main.transform.right, camToObstacleMin, Camera.main.transform.up);
            float yAngleMin = Vector3.SignedAngle(Camera.main.transform.up, camToObstacleMin, Camera.main.transform.right);
            float obstAngleUpperBounds = Vector3.Angle(gazeDirection, camToObstacleMin);

            obst.ObstXValues.Add(Mathf.Cos(xAngleMin * Mathf.Deg2Rad));
            obst.ObstYValues.Add(Mathf.Cos(yAngleMin * Mathf.Deg2Rad));
            obst.ObstAngles.Add(obstAngleUpperBounds);

            Vector3 camToObstacleMax = obst.ObstBounds.max - headPosition;
            float xAngleMax = Vector3.SignedAngle(Camera.main.transform.right, camToObstacleMax, Camera.main.transform.up);
            float yAngleMax = Vector3.SignedAngle(Camera.main.transform.up, camToObstacleMax, Camera.main.transform.right);
            float obstAngleLowerBounds = Vector3.Angle(gazeDirection, camToObstacleMax);

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
            if (obst.IsFront)
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
                    "\nTarget obstacle: " + targetObst.ObstName.ToString() +
                    "\nDistance: " + Mathf.Round(100 * targetObst.ObstMinDist) / 100 +
                    "\nMin and max angle: " + Mathf.Round(targetObst.ObstAngleMin) + ", " + Mathf.Round(targetObst.ObstAngleMax) +
                    "\nMin and max X factor: " + Mathf.Round(100 * targetObst.ObstXmin) / 100 + ", " + Mathf.Round(100 * targetObst.ObstXmax) / 100 +
                    "\nMin and max Y factor: " + Mathf.Round(100 * targetObst.ObstYmin) / 100 + ", " + Mathf.Round(100 * targetObst.ObstYmax) / 100;
            }



            if (targetObst.ObstXmin >= HUDThreshold) //Turn on right HUD
            {
                GameObject Cue = HUD_East;
                Cue.SetActive(true);
                eastMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, targetObst.ObstMinDist);
                Cue.transform.localScale = new Vector3(Vector3.one.x * eastMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(0.5f - 0.05f * eastMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);

                if (debugText.activeSelf)
                    debugText.GetComponent<TextMeshProUGUI>().text += "\nEast Cue width multiplier: " + Mathf.Round(eastMultiplier * 100) / 100;
            }

            else if (targetObst.ObstXmax <= -1 * HUDThreshold) //Turn on left HUD 
            {
                GameObject Cue = HUD_West;
                Cue.SetActive(true);
                westMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, targetObst.ObstMinDist);
                Cue.transform.localScale = new Vector3(Vector3.one.x * westMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(-0.5f + 0.05f * westMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);

                if (debugText.activeSelf)
                    debugText.GetComponent<TextMeshProUGUI>().text += "\nWest Cue width multiplier: " + Mathf.Round(westMultiplier * 100) / 100;
            }

            if (targetObst.ObstYmin >= HUDThreshold) //Turn on top HUD
            {
                GameObject Cue = HUD_North;
                Cue.SetActive(true);
                northMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, targetObst.ObstMinDist);
                Cue.transform.localScale = new Vector3(Vector3.one.x * northMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, 0.5f - 0.05f * northMultiplier, Cue.transform.localPosition.z);

                if (debugText.activeSelf)
                    debugText.GetComponent<TextMeshProUGUI>().text += "\nNorth Cue width multiplier: " + Mathf.Round(northMultiplier * 100) / 100;
            }

            else if (targetObst.ObstYmax <= -1* HUDThreshold) //Turn on bottom HUD
            {
                GameObject Cue = HUD_South;
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
