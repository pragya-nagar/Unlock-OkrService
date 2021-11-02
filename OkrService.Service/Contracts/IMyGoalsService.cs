using OKRService.EF;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRService.Service.Contracts
{
    public interface IMyGoalsService
    {
        Task<List<MyGoalsRequest>> InsertGoalObjective(List<MyGoalsRequest> myGoalsRequests, UserIdentity loginUser, string jwtToken);
        Task<string> DeleteOkr(long goalKeyId, long goalObjectiveId, UserIdentity loginUser);
        List<TypeOfGoalCreation> GetTypeOfGoalCreations();
        Task<MyGoalResponse> MyGoal(long empId, int cycle, int year, string token, UserIdentity identity);
        Task<MyGoalsRequest> UpdateObjective(MyGoalsRequest myGoalsRequests, UserIdentity loginUser, string jwtToken);
        Task<string> DeleteContributors(long employeeId, long goalKeyId, UserIdentity userIdentity, string jwtToken);
        Task<bool> DeleteOkrKr(long employeeId, long goalObjectiveId, int goalType, UserIdentity userIdentity, string jwtToken);
        Task<List<DeleteKrResponse>> DeleteOkrKr(long employeeId, long goalObjectiveId, DateTime cycleStartDate, DateTime cycleEndDate);
        List<ContributorsResponse> GetContributorsByGoalTypeAndId(int goalType, long goalId, string token);
        Task<string> UpdateKeyScore(KeyScoreUpdate keyScoreUpdate, UserIdentity userIdentity, string token);
        Task<AlignResponse> AlignObjective(long empId, int cycle, int year, string token, UserIdentity loginUser);
        Task<string> LockGoals(UnLockRequest unLockRequest, string jwtToken);
        MyGoalDetailResponse MyGoalDetailResponse(int type, long typeId, string token);
        Task<UnLockLog> UnLockGoal(UnLockRequest unLockRequest, UserIdentity userIdentity);
        Task<long> BulkUnlockApprove(List<UnLockRequest> unLockRequest, UserIdentity userIdentity, string jwtToken);
        List<UnLockLog> UnlockLog();
        AlignStatusResponse AlignStatus(long employeeId, int sourceType, long sourceId);
        Task<MyGoalsPdfResponse> DownloadPDf(long empId, int cycle, int year, string token, UserIdentity loginUser);
        GoalObjective GetGoalObjective(long goalObjectiveId);
        List<GoalObjective> GetObjectiveContributor(int goalType, long goalId);
        Task<bool> IsAlreadyRequestedAsync(UnLockRequest unLockRequest);
        Task<GoalObjective> InsertObjective(GoalObjective goalObjective);
        Task<GoalKey> InsertKeyResults(GoalKey goalKey);
        Task<GoalKey> UpdateKeyResults(GoalKey goalKey);
        GoalKey GetGoalKeyByObjectiveId(long goalObjectiveId);
        GoalKey GetKeyFromKeyId(long goalKeyId);
        List<GoalObjective> GetEmployeeOkrByCycleId(long empId, int cycleId, int year);
        List<GoalKey> GetGoalKey(long goalObjectiveId);
        int GetKeyCount(long goalObjectiveId);
        string KeyScore(KeyScoreUpdate keyScoreUpdate, long loginEmpId);
        GoalObjective GetChildObjective(long empId, long goalObjectiveId);
        bool IsKeyImported(long goalKeyId, long empId);
        bool IsObjectiveImported(long goalObjectiveId, long empId);
        bool IsKeyImportedGoal(long goalKeyId, long empId);
        List<GoalObjective> GetAssignedObjectiveByGoalObjectiveId(long goalObjId, int cycleId, int year);
        List<GoalObjective> GetAssignedObjectiveByImportId(long importId, int cycleId, int year);
        List<GoalObjective> GetAssignedAlignedObjective(long goalId, int cycleId, int year, bool isAlign);
        Task<UnLockLog> SaveUnlockLog(UnLockLog unLockLog);
        Task<UnLockLog> UpdateUnlockLog(UnLockLog unLockLog);
        string GetKeyDesc(long goalKeyId);
        GoalKey GetImportedKey(long importedId);
        UnLockLog GetUnLockLogDetails(long empId, int year, int cycle);
        List<GoalObjective> GetObjectiveSource(long goalObjectiveId);
        GoalObjective GetGoalObjectiveByImportedId(long importedId, long empId);
        Task<UnLockLog> GetUnLockLogAsync(UnLockRequest unLockRequest);
        int GetCountOfKeyResult(long goalId);
        int GetCount(long goalId);
        Task<bool> UpdateOkrSequence(List<UpdateSequenceRequest> updateSequenceRequests, UserIdentity userIdentity);
        List<KrStatusContributorResponse> GetKrStatusContributors(int goalType, long goalKeyId, string token);
        Task<long> UpdateGoalAttributes(MyGoalsDetails data, UserIdentity loginUser, string jwtToken);
        Task<KeyDetailsResponse> GetKeyDetails(long goalKeyId, string token);
        Task<GoalObjectiveResponse> GetGoalByGoalObjectiveId(long goalObjectiveId);
        Task<long> UpdateContributorsKeyResult(ContributorKeyResultRequest contributorKeyResultRequest, UserIdentity userIdentity, string jwtToken);
        Task<long> UpdateGoalObjective(UpdateGoalRequest updateGoalRequest, UserIdentity userIdentity, string token);
        Task<ContributorDetailRequest> AddContributor(ContributorDetailRequest contributorDetails, UserIdentity loginUser, string jwtToken);
        ////void KeyValueUpdate(long empId, long goalKeyId, decimal currentValue, DateTime dueDate, DateTime cycleStartDate, DateTime cycleEndDate);
        Task<bool> UpdateGoalDescription(UpdateGoalDescriptionRequest updateGoalDescriptionRequest, UserIdentity userIdentity, string token);
        GoalSequence GetGoalSequence(UpdateSequenceRequest sequenceRequest, long employeeId);
        Task<GoalSequence> UpdateGoalSequence(GoalSequence goalSequence);
        Task<long> BecomeContributor(AddContributorRequest addContributorRequest, UserIdentity loginUser, string jwtToken);
        GoalKey GetGoalKeyDetails(long goalKeyId);
        Task<List<GoalKey>> GetKeyContributorAsync(int goalType, long goalKeyId);
        Task<bool> DeleteOkrKrTeam(long employeeId, long goalObjectiveId, int goalType, long teamId, UserIdentity userIdentity, string jwtToken);
        Task<List<LinkedObjectiveResponse>> LinkObjectivesAsync(long searchEmployeeId, int searchEmployeeCycleId, string token, UserIdentity identity);
        Task<bool> ResetOkr(long employeeId, long goalObjectiveId, int goalType, UserIdentity userIdentity, string jwtToken);
        Task<bool> ChangeOwner(UserIdentity identity, long goalObjectiveId, long newOwner, string jwtToken);
        Task<bool> UpdateTeamLeaderOkr(UpdateTeamLeaderOkrRequest teamLeaderOkrRequest, UserIdentity identity, string jwtToken);
        Task<List<DueDateResponse>> UpdateDueDateAlignment(UpdateDueDateRequest updateDueDateRequest, UserIdentity userIdentity, string jwtToken, bool IsNotifications);
        Task<bool> IsAnyOkr(long employeeId);

    }
}
