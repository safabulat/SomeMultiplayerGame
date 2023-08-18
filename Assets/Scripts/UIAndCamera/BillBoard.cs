using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    GameObject mainCam;
    Transform cam;

    private void Start()
    {
        mainCam = GameObject.Find("Main Camera");
        cam = mainCam.GetComponent<Transform>();

    }
    
    void LateUpdate()
    {
        transform.LookAt(transform.position +  cam.forward);
        //transform.LookAt(transform.position + transform.forward);
    }
}
