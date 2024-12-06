using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Course : MonoBehaviour
{

    //Courses have a lap count, starting points for racers, checkpoints, music, .

    //Data synthesis is performed by the game manager on course load.
    // Start is called before the first frame update

    [SerializeField]
    public string courseName; 
    [SerializeField]
    public string courseDescription;
    [SerializeField]
    public int lapCount;
    [SerializeField]
    public string musicPath;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
