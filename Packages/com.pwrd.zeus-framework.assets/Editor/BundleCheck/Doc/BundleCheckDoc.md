## 界面位置
Editor下通过Zeus -> Asset -> Bundle Dependencies Check 打开界面。
## 功能介绍
> 使用之前，务必更新所有资源bundle
## 检查全部Bundle
点击 检查全部Bundle Tab，会自动检查并输出所有Bundle的最大依赖深度和所依赖的Bundle（包括间接依赖）以及循环依赖情况，循环依赖环以列表形式输出（表每个节点依赖表中的下一个节点，表尾依赖表首）；

点击 输出Json 按钮，将检查结果输出到工程目录下，具体位置详见Console输出。
## 筛选Bundle
点击 筛选Bundle Tab，进入Bundle筛选界面。该界面可以筛选出依赖深度>=指定数值的Bundle，勾选 只包含含有循环依赖的Bundle Toggle，进一步过滤掉不包含循环依赖的Bundle。

点击 筛选出目标Bundle，并展示依赖信息按钮，工具会根据筛选条件筛选出符合条件的Bundle，并展示Bundle的最大依赖深度和所依赖的Bundle（包括间接依赖）以及循环依赖情况，循环依赖环以列表形式输出（表每个节点依赖表中的下一个节点，表尾依赖表首）；

点击 输出Json 按钮，将检查结果输出到工程目录下，具体位置详见Console输出。

## 查看具体Bundle
点击 查看具体Bundle Tab，进入查看具体Bundle依赖信息界面。

在BundleName文本框输入Bundle 名称，名称格式和之前两个功能展示的BundleName相同；

点击 展示指定Bundle的依赖信息 按钮，工具会获取该Bundle的最大依赖深度和所依赖的Bundle（包括间接依赖）以及循环依赖情况，并展示。如果Bundle不存在会有提示。

点击 输出Json 按钮，将检查结果输出到工程目录下，具体位置详见Console输出。
