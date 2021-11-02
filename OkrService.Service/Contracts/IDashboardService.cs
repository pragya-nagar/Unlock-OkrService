using OKRService.EF;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRService.Service.Contracts
{
    public interface IDashboardService
    {
        Task<DashboardOkrResponse> GetGoalDetailById(long goalObjectiveId, int cycle, int year, string token, UserIdentity identity);
        Task<GoalObjective> GetDeletedGoalObjective(long goalObjectiveId);
        Task<List<GoalObjective>> GetEmployeeOkrByCycleId(long empId, int cycleId, int year);
        Task<List<GoalKey>> GetGoalKey(long goalObjectiveId);
        Task<GoalObjective> GetGoalObjective(long goalObjectiveId);
        Task<List<TeamOkrCardResponse>> GetTeamOkrCardDetails(long empId, int cycle, int year, string token, UserIdentity identity);
        Task<bool> NudgeTeamAsync(NudgeTeamRequest nudgeTeamRequest, string token, UserIdentity identity);
        Task<List<GoalObjective>> GetEmployeeOkrByTeamId(long teamId, long empId, int cycleId, int year);
        Task<TeamOkrResponse> GetTeamOkrGoalDetailsById(long teamId, long empId, int cycle, int year, string token, UserIdentity identity);
        Task<bool> UpdateTeamOkrCardSequence(List<UpdateTeamSequenceRequest> updateTeamSequenceRequests, UserIdentity userIdentity);
        Task<TeamSequence> GetTeamSequence(UpdateTeamSequenceRequest sequenceRequest, long employeeId);
        Task<TeamSequence> UpdateTeamSequence(TeamSequence teamSequence);
        Task<TeamSequence> InsertTeamSequence(TeamSequence teamSequence);
        Task<List<TeamSequence>> GetTeamSequenceById(long empId, int cycleId);
        Task<List<GoalKey>> GetGoalKeyByTeamId(long teamId, long goalObjectiveId);
        Task<List<DirectReportsResponse>> AllDirectReportsResponseAsync(long empId, List<string> searchTexts, int cycle, int year, string token, UserIdentity identity, string sortBy);
        Task<bool> NudgeDirectReportAsync(long empId, string token, UserIdentity identity);
        Task<AllOkrDashboardResponse> AllOkrDashboardAsync(long empId, int cycle, int year, string token, UserIdentity identity);
        Task<DeltaResponse> DeltaScore(long empId, int cycle, int year, UserIdentity identity, string token, EmployeeResult allEmployee, QuarterDetails quarterDetails, OrganisationCycleDetails cycleDurationDetails);
        Task<List<RecentContributionResponse>> RecentContribution(long empId, int cycle, int year, UserIdentity identity, EmployeeResult allEmployee);
        Task<List<EmailTeamLeaderResponse>> GetTeamGoals(long teamId, int cycle, int year);
        Task<VirtualAlignmentResponse> GetVirtualAlignment(long goalObjectiveId, UserIdentity identity, string token);
        Task<bool> IsAlreadyCreatedOkr(long employeeId);
        Task<TeamDetails> TeamDetailsById(long teamId, long sourceId, long goalKeyId, string jwtToken);
        Task<AllOkrDashboardResponse> ArchiveDashboardAsync(long empId, int cycle, int year, string token, UserIdentity identity);
        Task<EmployeeScoreResponse> GetEmployeeScoreDetails(long empId, int cycle, int year, string token, UserIdentity identity);

    }
}
