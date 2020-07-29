using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Android.Content.PM;
using AndroidApp = Android.App.Application;

namespace RoosterBlock.Droid
{

    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[]
        {
            Android.Provider.Telephony.Sms.Intents.SmsReceivedAction,
            Android.Provider.Telephony.Sms.Intents.WapPushReceivedAction
        },
        Priority = (int)IntentFilterPriority.HighPriority,
        DataMimeType = "application/vnd.wap.mms-message")]
    public class MMSReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "MMS Broadcast Receiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent action received: " + intent.Action);
            string title = "Oh no";
            string message = "Bad pic, pls delete thx";
            DependencyService.Get<INotificationManager>().ReceiveNotification(title, message);
        }
    }
}