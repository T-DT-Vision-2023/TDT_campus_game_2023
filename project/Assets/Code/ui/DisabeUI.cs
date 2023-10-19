using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisabeUI : MonoBehaviour
{
    public GameObject canvas;

    private bool has_diabled=false;
    
    // Start is called before the first frame update
    public void OnCollisionEnter(Collision other)
    {
        //这个地方可以用来判断大小弹
        if (!has_diabled)
        {
           canvas.SetActive(false);
           has_diabled = !has_diabled;
        }
        else
        {
            canvas.SetActive(true);
            has_diabled = !has_diabled;
        }

    }
}
