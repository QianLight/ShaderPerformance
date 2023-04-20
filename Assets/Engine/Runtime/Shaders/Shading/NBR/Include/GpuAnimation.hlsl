#ifndef GPU_ANIMATION_INCLUDE
#define GPU_ANIMATION_INCLUDE

 
#ifdef _GPU_ANIMATION

    #define _AnimationTex _ProcedureTex3	
    FLOAT4 _ProcedureTex3_TexelSize;
    #define _AnimationTexWidthInv _ProcedureTex3_TexelSize.x
    #define _AnimationTexWidth _ProcedureTex3_TexelSize.z
    #define _AnimationTexHightInv _ProcedureTex3_TexelSize.y//

    #define _AnimationTime _CustomTime.x
    #ifdef _INSTANCE
        IBUFFER_START(Param)
            REAL4 param;
        IBUFFER_END(paramArray,64)
    #endif
    inline REAL DecodeRealRG( REAL2 enc )
    {
        REAL2 kDecodeDot = REAL2(1.0, 1/255.0);
        return dot( enc, kDecodeDot );
    }

    REAL4 GPUAnimation( inout FVertexInput Input ,REAL4 localPos INSTANCE_ID)
    {
        REAL4 Pos = localPos;

#ifdef _CUSTOM_GPU_POS
        Pos = CustomGPUPos(Input, Pos INSTANCE_PARAM);
#endif

#ifdef _ROLE_GPU
        #ifdef _INSTANCE
            uint offset = instanceID + _InstanceOffset;
            REAL time = clamp(_AnimationTime - paramArray[offset].param.x,0,0.9999);
        #else
            REAL time = clamp(_AnimationTime,0,0.9999);
        #endif
        time *= _AnimationTexWidth;
        REAL4 pos = FLOAT4(localPos.xyz,1);
        REAL floorTime = floor(time*0.25)*4*_AnimationTexWidthInv;
        REAL x0 = _AnimationTexWidthInv*0.1 + floorTime;
        REAL x1 = _AnimationTexWidthInv*1.1 + floorTime;
        REAL x2 = _AnimationTexWidthInv*2.1 + floorTime;
        REAL x3 = _AnimationTexWidthInv*3.1 + floorTime;

        REAL y = Input.uv2.x*256*_AnimationTexHightInv+0.0001;

        REAL4 anim0 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x0,y),0);
        REAL4 anim1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x1,y),0);
        REAL4 anim2 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x2,y),0);
        REAL4 anim3 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x3,y),0);

        REAL posX = DecodeRealRG(REAL2(anim0.a,anim3.r));
        REAL posY = DecodeRealRG(REAL2(anim1.a,anim3.g));
        REAL posZ = DecodeRealRG(REAL2(anim2.a,anim3.b));
        
        REAL4x4 boneMatrix = REAL4x4(
            anim0.xyz*2-1 , (posX*2-1)*10,
            anim1.xyz*2-1 , (posY*2-1)*10,
            anim2.xyz*2-1 , (posZ*2-1)*10,
            0,0,0,1
            );

        REAL4 pos0 = mul(boneMatrix,pos);
        REAL3 normal0 = normalize (mul((REAL3x3)boneMatrix,Input.TangentX));
        REAL4 tangent0 = REAL4(normalize (mul((REAL3x3)boneMatrix,Input.TangentZ.xyz)),Input.TangentZ.w);
        REAL3 Normal = normal0;
        REAL4 Tangent = tangent0;
        Pos = pos0;
        
        #ifdef _BONE_2
            REAL y1 = Input.uv2.y*256*_AnimationTexHightInv+0.0001;
            REAL4 anim0_1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x0,y1),0);
            REAL4 anim1_1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x1,y1),0);
            REAL4 anim2_1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x2,y1),0);
            REAL4 anim3_1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x3,y1),0);
            REAL posX_1 = DecodeRealRG(REAL2(anim0_1.a,anim3_1.r));
            REAL posY_1 = DecodeRealRG(REAL2(anim1_1.a,anim3_1.g));
            REAL posZ_1 = DecodeRealRG(REAL2(anim2_1.a,anim3_1.b));

            REAL4x4 boneMatrix1 = REAL4x4(
                anim0_1.xyz*2-1 , (posX_1*2-1)*10,
                anim1_1.xyz*2-1 , (posY_1*2-1)*10,
                anim2_1.xyz*2-1 , (posZ_1*2-1)*10,
                0,0,0,1
                );
            REAL4 pos1 = mul(boneMatrix1,pos);
            REAL3 normal1 = normalize(mul((REAL3x3)boneMatrix1,Input.TangentX));
            REAL4 tangent1 = REAL4(normalize (mul((REAL3x3)boneMatrix1,Input.TangentZ.xyz)),Input.TangentZ.w);
            Normal = lerp(normal1,normal0,Input.uv2.z); 
            Tangent = lerp(tangent1,tangent0,Input.uv2.z);
            Pos = lerp(pos1,pos0,Input.uv2.z);  
        #endif//_BONE_2

        Input.TangentX = Normal;
        Input.TangentZ = Tangent;
#endif// _ROLE_GPU

        return Pos;
    }

#endif//_GPU_ANIMATION





#endif //GPU_ANIMATION_INCLUDE