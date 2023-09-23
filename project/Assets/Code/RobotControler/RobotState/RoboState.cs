using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Code.RobotControler;
public abstract class  RoboState
{
        
        private RoboControler roboControler;
        public RoboState(RoboControler r)
        {
                this.roboControler = r;
        }
        public RoboState()
        {
                Debug.LogWarning("没有初始的控制器可能导致严重错误！");
        }
        
       
        public abstract void be_atacked();
        
        public abstract void On_update();

        public abstract void enter_state();
        
        public abstract void quite_state();
        
}
