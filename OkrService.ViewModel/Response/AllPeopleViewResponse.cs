using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    
    public class PeopleViewResponse
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string Image { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public decimal Score { get; set; }
        public bool IsContributorExist { get; set; }
        public bool IsSourceExist { get; set; }
        public int ObjectiveCount { get; set; }
        public int KeyResultCount { get; set; }
        public string EmployeeUniqueId { get; set; }
        public List<string> Parent { get; set; } = new List<string>();
        public bool IsParent { get; set; }
        public int ActionLevel { get; set; }
        public DateTime? CycleEndDate { get; set; }
        public int Progress { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public List<string> NameList { get; set; } = new List<string>();
        public List<PeopleViewObjectives> PeopleViewObjectives { get; set; }
        public List<PeopleViewContributors> PeopleViewContributors { get; set; }
    }

    public class PeopleViewContributors
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string Image { get; set; }
    }

    public class PeopleViewKeyResults
    {
        public long KrId { get; set; }
        public string KrName { get; set; }
        public decimal KrScore { get; set; }
        public DateTime? KrLastUpdatedTime { get; set; }
        public int KrProgress { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string Cycle { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedOn { get; set; }

    }
    public class PeopleViewObjectives
    {
        public int ObjectiveType { get; set; }
        public long ObjectiveId { get; set; }
        public string Name { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Score { get; set; }
        public string Cycle { get; set; }
        public int Progress { get; set; }
        public int ObjectiveTypeId { get; set; }
        public int ObjectiveStatusId { get; set; }
        public DateTime? ObjectiveLastUpdatedTime { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<PeopleViewKeyResults> PeopleViewKeyResults { get; set; }
    }


}
