using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrackObject : MonoBehaviour
{

    public GameObject trackObject;


    
    // Start is called before the first frame update
    void Start()
    {
        // trackObject = gameObject;
    }

    public static CameraTrackObject instance;
    void Awake(){
        CameraTrackObject.instance = this;
    }

    Vector3 off = new Vector3(0,0,-10);

    // Update is called once per frame
    void Update()
    {
        if(trackObject != null)
            transform.position = trackObject.transform.position+off;
    }
}
