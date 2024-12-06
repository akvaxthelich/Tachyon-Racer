using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PlayerRacer : BaseRacer
{
    
    public string controlHorizontalAxis;
    public string controlVerticalAxis;


    protected override void Start()
    {
        base.Start();
    }
    
    protected override void Update()
    {
        base.Update();
        throttle = Input.GetAxisRaw(controlVerticalAxis);
        rotateInput = Input.GetAxisRaw(controlHorizontalAxis) * control;

        if (throttle < 0) {
            throttle *= control;
        }
        
        flames.SetActive(throttle > 0);

    }

    void FixedUpdate() {

        rb.AddForce(transform.up * throttle * acceleration, ForceMode2D.Force);

        rb.AddTorque(-rotateInput * maxRotateSpeed);
        if (rb.velocity.magnitude > maxSpeed) { 
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        
    }



}
