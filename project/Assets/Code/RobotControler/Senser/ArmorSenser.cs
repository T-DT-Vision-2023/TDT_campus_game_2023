using System;

using UnityEngine;
using UnityEngine.Events;

namespace Code.RobotControler.Senser
{
    public class ArmorSenser : MonoBehaviour
    {
        public UnityEvent OnBulletHit=new UnityEvent();



        public void OnCollisionEnter(Collision other)
        {
            //这个地方可以用来判断大小弹
            
            Debug.Log(other.contacts[0].point);
            Debug.Log("be attacked by bullet!");
            OnBulletHit.Invoke();
            
        
        }
    }
}