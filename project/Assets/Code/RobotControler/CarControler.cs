using System;
using System.Collections;
using System.Collections.Generic;
using Code.RobotControler;
using Code.RobotControler.RobotState;
using Code.RobotControler.Senser;
using Code.util;
using Unity.VisualScripting;
using UnityEngine;
using FixedUpdate = UnityEngine.PlayerLoop.FixedUpdate;

public class CarControler : RoboControler
{
    public int car_defalt_state = 1;
    public Material[] materials;

    public WheelCollider[] wheelColliders;
    public Transform true_wheel;
    public List<Transform> armors;
    public List<Transform> light_bars;
    public Transform health_bar;
    public HealthBar Healthbar_controler;

    public float motorTorque = 1f;
    public float rotationSpeed = 5f;
    public float forwardExtremumSlip = 0.4f;
    public float forwardExtremumValue = 1.0f;
    public float forwardAsymptoteSlip = 0.8f;
    public float forwardAsymptoteValue = 0.5f;
    public float forwardStiffness = 1f;
    public float sidewaysExtremumSlip = 0.2f;
    public float sidewaysExtremumValue = 1.0f;
    public float sidewaysAsymptoteSlip = 0.5f;
    public float sidewaysAsymptoteValue = 0.75f;
    public float sidewayStiffness = 1f;
    public float camera_x = 0;
    public float camera_y = 0;
    public float camera_z = -0.1f;
    public float rotaion_sensitivity = 1.0f;
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private float steerAngle_temp = 0.0f;

    // 滤波器参数，暂时不要了
    // private KalmanFilter yawFilter;
    // private KalmanFilter pitchFilter;

    // pid参数
    public double yaw_kp, yaw_ki, yaw_kd;
    public double pitch_kp, pitch_ki, pitch_kd;
    private double yaw_last_error, pitch_last_error, yaw_accumulate_error, pitch_accumulate_error;
    public double max_yaw_accmulate_error, max_pitch_accmulate_error;
    private Vector2 targetPosition;

    public double Ayaw_kp, Ayaw_ki, Ayaw_kd;
    public double Apitch_kp, Apitch_ki, Apitch_kd;
    private double Ayaw_last_error, Apitch_last_error, Ayaw_accumulate_error, Apitch_accumulate_error;
    public double Amax_yaw_accmulate_error, Amax_pitch_accmulate_error;
    private Vector2 targetSpeed;

    private IEnumerator inner_pid, outer_pid;
    private IEnumerator cameraMove;
    public float control_cycle_time;
    public float LimitPitch = 70.0f;

    private Vector2 cnt_speed;
    private Vector2 cnt_accSpeed;


    public Transform head;
    public Transform neck;
    public Transform chassis;


    private float rotationInput;
    private float Angle = 0.0f;
    public int little_bullet_damage = 10;

    public string control_mode = "player_mode";

    public string color = "blue";


