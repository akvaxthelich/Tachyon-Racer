using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemporaryControlMeterHandler : MonoBehaviour
{
    //TODO relinquish this code to the gamemanager per player. also consider zero-hud.
    // Start is called before the first frame update
    public BaseRacer b;
    Image i;
    void Start()
    {
        i = GetComponent<Image>();    
        
    }

    // Update is called once per frame
    void Update()
    {
        i.fillAmount = b.GetControl();
    }
}
