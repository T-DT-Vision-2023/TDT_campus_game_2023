using System;
using Code.RobotControler.RobotState;
using Unity.VisualScripting;
using UnityEngine;
using State = Code.RobotControler.RobotState.CameraState;

namespace Code.RobotControler.Senser
{
    //理论上来说现在应该有图传和视觉相机两种
    public class FakeCamera : MonoBehaviour
    {

        public CarControler car=null;

        public State camera_state = null;
        
        public string key_message = "";
        //应该有两个状态，一个是没有控制车的时候，一个是已经控制车的时候
        void OnGUI()
        {
           // GUI.Label(new Rect(0.5f*Screen.width, 0.5f*Screen.height, 100, 20), this.key_message);

        }
        private void Start()
        {
            if (this.car==null)
            {
                this.camera_state = new FakeCameraOutOfControl(this);
            }
            else
            {
                this.camera_state = new FackCameraInControl(this);
            }
            
            this.camera_state.enter_state();
        }

        private void Update()
        {
            
            this.camera_state.On_update();   
            
         
        }
        
        
    }
}