Shader "Hidden/Custom/Editor/Terrain_Bake"
{
    Properties
    {
		_BlendTex("Blend Tex ", 2D) = "white" {}
		_MainTex0("Main Tex 0", 2D) = "black" {}
		_MainTex1("Main Tex 0", 2D) = "black" {}
		_MainTex2("Main Tex 0", 2D) = "black" {}
		_MainTex3("Main Tex 0", 2D) = "black" {}
		_Scale("Scale0",Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _BlendTex;
		sampler2D _MainTex0;
		sampler2D _MainTex1;
		sampler2D _MainTex2;
		sampler2D _MainTex3;
		float4 _Scale;

        struct Input
        {
            float2 uv_MainTex;
        };

        // half _Glossiness;
        // half _Metallic;
        // fixed4 _Color;


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 b = tex2D(_BlendTex, IN.uv_MainTex);
            float4 c0 = tex2D (_MainTex0, IN.uv_MainTex*_Scale.x);
			float4 c1 = tex2D (_MainTex1, IN.uv_MainTex*_Scale.y);
			float4 c2 = tex2D (_MainTex2, IN.uv_MainTex*_Scale.z);
			float4 c3 = tex2D (_MainTex3, IN.uv_MainTex*_Scale.w);
            o.Albedo =  c0*b.r + c1*b.g + c2*b.b + c3*b.a;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
