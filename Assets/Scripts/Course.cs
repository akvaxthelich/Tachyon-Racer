using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Course : MonoBehaviour
{

    //Courses have a lap count, starting points for racers, checkpoints, music, .

    //Data synthesis is performed by the game manager on course load.
    //Game manager also counts startpoints per race, since some racers may be crashed out or retired.

    [SerializeField]
    public string courseName; 
    [SerializeField]
    public string courseDescription;
    [SerializeField]
    public int lapCount;
    [SerializeField]
    public string musicPath;
    [SerializeField]
    public Color bgColor;
    

}
