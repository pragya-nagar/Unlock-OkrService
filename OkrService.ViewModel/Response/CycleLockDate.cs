
using System;

namespace OKRService.ViewModel.Response
{
    public class CycleLockDate
    {
        public bool IsGaolLocked { get; set; } = true;
        public bool IsScoreLocked { get; set; } = true;
        public DateTime GoalSubmitDate { get; set; }
    }
}
