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
    public List<BaseRacer> racers; //includes player and ai


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
        if (!isCourseNaive) { 
            TryGetStartPoints();
        }
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

    List<Transform> TryGetStartpoints() { //post naivety. List of transforms because the data i
        GameObject startpointParent = GameObject.Find("Startpoint Parent");
        if (startpointParent == null) {
            Debug.LogWarning("This course is missing a Startpoint Parent, Racers will spawn with algorithmic pattern.");
            return null; //TODO spawn them at zero
        }
        startPoints = startpointParent.GetComponentsInChildren<Transform>().ToList(); //Startpoints are in order of assigned placement.
        return startPoints;
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

            checkpoints[i].differential = Vector2.Distance(checkpoints[i].transform.position, checkpoints[i].nextCheckpoint.transform.position);
                        
            trackLength += checkpoints[i].differential;
        }

        float progressiveDifferential = 0f;
        for (int j = 0; j < checkpoints.Count(); j++){

            checkpoints[j].terminalDifferential = trackLength - progressiveDifferential;
            
            progressiveDifferential += checkpoints[j].differential;
        
        }
        
    }

    private void InitRacersAndPlacement() {
        //consider saving each racer's lastraceplacement when spawning them?
        //for (int i = 0; i < racers.Count; i++) {
        //    GameObject.Instantiate(racers[i], );
        //}
        //Instantiate racers at startpoints based on last race placement
        //if -1, shuffle and spawn randomly, and assign currPlacement per racer at spawn time
        //reset all racers' control stats to 1. can't delete
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

