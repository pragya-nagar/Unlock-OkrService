using OKRService.ViewModel.Response;
using System.Collections.Generic;

namespace OKRService.ViewModel.Request
{
    public class PeopleViewRequest 
    {        
        public long EmployeeId { get; set; }
        public string EmployeeUniqueId { get; set; }
        public string EmployeeParentId { get; set; }
        public int CycleId { get; set; }
        public int Year { get; set; }
        public bool IsNested { get; set; }
        public bool IsParent { get; set; }
        public bool IsSourceExist { get; set; }
        public bool IsContributorExist { get; set; }
        public long OrgId { get; set; }
        public string Token { get; set; }
        public int ActionLevel { get; set; }
        public string Cycle { get; set; }
        public long TeamId { get; set; }
        public int GoalType { get; set; }
        public int KrCount { get; set; }
        public int ObjCount { get; set; }
        public decimal AvgScore { get; set; }
        public List<PeopleViewContributors> PeopleViewContributors { get; set; }
        public UserResponse UserResponse { get; set; }
        public PeopleViewObjectives PeopleViewObjective { get; set; }
        public List<PeopleViewObjectives> PeopleViewObjectives { get; set; } 
        public List<PeopleViewKeyResults> PeopleViewKeyResults { get; set; }
        public List<ParentObjectiveResponse> ParentObjList { get; set; }
        public List<long> SourceParentObjList { get; set; }
        public EmployeeResult AllEmployee { get; set; }
        public CycleDetails CycleDetail { get; set; }
        public OrganisationCycleDetails OrganisationCycleDetails { get; set; }
        public List<PeopleViewResponse> PeopleViewResponse { get; set; }
        public List<string> NameList { get; set; } = new List<string>();
    }

    public class PeopleObjectiveRequest
    {

    }
   
}
