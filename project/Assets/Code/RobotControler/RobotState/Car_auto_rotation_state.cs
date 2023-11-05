using Code.RobotControler.Senser;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

namespace Code.RobotControler.RobotState
{

    public class Car_auto_rotation_state : RoboState
    {
     
        public CarControler _controler;

        public float rotation_speed = 1f;
        

        public float time_counter = 0f;
        public float move_speed = 0.5f;
        private float move_change_time = 2;
        private bool change = true;
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
            time_counter += Time.deltaTime;
            
            this._controler.chassis.Rotate(Vector3.up*rotation_speed);

            if (time_counter > this.move_change_time)
            {
                this.change = !this.change;

                time_counter = 0;
                
            }
            
            if (change)
            {
                this._controler.act_vertical_and_horizontal(0,1);
            }
            else
            {
                this._controler.act_vertical_and_horizontal(0,-1);

            }
            
            //尝试自动同步

            if (_controler.color=="red")
            {
                Network.GameManager.sendData.RedHp = this._controler.blood;
            }
            else
            {
                Network.GameManager.sendData.BlueHP = this._controler.blood;
            }
            

        }

        
        public override void enter_state()
        {
            foreach (Transform armor in _controler.armors)
            {
                armor.GetComponent<ArmorSenser>().OnBulletHit.AddListener(this._controler.state.be_atacked);
            }
           
            
            
        }

        public override void quite_state()
        {
            
        }

    }
}