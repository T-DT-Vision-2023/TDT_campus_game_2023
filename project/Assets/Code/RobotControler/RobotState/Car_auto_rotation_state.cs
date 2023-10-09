using UnityEngine;

namespace Code.RobotControler.RobotState
{
    public class Car_auto_rotation_state : RoboState
    {
        public CarControler _controler;

        public float rotation_speed = 100f;

        public float time_counter = 0f;

        public Car_auto_rotation_state(CarControler controler)
        {
            this._controler = controler;

        }
    
        public override void be_atacked()
        {
           
        }

        public override void On_update()
        {
            time_counter += Time.deltaTime;
            
            _controler.chassis.Rotate(Vector3.up*Time.deltaTime*rotation_speed);

        }

        public override void enter_state()
        {
            
        }

        public override void quite_state()
        {
            
        }
        

    }
}