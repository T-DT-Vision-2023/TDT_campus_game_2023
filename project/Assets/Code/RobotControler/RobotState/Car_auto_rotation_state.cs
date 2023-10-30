using Code.RobotControler.Senser;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

namespace Code.RobotControler.RobotState
{

    public class Car_auto_rotation_state : RoboState
    {
     
        public CarControler _controler;

        public float rotation_speed = 100f;

        public float time_counter = 0f;

        public float move_speed = 0.5f;
        public float yaw=0.0f;
        public float pitch=0.0f;

        public Car_auto_rotation_state(CarControler controler)
        {
            this._controler = controler;
        }
    
        public override void be_atacked()
        {
           this._controler.change_blood(-this._controler.little_bullet_damage);
        }

        public override void On_update()
        {
            
            // test

        }

        
        public override void enter_state()
        {
            foreach (Transform armor in _controler.armors)
            {
                armor.GetComponent<ArmorSenser>().OnBulletHit.AddListener(this._controler.state.be_atacked);
            }
            this._controler.Reset_Rotation();
        }

        public override void quite_state()
        {
            
        }

    }
}