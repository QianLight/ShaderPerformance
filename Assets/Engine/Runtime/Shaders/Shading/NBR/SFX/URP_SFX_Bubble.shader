Shader "URP/SFX/Bubble"
{
    Properties
    {
	    _MainColor("MainColor", color) = (1,1,1,1)
		_ShadowColor("ShadowColor", color) = (1,1,1,1)
        [NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
        [NoScaleOffset]_MainTexNoise("Noise", 2D) = "black" {}
        [NoScaleOffset]_Noise("Noise", 2D) = "black" {}
		[NoScaleOffset]_NormalTex ("Normal", 2D) = "bump" {}

        _MainUVTiling("UVTiling(x:MainT y:MainT z:NoiseT w:NoiseT)",vector) = (1,0,1,1)             
        _MainUVSpeed("MainUVSpeed(x:MainS y:MainS z:NoiseS w:NoiseS)",vector) = (1,0,1,1)
        _MainControl("Mainadjust(x:Distort y:Bright z:Contrast w:Alpha)",vector) = (1,1,1,1) 

        _UVControl("UVControl(xy:MainUV zw:NoiseUV)",vector) = (1,1,1,1)
        _WaveControl("WaveControl(x:XSpeed y:YSpeed z:ZSpeed w:worldSize)",vector) = (1,0,1,1)
		_ShadowControl("ShadowControl(x:ShadowRange y:ShadowSmooth z:speed w:normalint)" , vector ) = (1,1,1,1)
     
       
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
          	Name"FORWARD"
		    Tags 
	        { 
				"LightMode"="UniversalForward"
	        }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
				float4 color : TEXCOORD1;
				float3 normal : TEXCOORD5;
				float3 WorldNormal : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
				float3 bitangentDir : TEXCOORD4;
				
                
            };

            sampler2D _NormalTex,_MainTex,_Noise,_MainTexNoise;
            CBUFFER_START(UnityPerMaterial)
            half4 _WindControl,_WaveControl,_ShadowControl,_ShadowColor,_MainColor,_UVControl,_MainUVSpeed,_MainControl,_MainUVTiling;
			CBUFFER_END
           		    
            v2f vert (appdata v)
            {
                v2f o;
			//	VertexOutput VOUT;
                // UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);

                o.WorldNormal= normalize(TransformObjectToWorldNormal(v.normal));
				o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.bitangentDir = normalize(cross(o.WorldNormal, o.tangentDir) * v.tangent.w);

				//v.vertex.xy += v.normal.xy*0.004;

			    float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float2 samplePos = worldPos.xz / _WaveControl.w;
                samplePos += _Time.x * -_WaveControl.xz;
                half waveSample = tex2Dlod(_Noise, float4(samplePos, 0, 0)).r;
               // worldPos.x += sin(waveSample * _WindControl.x) * _WaveControl.x * _WindControl.w * v.uv.y;
                worldPos.z += sin(waveSample) * _WaveControl.z  *v.color.r*_WaveControl.y;

				//o.pos = UnityObjectToClipPos(worldPos);
                o.pos = mul(UNITY_MATRIX_VP, worldPos);
                o.uv = v.uv;
				

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                //diffuse
                float3 maincolorUV = float3(_MainUVSpeed.x*_Time.y,_MainUVSpeed.y*_Time.y,_MainControl.x);
                float  noisedistort =(tex2D(_MainTexNoise,float2(i.uv.x*_MainUVTiling.z,i.uv.y*_MainUVTiling.w)+ float2(_MainUVSpeed.z,_MainUVSpeed.w)*_Time.y).x-0.5)*maincolorUV.z;
                float4 maincolor = tex2D(_MainTex ,float2(float2(i.uv.x*_MainUVTiling.x,i.uv.y*_MainUVTiling.y)+maincolorUV.xy)+noisedistort);
                float4 Coloradjust = pow(maincolor , _MainControl.z)*_MainControl.y;

                //noise
                half2 noise = float2(tex2D(_Noise,float2(i.uv.x*_UVControl.z,i.uv.y*_UVControl.w)+float2(_Time.y * _ShadowControl.z,0)).r,0);
                float2 NoiseUV = float2(_Time.y * _ShadowControl.z, 0);
                //normal
                half3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.WorldNormal);
                half3 norm = tex2D(_NormalTex,float2(i.uv.x*_UVControl.x,i.uv.y*_UVControl.y)+NoiseUV+noise);

                float2 normOff = norm.xy * 2.0f - float2(1.0f,1.0f);
                float3 _Normalmap_var = float3(normOff.rg, 1.0f);
                float3 normalLocal = _Normalmap_var.rgb;
                half3 normalWorld = normalize(mul(normalLocal, tangentTransform)); 
                      normalWorld = lerp(i.WorldNormal,normalWorld,_ShadowControl.w);
                
				half3  V = normalize(_WorldSpaceCameraPos.xyz - i.pos.xyz);
				float3 N = normalWorld;
                float3 L = normalize( _MainLightColor.xyz);
				float3 H = normalize(L+V);
                float NdL = saturate(dot(N,L));
				float NdH = saturate(dot(N,H));
				//float Spec = pow(NdH,_ShadowControl.z)*_ShadowControl.w;
				
                 half halfLambert = NdL * 0.5 + 0.5;
                 half ramp = smoothstep(0, _ShadowControl.y, halfLambert - _ShadowControl.x);
                 half3 diffuse = lerp(_ShadowColor.xyz*Coloradjust.xyz, _MainColor.xyz*Coloradjust.xyz, ramp);


                half3 col =diffuse;
				half4 color =float4(col,_MainControl.w*Coloradjust.w);
                
                return color;
            }
            ENDHLSL
        }
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBaseT"}

			Blend One One
			HLSLPROGRAM

			#pragma only_renderers d3d11
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

			ENDHLSL
		}

    }
}