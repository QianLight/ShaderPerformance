#ifndef PBR_Function_01_INCLUDE
#define PBR_Function_01_INCLUDE
         //D项 法线微表面分布函数 
         //为0时1/PI是大面积灰色，为0.092时为最亮的高光点
         float D_Function(float nh,float roughness)
         {
            half lerpSquareRoughness = pow(lerp(0.0001, 1, roughness), 2);
            half D = lerpSquareRoughness / (pow((pow(nh, 2) * (lerpSquareRoughness - 1) + 1), 2) * PI);
            return D;
         }
         //G项
        float G_Function(float nl,float nv,float squareRoughness)
        {
                float kInDirectLight = pow(squareRoughness + 1, 2) / 8;
                //float kInIBL = pow(squareRoughness, 2) / 8;
                float GLeft = nl / lerp(nl, 1, kInDirectLight);
                float GRight = nv / lerp(nv, 1, kInDirectLight);
                //float GLeft =  1/lerp(nl, 1, kInDirectLight);
                //float GRight =  1/lerp(nv, 1, kInDirectLight);
                float G = GLeft * GRight;//越粗糙吸收能量越多，也就越暗
                return G;
        }
        //F项 直接光
        real3 F_Function(float lh,float3 F0)
        {
            float Fre=exp2((-5.55473*lh-6.98316)*lh);//L代替V-优化
            return lerp(Fre,1,F0);
        }

         //F项 间接光
         real3 IndirF_Function(float NdotV,float3 F0,float roughness)
         {
             float Fre=exp2((-5.55473*NdotV-6.98316)*NdotV);
             return F0+Fre*saturate(1-roughness-F0);//1
         }

         //间接光漫反射 球谐函数 光照探针
         real3 SH_IndirectionDiff(float3 normalWS)
         {
             real4 SHCoefficients[7];
             SHCoefficients[0]=unity_SHAr;
             SHCoefficients[1]=unity_SHAg;
             SHCoefficients[2]=unity_SHAb;
             SHCoefficients[3]=unity_SHBr;
             SHCoefficients[4]=unity_SHBg;
             SHCoefficients[5]=unity_SHBb;
             SHCoefficients[6]=unity_SHC;
             float3 Color=SampleSH9(SHCoefficients,normalWS);
             return max(0,Color);
         }

         //间接光高光 反射探针
         real3 IndirSpeCube(float3 normalWS,float3 viewWS,float roughness)
         {
             float3 reflectDirWS=reflect(-viewWS,normalWS);
             roughness=roughness*(1.7-0.7*roughness);//Unity内部不是线性 调整下拟合曲线求近似
             float MidLevel=roughness*6;//把粗糙度remap到0-6 7个阶级 然后进行lod采样
             float4 speColor=SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0,reflectDirWS,MidLevel);//根据不同的等级进行采样
             #if !defined(UNITY_USE_NATIVE_HDR)
             //用DecodeHDREnvironment将颜色从HDR编码下解码。可以看到采样出的rgbm是一个4通道的值，最后一个m存的是一个参数，解码时将前三个通道表示的颜色乘上xM^y，x和y都是由环境贴图定义的系数，存储在unity_SpecCube0_HDR这个结构中。
             return DecodeHDREnvironment(speColor,unity_SpecCube0_HDR);
             #else
             return speColor.xyz;
             #endif
         }

         //间接高光 曲线拟合 放弃LUT采样而使用曲线拟合
         real3 IndirSpeFactor(float roughness,float smoothness,float3 BRDFspe,float3 F0,float NdotV)
         {
             #ifdef UNITY_COLORSPACE_GAMMA
             float SurReduction=1-0.28*roughness,roughness;
             #else
             float SurReduction=1/(roughness*roughness+1);
             #endif

             #if defined(SHADER_API_GLES)//Lighting.hlsl 261行
             float Reflectivity=BRDFspe.x;
             #else
             float Reflectivity=max(max(BRDFspe.x,BRDFspe.y),BRDFspe.z);
             #endif
             half GrazingTSection=saturate(Reflectivity+smoothness);
             float Fre=Pow4(1-NdotV);//lighting.hlsl第501行 
             return lerp(F0,GrazingTSection,Fre)*SurReduction;
         }



            /// lightColor：光源颜色
            /// lightDirectionWS：世界空间下光线方向
            /// lightAttenuation：光照衰减
            /// normalWS：世界空间下法线
            /// viewDirectionWS：世界空间下视角方向
            half3 LightingBased(half3 lightColor, half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half3 viewDirectionWS,half smoothness,half Gloss)
            {
                // 兰伯特漫反射计算
                half NdotL = saturate(dot(normalWS, lightDirectionWS));
                half3 radiance = lightColor * (lightAttenuation * NdotL) ;//* _Diffuse.rgb
                // BlinnPhong高光反射
                half3 halfDir = normalize(lightDirectionWS + viewDirectionWS);
                half3 specular = lightColor * pow(saturate(dot(normalWS, halfDir)), Gloss)  * smoothness;// * _Specular.rgb
                return radiance + specular;
            }

            half3 LightingBased(Light light, half3 normalWS, half3 viewDirectionWS ,half smoothness,half3 Albedo,half Gloss)
            {
                // 注意light.distanceAttenuation * light.shadowAttenuation，这里已经将距离衰减与阴影衰减进行了计算
                return LightingBased(light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS,smoothness,Gloss)*Albedo;
            }


#endif