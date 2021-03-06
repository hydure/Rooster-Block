using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

// The assembly attribute registers the INotificationManager interface implementation with the Android DependencyService.
[assembly: Dependency(typeof(RoosterBlock.Droid.AndroidNotificationManager))]
namespace RoosterBlock.Droid
{
    public class AndroidNotificationManager : INotificationManager
    {
        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";
        const int pendingIntentId = 0;

        public const string TitleKey = "title";
        public const string MessageKey = "message";

        bool channelInitialized = false;
        int messageId = -1;
        NotificationManager manager;

        public event EventHandler NotificationReceived;

        public void Initialize()
        {
            CreateNotificationChannel();
        }

        const string MarkAsReadAction = "Mark as Read";

        public int ScheduleNotification(string title, string message)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            messageId++;

            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            intent.SetAction(MarkAsReadAction);

            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId, intent, PendingIntentFlags.OneShot);

            PendingIntent intent2 = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId, intent, PendingIntentFlags.OneShot);

            // Setting SetPriority to PriorityHigh is step 1 of 2 to make notifications appear as Heads-Up
            // Notifications.
            // https://stackoverflow.com/questions/29949501/android-show-notification-with-a-popup-on-top-of-any-application
            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.largeRooosterBlock))
                .SetSmallIcon(Resource.Drawable.smallRoosterBlock)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate)
                .SetPriority(NotificationCompat.PriorityHigh)
                .AddAction(Resource.Drawable.ic_mtrl_chip_checked_black, MarkAsReadAction, intent2);

            var notification = builder.Build();
            manager.Notify(messageId, notification);

            return messageId;
        }

        public void ReceiveNotification(string title, string message)
        {
            var args = new NotificationEventArgs()
            {
                Title = title,
                Message = message,
            };

            NotificationReceived?.Invoke(null, args);
        }

        void CreateNotificationChannel()
        {
            manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);

                // Setting NotificationImportance to High is step 2 of 2 to make notifications appear as Heads-Up
                // Notifications.
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.High)
                {
                    Description = channelDescription
                };

                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }
    }
}
