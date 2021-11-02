using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class TeamPerformanceResponse
    {
        public decimal AvgOrganisationalScore { get; set; }
        public string MinimumOrganisationThreshold { get; set; }
        public List<ContributorsKrResponse> ContributorsKrResponse { get; set; }
    }
}
