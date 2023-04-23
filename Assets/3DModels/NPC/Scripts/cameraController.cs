using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{

    public float fieldOfView = 68f;

    private GameObject atachedVehicle;
    private int locationIndicator = 0 ;
    private Controller controllerRef;
    private Camera camera;

    private float bias ;
    [Range(0,1)]public float smoothTime = .5f;
    private float smoothTimemin = .5f , max = .9f;
    private Vector3 newPos;
    private Transform target;
    
    private float bandEffect;

    public GameObject focusPoint;

    public Vector2[] cameraPos;
    
    void Start(){

        cameraPos = new Vector2[4];
        cameraPos[0] = new Vector2(2,0);
        cameraPos[1] = new Vector2(7.5f,0.5f);
        cameraPos[2] = new Vector2(8.4f,1.6f);
        cameraPos[3] = new Vector2(8.4f,1.6f);

        camera = gameObject.GetComponent<Camera>();
        atachedVehicle = GameObject.FindGameObjectWithTag("Player");

        //focusPoint = atachedVehicle.transform.Find("focus").gameObject;

        target = focusPoint.transform;

        controllerRef = atachedVehicle.GetComponent<Controller>();

        camera.usePhysicalProperties = true;
        camera.fieldOfView = fieldOfView;

    }
    private Vector3 velocity;
    void FixedUpdate(){

        updateCam();

    }

    public void cycleCamera(){
        if(locationIndicator >= cameraPos.Length-1 || locationIndicator < 0) locationIndicator = 0;
            else locationIndicator ++;
    }

    public void updateCam(){
        if(Input.GetKeyDown(KeyCode.F1)){
            cycleCamera();
        }    

        newPos = target.position - (target.forward * cameraPos[locationIndicator].x) + (target.up * cameraPos[locationIndicator].y) ;
        //transform.position = newPos * (1 - smoothTime) + transform.position * smoothTime;
        transform.position = Vector3.SmoothDamp(transform.position , newPos , ref velocity , smoothTime );
        transform.LookAt(target.transform);

        
    }

}
