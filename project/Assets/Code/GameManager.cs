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
        public string ClientIP = "localhost";
        public int ClientPort = 5559;
        public int ServerPort = 5558;
        // momo: 这里一律使用Client指代C++客户端，Server指代Unity服务端喵

        public int width = 1440;
        public int height = 1080;

        private string clientAddress;
        private PushSocket pushSocket;
        private PullSocket pullSocket;
        private Thread receiveThread;
        private Thread sendThread;
        private bool recv_msg = true;
        private bool send_msg = true;
        private bool trans_frame = true;
        private bool is_registered = false;
        private DateTime lastPulseReceivedTime;


        private RecvStruct receivedData = new();
        private SendStruct sendData = new();

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
            pushSocket.Bind($"tcp://*:{ServerPort}");

            clientAddress = $"tcp://{ClientIP}:{ClientPort}";
            pullSocket = new PullSocket();
            pullSocket.Options.ReceiveHighWatermark = 1500;
            pullSocket.Connect(clientAddress);

            receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            StartCoroutine(CaptureAndSendImage());
        }

        private void ReceiveMessages()
        {
            while (recv_msg)
            {
                if (pullSocket.TryReceiveFrameString(out var header))
                {
                    Debug.Log($"Received message: {header}");

                    switch (header)
                    {
                        case "msg":
                        {
                            if (pullSocket.TryReceiveFrameString(out var message)) HandleMessage(message);
                            break;
                        }
                        case "data":
                        {
                            if (pullSocket.TryReceiveFrameString(out var data)) HandleData(data);
                            break;
                        }
                    }
                }

                if (!is_registered)
                {
                    Debug.Log("未注册，等待客户端注册喵");
                    Thread.Sleep(1000);
                    continue;
                }

                if (!((DateTime.Now - lastPulseReceivedTime).TotalSeconds > 5)) return; // 5秒未收到心跳包
                Debug.Log("未接收到心跳包，客户端可能离线捏");
                Thread.Sleep(1000);
                Debug.Log("尝试重新连接客户端喵");
            }
        }


        private void HandleMessage(string message)
        {
            var json = JsonConvert.DeserializeObject<JObject>(message);

            switch (json["type"]?.ToString())
            {
                case "register":
                {
                    Debug.Log("Register success~");

                    is_registered = true;
                    sendThread = new Thread(SendMessage);
                    sendThread.Start();

                    var responseJson = "{\"type\":\"register success\"}";
                    pushSocket.SendFrame("msg", true);
                    pushSocket.SendFrame(responseJson);
                    break;
                }
                case "offline":
                {
                    Debug.Log("Client offline~");

                    var responseJson = "{\"type\":\"offline success\"}";
                    pushSocket.SendFrame("msg", true);
                    pushSocket.SendFrame(responseJson);
                    break;
                }
                case "pulse":
                    lastPulseReceivedTime = DateTime.Now; // 更新上一次接收到心跳包的时间
                    break;
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

        private IEnumerator CaptureAndSendImage()
        {
            while (true)
            {
                if (trans_frame)
                {
                    var screenCapture = ScreenCapture.CaptureScreenshotAsTexture();
                    var imgData = screenCapture.EncodeToJPG();
                    Destroy(screenCapture);

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
            while (send_msg)
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
                    json["img"] = Convert.ToBase64String(sendData.Img);

                var message = json.ToString();
                pushSocket.SendFrame("data", true);
                pushSocket.SendFrame(message);
            }
        }


        private void OnDestroy()
        {
            send_msg = false;
            recv_msg = false;
            if (sendThread != null && sendThread.IsAlive)
                sendThread.Join();
            if (receiveThread != null && receiveThread.IsAlive)
                receiveThread.Join();
            pushSocket.Close();
            pullSocket.Close();
            NetMQConfig.Cleanup();
        }
    }
}