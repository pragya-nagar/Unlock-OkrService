using System.Collections.Generic;

namespace OKRService.ViewModel.Request
{
    public class NotificationsCommonRequest
    {
        public long By { get; set; }
        public string NotificationText { get; set; }
        public int AppId { get; set; }
        public long NotificationType { get; set; }
        public int MessageType { get; set; }
        public string JwtToken { get; set; } = null;
        public string Url { get; set; } = "";
        public List<long> NotificationToList { get; set; } = null;
        public long To { get; set; } = 0;
    }
}
