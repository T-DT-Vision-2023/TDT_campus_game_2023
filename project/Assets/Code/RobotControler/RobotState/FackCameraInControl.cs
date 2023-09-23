using Code.RobotControler.Senser;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Code.util;

namespace Code.RobotControler.RobotState
{
    public class FackCameraInControl : CameraState
    {
        
        public float  rotation_sensitivity = 3f ;
        public float move_speed = 10.0f;
        private float xRotation = 0.0f;
        private float yRotation = 0.0f;
        public FakeCamera camera;
        public float speed = 5.0f;
        
        //用于退出控制状态
        private int quit_count = 0;
        private float quit_time_counter = 0.0f;
        
        public FackCameraInControl(FakeCamera f)
        {
            
            this.camera = f;
            
        }

        public override void On_update()
        {
            //通过不断的计算最短路径可以确定方向可以得到一个比较好的前进曲线//如果大于一个值就采用位移，小于一个值就采用固定位置 
            
           
            Vector3 camara_position_fix =
                new Vector3(this.camera.car.camera_x, this.camera.car.camera_y, this.camera.car.camera_z);
            Vector3 trans_vector = this.camera.car.get_head_position()+camara_position_fix - this.camera.transform.position;
            if (trans_vector.magnitude > 1f) // 你可以根据需要调整这个阈值
            {
                // trans_vector=trans_vector.normalized;
                /*this.camera.transform.LookAt(this.camera.car.head_position);*/
                this.camera.transform.position=this.camera.transform.position+trans_vector*Time.deltaTime*speed;
                
               
            }
            else
            {
                //进入控制模式
                this.camera.transform.position = this.camera.car.get_head_position()+camara_position_fix;

                this.camera.transform.rotation =
                    UtilsForGameobject.GetLocalRotation(this.camera.car.head, new Vector3(0, 180, 0));
                
                // 获取各种输入并且调用函数
             
                this.camera.car.act_vertical_and_horizontal(Input.GetAxis("Vertical"),Input.GetAxis("Horizontal"));
                
                this.camera.car.act_mousex_mousey(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
                
                
                //连续三次按下快捷键弹出车辆
                
            }
            // Vector3 position = this.camera.car.head_position + new Vector3(this.camera.car.camera_x,
            //    this.camera.car.camera_y, this.camera.car.camera_z);
            
            // this.camera.transform.position = position;
            if (Input.GetKeyDown("l"))
            {
                
            }

        }

        public override void enter_state()
        {

            this.camera.key_message = "";



        }

        public override void quite_state()
        {
            
        }
        
    }
}