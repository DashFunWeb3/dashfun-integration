# DashFun-Unity 接入指引V0.1

# 简介
详细介绍如何在Unity环境下接入DashFun Sdk

# 接入要求
1. Unity的PlayerSetting/Player中Splash Image的设置里，Logos必须包含DashFun的logo
2. 必须使用dashfun的玩家数据保存和读取功能
3. 付费ui显示的货币必须改为星星，50个星星=1美金

# 接入步骤
## Step 1: Clone 或者 下载 当前SDK
## Step 2: Setup the DashFun Bridge
1. 拷贝assets/DashFun 到Unity项目的assets目录
2. 在Unity中开启游戏的初始场景
3. 将 assets/DashFun/prefabs/ 目录中的 prefab "DashFunBridge" 拖拽到场景中
4. 在代码中通过 DashFunBridge.Inst 访问 DashFunBridge的各个功能

## Step 3: WebGL Template
- **如果当前没有使用自定义的WebGL Template**

    拷贝assets/WebGLTemplates到Unity项目的assets目录即可
- **如果使用了自定义的WebGL Template**
1. 拷贝Assets\WebGLTemplates\CustomTemplate下的DashFunBridge.js到 index.html 所在的目录中
2. 编辑 index.html文件，添加如下内容到<head></head>中
```html
        <head>
            ...
            <script src="DashFunBridge.js"></script>
        </head>
```
3. 更新Unity Loading Code
    
    找到createUnityInstance调用的部分，增加如下代码
```javascript
    //增加创建dashFun代码
    const dashFunInst = dashfunBridge();
    createUnityInstance(canvas, config, (progress) => {
        ... //原有代码
        //增加如下代码
        dashFunInst?.setLoading(progress * 100);
    }).then(unityInstance => {
        ... //原有代码
        //增加如下代码
        dashFunInst?.init(unityInstance);
        dashFunInst?.setLoading(100)
    });
```

## Step 4: 检查Unity Player Setting
- 打开 `PlayeSetting/Player`，在 `Resolution and Presentation` 中，检查 `WebGL Template` 是否正确选中了 `Custom Template`

## step 5: 接入用户数据
- 如果游戏需要用户id和登录相关信息，例如用户名等，可调用 `DashFunBridge.Inst.getUserProfile(user=>{})`进行获取

## step 6: 存盘数据接入
- 游戏的存盘数据需要保存在DashFun的服务器中，不要存在本地
- 保存数据时调用 `DashFunBridge.Inst.setData()` 进行保存
- 例： 
``` C#
    var gamedata = new TestGameData{
        userId:10,
        level:20,
        progress:100,
    }
    var key = "testgame";  //保存数据的key
    var saveData = JsonUtility.ToJson(gamedata); //要保存的数据，必须将数据对象转化为json字符串
    DashFunBridge.Inst.setData(key, saveData, state=>{})
```
- 读取数据时调用 `DashFunBridge.Inst.getData()` 进行读取
- 例：
``` C#
    var key = "testgame"; //要读取数据的key
    DashFunBridge.Inst.getData(key, data=>{
        //数据会返回对应的data，需要开发者自行将json字符串转换回数据对象
        var gamedata = JsonUtility.FromJson<TestGameData>(data);
    })
```

## step 7: 支付接入
- 支付分为两步
    1. 向DashFun获取paymentId和invoiceLink
    ```C#
    DashFunBridge.Inst.requestPayment(new PaymentRequest{
        title: "测试道具", //付费项目名称，将显示在telegram的支付界面上
        desc: "测试道具详情", //项目描述
        info: "testitem1", //支付携带信息
        price: 200, //付费项目价格，单位为telegram stars
    }, result=>{
        //result中会返回
        result.invoiceLink: string, //tg invokce 链接
        result.paymentId: string, //dashfun平台paymentId
    });
    ```
    2. 调起tg支付
    ```C#
    DashFunBridge.Inst.openInvoice({
        invoiceLink: "", //tg invokce 链接，从requestPayment中获取
        paymentId: "", //dashfun平台paymentId，从requestPayment中获取
    }, result=>{
        //result中会返回
        result.paymentId: string, //dashfun平台paymentId
        result.status: "paid" | "canceled" | "failed" //开发者根据status返回的值，如果是paid则为付费成功，客户端增加道具，其他视为失败
    });
    ```

## Step 8: 测试
1. 编辑器中运行时，不会连接DashFun，如果需要测试，请将游戏打包WebGL并运行，获取运行地址，例如 http://localhost:7456
2. 在Telegram中搜索 DashFun [https://t.me/DashFunBot](https://t.me/DashFunBot)
3. 向 DashFun 发送消息 `/test http://localhost:7456`
4. DashFun会回复一条消息，点击 Open Test Game 开始测试