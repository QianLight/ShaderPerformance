Shader "UI/SquareMask2"
{
  Properties
  {
    _MainTex("Texture", 2D) = "white" {}
    _Param0("x:Softness y:Clip z:AlphaScale", Vector) = (-100,0.25,0.8,0)
    _Color ("Tint", Color) = (1,1,1,1)
    _Param1("xy:min zw:max", Vector) = (0,0,0,0)
    _Sm("sm",Range(0.001,0.1))=0.012
  }
  SubShader
  {
    Tags { "Queue" = "Transparent" }
    LOD 100

    Pass
    {
      Tags{ "LightMode" = "UniversalForward" }
      Blend SrcAlpha OneMinusSrcAlpha
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      //#include "UnityUI.cginc"

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

      sampler2D _MainTex;
      float4 _Param0;
      float4 _Param1;
      float4 _Color;
      #define _Softness _Param0.x
      #define _Clip _Param0.y
      #define _AlphaScale _Param0.z
      #define _Rect _Param1
      half _Sm;
      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }
    float UnityGet2DClipping (float2 position,float4 clipRect)
    {
        //float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
        //return inside.x * inside.y;

        float min01 = _Sm+clipRect.z;
        float min02 = (_Sm+clipRect.w);
        float min03 = (_Sm*-1+clipRect.x);
        float min04 = (_Sm*-1+clipRect.y);
        float insideXrame = smoothstep(min01,clipRect.z-_Sm,position.x) * smoothstep(min03,clipRect.x+_Sm,position.x);
        float insideYrame = smoothstep(min02,clipRect.w-_Sm,position.y) * smoothstep(min04,clipRect.y+_Sm,position.y);
        return insideXrame*insideYrame;
    }
      half4 frag(v2f i) : SV_Target
      {
        half4 col = half4(0,0,0,1);
        half2 offset = half2(i.uv.x - 0.5, i.uv.y - 0.5);
        half a = UnityGet2DClipping(offset,_Rect);
        //return a;
        col.a *= (saturate(_Clip - a) * _Softness) * _AlphaScale;
        col.rgb = lerp(col.rgb,_Color.rgb,_Color.a);
        return col;
      }
      ENDCG
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
