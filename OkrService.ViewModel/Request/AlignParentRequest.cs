using OKRService.ViewModel.Response;
using System.Collections.Generic;

namespace OKRService.ViewModel.Request
{
    public class AlignParentRequest
    {        
        public long ObjId { get; set; }
        public int Cycle { get; set; }
        public int Year { get; set; }
        public bool IsAligned { get; set; }
        public long OrgId { get; set; }
        public string Token { get; set; }
        public List<AlignmentMapResponse> MyGoalOkrResponses { get; set; }
        public EmployeeResult AllEmployee { get; set; }
        public CycleDetails CycleDetail { get; set; }
    }
}
