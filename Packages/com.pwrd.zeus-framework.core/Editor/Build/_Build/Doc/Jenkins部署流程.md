# Jenkins部署流程
### 一.插件安装
使用第三方插件简化Jenkins部署流程
#### 1.通用插件
- Build Name and Description Setter 设置构建任务名称和描述，描述若需要设置成图片格式要在Manage Jenkins->Configure Global Security->标记格式器设置为Safe HTML
- Environment Injector Plugin 可以通过.properties文件设置环境变量，可以用来以环境变量的形式从脚本中传输设置Build Name和Description需要的参数
- File Operations Plugin 支持构建中对文件进行操作
- Git Plugin, Git Paramter Plugin, Gitlab Plugin 通过Git拉取代码
- Locale Plugin, Localization：Chinese(simplified) 本地化
- Multiple SCMS 支持从多个仓库拉取代码
- Readonly Parameter plugin 设置只读变量
- user build vars plugin 获得当前操作的用户信息，包括ID，Name等

#### 2.IOS插件
- Keychains and Provisionging Profiles Management 管理Mac Slave Node的钥匙串

### 二.Jenkins设置
- 设置URL
- 全局属性->Environment variables设置Sdk, Ndk路径
- Locale->Default Language设置zh_CN
- GitLab 设置Gitlab参数
- 添加登录Git等需要的凭据

### 三.Job设置

#### 1.参数获取
##### a.Jenkins中获取参数
- 设置Unity路径，项目路径，和需要重载的参数，windows平台下的Unity路径需要加双引号
- 由于batch command支持中文环境变量，所以可以在Windows Node负责的Job中设置变量Name为中文，这样的好处是Jenkins中参数清晰明了，但是需要通过set关键字重新设置英文环境变量以供其它不支持中文变量的脚本获取，所以不建议将Name设置为中文

##### b.通过.properties文件读取获取参数
properties文件路径为$PROJECTPATH/_Build/Config
- 在Jenkins中设置固定的.properties文件名，通过选择文件名选择要读取的文件
- properties文件中设置所有打包用到的参数包括自定义参数

#### 2.源码管理
##### Git
需要设置URL和凭据，可以选择分支，也可以选择检出的位置，建议检出到workspace

#### 3.构建触发器
可设置根据时间周期自动构建

#### 4.构建
- (可选)创建env.properties文件
- (可选)将打包用到的.properties文件注入环境变量，可在python脚本中直接获取其中的参数
- 通过bat或shell执行python脚本，可以在执行前重新设置一下环境变量，脚本路径为MainProject/_Build/Jenkins/Build.py
- (可选)通过env.properties注入环境变量，环境变量可用于Jenkins中设置Build Name, Build Description等
- (可选)删除env.properties文件

### 四.在Mac上创建Slave Node
- 设置标签，可以在Job中根据标签限制项目的运行节点
- 设置ip地址，凭据，用法，工作目录等参数，可以通过Jenkins的URL去访问Node工作目录下的文件，工作目录需要开启共享权限
- Slave Node需要装JDK，无需安装Jenkins。Mac需要开启远程登陆、访问权限
- 安装python3, python脚本中import时可以通过sys.path.append添加Package路径

### 五.Xcode打包
- 可自动管理签名，需要在properties文件里提供Team ID，例："DEVELOPMENT_TEAM=##########"，Team ID来自于钥匙串访问中的证书的组织单位

### 六.ipa文件的扫码下载
- ipa文件的下载链接应指向.plist文件，链接格式为"itms-services://?action=download-manifest&url=${plistUrl}
- plist文件内容参考build.py
- 链接应为https协议，可以在mac上搭建apache服务器，提供链接访问证书给需要访问的设备

