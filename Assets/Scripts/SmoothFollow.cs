using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    Vector3 offset = new Vector3(0f, 0f, -10f);
    public Transform target; //what are we following?
    
    public float lerpAmount = 0.15f;

    Vector3 velocity = Vector3.zero;

    Vector3 cameraTargetPosition;
    
    // Start is called before the ffirst frame update
    void Start()
    {
        transform.position = new Vector3(target.position.x, target.position.y, -10);
    }

    // Update is called once per frame
    void Update()
    {
        cameraTargetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, cameraTargetPosition, ref velocity, lerpAmount);
    }
}
