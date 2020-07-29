using Android.Content;
using Android.Util;
using Android.Telephony;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;
using System;
using System.Text.RegularExpressions;
using Android.Icu.Text;
using System.Collections.Generic;

namespace RoosterBlock.Droid
{
    public class MMSReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "MMS Broadcast Receiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent action received: " + intent.Action);
            string title = "New Pic Received";
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
                } while (query.MoveToNext());
            }
        }
    }

    public class SMSReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "SMS Broadcast Receiver";
        private readonly string[] roosterWords = { "dick", "pussy", "penis", "ass", "butt", "vagina", "bitch", "slut", "whore" };

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent action received: " + intent.Action);

            
            SmsMessage msg = Android.Provider.Telephony.Sms.Intents.GetMessagesFromIntent(intent)[0];
            string title = "New Text Recieved From: " + msg.DisplayOriginatingAddress;
            string message = msg.DisplayMessageBody;

            int numOfSaucyWordsInMessage = 0;
            List<string> saucyWordsFound = new List<string>(); ;
            foreach (string word in roosterWords)
            {
                saucyWordsFound.Add(word);
                if(Regex.IsMatch(message, String.Format(@"\b{0}\b", word, RegexOptions.IgnoreCase)))
                {
                    numOfSaucyWordsInMessage += 1;
                }
            }

            if (numOfSaucyWordsInMessage > 0)
            {
                foreach (string word in saucyWordsFound)
                {
                    string pattern = String.Format(@"\b{0}\b", word);
                    message = Regex.Replace(message, pattern, "&#%!", RegexOptions.IgnoreCase);
                }
                DependencyService.Get<INotificationManager>().ScheduleNotification(title, message);
            }   
        }
    }
}