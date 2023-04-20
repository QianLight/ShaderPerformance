## Text

### 提要
1. yoga的布局要求，必须先获取到文本内容的 宽，高。
2. 根据文本的宽高，以及 元素本身width/height 等属性，yoga 会给出一个最终的宽高
3. 在JS测传递过来的结果中 <Text>aa{"bb"}cc</Text> 被解析为 <Text><RawText text="aa"/><RawText text="bb"/><RawText text="cc"/></Text>

### 实现
ReactTextShadowNode 负责处理文本的布局， 首先给文本的yogaNode 提供一个TextMeasureFunction 方法， 这个方法负责根据 文本的配置计算文本的宽高。 

ReactTextShadowNode的children属性 将持有多个ReactRawTextShadowNode， 代表 RawText。ReactRawTextShadowNode的text属性 存储自己所表示的文本。 

当初始化/更新 的时候 ReactTextShadowNode 收集出完整的text。提供给TextMeasureFunction，计算属性。 同时触发一个属性更新操作 UpdateViewOperation，负责把这个文本刷给 UI.Text。

#### TextMeasure
提供给yoga 计算内容区域的函数。 当区域需要由内容决定（比如文本）的时候，需要提供此方法。 TextMeasureFunction利用 TextGenerator获取文本宽高。 

```
private YogaSize TextMeasureFunction(YogaNode node, float width, YogaMeasureMode widthMode, float height, YogaMeasureMode heightMode)
{
    ...
    float preferredWidth = commonTextGenerator.GetPreferredWidth(text, settings);
    float preferredHeight = commonTextGenerator.GetPreferredHeight(text, settings);

    return MeasureOutput.Make(preferredWidth, preferredHeight);
}
```

#### 文本更新/初始化
无论是文本的更新还是初始化 都发生在RawText 节点。 比如更新，JS测只会发送 RawText节点的更新属性信息。 然，文本实际的展示是在 Text节点（RawText父节点）。 未来，其他也可能存在这种情形，即某些节点的属性修改，在Unity测会导致 其他节点的更新，故提供了一个 ExtraUpdaterShadowNodeQueue 类，专门处理这种情况。 当ReactSimpleShadowNode 发现自己的更新 需要其他节点更新的时候，可以把这个节点 添加到ExtraUpdaterShadowNodeQueue 队列。 在每次UiManagerModule的batch中，ExtraUpdaterShadowNodeQueue会在布局之前处理 


#### 文本属性
fontSize, fontStyle, textAlign, textAlignVertical, color, lineHeight等文本特有的css属性，会映射到 Unity的 TextGenerationSettings 上，同时这个TextGenerationSettings 也会被用来设置 UI.Text



### 后续可优化点
1. JS测Text组件 简化，测试发现 1000个Text组件的JS耗时是1000个View的 7倍左右 （30ms / 210ms）。 
2. UI.Text 复用 ReactTextShadowNode 的TextGenerationSettings， （1000个文本的计算，耗时在 15ms左右， 复用提升不大）
