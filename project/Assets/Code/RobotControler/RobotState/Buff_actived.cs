using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Code.RobotControler.RobotState
{
    public class Buff_actived : RoboState
    {
        private RoboControler roboControler;
        public Buff_actived(RoboControler r)
        {
            this.roboControler = r;
        }
        
     
        
        public override void be_atacked()
        {
            throw new System.NotImplementedException();
        }

        public override void On_update()
        {
           
        }
        
        public override void enter_state()
        {
            
        }

        public override void quite_state()
        {
            
        }
    }
}