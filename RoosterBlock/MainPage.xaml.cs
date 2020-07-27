using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RoosterBlock
{
    public partial class MainPage : ContentPage
    {
        INotificationManager notificationManager;
        int percentage = 0;

        public MainPage()
        {
            InitializeComponent();

            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var evtData = (NotificationEventArgs)eventArgs;
                ShowNotification(evtData.Title, evtData.Message);
            };
        }

        void OnScheduleClick(object sender, EventArgs e)
        {
            Random random = new Random();
            percentage = random.Next(100);

            string title = $"MMS Picture Received";
            string message = $"{percentage}% chance of containing a \"rooster\".";
            notificationManager.ScheduleNotification(title, message);
        }

        void ShowNotification(string title, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var msg = new Label()
                {
                    Text = $"Notification Received:\nTitle: {title}\nMessage: {message}"
                };

                stackLayout.Children.Add(msg);
            });
        }
    }
}
