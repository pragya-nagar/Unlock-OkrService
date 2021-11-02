using OKRService.ViewModel.Request;
using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class KeyDetailsResponse
    {
        public long GoalKeyId { get; set; }
        public string KeyDescription { get; set; }
        public long ObjectiveId { get; set; }
        public string ObjectiveName { get; set; }
        public decimal Score { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int Progress { get; set; }
        public int MetricId { get; set; }
        public int CurrencyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int AssignmentTypeId { get; set; }
        public int KrStatusId { get; set; }
        public string CurrencyCode { get; set; }
        public int GoalStatusId { get; set; }
        public decimal StartValue { get; set; }

        public int ImportedType { get; set; }
        public long ImportedId { get; set; }

        public List<ContributorsResponse> Contributors { get; set; }
    }
}
