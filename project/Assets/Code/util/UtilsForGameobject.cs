using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Code.util
{
    public class UtilsForGameobject
    {
        public static List<Transform> getallChildren(Transform father)
        {
            List<Transform> result = new List<Transform>();
            //use bfs to get all children
            Queue<Transform> q = new Queue<Transform>();
            q.Enqueue(father);
            while (q.Count!=0)
            {
                Transform temp = q.Dequeue();
               
                result.Add(temp);
               
                Debug.Log(temp.name);
                
                for (int i = 0; i < temp.childCount; i++)
                {   
                    q.Enqueue(temp.GetChild(i));
                }
            }
            

            return result;
        }
        
        public static List<Transform> getallChildren_by_keyword(Transform father,string keyword)
        {
            List<Transform> result = new List<Transform>();
            //use bfs to get all children
            Queue<Transform> q = new Queue<Transform>();
            q.Enqueue(father);
            while (q.Count!=0)
            {
                Transform temp = q.Dequeue();
                if (Regex.Match(temp.name, @keyword).Success)
                {
                    result.Add(temp);
             
                }
                for (int i = 0; i < temp.childCount; i++)
                {   
                    q.Enqueue(temp.GetChild(i));
                }
            }
            return result;
        }


        public static Transform get_granfather(Transform son)
        {
            if (son==null)
            {
                Debug.LogError("尝试寻找一个空物体的父亲!");
                return son;
            }
                        
            while (son.transform.parent!=null)
            {
                son = son.transform.parent;
            }
            
            return son;
        }
        
        
        public static Quaternion GetLocalRotation(Transform transform, Vector3 relativeEulerAngles) {
            // 将相对旋转欧拉角转换为局部四元数
            Quaternion localRotation = Quaternion.Euler(relativeEulerAngles);

            // 将局部四元数与物体的世界旋转相乘，得到最终的旋转
            Quaternion finalRotation = transform.rotation * localRotation;

            // 返回最终的旋转
            return finalRotation;
        }
        
    }
    
    
    
}