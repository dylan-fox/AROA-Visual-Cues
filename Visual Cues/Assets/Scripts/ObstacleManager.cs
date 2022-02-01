using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Greyman;
using TMPro;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Audio;

public class ObstacleManager : MonoBehaviour
{
    //<summary>A script to manage obstacles and visual cues.</summary>

    //private bool obstaclesOn = true;
    //public GameObject[] digitalObstacles;
    //public List<Vector3> obstacleStartingLocations;
    //public List<Quaternion> obstacleStartingRotations;
    //public List<Vector3> obstacleStartingScales;

    public bool collocatedCuesOn = true;
    public bool hudCuesOn = true;
    public bool gestureLock = false;

    [HideInInspector]
    public List<GameObject> visualCues;
    //public GameObject[] visualCues;

    [HideInInspector]
    public List<Vector3> cueStartingLocations;

    [HideInInspector]
    public List<Quaternion> cueStartingRotations;

    [HideInInspector]
    public List<Vector3> cueStartingScales;

    public GameObject interfaceObject;
    public GameObject cuesParent;
    public GameObject calibrationCue;
    public GameObject debugCanvas;
    public Experiment_Logger experimentLogger;
    

    public GameObject HUDManager;
    //private HUDIndicator HUDIndicator;
    //private HUD_Revised HUD_Revised;
    private HUD_Revised_v2 HUD_Revised;

    public TextToSpeech textToSpeech;

    public GameObject numShownTextObject;
    //public GameObject cuePrefab; //Necessary for AddCue, not ShowCue

    /*
    [Tooltip("Number of cues to show at start.")]
    public int startingCues = 3; //
    private int numShown; //Indicates how many cues are currently shown
    */

    [Tooltip("Default user height in meters.")]
    public float defaultHeight = 1.6256f; //5'4" should be roughly average eye height

    [Tooltip("Maximum distance to show obstacles.")]
    public float maxDisplayDistance = 5f;

    private LayerMask defaultMask = ~0;
    private LayerMask obstacleMask;
    [HideInInspector]
    public bool distanceCap = true;

    [Tooltip("Height of high obstacles in meters.")]
    public float highObstHeight = 1.524f;


    // Start is called before the first frame update
    void Start()
    {
        SavePositions();
        //HUDIndicator = HUDManager.GetComponent<HUDIndicator>();
        HUD_Revised = HUDManager.GetComponent<HUD_Revised_v2>();

        /*
        numShown = visualCues.Count;

        //Hide cues until the number shown matches startingCues.
        while (numShown > startingCues)
        {
            HideCue();
        }
        */

        obstacleMask = LayerMask.GetMask("Obstacles");
        obstacleMask = ~obstacleMask;

        Camera.main.farClipPlane = maxDisplayDistance;

        //Start with cues on, calibration on, gestures and interface off
        //CollocatedCuesToggle();
        //HUDCuesToggle();
        ToggleInterface();
        ToggleGestures();
        textToSpeech.StartSpeaking("Ready");
    }

    // Update is called once per frame
    void Update()
    {
        if (debugCanvas.activeSelf)
        {
            Debug.Log("Debug text object: " + debugCanvas.ToString());
            TextMeshProUGUI debug = debugCanvas.transform.Find("Debug Text").gameObject.GetComponent<TextMeshProUGUI>();
            debug.text =
                "Mode: " + experimentLogger.cueCondition + "\n" +
                "Layout: " + experimentLogger.layout + "\n" +
                "High obstacle height: " + Mathf.Round(highObstHeight * 39.37f) + " inches" + "\n" + 
                "Distance capped: " + distanceCap + "\n" +
                "HUD calibration: " + HUD_Revised.HUDCalibration;
                
            if (!distanceCap || HUD_Revised.HUDCalibration)
            {
                //If distance is uncapped or HUD calibration is on, this will warn experimenter not to run
                debug.text += "\nDO NOT RUN";
            }
            else
                debug.text += "\nOK TO EXPERIMENT";
        }
    }


    public void CollocatedCuesToggle()
    {
        //Debug.Log("Toggling collocated visual cues.");

        if (collocatedCuesOn)
        {
            Debug.Log("Collocated cues off.");
            textToSpeech.StartSpeaking("Co-located cues off.");

            Camera.main.cullingMask = obstacleMask;

            collocatedCuesOn = false;

        }

        else
        {
            Debug.Log("Collocated cues on.");
            textToSpeech.StartSpeaking("Co-located cues on.");

            Camera.main.cullingMask = defaultMask;

            collocatedCuesOn = true;
        }
    }

