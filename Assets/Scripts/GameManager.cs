using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    [SerializeField]
    private int lapCount { get; } //TODO return from course itself

    [SerializeField]
    public List<BaseRacer> racers; //includes


    [SerializeField]
    private float trackLength;
    [SerializeField]
    private float totalTrackLength; //Synthesize later from trackLength * laps

    [SerializeField]
    public GameObject checkpointParent;
    public List<Checkpoint> checkpoints;

    [SerializeField]
    public GameObject startPointParent;
    public List<Transform> startPoints;
    [SerializeField]
    public bool isCourseNaive;
    private void Awake() {
        Application.targetFrameRate = 60;
        //Singleton architecture
        if (instance != null) {
            Destroy(this);
        }
        instance = this;


        LoadCourse(); //later must include course prefabs which themselves contain the checkpoint parent. for current intent
    }

    void Start() {
        
    }


    void LoadCourse() {
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

            //total track length
            trackLength += checkpoints[i].differential;
        }

    }

    private void InitRacers() {
        //
    }

    private void InitCourse() {
        //TODO get course by name in resources folder
    }

}

