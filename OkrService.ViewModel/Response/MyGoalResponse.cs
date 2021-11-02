using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class MyGoalResponse
    {
        public int OkrCount { get; set; }
        public int KeyCount { get; set; }
        public bool IsLocked { get; set; }
        public bool IsScoreLocked { get; set; }
        public List<MyGoalOkrResponse> MyGoalOkrResponses { get; set; }
    }

    public class AlignmentResponse
    {
        public int OkrCount { get; set; }
        public int KeyCount { get; set; }
        public bool IsLocked { get; set; }
        public bool IsScoreLocked { get; set; }
        public bool IsUnLockRequested { get; set; }
        public List<AlignmentMapResponse> MyGoalOkrResponses { get; set; }
    }
}
