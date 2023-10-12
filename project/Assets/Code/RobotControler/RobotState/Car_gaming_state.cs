using Code.RobotControler.Senser;
using UnityEngine;

namespace Code.RobotControler.RobotState
{
    public class Car_gaming_state : RoboState
    {
        public CarControler _controler;
        public float rotation_speed = 3f;
        public float gun_move_speed = 3f;
        public float time_counter = 0f;

        public Car_gaming_state(CarControler controler)
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

            _controler.chassis.Rotate(Vector3.up*Time.deltaTime*rotation_speed);


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