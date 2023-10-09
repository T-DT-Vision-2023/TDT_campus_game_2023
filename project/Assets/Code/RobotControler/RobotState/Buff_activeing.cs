using System;

using UnityEngine;
using Random = UnityEngine.Random;
namespace Code.RobotControler.RobotState
{
    public class Buff_activeing : RoboState
    {
        
        private RoboControler roboControler;
        private BuffControler buffcontroler;
        private float timecounter_fortarget=0.0f;
        private int active_num = 0;

        public Buff_activeing(RoboControler r)
        {
            this.roboControler = r;
            this.buffcontroler = (BuffControler)r;
        }
        
        public override void be_atacked()
        {
            
            
            
            //直接点亮所有被激活的
            foreach (FanControler fan in this.buffcontroler.bufffans)
            {
                if (fan.active_state==3)
                {   
                    Debug.LogError("点亮！！！！");
                    fan.enter_actived_mode();
                    active_num += 1;
                    Debug.Log(active_num);
                }
            }
            
            //点亮所有的符要有一个提示！
            //TODO 等待后续代码
            if (active_num>=this.buffcontroler.bufffans.Length)
            {
                Debug.Log(this.buffcontroler.bufffans.Length);
                Debug.LogWarning("all active!");
            }
            else
            {
                //重置倒计时
                //激活成功之后应该为0
                this.timecounter_fortarget = 0;
                
                //随机选择一个状态扇叶激活
                
                int temp_num = Random.Range(0, buffcontroler.bufffans.Length);
                int counter = 0;
                
                //这个地方逻辑有问题
                while (buffcontroler.bufffans[temp_num].active_state!=0)
                {
                    temp_num = Random.Range(0, buffcontroler.bufffans.Length);
                    counter++;
                    if (counter>buffcontroler.bufffans.Length)
                    {
                        
                        Debug.LogError(this.buffcontroler.bufffans.Length);
                        Debug.LogError(active_num);
                        Debug.LogError("算法出现错误！");
                        Debug.LogError(temp_num);
                        Debug.LogError(buffcontroler.bufffans[temp_num].active_state);
                        counter = 0;
                     
                    }

                }
                
                Debug.LogError("当前选中的扇叶"+temp_num);
                buffcontroler.bufffans[temp_num].active_state = 1;
                buffcontroler.bufffans[temp_num].enter_target_mode();
            }
           
            
        }
        
        public  override void On_update()
        {
            
            if (buffcontroler.buff_rotation)
            {
            //保持正常旋转，同时还要保持每次只有一个扇叶可以被激活
                float angle = Random.value * 0.265f + 0.75f;
                float w = Random.value * 0.116f + 1.884f;
                float b = 2.09f - angle;
                float spd = angle * (float)Math.Sin(w * buffcontroler.time_counter) + b;
                buffcontroler.time_counter += Time.deltaTime;
                //这里有问题，理论上应该退出当前模式进入下一个模式
                if (buffcontroler.time_counter>30)
                {
                    buffcontroler.time_counter = 0;
                }
                buffcontroler.buff_base.Rotate(Vector3.left * Time.deltaTime * spd / (float)Math.PI * 180 * buffcontroler.rotationbalace);
            }
            
            //每隔一段时间选择一个新的符
            
            if (active_num<buffcontroler.bufffans.Length)
            {
                this.timecounter_fortarget += Time.deltaTime;
                
            }
            this.timecounter_fortarget += Time.deltaTime;
            if (timecounter_fortarget>5)
            {
               //取消所有装甲状态，然后新增一个状态

               active_num = 0;

               foreach (FanControler fan in buffcontroler.bufffans)
               {
                   fan.active_state = 0;
                   fan.enter_unaactiveable_mode();
               }

               int temp_num = Random.Range(0, buffcontroler.bufffans.Length);
            
               buffcontroler.bufffans[temp_num].active_state = 1;
               buffcontroler.bufffans[temp_num].enter_target_mode();

               this.timecounter_fortarget = 0;

            }

        }
        
    

        public void randam_active()
        {
            
            int fan_num = Random.Range(0, 4);
            int ring_num = Random.Range(0, 9);
            this.buffcontroler.bufffans[fan_num].turn_lightring_onbynum(ring_num);
            

        }
        
        public override void enter_state()
        {
            
            /*
            //确认激活状态
            foreach (FanControler buff_fan in bufffans)
                {
                    if (buff_fan.is_active==true)
                    {
                        active_num += 1;
                    }    
                }
            */
            
            //为每个传感器添加对应的响应函数.传入attacked函数
            for (int i = 0; i < this.buffcontroler.bufffans.Length; i++)
            {
                this.buffcontroler.bufffans[i].lighbar_rings[0].GetComponent<RingSenser>().OnBulletHit.AddListener(this.buffcontroler.state.be_atacked);
            }
            
            
        }

        public override void quite_state()
        {
            
        }
        
    }
}