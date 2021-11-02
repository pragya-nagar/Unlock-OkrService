using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Request
{
   public class UpdateDueDateRequest
    {
        public int GoalType { get; set; }
        public long GoalId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }


    }
}
