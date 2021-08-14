#   SoapProxy.WebApiHost    Soap WebService 代理

代理 WebService 暴露新的 JSON 数据格式的 WebAPI。

##  Features
1.  使用配置文件来配置请求路径与 SOAP 代理类/Action 的映射关系
2.  使用配置文件管理被代理服务地址
3.  支持配置 HTTP 代理(可以通过 Fiddler/Whistle 等代理抓包工具检查源服务的请求与响应)
4.  支持动态加载 SOAP 服务代理类 DLL
5.  支持配置服务IP:Port绑定

##  使用方法
1.  获取 wsdl 文件
    -   从 webservice 获取，浏览器访问服务的 *.asmx 地址后追加 wsdl 参数(如 https://commservice.comm100.io/AndroidPushNotification/AndroidPush.asmx?wsdl)，将打开的XML文件保存为 .wsdl 文件

2.  生成代理类 C# 代码
    -   打开 VS 的开发者命令工具 (VS > Tools > Command Line > Developer Command Prompt)
    -   执行命令 `wsdl /l:cs /out:AndroidPushService.cs AndroidPushService.wsdl` 生成 `AndroidPushService.cs` 代理类文件
    -   执行命令 `csc /out:AndroidPushService.dll /T:library AndroidPushService.cs` 生成 `AndroidPushService.dll`
    -   拷贝`AndroidPushService.dll`到程序根目录下(`SoapProxy.WebApiHost.exe`文件目录)

3.  配置接口映射
    -   程序根目录下创建 `appSettings.json` 文件，并新建 Json 对象
    -   以新的API根路径为属性，并按如下格式完成配置

            "AndroidPushNotification": {            // 新的API请求根路径
                "serviceClient": "AndroidPush",     // 代理类的类名，查看生成的 `.cs` 文件
                "originSvcUrl": "http://local.comm100.io/AndroidPushNotification/AndroidPush.asmx",         // 源服务地址
                "http_proxy":  "http://127.0.0.1:8899",     // HTTP 代理
                "pathActions": {        // 子路径与 SOAP Action 映射关系
                    "Notification": "NewPushNotification"   // 属性名为请求根路径后的路劲，值为要代理的 SOAP Action (可从 .asmx 页面、.wsdl 文件或 .cs 代理类文件中获取)
                }
            }

4.  配置API服务绑定 IP:Port(默认为localhost:9000)
    -   修改配置文件 `SoapProxy.WebApiHost.exe.config`

5.  启动应用(运行`SoapProxy.WebApiHost.exe`，若绑定了全部IP可能需要使用管理员权限运行程序)