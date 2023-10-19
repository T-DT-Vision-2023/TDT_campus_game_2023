using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Reloadscene : MonoBehaviour
{
    public void OnCollisionEnter(Collision other)
    {
        //这个地方可以用来判断大小弹
        if (other.transform.tag=="bullet")
        {
            SceneManager.LoadScene("SampleScene");
        }

    }
}
