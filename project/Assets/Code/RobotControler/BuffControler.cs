 using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
 using System.Threading;
 using Code.RobotControler;
using Code.RobotControler.RobotState;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Code.util;
 
 //TODO 实际上每个机器人应该都有类似的机制就是应该在身上有传感器，同时还有控制器
public class BuffControler : RoboControler
{

    // Start is called before the first frame update
    public bool buff_rotation = false;
    public float rotationbalace = 1.0f;
    public float time_counter=0.0f;
    private int count = 0;
    
    //buff rotation
    public Transform buff_base;
    public FanControler[] bufffans;
    public RoboState state;
    
    public override void change_state(RoboState state)
    {
        state.quite_state();
        this.state = state;
        this.state.enter_state();
    }
    
    
    void Start()
    {
        this.state = new Buff_activeing(this);
        
        bufffans = new FanControler[5];
        int fan_bum = 0;
        
        //find object
        for (int i = 0; i < transform.childCount; i++)
        {    
            Transform tempchild = transform.GetChild(i);
            
            if (Regex.Match(tempchild.name, @"buff_base").Success)
            {
                buff_base = transform.GetChild(i);
            }
            
        }

        List<Transform> fan_transform = UtilsForGameobject.getallChildren_by_keyword(this.transform, "fan_blade");
        
        if (fan_transform.Count == 5)
        {
            for (int i = 0; i < fan_transform.Count; i++)
            {
                this.bufffans[i] = fan_transform[i].GetComponent<FanControler>();
            }
        }
        //为每个击打区域添加传感器，逻辑思考后应该只给最大的添加碰撞器

        foreach (FanControler fan in bufffans)
        {
            fan.lighbar_rings[0].AddComponent<RingSenser>();
        }
        
        this.state.enter_state();
        
       
        
    }

    // Update is called once per frame
    void Update()
    {
        this.state.On_update();
    }
    
    
    
    
    /// <summary>
    /// return the rotation of the buff_rotation
    /// </summary>
    /// <returns></returns>
    public void buff_rotation_stop()
    {
        buff_rotation=!buff_rotation;

        /*return buff_rotation;*/
    }

    public void buff_rotation_reset()
    {
        this.time_counter = 0.0f;
        buff_rotation=false;
        Debug.Log(this.count);
    }

    
    
    public void randam_active()
    {
            
        int fan_num = Random.Range(0, 5);
      
        
        bufffans[fan_num].active_state=2;
        
        this.state.be_atacked();

    }
    /// <summary>
    /// 保证每次已经激活的并不会激活
    /// </summary>
    public void randam_active_normal()
    {
            
        int fan_num = Random.Range(0, 5);
        int ring_num = Random.Range(0, 9);
        bufffans[fan_num].turn_lightring_onbynum(ring_num);
        
    }
    
    


}
