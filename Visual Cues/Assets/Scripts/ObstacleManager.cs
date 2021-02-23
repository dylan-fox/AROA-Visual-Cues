using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Greyman;

public class ObstacleManager : MonoBehaviour
{
    //<summary>A script to manage obstacles and visual cues.</summary>

    private bool obstaclesOn = true;
    public GameObject[] digitalObstacles;
    public List<Vector3> obstacleStartingLocations;
    public List<Quaternion> obstacleStartingRotations;
    public List<Vector3> obstacleStartingScales;

    private bool cuesOn = true;
    public GameObject[] visualCues;
    public List<Vector3> cueStartingLocations;
    public List<Quaternion> cueStartingRotations;
    public List<Vector3> cueStartingScales;

    public GameObject HUDManager;


    // Start is called before the first frame update
    void Start()
    {
        //Get digital obstacles and record original positions
        digitalObstacles = GameObject.FindGameObjectsWithTag("digital_obstacle");

        Debug.Log(digitalObstacles.Length + " digital obstacles added to list.");

        foreach (GameObject Obstacle in digitalObstacles)
        {
            //Debug.Log("Obstacle " + Obstacle.name + " added at " + Obstacle.transform.position);
            obstacleStartingLocations.Add(Obstacle.transform.position);
            obstacleStartingRotations.Add(Obstacle.transform.rotation);
            obstacleStartingScales.Add(Obstacle.transform.localScale);
        }


        //Get visual cues and record original positions
        visualCues = GameObject.FindGameObjectsWithTag("visual_cue");

        Debug.Log(visualCues.Length + " visual cues added to list.");

        foreach (GameObject Cue in visualCues)
        {
            //Debug.Log("Cue " + Cue.name + " added at " + Cue.transform.position);
            cueStartingLocations.Add(Cue.transform.position);
            cueStartingRotations.Add(Cue.transform.rotation);
            cueStartingScales.Add(Cue.transform.localScale);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DigitalObstacleToggle ()
    {
        Debug.Log("Toggling digital obstacles."); 

        if (obstaclesOn)
        {
            Debug.Log("Obstacles off.");
            foreach (GameObject Obstacle in digitalObstacles)
            {
                Obstacle.SetActive(false);
            }

            obstaclesOn = false;
        }

        else
        {
            Debug.Log("Obstacles on.");
            foreach (GameObject Obstacle in digitalObstacles)
            {
                Obstacle.SetActive(true);
            }

            obstaclesOn = true;
        }
    }

    public void VisualCuesToggle()
    {
        Debug.Log("Toggling visual cues.");

        if (cuesOn)
        {
            Debug.Log("Cues off.");
            foreach (GameObject Cue in visualCues)
            {
                Cue.SetActive(false);
            }

            HUDManager.GetComponent<HUDIndicator>().GetComponent<HUDIndicatorManagerVR>().HideIndicators();

            cuesOn = false;
        }

        else
        {
            Debug.Log("Cues on.");
            foreach (GameObject Cue in visualCues)
            {
                Cue.SetActive(true);
            }

            HUDManager.GetComponent<HUDIndicator>().GetComponent<HUDIndicatorManagerVR>().ShowIndicators();

            cuesOn = true;
        }
    }

    public void ResetPositions()
    {
        Debug.Log("Resetting positions.");

        int obstacleCount = 0;
        int cueCount = 0;

        foreach (GameObject Obstacle in digitalObstacles)
        {
            Obstacle.transform.position = obstacleStartingLocations[obstacleCount];
            Obstacle.transform.rotation = obstacleStartingRotations[obstacleCount];
            Obstacle.transform.localScale = obstacleStartingScales[obstacleCount];
            obstacleCount++;
        }

        foreach (GameObject Cue in visualCues)
        {
            Cue.transform.position = cueStartingLocations[cueCount];
            Cue.transform.rotation = cueStartingRotations[cueCount];
            Cue.transform.localScale = cueStartingScales[cueCount];
            cueCount++;
        }
    }
}
