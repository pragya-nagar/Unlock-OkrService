using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class AvgOkrScoreResponse
    {
        public decimal AvgOrganisationalScore { get; set; }
        public string MinimumOrganisationThreshold { get; set; }
        public List<ContributorsDotResponse> ContributorsDotResponses { get; set; }
    }
}
