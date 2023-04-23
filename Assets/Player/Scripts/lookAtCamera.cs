using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtCamera : MonoBehaviour{
    
    public GameObject  cameraObject;
    
    // Start is called before the first frame update
    void Start()
    {
        cameraObject = FindObjectOfType<Camera>().gameObject;
    }

    void FixedUpdate(){
        if(cameraObject == null) return;
        transform.LookAt(cameraObject.transform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y ,0);    
    }
}
