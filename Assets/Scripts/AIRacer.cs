using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class AIRacer : BaseRacer {
    public float turnDetectionDistance;

    float rotateDirection;

    public LayerMask walls;

    RaycastHit2D leftHit;
    RaycastHit2D forwardHit;
    RaycastHit2D rightHit;

    //turn detection vector

    //Vector2 turnDetection; x axis is stagnant, y of vector is proportional to velocity, 
    //TODO


    [Header("AI")]

    [SerializeField]
    //does this ai use checkpoints?
    protected bool naive;

    CapsuleCollider2D collider;

    [SerializeField]
    protected float turnThreshold = 50f; //how far from the correct angle to the next checkpoint can i be in degrees?

    [SerializeField]
    protected float turnDetectionSpacing;

    protected UnityAction Act; //call act every frame to make use of ai.

    protected enum STATE {

        NONE,               //remove all behaviors 
        GO,                 //full throttle, no course correction or avoidance
        STOP,       //remove all behaviors, attempt to stop moving by braking in the correct direction. think recover from smash training
        NAVIGATE_COURSE,    //follow checkpoints, prioritizing proximity based avoidance
        NAVIGATE_NAIVE,     //fuck a checkpoint, avoidance behavior only 
        SPINOUT,            //turn uncontrollably, transition out. 
        DIE
    }
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        naive = GameManager.instance.isCourseNaive;
        collider = GetComponent<CapsuleCollider2D>();
        turnDetectionSpacing = collider.size.x;
    }


    protected override void Update() {
        base.Update();
        flames.SetActive(throttle > 0);

        turnDetectionDistance = rb.velocity.magnitude;
    }

    void FixedUpdate() {
        //Go();
        if (!naive) {
            HandleUpdateCheckpoint();
            NavigateTrack();
        }
        else {
            NavigateTrackNaive();
        }
        rb.AddForce(transform.up * throttle * acceleration, ForceMode2D.Force);
        rb.AddTorque(-rotateDirection * maxRotateSpeed);
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

    }

    
    private void OnDrawGizmos() {

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, -turnRadiusOffset / 2) * transform.up * PriorityNonZero(leftHit.distance, turnDetectionDistance));
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, transform.up * PriorityNonZero(forwardHit.distance, turnDetectionDistance));
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, turnRadiusOffset / 2) * transform.up * PriorityNonZero(rightHit.distance, turnDetectionDistance));
        
    }

    void Go() {
        throttle = 1f;
    }
   
    //HelloWorld(printf);
    void NavigateTrack() {

        //TODO: Prioritize avoidance or navigation based on distance to either. Should be inversely proportional
        //Avoid if obstacle is closer
        //match checkpoint if no obstacle.
        leftHit = Physics2D.Raycast(transform.position, Quaternion.Euler(0, 0, -turnRadiusOffset / 2) * transform.up, turnDetectionDistance, walls);
        forwardHit = Physics2D.Raycast(transform.position, transform.up, turnDetectionDistance, walls);
        rightHit = Physics2D.Raycast(transform.position, Quaternion.Euler(0, 0, turnRadiusOffset / 2) * transform.up, turnDetectionDistance, walls);

        bool avoidance = (ThreeMinIgnoreZero(leftHit.distance, forwardHit.distance, rightHit.distance) < currentCheckpoint.differential);

        if (avoidance) {
            //we want the angle that we can see at to be inversely proportional with our current throttle, like turnfactor
            //reasoning: if we're trying to move slower, it's likely because we're turning, and in turny/zig zaggificatious areas we want turndetection angle to be wider

            //this would USE TURNRADIUSOFFSET SCALED BETWEEN 20 and 90, based on the Uras Factor (???)

            RaycastHit2D minDistHit = RayCastHit2DMinDist(leftHit, forwardHit, rightHit);
            if (minDistHit == leftHit) {
                rotateDirection = -1;
            }
            if (minDistHit == rightHit) {
                rotateDirection =  1;
            }
        }
        else {
            float angle = targetCheckpoint.transform.eulerAngles.z - transform.eulerAngles.z; //difference between target and current

            if (Mathf.Abs(angle) > 180) {
                angle = 360 - Mathf.Abs(angle);
            }

            //Distance to targetCheckpoint - tolerance
            float dst = (Vector2.Distance(transform.position, targetCheckpoint.transform.position) - targetCheckpoint.tolerance);


            float turnFactor = 1 - Mathf.Clamp((Mathf.Pow(dst, 2) / (currentCheckpoint.differential)), 0.5f, 1f); //inverse square of the distance / differential
            turnFactor += (Mathf.Abs(angle) / 181) / 2;

            float throttleFactor = 1 - turnFactor;

            rotateDirection = turnFactor * -Mathf.Sign(angle); //stop changing the fucking sign on this it's CORRECT PLEASE ANDREW JESUS PLEASE //L

            throttle = throttleFactor;


        }


    }

    void NavigateTrackNaive() { //TODO consolidate into "Avoidance()" and make NavigateTrack contain calls to "NavigateCheckpoints()" and "Avoidance()"

        //TODO: Prioritize avoidance or navigation based on distance to either. Should be inversely proportional
        //Avoid if obstacle is closer
        //match checkpoint if no obstacle.
        leftHit = Physics2D.Raycast(transform.position, Quaternion.Euler(0, 0, -turnRadiusOffset / 2) * transform.up, turnDetectionDistance, walls);
        forwardHit = Physics2D.Raycast(transform.position, transform.up, turnDetectionDistance, walls);
        rightHit = Physics2D.Raycast(transform.position, Quaternion.Euler(0, 0, turnRadiusOffset / 2) * transform.up, turnDetectionDistance, walls);

        //we want the angle that we can see at to be inversely proportional with our current throttle, like turnfactor
        //reasoning: if we're trying to move slower, it's likely because we're turning, and in turny/zig zaggificatious areas we want turndetection angle to be wider

        //this would USE TURNRADIUSOFFSET SCALED BETWEEN 20 and 90, based on the Uras Factor (???)

        throttle = 1;

            RaycastHit2D minDistHit = RayCastHit2DMinDist(leftHit, forwardHit, rightHit);
            if (minDistHit == leftHit) {
                rotateDirection = -1;
            }
            if (minDistHit == rightHit) {
                rotateDirection = 1;
            }

    }
    //Raycasts are length zero if they don't hit, which breaks Min logic, thus:
    float PriorityNonZero(float primary, float secondary) {

        if (primary > 0) {
            return primary;
        }
        else {
            return secondary;
        }

    }

    float ThreeMinIgnoreZero(float a, float b, float c) {

        float fixA = a == 0 ? float.MaxValue : a;
        float fixB = b == 0 ? float.MaxValue : b;
        float fixC = c == 0 ? float.MaxValue : c;
        
        return Mathf.Min(fixA, fixB, fixC);   

    }

    RaycastHit2D RayCastHit2DMinDist(RaycastHit2D a, RaycastHit2D b, RaycastHit2D c) {

        float min = ThreeMinIgnoreZero(a.distance,b.distance,c.distance);
        if (a.distance == min) { 
            return a;
        }
        if (b.distance == min) {
            return b;
        }
        if (c.distance == min) {
            return c;
        }

        return b;

    }

}
