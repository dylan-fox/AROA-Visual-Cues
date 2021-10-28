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

    private string cueCondition = "Default Condition";
    private string layout = "Default Layout";
    public float startTime;
    public float endTime;

    public bool beganLogging = false;

    private string filePath;
    private string fileName;
    private Vector3 tempPos;
    private Vector3 tempRot;
    private float tempTime;
    private float timeStamp;
    private bool eyeTrackingEnabled;
    private Vector3 gazeDirection;

  //private PTDataFileWriter dataWriter = new PTDataFileWriter();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Is eye tracking enabled: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabled);
        Debug.Log("Is eye calibration valid: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeCalibrationValid);
        Debug.Log("Is eye tracking enabled and valid: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabledAndValid);
        Debug.Log("Is eye tracking data valid: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingDataValid);
        Debug.Log("Gaze origin: " + CoreServices.InputSystem.EyeGazeProvider.GazeOrigin.ToString());
        Debug.Log("Camera forward vector (not normalized): + " + Camera.transform.forward.ToString());
        Debug.Log("Camera forward vector (normalized): " + Camera.transform.forward.normalized.ToString());
        Debug.Log("Camera direction (normalized): " + Camera.transform.rotation.eulerAngles.normalized.ToString());
        Debug.Log("Gaze direction (normalized): " + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized.ToString());
    }

    public void BeginLogging()
    {
        Debug.Log("Beginning logging.");
        textToSpeech.StartSpeaking("Beginning logging.");

        // Create filename with experiment conditions
        //Set cue condition
        if (obstacleManager.collocatedCuesOn && obstacleManager.hudCuesOn)
            cueCondition = "Combined";
        else if (obstacleManager.collocatedCuesOn && !obstacleManager.hudCuesOn)
            cueCondition = "Collocated";
        else if (!obstacleManager.collocatedCuesOn && obstacleManager.hudCuesOn)
            cueCondition = "HUD";
        else
            cueCondition = "Control";
        Debug.Log("Cue condition: " + cueCondition);

        //Get obstacle layout
        layout = qRCodes_AROA.layout;
        Debug.Log("Layout: " + layout);

        //Track start time
        startTime = Time.time;

        //Set timestamp and path name
        string timeStamp = string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now);
        fileName = timeStamp + '_' + cueCondition + '_' + layout + ".txt";
        Debug.Log("Filename: " + fileName);
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log("File path: " + filePath);


        //define variable values at start
        tempPos = Camera.transform.position;
        tempRot = Camera.transform.rotation.eulerAngles.normalized;
        gazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized;

        // create output file and write header
        //System.IO.File.WriteAllText(filePath, fileName);

        using (TextWriter writer = File.AppendText(filePath))
        {
            writer.WriteLine("Logging begun. Format: Cue Condition; Layout; Time; Position X; Y; Z; " +
                "Rotation X; Y; Z; Eye Tracking Enabled (true/false); Gaze Direction X; Y; Z");
        }

        beganLogging = true;

    }

    public void EndLogging()
    {
        Debug.Log("Ending logging.");
        textToSpeech.StartSpeaking("Ending logging.");
        beganLogging = false;
        using (TextWriter writer = File.AppendText(filePath))
        {
            writer.WriteLine("Logging ended at " + tempTime);
        }
    }

    void FixedUpdate()
    {
        if (beganLogging)
        {

            tempPos = Camera.transform.position;
            tempRot = Camera.transform.rotation.eulerAngles.normalized;
            tempTime = Time.time - startTime;
            eyeTrackingEnabled = CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabledAndValid;
            gazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized;


            using (TextWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine(cueCondition + "; " + layout + "; " + tempTime + "; " +
                    tempPos.x + "; " + tempPos.y + "; " + tempPos.z + "; " +
                    tempRot.x + "; " + tempRot.y + "; " + tempRot.z + "; " + 
                    eyeTrackingEnabled + "; " + 
                    gazeDirection.x + "; " + gazeDirection.y + "; " + gazeDirection.z + "\n");
            }
        }
    }
}
