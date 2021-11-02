using System;

namespace OKRService.ViewModel.Response
{
    public class GoalUnlockDate
    {
        public long Id { get; set; }
        public long OrganisationCycleId { get; set; }
        public int Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime SubmitDate { get; set; }
        public long ObjectStateEnum { get; set; }
    }
}
