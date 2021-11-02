using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class OrganisationCycleDetails
    {
        public string OrganisationName { get; set; }
        public string OrganisationId { get; set; }
        public long CycleDurationId { get; set; }
        public string CycleDuration { get; set; }
        public List<CycleDetails> CycleDetails { get; set; }
    }

    public class CycleDetails
    {
        public string Year { get; set; }
        public bool IsCurrentYear { get; set; }
        public List<QuarterDetails> QuarterDetails { get; set; }
    }

    public class QuarterDetails
    {
        public long OrganisationCycleId { get; set; }
        public bool IsCurrentQuarter { get; set; }
        public string Symbol { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
