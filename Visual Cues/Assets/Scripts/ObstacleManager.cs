using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Greyman;
using TMPro;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.UI;

public class ObstacleManager : MonoBehaviour
{
    //<summary>A script to manage obstacles and visual cues.</summary>

    //private bool obstaclesOn = true;
    //public GameObject[] digitalObstacles;
    //public List<Vector3> obstacleStartingLocations;
    //public List<Quaternion> obstacleStartingRotations;
    //public List<Vector3> obstacleStartingScales;

    private bool collocatedCuesOn = true;
    private bool hudCuesOn = true;

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

    public GameObject HUDManager;
    private HUDIndicator HUDIndicator;

    public GameObject numShownTextObject;
    //public GameObject cuePrefab; //Necessary for AddCue, not ShowCue

    [Tooltip("Number of cues to show at start.")]
    public int startingCues = 1; //
    private int numShown = 10; //Indicates how many cues are currently shown


    // Start is called before the first frame update
    void Start()
    {
        SavePositions();
        HUDIndicator = HUDManager.GetComponent<HUDIndicator>();

        //Hide cues until the number remaining matches numShown.
        for (int n = visualCues.Count; n> startingCues; n--)
        {
            HideCue();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
     //No longer necessary since collocated cues are now used as targets for HUD cues

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
    */

    public void CollocatedCuesToggle()
    {
        Debug.Log("Toggling collocated visual cues.");

        if (collocatedCuesOn)
        {
            Debug.Log("Collocated cues off.");

            cuesParent.SetActive(false);

            collocatedCuesOn = false;
        }

        else
        {
            Debug.Log("Collocated cues on.");
            cuesParent.SetActive(true);

            collocatedCuesOn = true;
        }
    }

    public void HUDCuesToggle()
    {
        Debug.Log("Toggling HUD visual cues.");

        if (hudCuesOn)
        {
            Debug.Log("HUD cues off.");
            HUDManager.GetComponent<HUDIndicator>().GetComponent<HUDIndicatorManagerVR>().HideIndicators();
            hudCuesOn = false;
        }

        else
        {
            Debug.Log("HUD cues on.");
            HUDManager.GetComponent<HUDIndicator>().GetComponent<HUDIndicatorManagerVR>().ShowIndicators();
            hudCuesOn = true;

        }
    }

    /*
     * Disabling AddCue in favor of ShowCue due to difficulties in adding a HUD arrow during runtime.
    public void AddCue()
    {
        //Set a position 1 meter in front of the user
        Vector3 spawnPos = Camera.main.transform.position + (Camera.main.transform.forward); //+ new Vector3(1, 1, 1));

        //Instantiage collocated cue at the selected point
        GameObject newCue = Instantiate(cuePrefab, spawnPos, Quaternion.identity, cuesParent.transform);

        //Add it to the list of visual cues
        //Note that "reset positions" will cause new cues to go back to where they were created
        visualCues = GameObject.FindGameObjectsWithTag("visual_cue");
        cueStartingLocations.Add(newCue.transform.position);
        cueStartingRotations.Add(newCue.transform.rotation);
        cueStartingScales.Add(newCue.transform.localScale);


        //Add a HUD cue
        HUDIndicator hudIndicator = HUDManager.GetComponent<HUDIndicator>();
        hudIndicator.AddIndicator(newCue.transform, hudIndicator.indicators.Length);
        //RESUME HERE - how to do this during runtime instead on wake up?

        Debug.Log("Adding cue. Total cues: " + visualCues.Length);

    }
    */

    public void ShowCue()
    {
        //Activate the collocated and HUD cues for the next object.
        if (numShown < visualCues.Count)
        {
            visualCues[numShown].SetActive(true);
            HUDIndicator.AddIndicator(cuesParent.transform.GetChild(numShown), numShown);

            numShown++;
            numShownTextObject.GetComponent<TMP_Text>().text = "Show Cue (" + numShown + "/10)";
            Debug.Log("Showing additional cue. Number shown: " + numShown);
        }
        else Debug.Log("Max cues shown.");
    }

    public void HideCue()
    {
        //Hide the last collocated and HUD cue.
        if (numShown >= 1)
        {
            visualCues[numShown - 1].SetActive(false);
            HUDIndicator.RemoveIndicator(visualCues[numShown - 1].transform);
            Debug.Log("Indicator removed.");

            numShown--;
            numShownTextObject.GetComponent<TMP_Text>().text = "Show Cue (" + numShown + "/10)";
            Debug.Log("Hiding last cue. Number shown: " + numShown);
        }
        else Debug.Log("All cues hidden.");
    }


    public void SavePositions()
    {
        //Get visual cues and record original positions

        visualCues.Clear();
        Debug.Log("Visual cues cleared; length is now " + visualCues.Count);
        cueStartingLocations.Clear();
        cueStartingRotations.Clear();
        cueStartingScales.Clear();


        for (int n = 0; n < cuesParent.transform.childCount; n++)
        {
            visualCues.Add(cuesParent.transform.GetChild(n).gameObject);
        }

        Debug.Log(visualCues.Count + " visual cues added to list.");

        foreach (GameObject Cue in visualCues)
        {
            //Debug.Log("Cue " + Cue.name + " added at " + Cue.transform.position);
            cueStartingLocations.Add(Cue.transform.position);
            cueStartingRotations.Add(Cue.transform.rotation);
            cueStartingScales.Add(Cue.transform.localScale);
        }
    }

    public void ResetPositions()
    {
        Debug.Log("Resetting positions.");

        //int obstacleCount = 0;
        int cueCount = 0;

        /*
        foreach (GameObject Obstacle in digitalObstacles)
        {
            Obstacle.transform.position = obstacleStartingLocations[obstacleCount];
            Obstacle.transform.rotation = obstacleStartingRotations[obstacleCount];
            Obstacle.transform.localScale = obstacleStartingScales[obstacleCount];
            obstacleCount++;
        }
        */

        foreach (GameObject Cue in visualCues)
        {
            Cue.transform.position = cueStartingLocations[cueCount];
            Cue.transform.rotation = cueStartingRotations[cueCount];
            Cue.transform.localScale = cueStartingScales[cueCount];
            cueCount++;
        }
    }

    public void ToggleInterface()
    {
        //Toggles visibility of user interface.
        
        if (interfaceObject.activeSelf)
        {
            interfaceObject.SetActive(false);
        }
        else
        {
            interfaceObject.SetActive(true);
        }

    }
}
