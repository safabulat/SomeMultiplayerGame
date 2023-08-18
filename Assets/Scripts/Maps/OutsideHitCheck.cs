using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutsideHitCheck : MonoBehaviour
{
    BoxCollider BoxCollider;

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();    
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Dead");
        Destroy(collision.gameObject);
    }


}
