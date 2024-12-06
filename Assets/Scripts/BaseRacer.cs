using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;

public class BaseRacer : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody2D rb;
    [SerializeField]
    protected float throttle; //between -1 and 1, input TODO
    [SerializeField]
    protected float rotateInput;
    
    protected GameObject flames;
    [SerializeField]
    protected float acceleration;
    [SerializeField]
    protected float maxSpeed;
    [SerializeField]
    protected float maxRotateSpeed;
    [SerializeField, Range(0f, 65f)]
    protected float turnRadiusOffset = 45f;
    [SerializeField, Range(0f, 1f)]
    protected float control;
    [SerializeField, Range(0f, 1f)]
    protected float crashOutDotThreshold; //base at 0.95f
    [SerializeField, Range (0f, 1f)]
    protected float crashOutSpeedThreshold; //base at 0.85f, might be the same threshold to start cooling down, not sure yet.

    [SerializeField]
    Color racerColor = Color.white;
    //TODO SEPARATE OUT THE INDIVIDUAL RACER STATS

    protected float controlRecoveryTimer;

    [SerializeField, Range(0f, 5f)]
    protected float controlRecoveryCooldown; //should be based on a range given racer class. 'Rebound' racers 

    [SerializeField, Range(0f,30f)]
    protected float controlRecoveryDegreeMinimum; //how little can this vehicle turn before allowing the control recovery?
    [SerializeField, Range(0f, 2f)]
    protected float controlRecoverySpeed;  //keep below 1, or 0 to 2 for Rebound.
    /// <summary>
    /// Control determines how much a Racer can turn, brake, and the angle at which they can crash out.
    /// It ranges from 0 to 1. At 1, a Racer can turn normally, brake normally, and crashes at 10 degrees, above some minimum speed. 
    /// At 0, a player cannot turn, has reduced brakes, and can crash at 90 degrees.
    /// </summary>


    [SerializeField]
    AudioSource asrc;
    [SerializeField]
    protected AudioClip[] damageClips;
    //TODO shadows might render on the sky/outside road boundaries
    #region Flash shader + graphics (Shadow behavior in 'SimpleShadow.cs') 
    SpriteRenderer sr;
    Shader defaultShader;
    [SerializeField]
    protected Shader flashShader;

    #endregion

    [SerializeField]
    protected Checkpoint targetCheckpoint;      //distinct from gamemanager, these are for ai calcs and 
    
    [SerializeField]
    protected Checkpoint currentCheckpoint;     //most recently flagged checkpoint
    
    [SerializeField] 
    protected Checkpoint previousCheckpoint;    //due to reflagging tolerance 
    
    [SerializeField]
    protected Checkpoint farthestCheckpoint;    //only for respawning, do not use in AI

    
    //beneficial to include float constant for timing
    //ie: -1.0f means this state will go until it is transitioned
    //ie 5.0f this behavior

    protected virtual void Start() {
        asrc = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        InitShader();

        flames = transform.GetChild(0).gameObject;

        control = 1.0f;
        if (!GameManager.instance.isCourseNaive) {
            InitCheckpoint();
        }


    }

    protected virtual void Update() {

        if (rb.angularVelocity < controlRecoveryDegreeMinimum) {
            controlRecoveryTimer += Time.deltaTime;
            if (controlRecoveryTimer >= controlRecoveryCooldown) {
                RecoverControl();
            }

        }
        else {
            controlRecoveryTimer = 0;
        }
        
    }
    protected void InitCheckpoint() {

        int cCount = GameManager.instance.checkpoints.Count;
        previousCheckpoint = GameManager.instance.checkpoints[cCount - 1];
        currentCheckpoint = GameManager.instance.checkpoints[0];
        targetCheckpoint = GameManager.instance.checkpoints[1];

        
    }

    protected void AdvanceCheckpoint() {
        
        if (Vector2.Distance(transform.position, targetCheckpoint.transform.position) < targetCheckpoint.tolerance) {
            Checkpoint temp = targetCheckpoint;
            Checkpoint temp2 = currentCheckpoint;
            targetCheckpoint = targetCheckpoint.nextCheckpoint;
            currentCheckpoint = temp;
            previousCheckpoint = temp2;

            //print("Current: " + currentCheckpoint);
            //print("Target: " + targetCheckpoint);
        }
        
    }

    protected void RetreatCheckpoint() {

        if (Vector2.Distance(transform.position, previousCheckpoint.transform.position) < previousCheckpoint.tolerance) {
            Checkpoint temp = currentCheckpoint;
            currentCheckpoint = currentCheckpoint.prevCheckpoint; //cause of this,
            targetCheckpoint = temp;
            previousCheckpoint = currentCheckpoint.prevCheckpoint; //guh order matters. durr. !anteayer!
            
        }

    }

    protected void HandleUpdateCheckpoint() { 

        AdvanceCheckpoint(); 
        RetreatCheckpoint();

    }

    public float GetControl() {
        return control;
    }

    protected void RecoverControl() {
        UpdateControl(Time.deltaTime * controlRecoverySpeed);
    }
    protected void UpdateControl(float amount) {
        control = Mathf.Clamp01(control + amount);
    }

    protected void CrashOut() { 
    //destroy the racer 
    //fix the camera
    //spawn some particle effects
    //play some sound
    
        //tell the gamemanager that this racer died, let the game manager handle the rest. (if AI, remove from list, etc)
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Flash();
        ContactPoint2D contactPoint = collision.GetContact(0);
        float currentRelativeToMaxSpeedRatio = contactPoint.relativeVelocity.magnitude / maxSpeed;
            //needed to use relativeVelocity here otherwise unity thinks the vehicle is at zero velocity when the collision happens
        float dotProductWallNormal = Vector2.Dot(-contactPoint.normal, new Vector2(transform.up.x, transform.up.y));

        if (dotProductWallNormal > crashOutDotThreshold && currentRelativeToMaxSpeedRatio > crashOutSpeedThreshold) {

            print("DEAD!");
            
        }
        else {
            //TODO resistance value? mass? reduce damage based on some shit idk
            float damage = -currentRelativeToMaxSpeedRatio * Mathf.Abs(dotProductWallNormal);
            PlayDamageSound(Mathf.Abs(damage));
            UpdateControl(damage);
        }
    }

    private void InitShader() {
        defaultShader = sr.material.shader;
    }
    void Flash() { 
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine() {
        sr.material.shader = flashShader;
        yield return new WaitForSeconds(.15f); //check out this magic number fuckwad!
        sr.material.shader = defaultShader;
    }

    void PlayDamageSound(float damageAmount) { 
    
        //12 total damage sounds. not a permanent solution.
        int choice = Random.Range(0, 3);

        if (damageAmount <= 0.2) {
            asrc.PlayOneShot(damageClips[choice]);
            return;
        }
        if (damageAmount <= 0.5) {
            asrc.PlayOneShot(damageClips[choice + 4]);
            return;
        }
        if(damageAmount <= 1) {
            asrc.PlayOneShot(damageClips[choice + 8]);
        }

    }
}
