Shader "Hidden/ShowMeshInfo/ShowMeshShader"
{
	CGINCLUDE
	#include "UnityCG.cginc"
	
	float _VertexSize;
	float _NormalSize;
	float _TangentSize;
	float _BinormalSize;
		
	float4 _NormalColor;
	float4 _TangentColor;
	float4 _BinormalColor;
	float4 _VertexColor;

	float4x4 _Transform;

	StructuredBuffer<float3> _Points;
	StructuredBuffer<float3> _Normals;
	StructuredBuffer<float4> _Tangents;

	float4 _CustomUVIndexMask0;
	float4 _CustomUVIndexMask1;
	float4 _CustomUVMaskX;
	float4 _CustomUVMaskY;
	float4 _CustomUVMaskZ;

	struct ia_mesh_normal_out
	{
	    float4 vertex : POSITION;
	    float3 normal : NORMAL;
	};

	struct ia_mesh_normal_tangent_out
	{
	    float4 vertex : POSITION;
	    float3 normal : NORMAL;
	    float4 tangent : TANGENT;
	};

	struct ia_mesh_color_out
	{
	    float4 vertex : POSITION;
	    float4 color : COLOR;
	};

	struct ia_uv_out
	{
	    float4 vertex : POSITION;
	    float4 uv  : TEXCOORD0;
	    float4 uv2 : TEXCOORD1;
	    float4 uv3 : TEXCOORD2;
	    float4 uv4 : TEXCOORD3;
	    float4 uv5 : TEXCOORD4;
	    float4 uv6 : TEXCOORD5;
	    float4 uv7 : TEXCOORD6;
	    float4 uv8 : TEXCOORD7;
	};

	struct ia_out
	{
	    float4 vertex : POSITION;
	    float3 normal : NORMAL;
	    float2 uv : TEXCOORD0;
	    float4 color : COLOR;
	    uint instanceID : SV_InstanceID;
	};

	struct vs_out
	{
	    float4 vertex : SV_POSITION;
	    float4 color : TEXCOORD0;
	};

	vs_out vert_normals(ia_out v)
	{
	    float3 pos = (mul(_Transform, float4(_Points[v.instanceID], 1.0))).xyz;
	    float3 dir = normalize((mul(_Transform, float4(_Normals[v.instanceID], 0.0))).xyz);

	    float s = min(abs(mul(UNITY_MATRIX_V, float4(pos, 1.0)).z),1);
	    float4 vertex = v.vertex;
	    vertex.xyz += pos + dir * v.uv.x * _NormalSize * s;
	    vertex = mul(UNITY_MATRIX_VP, vertex);

	    vs_out o;
	    o.vertex = vertex;
	    o.color = _NormalColor;
	    o.color.a = 1.0 - v.uv.x;
	    return o;
	}

	vs_out vert_tangents(ia_out v)
	{
	    float3 pos = (mul(_Transform, float4(_Points[v.instanceID], 1.0))).xyz;
	    float4 tangent = _Tangents[v.instanceID];
	    float3 dir = normalize((mul(_Transform, float4(tangent.xyz * tangent.w, 0.0))).xyz);

	    float s = min(abs(mul(UNITY_MATRIX_V, float4(pos, 1.0)).z),1);
	    float4 vertex = v.vertex;
	    vertex.xyz += pos + dir * v.uv.x * _TangentSize * s;
	    vertex = mul(UNITY_MATRIX_VP, vertex);

	    vs_out o;
	    o.vertex = vertex;
	    o.color = _TangentColor;
	    o.color.a = 1.0 - v.uv.x;
	    return o;
	}

	vs_out vert_binormals(ia_out v)
	{
	    float3 pos = (mul(_Transform, float4(_Points[v.instanceID], 1.0))).xyz;
	    float4 tangent = _Tangents[v.instanceID];
	    float3 binormal = normalize(cross(_Normals[v.instanceID], tangent.xyz * tangent.w));
	    float3 dir = normalize((mul(_Transform, float4(binormal, 0.0))).xyz);

	    float s = min(abs(mul(UNITY_MATRIX_V, float4(pos, 1.0)).z),1);
	    float4 vertex = v.vertex;
	    vertex.xyz += pos + dir * v.uv.x * _BinormalSize * s;
	    vertex = mul(UNITY_MATRIX_VP, vertex);

	    vs_out o;
	    o.vertex = vertex;
	    o.color = _BinormalColor;
	    o.color.a = 1.0 - v.uv.x;
	    return o;
	}

	vs_out vert_vertexcolors(ia_mesh_color_out v)
	{
		vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color = v.color;

	    return o;
	}

	vs_out vert_vertexalphas(ia_mesh_color_out v)
	{
		vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color = half4(v.color.aaa, 1);

	    return o;
	}

	vs_out vert_vertexcolorrgb(ia_mesh_color_out v)
	{
		vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color = half4(v.color.rgb, 1);

	    return o;
	}

	vs_out vert_vertices(ia_out v)
	{
	    float3 pos = (mul(_Transform, float4(_Points[v.instanceID], 1.0))).xyz;
	    float4 vertex = v.vertex;
	    vertex.xyz *= _VertexSize;
	    vertex.xyz *= min(abs(mul(UNITY_MATRIX_V, float4(pos, 1.0)).z),1);
	    vertex.xyz += pos;
	    vertex = mul(UNITY_MATRIX_VP, vertex);

	    vs_out o;
	    o.vertex = vertex;
	    o.color = _VertexColor;

	    return o;
	}

	vs_out vert_local_space_normals(ia_mesh_normal_out v)
	{
	    vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color.rgb = v.normal.xyz * 0.5 + 0.5;
	    o.color.a = 1.0;
	    return o;
	}

	vs_out vert_local_space_tangents(ia_mesh_normal_tangent_out v)
	{
	    vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color.rgb = v.tangent.xyz * 0.5 + 0.5;
	    o.color.a = 1.0;
	    return o;
	}

	vs_out vert_local_space_binormals(ia_mesh_normal_tangent_out v)
	{
	    vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color.rgb = normalize(cross(v.normal, v.tangent.xyz) * v.tangent.w) * 0.5 + 0.5;
	    o.color.a = 1.0;
	    return o;
	}

	vs_out vert_tangent_space_normals(ia_mesh_normal_tangent_out v)
	{
	    vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);

	    float3 base_normal = normalize(v.normal);
	    float4 base_tangent = normalize(v.tangent);
	    float3 base_binormal = normalize(cross(base_normal, base_tangent.xyz) * base_tangent.w * unity_WorldTransformParams.w);
	    float3x3 tbn = float3x3(base_tangent.xyz, base_binormal, base_normal);
	    
	    float3 n = float3(0,0,1);

	    //float3 normal = normalize(mul(n, transpose(tbn)));
	    float3 normal = normalize(mul(tbn,n));
	    o.color.rgb = normal * 0.5 + 0.5;
	    o.color.a = 1.0;
	    return o;
	}

	vs_out vert_tangent_space_tangents(ia_mesh_normal_tangent_out v)
	{
	    vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);

	    float3 base_normal = normalize(v.normal);
	    float4 base_tangent = normalize(v.tangent);
	    float3 base_binormal = normalize(cross(base_normal, base_tangent.xyz) * base_tangent.w * unity_WorldTransformParams.w);
	    float3x3 tbn = float3x3(base_tangent.xyz, base_binormal, base_normal);
	    
	    float3 n = float3(1,0,0);

	    //float3 normal = normalize(mul(n, transpose(tbn)));
	    float3 normal = normalize(mul(tbn,n));

	    o.color.rgb = normal * 0.5 + 0.5;
	    o.color.a = 1.0;
	    return o;
	}

	vs_out vert_tangent_space_binormals(ia_mesh_normal_tangent_out v)
	{
	    vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);

	    float3 base_normal = normalize(v.normal);
	    float4 base_tangent = normalize(v.tangent);
	    float3 base_binormal = normalize(cross(base_normal, base_tangent.xyz) * base_tangent.w * unity_WorldTransformParams.w);
	    float3x3 tbn = float3x3(base_tangent.xyz, base_binormal, base_normal);
	    
	    float3 n = float3(0,1,0);

	    //float3 normal = normalize(mul(n, transpose(tbn)));
	    float3 normal = normalize(mul(tbn,n));

	    o.color.rgb = normal * 0.5 + 0.5;
	    o.color.a = 1.0;
	    return o;
	}

	vs_out vert_uv(ia_uv_out v)
	{
		vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color = v.uv;

	    return o;
	}

	vs_out vert_uv2(ia_uv_out v)
	{
		vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color = v.uv2;

	    return o;
	}

	vs_out vert_uv3(ia_uv_out v)
	{
		vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color = v.uv3;

	    return o;
	}

	vs_out vert_uv4(ia_uv_out v)
	{
		vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);
	    o.color = v.uv4;
	    return o;
	}

	vs_out vert_custom_uv(ia_uv_out v)
	{
		vs_out o;
	    o.vertex = UnityObjectToClipPos(v.vertex);

		float4 uv = v.uv * _CustomUVIndexMask0.x
			+ v.uv2 * _CustomUVIndexMask0.y
			+ v.uv3 * _CustomUVIndexMask0.z
			+ v.uv4 * _CustomUVIndexMask0.w
			+ v.uv5 * _CustomUVIndexMask1.x
			+ v.uv6 * _CustomUVIndexMask1.y
			+ v.uv7 * _CustomUVIndexMask1.z
			+ v.uv8 * _CustomUVIndexMask1.w;

		float x = dot(uv, _CustomUVMaskX);
		float y = dot(uv, _CustomUVMaskY);
		float z = dot(uv, _CustomUVMaskZ);
		
	    o.color = float4(x, y, z, 1);
		
	    return o;
	}
	
	float4 frag(vs_out v) : SV_Target
	{
	    return v.color ;
	}

	float4 fragTransparent(vs_out v) : SV_Target
	{
	    return v.color ;
	}

	ENDCG

	SubShader
    {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+100" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        
        // pass 0: visualize normals
        Pass
        {
        	Name  "NORMAL"
            ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vert_normals
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 1: visualize tangents
        Pass
        {
        	Name  "TANGENT"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_tangents
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 2: visualize binormals
        Pass
        {
        	Name  "BINORMAL"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_binormals
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 3: visualize vertex color
        Pass
        {
        	Name  "VERTEX_COLOR_WITH_ALPHA"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_vertexcolors
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
        // pass 4: visualize vertex shape
        Pass
        {
        	Name  "VERTEX"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_vertices
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 5: visualize local space normal color
        Pass
        {
        	Name  "LOCALSPACENORMAL"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_local_space_normals
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 6: visualize local space tangent color
        Pass
        {
        	Name  "LOCALSPACETANGENT"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_local_space_tangents
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 7: visualize local space binormal color
        Pass
        {
        	Name  "LOCALSPACEBINORMAL"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_local_space_binormals
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 8: visualize tangent space normal color
        Pass
        {
        	Name  "TANGENTSPACENORMAL"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_tangent_space_normals
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 9: visualize tangent space tangent color
        Pass
        {
        	Name  "TANGENTSPACETANGENT"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_tangent_space_tangents
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 10: visualize tangent space binormal color
        Pass
        {
        	Name  "TANGENTSPACEBINORMAL"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_tangent_space_binormals
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 11: visualize uv color
        Pass
        {
        	Name  "UV"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_uv
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 12: visualize uv2 color
        Pass
        {
        	Name  "UV2"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_uv2
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 13: visualize uv3 color
        Pass
        {
        	Name  "UV3"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_uv3
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 14: visualize uv4 color
        Pass
        {
        	Name  "UV4"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_uv4
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
    	
		// pass 15: visualize vertex color
        Pass
        {
        	Name  "VERTEX_ALPHA"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_vertexalphas
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
    	
		// pass 16: visualize vertex color rgb
        Pass
        {
        	Name  "VERTEX_COLOR_RGB"
            ZTest LEqual
        	ZWrite On

            CGPROGRAM
            #pragma vertex vert_vertexcolorrgb
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
    	
    	// pass 17: visualize custom uv value
        Pass
        {
        	Name  "UV4"
            ZTest LEqual
        	ZWrite On

            CGPROGRAM
            #pragma vertex vert_custom_uv
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
    }
}