    private void Start()
    {
        //设置车辆的属性
        for (var i = 0; i < wheelColliders.Length; i++)
        {
            var forwardFriction = new WheelFrictionCurve();

            forwardFriction.extremumSlip = forwardExtremumSlip;
            forwardFriction.extremumValue = forwardExtremumValue;
            forwardFriction.asymptoteSlip = forwardAsymptoteSlip;
            forwardFriction.asymptoteValue = forwardAsymptoteValue;
            forwardFriction.stiffness = forwardStiffness;

            var sidewayFriction = new WheelFrictionCurve();
            sidewayFriction.extremumSlip = sidewaysExtremumSlip;
            sidewayFriction.extremumValue = sidewaysExtremumValue;
            sidewayFriction.asymptoteSlip = sidewaysAsymptoteSlip;
            sidewayFriction.asymptoteValue = sidewaysAsymptoteValue;
            sidewayFriction.stiffness = sidewayStiffness;

            wheelColliders[i].forwardFriction = forwardFriction;
            wheelColliders[i].sidewaysFriction = sidewayFriction;
        }

        head = UtilsForGameobject.getallChildren_by_keyword(transform, "head")[0];
        neck = UtilsForGameobject.getallChildren_by_keyword(transform, "neck")[0];
        chassis = UtilsForGameobject.getallChildren_by_keyword(transform, "fuck")[0];
        armors = UtilsForGameobject.getallChildren_by_keyword(transform, "armor");
        light_bars = UtilsForGameobject.getallChildren_by_keyword(transform, "light_bar");


        // 设定滤波器参数 暂时不要了
        // yawFilter = new KalmanFilter(0.015f, 0.1f, 1f, 0f);
        // pitchFilter = new KalmanFilter(0.015f, 0.1f, 1f, 0f);


        foreach (var armor in armors) armor.AddComponent<ArmorSenser>();

        Healthbar_controler = health_bar.GetComponent<HealthBar>();
        Healthbar_controler.SetMaxHealth((int)blood);


        switch (car_defalt_state)
        {
            case 0:
                break;

            case 1:

                print("初始化车辆自旋状态");

                RoboState temp = new Car_gaming_state(this);

                change_state(temp);

                break;

            case 2:

                print("初始化车辆自旋状态");

                RoboState temp1 = new Car_auto_rotation_state(this);

                change_state(temp1);

                break;
        }

        // 初始化灯条颜色

        change_light_bar_color(color);

        // 初始化PID协程
        inner_pid = innerPID();
        outer_pid = outerPID();
        cameraMove = Move();
        StartCoroutine(inner_pid);
        StartCoroutine(outer_pid);
        StartCoroutine(cameraMove);
    }

    // 理论上来说这里被控制单位不应该有update
    private void Update()
    {
        if (state != null) state.On_update();
    }

    private IEnumerator Move()
    {
        while (true)

        {
            yield return new WaitForSeconds(control_cycle_time);
            if (control_mode != "remote_control_mode") continue;
            var yaw = head.transform.localEulerAngles.y;
            var pitch = head.transform.localEulerAngles.x;
            // if (head.transform.localEulerAngles.z > 0.1f)
            // {
            //     yaw -= 180.0f;
            //     pitch = -(pitch - 180);
            // }

            Debug.Log("before" + new Vector2(yaw, pitch));
            cnt_speed += cnt_speed * control_cycle_time;
            yaw += cnt_speed.x * control_cycle_time;
            pitch += cnt_speed.y * control_cycle_time;
            // yaw = (yaw + 540) % 360 - 180;
            // pitch = (pitch + 540) % 360 - 180;
            // if (pitch > 90) pitch = 90;
            // if (pitch < -90) pitch = -90;
            Debug.Log("cnt" + new Vector2(yaw, pitch));
            // head.transform.rotation =
            //     Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);

            var rotation_head = Quaternion.Euler(yaw, pitch, 0);
            head.localRotation = rotation_head;
        }
    }

    private IEnumerator innerPID()
    {
        while (true)
        {
            yield return new WaitForSeconds(control_cycle_time);
            if (control_mode != "remote_control_mode") continue;
            double cnt_yaw = head.transform.localEulerAngles.y;
            double cnt_pitch = head.transform.localEulerAngles.x;
            if (gameObject.transform.localEulerAngles.z > 0.1f)
            {
                cnt_yaw -= 180.0f;
                cnt_pitch = -(cnt_pitch - 180);
            }

            var error_yaw = targetPosition.x - cnt_yaw;
            error_yaw = (error_yaw + 540) % 360 - 180;
            var error_pitch = targetPosition.y - cnt_pitch;
            error_pitch = (error_pitch + 540) % 360 - 180;
            //Debug.Log("error" + new Vector2((float)error_yaw, (float)error_pitch));
            var set_yaw_acc = yaw_kp * error_yaw + yaw_ki * yaw_accumulate_error +
                              yaw_kd * (error_yaw - yaw_last_error);
            var set_pitch_acc = pitch_kp * error_pitch + pitch_ki * pitch_accumulate_error +
                                pitch_kd * (error_pitch - pitch_last_error);

            yaw_accumulate_error += error_yaw;
            if (Math.Abs(yaw_accumulate_error) > max_yaw_accmulate_error)
            {
                if (yaw_accumulate_error < 0)
                    yaw_accumulate_error = -max_yaw_accmulate_error;
                else
                    yaw_accumulate_error = max_yaw_accmulate_error;
            }

            pitch_accumulate_error += error_pitch;
            if (Math.Abs(pitch_accumulate_error) > max_pitch_accmulate_error)
            {
                if (pitch_accumulate_error < 0)
                    pitch_accumulate_error = -max_pitch_accmulate_error;
                else
                    pitch_accumulate_error = max_pitch_accmulate_error;
            }

            yaw_last_error = error_yaw;
            pitch_last_error = error_pitch;

            targetSpeed = new Vector2((float)set_yaw_acc, (float)set_pitch_acc);
        }
    }

