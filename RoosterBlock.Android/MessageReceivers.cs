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
using Java.Lang;

namespace RoosterBlock.Droid
{
    public class MMSReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "MMS Broadcast Receiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent action received: " + intent.Action);

            // Get the MMS ID. Adapted from: https://stackoverflow.com/questions/10065249/how-to-get-mms-id-android-application
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
                catch (System.Exception error)
                {
                    Log.Error(TAG, "Error requesting the MMS ID: " + error.Message);
                }
            }// if (mmsInboxCursor != null)

            // Get text and picture from MMS message. Adapted from: https://stackoverflow.com/questions/3012287/how-to-read-mms-data-in-android
            string message = ""; // text
            Android.Graphics.Bitmap bitmap = null; // picture
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
                    // Get text.
                    if ("text/plain".Equals(type))
                    {
                        string data = cursor.GetString(cursor.GetColumnIndex("_data"));
                        
                        if (data != null)
                        {
                            message = GetMmsText(partId);
                            Log.Info(TAG, "Body is this: " + message);
                        }
                        else
                        {
                            message = cursor.GetString(cursor.GetColumnIndex("text"));
                            Log.Info(TAG, "Body is this: " + message);
                        }
                    }
                    //Get picture.
                    if ("image/jpeg".Equals(type) || "image/bmp".Equals(type) ||
                            "image/gif".Equals(type) || "image/jpg".Equals(type) ||
                            "image/png".Equals(type))
                    {
                        bitmap = GetMmsImage(partId);
                    }
                } while (cursor.MoveToNext());
            }// if (cursor.MoveToFirst())

            // Analyze message, if there is one.
            string title = "";
            if (message != "")
            {
                (string, bool) result = CleanUpMessage(message);

                // If there were one or more rooster words.
                if (result.Item2)
                {
                    title = "Rooster Text Received From: " + GetAddressNumber(id);
                }
            }
            // TODO: Factor in CNN.
            string probability = "";
            message = "WARNING " + probability + "% chance you received a rooster pic.";
            
            if (title == "")
            {
                title = "Rooster Pic Received From: " + GetAddressNumber(id);
            }
            else
            {
                title = "Rooster Pic & Text Received From: " + GetAddressNumber(id);
            }
            DependencyService.Get<INotificationManager>().ScheduleNotification(title, message);
        }// public override void OnReceive(Context context, Intent intent)

        public static (string, bool) CleanUpMessage(string message)
        {
            string[] roosterWords = { "dick", "pussy", "penis", "ass", "butt", "vagina", "bitch", "slut", "whore", "cock", "cunt", "fuck" };
            bool containedRoosterWords = false;
            // Detect any rooster words from the message.
            int numOfRoosterWordsInMessage = 0;
            List<string> saucyWordsFound = new List<string>(); ;
            foreach (string word in roosterWords)
            {
                saucyWordsFound.Add(word);
                if (Regex.IsMatch(message, string.Format(@"\b{0}\b", word, RegexOptions.IgnoreCase)))
                {
                    numOfRoosterWordsInMessage += 1;
                }
            }

            // If there are any rooster words, replace them with "&#%!" and then notify the user that a
            // rooster has begun messaging them.
            if (numOfRoosterWordsInMessage > 0)
            {
                containedRoosterWords = true;
                foreach (string word in saucyWordsFound)
                {
                    string pattern = string.Format(@"\b{0}\b", word);
                    message = Regex.Replace(message, pattern, "&#%!", RegexOptions.IgnoreCase);
                }
            }
            return (message, containedRoosterWords);
        }

        //Adapted from: https://stackoverflow.com/questions/3012287/how-to-read-mms-data-in-android
        private string GetMmsText(string id)
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
                    string temp = reader.ReadLine();
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

        //Adapted from: https://stackoverflow.com/questions/3012287/how-to-read-mms-data-in-android
        private Android.Graphics.Bitmap GetMmsImage(string _id)
        {
            Android.Net.Uri mmsTextUri = Android.Net.Uri.Parse("content://mms/part/" + _id);
            System.IO.Stream inputStream = null;
            Android.Graphics.Bitmap bitmap = null;
            try
            {
                ContentResolver contentResolver = AndroidApp.Context.ContentResolver;
                inputStream = contentResolver.OpenInputStream(mmsTextUri);
                bitmap = Android.Graphics.BitmapFactory.DecodeStream(inputStream);
            }
            catch (System.IO.IOException error) 
            {
                Log.Error(TAG, "Error reading MMS picture: " + error);
            }
            finally
            {
                if (inputStream != null)
                {
                    try
                    {
                        inputStream.Close();
                    }
                    catch (System.IO.IOException error)
                    {
                        Log.Error(TAG, "Error closing input stream for reading MMS picture: " + error);
                    }
                }
            }
            return bitmap;
        }

        //Adapted from: https://stackoverflow.com/questions/3012287/how-to-read-mms-data-in-android
        private string GetAddressNumber(int id)
        {
            string selectionAdd = new string("msg_id=" + id);
            string uriStr = MessageFormat.Format("content://mms/{0}/addr", id);
            Android.Net.Uri uriAddress = Android.Net.Uri.Parse(uriStr);
            ContentResolver contentResolver = AndroidApp.Context.ContentResolver;
            Android.Database.ICursor query = contentResolver.Query(uriAddress, null,
                selectionAdd, null, null);
            string name = null;
            if (query.MoveToFirst())
            {
                do
                {
                    string number = query.GetString(query.GetColumnIndex("address"));
                    if (number != null)
                    {
                        try
                        {
                            Java.Lang.Long.ParseLong(number.Replace("-", ""));
                            name = number;
                        }
                        catch (NumberFormatException exception)
                        {
                            if (name == null)
                            {
                                name = number;
                            }
                            Log.Error(TAG, "Get address number received an exception: " + exception);
                        }
                    }
                } while (query.MoveToNext());
            }
            if (query != null)
            {
                query.Close();
            }
            return name;
        }
    }

    public class SMSReceiver : BroadcastReceiver
    {
        private static readonly string TAG = "SMS Broadcast Receiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Intent action received: " + intent.Action);

            // Retrieve message from the intent and analyze it.
            SmsMessage msg = Android.Provider.Telephony.Sms.Intents.GetMessagesFromIntent(intent)[0];
            string message = msg.DisplayMessageBody;
            (string, bool) result = MMSReceiver.CleanUpMessage(message);

            // If there were one or more rooster words.
            if (result.Item2)
            {
                string title = "Rooster Text Received From: " + msg.DisplayOriginatingAddress;
                DependencyService.Get<INotificationManager>().ScheduleNotification(title, message);
            }   
        }
    }
}