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
    //[Activity(Label = "MMSActivity")]
    //public class MMSActivity : Activity
    //{
    //    public static readonly string MMS_RECEIVED = "MMSObserver.intent.action.MMS_RECEIVED";

    //    static readonly Android.Net.Uri MMS_URI = (Android.Net.Uri) "content://mms";

    //    protected override void OnCreate(Bundle savedInstanceState)
    //    {
    //        base.OnCreate(savedInstanceState);

    //        // Create and register the MMS Observer to the content resolver.
    //        MMSObserver mmsObserver = new MMSObserver(MMS_URI);
    //        ContentResolver.RegisterContentObserver(MMS_URI, false, mmsObserver);

    //        // Create and register the MMS Receiver to receive only MMS_RECEIVED intents, 
    //        // which only MMS Observer sends.
    //        MMSReceiver mmsReceiver = new MMSReceiver();
    //        IntentFilter mmsReceiverIntentFilter = new IntentFilter(MMS_RECEIVED);
    //        AndroidApp.Context.RegisterReceiver(mmsReceiver, mmsReceiverIntentFilter);
    //    }
    //}

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

    [BroadcastReceiver]
    public class MMSReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "MMS Broadcast Receiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent received: " + intent.Action);
            Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
        }
    }
}