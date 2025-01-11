using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class PlayerRacer : BaseRacer
{
    
    public string controlHorizontalAxis;
    public string controlVerticalAxis;
    //Controller Only
    public string accelerateAndBrakeAxis;

    float rotateWithControl;
    public float controllerDebugRadius;
    Vector2 controllerInputAxes;

    //why dont you suck my left balls?
    protected override void Start()
    {
        base.Start();
        controllerInputAxes = Vector2.zero;
    }
    
    protected override void Update()
    {
        base.Update();
        verticalInput = Input.GetAxisRaw(controlVerticalAxis);       //used as y axis in atan2 calculation for controller ONLY
        horizontalInput = Input.GetAxisRaw(controlHorizontalAxis);  //used as x axis "", TODO CLEAN UP
        accelerateControllerInput = Input.GetAxisRaw(accelerateAndBrakeAxis);

        rotateWithControl = horizontalInput * control;

        controllerInputAxes = new Vector2(horizontalInput, verticalInput).normalized;
        

        if (verticalInput < 0) {
            verticalInput *= control;
        }
        HandleEngineAudio();
        //flames.SetActive(accelerateControllerInput > 0);
        flames.SetActive(verticalInput > 0);
        HandleUpdateCheckpoint();
        
    }

    void FixedUpdate() {
        
        Accelerate();
        Brake();
        //AccelerateController();
        //BrakeController();
        //ControllerRotate();

        //Rotation
        //Note: Reserve usage for keyboard controls.
        rb.AddTorque(-rotateWithControl * maxRotateSpeed);
        
        
        //cap speed
        if (rb.velocity.magnitude > maxSpeed) { 
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        
    }

    private void OnDrawGizmos() {
        //not magic number here, must be one to match input vector.
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 1);
        Gizmos.color = Color.red;

        Vector3 controlDirectionGizmo = new Vector3(controllerInputAxes.x, controllerInputAxes.y, 0.0f) + transform.position;
        Gizmos.DrawWireSphere(controlDirectionGizmo, controllerDebugRadius);
    }

    protected override void HandleEngineAudio() {
        asrcs[1].pitch = Mathf.Clamp(rb.velocity.magnitude / maxSpeed, 0.75f, 1.2f);
        if (Input.GetButtonDown(controlVerticalAxis) && Input.GetAxisRaw(controlVerticalAxis) == 1) {
            asrcs[1].PlayOneShot(engineClips[0]);
            asrcs[1].clip = engineClips[1];
            asrcs[1].Play();
        }
        else if (Input.GetButtonUp(controlVerticalAxis)) {
            asrcs[1].PlayOneShot(engineClips[2]);
            asrcs[1].clip = null;
            asrcs[1].Stop();
        }
    }

    void ControllerRotate() {

        //modify for deadzone
        //check if closer to trn other way somehow. shower thought coming soon to a codebase near you
        if (Mathf.Atan2(transform.up.y, transform.up.x) > Mathf.Atan2(controllerInputAxes.y, controllerInputAxes.x)) {
            Debug.Log("Should turn right.");
            rb.AddTorque(-maxRotateSpeed * control);
        }
        else if (Mathf.Atan2(transform.up.y, transform.up.x) < Mathf.Atan2(controllerInputAxes.y, controllerInputAxes.x)) {
            Debug.Log("Should turn left.");
            rb.AddTorque(maxRotateSpeed * control);
        }

    }

}
