using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool startingLine;

    public float differential; //calculated and set by game manager, distance to next checkpoint in sequence.

    public Checkpoint nextCheckpoint;
    public Checkpoint prevCheckpoint;

    public float tolerance;

    private void OnDrawGizmos() {
        //variable = condition ? consequent : alternative
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tolerance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + transform.up); //checkpoint normal
        Gizmos.color = startingLine ? Color.red: Color.green;
        Gizmos.DrawLine(transform.position - (2 * transform.right), transform.position + (2 * transform.right));
        
    }

}
