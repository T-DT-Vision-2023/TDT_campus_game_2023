using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class RingSenser : MonoBehaviour
{
    public UnityEvent OnBulletHit=new UnityEvent();

    public String name = "hahah";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.contacts[0].point);
        
        
        if (transform.parent.transform.GetComponent<FanControler>().active_state!=1)
        {
            Debug.Log("无法激活！");
        }
        else
        {   
            Debug.Log("激活！");
            transform.parent.transform.GetComponent<FanControler>().active_state = 3;
            OnBulletHit.Invoke();
        }
        
    }
}