    private IEnumerator outerPID()
    {
        while (true)
        {
            yield return new WaitForSeconds(control_cycle_time);
            if (control_mode != "remote_control_mode") continue;

            var cntSpeed = cnt_speed;
            double error_yaw = targetSpeed.x - cntSpeed.x;
            double error_pitch = targetSpeed.y - cntSpeed.y;
            var set_yaw_acc = Ayaw_kp * error_yaw + Ayaw_ki * Ayaw_accumulate_error +
                              Ayaw_kd * (error_yaw - Ayaw_last_error);
            var set_pitch_acc = Apitch_kp * error_pitch + Apitch_ki * Apitch_accumulate_error +
                                Apitch_kd * (error_pitch - Apitch_last_error);

            Ayaw_accumulate_error += error_yaw;
            if (Math.Abs(Ayaw_accumulate_error) > Amax_yaw_accmulate_error)
            {
                if (Ayaw_accumulate_error < 0)
                    Ayaw_accumulate_error = -Amax_yaw_accmulate_error;
                else
                    Ayaw_accumulate_error = Amax_yaw_accmulate_error;
            }

            Apitch_accumulate_error += error_pitch;
            if (Math.Abs(Apitch_accumulate_error) > Amax_pitch_accmulate_error)
            {
                if (Apitch_accumulate_error < 0)
                    Apitch_accumulate_error = -Amax_pitch_accmulate_error;
                else
                    Apitch_accumulate_error = Amax_pitch_accmulate_error;
            }

            Ayaw_last_error = error_yaw;
            Apitch_last_error = error_pitch;

            UpdateAccSpeed(new Vector2((float)set_yaw_acc, (float)set_pitch_acc));
        }
    }

    public Vector3 get_head_position()
    {
        return head.transform.position;
    }

    public void act_vertical_and_horizontal(float forwardInput, float horizontalinput)
    {
        //还在有一些小问题，但是已经不重要了

        for (var i = 0; i < wheelColliders.Length; i++)
        {
            if (forwardInput > 0)
            {
                //现在角度有问题
                wheelColliders[i].steerAngle = (head.localRotation.eulerAngles.y + Mathf.Rad2Deg *
                    Mathf.Asin(horizontalinput /
                               Mathf.Sqrt(Mathf.Pow(forwardInput, 2) + Mathf.Pow(horizontalinput, 2)))) % 360;
                steerAngle_temp = wheelColliders[i].steerAngle;
            }
            else
            {
                if (forwardInput == 0 && horizontalinput == 0)
                {
                    wheelColliders[i].steerAngle = steerAngle_temp;
                    //一段时间不控制之后回正
                }
                else
                {
                    wheelColliders[i].steerAngle = (head.localRotation.eulerAngles.y - Mathf.Rad2Deg *
                        Mathf.Asin(horizontalinput /
                                   Mathf.Sqrt(Mathf.Pow(forwardInput, 2) + Mathf.Pow(horizontalinput, 2)))) % 360;
                    steerAngle_temp = wheelColliders[i].steerAngle;
                }
            }
            /*Debug.Log("steer angle"+ wheelColliders[i].steerAngle);
            Debug.Log(this.steerAngle_temp);
            Debug.Log("head"+this.head.localRotation.eulerAngles);*/


            if (horizontalinput > 0)
            {
                if (forwardInput > 0)
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput - horizontalinput * motorTorque / 2;
                else
                    wheelColliders[i].motorTorque = -motorTorque / 2 * forwardInput +
                                                    horizontalinput * motorTorque / 2;
            }
            else
            {
                if (forwardInput > 0)
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput + horizontalinput * motorTorque / 2;
                else
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput - horizontalinput * motorTorque / 2;
            }
        }
    }


