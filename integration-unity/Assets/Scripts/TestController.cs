using System.Collections;
using System.Collections.Generic;
using DashFun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TestGameData
{
    public string userId;
    public int level = Random.Range(1, 10);
    public int progress = Random.Range(10, 20);
}

public class TestController : MonoBehaviour
{
    public Text txtInfo;

    public void GetUserProfile()
    {
        DashFunBridge.Inst.GetUserProfile(user => { txtInfo.text = $"ID:{user.id} TGID:{user.channelId} Name:{user.displayName}"; });
    }

    public void TryToPay1Star()
    {
        DashFunBridge.Inst.RequestPayment(new PaymentRequest
        {
            title = "Test Item",
            desc = "for test",
            info = "for test",
            price = 1
        }, result => { DashFunBridge.Inst.OpenInvoice(result.invoiceLink, result.paymentId, invoiceResult => { txtInfo.text = $"Payment[{invoiceResult.paymentId}] -- [{invoiceResult.status}]"; }); });
    }

    public void SaveGameData()
    {
        var d = new TestGameData();
        DashFunBridge.Inst.GetUserProfile(user =>
        {
            d.userId = user.id;
            DashFunBridge.Inst.SetData("TestGame", JsonUtility.ToJson(d), s =>
            {
                //Unity Editor运行模式下，数据不会被保存，只会输出到Console中
            });
        });
    }

    public void LoadGameData()
    {
        var key = "TestGame";

        DashFunBridge.Inst.GetData(key, result =>
        {
            if (result == "")
            {
                Debug.Log("save data not found");
            }
            else
            {
                var d = JsonUtility.FromJson<TestGameData>(result);
                Debug.Log("Get Data: " + key + "-->");
                Debug.Log(d);
            }
        });
    }
}