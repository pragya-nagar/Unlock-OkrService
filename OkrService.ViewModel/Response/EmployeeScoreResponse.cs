using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class EmployeeScoreResponse
    {
        public int OkrCount { get; set; }
        public int KrCount { get; set; }
        public decimal AvgScore { get; set; }
    }
}