    public void act_vertical_and_horizontal_in_coordinate_system(float forwardInput, float horizontalinput, float y)
    {
        //还在有一些小问题，但是已经不重要了


        for (var i = 0; i < wheelColliders.Length; i++)
        {
            if (forwardInput > 0)
            {
                //现在角度有问题
                wheelColliders[i].steerAngle = (y + Mathf.Rad2Deg * Mathf.Asin(horizontalinput /
                                                                               Mathf.Sqrt(Mathf.Pow(forwardInput, 2) +
                                                                                   Mathf.Pow(horizontalinput, 2)))) %
                                               360;
                steerAngle_temp = wheelColliders[i].steerAngle;
            }
            else
            {
                if (forwardInput == 0 && horizontalinput == 0)
                {
                    wheelColliders[i].steerAngle = steerAngle_temp;
                    //一段时间不控制之后回正
                }
                else
                {
                    wheelColliders[i].steerAngle = (y - Mathf.Rad2Deg * Mathf.Asin(horizontalinput /
                        Mathf.Sqrt(Mathf.Pow(forwardInput, 2) + Mathf.Pow(horizontalinput, 2)))) % 360;
                    steerAngle_temp = wheelColliders[i].steerAngle;
                }
            }
            /*Debug.Log("steer angle"+ wheelColliders[i].steerAngle);
            Debug.Log(this.steerAngle_temp);
            Debug.Log("head"+this.head.localRotation.eulerAngles);*/


            if (horizontalinput > 0)
            {
                if (forwardInput > 0)
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput - horizontalinput * motorTorque / 2;
                else
                    wheelColliders[i].motorTorque = -motorTorque / 2 * forwardInput +
                                                    horizontalinput * motorTorque / 2;
            }
            else
            {
                if (forwardInput > 0)
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput + horizontalinput * motorTorque / 2;
                else
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput - horizontalinput * motorTorque / 2;
            }
        }
    }

    public void act_vertical_and_horizontal_auto(float forwardInput, float horizontalinput)
    {
        //还在有一些小问题，但是已经不重要了

        for (var i = 0; i < wheelColliders.Length; i++)
        {
            if (forwardInput > 0)
            {
                //现在角度有问题
                wheelColliders[i].steerAngle = head.localRotation.eulerAngles.y + Mathf.Rad2Deg *
                    Mathf.Asin(horizontalinput /
                               Mathf.Sqrt(Mathf.Pow(forwardInput, 2) + Mathf.Pow(horizontalinput, 2)));
                steerAngle_temp = wheelColliders[i].steerAngle;
            }
            else
            {
                if (forwardInput == 0 && horizontalinput == 0)
                {
                    wheelColliders[i].steerAngle = steerAngle_temp;
                    //一段时间不控制之后回正
                }
                else
                {
                    wheelColliders[i].steerAngle = +head.localRotation.eulerAngles.y - Mathf.Rad2Deg *
                        Mathf.Asin(horizontalinput /
                                   Mathf.Sqrt(Mathf.Pow(forwardInput, 2) + Mathf.Pow(horizontalinput, 2)));
                    steerAngle_temp = wheelColliders[i].steerAngle;
                }
            }


            if (horizontalinput > 0)
            {
                if (forwardInput > 0)
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput - horizontalinput * motorTorque / 2;
                else
                    wheelColliders[i].motorTorque = -motorTorque / 2 * forwardInput +
                                                    horizontalinput * motorTorque / 2;
            }
            else
            {
                if (forwardInput > 0)
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput + horizontalinput * motorTorque / 2;
                else
                    wheelColliders[i].motorTorque =
                        -motorTorque / 2 * forwardInput - horizontalinput * motorTorque / 2;
            }
        }
    }


