using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class DueDateResponse
    {
        public long OkrId { get; set; }
        public List<long> KrId { get; set; } = new List<long>();
        public DateTime DueDate { get; set; }
        public long EmployeeId { get; set; }
      
    }
}
