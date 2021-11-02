using System;

namespace OKRService.ViewModel.Request
{
    public class ContributorDetailRequest
    {

        public long EmployeeId { get; set; }
        public int AssignmentTypeId { get; set; }
        public string ObjectiveName { get; set; }
        public string KeyDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long Source { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public string KrAssignerMessage { get; set; }
        public int KrStatusId { get; set; }
        public int CycleId { get; set; }
        public int MetricId { get; set; }
        public int GoalStatusId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal StartValue { get; set; }
    }
}
