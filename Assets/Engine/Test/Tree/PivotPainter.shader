Shader "Unlit/PivotPainter_Old"
{
    Properties
    {
        _PosAndIndexTex ("pos and index", 2D) = "white" {}
        _XExtentTex ("XExtent ", 2D) = "white" {}
        _WindMap ("windMap ", 2D) = "white" {}
        _WindGustWorldScale("WindGustWorldScale",Float)=1
        _WindSpeed("WindSpeed",Float)=1
        
        _RandomRotationInfulence0("RandomRotationInfulence0",Float)=1
        _DampingRadiusMultipiler0("DampingRadiusMultipiler0",Float)=1
        _MaxRotation0("MaxRotation0",Float)=0.1
        _WindRandomRotationChangeRate0("WindRandomRotationChangeRate0",Float)=1
        _WindGustOffset0("WindGustOffset0",Float)=0
        _ParentRotationAngleInfulence0("ParentRotationAngleInfulence0",Float)=0

        _RandomRotationInfulence1("RandomRotationInfulence1",Float)=1
        _DampingRadiusMultipiler1("DampingRadiusMultipiler1",Float)=1
        _MaxRotation1("MaxRotation1",Float)=0.1
        _WindRandomRotationChangeRate1("WindRandomRotationChangeRate1",Float)=1
        _WindGustOffset1("WindGustOffset1",Float)=0
        _ParentRotationAngleInfulence1("ParentRotationAngleInfulence1",Float)=0        


        _RandomRotationInfulence2("RandomRotationInfulence2",Float)=1
        _DampingRadiusMultipiler2("DampingRadiusMultipiler2",Float)=1
        _MaxRotation2("MaxRotation2",Float)=0.1
        _WindRandomRotationChangeRate2("WindRandomRotationChangeRate2",Float)=1
        _WindGustOffset2("WindGustOffset2",Float)=0
        _ParentRotationAngleInfulence2("ParentRotationAngleInfulence2",Float)=0        


        _RandomRotationInfulence3("RandomRotationInfulence3",Float)=1
        _DampingRadiusMultipiler3("DampingRadiusMultipiler3",Float)=1
        _MaxRotation3("MaxRotation3",Float)=0.1
        _WindRandomRotationChangeRate3("WindRandomRotationChangeRate3",Float)=1
        _WindGustOffset3("WindGustOffset3",Float)=0
        _ParentRotationAngleInfulence3("ParentRotationAngleInfulence3",Float)=0                        
        
        _WindHorizontalSpeed("WindHorizontalSpeed",Float)=1
        _WindXAxis("WindXAxis",Vector)=(1,0,0,0)
        _WindYAxis("WindYAxis",Vector)=(0,1,0,0)


        sss("sss",Range(0,2))=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };  

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 localPos: TEXCOORD2;
                float3 dbg: TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            float sss;
            sampler2D _PosAndIndexTex;
            sampler2D _XExtentTex;
            sampler2D _WindMap ;
            float4 _PosAndIndexTex_TexelSize;
        
        
            float _WindGustWorldScale;
            float _WindSpeed;
            
            float _RandomRotationInfulence0;
            float _DampingRadiusMultipiler0;
            float _MaxRotation0;
            float _WindRandomRotationChangeRate0;
            float _WindGustOffset0;
            float _ParentRotationAngleInfulence0;

            float _RandomRotationInfulence1;
            float _DampingRadiusMultipiler1;
            float _MaxRotation1;
            float _WindRandomRotationChangeRate1;
            float _WindGustOffset1;
            float _ParentRotationAngleInfulence1;

            float _RandomRotationInfulence2;
            float _DampingRadiusMultipiler2;
            float _MaxRotation2;
            float _WindRandomRotationChangeRate2;
            float _WindGustOffset2;
            float _ParentRotationAngleInfulence2;

            float _RandomRotationInfulence3;
            float _DampingRadiusMultipiler3;
            float _MaxRotation3;
            float _WindRandomRotationChangeRate3;
            float _WindGustOffset3;
            float _ParentRotationAngleInfulence3;                                    
            
            float _WindHorizontalSpeed;
            float3 _WindXAxis;
            float3 _WindYAxis;
            float2 FixUV(float2 uv)
            {
                return float2(uv.x, 1-uv.y);
            }

           float UVtoIndex(float2 uv, float2 textureSize)
            {
                uv = floor(FixUV(uv) * textureSize);
                return uv.x + uv.y*textureSize.x;
            }
            float2 IndexToUV(float parentIndex, float2 textureSize)
            {
                float w = textureSize.x;
                float h = textureSize.y;                
                float2 uv =  float2(  floor(fmod(parentIndex,w))  , floor(parentIndex / w))+0.5f;
                uv /= textureSize;
                return FixUV(uv);
            }
 
            float4 GetPosAndIndex(float2 uv, sampler2D PosAndIndexMap)
            {               
                float4 pivotAndIndex = tex2Dlod(PosAndIndexMap, float4(uv,0,0)); 
                //pivotAndIndex.xyz = pow(pivotAndIndex.xyz,2.2);
                pivotAndIndex.xyz = (pivotAndIndex.xyz-128.0)*float3(-1,-1,1);
                return pivotAndIndex;
            }

            float4 GetXExtentAndLength(float2 uv, sampler2D XExtentsAndLengthMap)
            {               
                float4 xExtentAndLength = tex2Dlod(XExtentsAndLengthMap, float4(uv,0,0)); 
                //decode xextents:
                xExtentAndLength.xyz = (xExtentAndLength.xyz - 0.5)*2.0f;
                xExtentAndLength.xyz = normalize(xExtentAndLength.xyz*float3(-1,-1,1));         
                //decode length
                xExtentAndLength.w = min(8, xExtentAndLength.w *2048);
                return xExtentAndLength;
            }
            float4 GetXExtent(float2 uv, sampler2D XExtentMap)
            {
                return tex2D(XExtentMap, uv);  
            } 


            void GetPivots(float2 uv, sampler2D PosAndIndexMap, out float4 pivots[4], sampler2D XExtentsAndLengthMap, out float4 xExtents[4], float2 textureSize)
            {

                pivots[0] = GetPosAndIndex(uv, PosAndIndexMap);
                xExtents[0] = GetXExtentAndLength(uv, XExtentsAndLengthMap);
                uv = IndexToUV(pivots[0].a,textureSize);

                pivots[1] = GetPosAndIndex(uv, PosAndIndexMap);
                xExtents[1] = GetXExtentAndLength(uv, XExtentsAndLengthMap);
                uv = IndexToUV(pivots[1].a,textureSize);

                pivots[2] = GetPosAndIndex(uv, PosAndIndexMap);
                xExtents[2] = GetXExtentAndLength(uv, XExtentsAndLengthMap);
                uv = IndexToUV(pivots[2].a,textureSize);

                pivots[3] = GetPosAndIndex(uv, PosAndIndexMap);
                xExtents[3] = GetXExtentAndLength(uv, XExtentsAndLengthMap);

                pivots[0].xyz = mul(unity_ObjectToWorld, float4(pivots[0].xyz,1));
                pivots[1].xyz = mul(unity_ObjectToWorld, float4(pivots[1].xyz,1));
                pivots[2].xyz = mul(unity_ObjectToWorld, float4(pivots[2].xyz,1));
                pivots[3].xyz = mul(unity_ObjectToWorld, float4(pivots[3].xyz,1));
                
                xExtents[0].xyz = mul((float3x3)unity_ObjectToWorld, xExtents[0].xyz);
                xExtents[1].xyz = mul((float3x3)unity_ObjectToWorld, xExtents[1].xyz);
                xExtents[2].xyz = mul((float3x3)unity_ObjectToWorld, xExtents[2].xyz);
                xExtents[3].xyz = mul((float3x3)unity_ObjectToWorld, xExtents[3].xyz);
                                
            }
 
 
            float3 RotateAboutAxis(float4 NormalizedRotationAxisAndAngle, float3 PositionOnAxis, float3 Position)
            {
                // Project Position onto the rotation axis and find the closest point on the axis to Position
                float3 ClosestPointOnAxis = PositionOnAxis + NormalizedRotationAxisAndAngle.xyz * dot(NormalizedRotationAxisAndAngle.xyz, Position - PositionOnAxis);
                // Construct orthogonal axes in the plane of the rotation
                float3 UAxis = Position - ClosestPointOnAxis;
                float3 VAxis = cross(NormalizedRotationAxisAndAngle.xyz, UAxis);
                float CosAngle;
                float SinAngle;
                sincos(NormalizedRotationAxisAndAngle.w, SinAngle, CosAngle);
                // Rotate using the orthogonal axes
                float3 R = UAxis * CosAngle + VAxis * SinAngle;
                // Reconstruct the rotated world space position
                float3 RotatedPosition = ClosestPointOnAxis + R;
                // Convert from position to a position offset
                return RotatedPosition - Position;
            }

            float3 AnimateLevel( float3 worldPosition, float time, float3 windXAxis, float3 windYAxis, float4 pivotWorld, float4 xextentWorld, float windGustWorldScale, 
            float windSpeed, float windHorizontalSpeed, sampler2D windMap, float randomRotationTextureSampleScale, 
            float randomRotationInfulence, float DampingRadiusMultipiler, float MaxRotation, float windGustOffset,
             float parentRotationAngleInfulence, out float3 dbg)
            {
                float3 tip = xextentWorld.xyz * xextentWorld.w + pivotWorld.xyz;
                float2 windUV = frac(float2(dot(tip, windXAxis), dot(tip, windYAxis))/windGustWorldScale + time*float2(windSpeed, windHorizontalSpeed));
                float windGustMag = tex2Dlod(windMap, float4(windUV, 0,0)).a;
                float3 windRotation = randomRotationInfulence * (tex2Dlod(windMap, float4(frac(windUV / randomRotationTextureSampleScale),0,0) ).xyz-0.5);
                
                
                float3 rotateAxis = cross(xextentWorld.xyz, pow(dot(xextentWorld.xyz, windXAxis),5)*float3(0,0,-0.2) + windXAxis);
                rotateAxis = (rotateAxis + windRotation) / length(rotateAxis + windRotation); // why not just normalize?

                float3 objScale = float3(1,1,1);

                float maskMotion = saturate(dot((worldPosition - pivotWorld.xyz)/objScale,normalize(xextentWorld.xyz))/(DampingRadiusMultipiler * xextentWorld.w));
                float angleOffset = windGustOffset + windGustMag;
                float rotateAngle = (MaxRotation * angleOffset + parentRotationAngleInfulence) * maskMotion;

                float3 offset = RotateAboutAxis(float4(rotateAxis,rotateAngle), pivotWorld, worldPosition);
                dbg = pivotWorld;
                return offset;
            }

            
            float3 FoliagePivotPaitner2(float2 uv, float time, float3 worldPosition, sampler2D posAndIndexTex, sampler2D XExtentsAndLengthMap, sampler2D windMap, float2 pivotAndIndexTextureSize )
            {
                float4 pivots[4];
                float4 xextents[4];
                float totalDepth = 0;
                GetPivots(uv, posAndIndexTex, pivots, XExtentsAndLengthMap, xextents,pivotAndIndexTextureSize );
                float currentIndex = UVtoIndex( uv,pivotAndIndexTextureSize );

                if ( floor(pivots[0].a) == currentIndex)
                    totalDepth = 0;
                else if (pivots[1].a == pivots[0].a)
                    totalDepth = 1;
                else if (pivots[2].a == pivots[1].a) 
                    totalDepth = 2;
                else 
                    totalDepth = 3;
                
                float3 dbg;
                float3 totalOffset = 0;
                float3 offsetLevel1 = AnimateLevel(worldPosition, time, _WindXAxis, _WindYAxis, pivots[totalDepth], xextents[totalDepth], _WindGustWorldScale,_WindSpeed, _WindHorizontalSpeed, windMap,
                _WindRandomRotationChangeRate0,_RandomRotationInfulence0,_DampingRadiusMultipiler0,_MaxRotation0,_WindGustOffset0,_ParentRotationAngleInfulence0 ,dbg);
                totalOffset+=offsetLevel1;
                totalDepth --;
                float3 offsetLevel2 = AnimateLevel(worldPosition, time, _WindXAxis, _WindYAxis, pivots[totalDepth], xextents[totalDepth], _WindGustWorldScale,_WindSpeed, _WindHorizontalSpeed, windMap,
                _WindRandomRotationChangeRate1,_RandomRotationInfulence1,_DampingRadiusMultipiler1,_MaxRotation1,_WindGustOffset1,_ParentRotationAngleInfulence1 ,dbg);
                totalOffset+=offsetLevel2*saturate(totalDepth+1);
                totalDepth --;
                float3 offsetLevel3 = AnimateLevel(worldPosition, time, _WindXAxis, _WindYAxis, pivots[totalDepth], xextents[totalDepth], _WindGustWorldScale,_WindSpeed, _WindHorizontalSpeed, windMap,
                _WindRandomRotationChangeRate2,_RandomRotationInfulence2,_DampingRadiusMultipiler2,_MaxRotation2,_WindGustOffset2,_ParentRotationAngleInfulence2 ,dbg);
                totalOffset+=offsetLevel3*saturate(totalDepth+1);
                totalDepth --;
                float3 offsetLevel4 = AnimateLevel(worldPosition, time, _WindXAxis, _WindYAxis, pivots[totalDepth], xextents[totalDepth], _WindGustWorldScale,_WindSpeed, _WindHorizontalSpeed, windMap,
                _WindRandomRotationChangeRate3,_RandomRotationInfulence3,_DampingRadiusMultipiler3,_MaxRotation3,_WindGustOffset3,_ParentRotationAngleInfulence3 ,dbg);
                totalOffset+=offsetLevel4*saturate(totalDepth+1);


                return worldPosition + totalOffset;
                
            }
            v2f vert (appdata v)
            {
                v2f o;
                o.uv2 = v.uv2;
                o.localPos = v.vertex;
                float4 pivots[4];
                float4 xextents[4];
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1));

                GetPivots(v.uv2, _PosAndIndexTex, pivots, _XExtentTex, xextents,_PosAndIndexTex_TexelSize.zw );

                worldPos = (worldPos-pivots[0])*sss+pivots[0];
                float3 dbg = FoliagePivotPaitner2(v.uv2, _Time.y, worldPos, _PosAndIndexTex, _XExtentTex, _WindMap,  _PosAndIndexTex_TexelSize.zw);
                
                o.vertex = mul(UNITY_MATRIX_VP,float4(dbg,1));
                o.uv = v.uv;
                o.dbg = (pivots[0]);
                return o;  
            }
           
            fixed4 frag (v2f i) : SV_Target
            {
                return float4((frac(i.uv2)),0,1) ;
            }
            ENDCG
        }
		UsePass"Hidden/Preview_Overdraw/OverdrawF"
    }
}
