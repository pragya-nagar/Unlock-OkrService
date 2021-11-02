using OKRService.EF;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRService.Service.Contracts
{
    public interface IReportService
    {
        List<ReportMostLeastObjective> ReportMostLeastObjective(long empId, int cycle, int year, long orgId, string token);
        List<ReportContributorsWithScore> GetContributorsByObj(int goalType, long goalId, List<UserResponse> allEmployee);
        Task<AvgOkrScoreResponse> GetAvgOkrScoreReport(long empId, int cycleId, int year, string token, UserIdentity user);
        List<ReportMostLeastObjectiveKeyResult> ReportMostLeastObjectiveKeyResult(long empId, int cycle, int year, long orgId, string token);
        Task<List<UserGoalKeyResponse>> GetWeeklyKrUpdatesReport(long empId, int cycleId, int year, string token, UserIdentity user);
        Task<WeeklyReportResponse> WeeklyReportResponse(long empId, int cycle, int year, string token, UserIdentity identity);
        List<ProgressReportResponse> ProgressReport(int cycle, int year, string token);
        List<QuarterReportResponse> QuarterReport(int cycle, int year, string token, UserIdentity identity);
        List<StatusReportResponse> StatusReport(int cycle, int year, string token, UserIdentity identity);
        Task<decimal> AvgOrganisationalScore(int cycleId, int Year);
        string MinimumOrganisationThreshold();
        List<GoalObjective> GetEmplyeeOkrByCycleId(long empId, int cycleId, int year);
        List<ReportContributorsWithScore> NLevelContributors(long empId, long goalId, int goalType, List<UserResponse> allEmployee);
        List<ObjectiveContributors> GetObjContributlorsByGolaTypeAndId(int goalType, long goalId, long employeeId = 0);
        Task<List<GoalKey>> GetGoalKey(long goalObjectiveId);
        int GetKrUpdates(long GoalKeyId);
        string GetObjectiveName(long goalobjectiveId);
        string GetObjectiveDescription(long goalobjectiveId);
        string GetUpdatedScore(DateTime? StartDate, DateTime endDate, long GoalkeyId);
        int GetCycleStatus(DateTime? StartDate, DateTime? endDate);
        Task<List<GoalKey>> GetGoalScoreDesc(long goalObjectiveId);
        List<GoalObjective> GetEmplyeeOkrByCycleIdOrderbyScore(long empId, int cycleId, int year);
        List<ProgressReportResponse> GetProgressReport(int cycleId, long year, List<UserResponse> allEmployee);
        List<QuarterReportResponse> GetQuarterReport(int cycleId, long year, List<UserResponse> allEmployee);
        GoalObjective GetObjectiveByEmployee(long employeeId, int cycleId, int year);

        Task<TeamPerformanceResponse> TeamPerformance(long empId, int cycleId, int year, string token,
            UserIdentity user);
    }
}
