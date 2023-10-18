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


namespace Network
{
    public class RecvStruct
    {
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public int Shoot { get; set; }
        public double TimeStamp { get; set; }
        public int RequiredImageWidth { get; set; }
        public int RequiredImageHeight { get; set; }
    }

    public class SendStruct
    {
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public byte[] Img { get; set; }
        public double TimeStamp { get; set; }
        public double EnemyHp { get; set; }
        public double MyHp { get; set; }
        public int RestBullets { get; set; }
        public int RestTime { get; set; }
        public int BuffOverTime { get; set; }
    }

    public class GameManager : MonoBehaviour
    {
        public string ClientIP = "127.0.0.1";
        public int ClientPort = 5559;
        public int ServerPort = 5558;
        // momo: 这里一律使用Client指代C++客户端，Server指代Unity服务端喵

        public int width = 1440;
        public int height = 1080;

        private string serverAddress;
        private string newestMessage;
        
        
        //客户端基本信息
        public bool registeed = false;
        public string team_name = "";
        public string id = "";
        public string time = "";
        public string pwd =  "";
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


        private RecvStruct receivedData = new RecvStruct();
        private SendStruct sendData = new SendStruct();
 

        public RecvStruct GetReceivedData()
        {
            return receivedData;
        }

        public void SetReceivedData(RecvStruct data)
        {
            receivedData = data;
        }

        public SendStruct GetSendData()
        {
            return sendData;
        }

        public void SetSendData(SendStruct data)
        {
            sendData = data;
        }

        private void Start()
        {
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
        }

        private void Update()
        {
            //这个地方只要每一帧获取最新的就可以了
                ReceiveMessages();
                
            if (registeed)
            {
                SendMessage();
                
            }
        }

        private void ReceiveMessages()
        {
            
            /*
             * 这里修改为始终获取最新的一帧
             */
            
            if (recv_msg)
            {
                string tempmessage;

                int counter = 0;
                while (pullSocket.TryReceiveFrameString(out tempmessage))
                {
                   
                    Debug.Log(tempmessage);
                    
                    JObject json = JsonConvert.DeserializeObject<JObject>(tempmessage);
                    Debug.Log((string)json["type"]);

                    switch ((string)json["type"])
                    {

                        case "register":
                            Debug.Log("检测到注册信息");
                            this.registeed = false;
                            handleRegister(json);
                            break;

                        case "regist success!":

                            Debug.Log("注册完成");
                            this.registeed = true;
                            break;

                        default:
                            counter += 1;
                            newestMessage = tempmessage;
                            break;

                    }

                }


                if (counter > 0)
                {
                    Debug.Log($"当前最新消息:{newestMessage}");
                    HandleData(newestMessage);
                }

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
        

        


        private void HandleData(string data)
        {
            var json = JsonConvert.DeserializeObject<JObject>(data);
            receivedData.Yaw = (double)json["yaw"];
            receivedData.Pitch = (double)json["pitch"];
            receivedData.Shoot = (int)json["shoot"];
            receivedData.TimeStamp = (double)json["time_stamp"];
            receivedData.RequiredImageWidth = (int)json["required_image_width"];
            receivedData.RequiredImageHeight = (int)json["required_image_height"];
        }

        private void handleRegister(JObject data)
        {
            this.team_name = (string)data["info"];

            this.id = (string)data["id"];

            this.time = (string)data["time"];

            this.pwd = (string)data["pwd"];

            this.version = (string)data["version"];
            
            Debug.Log($"检测到c++操作端注册，开始尝试返回请求");

            this.pushSocket.TrySendFrame("regist success!");
            
            
        }

        private IEnumerator CaptureAndSendImage()
        {
            while (true)
            {
                if (trans_frame)
                {
                    var screenCapture = ScreenCapture.CaptureScreenshotAsTexture();
              
                    var imgData = screenCapture.EncodeToJPG();
                    Destroy(screenCapture);
                    
                    
                    /*
                     * 这个到底是在干什么？？？？？
                     * 这封装的简直离谱
                     */
                    
                    var sendStruct = GetSendData();
                    sendStruct.Img = imgData;

                    SetSendData(sendStruct);
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
                    ["enemy_hp"] = sendData.EnemyHp,
                    ["my_hp"] = sendData.MyHp,
                    ["rest_bullets"] = sendData.RestBullets,
                    ["rest_time"] = sendData.RestTime,
                    ["buff_over_time"] = sendData.BuffOverTime
                };

                if (sendData.Img != null && sendData.Img.Length > 0)
                {
                    json["img"] = sendData.Img;
                    
                    Debug.Log(json["img"]);
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