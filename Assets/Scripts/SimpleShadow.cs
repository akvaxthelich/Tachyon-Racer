using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleShadow : MonoBehaviour
{
    public Vector2 globalOffset; //TODO grab this value from some data file, from the course, etc
    GameObject objectToShadow;
    // Start is called before the first frame update
    void Start()
    {
        objectToShadow = transform.parent.gameObject;
        transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(objectToShadow.transform.position.x + globalOffset.x, objectToShadow.transform.position.y + globalOffset.y);
        transform.rotation = objectToShadow.transform.rotation;
    }
}
