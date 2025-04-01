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
    public class AdRequest
    {
        public string title;
        public string desc;
    }

    public class AdRequestResult
    {
        public string data; //广告结果，success为成功，其他则为失败
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
        private static extern void DashFunBridge_PostMessage_RequestAd(string data);

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
        private Action<AdRequestResult> _adRequestResultCallback;
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

        [Header("编辑器模式下返回的测试用户数据")] public DashFunUser userInEditor = new DashFunUser
        {
            id = "dashfun",
            channelId = "dashfun",
            displayName = "DashFunTest",
            userName = "dashfun",
        };

        [Header("编辑器模式下返回的存盘数据")] [TextArea] public string saveDataInEditor;

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

        /// <summary>
        /// 向DashFun请求用户信息
        /// </summary>
        /// <param name="callback"></param>
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

        /// <summary>
        /// 向DashFun请求创建支付订单
        /// </summary>
        /// <param name="paymentRequest"></param>
        /// <param name="callback"></param>
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


        /// <summary>
        /// 向DashFun请求观看广告
        /// </summary>
        /// <param name="adRequest"></param>
        /// <param name="callback"></param>
        public void RequestAd(AdRequest adRequest, Action<AdRequestResult> callback)
        {
#if UNITY_EDITOR
            var result = new AdRequestResult
            {
                data = "editor_test"
            };
            callback?.Invoke(result);
            return;
#endif
            _adRequestResultCallback = callback;
            var json = JsonUtility.ToJson(adRequest);
            DashFunBridge_PostMessage_RequestAd(json);
        }

        /// <summary>
        /// 向DashFun请求开启支付界面
        /// </summary>
        /// <param name="invoiceLink">支付链接，从RequestPayment中获取</param>
        /// <param name="paymentId">支付Id，从RequestPayment中获取</param>
        /// <param name="callback"></param>
        public void OpenInvoice(string invoiceLink, string paymentId, Action<OpenInvoiceRequestResult> callback)
        {
#if UNITY_EDITOR
            var status = new[] { "paid", "canceled", "failed" };

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

        /// <summary>
        /// 从DashFun获取指定key的数据，空串表示没有找到数据<br/>
        /// <b>注：</b>编辑器模式下会先读取saveDataInEditor，如果没有值，则从PlayerPrefs中读取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        public void GetData(string key, Action<string> callback)
        {
#if UNITY_EDITOR
            //编辑器模式下会先读取saveDataInEditor，如果没有值，则从PlayerPrefs中读取
            var bs = saveDataInEditor;
            if (bs == "")
            {
                bs = PlayerPrefs.GetString(key, "");
            }

            callback?.Invoke(bs);
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

        /// <summary>
        /// 向DashFun保存数据<br/>
        /// 在WebGL模式下，由于浏览器可能不会触发OnApplicationQuit, 所以尽量在用户数据发生变化时实时保存用户数据，以免丢失<br/>
        /// <b>注：</b>编辑器模式下会保存到本地的PlayerPrefs中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        public void SetData(string key, string data, Action<string> callback)
        {
#if UNITY_EDITOR
            PlayerPrefs.SetString(key, data);
            Debug.Log("game data saved: " + key + "--> " + data);
            callback?.Invoke(key);
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

        /// <summary>
        /// 注册程序卸载事件<br/>
        /// DashFun会在页面关闭或刷新时调用这个方法，可将数据保存方法放到回调中<br/>
        /// 但请注意，浏览器在关闭或刷新页面时可能不会等待OnUnload执行完毕，所以不要把保存数据的方法只放在这个OnUnload回调中
        /// </summary>
        /// <param name="onUnload"></param>
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

        private void RequestAdResult(string data)
        {
            var converted = JsonUtility.FromJson<AdRequestResult>(data);
            _adRequestResultCallback?.Invoke(converted);
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

        private void OnApplicationQuit()
        {
            _onUnloadCallback?.Invoke();
        }
    }
}