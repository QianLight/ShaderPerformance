Shader "Urp/InteriorMapping"
{
	Properties
	{
		_RoomTint("RoomTint",Color) = (1,1,1,1)
		_RoomTex("RoomTex", 2D) = "white" {}
		_Depth("Depth",float) = 1
		[HideInInspector]_RoomDepth("RoomDepth", range(0.001,0.999)) = 0.5
		_Rooms("Room Atlas Rows&Cols (XY)", Vector) = (1,1,0,0)
		_RefelectionColor("ReflectionColor", Color) = (0.5,0.5,0.5,0)
		[HDR]_EmissionColor("EmissionColor",Color) = (0,0,0,1)
        [NoScaleOffset]_EmitTex("EmitTex", 2D) = "black" {}
		_Roughness("Roughness",range(0,1)) = 1
        [NoScaleOffset]_DirtTex("RoughnessTex", 2D) = "white" {}
        [NoScaleOffset]_FrameTex("FrameTex", 2D) = "black" {}
        _FrameCol("FrameCol",Color) = (0,0,0,0)

	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		
		Cull Back
		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 tangentViewDir : TEXCOORD1;
				float3 normal : TEXCOORD2;
				float3 positionWs : TEXCOORD3;
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _RoomTex_ST;
			float4 _RefelectionColor;
			float4 _RoomTint;
			float4 _EmitTex_ST;
			float4 _EmissionColor;
            float4 _DirtTex_ST;	
			float4 _FrameTex_ST;
            float4 _FrameCol;
			float2 _Rooms;
			float _Depth;
			float _RoomDepth;
			float _Roughness;		
			CBUFFER_END
			sampler2D _RoomTex;
			
			sampler2D _EmitTex;
			sampler2D _DirtTex;
			sampler2D _FrameTex;						
			
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				o.uv = v.uv.xy * _RoomTex_ST.xy + _RoomTex_ST.zw;
				float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));

				v.tangent.xy *=_Depth;
				float3 viewDir =  v.vertex.xyz - objCam.xyz;

				float tangentSign = v.tangent.w * unity_WorldTransformParams.w;

				float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;
                    o.tangentViewDir = float3(
                        dot(viewDir, v.tangent.xyz),
                        dot(viewDir, bitangent),
                        dot(viewDir, v.normal)
                        );


				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );
				o.normal = TransformObjectToWorldNormal(v.normal);
				o.positionWs = positionWS;
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.pos = positionCS;
				return o;
			}
			
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half4 frag ( VertexOutput i  ) : SV_Target
			{

				float2 roomUV = frac(i.uv);
                float2 roomIndexUV = floor(i.uv);
 				float farFrac = _RoomDepth;//tex2D(_RoomTex, (roomIndexUV + 0.5) / _Rooms).a;
                float depthScale = 1.0 / (1.0 - farFrac) - 1.0;
				float3 pos = float3(roomUV * 2 - 1, -1);
				i.tangentViewDir.z *= -depthScale;


                float3 id = 1.0 / i.tangentViewDir;
                float3 k = abs(id) - pos * id;
                float kMin = min(min(k.x, k.y), k.z);
                pos += kMin * i.tangentViewDir;
                float interp = pos.z * 0.5 + 0.5;

                float realZ = interp / depthScale + 1;
                interp = 1.0 - (1.0 / realZ);
                interp *= depthScale + 1.0;

                float2 interiorUV = pos.xy * lerp(1.0, farFrac, interp);
                interiorUV = interiorUV * 0.5 + 0.5;

                float4 room = tex2D(_RoomTex, (roomIndexUV + interiorUV.xy) / _Rooms);
				
				float4 emit = tex2D(_EmitTex, (roomIndexUV + interiorUV.xy) / _Rooms);
				float dirt = tex2D(_DirtTex,i.uv).r;
				float frame = tex2D(_FrameTex,i.uv).r;
                
				float3 worldViewDir = normalize(_WorldSpaceCameraPos.xyz - i.positionWs.xyz);
                float ndv = 1- dot(worldViewDir, i.normal);
                float fresnel = pow(ndv, 10);

                float3 reflectionDir = reflect(-worldViewDir, i.normal);
				float roughness = dirt * _Roughness;
				float3 specular = GlossyEnvironmentReflection(reflectionDir,roughness,1);
 				specular = specular* saturate(_RefelectionColor + fresnel);
				
				float3 emitCol =  emit.xyz * emit.w  * _EmissionColor;
				
				float4 col = lerp(room * _RoomTint + float4(specular,1),frame * _FrameCol,frame) + float4(emitCol,1);
				APPLY_FOG(col, i.positionWs.xyz);
				return col;
				//return  room + float4(specular,0);
			}
			ENDHLSL
		}

	}
}