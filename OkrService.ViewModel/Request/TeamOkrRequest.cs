using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Request
{
    public class TeamOkrRequest
    {
        public long TeamId { get; set; }
        public long GoalObjectiveId { get; set; }
        public  List<TeamEmployee> TeamEmployees { get; set; }
    }

    public class TeamEmployee
    {
        public long EmployeeId { get; set; }
        public long GoalObjectiveId { get; set; }
    }
}
