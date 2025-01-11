using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Course : MonoBehaviour
{

    //Courses have a lap count, starting points for racers, checkpoints, music, .

    //Data synthesis is performed by the game manager on course load.


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
