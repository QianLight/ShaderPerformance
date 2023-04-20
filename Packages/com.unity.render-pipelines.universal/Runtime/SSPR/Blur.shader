Shader "Unlit/Blur" 
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BlurSize("Blur Size", Float) = 3
    }
    SubShader
    {
        //使用CGINCLUDE可以在多个pass中使用同一个函数，避免重复代码
        CGINCLUDE

        #include "UnityCG.cginc"

        sampler2D _MainTex;
        half4 _MainTex_TexelSize;
        float _BlurSize;

        struct v2f 
        {
            float4 pos : SV_POSITION;
            half2 uv[5]: TEXCOORD0;
        };


        v2f vertBlur(appdata_img v) 
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);

            half2 uv = v.texcoord;

            o.uv[0] = uv;
            o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[3] = uv + float2(0.0,_MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[4] = uv - float2(0.0,_MainTex_TexelSize.y * 1.0) * _BlurSize;
            return o;
        }

        fixed4 fragBlur(v2f i) : SV_Target
        {
            float4 sum = 0;
            float count = 0;
            
            float4 color1 =  tex2D(_MainTex, i.uv[0]);
            count += saturate(ceil(color1.r - 0.0001) + ceil(color1.g - 0.0001) + ceil(color1.b - 0.0001));
            float4 color2 =  tex2D(_MainTex, i.uv[1]);
            count += saturate(ceil(color2.r - 0.0001) + ceil(color2.g - 0.0001) + ceil(color2.b - 0.0001));
            float4 color3 = tex2D(_MainTex, i.uv[2]);
            count += saturate(ceil(color3.r - 0.0001) + ceil(color3.g - 0.0001) + ceil(color3.b - 0.0001));
            float4 color4 = tex2D(_MainTex, i.uv[3]);
            count += saturate(ceil(color4.r - 0.0001) + ceil(color4.g - 0.0001) + ceil(color4.b - 0.0001));
            float4 color5 = tex2D(_MainTex, i.uv[4]);
            count += saturate(ceil(color5.r - 0.0001) + ceil(color5.g - 0.0001) + ceil(color5.b - 0.0001));
            sum = (color1 + color2 + color3 +color4 + color5) / max(count,0.0001);
            return sum;
        }

        ENDCG

        ZTest Always Cull Off ZWrite Off

        Pass 
        {
            CGPROGRAM
    
            #pragma vertex vertBlur  
            #pragma fragment fragBlur

            ENDCG
        }

    }
        FallBack "Diffuse"
}