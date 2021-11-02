using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Request
{
    public class UpdateNotificationURLRequest
    {
        public long NotificationsDetailsId { get; set; }
        public string URL { get; set; }
    }
}
