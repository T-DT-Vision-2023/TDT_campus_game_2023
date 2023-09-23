using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text FPSText;
    /*
     * ! 不知道为什么这里没法直接获得TextMesh的指针 
     */
    //public TextMeshPro test;
    void Start()
    {
        FPSText=GetComponent<TMP_Text>() ;
    }

    // Update is called once per frame
    void Update()
    {

        float FPS = 1 / Time.deltaTime;

        FPSText.SetText(FPS.ToString());
        
        /*Debug.Log("test"+FPS);*/
       
    }
}
