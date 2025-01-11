using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;

public class BaseRacer : MonoBehaviour
{
    #region Movement
    [SerializeField]
    protected Rigidbody2D rb;
    [SerializeField]
    protected float verticalInput; //between -1 and 1, input TODO
    [SerializeField]
    protected float horizontalInput;
    [SerializeField]
    protected float accelerateControllerInput;

    protected GameObject flames;
    [SerializeField]
    protected float acceleration;
    [SerializeField]
    protected float brakeQuotient;
    [SerializeField]
    protected float maxSpeed;
    [SerializeField]
    protected float maxRotateSpeed;
    [SerializeField, Range(0f, 65f)]
    protected float turnRadiusOffset = 45f;
    #endregion

    [SerializeField]
    Color racerColor = Color.white;

    #region Race Specifics
    int currLap;
    int cCount; 
    protected float effectiveDistance;
    [SerializeField]
    protected int checkpointsPassed;
    //StatPlacement: -1 if first race, -1 by default.
    //currPlacement is set per frame, using closest point

    //Only players need to know what their placements are live, game manager only checks placement as a player crosses final lap
    public int statPlacement = -1; //set on race finish in game manager
    int currPlacement = -1;        //set per frame
                                   //compare lap
                                   //calculate distance from closest point on vehicle to next checkpoint center, add up differentials to the start checkpoint
    #endregion

    #region Control
    [SerializeField, Range(0f, 1f)]
    protected float control;
    [SerializeField, Range(0f, 1f)]
    protected float crashOutDotThreshold; //base at 0.95f
    [SerializeField, Range(0f, 1f)]
    protected float crashOutSpeedThreshold; //base at 0.85f, might be the same threshold to start cooling down, not sure yet.
    [SerializeField, Range(0f, 5f)]
    protected float controlRecoveryCooldown; //should be based on a range given racer class. 'Rebound' racers 

    [SerializeField, Range(0f,30f)]
    protected float controlRecoveryDegreeMinimum; //how little can this vehicle turn before allowing the control recovery?
    [SerializeField, Range(0f, 2f)]
    protected float controlRecoverySpeed;  //keep below 1, or 0 to 2 for Rebound.

    protected float controlRecoveryTimer;
    /// <summary>
    /// Control determines how much a Racer can turn, brake, and the angle at which they can crash out.
    /// It ranges from 0 to 1. At 1, a Racer can turn normally, brake normally, and crashes at 10 degrees, above some minimum speed. 
    /// At 0, a player cannot turn, has reduced brakes, and can crash at 90 degrees.
    /// </summary>
    #endregion

    #region Audio
    [SerializeField]
    protected List<AudioSource> asrcs;
    [SerializeField]
    protected AudioSource damageSrc;
    [SerializeField]
    protected AudioSource engineSrc;
    [SerializeField]
    protected AudioClip[] damageClips; //TODO grab these from the gamemanager (might be faster than resources)
    [SerializeField]
    protected AudioSource engineAsrc;
    [SerializeField]
    protected AudioClip[] engineClips;
    #endregion
    //TODO shadows might render on the sky/outside road boundaries
    #region Flash shader + graphics (Shadow behavior in 'SimpleShadow.cs') 
    SpriteRenderer sr;
    Shader defaultShader;
    [SerializeField]
    protected Shader flashShader;

    #endregion

    #region Checkpoints
    [SerializeField]
    protected Checkpoint targetCheckpoint;      //distinct from gamemanager, these are for ai calcs and 
    
    [SerializeField]
    protected Checkpoint currentCheckpoint;     //most recently flagged checkpoint
    
    [SerializeField] 
    protected Checkpoint previousCheckpoint;    //due to reflagging tolerance 
    
    [SerializeField]
    protected Checkpoint farthestCheckpoint;    //only for respawning, do not use in AI
    #endregion

    protected virtual void Start() {
        asrcs = GetComponentsInChildren<AudioSource>().ToList<AudioSource>();
        damageSrc = asrcs[0];
        engineSrc = asrcs[1];
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        engineAsrc = GetComponentInChildren<AudioSource>();
        InitShader();

        flames = transform.GetChild(0).gameObject;

        control = 1.0f;
        if (!GameManager.instance.isCourseNaive) {
            InitCheckpoint();
            InitLap();
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
        CalculatePlacement();
    }
    protected void InitCheckpoint() {
        cCount = GameManager.instance.checkpoints.Count;
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

            if (previousCheckpoint == GameManager.instance.checkpoints[cCount - 1]) {
                AdvanceLap();
            }
            else {
                checkpointsPassed++;
            }
            
        }
        //count checkpoints passed on advance
    }

    protected void RetreatCheckpoint() {

        if (Vector2.Distance(transform.position, previousCheckpoint.transform.position) < previousCheckpoint.tolerance) {
            Checkpoint temp = currentCheckpoint;
            currentCheckpoint = currentCheckpoint.prevCheckpoint; //cause of this,
            targetCheckpoint = temp;
            previousCheckpoint = currentCheckpoint.prevCheckpoint; //guh order matters. durr. !anteayer!

            checkpointsPassed--;
        }
        
        //decrement checkpoints passed on retreat
    }

    protected void HandleUpdateCheckpoint() { 

        AdvanceCheckpoint(); 
        RetreatCheckpoint();

    }

    protected virtual void HandleEngineAudio() {
        //clip 0, 1, 2
        //open, engine, close
        //on transition observer pattern for ai,
        //use input for player
        
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
    protected void CalculatePlacement() {

        effectiveDistance = (((currentCheckpoint.terminalDifferential - currentCheckpoint.differential + Vector2.Distance(transform.position, targetCheckpoint.transform.position)) / GameManager.instance.trackLength) * (GameManager.instance.lapCount - currLap - 1)); 

        Debug.Log(effectiveDistance);
        //add current lap - 1 once laps work
    //take minimum thereafter
    }
    protected void InitLap() {
        currLap = 1;
        checkpointsPassed = 0;
    }
    protected void AdvanceLap() {
        currLap++;
        checkpointsPassed = 0;
        Debug.Log("You are now on lap: " + currLap);
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
            damageSrc.PlayOneShot(damageClips[choice]);
            return;
        }
        if (damageAmount <= 0.5) {
            damageSrc.PlayOneShot(damageClips[choice + 4]);
            return;
        }
        if(damageAmount <= 1) {
            damageSrc.PlayOneShot(damageClips[choice + 8]);
        }

    }

    protected void Accelerate() {
        if (verticalInput > 0) {
            rb.AddForce(transform.up * verticalInput * acceleration, ForceMode2D.Force);
        }
        
    }
    protected void Brake() {
        if (verticalInput < 0) {
            rb.AddForce(-rb.velocity.normalized * acceleration / brakeQuotient, ForceMode2D.Force);
        }
        
    }

    protected void AccelerateController() {
        if (accelerateControllerInput > 0) {
            rb.AddForce(transform.up * accelerateControllerInput * acceleration, ForceMode2D.Force);
        }

    }

    protected void BrakeController() {
        if (accelerateControllerInput < 0) {
            rb.AddForce(-rb.velocity.normalized * acceleration / brakeQuotient, ForceMode2D.Force);
        }

    }

}
