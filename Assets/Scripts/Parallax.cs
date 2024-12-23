using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Parallax : MonoBehaviour {


    public float speedX;
    public float speedY;
    public bool moveOppositeDir;

    public bool moveParallax = true;

    Transform cameraTransform;
    Vector3 prevCamPos;


    private void Start() {

        cameraTransform = Camera.main.gameObject.transform;
        prevCamPos = cameraTransform.position;

    }

    //only need to move a proportion of the distance the camera moved (determined by parallax mult)
    //if the distance this object has moved FROM THE INIT is greater than half its length (since we are looking at the center) - may have trouble on serial movement
    //increase this object's initial position over by the object's length
    private void LateUpdate() {

        if (!moveParallax) {
            return;
        }

        Vector3 distance = cameraTransform.position - prevCamPos;
        float direction = (moveOppositeDir) ? -1f : 1f;

        transform.position += Vector3.Scale(distance, new Vector3(speedX, speedY)) * direction;

        prevCamPos = cameraTransform.position;
    }

}
