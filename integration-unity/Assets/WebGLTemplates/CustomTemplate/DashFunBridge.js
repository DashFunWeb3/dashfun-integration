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
            window.addEventListener("unload", m => {
                _unityInstance.SendMessage("DashFunBridge", "OnWindowUnload", "unload");
            });
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