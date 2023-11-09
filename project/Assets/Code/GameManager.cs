using System;
using System.Collections;
using System.Collections.Generic;
using Code.util;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using TMPro;
using UnityEngine.UI;


namespace Network
{
    public class GameManager : MonoBehaviour
    {
        /*!
         这个类使用单例模式！！
         */
        public static Gamemanageer game_manager;

        public static Gamemanageer GetInstance
        {
            get
            {
                if (game_manager == null) game_manager = new GameObject("GameManager").AddComponent<Gamemanageer>();

                return game_manager;
            }
        }

        public string ClientIP = "127.0.0.1";
        public int ClientPort = 5559;
        public int ServerPort = 5558;
        // momo: 这里一律使用Client指代C++客户端，Server指代Unity服务端喵

        public int width = 1440;
        public int height = 1080;

        private string serverAddress;


        //客户端基本信息
        public bool registeed = false;
        public string team_name = "";
        public string id = "";
        public string time = "";
        public string pwd = "";
        public string version = "";

        private byte[] imgData;

        //


        private PushSocket pushSocket;
        private PullSocket pullSocket;

        private bool recv_msg = true;
        private bool send_msg = true;
        private bool trans_frame = true;
        private bool is_registered = false;
        private DateTime lastPulseReceivedTime;


        public static RecvStruct receivedData = new();
        public static SendStruct sendData = new();
        public string newestMessage;

        public RenderTexture _renderTexture;
        
        
        
        //ui

        public TMP_Text tip_text;

        public static float in_time = 0;

        public static float out_time = 0;

        public static bool control_state = false;

        public static bool game_mode = false;

        public static float time_total = 0;
        
        //score

        private static float score = 0;
        
        // game

        public GameObject buff;
        public GameObject buff_base;
        
        
        private void Start()
        {
            
            
            /*Resolution[] resolutions = Screen.resolutions;
            //设置当前分辨率
            Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);
            Screen.fullScreen = true; //设置成全屏, */
            
            _renderTexture=new RenderTexture(640, 480, 3);

            Debug.Log("NetworkManager Start 没有输出？？？？");
            Screen.SetResolution(width, height, false);

            pushSocket = new PushSocket();
            pushSocket.Options.SendHighWatermark = 1500;
            pushSocket.Connect($"tcp://{ClientIP}:{ClientPort}");


            serverAddress = $"tcp://*:{ServerPort}";
            pullSocket = new PullSocket();
            pullSocket.Options.ReceiveHighWatermark = 1500;
            pullSocket.Bind(serverAddress);

            StartCoroutine(CaptureAndSendImage());
            StartCoroutine(ReceiveMessagesCoroutine());
        }


        private void Update()
        {
            //这里增加对鼠标的基本控制和锁定

            if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;

            if (Input.GetMouseButtonDown(0)) Cursor.lockState = CursorLockMode.Locked;

            if (registeed) SendMessage();
            
            //update UI
            if (control_state)
            {
                tip_text.text = "Time:"+(sendData.TimeStamp-in_time+time_total)+"\n score:"+score;
            
                game_score_manage();
                
            }
            
            
            // UI data score 
            
            
            
        }

        private IEnumerator ReceiveMessagesCoroutine()
        {
            while (recv_msg)
            {
                string tempmessage;
                
                var json_message = new JObject();

                var counter = 0;
                while (pullSocket.TryReceiveFrameString(out tempmessage))
                {
                    json_message = JsonConvert.DeserializeObject<JObject>(tempmessage);

                    switch ((string)json_message["type"])
                    {
                        case "register":
                            Debug.Log("检测到注册信息");
                            registeed = false;
                            handleRegister(json_message);
                            break;

                        case "regist success!":
                            Debug.Log("注册完成");
                            registeed = true;
                            break;

                        case "control":
                            counter += 1;
                            newestMessage = tempmessage;
                            break;
                    }
                }

                if (counter > 0)
                {
                    Debug.Log($"当前最新消息:{newestMessage}");
                    HandleData(json_message);
                }

                // 等待直到下一帧
                yield return null;
            }
        }


        private void ReceiveRawMessages()
        {
            /*
             * 这里修改为始终获取最新的一帧
             */

            if (recv_msg)
            {
                string tempmessage;
                while (pullSocket.TryReceiveFrameString(out tempmessage))
                {
                    Debug.Log(tempmessage);
                    newestMessage = tempmessage;
                }
            }
        }


        private void HandleData(JObject json)
        {
            receivedData.Yaw = (float)json["yaw"];
            receivedData.Pitch = (float)json["pitch"];
            receivedData.Shoot = (int)json["shoot"];
            receivedData.TimeStamp = (float)json["time_stamp"];
            receivedData.RequiredImageWidth = (int)json["required_image_width"];
            receivedData.RequiredImageHeight = (int)json["required_image_height"];
        }

        private void handleRegister(JObject data)
        {
            team_name = (string)data["info"];

            id = (string)data["id"];

            time = (string)data["time"];

            pwd = (string)data["pwd"];

            version = (string)data["version"];

            Debug.Log($"检测到c++操作端注册，开始尝试返回请求");

            pushSocket.TrySendFrame("regist success!");
        }

        private IEnumerator CaptureAndSendImage()
        {
            
            
            while (true)
            {
                if (trans_frame & registeed)
                {
                    
                   
                    var screenCapture = ScreenCapture.CaptureScreenshotAsTexture();
                    //ScreenCapture.CaptureScreenshotIntoRenderTexture(_renderTexture);
                    //这个地方可以用多线程加速
                    
                    var imgData = screenCapture.EncodeToJPG();
             
                    Destroy(screenCapture);

                    sendData.Img = imgData;
                    
                    
                }

                yield return null;
                // 这里可以使用 yield return new WaitForSeconds(0.01f) 来控制获取图像的频率
            }
        }

        public void SendMessage()
        {
            if (send_msg)
            {
                var json = new JObject
                {
                    ["yaw"] = sendData.Yaw,
                    ["pitch"] = sendData.Pitch,
                    ["time_stamp"] = sendData.TimeStamp,
                    ["blue_hp"] = sendData.BlueHP,
                    ["red_hp"] = sendData.RedHp,
                    ["rest_bullets"] = sendData.RestBullets,
                    ["rest_time"] = sendData.RestTime,
                    ["buff_over_time"] = sendData.BuffOverTime
                };

                if (sendData.Img != null && sendData.Img.Length > 0)
                {
                    json["img"] = Convert.ToBase64String(sendData.Img);


                    json["hasimg"] = true;
                    /*
                     * 每次发送完都需要设置为null，否则就会重复发送画面,但是其实重复发送也没什么
                     */

                    sendData.Img = null;
                }
                else
                {
                    json["hasimg"] = false;
                }

                json["end"] = "this is a end!!!!!!!!!";

                pushSocket.TrySendFrame(json.ToString());
            }
        }

        public static void enter_control()
        {
            in_time = sendData.TimeStamp;

            control_state = !control_state;
        }

        public static void quit_control()
        {
            out_time = sendData.TimeStamp;

            time_total += sendData.TimeStamp - in_time;
            
            control_state = !control_state;
            
        }

        public void game_score_manage()
        {
            if ((sendData.TimeStamp-in_time+time_total)>30)
            {
                //解算大符分数
                this.buff.SetActive(false);
                this.buff_base.SetActive(false);
            }
            
        }

        public static void change_score(float score_add)
        {
            score += score_add;
            
        }


        private void OnDestroy()
        {
            send_msg = false;
            recv_msg = false;

            pushSocket.Close();
            pullSocket.Close();
            NetMQConfig.Cleanup();
        }
    }
}