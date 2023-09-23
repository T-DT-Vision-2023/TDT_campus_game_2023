using Code.RobotControler.Senser;
using UnityEngine;

namespace Code.RobotControler
{
    public  abstract class RoboControler : MonoBehaviour
    {
        public float blood;
        
        public Transform controler_transform;

        //等待添加摄像机类型，暂时还没有想好,用于机器人控制和画面传输，理想情况下应该是fackcamera类采取测略模式然后能够帮助进行输入
        public RoboState state;
            
        public FakeCamera fackcamera;

        public abstract void change_state(RoboState state);
        
        
        //添加通用方法
        
    }
}