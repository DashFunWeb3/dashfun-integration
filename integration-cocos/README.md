# DashFun-Cocos 接入指引V0.1

# 简介
详细介绍如何在cocos环境下接入DashFun Sdk

## Step 1: Clone 或者 下载 当前SDK
## Step 2: Setup the DashFun Bridge
1. 拷贝assets/DashFun 到cocos项目的assets目录
2. 在cocos中开启游戏的初始场景
3. 将 assets/DashFun/prefabs/ 目录中的 prefab "DashFunBridge" 拖拽到场景中
4. 在代码中通过 DashFunBridge.getInstance() 访问 DashFunBridge的各个功能

## Step 3: 接入载入进度提示
- 在游戏的资源全部载入完毕后，尽早调用  `DashFunBridge.getInstance().setLoadingProgress(100)`

- DashFun在接收到loading progress为100的时候，才会将Play按钮设置为可用

## step 4: 接入用户数据
- 如果游戏需要用户id和登录相关信息，例如用户名等，可调用 `DashFunBridge.getInstance().getUserProfile(user=>{})`进行获取

## step 5: 存盘数据接入
- 游戏的存盘数据需要保存在DashFun的服务器中，不要存在本地
- 保存数据时调用 `DashFunBridge.getInstance().setData()` 进行保存
- 例： 
``` typescript
    const savedata = {
        progress:10,
        level:20,
        money:1000,
    }
    const key = "testgame";  //保存数据的key
    const saveData = JSON.stringify(savedata); //要保存的数据，必须将数据对象转化为json字符串
    DashFunBridge.getInstance().setData(key, saveData, state=>{})
```
- 读取数据时调用 `DashFunBridge.getInstance().getDataV2()` 进行读取
- 例：
``` typescript
    const key = "testgame"; //要读取数据的key
    DashFunBridge.getInstance().getDataV2(key, {key:string, data:string}=>{
        //数据会返回对应的key和data，需要开发者自行将json字符串转换回数据对象
    })
```

## step 6: 支付接入
- 支付分为两步
    1. 向DashFun获取paymentId和invokceLink
    ```typescript
    DashFunBridge.getInstance().requestPayment({
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
    ```typescript
    DashFunBridge.getInstance().openInvoice({
        invoiceLink: "", //tg invokce 链接，从requestPayment中获取
        paymentId: "", //dashfun平台paymentId，从requestPayment中获取
    }, result=>{
        //result中会返回
        result.paymentId: string, //dashfun平台paymentId
        result.status: "paid" | "canceled" | "failed" //开发者根据status返回的值，如果是paid则为付费成功，客户端增加道具，其他视为失败
    });
    ```

## step 7: 测试
1. 本地直接运行cocos项目，会得到运行时地址，例如 http://localhost:7456
2. 在Telegram中搜索 DashFun [https://t.me/DashFunBot](https://t.me/DashFunBot)
3. 向 DashFun 发送消息 `/test http://localhost:7456`
4. DashFun会回复一条消息，点击 Open Test Game 开始测试