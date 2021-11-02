using System;

namespace OKRService.ViewModel.Response
{
    public class DirectReportsKeyResult
    {
        public long KrId { get; set; }
        public string KrName { get; set; }
        public decimal KrScore { get; set; }
        public DateTime? KrLastUpdatedTime { get; set; }
        public int KrProgress { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string Cycle { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
