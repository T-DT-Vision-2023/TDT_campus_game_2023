using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class WateLampControler : MonoBehaviour
{
    // Start is called before the first frame update


    public float speed=0;
   
    
    private Vector3 position;
    private Quaternion rotaion;
    private float counttime = 0;
    void Start()
    {
        BoxCollider box = gameObject.GetComponent<BoxCollider>();
        this.speed = -box.size.x*100/7;
        Debug.Log(speed);
        this.position = transform.localPosition;
        this.rotaion = transform.localRotation;
        InvokeRepeating(nameof(reset_position),1,1);
    
    }

    // Update is called once per frame
    void Update()
    {

       
        this.transform.Translate(Vector3.right*speed*Time.deltaTime);
        
        /*counttime += (Vector3.right * speed * Time.deltaTime).x;*/


    }

     void reset_position()
    {
        this.transform.SetLocalPositionAndRotation(this.position,this.rotaion);
        /*Debug.Log(counttime);
        counttime = 0;*/
    }
}
