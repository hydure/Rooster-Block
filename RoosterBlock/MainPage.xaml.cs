using System;
using Xamarin.Forms;

namespace RoosterBlock
{
    public partial class MainPage : ContentPage
    {
        readonly INotificationManager notificationManager;
        int percentage = 0;

        public MainPage()
        {
            InitializeComponent();

            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var eventData = (NotificationEventArgs)eventArgs;
                ShowNotification(eventData.Title, eventData.Message);
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