    public void HUDCuesToggle()
    {
        //Debug.Log("Toggling HUD visual cues.");

        if (hudCuesOn)
        {
            Debug.Log("HUD cues off.");
            textToSpeech.StartSpeaking("HUD cues off.");

            //HUDManager.GetComponent<HUDIndicator>().GetComponent<HUDIndicatorManagerVR>().HideIndicators();
            HUDManager.SetActive(false);
            hudCuesOn = false;
        }

        else
        {
            Debug.Log("HUD cues on.");
            textToSpeech.StartSpeaking("HUD cues on.");

            //HUDManager.GetComponent<HUDIndicator>().GetComponent<HUDIndicatorManagerVR>().ShowIndicators();
            HUDManager.SetActive(true);
            hudCuesOn = true;

        }
    }

    public void CalibrationToggle()
    {
        Debug.Log("Toggling calibration cue.");
        if (calibrationCue.activeSelf)
        { 
            calibrationCue.SetActive(false);
            textToSpeech.StartSpeaking("Calibration cue off.");
        }
        else
        {
            calibrationCue.SetActive(true);
            textToSpeech.StartSpeaking("Calibration cue on.");
        }
    }

    public void DebugToggle()
    {
        Debug.Log("Toggling debug text.");
        if (debugCanvas.activeSelf)
        {
            debugCanvas.SetActive(false);
            textToSpeech.StartSpeaking("Debug text off.");
        }
        else
        {
            debugCanvas.SetActive(true);
            textToSpeech.StartSpeaking("Debug text on.");
        }
    }

   
    /*
    public void ShowCue()
    {
        //Activate the collocated and HUD cues for the next object.
        if (numShown < visualCues.Count)
        {
            visualCues[numShown].SetActive(true);
            //HUDIndicator.AddIndicator(cuesParent.transform.GetChild(numShown), numShown);

            numShown++;
            numShownTextObject.GetComponent<TMP_Text>().text = "Show Cue (" + numShown + "/10)";
            Debug.Log("Showing additional cue. Number shown: " + numShown);
            textToSpeech.StartSpeaking("Showing additional cue.");

        }
        else Debug.Log("Max cues shown.");
    }

    public void HideCue()
    {
        //Hide the last collocated and HUD cue.
        if (numShown >= 1)
        {
            visualCues[numShown - 1].SetActive(false);
            //HUDIndicator.RemoveIndicator(visualCues[numShown - 1].transform);
            //Debug.Log("Indicator removed.");

            numShown--;
            numShownTextObject.GetComponent<TMP_Text>().text = "Show Cue (" + numShown + "/10)";
            Debug.Log("Hiding last cue. Number shown: " + numShown);
            textToSpeech.StartSpeaking("Hiding last cue.");

        }
        else Debug.Log("All cues hidden.");
    }
    */


    public void SavePositions()
    {
        //Get visual cues and record original positions

        visualCues.Clear();
        //Debug.Log("Visual cues cleared; length is now " + visualCues.Count);
        cueStartingLocations.Clear();
        cueStartingRotations.Clear();
        cueStartingScales.Clear();


        for (int n = 0; n < cuesParent.transform.childCount; n++)
        {
            visualCues.Add(cuesParent.transform.GetChild(n).gameObject);
            //Debug.Log("Added visual cue: " + visualCues[n].name);
        }

        Debug.Log("Positions saved for " + visualCues.Count + " visual cues.");
        textToSpeech.StartSpeaking("Positions saved.");


        foreach (GameObject Cue in visualCues)
        {
            //Debug.Log("Cue " + Cue.name + " added at " + Cue.transform.position);
            cueStartingLocations.Add(Cue.transform.localPosition);
            cueStartingRotations.Add(Cue.transform.localRotation);
            cueStartingScales.Add(Cue.transform.localScale);
        }
    }

    public void ResetPositions()
    {
        //Adjusts obstacles back to last saved position.
        Debug.Log("Resetting positions.");
        if (!textToSpeech.IsSpeaking())
            textToSpeech.StartSpeaking("Positions reset.");


        //int obstacleCount = 0;
        int cueCount = 0;

        foreach (GameObject Cue in visualCues)
        {
            Cue.transform.localPosition = cueStartingLocations[cueCount];
            Cue.transform.localRotation = cueStartingRotations[cueCount];
            Cue.transform.localScale = cueStartingScales[cueCount];
            cueCount++;
        }
    }

