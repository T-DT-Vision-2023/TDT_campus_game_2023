using Code.RobotControler.Senser;
using Code.RobotControler.Senser;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Code.util;

namespace Code.RobotControler.RobotState
{
    public class FakeCameraOutOfControl : CameraState
    {
        
        public float  rotation_sensitivity = 3f ;
        public float move_speed = 10.0f;
        private float xRotation = 0.0f;
        private float yRotation = 0.0f;
        public FakeCamera camera;
        GameObject bullet = Resources.Load<GameObject>("model/bullet_prefab");

        public FakeCameraOutOfControl(FakeCamera f)
        {
            this.camera = f;
        }


        
        public override void On_update()
        {
            //Input.GetAxis("MouseX")获取鼠标移动的X轴的距离
            
            xRotation -= Input.GetAxis("Mouse X")*rotation_sensitivity ;
            yRotation += Input.GetAxis("Mouse Y") * rotation_sensitivity;
            //欧拉角转化为四元数
            Quaternion rotation = Quaternion.Euler(-yRotation, -xRotation, 0);
            camera.transform.rotation = rotation;
            camera.transform.Translate(Vector3.forward * Input.GetAxis("Vertical")*move_speed*Time.deltaTime  );
            camera.transform.Translate(Vector3.right * Input.GetAxis("Horizontal")*move_speed*Time.deltaTime  );
            
            
            if (Input.GetKey("e"))
            {
                camera.transform.Translate(Vector3.up*move_speed*Time.deltaTime);
            }
            if (Input.GetKey("q"))
            {
                camera.transform.Translate(Vector3.down*move_speed*Time.deltaTime);
            }
            
            
            //处理选中物体
            Vector3 screenCenter = new Vector3(0.5f*Screen.width, 0.5f*Screen.height, 0f);
            
           
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);
                RaycastHit hit;
                Transform objectTransform=null;
                if (Physics.Raycast(ray, out hit)) {
                    objectTransform = hit.transform;
                    // 在这里可以使用objectTransform进行一些操作
                    /*Debug.Log(objectTransform.name);*/
                    
                    this.camera.key_message =  UtilsForGameobject.get_granfather(objectTransform).name;
                }
                
                if (Input.GetMouseButtonDown(1)) {
                    //寻找当前物体的父亲物体，并且拿到其中的controler最后进入下一个状态
                    Transform grandpa = UtilsForGameobject.get_granfather(objectTransform);
                    if (objectTransform==null)
                    {
                        //这里可以有提示语
                    }
                    else
                    {
                        if (grandpa.GetComponent<CarControler>()!=null)
                        {
                            change_to_incontrol();
                            this.camera.car = grandpa.GetComponent<CarControler>();
                        }
                        else
                        {
                            //这里也可以有提示
                        }
                        
                    }
                    
                }
                
                if (Input.GetMouseButtonDown(0))
                {
                    GameObject temp_bullet=Transform.Instantiate(this.bullet,this.camera.transform.position,this.camera.transform.rotation);
                    temp_bullet.GetComponent<Rigidbody>().velocity = this.camera.transform.forward*28.0f;
                    UnityEngine.GameObject.Destroy(temp_bullet,5);
                    
                }

        }

        public override void enter_state()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void quite_state()
        {
           
        }

        public void change_to_incontrol()
        {
            CameraState newstate = new FackCameraInControl(this.camera);
            
            newstate.enter_state();
            
            this.camera.camera_state = newstate;
            
        }
    }
}