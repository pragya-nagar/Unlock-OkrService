using OKRService.EF;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OKRService.Service.Contracts
{
    public interface ICommonService
    {
        List<CycleDetails> GetOrganisationCycleDetail(long orgId, string jwtToken);
        EmployeeResult GetAllUserFromUsers(string jwtToken);
        Task<UserIdentity> GetUserIdentity(string jwtToken);
        Task<UserIdentity> GetUserIdentity();
        OrganisationCycleDetails GetOrganisationCycleDurationId(long orgId, string jwtToken);

        List<KeyContributorsResponse> GetKeyContributors(int goalType, long goalId, List<UserResponse> allEmployee,
            long organisationId);

        List<ContributorsDotResponse> GetContributors(int goalType, long goalId, List<UserResponse> allEmployee,
            long organisationId);

        List<ContributorsResponse> GetObjectiveContributor(int goalType, long goalId, List<UserResponse> allEmployee);
        List<GoalObjective> GetObjectiveContributor(int goalType, long goalId);

        Task<List<ContributorsResponse>> GetObjectiveContributorAsync(int goalType, long goalId,
            List<UserResponse> allEmployee);

        Task<List<GoalObjective>> GetObjectiveContributorAsync(int goalType, long goalId);

        List<ContributorsResponse> GetKRContributor(int goalType, long goalId, List<UserResponse> allEmployee,
            decimal sourceTargetValue = 0);

        List<GoalKey> GetKRContributor(int goalType, long goalId);
        Task<List<GoalKey>> GetKRContributorAsync(int goalType, long goalId);

        Task<List<ContributorsResponse>> GetKRContributorAsync(int goalType, long goalId,
            List<UserResponse> allEmployee, decimal sourceTargetValue = 0);

        List<ContributorsResponse> GetContributor(int goalType, long goalId, List<UserResponse> allEmployee,
            decimal sourceTargetValue = 0);

        Task<List<ContributorsResponse>> GetContributorAsync(int goalType, long goalId, List<UserResponse> allEmployee,
            decimal sourceTargetValue = 0);

        Task<CycleLockDate> IsOkrLocked(DateTime startDate, DateTime dueDate, long empId, int cycleId, int year,
            string jwtToken);

        int GetContributeKrUpdates(int goalType, long goalId);
        void SaveLog(string pageName, string functionName, string errorDetail);
        List<PassportEmployeeResponse> GetAllPassportUsers();
        Task<MailerTemplate> GetMailerTemplateAsync(string templateCode, string jwtToken = null);
        Task<bool> SentMailAsync(MailRequest mailRequest, string jwtToken = null);
        Task SaveNotificationAsync(NotificationsRequest notificationsResponse, string jwtToken = null);
        Task SaveNotificationWithAuthentication(string jwtToken, NotificationsRequest notificationsResponse);
        Task<List<FeedbackResponse>> GetAllFeedback(string jwtToken, long employeeId);
        Task NotificationsAsync(NotificationsCommonRequest notificationsCommonRequest);
        List<long> GetAllLevelContributors(int goalType, long goalId, long empId);
        Task<AlignStatusResponse> GetGoalObjectiveSource(long employeeId, long sourceId);
        Task<AlignStatusResponse> GetGoalKeySource(long employeeId, long sourceId);
        GoalKeyAudit LatestUpdateGoalKey(long goalKeyId);
        List<KrContributors> GetAllLevelKrContributors(int goalType, long goalId, long empId);
        UnLockLog GetApproveUnlockLog(long empId, int cycleId, int year, int status);
        string MinimumOrganisationThreshold();
        int GetKrUpdates(long GoalKeyId);
        long GetGoalkeyId(long goalobjective);
        int GetKrCount(long goalObjectiveId);
        int GetObjCount(long goalObjectiveId);
        DateTime GetDueDate(long goalkeyId);

        List<ContributorPdfResponse> GetObjectiveContributorForPdf(int goalType, long goalId,
            List<UserResponse> allEmployee);

        List<ContributorPdfResponse> GetKRContributorForPdf(int goalType, long goalId, List<UserResponse> allEmployee);
        List<ContributorPdfResponse> GetContributorForPdf(int goalType, long goalId, List<UserResponse> allEmployee);
        Task<string> GetBase64ImagePath(string imagePath);

        int GetProgressIdWithFormula(DateTime dueDate, DateTime cycleStartDate, DateTime cycleEndDate, decimal score,
            long cycleDurationId);

        Task<AllOkrMasterData> GetAllOkrMasterData(string jwtToken);
        Task<OrganisationCycleResponse> GetCurrentCycleAsync(long orgId, string jwtToken = null);
        Task<OrganisationCycleResponse> GetCurrentCycleAsync(long orgId);

        List<KrStatusContributorResponse> GetKrStatusContributor(int goalType, long goalId,
            List<UserResponse> allEmployee);

        Task<KrStatusMessage> UpdateKrStatus(long goalKeyId, string message);
        List<KrContributorResponse> GetKrContributor(int goalType, long goalKeyId, List<UserResponse> allEmployee);

        Task<List<ContributorsResponse>> GetAllContributorAsync(int goalType, long goalId,
            List<UserResponse> allEmployee, UserIdentity userIdentity, string token);

        long GetSourceId(long importedId, int importedType);
        Task UpdateNotificationText(UpdateNotificationURLRequest updateNotificationUrlRequest, string jwtToken = null);
        Task<NotificationsDetails> GetNotifications(long id, string jwtToken = null);
        TeamDetails GetTeamEmployeeByTeamId(long teamId, string jwtToken);
        Task<List<TeamDetails>> GetTeamEmployees();
        Task<OkrStatusMasterDetails> GetAllOkrFiltersAsync(long orgId, string jwtToken = null);
        Task<List<LeaderDetailsResponse>> SearchUserAsync(string finder, string jwtToken = null);
        Task<List<TeamHeadDetails>> GetLeaderOrganizationsAsync(int goalType, string jwtToken = null, long empId = 0, bool isCoach = false);
        List<KrContributors> GetAllLevelTeamKrContributors(int goalType, long goalId, long empId, long teamId);
        List<long> GetAllLevelTeamContributors(int goalType, long goalId, long empId, long teamId);
        List<DirectReportsDetails> GetDirectReportsById(long employeeId, string jwtToken);

        Task<LastSevenDaysProgressResponse> GetLastSevenDaysProgress(long empId, long teamId, int cycle,
            bool isTeamOkrDashboard, UserIdentity identity, long teamLeaderEmployeeId, bool isOwner);

        decimal KeyScore(decimal score);

        Task<List<ContributorsLastSevenDaysProgressResponse>> GetContributorsLastUpdateSevenDays(long employeeId,
            long teamId, int cycleId, bool isTeamOkrDashboard, List<UserResponse> userResponses, UserIdentity identity,
            long teamLeaderEmployeeId, DateTime cycleEndDate);

        Task<LastSevenDaysStatusCardProgress> GetLastSevenDaysStatusCardProgress(long empId, long teamId, int cycle,
            bool isTeamOkrDashboard, QuarterDetails quarterDetails, OrganisationCycleDetails cycleDurationDetails,
            UserIdentity identity, long teamLeaderEmployeeId);

        EmployeeOrganizationDetails GetOrganizationByEmployeeId(long employeeId, string jwtToken);
        DateTime DeltaProgressLastDay();
        Task<decimal> GetGoalKeyRecentProgress(long goalKeyId, DateTime? lastLoginTime);
        Task<decimal> GetGoalKeyRecentContributorValue(long goalKeyId, DateTime? lastLoginTime);

        Task<List<ContributorsLastSevenDaysProgressResponse>> GetContributorRecentProgress(long goalKeyId,
            DateTime? lastLoginTime, UserIdentity identity, EmployeeResult allEmployee);

        Task<bool> GetReadFeedbackResponse(long okrId, string jwtToken);
        Task<List<ActiveOrganisations>> GetAllActiveOrganisations(string jwtToken = null);
        List<AllLevelObjectiveResponse> GetObjectiveSubCascading(long goalKeyId);
        List<ContributorsResponse> GetDistinctObjContributor(List<UserResponse> allEmployee, long empId, List<AllOkrDashboardKeyResponse> keyResponse, bool isCoach, long ownerId);
        public ParentTeamDetails ParentTeamDetails(int goalType, long goalId, List<TeamDetails> teamDetails, long teamId);
        Task<OnBoardingControlResponse> OnBoardingControlDetailById(string jwtToken = null);
        Task<int> GetKeyCount(long empId, int cycleId);
        Task<int> GetOkrCount(long empId, int cycleId);
       
    }
}
