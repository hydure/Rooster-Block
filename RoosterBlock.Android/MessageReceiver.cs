﻿using Android.Content;
using Android.Util;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

namespace RoosterBlock.Droid
{
    public class MessageReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "MMS Broadcast Receiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent action received: " + intent.Action);
            string title = "New Text Received";
            string message = "Bad pic, pls delete thx";
            DependencyService.Get<INotificationManager>().ScheduleNotification(title, message);
            ContentResolver contentResolver = AndroidApp.Context.ContentResolver;
            string[] projection = new string[] { "*" };
            Android.Net.Uri uri = Android.Net.Uri.Parse("content://mms");
            Android.Database.ICursor query = contentResolver.Query(uri, projection, null, null, null);
            if (query.MoveToFirst())
            {
                do
                {
                    string columnIndexStr = query.GetString(query.GetColumnIndex("ct_t"));
                    if ("application/vnd.wap.multipart.related".Equals(columnIndexStr)) // it's MMS
                    {
                        
                    }
                    else // it's SMS
                    {
                        
                    }
                } while (query.MoveToNext());
            }
        }
    }
}