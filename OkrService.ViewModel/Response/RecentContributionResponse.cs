using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class RecentContributionResponse
    {
        public long GoalKeyId { get; set; }
        public decimal RecentContribution { get; set; }
        public List<ContributorsLastSevenDaysProgressResponse> ContributorsRecentProgress { get; set; }
    }
}
