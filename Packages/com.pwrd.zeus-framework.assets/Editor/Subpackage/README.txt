一，	记录资源工具的使用
1.	在EditorMenu中选择Zeus->Setting->Asset->分包->分包工具
2.	点击“开始记录资源”，当按钮文字显示“停止记录资源”则表示记录资源功能已开启，启动游戏就记录资源的使用
3.	AssetListTag中设置标签，标签用于标记记录顺序
4.	Asset Output Path栏设置记录保存路径，路径使用相对于工程目录的相对路径名。
5.	点击“保存Asset Log”会将记录保存到上一项设置的路径中，文件以“Tag_精确到毫秒的时间.json”命名。退出游戏时，如果没有手动保存Asset Log则会自动保存一次
二，	BundleSequence的生成
1.	测试人员将所有的AssetLog上传
2.	在EditorMenu中选择Zeus->Setting->Asset->分包->分包工具
3.	由一人点击“生成BundleSequence”就会在”ZeusSetting”目录中生成BundleSequence文件，其中记录的是Bundle的使用顺序
三，	上传工具的使用
由于要将子包资源上传到Cdn，所以需要在EditorMenu中选择Zeus->Setting->Asset->分包->upload bundle中进行配置，目前只支持阿里的OSS，其中”SourceBundleFolder”用于指定手动上传的目录路径，点击菜单中的Upload即可手动上传（打包时也会自动上传子包资源）
四，	包体合并工具的使用
包体合并工具用于将子包资源添加到安装包中，即可提供整包共热更分析使用
1.	在EditorMenu中选择Zeus->Setting->Asset->分包->包体合并工具
2.	SourceFolder选择子包资源目录路径，Zipfile选择安装包”.apk .obb .ipa等”，点击Merge就会在安装包的路径生成以合并后的整包，以“(Merge)”+安装包名命名。
五，	打包选项
1.	BuildWindow中如果勾选“UseBundleLoader”则可以通过“UseSubpackage”，“UploadBundle”和 “GenerateWholePackage”来分别控制是否要使用分包打包，打包后是否上传子包资源和是否生成合并后的整包。
2.	Jenkins中也可以通过“UseSubpackage”, “UploadBundle”,  “GenerateWholePackage”来传入命令行参数控制分包打包流程。

