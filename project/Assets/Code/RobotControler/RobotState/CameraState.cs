using Code.RobotControler.Senser;

namespace Code.RobotControler.RobotState
{
    public abstract class CameraState
    {
        public FakeCamera camera;

        
        public abstract void On_update();
        
        public abstract void enter_state();
        
        public abstract void quite_state();
    }
}