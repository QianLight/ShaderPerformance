# 背景
本文档为了更好地规范，以及指导新成员的开发。内容包含了以下几个方面：
* 目录结构说明，以便于熟悉代码结构
* 规范声明：包括开发规范和代码规范，二者是我们需要遵循的共同的开发语言，以便于为用户提供简洁而优雅的输出。
* 开发流程：指导新成员进行新功能的增删改。

## 定义
* 用户：GSDK为游戏CP提供游戏研发过程中的周边系统，我们是SDK服务的提供方，那么我们的直接用户就是游戏CP

# 目录结构
```
----------------------- Public 接口 -----------------------
IMService
    |- IIMService.cs : 对外暴露的接口，此文件不能包含任何具体实现
    |- IMPublicDefine.cs : 对外暴露的数据结构定义，此文件不任何包含任何内部临时数据结构。


-----------------------   内部实现   -----------------------
IMService
    |- IMService.cs : 对IIMService.cs的具体实现，对于用户来说，不可见。不能对用户暴露任何实现细节。
    |- IMInnerTools.cs : 内部工具类，主要用于与旧接口的转换，不可对外暴露。
                        
```


# 规范
## 开发原则
* 我们**禁止**对外暴露具体实现类（关键字为：class），只能提供接口类（关键字为interface），以使得我们对外提供的交付物更具扩展性。
* **逻辑和数据分离**：我们不提倡将错误码（逻辑）和业务数据放在一个数据结构里，以便用户能更直观地了解该如何使用我们接口所返回的数据。以减少对文档以及各种注释的依赖。这是因为：
1. 现在让CP通过多个错误码进行逻辑判断，这本质上是把内部的实现暴露给游戏CP，这严重违反了“面向抽象编程，而不是面向实现编程”原则。因此必须在GSDK内部将各种系统（第三方SDK、后台）的错误收敛（映射）为统一的字段表示。
2. 错误码本质上是一种“逻辑”，因为cp在写代码时候，要对错误码进行if...else...，这本质上是一种逻辑判断。而数据应该独立出来，数据不应该跟逻辑绑定在一起，否则就失去了灵活性。而且游戏cp也不知道哪些字段是成功的时候才有，而大部分情况下，失败本身就意味着没有数据（错误信息，不应该当作有效数据，而且现在设计的Result结构有足够的预留字段，承载错误信息）。


**建议**采用如下方式组织回调方式：
```
void FetchConversationListDelegate(Result result, List<ConversationInfo> conversationList);

```

**不建议**采用如下方式：

```
class CallbackResult
{
    public int Error;
    public string ErrorMessage;
}
class LoginResult : CallbackResult
{
    public string OpenID;
    public string Token;
}
```

*  使用event和delegate进行回调，二者的使用场景如下：
    * event：一般用于一对多（1 v N）广播，或者需要长驻监听（也即不只监听一次）的场景
    * delegate：一般用于一次性监听，也即通知完就释放的场景。

## 命名规范
* 事件（event)类型以EventHandler结尾（如，XXXXEventHandler）， 变量以Event结尾（如，XXXEvent）
* delegate ： 一般用于异步回调，类似于C++里的函数指针，其类型命名规则为XXXDelegate，如果用于回调，变量名一般为callback
* 命名：遵循C#的习惯，
    * 用Pascal规则（首字母大写PascalStyle）来命名属性、方法、事件和类名，
    * 用Camel规则（首字母小写camelStyle)来命名成员变量、局部变量和方法的参数
    * 不要使用匈牙利命名法：（接口除外，接口使用“I”前缀），其它类型的命名不得使用任何类型前缀，比如类不使用C开头，int类型的变量不带i或n前缀（使用var替代nVar），成员变量不加m_或m前缀
    * 命名：类及数据结构采用用名词或名词短语，方法使用动宾短语
* 
* 命名上禁止包含累赘的信息，比如，以下信息是累赘的：
```
namesapce GSDK
{
    public interface GSDKIM // 命名空间已经是GSDK了，这里类名的定义就不再需要重复GSDK了
    {
        public void IMLogin(); // IM跟类名本身有的意思得复了，所以，我们禁止。
        public void Logout(); // 简洁，清晰，优雅，因此我们提倡。
    }

    public enum AccountType
    {
        AccountTypeDouyin, // C#的枚举跟C++的不一样，本身就是强类型引用的，这里就不需要再赘述AccounType信息了。。
        Toutiao, // 简洁，清晰，优雅，因此我们提倡。
    }
}
```

# 开发流程
1. 在IIMService.cs和IMPublicDefine.cs分别定义好对外接口和数据结构
2. 在IMService.cs调用BaseIMSDK的相关实现接口
3. 