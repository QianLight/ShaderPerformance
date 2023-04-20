Shader "Hidden/Custom/Editor/Area_Mask"
{
	Properties
	{
		_Color("Color",Color) = (1,0,0,0.5)
		_MaskColor("Mask Color",Color) = (0,1,0,0.5)
		_PickRange("_PickRange",float)=1

	}

	SubShader
	{
		Cull Back ZWrite Off
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 100
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM


			#pragma vertex Vert
			#pragma fragment Frag
			#pragma target 4.5

			#include "UnityCG.cginc"

			struct Attributes
			{
				float4 vertex : POSITION;
			};
			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float3 wpos : TEXCOORD0;
			};
			struct LightProbeAreaData
			{
				float flag;
				float4 height;
			};
			StructuredBuffer<LightProbeAreaData> _LightProbeAreaData;
			StructuredBuffer<float> _AreaDataMousePos;

			float4 _AreaRange;
			float _AreaSize;
			float3 _MousePos;
			float  _PickRange;
			Varyings Vert(Attributes v)
			{
				Varyings o = (Varyings)0;
				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				o.wpos = WorldPosition.xyz;
				return o;
			}

			float4 _Color;
			float4 _MaskColor;
			half4 Frag(Varyings i) : SV_Target
			{
				half4 c = _Color;
				float2 offset = i.wpos.xz - _AreaRange.xy;
				float2 roundPos = (frac(offset/_AreaSize)-0.5)*2;
				if(abs(roundPos.x)>0.99||abs(roundPos.y)>0.99)
				{

				}
				else
				{
					c = 0;
				}
				
				int x = (int)(offset.x / _AreaSize);
				int z = (int)(offset.y / _AreaSize);
				int index = (int)(x + z * _AreaRange.z);
				LightProbeAreaData lpad = _LightProbeAreaData[index];
		    	float singledatamp=_AreaDataMousePos[index];
				float mask0 = lpad.height.x>-0.5&&abs(lpad.height.x - i.wpos.y)<(0.5);
				float mask1 = lpad.height.y>-0.5&&abs(lpad.height.y - i.wpos.y)<(0.5);
				float mask2 = lpad.height.z>-0.5&&abs(lpad.height.z - i.wpos.y)<(0.5);
				float mask3 = lpad.height.w>-0.5&&abs(lpad.height.w - i.wpos.y)<(0.5);
				float mask = saturate(mask0+mask1+mask2+mask3);
				c += lerp(0,_MaskColor,mask);

				//float2 editOffset = _MousePos.xz - _AreaRange.xy;
				//int xx = (int)(editOffset.x / _AreaSize);
				//int zz = (int)(editOffset.y / _AreaSize);
	 
				//int deltaX = abs(x-xx);
				//int deltaZ = abs(z-zz);
				//if(deltaX<1&&deltaZ<1&&_PickRange==1)
				//{
				//	c = half4(1,1,0,0.5f);
				//}
				if(singledatamp==1&&_PickRange==1)
				{
					c = half4(1,1,0,0.5f);
				}

				return c;

			}
			ENDCG
		}
	}
}
