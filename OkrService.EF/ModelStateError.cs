using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public class ModelStateError
    {
        public long? EmpId { get; set; } = null;
        public int? Cycle { get; set; } = null;
        public long? OrgId { get; set; } = null;
        public int? Year { get; set; } = null;
        public long? ObjId { get; set; } = null;
        public string ObjName { get; set; } = "-DoNotValidate-";
        public string KeyDesc { get; set; } = "-DoNotValidate-";
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public int MetricId { get; set; }
        public int CurrencyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public decimal ContributorTargetValue { get; set; }
        public DateTime? DueDate { get; set; } = null;
        public long? TeamId { get; set; } = null;
        public bool IsTeams { get; set; }
    }
}
