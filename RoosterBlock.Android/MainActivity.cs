using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Xamarin.Forms;

namespace RoosterBlock.Droid
{
    // TODO: The SingleTop mode prevents multiple instances of an Activity from being started while the application is in the foreground. This LaunchMode may
    // not be appropriate for applications that launch multiple activities in more complex notification scenarios. For more information about LaunchMode
    // enumeration values, see Android Activity LaunchMode.  *** The LaunchMode.SingleTop may not be the right mode for this app. ***
    // https://developer.android.com/guide/topics/manifest/activity-element#lmode
    [Activity(
        Label = "Rooster Block",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize,
        LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        // Constants for the MMS Observer and Receiver.
        public static readonly string MMS_RECEIVED = "MMSObserver.intent.action.MMS_RECEIVED";
        static readonly Android.Net.Uri MMS_URI = (Android.Net.Uri)"content://mms";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            // When the application is started by notification data, the Intent data will be passed to this OnCreate method.
            CreateNotificationFromIntent(Intent);

            // Create and register the MMS Observer to the content resolver.
            MMSObserver mmsObserver = new MMSObserver(MMS_URI);
            ContentResolver.RegisterContentObserver(MMS_URI, false, mmsObserver);

            // Create and register the MMS Receiver to receive only MMS_RECEIVED intents, 
            // which only MMS Observer sends.
            MMSReceiver mmsReceiver = new MMSReceiver();
            IntentFilter mmsReceiverIntentFilter = new IntentFilter(MMS_RECEIVED);
            RegisterReceiver(mmsReceiver, mmsReceiverIntentFilter);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// If the application is already running in the foreground, the Intent data will be passed to this OnNewIntent
        /// method. Note that when the application is started by notification data, the Intent data will be passed to
        /// the OnCreate method.
        /// </summary>
        /// <param name="intent">This contains the title and message of the notification.</param>
        protected override void OnNewIntent(Intent intent)
        {
            CreateNotificationFromIntent(intent);
        }

        /// <summary>
        /// Create the Android notification when a message is received.
        /// </summary>
        /// <param name="intent">This contains the title and message of the notification.</param>
        public static void CreateNotificationFromIntent(Intent intent)
        {
            // TODO: Android offers many advanced options for notifications. For more information, see Notifications in
            // Xamarin.Android.
            // https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/notifications/
            if (intent?.Extras != null)
            {
                string title = intent.Extras.GetString(AndroidNotificationManager.TitleKey);
                string message = intent.Extras.GetString(AndroidNotificationManager.MessageKey);
                DependencyService.Get<INotificationManager>().ReceiveNotification(title, message);
            }
        }
    }
}
