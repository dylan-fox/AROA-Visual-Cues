using System.Collections;
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
        ObstInfos.Clear();
        debugText.GetComponent<TextMeshProUGUI>().text = "Debug text: ";

        CueCast();
        ActivateHUD();
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
                for (float yAngle = -80; yAngle <= 80; yAngle += angleInterval)
                {
                    //Determine new ray direction based on a certain angle off of the gaze direction
                    var rayDirection = gazeDirection;
                    rayDirection = Quaternion.AngleAxis(xAngle, Camera.main.transform.right) * rayDirection;
                    rayDirection = Quaternion.AngleAxis(yAngle, Camera.main.transform.up) * rayDirection;


                    //Debug rays
                    if (Mathf.Abs(xAngle) <= minAngle && Mathf.Abs(yAngle) <= minAngle)
                        Debug.DrawRay(headPosition, rayDirection * maxDist, Color.red);
                    else
                        Debug.DrawRay(headPosition, rayDirection * maxDist, Color.green);

                    //Cast ray
                    RaycastHit hit;
                    Physics.SphereCast(headPosition, sphereRadius, rayDirection, out hit, maxDist);


                    //If ray hits something, log it
                    if (hit.transform != null)
                    {
                        Debug.Log("Hit obstacle: " + hit.transform.parent.gameObject.ToString());
                        //If it hit an obstacle, update its information in the obstacle info list
                        bool newObst = true;

                        foreach (ObstInfo obst in ObstInfos)
                        {
                            if (obst.ObstObject == hit.transform.parent.gameObject)
                            {
                                //Debug.Log("Existing obstacle found: " + hit.transform.parent.gameObject.ToString());
                                newObst = false;
                                //Check if the hit has a shorter or larger distance and angle than recorded
                                if (hit.distance >= obst.ObstMaxDist)
                                    obst.ObstMaxDist = hit.distance;

                                if (hit.distance <= obst.ObstMinDist)
                                    obst.ObstMinDist = hit.distance;

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
                            ObstInfos.Add(new ObstInfo(hit.transform.parent.gameObject.ToString(), hit.transform.parent.gameObject, hit.distance, hit.distance, xAngle, yAngle));
                            //Debug.Log("New obstacle found: " + hit.transform.parent.gameObject.ToString());
                        }
                    }
                }
            }
            debugText.GetComponent<TextMeshProUGUI>().text += ("\nObstacles in obstacle list: " + ObstInfos.Count.ToString());
            foreach (ObstInfo obst in ObstInfos)
            {
                debugText.GetComponent<TextMeshProUGUI>().text += ("\n" + obst.ObstName.ToString());
                debugText.GetComponent<TextMeshProUGUI>().text += ("\nX angles: " + string.Join(", ", obst.ObstXAngles));
                debugText.GetComponent<TextMeshProUGUI>().text += ("\nY angles: " + string.Join(", ", obst.ObstYAngles));
                debugText.GetComponent<TextMeshProUGUI>().text += ("\nMinimum hit distance: " + obst.ObstMinDist);
            }
        }

        //Print list of obstacles
        //Debug.Log("Number of obstacles: " + ObstInfos.Count);

    }

    public void ActivateHUD ()
    {
        //Assesses which cues should be illuminated and their appropriate size

        //For each cue, scan the obstacle info list and see if it should be on or not
        foreach (GameObject Cue in HUDCues)
        {
            //Debug.Log("Cue name: " + Cue.name);
            //Cues should be off and minimum width by default
            Cue.SetActive(false);
            float cueMultiplier = 1;
            Cue.transform.localScale = Vector3.one;

            foreach (ObstInfo obst in ObstInfos)
            {
                //Calculate minimum, maximum, and absolute minimum X and Y angles for each obstacle
                float minX = 1000;
                float minY = 1000;
                float maxX = -1000;
                float maxY = -1000;
                float absMinX = 1000;
                float absMinY = 1000;

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

                //Debug.Log("X angles: " + string.Join(", ", obst.ObstXAngles));
                //Debug.Log("Y angles: " + string.Join(", ", obst.ObstYAngles));



                //Debug.Log("Obstacle name: " + obst.ObstName + "; min X: " + minX + "; max X: " + maxX + "; absMinX: " + absMinX + ";min Y: " + minY + "; max Y: " + maxY + "; absMinY: " + absMinY);

                //Check that absolute value of X or Y exceed the min angle
                if (absMinX > minAngle || absMinY > minAngle)
                {
                    //Check for each HUD cue portion whether or not to light up. Set cue multiplier based on closest object.
                    if (Cue.name == "HUD Cue East" && maxY > minAngle && absMinX <= 30)
                    {
                        Cue.SetActive(true);
                        float tempMultiplier = CalculateCueMultiplier(cueWidthMaxMultiplier, minDist, maxDist, obst.ObstMinDist);

                        if (tempMultiplier >= cueMultiplier)
                        {
                            cueMultiplier = tempMultiplier;
                            Cue.transform.localScale = new Vector3(Cue.transform.localScale.x * cueMultiplier, Cue.transform.localScale.y, Cue.transform.localScale.z);
                            Cue.transform.localPosition = new Vector3(0.5f - 0.05f * cueMultiplier, Cue.transform.localPosition.y, Cue.transform.localPosition.z);
                        }
                        debugText.GetComponent<TextMeshProUGUI>().text += "\nEast Cue width multiplier: " + cueMultiplier;

                    }

                    else if (Cue.name == "HUD Cue South" && maxX > minAngle && absMinY <= 30)
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
                        debugText.GetComponent<TextMeshProUGUI>().text += "\nSouth Cue width multiplier: " + cueMultiplier;


                    }

                    else if (Cue.name == "HUD Cue West" && minY < minAngle * -1 && absMinX <=30)
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
                        debugText.GetComponent<TextMeshProUGUI>().text += "\nWest Cue width multiplier: " + cueMultiplier;

                    }

                    else if (Cue.name == "HUD Cue North" && minX < minAngle * -1 && absMinY <= 30)
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
                        debugText.GetComponent<TextMeshProUGUI>().text += "\nNorth Cue width multiplier: " + cueMultiplier;

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
        var offsetVector = HUDFrame.GetComponent<SolverHandler>().AdditionalOffset;
        HUDFrame.GetComponent<SolverHandler>().AdditionalOffset = new Vector3(offsetVector.x + xShift, offsetVector.y, offsetVector.z);
    }

    public void shiftHUDUp (float yShift)
    {
        var offsetVector = HUDFrame.GetComponent<SolverHandler>().AdditionalOffset;
        HUDFrame.GetComponent<SolverHandler>().AdditionalOffset = new Vector3(offsetVector.x, offsetVector.y + yShift, offsetVector.z);
    }

    public void scaleHUD (float multiplier)
    {
        HUDFrame.transform.localScale *= multiplier;
    }

    public class ObstInfo
    {
        public string ObstName { get; set; }
        public GameObject ObstObject { get; set; }
        public float ObstMinDist { get; set; }
        public float ObstMaxDist { get; set; }
        public List<float> ObstXAngles { get; set; }
        public List<float> ObstYAngles { get; set; }
        /*
         * public float ObstMinAngleX { get; set; }
        public float ObstMaxAngleX { get; set; }
        public float ObstMinAngleY { get; set; }
        public float ObstMaxAngleY { get; set; }
        */
        public ObstInfo()
        {
            ObstName = "unknown";
            ObstObject = null;
            ObstMinDist = 0;
            ObstMaxDist = 0;
            //ObstMinAngleX = 0;
            //ObstMinAngleY = 0;
            ObstXAngles = null;
            ObstYAngles = null;
        }

        public ObstInfo (string obstName, GameObject obstObject, float minDist, float maxDist, float angleX, float angleY)
        {
            ObstName = obstName;
            ObstObject = obstObject;
            ObstMinDist = minDist;
            ObstMaxDist = maxDist;
            ObstXAngles = new List<float>();
            ObstYAngles = new List<float>();
            ObstXAngles.Add(angleX);
            ObstYAngles.Add(angleY);
        }


    }
}
