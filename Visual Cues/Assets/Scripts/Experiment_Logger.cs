using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality;
using System;
using System.IO;
using UnityEngine.Windows.Speech;
using QRTracking;
using Microsoft.MixedReality.Toolkit.Audio;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class Experiment_Logger : MonoBehaviour
{

    public GameObject Camera;
    public ObstacleManager obstacleManager;
    public QRCodes_AROA qRCodes_AROA;
    public TextToSpeech textToSpeech;
    public GameObject HUDFrame; //Frame containing HUD cues
    [HideInInspector]
    public List<GameObject> HUDCues; //All HUD cue objects

    [HideInInspector]
    public string cueCondition = "Default Condition";

    [HideInInspector]
    public string layout = "Default Layout";
    private float startTime;
    private float endTime;

    public bool loggingInProcess = false;

    private string filePath;
    private string filePath2;
    private string fileName;
    private string fileName2;
    public bool forward = true; //Forward if true, backward if false
    private Vector3 tempPos;
    private Vector3 tempRot;
    private float tempTime;
    private float timeStamp;
    private bool eyeTrackingEnabled;
    private bool eyeTrackingDataValid;
    private Vector3 gazeDirection;
    private Vector3 eyeMovement;

    private bool northOn;
    private bool eastOn;
    private bool southOn;
    private bool westOn;

  //private PTDataFileWriter dataWriter = new PTDataFileWriter();


    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform Cue in HUDFrame.transform)
        {
            HUDCues.Add(Cue.gameObject);
        }

        Debug.Log("HUD Cues length: " + HUDCues.Count);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Debug.Log("Is eye tracking enabled: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabled);
        Debug.Log("Is eye calibration valid: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeCalibrationValid);
        Debug.Log("Is eye tracking enabled and valid: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabledAndValid);
        Debug.Log("Is eye tracking data valid: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingDataValid);
        Debug.Log("Gaze origin: " + CoreServices.InputSystem.EyeGazeProvider.GazeOrigin.ToString());
        Debug.Log("Camera forward vector: " + Camera.transform.forward.ToString());
        Debug.Log("Camera euler angles: " + Camera.transform.rotation.eulerAngles.ToString());
        Debug.Log("Gaze direction: " + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.ToString());
        Debug.Log("Eye movement: " + (Camera.transform.forward - CoreServices.InputSystem.EyeGazeProvider.GazeDirection).ToString());
        */

        //Set cue condition
        if (obstacleManager.collocatedCuesOn && obstacleManager.hudCuesOn)
            cueCondition = "Combined";
        else if (obstacleManager.collocatedCuesOn && !obstacleManager.hudCuesOn)
            cueCondition = "Collocated";
        else if (!obstacleManager.collocatedCuesOn && obstacleManager.hudCuesOn)
            cueCondition = "HUD";
        else
            cueCondition = "No Cues";
        //Debug.Log("Cue condition: " + cueCondition);

        //Get obstacle layout
        //layout = qRCodes_AROA.layout; //Now passed directly from QRCodes_AROA when a QR code is scanned
        //Debug.Log("Layout: " + layout);


        //Check if each of the HUD cues is on.
        foreach (GameObject Cue in HUDCues)
        {
            if (Cue.name == "HUD Cue North")
            {
                if (Cue.activeSelf)
                    northOn = true;
                else
                    northOn = false;
            }

            else if (Cue.name == "HUD Cue East")
            {
                if (Cue.activeSelf)
                    eastOn = true;
                else
                    eastOn = false;
            }

            else if (Cue.name == "HUD Cue South")
            {
                if (Cue.activeSelf)
                    southOn = true;
                else
                    southOn = false;
            }

            else if (Cue.name == "HUD Cue West")
            {
                if (Cue.activeSelf)
                    westOn = true;
                else
                    westOn = false;
            }
        }

        if (loggingInProcess)
        {

            tempPos = Camera.transform.position;
            tempRot = Camera.transform.rotation.eulerAngles;
            tempTime = Time.time - startTime;
            eyeTrackingEnabled = CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabled;
            eyeTrackingDataValid = CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingDataValid;
            gazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
            eyeMovement = Camera.transform.forward - gazeDirection;


            using (TextWriter writer = File.AppendText(filePath2))
            {
                writer.WriteLine("Update; " + cueCondition + "; " + layout + "; " + tempTime + "; " +
                    tempPos.x + "; " + tempPos.y + "; " + tempPos.z + "; " +
                    tempRot.x + "; " + tempRot.y + "; " + tempRot.z + "; " +
                    eyeTrackingEnabled + "; " + eyeTrackingDataValid + "; " +
                    gazeDirection.x + "; " + gazeDirection.y + "; " + gazeDirection.z + "; " +
                    eyeMovement.x + "; " + eyeMovement.y + "; " + eyeMovement.z + "; " +
                    northOn + "; " + eastOn + "; " + southOn + "; " + westOn);
            }
        }

    }

    public void BeginLogging()
    {
        if (!loggingInProcess)
        {
            loggingInProcess = true;

            Debug.Log("Beginning logging.");
            textToSpeech.StartSpeaking("Beginning logging.");

            // Create filename with experiment conditions

            //Track start time
            startTime = Time.time;

            //Set timestamp and path name
            string timeStamp = string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now);
            if (forward)
            {
                fileName = timeStamp + '_' + cueCondition + '_' + layout + '_' + "forward" + "_fixed.txt";
                fileName2 = timeStamp + '_' + cueCondition + '_' + layout + '_' + "forward" + "_update.txt";
            }

            else
            {
                fileName = timeStamp + '_' + cueCondition + '_' + layout + '_' + "backward" + "_fixed.txt";
                fileName2 = timeStamp + '_' + cueCondition + '_' + layout + '_' + "backward" + "_update.txt";
            }

            Debug.Log("Filename: " + fileName);
            Debug.Log("Filename 2: " + fileName2);
            filePath = Path.Combine(Application.persistentDataPath, fileName);
            filePath2 = Path.Combine(Application.persistentDataPath, fileName2);
            Debug.Log("File path: " + filePath);
            Debug.Log("File path: " + filePath2);


            //define variable values at start
            tempPos = Camera.transform.position;
            tempRot = Camera.transform.rotation.eulerAngles.normalized;
            gazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized;

            // create output file and write header
            //System.IO.File.WriteAllText(filePath, fileName);

            using (TextWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine("Logging begun. Format: \n" +
                    "Log Type; Cue Condition; Layout; Time; Position X; Position Y; Position Z; " +
                    "Rotation X; Rotation Y; Rotation Z; " +
                    "Eye Tracking Enabled (true/false); " + "Eye Tracking Data Valid (true/false); " + 
                    "Gaze Direction X; Gaze Direction Y; Gaze Direction Z; " +
                    "Eye Movement X; Eye Movement Y; Eye Movement Z; " +
                    "HUD Cue Up; HUD Cue Right; HUD Cue Down; HUD Cue Left");
            }
        }
 
        else
        {
            Debug.Log("Logging is currently in process.");
        }


    }

    public void EndLogging()
    {
        if (loggingInProcess)
        {
            loggingInProcess = false;

            Debug.Log("Ending logging.");
            textToSpeech.StartSpeaking("Ending logging.");

            using (TextWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine("Logging ended at " + tempTime);
            }

            using (TextWriter writer = File.AppendText(filePath2))
            {
                writer.WriteLine("Logging ended at " + tempTime);
            }
            
            //change from forward to backward or vice versa
            forward = !forward;
        }

        else
            Debug.Log("Logging not currently in process.");

    }

    void FixedUpdate()
    {
        //Set cue condition
        if (obstacleManager.collocatedCuesOn && obstacleManager.hudCuesOn)
            cueCondition = "Combined";
        else if (obstacleManager.collocatedCuesOn && !obstacleManager.hudCuesOn)
            cueCondition = "Collocated";
        else if (!obstacleManager.collocatedCuesOn && obstacleManager.hudCuesOn)
            cueCondition = "HUD";
        else
            cueCondition = "No Cues";
        //Debug.Log("Cue condition: " + cueCondition);

        //Get obstacle layout
        //layout = qRCodes_AROA.layout; //Now passed directly from QRCodes_AROA when a QR code is scanned
        //Debug.Log("Layout: " + layout);


        //Check if each of the HUD cues is on.
        foreach (GameObject Cue in HUDCues)
        {
            if (Cue.name == "HUD Cue North")
            {
                if (Cue.activeSelf)
                    northOn = true;
                else
                    northOn = false;
            }

            else if (Cue.name == "HUD Cue East")
            {
                if (Cue.activeSelf)
                    eastOn = true;
                else
                    eastOn = false;
            }

            else if (Cue.name == "HUD Cue South")
            {
                if (Cue.activeSelf)
                    southOn = true;
                else
                    southOn = false;
            }

            else if (Cue.name == "HUD Cue West")
            {
                if (Cue.activeSelf)
                    westOn = true;
                else
                    westOn = false;
            }
        }

        if (loggingInProcess)
        {

            tempPos = Camera.transform.position;
            tempRot = Camera.transform.rotation.eulerAngles;
            tempTime = Time.time - startTime;
            eyeTrackingEnabled = CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabled;
            eyeTrackingDataValid = CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingDataValid;
            gazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
            eyeMovement = Camera.transform.forward - gazeDirection;


            using (TextWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine("Fixed; " + cueCondition + "; " + layout + "; " + tempTime + "; " +
                    tempPos.x + "; " + tempPos.y + "; " + tempPos.z + "; " +
                    tempRot.x + "; " + tempRot.y + "; " + tempRot.z + "; " + 
                    eyeTrackingEnabled + "; " + eyeTrackingDataValid + "; " + 
                    gazeDirection.x + "; " + gazeDirection.y + "; " + gazeDirection.z + "; " +
                    eyeMovement.x + "; " + eyeMovement.y + "; " + eyeMovement.z + "; " +  
                    northOn + "; " + eastOn + "; " + southOn + "; " + westOn);
            }
        }
    }


}
