using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Code.RobotControler.Senser;
using UnityEngine;
using Code.util;

namespace Code.RobotControler.RobotState
{
    public class FackCameraInControl : CameraState
    {
        public float rotation_sensitivity = 3f;
        public float move_speed = 10.0f;
        private float xRotation = 0.0f;
        private float yRotation = 0.0f;
        public FakeCamera camera;
        public float speed = 5.0f;
        private GameObject bullet = Resources.Load<GameObject>("model/bullet_prefab");
        public float shoot_time = 0.0f;
        public float shoot_time_interval = 0.2f;
        public string control_mode = "player_mode";
        public Quaternion start_coordinate;

        private float fix_y;

        public FackCameraInControl(FakeCamera f)
        {
            camera = f;
        }

        public override void On_update()
        {
            //通过不断的计算最短路径可以确定方向可以得到一个比较好的前进曲线//如果大于一个值就采用位移，小于一个值就采用固定位置 


            var camara_position_fix =
                new Vector3(camera.car.camera_x, camera.car.camera_y, camera.car.camera_z);
            var trans_vector = camera.car.get_head_position() + camara_position_fix -
                               camera.transform.position;
            if (trans_vector.magnitude > 1f) // 你可以根据需要调整这个阈值
            {
                // trans_vector=trans_vector.normalized;
                /*this.camera.transform.LookAt(this.camera.car.head_position);*/
                camera.transform.position = camera.transform.position + trans_vector * Time.deltaTime * speed;
            }
            else
            {
                camera.car.control_mode = control_mode;


                switch (control_mode)
                {
                    case "player_mode":
                        //进入控制模式
                        camera.transform.position = camera.car.get_head_position() + camara_position_fix;

                        camera.transform.rotation =
                            UtilsForGameobject.GetLocalRotation(camera.car.head, new Vector3(0, 180, 0));

                        // 获取各种输入并且调用函数

                        camera.car.act_vertical_and_horizontal(Input.GetAxis("Vertical"),
                            Input.GetAxis("Horizontal"));

                        camera.car.act_mousex_mousey(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

                        shoot_time += Time.deltaTime;
                        if (Input.GetMouseButtonDown(0))
                            if (shoot_time > shoot_time_interval)
                            {
                                shoot_time = 0.0f;

                                var fix_position = camera.transform.position +
                                                   0.4f * camera.transform.forward;

                                var temp_bullet = Object.Instantiate(bullet, fix_position,
                                    camera.transform.rotation);
                                temp_bullet.GetComponent<Rigidbody>().velocity = camera.transform.forward * 28.0f;
                                Object.Destroy(temp_bullet, 5);
                            }


                        break;

                    case "remote_control_mode":
                        //进入控制模式

                        // 获取各种输入并且调用函数

                        //加一个虚拟云台用来解决这个问题！！！！！！！！！
                        //this.camera.car.act_vertical_and_horizontal_in_coordinate_system(Input.GetAxis("Vertical"),Input.GetAxis("Horizontal"),start_coordinate.eulerAngles.y);

                        //this.camera.car.act_mousex_mousey(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
                        /*
                         * 这里需要记录一下现在底盘相对于一开始的旋转
                         */
                        var temp = new Vector2(Network.GameManager.receivedData.Yaw,
                            Network.GameManager.receivedData.Pitch);
                        camera.car.SetTargetPosition(temp);


                        shoot_time += Time.deltaTime;
                        if (Network.GameManager.receivedData.Shoot == 1)
                            if (shoot_time > shoot_time_interval)
                            {
                                shoot_time = 0.0f;

                                var fix_position = camera.transform.position +
                                                   0.4f * camera.transform.forward;

                                var temp_bullet = Object.Instantiate(bullet, fix_position,
                                    camera.transform.rotation);
                                temp_bullet.GetComponent<Rigidbody>().velocity = camera.transform.forward * 28.0f;
                                Object.Destroy(temp_bullet, 5);
                            }

                        //设置最新消息
                        //这个地方的设计理念是，谁修改了谁自己管

                        Network.GameManager.sendData.TimeStamp = Time.time;
                        Network.GameManager.sendData.Yaw = camera.car.get_head_yaw();
                        Network.GameManager.sendData.Pitch = camera.car.get_head_pitch();

                        if (camera.car.color == "blue")
                            Network.GameManager.sendData.BlueHP = camera.car.blood;
                        else
                            Network.GameManager.sendData.RedHp = camera.car.blood;


                        break;
                }


                //连续三次按下快捷键弹出车辆
            }
            // Vector3 position = this.camera.car.head_position + new Vector3(this.camera.car.camera_x,
            //    this.camera.car.camera_y, this.camera.car.camera_z);

            // this.camera.transform.position = position;


            if (Input.GetKeyDown("r"))
                if (camera.car != null)
                {
                    RoboState newstate = new Car_auto_rotation_state(camera.car);
                    // newstate.enter_state();
                    camera.car.change_state(newstate);
                }


            if (Input.GetKeyDown("g"))
                if (camera.car != null)
                {
                    RoboState newstate = new Car_gaming_state(camera.car);
                    // newstate.enter_state();
                    camera.car.change_state(newstate);
                }

            //摄像机退出控制状态
            if (Input.GetKeyDown("l"))
            {
                CameraState new_state = new FakeCameraOutOfControl(camera);

                new_state.enter_state();

                camera.change_state(new_state);
            }

            if (Input.GetKeyDown("i"))
            {
                /*
                 * 这里有个问题，如何能够以第三视角查看车辆呢？
                 */

                if (control_mode == "player_mode")
                    control_mode = "remote_control_mode";
                else
                    control_mode = "player_mode";
            }
        }

        public override void enter_state()
        {
            Debug.Log("控制车辆");
            start_coordinate = camera.car.head.localRotation;

            Debug.Log(start_coordinate.eulerAngles);
        }

        public override void quite_state()
        {
            Debug.Log("离开车辆");
        }
    }
}