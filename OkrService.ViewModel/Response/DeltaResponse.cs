using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class DeltaResponse
    {
        
        public decimal LastSevenDaysProgress { get; set; }
        public List<ContributorsLastSevenDaysProgressResponse> ContributorsLastSevenDaysProgress { get; set; }
        public decimal OnTrack { get; set; }
        public decimal AtRisk { get; set; }
        public decimal Lagging { get; set; }
    }
}
