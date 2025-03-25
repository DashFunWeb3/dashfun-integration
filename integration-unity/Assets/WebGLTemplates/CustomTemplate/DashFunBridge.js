const dashfunBridge = (function () {
    const p = window.parent.window;
    let _unityInstance = null;


    const handleMessage = (message) => {
        const dashfun = message?.data?.dashfun;
        if (!dashfun) return;

        console.log("dashfun message", dashfun);

        const method = dashfun.method;

        if (method == "getUserProfileResult") {
            _unityInstance.SendMessage("DashFunBridge", "GetUserProfileResult", JSON.stringify(dashfun.result.data));
        } else if (method == "requestPaymentResult") {
            _unityInstance.SendMessage("DashFunBridge", "RequestPaymentResult", JSON.stringify(dashfun.result.data));
        } else if (method == "requestAdResult") {
                _unityInstance.SendMessage("DashFunBridge", "RequestAdResult", JSON.stringify(dashfun.result.data));
        } else if (method == "openInvoiceResult") {
            _unityInstance.SendMessage("DashFunBridge", "OpenInvoiceResult", JSON.stringify(dashfun.result.data));
        } else if (method == "setDataResult") {
            _unityInstance.SendMessage("DashFunBridge", "SetDataResult", JSON.stringify(dashfun.result.data));
        } else if (method == "getDataResult") {
            _unityInstance.SendMessage("DashFunBridge", "GetDataResult", dashfun.result.data);
        }
    }

    return {
        init: (unityInstance) => {
            _unityInstance = unityInstance;
            window.addEventListener("message", handleMessage);
            const unityOnBeforeUnload = () => {
                console.log("Browser is closing or refreshing");
                unityInstance.SendMessage('DashFunBridge', 'OnWindowUnload', "unload");
            }
            window.onbeforeunload = unityOnBeforeUnload;
        },
        setLoading: (progress) => {
            p.postMessage({
                dashfun: {
                    method: "loading",
                    payload: {
                        value: progress
                    }
                }
            }, "*")
        }
    }

});