﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;



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

    //Maximum multiplier for cue width - will scale to it as distance to obstacle shrinks
    public float cueWidthMaxMultiplier = 2f;

    //Minimum and maximum Obst distance at which to show cues
    public float minDist = 0f;
    public float maxDist = 2.5f;

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




    // Start is called before the first frame update
    void Awake()
    {
        foreach (Transform Cue in HUDFrame.transform)
        {
            HUDCues.Add(Cue.gameObject);
        }

        obstacleMask = LayerMask.GetMask("Obstacles");
    }

    // Update is called once per frame
    void Update()
    {
        ObstInfos.Clear();
        if (debugText.activeSelf)
            debugText.GetComponent<TextMeshProUGUI>().text = "Debug text: ";

        CueCast();
        ActivateHUD();
        /*
        Debug.Log("List of obstacles: ");
        foreach (ObstInfo obst in ObstInfos)
        {
            Debug.Log(obst.ObstName + "\n");
        }
        */
    }

    public void CueCast()
    {
        //Assesses locations of obstacles using raycasts

        //Capture camera's location and orientation
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;




        if (angleInterval >= 5)
        {
            //Cast at intervals from -90 degrees to positive 90 degrees X and Y
            for (float xAngle = -80; xAngle <= 80; xAngle += angleInterval)
            {
                if (Mathf.Cos(Mathf.Deg2Rad * xAngle) > 0)
                {
                    //int yCount = 0; 
                    //Adjust y angle interval based on x angle so you get equivalent sampling over a given area
                    for (float yAngle = -80; yAngle <= 80; yAngle += angleInterval / Mathf.Cos(Mathf.Deg2Rad * xAngle))
                    {
                        //Debug.Log("Y angle: " + yAngle);
                        //yCount++;
                        //Determine new ray direction based on a certain angle off of the gaze direction
                        var rayDirection = gazeDirection;
                        rayDirection = Quaternion.AngleAxis(xAngle, Camera.main.transform.right) * rayDirection;
                        rayDirection = Quaternion.AngleAxis(yAngle, Camera.main.transform.up) * rayDirection;


                        //Debug rays
                        if (DebugRays)
                        {
                            if (Mathf.Abs(xAngle) <= minAngle && Mathf.Abs(yAngle) <= minAngle)
                                Debug.DrawRay(headPosition, rayDirection * maxDist, Color.red);
                            else
                                Debug.DrawRay(headPosition, rayDirection * maxDist, Color.green);
                        }

                        //Cast ray
                        RaycastHit hit;
                        Physics.SphereCast(headPosition, sphereRadius, rayDirection, out hit, maxDist, obstacleMask);


                        //If ray hits something, log it
                        if (hit.transform != null)
                        {
                            //Debug.Log("Hit obstacle: " + hit.transform.parent.gameObject.ToString());
                            //If it hit an obstacle, update its information in the obstacle info list
                            bool newObst = true;

                            foreach (ObstInfo obst in ObstInfos)
                            {
                                if (obst.ObstObject == hit.transform.parent.gameObject)
                                {
                                    //Debug.Log("Existing obstacle found: " + hit.transform.parent.gameObject.ToString());
                                    newObst = false;
                                    //Check if the hit has a shorter or larger distance and angle than recorded
                                    //if (hit.distance >= obst.ObstMaxDist)
                                    //    obst.ObstMaxDist = hit.distance;

                                    /* Trying to use distance from camera to center of object to maintain consistency
                                    if (hit.distance <= obst.ObstMinDist)
                                        obst.ObstMinDist = hit.distance;
                                    */
                                    obst.ObstMinDist = (headPosition - obst.ObstObject.transform.position).magnitude;

                                    //Record X and Y angles
                                    if (!obst.ObstXAngles.Contains(xAngle))
                                        obst.ObstXAngles.Add(xAngle);

                                    if (!obst.ObstYAngles.Contains(yAngle))
                                        obst.ObstYAngles.Add(yAngle);

                                }


                            }

                            //Debug.Log("Onhit triggered");
                            //If the obstacle was not found, create a new one
                            if (newObst)
                            {
                                //ObstInfos.Add(new ObstInfo(hit.transform.parent.gameObject.ToString(), hit.transform.parent.gameObject, hit.distance, hit.distance, xAngle, yAngle));
                                float minDist = (headPosition - hit.transform.parent.transform.position).magnitude; //Using distance from head position to object to keep things consistent
                                //ObstInfos.Add(new ObstInfo(hit.transform.parent.gameObject.ToString(), hit.transform.parent.gameObject, minDist, hit.distance, xAngle, yAngle));
                                ObstInfos.Add(new ObstInfo(hit.transform.parent.gameObject.ToString(), hit.transform.parent.gameObject, minDist, xAngle, yAngle));


                                //Debug.Log("New obstacle found: " + hit.transform.parent.gameObject.ToString());
                            }
                        }
                    }
                    //Debug.Log("Y count for X angle " + xAngle + ": " + yCount);
                }
            }
            if (debugText.activeSelf)
            {
                debugText.GetComponent<TextMeshProUGUI>().text += ("\nObstacles in obstacle list: " + ObstInfos.Count.ToString());

                foreach (ObstInfo obst in ObstInfos)
                {
                    debugText.GetComponent<TextMeshProUGUI>().text += ("\n" + obst.ObstName);
                    debugText.GetComponent<TextMeshProUGUI>().text += ("\nX angles: ");
                    foreach (float angle in obst.ObstXAngles)
                        debugText.GetComponent<TextMeshProUGUI>().text += Mathf.Round(angle) + ", ";
                    debugText.GetComponent<TextMeshProUGUI>().text += ("\nY angles: ");
                    foreach (float angle in obst.ObstYAngles)
                        debugText.GetComponent<TextMeshProUGUI>().text += Mathf.Round(angle) + ", ";
                    debugText.GetComponent<TextMeshProUGUI>().text += ("\nMinimum hit distance: " + Mathf.Round(obst.ObstMinDist * 100) / 100);
                }
            }

        }

        //Print list of obstacles
        //Debug.Log("Number of obstacles: " + ObstInfos.Count);

    }

    public void ActivateHUD ()
    {
        //Assesses which cues should be illuminated and their appropriate size
        foreach (ObstInfo obst in ObstInfos)
        {
            /*
            //Calculate minimum, maximum, and absolute minimum X and Y angles for each obstacle
            float minX = 100;
            float minY = 100;
            float maxX = -100;
            float maxY = -100;
            float absMinX = 100;
            float absMinY = 100;

            foreach (float x in obst.ObstXAngles)
            {
                if (x <= minX)
                    minX = x;

                if (x >= maxX)
                    maxX = x;

                if (Mathf.Abs(absMinX) >= Mathf.Abs(x))
                    absMinX = Mathf.Abs(x);
            }

            foreach (float y in obst.ObstYAngles)
            {
                if (y <= minY)
                    minY = y;

                if (y >= maxY)
                    maxY = y;

                if (Mathf.Abs(absMinY) >= Mathf.Abs(y))
                    absMinY = Mathf.Abs(y);
            }
            */

            foreach (float x in obst.ObstXAngles)
            {
                if (x <= obst.ObstMinX)
                    obst.ObstMinX = x;

                else if (x >= obst.ObstMaxX)
                    obst.ObstMaxX = x;

                if (Mathf.Abs(x) <= Mathf.Abs(obst.ObstMinAbsX))
                    obst.ObstMinAbsX = Mathf.Abs(x);
            }

            foreach (float y in obst.ObstYAngles)
            {
                if (y <= obst.ObstMinY)
                    obst.ObstMinY = y;

                else if (y >= obst.ObstMaxY)
                    obst.ObstMaxY = y;

                if (Mathf.Abs(y) <= Mathf.Abs(obst.ObstMinAbsY))
                    obst.ObstMinAbsY = Mathf.Abs(y);
            }

        }


            //For each cue, scan the obstacle info list and see if it should be on or not
        foreach (GameObject Cue in HUDCues)
        {
            //Debug.Log("Cue name: " + Cue.name);
            //Cues should be off and minimum width by default
            Cue.SetActive(false);
            float cueMultiplier = 1;
            Cue.transform.localScale = Vector3.one;


            if (HUDCalibration) 
            {
                //Keep all cues visible and reset to default positions
                Cue.SetActive(true);
                if (Cue.name == "HUD Cue East")
                    Cue.transform.localPosition = new Vector3(0.5f - 0.05f * cueMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);
                else if (Cue.name == "HUD Cue South")
                    Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, -0.5f + 0.05f * cueMultiplier, Cue.transform.localPosition.z);
                else if (Cue.name == "HUD Cue West")
                    Cue.transform.localPosition = new Vector3(-0.5f + 0.05f * cueMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);
                else if (Cue.name == "HUD Cue North")
                    Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, 0.5f - 0.05f * cueMultiplier, Cue.transform.localPosition.z);
            }

            else
            {
                foreach (ObstInfo obst in ObstInfos)
                {
                    //Check whether the obstacle is behind the user
                    bool isFront = true;
                    var heading = Vector3.Normalize(obst.ObstObject.transform.position - Camera.main.transform.position);
                    float dot = Vector3.Dot(heading, Camera.main.transform.forward);
                    if (dot < 0)
                        isFront = false;

                    //Debug.Log("Dot for " + obst.ObstName + ": " + (Mathf.Round(dot * 100))/100);

                    //Check that absolute value of X or Y exceed the min angle, and that it's not behind the user
                    if ((obst.ObstMinAbsX > minAngle || obst.ObstMinAbsY > minAngle) && isFront)
                    {
                        //Check for each HUD cue portion whether or not to light up. Set cue multiplier based on closest object.
                        if (Cue.name == "HUD Cue East" && obst.ObstMinY > minAngle && !(obst.ObstMaxY < minAngle * -1)) //&& obst.ObstMinAbsX <= minAngle)
                        {
                            Cue.SetActive(true);
                            float tempMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, obst.ObstMinDist);

                            if (tempMultiplier >= cueMultiplier)
                            {
                                cueMultiplier = tempMultiplier;
                                Cue.transform.localScale = new Vector3(Cue.transform.localScale.x * cueMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                                Cue.transform.localPosition = new Vector3(0.5f - 0.05f * cueMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);
                            }
                            if (debugText.activeSelf)
                                debugText.GetComponent<TextMeshProUGUI>().text += "\nEast Cue width multiplier: " + Mathf.Round(cueMultiplier * 100) / 100;

                        }

                        else if (Cue.name == "HUD Cue South" && obst.ObstMinX > minAngle && !(obst.ObstMaxX < minAngle * -1)) //&& obst.ObstMinAbsY <= minAngle)
                        {
                            Cue.SetActive(true);
                            float tempMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, obst.ObstMinDist);
                            if (tempMultiplier >= cueMultiplier)
                            {
                                cueMultiplier = tempMultiplier;
                                Cue.transform.localScale = new Vector3(Cue.transform.localScale.x * cueMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                                Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, -0.5f + 0.05f * cueMultiplier, Cue.transform.localPosition.z);
                                //Debug.Log("South cue on. Width multiplier: " + cueMultiplier);
                            }
                            if (debugText.activeSelf)
                                debugText.GetComponent<TextMeshProUGUI>().text += "\nSouth Cue width multiplier: " + Mathf.Round(cueMultiplier * 100) / 100;


                        }

                        else if (Cue.name == "HUD Cue West" && obst.ObstMaxY < minAngle * -1 && !(obst.ObstMinY > minAngle))  //&& obst.ObstMinAbsX <= minAngle)
                        {
                            Cue.SetActive(true);
                            float tempMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, obst.ObstMinDist);
                            if (tempMultiplier >= cueMultiplier)
                            {
                                cueMultiplier = tempMultiplier;
                                Cue.transform.localScale = new Vector3(Cue.transform.localScale.x * cueMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                                Cue.transform.localPosition = new Vector3(-0.5f + 0.05f * cueMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);

                                //Debug.Log("West cue on. Width multiplier: " + cueMultiplier);
                            }
                            if (debugText.activeSelf)
                                debugText.GetComponent<TextMeshProUGUI>().text += "\nWest Cue width multiplier: " + Mathf.Round(cueMultiplier * 100) / 100;

                        }

                        else if (Cue.name == "HUD Cue North" && obst.ObstMaxX < minAngle * -1 && !(obst.ObstMinX > minAngle)) //&& obst.ObstMinAbsY <= minAngle)
                        {
                            Cue.SetActive(true);
                            float tempMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, obst.ObstMinDist);
                            if (tempMultiplier >= cueMultiplier)
                            {
                                cueMultiplier = tempMultiplier;
                                Cue.transform.localScale = new Vector3(Cue.transform.localScale.x * cueMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                                Cue.transform.localPosition = new Vector3(Cue.transform.localPosition.x, 0.5f - 0.05f * cueMultiplier, Cue.transform.localPosition.z);
                                //Debug.Log("North cue on. Width multiplier: " + cueMultiplier);
                            }
                            if (debugText.activeSelf)
                                debugText.GetComponent<TextMeshProUGUI>().text += "\nNorth Cue width multiplier: " + Mathf.Round(cueMultiplier * 100) / 100;

                        }
                    }
                }
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
        //var offsetVector = HUDFrame.GetComponent<SolverHandler>().AdditionalOffset;
        //HUDFrame.GetComponent<SolverHandler>().AdditionalOffset = new Vector3(offsetVector.x + xShift, offsetVector.y, offsetVector.z);
        var curPos = HUDFrame.transform.localPosition;
        HUDFrame.transform.localPosition = new Vector3(curPos.x + xShift, curPos.y, curPos.z);
    }

    public void shiftHUDUp (float yShift)
    {
        //var offsetVector = HUDFrame.GetComponent<SolverHandler>().AdditionalOffset;
        //HUDFrame.GetComponent<SolverHandler>().AdditionalOffset = new Vector3(offsetVector.x, offsetVector.y + yShift, offsetVector.z);
        var curPos = HUDFrame.transform.localPosition;
        HUDFrame.transform.localPosition = new Vector3(curPos.x, curPos.y + yShift, curPos.z);
    }

    public void scaleHUD (float multiplier)
    {
        var currentScale = HUDFrame.transform.localScale;
        HUDFrame.transform.localScale = new Vector3(currentScale.x * multiplier, currentScale.y, currentScale.z);
    }

    public class ObstInfo
    {
        public string ObstName { get; set; }
        public GameObject ObstObject { get; set; }
        public float ObstMinDist { get; set; }
        //public float ObstMaxDist { get; set; }
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
            //ObstMaxDist = 0;
            //ObstMinX = 0;
            //ObstMinY = 0;
            ObstXAngles = null;
            ObstYAngles = null;
        }

        //public ObstInfo (string obstName, GameObject obstObject, float minDist, float maxDist, float angleX, float angleY)
        public ObstInfo (string obstName, GameObject obstObject, float minDist,  float angleX, float angleY) //Got rid of max distance for obstacles

        {
            ObstName = obstName;
            ObstObject = obstObject;
            ObstMinDist = minDist;
            //ObstMaxDist = maxDist;
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
}