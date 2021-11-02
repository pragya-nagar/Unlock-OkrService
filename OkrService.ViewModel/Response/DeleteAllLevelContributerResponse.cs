using System;

namespace OKRService.ViewModel.Response
{
    public class DeleteAllLevelContributerResponse
    {
        public long GoalId { get; set; }
        public long GoalImportedId { get; set; }
        public long EmployeeId { get; set; }
        public int ObjLevel { get; set; }

    }
    public class DeleteAllLevelKrContributerResponse
    {
        public long GoalId { get; set; }
        public long GoalImportedId { get; set; }
        public long? EmployeeId { get; set; }
        public int ObjLevel { get; set; }

    }

    public class AllLevelObjectiveResponse
    {
        public long GoalId { get; set; }
        public long GoalImportedId { get; set; }
        public long EmployeeId { get; set; }
        public int ObjLevel { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public decimal Score { get; set; }
        public int Progress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrivate { get; set; }
        public long LinkedObjectiveId { get; set; }
        public long Owner { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public int ObjectiveCycleId { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; } 
        public int Year { get; set; }
        public long Source { get; set; }
        public int Sequence { get; set; }
        public int GoalStatusId { get; set; }
        public int GoalTypeId { get; set; }
        public long TeamId { get; set; }
        public bool IsCoachCreation { get; set; } 

    }

    public class AllLevelKrResponse
    {
        public long GoalId { get; set; }
        public long GoalImportedId { get; set; }
        public long? EmployeeId { get; set; }
        public int ObjLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public long Owner { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public long GoalobjectiveId { get; set; }

        public string KeyDescription { get; set; }
        public decimal Score { get; set; }
       
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } 
 
        public bool IsActive { get; set; } = true;
        public int Progress { get; set; }
       
        public long Source { get; set; }
      
        public int MetricId { get; set; }
        public int AssignmentTypeId { get; set; }
        public int CurrencyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int KrStatusId { get; set; }
        public int CycleId { get; set; }
        public string CurrencyCode { get; set; }
        public int GoalStatusId { get; set; }
        public decimal ContributorValue { get; set; }
        public decimal StartValue { get; set; }

        public long TeamId { get; set; }
        public string KeyNotes { get; set; } = "";
   
    }

    public class CteResponse
    {
        public long GoalKeyId { get; set; }
        public long GoalObjectiveId { get; set; }
        public decimal Score { get; set; }
        public decimal StartValue { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ContributorValue { get; set; }
        public decimal TargetValue { get; set; }
        public long ImportedId { get; set; }
        public long Source { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }

    }

    public class GoalScore
    {
        public long GoalObjectiveId { get; set; }
        public decimal Score { get; set; }
    }
}
