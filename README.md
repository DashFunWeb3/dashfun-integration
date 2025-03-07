# DashFun接入说明

## 必要接入
1. 闪屏必须出现DashFun的logo(接入项目里的dashfun-logo.png)，同时要去掉其他logo
2. 必须接入dashfun的存档读档功能，不能存档在本地
3. 付费UI上的付费货币必须改为星星图标(接入项目里的star-icon.png)，50星星等于1美金

## 测试方法
1. 运行游戏，获取运行地址
- cocos引擎，先运行游戏，获取运行地址，例如 `http://localhost:7456`
- Unity引擎，需要build and run之后获得地址
2. 在浏览器中输入地址 `https://dashfun-test.nexgami.com/#game?`，并将本地运行地址加在 ? 后面

- 例如：
`https://dashfun-test.nexgami.com/#game?http://localhost:7456`
- 浏览器会出现如下界面，点击Play即可进行游戏测试
- **注：测试模式下付费会直接成功**

  ![image](https://github.com/user-attachments/assets/746bfc96-68a3-43b7-a2ea-7d093cc1dd47)


