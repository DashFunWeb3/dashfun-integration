using System.Collections;
using System.Collections.Generic;
using DashFun;
using UnityEngine;
using UnityEngine.UI;

public class TestController : MonoBehaviour
{
    public Text txtUserInfo;

    public void GetUserProfile()
    {
        DashFunBridge.Inst.GetUserProfile(user => { txtUserInfo.text = $"ID:{user.id} TGID:{user.channelId} Name:{user.displayName}"; });
    }

    public void TryToPay1Star()
    {
        DashFunBridge.Inst.RequestPayment(new PaymentRequest
        {
            title = "Test Item",
            desc = "for test",
            info = "for test",
            price = 1
        }, result => { DashFunBridge.Inst.OpenInvoice(result.invoiceLink, result.paymentId, invoiceResult => { txtUserInfo.text = $"Payment[{invoiceResult.paymentId}] -- [{invoiceResult.status}]"; }); });
    }
}