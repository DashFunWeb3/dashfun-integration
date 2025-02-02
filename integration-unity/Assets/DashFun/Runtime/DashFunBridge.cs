using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using static DashFun.Constants;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace DashFun
{
    [Serializable]
    public class DashFunUser
    {
        public string id; //dashfun userId
        public string channelId; //渠道方id，此处为TG的用户Id
        public string displayName; //显示名称
        public string userName; //用户名
        public string avatarUrl; //avatar地址
        public int from; //用户来源
        public long createData; //创建时间
        public long loginTime; //登录时间
        public long logoffTime; //登出时间
        private string language; //language code
    }

    [Serializable]
    public class PaymentRequest
    {
        /// <summary>
        /// 付费项目名称，将显示在telegram的支付界面上
        /// </summary>
        public string title;

        /// <summary>
        /// 项目描述
        /// </summary>
        public string desc;

        /// <summary>
        /// 支付携带信息
        /// </summary>
        public string info;

        /// <summary>
        /// 付费项目价格，单位为telegram stars
        /// </summary>
        public int price;
    }

    [Serializable]
    public class PaymentRequestResult
    {
        public string invoiceLink; //tg invokce 链接
        public string paymentId; //dashfun平台paymentId
    }

    [Serializable]
    public class OpenInvoiceRequest
    {
        public string invoiceLink; //tg invokce 链接
        public string paymentId; //dashfun平台paymentId
    }

    [Serializable]
    public class OpenInvoiceRequestResult
    {
        public string paymentId; //dashfun平台paymentId
        public string status; // "paid"|"canceled"|"failed"
    }

    [Serializable]
    public class GetData
    {
        public string key;
    }

    [Serializable]
    public class SetData
    {
        public string key;
        public string data;
    }

    public class DashFunBridge : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void DashFunBridge_PostMessage(string method);

        [DllImport("__Internal")]
        private static extern void DashFunBridge_PostMessage_RequestPayment(string data);

        [DllImport("__Internal")]
        private static extern void DashFunBridge_PostMessage_OpenInvoice(string data);

        [DllImport("__Internal")]
        private static extern void DashFunBridge_PostMessage_GetData(string data);

        [DllImport("__Internal")]
        private static extern void DashFunBridge_PostMessage_SetData(string data);

        //dashfun data
        private DashFunUser _user;

        //callbacks
        private Action<DashFunUser> _getUserProfileCallback;
        private Action<PaymentRequestResult> _paymentRequestResultCallback;
        private Action<OpenInvoiceRequestResult> _openInvoiceRequestResultCallback;
        private Action<string> _getDataCallback;
        private Action<string> _setDataCallback;

        private Action _onUnloadCallback;

        public DashFunUser User => _user;

        private static DashFunBridge _inst;

        public static DashFunBridge Inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = FindObjectOfType<DashFunBridge>();
                    if (_inst == null)
                    {
                        _inst = new GameObject("DashFunBridge").AddComponent<DashFunBridge>();
                    }
                }

                return _inst;
            }
        }

        public DashFunUser userInEditor = new DashFunUser
        {
            id = "dashfun",
            channelId = "dashfun",
            displayName = "DashFunTest",
            userName = "dashfun",
        };

        [TextArea] public string saveDataInEditor;

        private void Awake()
        {
            if (_inst != null)
            {
                Destroy(gameObject);
                return;
            }

            _inst = this;
            DontDestroyOnLoad(gameObject);
        }

        //send message to dashfun
        public void GetUserProfile(Action<DashFunUser> callback)
        {
#if UNITY_EDITOR
            _user = userInEditor;
            callback?.Invoke(_user);
            return;
#endif
            _getUserProfileCallback = callback;
            DashFunBridge_PostMessage(Method.GetUserProfile);
        }

        public void RequestPayment(PaymentRequest paymentRequest, Action<PaymentRequestResult> callback)
        {
#if UNITY_EDITOR
            var result = new PaymentRequestResult
            {
                invoiceLink = "",
                paymentId = "editor_test"
            };
            callback?.Invoke(result);
            return;
#endif
            _paymentRequestResultCallback = callback;
            var json = JsonUtility.ToJson(paymentRequest);
            DashFunBridge_PostMessage_RequestPayment(json);
        }

        public void OpenInvoice(string invoiceLink, string paymentId, Action<OpenInvoiceRequestResult> callback)
        {
#if UNITY_EDITOR
            var status = new string[] { "paid", "canceled", "failed" };

            //random result in editor
            var idx = Random.Range(0, status.Length);
            var result = new OpenInvoiceRequestResult
            {
                paymentId = paymentId,
                status = status[idx]
            };
            callback?.Invoke(result);
            return;
#endif
            _openInvoiceRequestResultCallback = callback;
            var req = new OpenInvoiceRequest
            {
                invoiceLink = invoiceLink,
                paymentId = paymentId
            };
            var json = JsonUtility.ToJson(req);
            DashFunBridge_PostMessage_OpenInvoice(json);
        }

        public void GetData(string key, Action<string> callback)
        {
#if UNITY_EDITOR
            var bs = Convert.FromBase64String(saveDataInEditor);
            callback?.Invoke(Encoding.UTF8.GetString(bs));
            return;
#endif
            _getDataCallback = callback;
            var req = new GetData
            {
                key = key
            };
            var json = JsonUtility.ToJson(req);
            DashFunBridge_PostMessage_GetData(json);
        }

        public void SetData(string key, string data, Action<string> callback)
        {
#if UNITY_EDITOR
            return;
#endif
            _setDataCallback = callback;
            var req = new SetData
            {
                key = key,
                data = data
            };
            var json = JsonUtility.ToJson(req);
            DashFunBridge_PostMessage_SetData(json);
        }

        public void OnUnload(Action onUnload)
        {
            _onUnloadCallback = onUnload;
        }

        //receive message from dashfun
        private void GetUserProfileResult(string data)
        {
            var converted = JsonUtility.FromJson<DashFunUser>(data);
            _user = converted;
            _getUserProfileCallback?.Invoke(_user);
        }

        private void RequestPaymentResult(string data)
        {
            var converted = JsonUtility.FromJson<PaymentRequestResult>(data);
            _paymentRequestResultCallback?.Invoke(converted);
        }

        private void OpenInvoiceResult(string data)
        {
            var converted = JsonUtility.FromJson<OpenInvoiceRequestResult>(data);
            _openInvoiceRequestResultCallback?.Invoke(converted);
        }

        private void GetDataResult(string data)
        {
            // Debug.Log("GetDataResult " + data);
            // Debug.Log("GetDataResult Length " + data.Length);
            // Debug.Log("equals, " + (data == saveDataInEditor));
            var bs = Convert.FromBase64String(data);
            _getDataCallback.Invoke(Encoding.UTF8.GetString(bs));
        }

        private void SetDataResult(string data)
        {
            _setDataCallback.Invoke(data);
        }

        private void OnWindowUnload(string n)
        {
            _onUnloadCallback?.Invoke();
        }
    }
}