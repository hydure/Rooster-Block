using System;

namespace RoosterBlock
{
    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; }

        public string Message { get; set; }
    }
}
