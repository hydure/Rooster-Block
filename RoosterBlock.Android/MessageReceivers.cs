using Android.Content;
using Android.Util;
using Android.Telephony;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;
using System;
using System.Text.RegularExpressions;
using Android.Icu.Text;
using System.Collections.Generic;
using Android.OS;
using Android.Provider;

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

            // Get the MMS ID.
            ContentResolver contentResolver = AndroidApp.Context.ContentResolver;
            Android.Net.Uri mmsInboxUri = Android.Net.Uri.Parse("content://mms");
            Android.Database.ICursor mmsInboxCursor = contentResolver.Query(mmsInboxUri, new string[]
                {"_id","msg_box","ct_t","date"}, "msg_box=1 or msg_box=2", null, null);
            int id = -1;
            if (mmsInboxCursor != null)
            {
                try
                {
                    if (mmsInboxCursor.MoveToFirst())
                    {
                        id = Int32.Parse(mmsInboxCursor.GetString(0));
                        Log.Info(TAG, "Id is this: " + mmsInboxCursor.GetString(0));
                    }
                }
                catch (Exception error)
                {
                    Log.Error(TAG, "Error requesting the MMS ID: " + error.Message);
                }
            }

            // Get text from MMS message.
            string selectionPart = "mid=" + id;
            Android.Net.Uri mmsTextUri = Android.Net.Uri.Parse("content://mms/part");
            Android.Database.ICursor cursor = contentResolver.Query(mmsTextUri, null,
                selectionPart, null, null);
            if (cursor.MoveToFirst())
            {
                do
                {
                    string partId = cursor.GetString(cursor.GetColumnIndex("_id"));
                    string type = cursor.GetString(cursor.GetColumnIndex("ct"));
                    if ("text/plain".Equals(type))
                    {
                        string data = cursor.GetString(cursor.GetColumnIndex("_data"));
                        string body;
                        if (data != null)
                        {
                            body = getMmsText(partId);
                            Log.Info(TAG, "Body is this: " + body);
                        }
                        else
                        {
                            body = cursor.GetString(cursor.GetColumnIndex("text"));
                            Log.Info(TAG, "Body is this: " + body);
                        }
                    }
                } while (cursor.MoveToNext());
            }
        }

        private String getMmsText(String id)
        {
            Android.Net.Uri partURI = Android.Net.Uri.Parse("content://mms/part/" + id);
            System.IO.Stream inputStream = null;
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            try
            {
                ContentResolver contentResolver = AndroidApp.Context.ContentResolver;
                inputStream = contentResolver.OpenInputStream(partURI);
                if (inputStream != null)
                {
                    Java.IO.InputStreamReader inputStreamReader = new Java.IO.InputStreamReader(inputStream, "UTF-8");
                    Java.IO.BufferedReader reader = new Java.IO.BufferedReader(inputStreamReader);
                    String temp = reader.ReadLine();
                    while (temp != null)
                    {
                        stringBuilder.Append(temp);
                        temp = reader.ReadLine();
                    }
                }
            }
            catch (System.IO.IOException error) 
            {
                Log.Error(TAG, "Error reading MMS text: " + error);
            }
            finally
            {
                if (inputStream != null)
                {
                    try
                    {
                        inputStream.Close();
                    }
                    catch (System.IO.IOException error) {
                        Log.Error(TAG, "Error closing input stream for reading MMS text: " + error);
                    }
                }
            }
            return stringBuilder.ToString();
        }
    }

    public class SMSReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "SMS Broadcast Receiver";
        private readonly string[] roosterWords = { 
            "dick", "pussy", "penis", "ass", "butt", "vagina", "bitch", "slut", "whore", "cock", "cunt", "fuck"
        };

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent action received: " + intent.Action);

            // Retrieve message from the intent.
            SmsMessage msg = Android.Provider.Telephony.Sms.Intents.GetMessagesFromIntent(intent)[0];
            string title = "New Text Recieved From: " + msg.DisplayOriginatingAddress;
            string message = msg.DisplayMessageBody;

            // Detect any rooster words from the message.
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

            // If there are any rooster words, replace them with "&#%!" and then notify the user that a
            // rooster has begun messaging them.
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