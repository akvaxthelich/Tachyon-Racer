using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;
    
    public string courseSelectedName; //Precondition: include underscores if present.
    string courseSelectedPath;
    Course course; 

    [SerializeField]
    public bool isCourseNaive;

    [SerializeField]
    public int lapCount;//TODO return from course itself

    [SerializeField]
    public List<BaseRacer> racers; //includes player and 


    [SerializeField]
    public float trackLength;
    [SerializeField]
    public float totalTrackLength; 

    [SerializeField]
    public GameObject checkpointParent;
    public List<Checkpoint> checkpoints;

    [SerializeField]
    public GameObject startPointParent;
    public List<Transform> startPoints;
    
    private void Awake() {
        Application.targetFrameRate = 60;
        //Singleton architecture
        if (instance != null) {
            Destroy(this);
        }
        instance = this;

        LoadCourse(); //later must include course prefabs which themselves contain the checkpoint parent. for current intent
        totalTrackLength = trackLength * lapCount;
    }
    void LoadCourse() {
        InitCourse();
        checkpoints = TryGetCheckpoints();
        isCourseNaive = (checkpoints == null);
    }
    List<Checkpoint> TryGetCheckpoints() { //determine naivety
        GameObject checkpointParent = GameObject.Find("Checkpoint Parent");
        
        if (checkpointParent == null) {
            Debug.LogWarning("This course is missing a Checkpoint Parent, course is naive.");
            return null;
        }
        checkpoints = checkpointParent.GetComponentsInChildren<Checkpoint>().ToList(); //good god

        //assuming it didnt throw an error here either, also set course to not naive (TODO fix bad practice)
        //because the checkpointParent may still not have any children, so that list may be empty or null
        SynthesizeCourseData(); //take those checkpoints and calculate the course length from all of the differentials between them
        return checkpoints;
    }

    List<Transform> TryGetStartPoints() {
        GameObject startPointParent = GameObject.Find("Startpoint Parent");
        if (startPointParent == null) {
            Debug.LogWarning("This course is missing a Startpoint Parent. Defaulting to 0,0.");
            return null;
        }
        startPoints = startPointParent.GetComponentsInChildren<Transform>().ToList();
        return startPoints;
    }


    void SynthesizeCourseData() {

        for(int i = 0; i < checkpoints.Count(); i++) { 
            checkpoints[i].nextCheckpoint = (i == checkpoints.Count() - 1) ? checkpoints[0] : checkpoints[i + 1];
            checkpoints[i].prevCheckpoint = (i == 0) ? checkpoints[checkpoints.Count() - 1] : checkpoints[i - 1];

            //calculate checkpoint differentials,
            checkpoints[i].differential = Vector2.Distance(checkpoints[i].transform.position, checkpoints[i].nextCheckpoint.transform.position);
            
            
            trackLength += checkpoints[i].differential;
        }
        //function does not produce correct terminal differentials which leads to an infinity error elsewhere.
        //Fixing...
        float progressiveDifferential = 0f;
        for (int j = 0; j < checkpoints.Count(); j++){

            checkpoints[j].terminalDifferential = trackLength - progressiveDifferential;
            
            progressiveDifferential += checkpoints[j].differential;
        
        }
        
    }

    private void InitRacers() {
        
    }

    private void InitCourse() {
        if (courseSelectedName != null) {
            //Path appears as: Course/
            courseSelectedPath = "Course/" + courseSelectedName;
            GameObject courseObj = Resources.Load<GameObject>(courseSelectedPath);
            course = courseObj.GetComponent<Course>();
            lapCount = course.lapCount;
            GameObject.Instantiate(courseObj);
        }//todo send you to hell (debug level)
    }

   
}

