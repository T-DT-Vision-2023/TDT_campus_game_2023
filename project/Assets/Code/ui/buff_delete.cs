using UnityEngine;

namespace Code.util
{
    
    
    public class buff_delete: MonoBehaviour
    {

        public GameObject Buff;
        public GameObject buff_base;
        public bool disable = true;
        
        
        public void OnCollisionEnter(Collision other)
        {
            //这个地方可以用来判断大小弹
            if (other.transform.tag=="bullet")
            {
                this.disable = !disable;
                this.Buff.SetActive(disable);
                this.buff_base.SetActive(disable);
           
            }

        }
        
    }
}