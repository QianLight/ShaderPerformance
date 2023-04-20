Shader "Hidden/Preview_Overdraw"
{
	Properties
	{
		[Toggle]_DepthMode("ZWrite", Float) = 1
		_Tex("Texture", 2D) = "black"{}
	}

//		HLSLINCLUDE
//
//			#include "../../StdLib.hlsl"
//
//			struct Attributes
//			{
//				FLOAT4 vertex : POSITION;
//			};
//			
//			struct Varyings
//			{
//				FLOAT4 vertex : SV_POSITION;
//			};
//			Varyings Vert(Attributes v)
//			{
//				Varyings o;
//				FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
//				o.vertex = mul(unity_MatrixVP, WorldPosition);
//				return o;
//			}
//
//			half4 Frag(Varyings i) : SV_Target
//			{
//				return half4(0.1, 0.04, 0.02, 1);
//			}
//
//		ENDHLSL

		SubShader
		{
			//Transparent/Opaque/Background + C + Option/Back/oFf/fronT + Forward/Distortion/Xray/Outline


			
			//Skybox
		Pass
		{
			Name "OverdrawSkybox"
			Tags{"LightMode" = "OverdrawSkybox"}
			
			ZWrite Off
			Cull Off
			Blend One One
			HLSLPROGRAM
			#include "../../Scene/Scene_SkyPCH.hlsl"
			#include "../../Include/Head.hlsl"
			// #include "../../Scene/Scene_Sky.hlsl"

			#pragma vertex vertOverdraw
			#pragma fragment fragskyoverdraw
			v2f vertOverdraw (appdata_t v)
			{
				v2f o;
				// FLOAT3 rotated = RotateAroundYInDegrees(v.vertex.xyz,_Time.y*_RotateSpeed);
				FLOAT4 worldPos = mul(unity_ObjectToWorld, FLOAT4(v.vertex.xyz, 1.0));
				o.vertex = mul(unity_MatrixVP, worldPos);
				o.texcoord = v.vertex.xyz;
				o.depth01 = o.vertex.zw;
				o.WorldPosition = worldPos;
				return o;
			}		
			MRTOutput fragskyoverdraw (v2f i)// : SV_Target
			{
				
				DECLARE_OUTPUT(MRTOutput, mrt);
				mrt.rt0 = half4(0.1, 0.04, 0.02, 0);
				mrt.rt1.xyz = EncodeFloatRGB(i.depth01.x/i.depth01.y);
				SET_BLOOM(mrt, EncodeAlpha(1, _IsRt1zForUIRT));
				return mrt;
			}

			ENDHLSL
		}
		//ForwardBase
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

		
		//Distortion
		Pass
		{
			Name "OverdrawD"
			Tags{"LightMode" = "OverdrawDistortion"}

			Blend One One
			ZWrite off
			Cull Off
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
		//PreZ
		Pass
		{
			Name "OverdrawZ"
			Tags{"LightMode" = "OverdrawPreZ" "RenderType"="Opaque" "PerformanceChecks" = "False"}
//				ColorMask 0
//				ColorMask RGBA 1

			Blend One One
			HLSLPROGRAM

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
			
			ENDHLSL
		}
		//SceneViewPreZ
		Pass
		{
			Name "OverdrawA"
			Tags{"LightMode" = "OverdrawAlways" "RenderType"="Opaque" "PerformanceChecks" = "False"}
			ZWrite On
			Cull Back
			ColorMask 0

			Blend One One
			HLSLPROGRAM

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
			
			ENDHLSL
		}
		//CustomTransparent
		Pass
		{
			Name "OverdrawCT"
			Tags{ "LightMode" = "OverdrawForwardTransparent" "Queue" = "Transparent-10" "RenderType" = "Transparent" }

			Blend One One
			ZWrite Off
			ZTest Equal

			HLSLPROGRAM
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
			
			ENDHLSL
		}
		//Outline
		Pass
		{
			Name "OverdrawO"
			Tags{"LightMode" = "OverdrawOutline"}
			LOD 100

			Blend One One
			HLSLPROGRAM

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
			
			ENDHLSL
		}
		
		//ForwardBase Stencil
		Pass
		{
			Name "OverdrawFS"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			ZWrite[_ZWrite]
			Stencil
			{
				Ref[_Stencil]
				Comp equal
			}
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
		//ForwardBase ZWriteOn Stencil
		Pass
		{
			Name "OverdrawFZWS"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			ZWrite On
			Stencil
            {
                Ref 0
                Comp equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }
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
		
		//ForwardBase Stencil CullOff
		Pass
		{
			Name "OverdrawFSC"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			ZWrite[_ZWrite]
			Cull Off
			Stencil
			{
				Ref[_Stencil]
				Comp equal
			}
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
		
		//ForwardBase Stencil ZWriteOff
		Pass
		{
			Name "OverdrawFSZW"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			ZWrite Off
			Stencil
			{
				Ref[_Stencil]
				Comp equal
			}
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
		
		//Outline Cartoon
		Pass
		{
			Name "OverdrawOC"
			Tags{"LightMode" = "OverdrawOutline"}

			Blend One One
			Cull Front
			Offset 1,1
			Stencil
			{
				Ref[_Stencil]
				Comp equal
			}
			HLSLPROGRAM

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
			
			ENDHLSL
		}
		
	}
}
