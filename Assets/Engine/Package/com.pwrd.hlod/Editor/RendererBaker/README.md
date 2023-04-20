# Renderer Baker

Renderer Baker是一个可以将渲染结果烘焙到贴图的工具

因为烘焙时观察角度与实时渲染时的观察角度不同,所以无法对基于观察视角的高光等烘焙到贴图上.

烘焙时,需要关闭背面剔除以保证烘焙结果.

## 原理

参与烘焙的Shader要支持_RENDERER_BAKE_关键字

烘焙工具的实现与lightMap烘焙类似,都需要用到lightmapUV(uv0或者uv2),且要求uv没有重叠部分.

烘焙时,在Vertex阶段,将PositionCS的坐标设置为lightMapUV坐标.在Fragment阶段,除了不使用PositionCS,不计算与观察位置相关的变量(如:高光,雾效)之外,所有计算没有其它变化.

在渲染结束后,截取渲染结果做为烘焙结果.


### 关键字_RENDERER_BAKE_
为了支持烘焙工具,需要修改被渲染物体所用的Shader,通过增加关键字_RENDER_BAKE_来保证原Shader的正常使用.

### 示例

```c++
V2F Vertex(Attributes input)
{
    V2F output = (V2F)0;
    
    /*
    *代码块
    */

#if _RENDERER_BAKE_
    output.positionCS = float4((input.lightmapUV* float2(1, -1) + float2(0, 1) - 0.5) * 2, 0.1, 1);
#endif

    return output;
}

float4 Fragment(V2F input)
{
    float4 color;

#if _RENDERER_BAKE_
    /*
    *代码块  没有雾效,高光等基于观察视角的计算
    */
#else
   /*
    *代码块  正常计算
    */
#endif

    return color;
}
```

