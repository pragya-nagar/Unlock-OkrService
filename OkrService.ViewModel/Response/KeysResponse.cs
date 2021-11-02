using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class KeysResponse
    {
        public long GoalKeyId { get; set; }
        public string KeyDescription { get; set; }
        public int Progress { get; set; }
        public decimal Score { get; set; }
    }
}
