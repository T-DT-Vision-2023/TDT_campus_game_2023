

Shader "Unlit/chapter-simple" {
	Properties{
			SelectedColor ("Color3",Color) = (1.0,1.0,1.0)
			
			balance ("balance",Float) = 1.0
		
			[HDR] _EmissionColor("Color", Color) = (0,0,0)
		
			
			}
	SubShader{
		
		Pass {
			
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normal)
#pragma exclude_renderers d3d11

			#include "UnityCG.cginc"
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			

			#pragma vertex vert

			#pragma fragment frag

			float4 SelectedColor;

			float balance;

			
			struct v2f
			{
				
				float3 normal : NORMAL;
				float4 vertex : SV_POSITION;
				
			};

			v2f vert(appdata_full v)  {

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				
				return o;		
				
			}

			float3 frag(v2f test) :SV_Target{
				
				float3 worldNormal = normalize(test.normal);
				
				return SelectedColor*balance;

			}

			ENDCG
		}
}
}