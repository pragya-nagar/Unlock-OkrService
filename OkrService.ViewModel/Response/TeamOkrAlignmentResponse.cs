using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
   public class TeamOkrAlignmentResponse
    {
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }
        public decimal TeamScore { get; set; }
        public int Progress { get; set; }
        public string ProgressCode { get; set; }
        public int KeyCount { get; set; }
        public int ObjectiveCount { get; set; }
       
        public long MemberCount { get; set; }
        public int Sequence { get; set; }
        public LeaderDetailsAlignmentResponse LeaderDetails { get; set; }

        public List<OkrViewResponse> OkrViewResponses { get; set; }



    }
}
