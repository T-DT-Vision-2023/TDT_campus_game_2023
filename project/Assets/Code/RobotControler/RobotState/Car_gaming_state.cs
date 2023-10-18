using Code.RobotControler.Senser;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

namespace Code.RobotControler.RobotState
{

    public class Car_gaming_state : RoboState
    {
        public enum MoveCommands {
        MOVE_FORWARD
            = 1 << 0,
        // 0000 0001
        MOVE_BACKWARD
            = 1 << 1,
        // 0000 0010
        MOVE_LEFT
            = 1 << 2,
        // 0000 0100
        MOVE_RIGHT
            = 1 << 3,
        // 0000 1000
        ROTATE_LEFT
            = 1 << 4,
        // 0001 0000
        ROTATE_RIGHT
            = 1 << 5
        // 0010 0000
    };
        public CarControler _controler;

        public float rotation_speed = 100f;

        public float time_counter = 0f;

        public float move_speed = 0.5f;
        public float yaw=0.0f;
        public float pitch=0.0f;

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


            // Move2yaw_pitch(yaw, pitch);


        }

       
        
        public void Move(MoveCommands direction)
        {
            switch (direction)
            {
               case MoveCommands.MOVE_FORWARD:
                   _controler.transform.Translate(Vector3.forward*Time.deltaTime*this.move_speed);
                   break;
               case MoveCommands.MOVE_BACKWARD:
                   _controler.transform.Translate(Vector3.back*Time.deltaTime*move_speed);
                   break;
               case MoveCommands.MOVE_LEFT:
                   _controler.transform.Translate(Vector3.left*Time.deltaTime*move_speed);
                   break;
               case MoveCommands.MOVE_RIGHT:
                   _controler.transform.Translate(Vector3.right*Time.deltaTime*move_speed);
                   break;
               case MoveCommands.ROTATE_LEFT:
                   _controler.chassis.Rotate(Vector3.up*Time.deltaTime*rotation_speed);
                   break;
               case MoveCommands.ROTATE_RIGHT:
                   _controler.chassis.Rotate(Vector3.down*Time.deltaTime*rotation_speed);
                   break;
            }
        }

        public void Move2yaw_pitch(float yaw, float pitch)
        {
            this._controler.act_mousex_mousey(150,0);
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