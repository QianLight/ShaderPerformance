Shader "UI/BlendTexture"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Tags{ "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "../StdLib.hlsl"	 
            #include "../Include/Common.hlsl"
            sampler2D _background;
            sampler2D _xbr;
            sampler2D _mask;
            float4 _backcolor;
            float4 ratiovalue;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;    
                float4 vertex : SV_POSITION;
            };

     
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            float4 frag (v2f i) : Color
            {
                float4 backcolor= tex2D(_background, i.uv);
                float4 xbrcolor = tex2D(_xbr, i.uv*ratiovalue.y+float2(-(1-ratiovalue.x),-(1-ratiovalue.x)));
                float4 maskcolor = tex2D(_mask, i.uv*ratiovalue.y+float2(-(1-ratiovalue.x),-(1-ratiovalue.x)));
                //gamma
                _backcolor=_backcolor*_backcolor;
                backcolor=backcolor*_backcolor*2.2;
                
                float xabs=abs(0.5-i.uv.x);
                float yabs=abs(0.5-i.uv.y);
                float4 col;

                float rcha=abs(xbrcolor.r-_backcolor.r);
                float gcha=abs(xbrcolor.g-_backcolor.g);
                float bcha=abs(xbrcolor.b-_backcolor.b);

                //mask 
                float masknum=step(0.001,maskcolor.r+maskcolor.g+maskcolor.b);

                // ground  0,1
                float xbackc=step(ratiovalue.z,xabs);
                float ybackc=step(ratiovalue.z,yabs);              
                //xbr 
                float rbool=step(rcha ,0.05);
                float gbool=step(gcha ,0.05);
                float bbool=step(bcha ,0.05);
                float xbool=step(1,(1-xbackc)*(1-ybackc));//1=inside
           
                xbool=xbool*step(1,xbool*((1-rbool*gbool*bbool)+masknum));
         
             
                float groundbool=1-xbool;     
                col=backcolor*groundbool+xbrcolor*xbool;          
                return col;
             // return float4(xbool.xxxx);
             //  return xbrcolor;
            }
            ENDHLSL
        }
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			struct Attributes
			{
				float4 vertex : POSITION;
			};
			
			struct Varyings
			{
				float4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}

			ENDCG
		}
    }
}
