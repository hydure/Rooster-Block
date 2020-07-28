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

    public class MMSObserver : ContentObserver
    {
        private readonly Android.Net.Uri _uri;
        private static readonly string TAG = "MMS Observer";
        public static readonly string MMS_RECEIVED = "MMSObserver.intent.action.MMS_RECEIVED";

        public MMSObserver (Android.Net.Uri uri): base(null)
        {
            _uri = uri;
        }

        public override void OnChange(bool selfChange)
        {
            Log.Info(TAG, "Observed a change.");
            Task.Run(() => {
                Intent mmsIntent = new Intent(MMS_RECEIVED);
                AndroidApp.Context.SendBroadcast(mmsIntent);
            });
            base.OnChange(selfChange);
        }
    }

    [BroadcastReceiver (Enabled = true)]
    //[IntentFilter(new[] { Android.Provider.Telephony.Sms.Intents.WapPushReceivedAction })]
    //[IntentFilter(new[] { "MMSObserver.intent.action.MMS_RECEIVED" })]
    public class MMSReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "MMS Broadcast Receiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent received: " + intent.Action);
        }
    }
}