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

    private KalmanFilter yawFilter;
    private KalmanFilter pitchFilter;


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
        

        // 设定滤波器参数
        yawFilter = new KalmanFilter(0.015f, 0.1f, 1f, 0f);
        pitchFilter = new KalmanFilter(0.015f, 0.1f, 1f, 0f);

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
        
        //初始化灯条颜色
        
        change_light_bar_color(this.color);
    }

    //理论上来说这里被控制单位不应该有update
    private void Update()
    {
        if (state != null) state.On_update();
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

    public void move_to_aw_pich_direct(float yaw, float pitch)
    {
        var filteredYaw = yawFilter.Update(yaw);
        var filteredPitch = pitchFilter.Update(pitch);
        var rotation_head = Quaternion.Euler(filteredYaw, filteredPitch, 0);
        head.localRotation = rotation_head;
    }

    public float get_head_pitch()
    {
        return head.localRotation.eulerAngles.x;
    }

    public float get_head_yaw()
    {
        return head.localRotation.eulerAngles.y;
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
                

                for (int i = 0; i < this.light_bars.Count; i++)
                {
                
                    this.light_bars[i].gameObject.GetComponent<Renderer>().material = materials[0];
                }
                break;
            case "red":

                this.color = color;
                for (int i = 0; i < this.light_bars.Count; i++)
                {
                    this.light_bars[i].gameObject.GetComponent<Renderer>().material = materials[1];
                }
                
                break;
            
            default:
                Debug.LogWarning("没有对应的材质:"+ color);
                
                break;
            
        }
    }
}