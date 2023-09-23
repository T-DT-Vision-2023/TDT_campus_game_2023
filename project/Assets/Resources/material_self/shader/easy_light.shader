Shader "Unlit/buff_easy_light_blue"
{
    Properties
    {
        _color ("Color", Color) = (1.0,1.0,1.0,1.0)
        _balance ("balace", float) = 1.0
    }
    SubShader
    {
        
       

        Pass
        {
            
            CGPROGRAM

            #include "UnityCG.cginc"
	
            float4 _color;
            float _balance;

            #pragma vertex vert
            #pragma fragment frag

            struct v2f
            {
                fixed4 vertex : POSITION;
            };


            v2f  vert(appdata_full v)
            {
                v2f result;
                result.vertex = UnityObjectToClipPos(v.vertex);
                return result;
            }

            fixed4 frag(v2f f) : SV_Target{

                return  _color*_balance;
            } 
            
            
            
          
            ENDCG
        }
    }
}
