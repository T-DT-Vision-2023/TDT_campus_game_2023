using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Back_to_start : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnCollisionEnter(Collision other)
    {
        //这个地方可以用来判断大小弹
        if (other.transform.tag=="bullet")
        {
           SceneManager.LoadScene("startUI");
           Cursor.lockState = CursorLockMode.Confined;
           
        }

    }
}
