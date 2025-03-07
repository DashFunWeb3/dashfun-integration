mergeInto(LibraryManager.library, {
    DashFunBridge_PostMessage: function (method) {
        const parent = window.parent.window;
        parent.postMessage(
            { dashfun: { method: UTF8ToString(method) } },
            "*"
        );
    },

    DashFunBridge_PostMessage_RequestPayment: function (data) {
        const parent = window.parent.window;
        const jsonData = JSON.parse(UTF8ToString(data));
        console.log("DashFunBridge_PostMessage_RequestPayment", jsonData);
        parent.postMessage(
            { dashfun: { method: "requestPayment", payload: jsonData } },
            "*"
        );
    },

    DashFunBridge_PostMessage_OpenInvoice: function (data) {
        const parent = window.parent.window;
        const jsonData = JSON.parse(UTF8ToString(data));
        console.log("DashFunBridge_PostMessage_OpenInvoice", jsonData);
        parent.postMessage(
            { dashfun: { method: "openInvoice", payload: jsonData } },
            "*"
        );
    },
    
    DashFunBridge_PostMessage_SetData: function (data) {
        const parent = window.parent.window;
        const jsonData = JSON.parse(UTF8ToString(data));
        console.log("DashFunBridge_PostMessage_SetData", jsonData);
        parent.postMessage(
            { dashfun: { method: "setData", payload: jsonData } },
            "*"
        );
    },    
    
    DashFunBridge_PostMessage_GetData: function (data) {
        const parent = window.parent.window;
        const jsonData = JSON.parse(UTF8ToString(data));
        console.log("DashFunBridge_PostMessage_GetData", jsonData);
        parent.postMessage(
            { dashfun: { method: "getData", payload: jsonData } },
            "*"
        );
    },    

});