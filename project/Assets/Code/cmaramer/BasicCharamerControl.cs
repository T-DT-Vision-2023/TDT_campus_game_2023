using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharamerControl : MonoBehaviour
{
    public float  rotation_sensitivity = 3f ;
    public float move_speed = 10.0f;
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        // Cursor.lockState = CursorLockMode.Confined;
        // Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        // //Input.GetAxis("MouseX")获取鼠标移动的X轴的距离
        // xRotation -= Input.GetAxis("Mouse X")*rotation_sensitivity ;
        // yRotation += Input.GetAxis("Mouse Y") * rotation_sensitivity;
        // //欧拉角转化为四元数
        // Quaternion rotation = Quaternion.Euler(-yRotation, -xRotation, 0);
        // transform.rotation = rotation;
        //
        //
        // transform.Translate(Vector3.forward * Input.GetAxis("Vertical")*move_speed*Time.deltaTime  );
        // transform.Translate(Vector3.right * Input.GetAxis("Horizontal")*move_speed*Time.deltaTime  );
        //
        //
        // if (Input.GetKey("e"))
        // {
        //  transform.Translate(Vector3.up*move_speed*Time.deltaTime);
        // }
        // if (Input.GetKey("q"))
        // {
        //     transform.Translate(Vector3.down*move_speed*Time.deltaTime);
        // }
        //

        

    }
}
