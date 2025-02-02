// Learn TypeScript:
//  - https://docs.cocos.com/creator/2.4/manual/en/scripting/typescript.html
// Learn Attribute:
//  - https://docs.cocos.com/creator/2.4/manual/en/scripting/reference/attributes.html
// Learn life-cycle callbacks:
//  - https://docs.cocos.com/creator/2.4/manual/en/scripting/life-cycle-callbacks.html

const { ccclass, property } = cc._decorator;

export type DashFunUserProfile = {
    id: string				//dashfun userId
    channelId: string		//渠道方id，此处为TG的用户Id
    displayName: string 	//显示名称
    userName: string 		//用户名
    avatarUrl: string		//avatar地址
    from: number			//用户来源
    createData: number		//创建时间
    loginTime: number		//登录时间
    logoffTime: number		//登出时间
    language: string		//language code
}

export type PaymentRequest = {
    title: string, //付费项目名称，将显示在telegram的支付界面上
    desc: string, //项目描述
    info: string, //支付携带信息
    price: number, //付费项目价格，单位为telegram stars
}

export type PaymentRequestResult = {
    invoiceLink: string, //tg invokce 链接
    paymentId: string, //dashfun平台paymentId
}

export type OpenInvoiceRequest = {
    invoiceLink: string, //tg invokce 链接
    paymentId: string, //dashfun平台paymentId
}

export type OpenInvoiceResult = {
    paymentId: string, //dashfun平台paymentId
    status: "paid" | "canceled" | "failed"
}

type GetUserProfileCallback = (userProfile: DashFunUserProfile) => void;
type GetDataCallback = {
    key: string,
    callback: (data: string) => void
}
type GetDataV2Callback = {
    key: string,
    callback: (data: { key: string, data: string }) => void
}
type SetDataCallback = {
    key: string,
    callback: (state: "success" | "error") => void
}
type RequestPaymentCallback = (result: PaymentRequestResult) => void;
type OpenInvoiceCallback = (result: OpenInvoiceResult) => void;
/**
 * 确保DashFunBridge只有一个实例
 */
@ccclass
export default class DashFunBridge extends cc.Component {

    private static instance: DashFunBridge = null;

    static getInstance() {
        if (DashFunBridge.instance == null) {
            //const node = new cc.Node("DashFunBridge");
            const node = cc.find("DashFunBridge");
            DashFunBridge.instance = node.getComponent(DashFunBridge);
        }

        return DashFunBridge.instance;
    }

    private userProfile: DashFunUserProfile = null;
    public get UserProfile() {
        return this.userProfile;
    }

    private onGetUserProfileResult: GetUserProfileCallback[] = [];
    private onGetDataResult: GetDataCallback[] = [];
    private onGetDataV2Result: GetDataV2Callback[] = [];
    private onSetDataResult: SetDataCallback[] = [];
    private onPaymentRequestResult: RequestPaymentCallback[] = [];
    private onOpenInvoiceResult: OpenInvoiceCallback[] = [];


    // LIFE-CYCLE CALLBACKS:  

    onLoad() {
        cc.game.addPersistRootNode(this.node);
        console.log("DashFunBridge.instance", DashFunBridge.instance)
        if (DashFunBridge.instance != null) {
            this.destroy();
            return;
        }
        window.addEventListener("message", this.onMessage.bind(this));
        this.setLoadingProgress(1);
        DashFunBridge.instance = this;
        console.log("DashFunBridge Loaded...");
    }

    start() {

    }

    private onMessage(event: MessageEvent) {
        const dashfun = event.data.dashfun;
        if (!dashfun) return;

        console.log("DashFunBridge.onMessage", dashfun);

        //回应消息的名字=发送消息的名字+Result
        if (dashfun.method == "getUserProfileResult") {
            console.log(dashfun.result.data) //UserProfile
            const userProfile = dashfun.result.data as DashFunUserProfile;
            this.userProfile = userProfile;

            this.onGetUserProfileResult.forEach(callback => callback(userProfile));
            this.onGetUserProfileResult = [];
        } else if (dashfun.method == "getDataResult") {
            console.log(dashfun.result.data) //data
            const data = dashfun.result.data;
            data.data = atob(data.data);
            const callbacks = this.onGetDataResult.filter(cb => cb.key == data.key);
            if (callbacks.length > 0) {
                callbacks.forEach(cb => cb.callback(data.data));
                this.onGetDataResult = this.onGetDataResult.filter(cb => cb.key != data.key);
            }
        } else if (dashfun.method == "getDataV2Result") {
            console.log(dashfun.result.data) //data
            const data = dashfun.result.data;
            data.data = atob(data.data);
            const callbacks = this.onGetDataV2Result.filter(cb => cb.key == data.key);
            if (callbacks.length > 0) {
                callbacks.forEach(cb => cb.callback(data.data));
                this.onGetDataResult = this.onGetDataResult.filter(cb => cb.key != data.key);
            }
        } else if (dashfun.method == "setDataResult") {
            console.log(dashfun.result.data) //state
            const state = dashfun.result.data;
            const callbacks = this.onGetDataResult.filter(cb => cb.key == state);
            if (callbacks.length > 0) {
                callbacks.forEach(cb => cb.callback(state.state));
                this.onGetDataResult = this.onGetDataResult.filter(cb => cb.key != state);
            }
        } else if (dashfun.method == "requestPaymentResult") {
            console.log(dashfun.result.data) //PaymentRequestResult
            const result = dashfun.result.data as PaymentRequestResult;
            this.onPaymentRequestResult.forEach(callback => callback(result));
            this.onPaymentRequestResult = [];
        } else if (dashfun.method == "openInvoiceResult") {
            console.log(dashfun.result.data) //OpenInvoiceResult
            const result = dashfun.result.data as OpenInvoiceResult;
            this.onOpenInvoiceResult.forEach(callback => callback(result));
            this.onOpenInvoiceResult = [];
        }
    }

    private sendMessageToDashFun(msg: { method: string, payload: any }) {
        window.parent.postMessage({ dashfun: msg }, "*");
    }

    setLoadingProgress(progress: number) {
        this.sendMessageToDashFun({ method: "loading", payload: { value: progress } });
    }

    getUserProfile(onResult: GetUserProfileCallback) {
        this.sendMessageToDashFun({ method: "getUserProfile", payload: {} });
        this.onGetUserProfileResult.push(onResult);
    }

    getData(key: string, onResult: (data: string) => void) {
        this.sendMessageToDashFun({ method: "getData", payload: { key } });
        this.onGetDataResult.push({ key, callback: onResult });
    }

    getDataV2(key: string, onResult: (data: { key: string, data: string }) => void) {
        this.sendMessageToDashFun({ method: "getDataV2", payload: { key } });
        this.onGetDataV2Result.push({ key, callback: onResult });
    }

    setData(key: string, value: string, onResult: (state: "success" | "error") => void) {
        this.sendMessageToDashFun({ method: "setData", payload: { key, data: value } });
        this.onSetDataResult.push({ key, callback: onResult });
    }

    requestPayment(paymentRequest: PaymentRequest, onResult: (result: PaymentRequestResult) => void) {
        this.sendMessageToDashFun({ method: "requestPayment", payload: paymentRequest });
        this.onPaymentRequestResult.push(onResult);
    }

    openInvoice(request: OpenInvoiceRequest, onResult: (result: OpenInvoiceResult) => void) {
        this.sendMessageToDashFun({ method: "openInvoice", payload: request });
        this.onOpenInvoiceResult.push((result) => {
            onResult({ paymentId: request.paymentId, status: result.status });
        });
    }



    //update(dt) { }
}