    public void SetLocation(string posName)
    {
        //Sets location of user to a given position by moving entire Collocated Cues object.
        if (posName == "front")
        {
            //Set cues parent to be at user's location and lower by height
            cuesParent.transform.position = Camera.main.transform.position - new Vector3 (0.0f, defaultHeight, 0.0f);

            //Then rotate in Y axis to match camera
            cuesParent.transform.eulerAngles = new Vector3(0f, 0f, 0f);
            cuesParent.transform.Rotate(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
            Debug.Log("Location reset.");
            textToSpeech.StartSpeaking("Location reset.");

        }

        else
        {
            Debug.Log("Position not found. Location unchanged.");
        }
    }


    public void MoveForward(float dist)
    {
        cuesParent.transform.localPosition += cuesParent.transform.forward * dist;
    }

    public void MoveRight(float dist)
    {
        cuesParent.transform.localPosition += cuesParent.transform.right * dist;
    }

    public void MoveUp(float dist)
    {
        cuesParent.transform.localPosition += cuesParent.transform.up * dist;
    }


    public void ManualRotate(float degrees)
    {
        //Manually rotates cues left or right.
        cuesParent.transform.Rotate(0f, degrees, 0f);
    }



    public void ToggleInterface()
    {
        //Toggles visibility of user interface.
        
        if (interfaceObject.activeSelf)
        {
            interfaceObject.SetActive(false);
            textToSpeech.StartSpeaking("Interface hidden.");

        }
        else
        {
            interfaceObject.SetActive(true);
            textToSpeech.StartSpeaking("Interface shown.");

        }

    }

    public void ToggleGestures()
    {
        //Locks gesture-based movement controls so users cannot accidentally move digital obstacles.
        if (!gestureLock)
        {
            //Turn gesture lock on by disabling Object Manipulators and Bounds Controls on all collocated cues
            Debug.Log("Locking gestures.");
            foreach (GameObject cue in visualCues)
            {
                if (cue.GetComponent<ObjectManipulator>() != null)
                {
                    cue.GetComponent<ObjectManipulator>().enabled = false;
                }

                if (cue.GetComponent<BoundsControl>() != null)
                {
                    cue.GetComponent<BoundsControl>().enabled = false;
                }
            }
            gestureLock = true;
            textToSpeech.StartSpeaking("Gestures locked.");

        }

        else
        {
            //Turn gesture lock off by enabling Object Manipulators and Bounds Controls on all collocated cues
            Debug.Log("Unlocking gestures.");
            foreach (GameObject cue in visualCues)
            {
                if (cue.GetComponent<ObjectManipulator>() != null)
                {
                    cue.GetComponent<ObjectManipulator>().enabled = true;
                }

                if (cue.GetComponent<BoundsControl>() != null)
                {
                    cue.GetComponent<BoundsControl>().enabled = true;
                }
            }
            gestureLock = false;
            textToSpeech.StartSpeaking("Gestures unlocked.");

        }
    }

    public void ToggleDistanceCap ()
    {
        if (distanceCap)
        {
            Camera.main.farClipPlane = 1000;
            distanceCap = false;
            Debug.Log("Display distance uncapped.");
            //textToSpeech.StartSpeaking("Display distance uncapped.");
        }

        else
        {
            Camera.main.farClipPlane = maxDisplayDistance;
            distanceCap = true;
            Debug.Log("Display distance capped.");
            //textToSpeech.StartSpeaking("Display distance capped.");
        }
    }

    public void AdjustHighObstHeight (float heightAdjust)
    {
        //This needs to both adjust current cue height, as well as tell the QR codes script to adjust height when moving cues.
        //The latter is done in QRCodes_AROA - it checks highObstHeight on Obstacle Manager.
        highObstHeight += heightAdjust;
        foreach (Transform Cue in cuesParent.transform)
        {
            if (Cue.name.Contains("High")) {
                Cue.position = new Vector3(Cue.position.x, Cue.position.y + heightAdjust, Cue.position.z);
            }
        }
        
        Debug.Log("High obstacle height is now " + Mathf.Round(highObstHeight * 39.37f) + " inches.");

    }
}