    public void act_mousex_mousey(float mouse_x, float mouse_y)
    {
        xRotation += mouse_x * rotaion_sensitivity;
        yRotation += mouse_y * rotaion_sensitivity;
        //欧拉角转化为四元数
        var rotation_head = Quaternion.Euler(yRotation, xRotation, 0);
        var rotation_neck = Quaternion.Euler(-90, xRotation, 0);

        var rotation_chassis = Quaternion.Euler(0, xRotation, 0);

        head.localRotation = rotation_head;
        neck.localRotation = rotation_neck;
        //chassis.localRotation = rotation_chassis;
    }

    // public void move_to_aw_pich_direct(float yaw, float pitch)
    // {
    //     var filteredYaw = yawFilter.Update(yaw);
    //     var filteredPitch = pitchFilter.Update(pitch);
    //     var rotation_head = Quaternion.Euler(filteredYaw, filteredPitch, 0);
    //     head.localRotation = rotation_head;


    public float get_head_pitch()
    {
        return head.localRotation.eulerAngles.x;
    }

    public float get_head_yaw()
    {
        return head.localRotation.eulerAngles.y;
    }

    public void UpdateSpeed(Vector2 newSpeed)
    {
        cnt_speed = newSpeed;
    }

    public Vector2 GetSpeed()
    {
        return cnt_speed;
    }

    public Vector2 UpdateAccSpeed(Vector2 newAccSpeed)
    {
        cnt_speed = newAccSpeed;
        return cnt_accSpeed;
    }

    public Vector2 GetAccSpeed()
    {
        var u1 = new System.Random().NextDouble();
        var u2 = new System.Random().NextDouble();
        var z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);
        return cnt_accSpeed * 1 / (1 + (float)z / 10);
    }

    public Vector2 SetTargetPosition(Vector2 newTargetPosition)
    {
        var signal = newTargetPosition.y > 0 ? 1 : -1;

        newTargetPosition.y = Mathf.Abs(newTargetPosition.y) > LimitPitch ? signal * LimitPitch : newTargetPosition.y;
        targetPosition = newTargetPosition;
        return targetPosition;
    }

    public Vector2 GetTargetPosition()
    {
        return targetPosition;
    }

    public Vector2 SetTargetSpeed(Vector2 newTargetSpeed)
    {
        targetSpeed = newTargetSpeed;
        return newTargetSpeed;
    }

    public Vector2 GetTargetSpeed()
    {
        return targetSpeed;
    }

    public void Reset_Rotation()
    {
        // 将 xRotation 和 yRotation 重置为 0
        xRotation = 0.0f;
        yRotation = 0.0f;

        // 将头部和脖子的旋转重置为初始状态
        var rotation_head = Quaternion.Euler(0, 0, 0); // 0度旋转
        var rotation_neck = Quaternion.Euler(-90, 0, 0); // -90度旋转（如果脖子需要调整）

        head.localRotation = rotation_head;
        neck.localRotation = rotation_neck;
    }


    public void change_blood(int value)
    {
        blood += value;
        Healthbar_controler.SetHealth((int)blood);
    }


    public override void change_state(RoboState state)
    {
        state.quite_state();

        this.state = state;
        this.state.enter_state();
    }

    public void change_light_bar_color(string color)
    {
        switch (color)
        {
            case "blue":
                this.color = color;


                for (var i = 0; i < light_bars.Count; i++)
                    light_bars[i].gameObject.GetComponent<Renderer>().material = materials[0];
                break;
            case "red":

                this.color = color;
                for (var i = 0; i < light_bars.Count; i++)
                    light_bars[i].gameObject.GetComponent<Renderer>().material = materials[1];

                break;

            default:
                Debug.LogWarning("没有对应的材质:" + color);

                break;
        }
    }
}