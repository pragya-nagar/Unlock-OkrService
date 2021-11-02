using OKRService.EF;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRService.Service.Contracts
{
    public interface INotificationService
    {
        Task KrUpdateNotifications(string jwtToken, long loginUser, long GoalKeyId, long GoalObjectiveId, int GoalObjectiveProgress, int GoalKeyProgress);
        Task ObjContributorsNotifications(string jwtToken, long loginUser, long ContriEmployeeId, long objId, long ContribObjId);
        Task LockGoalsNotifications(long empId, string jwtToken);
        Task DeleteOkrNotifications(List<long> OkrContributors, int goalType, long GoalId, long empId, string jwtToken);
        Task AlignObjNotifications(string jwtToken, long loginUser, int importedType, long importedId);
        Task BulkUnlockApproveNotificationsAndEmails(EmployeeResult users, UnLockRequest item, string jwtToken);
        Task DeleteContributorsByChangingType(long loginempId, string jwtToken, string objective, long ContriEmp);
        Task DeleteKrNotifications(int Count, List<DeleteKrResponse> deleteKrResponses, List<KrContributors> krContributors, int goalType, long GoalId, long empId, string jwtToken);
        GoalObjective GetGoalObjective(long GoalObjectiveId);
        GoalObjective GetImportedObjective(long importedId);
        GoalObjective GetObjective(long goalId);
        long GetObjectiveParent(long goalId);
        GoalKey GetDeletedKey(long GoalKeyId);
        long? GetKeyParent(long goalkeyId);
        GoalKey GetGoalKey(long goalKeyId);
        Task KeyContributorsNotifications(string jwtToken, long loginUser, long contriEmployeeId, long keyId, long contribKeyId, GoalKey goalKey);
        Task AligningParentObjective(string jwtToken, long loginUser, AddContributorRequest addContributorRequest, GoalKey goalKey);
        Task AcceptsOkr(string jwtToken, long loginUser, ContributorKeyResultRequest contributorKeyResultRequest);
        Task DeclineKr(string jwtToken, long loginUser, ContributorKeyResultRequest contributorKeyResultRequest);
        Task DraftOkrNotifications(string jwtToken, UserIdentity loginUser, GoalObjective goalObjective);
        Task NudgeTeamNotifications(string jwtToken, UserIdentity loginUser, long teamEmployeeId);
        Task TeamKeyContributorsNotifications(string jwtToken, long loginUser, long contribEmployeeId, long keyId, long contribKeyId, GoalKey goalKey);
        Task UpdateContributorsOkrNotifications(GoalObjective goalObjective, UserIdentity userIdentity, string jwtToken);
        Task UpdateContributorsKeyNotifications(GoalKey goalKey, UserIdentity userIdentity, string jwtToken);
        Task UpdateProgress(long? senderEmpid, GoalKey contributors, string jwtToken, long keyId,decimal currentValue, int year);
        Task VirtualLinkingNotifications(long empId, UserIdentity userIdentity, string jwtToken);
        Task UpdateDueDateNotifications(List<DueDateResponse> dueDateResponse, int goaltype, string jwtToken, long loginEmpId);

    }
}
