using Microsoft.EntityFrameworkCore;
using OKRService.Common;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OKRService.Service
{
    [ExcludeFromCodeCoverage]
    public class DashboardService : BaseService, IDashboardService
    {
        private readonly IRepositoryAsync<GoalObjective> goalObjectiveRepo;
        private readonly IRepositoryAsync<MessageMaster> messageMaster;
        private readonly IRepositoryAsync<GoalKey> goalKeyRepo;
        private readonly IMyGoalsService myGoalsService;
        private readonly ICommonService commonService;
        private readonly IRepositoryAsync<GoalSequence> goalSequenceRepo;
        private readonly INotificationService notificationService;
        private readonly IRepositoryAsync<TeamSequence> teamSequenceRepo;
        private readonly IRepositoryAsync<GoalKeyAudit> goalKeyAuditRepo;
        private readonly IRepositoryAsync<GoalKeyHistory> goalKeyHistoryRepo;
        private readonly IRepositoryAsync<Constant> constantRepo;

        public DashboardService(IServicesAggregator servicesAggregateService, IMyGoalsService myGoalsServices, ICommonService commonServices, INotificationService notificationServices) : base(servicesAggregateService)
        {
            goalObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<GoalObjective>();
            messageMaster = UnitOfWorkAsync.RepositoryAsync<MessageMaster>();
            goalKeyRepo = UnitOfWorkAsync.RepositoryAsync<GoalKey>();
            myGoalsService = myGoalsServices;
            commonService = commonServices;
            goalSequenceRepo = UnitOfWorkAsync.RepositoryAsync<GoalSequence>();
            notificationService = notificationServices;
            teamSequenceRepo = UnitOfWorkAsync.RepositoryAsync<TeamSequence>();
            goalKeyAuditRepo = UnitOfWorkAsync.RepositoryAsync<GoalKeyAudit>();
            goalKeyHistoryRepo = UnitOfWorkAsync.RepositoryAsync<GoalKeyHistory>();
            constantRepo = UnitOfWorkAsync.RepositoryAsync<Constant>();
        }

        public async Task<DashboardOkrResponse> GetGoalDetailById(long goalObjectiveId, int cycle, int year, string token, UserIdentity identity)
        {
            var dashboardOkrResponse = new DashboardOkrResponse();
            var goalObjective = await GetGoalObjective(goalObjectiveId);
            if (goalObjective != null)
            {
                var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
                var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
                var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
                if (quarterDetails != null)
                {
                    var dashboardKeyResponses = new List<DashboardKeyResponse>();
                    var allEmployee = commonService.GetAllUserFromUsers(token);
                    var allFeedback = await commonService.GetAllFeedback(token, identity.EmployeeId);
                    var keyDetail = await GetGoalKey(goalObjectiveId);

                    foreach (var key in keyDetail)
                    {
                        var keyProgressId = commonService.GetProgressIdWithFormula(key.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), key.Score, cycleDurationDetails.CycleDurationId);
                        var keyScoreUpdateDetail = commonService.LatestUpdateGoalKey(key.GoalKeyId);
                        dashboardKeyResponses.Add(new DashboardKeyResponse
                        {
                            GoalKeyId = key.GoalKeyId,
                            DueDate = key.DueDate,
                            Progress = keyProgressId,
                            Score = key.Score,
                            KeyProgressTime = keyScoreUpdateDetail == null ? key.CreatedOn : keyScoreUpdateDetail.UpdatedOn,
                            Source = key.Source,
                            ImportedType = key.ImportedType,
                            ImportedId = key.ImportedId,
                            KeyDescription = key.KeyDescription,
                            IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId),
                            Contributors = await commonService.GetContributorAsync((int)GoalType.GoalKey, key.GoalKeyId, allEmployee.Results)
                        });
                    }

                    var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == goalObjective.EmployeeId);
                    var objProgressId = commonService.GetProgressIdWithFormula(goalObjective.EndDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), goalObjective.Score, cycleDurationDetails.CycleDurationId);
                    dashboardOkrResponse.MyGoalsDetails = dashboardKeyResponses;
                    dashboardOkrResponse.GoalObjectiveId = goalObjective.GoalObjectiveId;
                    dashboardOkrResponse.Year = goalObjective.Year;
                    dashboardOkrResponse.IsPrivate = goalObjective.IsPrivate;
                    dashboardOkrResponse.ObjectiveDescription = goalObjective.ObjectiveDescription;
                    dashboardOkrResponse.EmployeeId = goalObjective.EmployeeId;
                    dashboardOkrResponse.FirstName = objUser == null ? "N" : objUser.FirstName;
                    dashboardOkrResponse.LastName = objUser == null ? "A" : objUser.LastName;
                    dashboardOkrResponse.ImagePath = objUser?.ImagePath?.Trim();
                    dashboardOkrResponse.ObjectiveName = goalObjective.ObjectiveName;
                    dashboardOkrResponse.StartDate = goalObjective.StartDate;
                    dashboardOkrResponse.EndDate = goalObjective.EndDate;
                    dashboardOkrResponse.DueCycle = quarterDetails.Symbol + "-" + year;
                    dashboardOkrResponse.Progress = objProgressId;
                    dashboardOkrResponse.GoalProgressTime = dashboardKeyResponses.Count <= 0 ? goalObjective.CreatedOn : dashboardKeyResponses.OrderByDescending(x => x.KeyProgressTime).FirstOrDefault().KeyProgressTime;
                    dashboardOkrResponse.Score = goalObjective.Score;
                    dashboardOkrResponse.DueDate = goalObjective.EndDate;
                    dashboardOkrResponse.Source = goalObjective.Source;
                    dashboardOkrResponse.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == goalObjective.GoalObjectiveId);
                    dashboardOkrResponse.Contributors = await commonService.GetContributorAsync((int)GoalType.GoalObjective, goalObjective.GoalObjectiveId, allEmployee.Results);
                }
            }

            return dashboardOkrResponse;
        }

        public async Task<GoalObjective> GetDeletedGoalObjective(long goalObjectiveId)
        {
            return await goalObjectiveRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && !x.IsActive).FirstOrDefaultAsync();
        }

        public async Task<GoalKey> GetGoalKeyById(long goalKeyId)
        {
            return await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalKeyId == goalKeyId && x.IsActive);
        }

        public async Task<List<GoalObjective>> GetEmployeeOkrByCycleId(long empId, int cycleId, int year)
        {
            return await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive && x.Year == year).OrderByDescending(x => x.GoalObjectiveId).OrderBy(x => x.Sequence).ToListAsync();
        }

        public async Task<List<GoalObjective>> GetEmployeeOkrByTeamId(long teamId, long empId, int cycleId, int year)
        {
            return await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive && x.Year == year && x.TeamId == teamId).OrderByDescending(x => x.GoalObjectiveId).OrderBy(x => x.Sequence).ToListAsync();
        }

        public async Task<List<GoalKey>> GetGoalKey(long goalObjectiveId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined).ToListAsync();
        }

        public async Task<List<GoalKey>> GetGoalKeyByTeamId(long teamId, long goalObjectiveId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined && x.TeamId == teamId).ToListAsync();
        }

        public async Task<List<GoalKey>> GetOrphanKey(long employeeId, int cycleId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == 0 && x.CycleId == cycleId && x.IsActive && x.KrStatusId != 3 && x.EmployeeId == employeeId).ToListAsync();
        }

        public async Task<GoalObjective> GetGoalObjective(long goalObjectiveId)
        {
            return await goalObjectiveRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive).FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetAllMessage()
        {
            return await messageMaster.GetQueryable().Select(x => x.MessageDesc).ToListAsync();

        }
        public async Task<List<GoalSequence>> GetGoalSequence(long empId, int cycleId)
        {
            return await goalSequenceRepo.GetQueryable().Where(x => x.GoalCycleId == cycleId && x.EmployeeId == empId && x.IsActive).ToListAsync();
        }

        public async Task<List<GoalObjective>> GetCollaboratorOkrByTeamId(long teamId, long empId, int cycleId, int year)
        {
            var allTeamGoalObjective = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.ImportedId == 0 && x.IsActive && x.Year == year && x.TeamId == teamId && x.Owner != empId).OrderByDescending(x => x.GoalObjectiveId).ThenBy(x => x.Sequence).ToListAsync();

            var empObjectives = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive && x.Year == year && x.TeamId == teamId).Select(x => x.ImportedId).ToListAsync();

            var collaboratorList = (from teamObj in allTeamGoalObjective
                                    where !empObjectives.Contains(teamObj.GoalObjectiveId)
                                    select teamObj).ToList();

            return collaboratorList.ToList();
        }

        public async Task<List<TeamOkrCardResponse>> GetTeamOkrCardDetails(long empId, int cycle, int year, string token, UserIdentity identity)
        {
            var teamOkrCardList = new List<TeamOkrCardResponse>();
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            if (quarterDetails == null) return teamOkrCardList.OrderBy(x => x.Progress).ToList();
            {
                var allEmployee = commonService.GetAllUserFromUsers(token);
                var allTeamEmployees = await commonService.GetTeamEmployees();
                var okrTeamIds = new List<long>();
                var okrTeam = new List<long>();
                var headOrganizations = await commonService.GetLeaderOrganizationsAsync(Constants.ObjFeedbackOnTypeId, token, empId, false);
                if (headOrganizations.Count > 0)
                {
                    var teamIds = headOrganizations.Select(x => x.OrganisationId).Distinct().ToList();
                    if (teamIds.Count > 0)
                    {
                        okrTeam.AddRange(teamIds);
                    }
                }

                ////var okrTeamId = await goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == empId && x.CycleId == cycle && x.KrStatusId != (int)KrStatus.Declined && x.TeamId > 0 && x.IsActive).Select(x => x.TeamId).Distinct().ToListAsync();
                ////if (okrTeamId.Count > 0)
                ////{
                ////    okrTeam.AddRange(okrTeamId);
                ////}
                ////var collaboratorOkr = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycle && x.ImportedId == 0 && x.IsActive && x.Year == year && x.TeamId == identity.OrganisationId && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.TeamId).ToListAsync();
                ////if (collaboratorOkr.Count > 0)
                ////{
                ////    okrTeam.AddRange(collaboratorOkr);
                ////}

                var collaboratorDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == empId);
                if (collaboratorDetails != null && collaboratorDetails.OrganisationID != 0)
                {
                    okrTeam.Add(collaboratorDetails.OrganisationID);
                }

                okrTeamIds = okrTeam.Distinct().ToList();
                ////Due to cache issue we are doing this quick fixes.
                var activeOrganisations = await commonService.GetAllActiveOrganisations();
                foreach (var teamId in okrTeamIds)
                {
                    var allTeamOKrById = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycle && x.TeamId == teamId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public).ToListAsync();
                    var ownerOkr = allTeamOKrById.Where(x => x.EmployeeId == x.Owner).ToList();
                    var teamDetailsById = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == teamId);
                    if (teamDetailsById.OrganisationId != 0)
                    {
                        var leaderDetailResponse = new LeaderDetailsResponse();
                        var teamHeadDetailsById = activeOrganisations.FirstOrDefault(x => x.OrganisationId == teamId);
                        var leaderDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == Convert.ToInt64(teamHeadDetailsById.HeadCode));
                        if (leaderDetails != null)
                        {
                            var searchUserDetails = await commonService.SearchUserAsync(leaderDetails.FirstName, token);
                            var leaderProfileDetails = searchUserDetails.FirstOrDefault(x => x.EmployeeId == leaderDetails.EmployeeId);
                            if (leaderProfileDetails != null)
                            {
                                leaderDetailResponse.CycleDuration = leaderProfileDetails.CycleDuration;
                                leaderDetailResponse.CycleId = leaderProfileDetails.CycleId;
                                leaderDetailResponse.EmployeeId = leaderProfileDetails.EmployeeId;
                                leaderDetailResponse.Designation = leaderProfileDetails.Designation;
                                leaderDetailResponse.Year = leaderProfileDetails.Year;
                                leaderDetailResponse.EmailId = leaderProfileDetails.EmailId;
                                leaderDetailResponse.EmployeeCode = leaderProfileDetails.EmployeeCode;
                                leaderDetailResponse.EndDate = leaderProfileDetails.EndDate;
                                leaderDetailResponse.FirstName = leaderProfileDetails.FirstName;
                                leaderDetailResponse.ImagePath = leaderProfileDetails.ImagePath;
                                leaderDetailResponse.LastName = leaderProfileDetails.LastName;
                                leaderDetailResponse.OrganisationId = leaderProfileDetails.OrganisationId;
                                leaderDetailResponse.OrganisationName = leaderProfileDetails.OrganisationName;
                                leaderDetailResponse.ReportingName = leaderProfileDetails.ReportingName;
                                leaderDetailResponse.ReportingTo = leaderProfileDetails.ReportingTo;
                                leaderDetailResponse.ReportingToDesignation = leaderProfileDetails.ReportingToDesignation;
                                leaderDetailResponse.RoleId = leaderProfileDetails.RoleId;
                                leaderDetailResponse.RoleName = leaderProfileDetails.RoleName;
                                leaderDetailResponse.StartDate = leaderProfileDetails.StartDate;
                            }
                        }

                        var okrStatusDetail = await commonService.GetAllOkrFiltersAsync(teamId, token);
                        var avgScore = ownerOkr.Count > 0 ? ownerOkr.Select(x => x.Score).Average() : 0;
                        var teamProgressId = commonService.GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), avgScore, cycleDurationDetails.CycleDurationId);
                        var okrStatusCode = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == teamProgressId);
                        var teamSequence = await GetTeamSequenceById(empId, cycle);
                        var teamOkrCard = new TeamOkrCardResponse()
                        {
                            TeamId = teamId,
                            TeamName = teamDetailsById.OrganisationName,
                            ColorCode = teamDetailsById.ColorCode,
                            BackGroundColorCode = teamDetailsById.BackGroundColorCode,
                            TeamLogo = teamDetailsById.ImagePath,
                            TeamScore = commonService.KeyScore(avgScore),
                            Progress = teamProgressId,
                            ProgressCode = okrStatusCode == null ? "" : okrStatusCode.Color,
                            KeyCount = ownerOkr.Count,
                            Sequence = teamSequence.Where(x => x.TeamId == teamId).Select(x => x.Sequence).FirstOrDefault(),
                            LeaderDetails = leaderDetailResponse,
                            MembersCount = teamDetailsById.MembersCount
                        };

                        teamOkrCardList.Add(teamOkrCard);
                    }
                }
            }

            return teamOkrCardList.OrderBy(x => x.Progress).ToList();
        }

        public async Task<bool> NudgeTeamAsync(NudgeTeamRequest nudgeTeamRequest, string token, UserIdentity identity)
        {
            var mailSent = false;
            var teamEmployeeIds = await goalKeyRepo.GetQueryable().Where(x => x.CycleId == nudgeTeamRequest.Cycle && x.TeamId == nudgeTeamRequest.TeamId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).Distinct().ToListAsync();
            teamEmployeeIds.Remove(identity.EmployeeId);
            foreach (var teamEmployee in teamEmployeeIds)
            {
                mailSent = true;
                await Task.Run(async () =>
                {
                    await notificationService.NudgeTeamNotifications(token, identity, Convert.ToInt64(teamEmployee)).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }

            return mailSent;
        }

        public async Task<TeamOkrResponse> GetTeamOkrGoalDetailsById(long teamId, long empId, int cycle, int year, string token, UserIdentity identity)
        {
            var teamResponse = new TeamOkrResponse();
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);

            var keyCount = 0;
            var keyNotStartedCount = 0;
            var keyAtRiskCount = 0;
            var keyLaggingCount = 0;
            var keyOnTrackCount = 0;
            var objNotStartedCount = 0;
            var objAtRiskCount = 0;
            var objLaggingCount = 0;
            var objOnTrackCount = 0;
            var alertMessageCount = 0;

            var teamOkrResponses = new List<DashboardOkrKRResponse>();
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var allFeedback = await commonService.GetAllFeedback(token, empId);
            var allTeamEmployees = await commonService.GetTeamEmployees();
            var teamDetailsById = commonService.GetTeamEmployeeByTeamId(teamId, token);
            if (teamDetailsById != null && quarterDetails != null)
            {
                var activeOrganisations = await commonService.GetAllActiveOrganisations();
                var timeDifference = commonService.DeltaProgressLastDay().Date;
                var teamHeadDetailsById = activeOrganisations.FirstOrDefault(x => x.OrganisationId == teamId);
                var leaderDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == Convert.ToInt64(teamHeadDetailsById.HeadCode));
                teamResponse.TeamId = teamDetailsById.OrganisationId;
                teamResponse.TeamName = teamDetailsById.OrganisationName;
                teamResponse.TeamLogo = teamHeadDetailsById.OrgLogo;
                teamResponse.LeaderEmployeeId = leaderDetails == null ? 0 : leaderDetails.EmployeeId;
                teamResponse.LeaderName = leaderDetails == null ? "" : leaderDetails.FirstName + " " + leaderDetails.LastName;
                teamResponse.FirstName = leaderDetails == null ? "N" : leaderDetails.FirstName;
                teamResponse.LastName = leaderDetails == null ? "A" : leaderDetails.LastName;
                teamResponse.ImagePath = leaderDetails == null ? "" : leaderDetails.ImagePath;
                teamResponse.TeamEmployeeCount = teamDetailsById.MembersCount;
                teamResponse.ColorCode = teamDetailsById.ColorCode;
                teamResponse.BackGroundColorCode = teamDetailsById.BackGroundColorCode;

                var allTeamGoalObjective = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycle && x.IsActive && x.Year == year && x.TeamId == teamId && x.GoalStatusId == (int)GoalStatus.Public && x.EmployeeId == leaderDetails.EmployeeId).OrderByDescending(x => x.GoalObjectiveId).ThenBy(x => x.Sequence).ToListAsync();
                var teamOkrAtRisk = new List<GoalObjective>();
                var teamOkrLagging = new List<GoalObjective>();
                var teamOkrOnTrack = new List<GoalObjective>();

                if (allTeamGoalObjective.Count != 0)
                {
                    foreach (var obj in allTeamGoalObjective)
                    {
                        var dashboardKeyResponses = new List<DashboardKeyResponse>();

                        var keyDetail = await GetGoalKeyByTeamId(teamId, obj.GoalObjectiveId);

                        foreach (var key in keyDetail)
                        {
                            if ((key.GoalStatusId == (int)GoalStatus.Draft && key.ImportedId == 0) || (key.GoalStatusId != (int)GoalStatus.Draft && key.ImportedId >= 0))
                            {
                                if (!teamResponse.IsDeltaVisible)
                                {
                                    teamResponse.IsDeltaVisible = key.CreatedOn.Date <= timeDifference.Date;
                                }
                                keyCount += 1;
                                var keyProgressId = commonService.GetProgressIdWithFormula(key.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), key.Score, cycleDurationDetails.CycleDurationId);
                                SeparationKRStatusCount_DashboardAsync(keyProgressId, ref keyNotStartedCount, ref keyAtRiskCount, ref keyLaggingCount, ref keyOnTrackCount);
                                var keyScoreUpdateDetail = commonService.LatestUpdateGoalKey(key.GoalKeyId);

                                var goalUpdateProgressDate = await goalKeyAuditRepo.GetQueryable().Where(x => x.UpdatedGoalKeyId == key.GoalKeyId && x.UpdatedColumn == Constants.Progress).OrderByDescending(x => x.UpdatedOn).ToListAsync();
                                if (goalUpdateProgressDate != null && goalUpdateProgressDate.Count > 0)
                                {
                                    var currentDate = DateTime.UtcNow;
                                    var differenceInDays = (currentDate.Date - goalUpdateProgressDate[0].UpdatedOn.Date).TotalDays;
                                    if (differenceInDays > 7)
                                    {
                                        alertMessageCount += 1;
                                    }
                                }

                                ////var empGoalKey = await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == empId && x.IsActive && x.ImportedId == key.GoalKeyId);

                                dashboardKeyResponses.Add(new DashboardKeyResponse
                                {
                                    GoalKeyId = key.GoalKeyId,
                                    DueDate = key.DueDate,
                                    Progress = keyProgressId,
                                    Score = commonService.KeyScore(key.Score),
                                    KeyProgressTime = keyScoreUpdateDetail == null ? key.CreatedOn : keyScoreUpdateDetail.UpdatedOn,
                                    Source = key.Source,
                                    ImportedType = key.ImportedType,
                                    ImportedId = key.ImportedId,
                                    KeyDescription = key.KeyDescription,
                                    IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId),
                                    Contributors = await commonService.GetAllContributorAsync((int)GoalType.GoalKey, key.GoalKeyId, allEmployee.Results, identity, token),
                                    StartDate = key.StartDate,
                                    MetricId = key.MetricId,
                                    AssignmentTypeId = key.AssignmentTypeId,
                                    CurrencyId = key.CurrencyId,
                                    CurrentValue = key.CurrentValue,
                                    TargetValue = key.TargetValue,
                                    KrStatusId = key.KrStatusId,
                                    CurrencyCode = key.CurrencyCode,
                                    GoalStatusId = key.GoalStatusId,
                                    ContributorValue = key.ContributorValue,
                                    StartValue = key.StartValue,
                                    KeyNotes = key.KeyNotes,
                                    IsLastStatusDraft = key.GoalStatusId == (int)GoalStatus.Archived && key.KrStatusId == (int)KrStatus.Accepted,
                                    TeamId = key.TeamId,
                                    TeamName = teamDetailsById.OrganisationName,
                                    ColorCode = teamDetailsById.ColorCode,
                                    BackGroundColorCode = teamDetailsById.BackGroundColorCode,
                                    Owner = key.Owner,
                                    OwnerDesignation = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.Designation : "",
                                    OwnerEmailId = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.EmailId : "",
                                    OwnerEmployeeCode = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.EmployeeCode : "",
                                    OwnerFirstName = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.FirstName : "",
                                    OwnerImagePath = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.ImagePath : "",
                                    OwnerLastName = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.LastName : "",
                                    IsCollaborator = !commonService.GetGoalKeySource(identity.EmployeeId, key.Source == 0 ? key.GoalKeyId : key.Source).Result.IsAligned, /*!(empGoalKey != null || empId == key.Owner),*/
                                    IsAligned = key.CreatedBy == key.EmployeeId && key.ImportedId > 0,
                                    IsAssigned = key.CreatedBy != key.EmployeeId && key.ImportedId > 0,
                                    TeamMembersCount = teamDetailsById.MembersCount,
                                    TeamLogo = teamHeadDetailsById.OrgLogo,
                                    ParentTeamDetail = key.ImportedId > 0 ? commonService.ParentTeamDetails((int)GoalType.GoalKey, key.ImportedId, allTeamEmployees, key.TeamId) : null
                                });
                            }
                        }

                        var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);

                        if ((keyDetail.Count > 0 && keyDetail.Any(x => x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId != (int)GoalStatus.Draft)) || (obj.GoalStatusId == (int)GoalStatus.Draft && obj.ImportedId == 0))
                        {
                            var objProgressId = commonService.GetProgressIdWithFormula(obj.EndDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), obj.Score, cycleDurationDetails.CycleDurationId);
                            SeparationOkrStatusCount_DashboardAsync(objProgressId, ref objNotStartedCount, ref objAtRiskCount, ref objLaggingCount, ref objOnTrackCount);
                            if (objProgressId == (int)ProgressMaster.AtRisk)
                            {
                                teamOkrAtRisk.Add(obj);
                            }

                            else if (objProgressId == (int)ProgressMaster.Lagging)
                            {
                                teamOkrLagging.Add(obj);
                            }

                            else if (objProgressId == (int)ProgressMaster.OnTrack)
                            {
                                teamOkrOnTrack.Add(obj);
                            }

                            var linkedObjectiveDetails = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == obj.LinkedObjectiveId && x.IsActive);
                            var linkObjTeamName = string.Empty;
                            var linkObjTeamColor = string.Empty;
                            var linkObjLabelColor = string.Empty;
                            if (linkedObjectiveDetails != null && linkedObjectiveDetails.TeamId > 0)
                            {
                                var teamDetails = commonService.GetTeamEmployeeByTeamId(linkedObjectiveDetails.TeamId, token);
                                linkObjTeamName = teamDetails == null ? "" : teamDetails.OrganisationName;
                                linkObjTeamColor = teamDetails == null ? "" : teamDetails.ColorCode;
                                linkObjLabelColor = teamDetails == null ? "" : teamDetails.BackGroundColorCode;
                            }

                            var objSequences = await GetGoalSequence(obj.EmployeeId, cycle);

                            teamOkrResponses.Add(new DashboardOkrKRResponse()
                            {
                                MyGoalsDetails = dashboardKeyResponses,
                                GoalObjectiveId = obj.GoalObjectiveId,
                                Year = obj.Year,
                                IsPrivate = obj.IsPrivate,
                                ObjectiveDescription = obj.ObjectiveDescription,
                                EmployeeId = obj.EmployeeId,
                                FirstName = objUser == null ? "N" : objUser.FirstName,
                                LastName = objUser == null ? "A" : objUser.LastName,
                                ImagePath = objUser?.ImagePath?.Trim(),
                                ObjectiveName = obj.ObjectiveName,
                                StartDate = obj.StartDate,
                                EndDate = obj.EndDate,
                                DueCycle = quarterDetails == null ? "" : quarterDetails.Symbol + "," + year,
                                Progress = objProgressId,
                                GoalProgressTime = dashboardKeyResponses.Count <= 0 ? obj.CreatedOn : dashboardKeyResponses.OrderByDescending(x => x.KeyProgressTime).FirstOrDefault().KeyProgressTime,
                                Score = commonService.KeyScore(obj.Score),
                                DueDate = obj.EndDate,
                                Source = obj.Source,
                                Sequence = objSequences.Where(x => x.GoalType == GoalType.GoalObjective.GetHashCode() && x.GoalId == obj.GoalObjectiveId).OrderByDescending(x => x.SequenceId).Select(x => x.Sequence).FirstOrDefault(),
                                IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == obj.GoalObjectiveId),
                                Contributors = new List<ContributorsResponse>(),
                                GoalStatusId = obj.GoalStatusId,
                                GoalTypeId = obj.GoalTypeId,
                                AssignmentTypeId = (int)AssignmentType.WithParentObjective,
                                KrStatusId = (int)KrStatus.Accepted,
                                IsLastStatusDraft = obj.GoalStatusId == (int)GoalStatus.Archived && dashboardKeyResponses.Any(x => x.IsLastStatusDraft),
                                TeamId = obj.TeamId,
                                TeamName = teamDetailsById.OrganisationName,
                                ColorCode = teamDetailsById.ColorCode,
                                BackGroundColorCode = teamDetailsById.BackGroundColorCode,
                                Owner = obj.Owner,
                                OwnerDesignation = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.Designation : "",
                                OwnerEmailId = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.EmailId : "",
                                OwnerEmployeeCode = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.EmployeeCode : "",
                                OwnerFirstName = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.FirstName : "",
                                OwnerImagePath = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.ImagePath : "",
                                OwnerLastName = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.LastName : "",
                                TeamOkrRequests = new List<TeamOkrRequest>() { new TeamOkrRequest() { TeamId = obj.TeamId } },
                                LinkedObjective = linkedObjectiveDetails == null ? new LinkedObjectiveResponse() : new LinkedObjectiveResponse() { ObjectiveId = linkedObjectiveDetails.GoalObjectiveId, ObjectiveName = linkedObjectiveDetails.ObjectiveName, DueCycle = quarterDetails.Symbol + " - " + year, TeamId = linkedObjectiveDetails.TeamId, TeamName = linkObjTeamName, ColorCode = linkObjTeamColor, BackGroundColorCode = linkObjLabelColor },
                                CreatedBy = obj.CreatedBy,
                                IsAligned = dashboardKeyResponses.Any(x => x.IsAligned),
                                IsAssigned = dashboardKeyResponses.Any(x => x.IsAssigned),
                                ParentTeamDetail = obj.ImportedId > 0 ? commonService.ParentTeamDetails((int)GoalType.GoalObjective, obj.ImportedId, allTeamEmployees, obj.TeamId) : null,
                                TeamMembersCount = teamDetailsById.MembersCount,
                                TeamLogo = teamHeadDetailsById.OrgLogo,
                                CreatedOn = obj.CreatedOn,
                                IsCollaborator = dashboardKeyResponses.Any(x => x.IsCollaborator)
                            });
                        }
                    }
                }

                var atRiskAvgScore = teamOkrAtRisk.Count > 0 ? commonService.KeyScore(teamOkrAtRisk.Select(x => x.Score).Sum() / teamOkrResponses.Count) : 0;
                var laggingAvgScore = teamOkrLagging.Count > 0 ? commonService.KeyScore(teamOkrLagging.Select(x => x.Score).Sum() / teamOkrResponses.Count) : 0;
                var onTrackAvgScore = teamOkrOnTrack.Count > 0 ? commonService.KeyScore(teamOkrOnTrack.Select(x => x.Score).Sum() / teamOkrResponses.Count) : 0;

                var avgScore = teamOkrResponses.Count > 0 ? teamOkrResponses.Select(x => x.Score).Average() : 0;
                teamResponse.TeamAvgScore = commonService.KeyScore(avgScore);
                teamResponse.TeamProgress = commonService.GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), teamResponse.TeamAvgScore, cycleDurationDetails.CycleDurationId);
                var okrStatusDetail = await commonService.GetAllOkrFiltersAsync(teamId, token);
                var okrStatusCode = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == teamResponse.TeamProgress);
                teamResponse.ProgressCode = okrStatusCode == null ? "" : okrStatusCode.Color;

                teamResponse.MyGoalOkrResponses = teamOkrResponses;
                teamResponse.OkrCount = teamOkrResponses.Count;
                teamResponse.KeyCount = keyCount;
                var getProgress = await commonService.GetLastSevenDaysProgress(Constants.ZeroId, teamId, cycle, true, identity, Convert.ToInt64(teamDetailsById.OrganisationHead), false);
                teamResponse.LastSevenDaysProgress = teamResponse.TeamAvgScore - getProgress.Score <= 0 ? 0 : teamResponse.TeamAvgScore - getProgress.Score;
                teamResponse.ContributorsLastSevenDaysProgress = await commonService.GetContributorsLastUpdateSevenDays(Convert.ToInt64(teamDetailsById.OrganisationHead), teamId, cycle, true, allEmployee.Results, identity, Convert.ToInt64(teamDetailsById.OrganisationHead), Convert.ToDateTime(quarterDetails.EndDate));
                var sevenDaysStatusCardProgress = await commonService.GetLastSevenDaysStatusCardProgress(Constants.ZeroId, teamId, cycle, true, quarterDetails, cycleDurationDetails, identity, Convert.ToInt64(teamDetailsById.OrganisationHead));
                if (alertMessageCount > 0)
                {
                    teamResponse.AlertMessage = true;
                }
                teamResponse.NotStarted = new List<ProgressDetail>() { new ProgressDetail()
                {
                    ObjectiveCount = objNotStartedCount,
                    KeyResultCount = keyNotStartedCount,
                    Description = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == (int)ProgressMaster.NotStarted)?.Description
                }};
                teamResponse.AtRisk = new List<ProgressDetail>() { new ProgressDetail()
                {
                    ObjectiveCount = objAtRiskCount,
                    KeyResultCount = keyAtRiskCount,
                    Score = atRiskAvgScore,
                    LastSevenDaysProgress = sevenDaysStatusCardProgress.LastSevenDaysProgressAtRisk,
                    Description = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == (int)ProgressMaster.AtRisk)?.Description
                }};
                teamResponse.Lagging = new List<ProgressDetail>() { new ProgressDetail()
                {
                    ObjectiveCount = objLaggingCount,
                    KeyResultCount = keyLaggingCount,
                    Score = laggingAvgScore,
                    LastSevenDaysProgress = sevenDaysStatusCardProgress.LastSevenDaysProgressLagging,
                    Description = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == (int)ProgressMaster.Lagging)?.Description
                }};
                teamResponse.OnTrack = new List<ProgressDetail>() { new ProgressDetail()
                {
                    ObjectiveCount = objOnTrackCount,
                    KeyResultCount = keyOnTrackCount,
                    Score = onTrackAvgScore,
                    LastSevenDaysProgress = sevenDaysStatusCardProgress.LastSevenDaysProgressOnTrack,
                    Description = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == (int)ProgressMaster.OnTrack)?.Description
                }};
            }

            return teamResponse;
        }

        public async Task<TeamSequence> GetTeamSequence(UpdateTeamSequenceRequest sequenceRequest, long employeeId)
        {
            return await teamSequenceRepo.GetQueryable().FirstOrDefaultAsync(x => x.TeamId == sequenceRequest.TeamId && x.EmployeeId == employeeId && x.CycleId == sequenceRequest.CycleId && x.IsActive);
        }

        public async Task<TeamSequence> UpdateTeamSequence(TeamSequence teamSequence)
        {
            teamSequenceRepo.Update(teamSequence);
            await UnitOfWorkAsync.SaveChangesAsync();
            return teamSequence;
        }

        public async Task<TeamSequence> InsertTeamSequence(TeamSequence teamSequence)
        {
            teamSequenceRepo.Add(teamSequence);
            await UnitOfWorkAsync.SaveChangesAsync();
            return teamSequence;
        }
        public async Task<List<TeamSequence>> GetTeamSequenceById(long empId, int cycleId)
        {
            return await teamSequenceRepo.GetQueryable().Where(x => x.CycleId == cycleId && x.EmployeeId == empId && x.IsActive).ToListAsync();
        }

        public async Task<bool> UpdateTeamOkrCardSequence(List<UpdateTeamSequenceRequest> updateTeamSequenceRequests, UserIdentity userIdentity)
        {
            var result = false;
            foreach (var item in updateTeamSequenceRequests)
            {
                var obj = await GetTeamSequence(item, userIdentity.EmployeeId);
                if (obj != null)
                {
                    obj.Sequence = item.Sequence;
                    obj.UpdatedOn = DateTime.UtcNow;
                    await UpdateTeamSequence(obj);
                    result = true;
                }
                else
                {
                    var teamSequence = new TeamSequence()
                    {
                        CycleId = item.CycleId,
                        TeamId = item.TeamId,
                        EmployeeId = userIdentity.EmployeeId,
                        Sequence = item.Sequence,
                        UpdatedOn = DateTime.UtcNow,
                        IsActive = true
                    };

                    await InsertTeamSequence(teamSequence);
                    result = true;
                }
            }

            return result;
        }

        public async Task<bool> NudgeDirectReportAsync(long empId, string token, UserIdentity identity)
        {
            await Task.Run(async () =>
            {
                await notificationService.NudgeTeamNotifications(token, identity, empId).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return true;
        }

        public async Task<List<DirectReportsResponse>> AllDirectReportsResponseAsync(long empId, List<string> searchTexts, int cycle, int year, string token, UserIdentity identity, string sortBy)
        {
            var directReportsResponseList = new List<DirectReportsResponse>();

            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var okrStatusDetail = await commonService.GetAllOkrFiltersAsync(identity.OrganisationId, token);

            ////var progress7days = constantRepo.GetQueryable().FirstOrDefault(x => x.ConstantName == Constants.progress7days && x.IsActive)?.ConstantValue;
            ////var progress15days = constantRepo.GetQueryable().FirstOrDefault(x => x.ConstantName == Constants.progress15days && x.IsActive)?.ConstantValue;
            ////var progress30days = constantRepo.GetQueryable().FirstOrDefault(x => x.ConstantName == Constants.progress30days && x.IsActive)?.ConstantValue;


            ////int data7days = Convert.ToInt32(progress7days ?? "7");
            ////int data15days = Convert.ToInt32(progress15days ?? "15");
            ////int data30days = Convert.ToInt32(progress30days ?? "30");


            ////var leastprogress7days = DateTime.UtcNow.AddDays(-data7days).Date;
            ////var leastprogress15days = DateTime.UtcNow.AddDays(-data15days).Date;
            ////var leastprogress30days = DateTime.UtcNow.AddDays(-data30days).Date;


            if (quarterDetails != null)
            {
                var timeDifference = commonService.DeltaProgressLastDay().Date;
                var directReportDetails = commonService.GetDirectReportsById(empId, token);
                foreach (var item in directReportDetails)
                {
                    var directResponseDto = Mapper.Map<DirectReportsDetails, DirectReportsResponse>(item);
                    decimal avgScore;
                    var distinctObjectives = new List<GoalObjective>();
                    var objectiveList = new List<long>();
                    var keyResults = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == item.EmployeeId && x.IsActive && x.CycleId == cycle && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).ToList();
                    foreach (var key in keyResults)
                    {
                        if (key.GoalObjectiveId > 0)
                        {
                            var objectives = goalObjectiveRepo.GetQueryable().Where(x => x.GoalObjectiveId == key.GoalObjectiveId && x.EmployeeId == item.EmployeeId && x.IsActive && x.ObjectiveCycleId == cycle && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalObjectiveId).FirstOrDefault();
                            if (objectives > 0)
                            {
                                objectiveList.Add(objectives);
                            }
                        }
                    }

                    if (!directResponseDto.IsDeltaVisible)
                    {
                        directResponseDto.IsDeltaVisible = keyResults.Any(x => x.CreatedOn.Date <= timeDifference.Date);
                    }

                    var distinctObjectivesId = objectiveList.Distinct().ToList();
                    foreach (var objectives in distinctObjectivesId)
                    {
                        var objectivesDetails = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == objectives && x.EmployeeId == item.EmployeeId && x.IsActive && x.ObjectiveCycleId == cycle && x.GoalStatusId == (int)GoalStatus.Public);
                        distinctObjectives.Add(objectivesDetails);
                    }
                    directResponseDto.ObjectivesCount = distinctObjectives.Count;
                    directResponseDto.KeyResultCount = keyResults.Count;
                    directResponseDto.Contributions = GetContributionsOfEmployee(empId, cycle, item.EmployeeId);
                    directResponseDto.ContributorsCount = GetDistinctContributorsCount(item.EmployeeId, cycle);

                    ////var count = goalKeyRepo.GetQueryable().Where(x => x.CycleId == cycle && x.EmployeeId == item.EmployeeId && x.IsActive).Select(x => x.GoalKeyId).ToList();

                    ////directResponseDto.MostProgress = goalKeyHistoryRepo.GetQueryable().Where(x => count.Contains(x.GoalKeyId) && x.CreatedBy == item.EmployeeId).Count();

                    ////directResponseDto.LeastProgress7days = goalKeyHistoryRepo.GetQueryable().Where(x => count.Contains(x.GoalKeyId) && x.CreatedBy == item.EmployeeId && x.CreatedOn.Date <= leastprogress7days.Date).Count();

                    ////directResponseDto.LeastProgress15days = goalKeyHistoryRepo.GetQueryable().Where(x => count.Contains(x.GoalKeyId) && x.CreatedBy == item.EmployeeId && x.CreatedOn.Date <= leastprogress15days.Date).Count();

                    ////directResponseDto.LeastProgress30days = goalKeyHistoryRepo.GetQueryable().Where(x => count.Contains(x.GoalKeyId) && x.CreatedBy == item.EmployeeId && x.CreatedOn.Date <= leastprogress30days.Date).Count();


                    var contributionKeyResults = GetContributionsOkrOfEmployee(empId, cycle, item.EmployeeId);
                    var contributionObjectives = new List<DirectReportsObjectives>();

                    foreach (var key in contributionKeyResults)
                    {
                        var contKeysList = new List<DirectReportsKeyResult>();
                        key.Progress = commonService.GetProgressIdWithFormula(key.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), key.Score, cycleDurationDetails.CycleDurationId);
                        var contKeysDto = Mapper.Map<GoalKey, DirectReportsKeyResult>(key);
                        contKeysList.Add(contKeysDto);

                        if (key.GoalObjectiveId > 0)
                        {
                            var contObjectives = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == key.GoalObjectiveId);

                            if (contObjectives != null)
                            {
                                if (contributionObjectives.Any(x => x.GoalObjectiveId == contObjectives.GoalObjectiveId))
                                {
                                    contributionObjectives.FirstOrDefault(x => x.GoalObjectiveId == contObjectives.GoalObjectiveId)?.DirectReportsKeyResults?.Add(contKeysDto);

                                }
                                else
                                {
                                    contObjectives.Progress = commonService.GetProgressIdWithFormula(contObjectives.EndDate, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), contObjectives.Score, cycleDurationDetails.CycleDurationId);

                                    var contObjectivesDto = Mapper.Map<GoalObjective, DirectReportsObjectives>(contObjectives);
                                    contObjectivesDto.DirectReportsKeyResults = contKeysList;
                                    contObjectivesDto.ObjectiveType = (int)GoalType.GoalObjective;
                                    contributionObjectives.Add(contObjectivesDto);
                                }
                            }

                        }
                        else
                        {
                            var contStandAloneKeyDto = Mapper.Map<GoalKey, DirectReportsObjectives>(key);
                            contStandAloneKeyDto.ObjectiveType = (int)GoalType.GoalKey;
                            contributionObjectives.Add(contStandAloneKeyDto);

                        }
                    }

                    directResponseDto.DirectReportsObjectives = contributionObjectives;

                    var standaloneKey = keyResults.Where(x => x.GoalObjectiveId == 0).ToList();
                    if (standaloneKey.Count > 0 && distinctObjectives.Count > 0)
                    {
                        var totalGoalCount = distinctObjectives.Count + standaloneKey.Count;
                        var standaloneKeyAvgScore = standaloneKey.Count > 0 ? standaloneKey.Select(x => x.Score).Sum() : 0;
                        var okrAvgScore = distinctObjectives.Count > 0 ? distinctObjectives.Select(x => x.Score).Sum() : 0;
                        avgScore = totalGoalCount > 0 ? commonService.KeyScore((standaloneKeyAvgScore + okrAvgScore) / totalGoalCount) : 0;
                    }
                    else if (standaloneKey.Count > 0)
                    {
                        avgScore = standaloneKey.Count > 0 ? standaloneKey.Select(x => x.Score).Average() : 0;
                    }
                    else
                    {
                        avgScore = distinctObjectives.Count > 0 ? distinctObjectives.Select(x => x.Score).Average() : 0;
                    }

                    directResponseDto.Score = commonService.KeyScore(avgScore);
                    directResponseDto.Progress = commonService.GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), directResponseDto.Score, cycleDurationDetails.CycleDurationId);
                    var okrStatusCode = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == directResponseDto.Progress);
                    directResponseDto.DirectReportColorCode = okrStatusCode == null ? "" : okrStatusCode.Color;

                    var allTeamOKrById = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycle && x.TeamId == item.OrganisationId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public).ToListAsync();
                    var ownerOkr = allTeamOKrById.Where(x => x.EmployeeId == x.Owner).ToList();
                    var teamScore = ownerOkr.Count > 0 ? ownerOkr.Select(x => x.Score).Average() : 0;

                    directResponseDto.TeamScore = commonService.KeyScore(teamScore);
                    directResponseDto.TeamProgress = commonService.GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), directResponseDto.TeamScore, cycleDurationDetails.CycleDurationId);
                    var okrTeamStatusCode = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == directResponseDto.TeamProgress);
                    directResponseDto.TeamColorCode = okrTeamStatusCode == null ? "" : okrTeamStatusCode.Color;

                    var getProgress = await commonService.GetLastSevenDaysProgress(item.EmployeeId, Constants.ZeroId, cycle, false, identity, Constants.ZeroId, false);
                    directResponseDto.LastSevenDaysProgress = directResponseDto.Score - getProgress.Score <= 0 ? 0 : directResponseDto.Score - getProgress.Score;

                    directResponseDto.ContributorsLastSevenDaysProgress = await commonService.GetContributorsLastUpdateSevenDays(item.EmployeeId, Constants.ZeroId, cycle, false, allEmployee.Results, identity, Constants.ZeroId, Convert.ToDateTime(quarterDetails.EndDate));

                    directReportsResponseList.Add(directResponseDto);
                }

                var index = Constants.Zero;

                var employeeDetail = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == empId);
                if (employeeDetail != null)
                {
                    var loginUserDetails = Mapper.Map<UserResponse, DirectReportsResponse>(employeeDetail);
                    directReportsResponseList?.Insert(index, loginUserDetails);
                }
            }

            List<DirectReportsResponse> finalResponse = new List<DirectReportsResponse>();

            if (searchTexts.Count > 0)
            {
                foreach (var search in searchTexts)
                {
                    if (search.ToLower() == Constants.AtRisk)
                    {
                        finalResponse.AddRange(directReportsResponseList.Where(x => x.Progress == (int)ProgressMaster.AtRisk));
                    }
                    else if (search.ToLower() == Constants.Lagging)
                    {
                        finalResponse.AddRange(directReportsResponseList.Where(x => x.Progress == (int)ProgressMaster.Lagging));
                    }
                    else if (search.ToLower() == Constants.OnTrack)
                    {
                        finalResponse.AddRange(directReportsResponseList.Where(x => x.Progress == (int)ProgressMaster.OnTrack));
                    }
                    else if (search.ToLower() == Constants.NotStarted)
                    {
                        finalResponse.AddRange(directReportsResponseList.Where(x => x.Progress == (int)ProgressMaster.NotStarted));
                    }

                }
                finalResponse.AddRange(directReportsResponseList.Where(x => x.EmployeeId == empId));
            }
            else
            {
                finalResponse = directReportsResponseList;
            }


            if (sortBy != null)
            {
                if (sortBy.ToLower() == Constants.MostContribution)
                {
                    finalResponse = finalResponse.OrderByDescending(x => x.Contributions).ThenBy(x => x.FirstName).ToList();
                }

                else if (sortBy.ToLower() == Constants.LeastContribution)
                {
                    finalResponse = finalResponse.OrderBy(x => x.Contributions).ThenBy(x => x.FirstName).ToList();
                }

                else if (sortBy.ToLower() == Constants.MostProgress)
                {
                    finalResponse = finalResponse.OrderByDescending(x => x.Progress).ThenBy(x => x.FirstName).ToList();
                }

                else if (sortBy.ToLower() == Constants.LeastProgress)
                {
                    finalResponse = finalResponse.OrderBy(x => x.Progress).ThenBy(x => x.FirstName).ToList();

                }

            }
            else
            {
                finalResponse = finalResponse.OrderBy(x => x.Progress).ThenBy(x => x.FirstName).ToList();
            }
            return finalResponse;
        }

        public async Task<AllOkrDashboardResponse> AllOkrDashboardAsync(long empId, int cycle, int year, string token, UserIdentity identity)
        {
            var dashboardResponse = new AllOkrDashboardResponse();
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            if (quarterDetails == null) return dashboardResponse;
            {
                var lockDate = await commonService.IsOkrLocked(Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), empId, cycle, year, token);
                var unLockRequest = new UnLockRequest() { EmployeeId = empId, Cycle = cycle, Year = year };

                dashboardResponse.IsLocked = lockDate.IsGaolLocked;
                dashboardResponse.IsScoreLocked = lockDate.IsScoreLocked;
                dashboardResponse.IsUnLockRequested = await myGoalsService.IsAlreadyRequestedAsync(unLockRequest);
                dashboardResponse.GoalSubmitDate = lockDate.GoalSubmitDate;

                var keyCount = 0;
                var keyNotStartedCount = 0;
                var keyAtRiskCount = 0;
                var keyLaggingCount = 0;
                var keyOnTrackCount = 0;
                var objNotStartedCount = 0;
                var objAtRiskCount = 0;
                var objLaggingCount = 0;
                var objOnTrackCount = 0;
                var publicOkrNotStarted = 0;
                var dashboardOkrResponses = new List<AllOkrDashboardOkrKRResponse>();
                var allEmployee = commonService.GetAllUserFromUsers(token);
                var allFeedback = await commonService.GetAllFeedback(token, empId);
                var objectives = GetEmployeeOkrByCycleId(empId, cycle, year).Result.Where(x => x.GoalStatusId != (int)GoalStatus.Archived).ToList();
                var objSequences = await GetGoalSequence(empId, cycle);
                var okrStatusDetail = await commonService.GetAllOkrFiltersAsync(identity.OrganisationId, token);
                var allTeamEmployees = await commonService.GetTeamEmployees();
                List<AllOkrDashboardOkrKRResponse> okrAtRisk;
                List<AllOkrDashboardOkrKRResponse> okrLagging;
                List<AllOkrDashboardOkrKRResponse> okrOnTrack;
                var publicOkr = new List<AllOkrDashboardOkrKRResponse>();
                var atRiskAvgScore = 0.0M;
                var laggingAvgScore = 0.0M;
                var onTrackAvgScore = 0.0M;

                var random = new Random();
                var messageList = await GetAllMessage();
                var index = random.Next(messageList.Count);
                dashboardResponse.WelcomeMessage = messageList[index];
                var timeDifference = commonService.DeltaProgressLastDay().Date;
                if (objectives.Count != 0)
                {
                    foreach (var obj in objectives)
                    {
                        var dashboardKeyResponses = new List<AllOkrDashboardKeyResponse>();

                        var keyDetail = GetGoalKey(obj.GoalObjectiveId).Result.Where(x => x.GoalStatusId != (int)GoalStatus.Archived).ToList();
                        foreach (var key in keyDetail)
                        {
                            if ((key.GoalStatusId == (int)GoalStatus.Draft && key.ImportedId == 0) || (key.GoalStatusId == (int)GoalStatus.Public && key.ImportedId >= 0))
                            {
                                if (!dashboardResponse.IsDeltaVisible)
                                {
                                    dashboardResponse.IsDeltaVisible = key.CreatedOn.Date <= timeDifference.Date;
                                }

                                var teamDetails = new TeamDetails();
                                if (key.TeamId > 0)
                                {
                                    teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == key.TeamId);
                                }

                                var keyProgressId = commonService.GetProgressIdWithFormula(key.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), key.Score, cycleDurationDetails.CycleDurationId);
                                SeparationKRStatusCount_DashboardAsync(keyProgressId, ref keyNotStartedCount, ref keyAtRiskCount, ref keyLaggingCount, ref keyOnTrackCount);
                                var keyScoreUpdateDetail = commonService.LatestUpdateGoalKey(key.GoalKeyId);
                                dashboardKeyResponses.Add(new AllOkrDashboardKeyResponse
                                {
                                    GoalKeyId = key.GoalKeyId,
                                    DueDate = key.DueDate,
                                    Progress = keyProgressId,
                                    Score = commonService.KeyScore(key.Score),
                                    KeyProgressTime = keyScoreUpdateDetail == null ? key.CreatedOn : keyScoreUpdateDetail.UpdatedOn,
                                    Source = key.Source,
                                    ImportedType = key.ImportedType,
                                    ImportedId = key.ImportedId,
                                    KeyDescription = key.KeyDescription,
                                    IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId),
                                    Contributors = await commonService.GetAllContributorAsync((int)GoalType.GoalKey, key.GoalKeyId, allEmployee.Results, identity, token),
                                    StartDate = key.StartDate,
                                    MetricId = key.MetricId,
                                    AssignmentTypeId = key.AssignmentTypeId,
                                    CurrencyId = key.CurrencyId,
                                    CurrentValue = key.CurrentValue,
                                    TargetValue = key.TargetValue,
                                    KrStatusId = key.KrStatusId,
                                    CurrencyCode = key.CurrencyCode,
                                    GoalStatusId = key.GoalStatusId,
                                    ContributorValue = key.ContributorValue,
                                    StartValue = key.StartValue,
                                    KeyNotes = key.KeyNotes,
                                    IsLastStatusDraft = key.GoalStatusId == (int)GoalStatus.Archived && key.KrStatusId == (int)KrStatus.Accepted,
                                    TeamId = key.TeamId,
                                    TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                                    ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                                    BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,
                                    Owner = key.Owner,
                                    OwnerDesignation = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.Designation : "",
                                    OwnerEmailId = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.EmailId : "",
                                    OwnerEmployeeCode = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.EmployeeCode : "",
                                    OwnerFirstName = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.FirstName : "",
                                    OwnerImagePath = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.ImagePath : "",
                                    OwnerLastName = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.LastName : "",
                                    IsAligned = key.CreatedBy == key.EmployeeId && key.ImportedId > 0,
                                    IsAssigned = key.CreatedBy != key.EmployeeId && key.ImportedId > 0,
                                    TeamMembersCount = teamDetails == null ? 0 : teamDetails.MembersCount,
                                    TeamLogo = teamDetails == null ? "" : teamDetails.ImagePath,
                                    ParentTeamDetail = key.ImportedId > 0 ? commonService.ParentTeamDetails((int)GoalType.GoalKey, key.ImportedId, allTeamEmployees, key.TeamId) : null
                                });
                            }
                        }

                        keyCount += dashboardKeyResponses.Count(x => x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted);

                        if ((keyDetail.Count > 0 && keyDetail.Any(x => x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId == (int)GoalStatus.Public)) || (obj.GoalStatusId == (int)GoalStatus.Draft && obj.ImportedId == 0))
                        {
                            var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);

                            var teamDetails = new TeamDetails();
                            if (obj.TeamId > 0)
                            {
                                teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == obj.TeamId); 
                            }

                            var objProgressId = commonService.GetProgressIdWithFormula(obj.EndDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), obj.Score, cycleDurationDetails.CycleDurationId);
                            SeparationOkrStatusCount_DashboardAsync(objProgressId, ref objNotStartedCount, ref objAtRiskCount, ref objLaggingCount, ref objOnTrackCount);

                            var linkedObjectiveDetails = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == obj.LinkedObjectiveId && x.IsActive);
                            var linkObjTeamName = string.Empty;
                            var linkObjTeamColor = string.Empty;
                            var linkObjLabelColor = string.Empty;
                            if (linkedObjectiveDetails != null && linkedObjectiveDetails.TeamId > 0)
                            {
                                var linkObjTeamDetail = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == linkedObjectiveDetails.TeamId);
                                linkObjTeamName = linkObjTeamDetail == null ? "" : linkObjTeamDetail.OrganisationName;
                                linkObjTeamColor = linkObjTeamDetail == null ? "" : linkObjTeamDetail.ColorCode;
                                linkObjLabelColor = linkObjTeamDetail == null ? "" : linkObjTeamDetail.BackGroundColorCode;
                            }

                            var virtualAlignment = await goalObjectiveRepo.GetQueryable().Where(x => x.LinkedObjectiveId == obj.GoalObjectiveId && x.IsActive).ToListAsync();
                            var parentObj = new GoalObjective();
                            if(obj.ImportedId > 0)
                            {
                                parentObj = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == obj.ImportedId && x.IsActive);
                            }
                            dashboardOkrResponses.Add(new AllOkrDashboardOkrKRResponse()
                            {
                                MyGoalsDetails = dashboardKeyResponses,
                                GoalObjectiveId = obj.GoalObjectiveId,
                                Year = obj.Year,
                                IsPrivate = obj.IsPrivate,
                                ObjectiveDescription = obj.ObjectiveDescription,
                                EmployeeId = obj.EmployeeId,
                                FirstName = objUser == null ? "N" : objUser.FirstName,
                                LastName = objUser == null ? "A" : objUser.LastName,
                                ImagePath = objUser?.ImagePath?.Trim(),
                                ObjectiveName = obj.ObjectiveName,
                                StartDate = obj.StartDate,
                                EndDate = obj.EndDate,
                                DueCycle = quarterDetails.Symbol + "," + year,
                                Progress = objProgressId,
                                GoalProgressTime = dashboardKeyResponses.Count <= 0 ? obj.CreatedOn : dashboardKeyResponses.OrderByDescending(x => x.KeyProgressTime).FirstOrDefault().KeyProgressTime,
                                Score = commonService.KeyScore(obj.Score),
                                DueDate = obj.EndDate,
                                Source = obj.Source,
                                Sequence = objSequences.Where(x => x.GoalType == GoalType.GoalObjective.GetHashCode() && x.GoalId == obj.GoalObjectiveId).OrderByDescending(x => x.SequenceId).Select(x => x.Sequence).FirstOrDefault(),
                                IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == obj.GoalObjectiveId),
                                Contributors = commonService.GetDistinctObjContributor(allEmployee.Results, empId, dashboardKeyResponses, obj.IsCoachCreation, obj.Owner),
                                GoalStatusId = obj.GoalStatusId,
                                GoalTypeId = obj.GoalTypeId,
                                AssignmentTypeId = (int)AssignmentType.WithParentObjective,
                                KrStatusId = (int)KrStatus.Accepted,
                                IsLastStatusDraft = obj.GoalStatusId == (int)GoalStatus.Archived && (dashboardKeyResponses.Count == 0 || dashboardKeyResponses.Any(x => x.IsLastStatusDraft)),
                                TeamId = obj.TeamId,
                                TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                                ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                                BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,
                                Owner = obj.Owner,
                                OwnerDesignation = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.Designation : "",
                                OwnerEmailId = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.EmailId : "",
                                OwnerEmployeeCode = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.EmployeeCode : "",
                                OwnerFirstName = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.FirstName : "",
                                OwnerImagePath = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.ImagePath : "",
                                OwnerLastName = obj.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.Owner)?.LastName : "",
                                TeamOkrRequests = new List<TeamOkrRequest>() { new TeamOkrRequest() { TeamId = obj.TeamId } },
                                LinkedObjective = linkedObjectiveDetails == null ? new LinkedObjectiveResponse() : new LinkedObjectiveResponse() { ObjectiveId = linkedObjectiveDetails.GoalObjectiveId, ObjectiveName = linkedObjectiveDetails.ObjectiveName, DueCycle = quarterDetails.Symbol + " - " + year, TeamId = linkedObjectiveDetails.TeamId, TeamName = linkObjTeamName, ColorCode = linkObjTeamColor, BackGroundColorCode = linkObjLabelColor },
                                CreatedBy = obj.CreatedBy,
                                IsAligned = dashboardKeyResponses.Any(x => x.IsAligned),
                                IsAssigned = dashboardKeyResponses.Any(x => x.IsAssigned),
                                ImportedId = obj.ImportedId,
                                ImportedType = obj.ImportedType,
                                IsCoach = obj.IsCoachCreation,
                                IsVirtualAlignment = virtualAlignment.Count > 0,
                                VirtualAlignmentCount = virtualAlignment.Count,
                                TeamMembersCount = teamDetails == null ? 0 : teamDetails.MembersCount,
                                TeamLogo = teamDetails == null ? "" : teamDetails.ImagePath,
                                ParentTeamDetail = obj.ImportedId > 0 ? commonService.ParentTeamDetails((int)GoalType.GoalObjective, obj.ImportedId, allTeamEmployees, obj.TeamId) : null,
                                ParentStartDate = parentObj?.StartDate,
                                ParentDueDate = parentObj?.EndDate
                            });
                        }
                    }

                    publicOkr = dashboardOkrResponses.Where(x => x.MyGoalsDetails.Any(y => y.GoalStatusId == (int)GoalStatus.Public && y.KrStatusId == (int)KrStatus.Accepted)).ToList();

                    dashboardResponse.AvgScore = publicOkr.Count > 0 ? commonService.KeyScore(publicOkr.Select(x => x.Score).Average()) : 0;
                    var progress = commonService.GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), dashboardResponse.AvgScore, cycleDurationDetails.CycleDurationId);
                    var okrStatusCode = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == progress);
                    dashboardResponse.ProgressCode = okrStatusCode == null ? "" : okrStatusCode.Color;
                    dashboardResponse.MyGoalOkrResponses = dashboardOkrResponses;
                    dashboardResponse.OkrCount = publicOkr.Count;

                    okrAtRisk = publicOkr.Where(x => x.Progress == (int)ProgressMaster.AtRisk).ToList();
                    okrLagging = publicOkr.Where(x => x.Progress == (int)ProgressMaster.Lagging).ToList();
                    okrOnTrack = publicOkr.Where(x => x.Progress == (int)ProgressMaster.OnTrack).ToList();

                    atRiskAvgScore = okrAtRisk.Count > 0 ? commonService.KeyScore(okrAtRisk.Select(x => x.Score).Sum() / publicOkr.Count) : 0;
                    laggingAvgScore = okrLagging.Count > 0 ? commonService.KeyScore(okrLagging.Select(x => x.Score).Sum() / publicOkr.Count) : 0;
                    onTrackAvgScore = okrOnTrack.Count > 0 ? commonService.KeyScore(okrOnTrack.Select(x => x.Score).Sum() / publicOkr.Count) : 0;

                    publicOkrNotStarted = publicOkr.Count(x => x.Progress == (int)ProgressMaster.NotStarted);
                }

                var orphanKrDetails = GetOrphanKey(empId, cycle).Result.Where(x => x.GoalStatusId != (int)GoalStatus.Archived).ToList();
                var dashboardOrphanKeyResponse = new List<AllOkrDashboardOkrKRResponse>();
                foreach (var orphanKey in orphanKrDetails)
                {
                    var sourceKeyDetails = await GetGoalKeyById(orphanKey.ImportedId);
                    var sourceUserDetails = sourceKeyDetails != null ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId) : null;
                    var keyScoreUpdateDetail = commonService.LatestUpdateGoalKey(orphanKey.GoalKeyId);

                    if ((orphanKey.GoalStatusId == (int)GoalStatus.Public) || (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
                    {
                        if (!dashboardResponse.IsDeltaVisible)
                        {
                            dashboardResponse.IsDeltaVisible = orphanKey.CreatedOn.Date <= timeDifference.Date;
                        }

                        var teamDetails = new TeamDetails();
                        if (orphanKey.TeamId > 0)
                        {
                            teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == orphanKey.TeamId);
                        }
                        var keyProgressId = commonService.GetProgressIdWithFormula(orphanKey.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), orphanKey.Score, cycleDurationDetails.CycleDurationId);
                        SeparationOkrStatusCount_DashboardAsync(keyProgressId, ref objNotStartedCount, ref objAtRiskCount, ref objLaggingCount, ref objOnTrackCount);                   
                        var parentObj = await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalKeyId == orphanKey.ImportedId && x.IsActive);
                        dashboardOrphanKeyResponse.Add(new AllOkrDashboardOkrKRResponse
                        {
                            GoalKeyId = orphanKey.GoalKeyId,
                            DueDate = orphanKey.DueDate,
                            Progress = keyProgressId,
                            EmployeeId = Convert.ToInt64(orphanKey.EmployeeId),
                            Score = commonService.KeyScore(orphanKey.Score),
                            KeyProgressTime = keyScoreUpdateDetail == null ? orphanKey.CreatedOn : keyScoreUpdateDetail.UpdatedOn,
                            Source = orphanKey.Source,
                            ImportedType = orphanKey.ImportedType,
                            ImportedId = orphanKey.ImportedId,
                            KeyDescription = orphanKey.KeyDescription,
                            IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == orphanKey.GoalKeyId),
                            Contributors = await commonService.GetAllContributorAsync((int)GoalType.GoalKey, orphanKey.GoalKeyId, allEmployee.Results, identity, token),
                            StartDate = orphanKey.StartDate,
                            MetricId = orphanKey.MetricId,
                            AssignmentTypeId = orphanKey.AssignmentTypeId,
                            CurrencyId = orphanKey.CurrencyId,
                            CurrentValue = orphanKey.CurrentValue,
                            TargetValue = orphanKey.TargetValue,
                            KrStatusId = orphanKey.KrStatusId,
                            CurrencyCode = orphanKey.CurrencyCode,
                            GoalStatusId = orphanKey.GoalStatusId,
                            ContributorValue = orphanKey.ContributorValue,
                            FirstName = sourceUserDetails == null ? "N" : sourceUserDetails.FirstName,
                            LastName = sourceUserDetails == null ? "A" : sourceUserDetails.LastName,
                            ImagePath = sourceUserDetails?.ImagePath?.Trim(),
                            Sequence = objSequences.Where(x => x.GoalType == GoalType.GoalKey.GetHashCode() && x.GoalId == orphanKey.GoalKeyId).OrderByDescending(x => x.SequenceId).Select(x => x.Sequence).FirstOrDefault(),
                            StartValue = orphanKey.StartValue,
                            KeyNotes = orphanKey.KeyNotes,
                            DueCycle = quarterDetails.Symbol + "," + year,
                            TeamId = orphanKey.TeamId,
                            TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                            ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                            BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,
                            Owner = orphanKey.Owner,
                            OwnerDesignation = orphanKey.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == orphanKey.Owner)?.Designation : "",
                            OwnerEmailId = orphanKey.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == orphanKey.Owner)?.EmailId : "",
                            OwnerEmployeeCode = orphanKey.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == orphanKey.Owner)?.EmployeeCode : "",
                            OwnerFirstName = orphanKey.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == orphanKey.Owner)?.FirstName : "",
                            OwnerImagePath = orphanKey.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == orphanKey.Owner)?.ImagePath : "",
                            OwnerLastName = orphanKey.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == orphanKey.Owner)?.LastName : "",
                            TeamOkrRequests = new List<TeamOkrRequest>() { new TeamOkrRequest() { TeamId = orphanKey.TeamId } },
                            CreatedBy = orphanKey.CreatedBy,
                            IsAligned = orphanKey.CreatedBy == orphanKey.EmployeeId && orphanKey.ImportedId > 0,
                            IsAssigned = orphanKey.CreatedBy != orphanKey.EmployeeId && orphanKey.ImportedId > 0,
                            TeamMembersCount = teamDetails == null ? 0 : teamDetails.MembersCount,
                            TeamLogo = teamDetails == null ? "" : teamDetails.ImagePath,
                            ParentStartDate = parentObj?.StartDate,
                            ParentDueDate = parentObj?.DueDate
                        });
                    }
                }

                if (dashboardOrphanKeyResponse.Count != 0)
                {
                    keyCount += dashboardOrphanKeyResponse.Count(x => x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted);
                    publicOkrNotStarted += dashboardOrphanKeyResponse.Count(x => x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted && x.Progress == (int)ProgressMaster.NotStarted);

                    var publicOrphanKr = dashboardOrphanKeyResponse.Where(x => x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                    var publicOrphanKrAvgScore = publicOrphanKr.Count > 0 ? publicOrphanKr.Select(x => x.Score).Sum() : 0;
                    var publicOkrAvgScore = publicOkr.Count > 0 ? publicOkr.Select(x => x.Score).Sum() : 0;
                    if (dashboardResponse.MyGoalOkrResponses != null)
                    {
                        dashboardResponse.MyGoalOkrResponses.AddRange(dashboardOrphanKeyResponse);
                        var totalGoalCount = publicOkr.Count + publicOrphanKr.Count;
                        dashboardResponse.AvgScore = totalGoalCount > 0 ? commonService.KeyScore((publicOkrAvgScore + publicOrphanKrAvgScore) / totalGoalCount) : 0;

                        var progress = commonService.GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), dashboardResponse.AvgScore, cycleDurationDetails.CycleDurationId);
                        var okrStatusCode = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == progress);
                        dashboardResponse.ProgressCode = okrStatusCode == null ? "" : okrStatusCode.Color;

                        publicOkr.AddRange(publicOrphanKr);

                        okrAtRisk = publicOkr.Where(x => x.Progress == (int)ProgressMaster.AtRisk).ToList();
                        okrLagging = publicOkr.Where(x => x.Progress == (int)ProgressMaster.Lagging).ToList();
                        okrOnTrack = publicOkr.Where(x => x.Progress == (int)ProgressMaster.OnTrack).ToList();

                        atRiskAvgScore = okrAtRisk.Count > 0 ? commonService.KeyScore(okrAtRisk.Select(x => x.Score).Sum() / totalGoalCount) : 0;
                        laggingAvgScore = okrLagging.Count > 0 ? commonService.KeyScore(okrLagging.Select(x => x.Score).Sum() / totalGoalCount) : 0;
                        onTrackAvgScore = okrOnTrack.Count > 0 ? commonService.KeyScore(okrOnTrack.Select(x => x.Score).Sum() / totalGoalCount) : 0;
                    }
                    else
                    {
                        dashboardResponse.MyGoalOkrResponses = dashboardOrphanKeyResponse;
                        dashboardResponse.AvgScore = publicOrphanKr.Count > 0 ? commonService.KeyScore(publicOrphanKr.Select(x => x.Score).Average()) : 0;
                        var progress = commonService.GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), dashboardResponse.AvgScore, cycleDurationDetails.CycleDurationId);
                        var okrStatusCode = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == progress);
                        dashboardResponse.ProgressCode = okrStatusCode == null ? "" : okrStatusCode.Color;

                        okrAtRisk = publicOrphanKr.Where(x => x.Progress == (int)ProgressMaster.AtRisk).ToList();
                        okrLagging = publicOrphanKr.Where(x => x.Progress == (int)ProgressMaster.Lagging).ToList();
                        okrOnTrack = publicOrphanKr.Where(x => x.Progress == (int)ProgressMaster.OnTrack).ToList();

                        atRiskAvgScore = okrAtRisk.Count > 0 ? commonService.KeyScore(okrAtRisk.Select(x => x.Score).Sum() / publicOrphanKr.Count) : 0;
                        laggingAvgScore = okrLagging.Count > 0 ? commonService.KeyScore(okrLagging.Select(x => x.Score).Sum() / publicOrphanKr.Count) : 0;
                        onTrackAvgScore = okrOnTrack.Count > 0 ? commonService.KeyScore(okrOnTrack.Select(x => x.Score).Sum() / publicOrphanKr.Count) : 0;
                    }
                }

                dashboardResponse.KeyCount = keyCount;
                dashboardResponse.ContributorsCount = GetDistinctContributorsCount(empId, cycle);
                dashboardResponse.IsFirstTimeUser = await IsFirstTimeUserAsync(empId);

                dashboardResponse.NotStarted = new List<AllOkrDashboardProgressDetail>() { new AllOkrDashboardProgressDetail()
                {
                    ObjectiveCount = publicOkrNotStarted,
                    KeyResultCount = keyNotStartedCount,
                    Description = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == (int)ProgressMaster.NotStarted)?.Description
                } };
                dashboardResponse.AtRisk = new List<AllOkrDashboardProgressDetail>() { new AllOkrDashboardProgressDetail()
                {
                    ObjectiveCount = objAtRiskCount,
                    KeyResultCount = keyAtRiskCount,
                    Score = atRiskAvgScore,
                    Description = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == (int)ProgressMaster.AtRisk)?.Description
                } };
                dashboardResponse.Lagging = new List<AllOkrDashboardProgressDetail>() { new AllOkrDashboardProgressDetail()
                {
                    ObjectiveCount = objLaggingCount,
                    KeyResultCount = keyLaggingCount,
                    Score = laggingAvgScore,
                    Description = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == (int)ProgressMaster.Lagging)?.Description
                } };
                dashboardResponse.OnTrack = new List<AllOkrDashboardProgressDetail>() { new AllOkrDashboardProgressDetail()
                {
                    ObjectiveCount = objOnTrackCount,
                    KeyResultCount = keyOnTrackCount,
                    Score = onTrackAvgScore,
                    Description = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == (int)ProgressMaster.OnTrack)?.Description
                } };

                var lastSequence = 0;
                var okrSequence = dashboardResponse.MyGoalOkrResponses?.Where(x => x.GoalStatusId != (int)GoalStatus.Archived).ToList();
                if (okrSequence != null)
                {
                    lastSequence = okrSequence.Count > 0 ? okrSequence.Last().Sequence : 0;
                }

                if (lastSequence > 0)
                {
                    dashboardResponse.MyGoalOkrResponses = dashboardResponse.MyGoalOkrResponses?.OrderBy(x => x.Sequence).ThenByDescending(x => x.GoalObjectiveId).ToList();
                }
                else
                {
                    var pendingStandaloneKr = dashboardResponse.MyGoalOkrResponses?.Where(x => x.KrStatusId == (int)KrStatus.Pending && x.GoalStatusId == (int)GoalStatus.Public && x.GoalObjectiveId == 0 && x.AssignmentTypeId == (int)AssignmentType.StandAlone && x.ImportedId > 0).ToList();
                    var pendingWithParentKr = dashboardResponse.MyGoalOkrResponses?.Where(x => x.GoalObjectiveId > 0 && x.MyGoalsDetails.Any(y => y.KrStatusId == (int)KrStatus.Pending && y.GoalStatusId == (int)GoalStatus.Public && (y.AssignmentTypeId == (int)AssignmentType.WithParentObjective || y.ImportedId > 0)) && x.GoalStatusId == (int)GoalStatus.Public).ToList();
                    var draftKr = dashboardResponse.MyGoalOkrResponses?.Where(x => x.GoalStatusId == (int)GoalStatus.Draft).ToList();
                    var newlyCreated = dashboardResponse.MyGoalOkrResponses?.Where(x => x.GoalStatusId == (int)GoalStatus.Public && x.ImportedId == 0 && x.MyGoalsDetails.All(y => y.ImportedId == 0)).OrderByDescending(x => x.GoalObjectiveId).ToList();
                    var acceptedStandaloneKr = dashboardResponse.MyGoalOkrResponses?.Where(x => x.KrStatusId == (int)KrStatus.Accepted && x.AssignmentTypeId == (int)AssignmentType.StandAlone && x.GoalStatusId == (int)GoalStatus.Public && x.ImportedId > 0).ToList();
                    var acceptedWithParentKr = dashboardResponse.MyGoalOkrResponses?.Where(x => x.GoalObjectiveId > 0 && x.MyGoalsDetails.Any(y => y.KrStatusId == (int)KrStatus.Accepted && y.GoalStatusId == (int)GoalStatus.Public && (y.AssignmentTypeId == (int)AssignmentType.WithParentObjective || y.ImportedId > 0)) && x.GoalStatusId == (int)GoalStatus.Public).ToList();
                    var archiveKr = dashboardResponse.MyGoalOkrResponses?.Where(x => x.GoalStatusId == (int)GoalStatus.Archived).ToList();
                    var excludeDuplicateKr = acceptedWithParentKr?.Except(newlyCreated).Except(pendingWithParentKr).ToList();

                    dashboardResponse.MyGoalOkrResponses = pendingStandaloneKr;
                    dashboardResponse.MyGoalOkrResponses?.AddRange(pendingWithParentKr);
                    dashboardResponse.MyGoalOkrResponses?.AddRange(draftKr);
                    dashboardResponse.MyGoalOkrResponses?.AddRange(newlyCreated);
                    dashboardResponse.MyGoalOkrResponses?.AddRange(acceptedStandaloneKr);
                    dashboardResponse.MyGoalOkrResponses?.AddRange(excludeDuplicateKr);
                    dashboardResponse.MyGoalOkrResponses?.AddRange(archiveKr);
                }
            }

            return dashboardResponse;
        }

        public async Task<DeltaResponse> DeltaScore(long empId, int cycle, int year, UserIdentity identity, string token, EmployeeResult allEmployee, QuarterDetails quarterDetails, OrganisationCycleDetails cycleDurationDetails)
        {
            DeltaResponse deltaResponse = new DeltaResponse();
            var prviousData = await commonService.GetLastSevenDaysProgress(empId, Constants.ZeroId, cycle, false, identity, Constants.ZeroId, false);
            deltaResponse.LastSevenDaysProgress = prviousData.Score;
            var sevenDaysStatusCardProgress = await commonService.GetLastSevenDaysStatusCardProgress(empId, Constants.ZeroId, cycle, false, quarterDetails, cycleDurationDetails, identity, Constants.ZeroId);
            deltaResponse.OnTrack = sevenDaysStatusCardProgress.LastSevenDaysProgressOnTrack;
            deltaResponse.AtRisk = sevenDaysStatusCardProgress.LastSevenDaysProgressAtRisk;
            deltaResponse.Lagging = sevenDaysStatusCardProgress.LastSevenDaysProgressLagging;
            deltaResponse.ContributorsLastSevenDaysProgress = await commonService.GetContributorsLastUpdateSevenDays(empId, Constants.ZeroId, cycle, false, allEmployee.Results, identity, Constants.ZeroId, Convert.ToDateTime(quarterDetails.EndDate));

            return deltaResponse;
        }

        public async Task<List<RecentContributionResponse>> RecentContribution(long empId, int cycle, int year, UserIdentity identity, EmployeeResult allEmployee)
        {
            DateTime? lastLoginTime = identity.LastLoginDateTime == null ? DateTime.UtcNow : identity.LastLoginDateTime;
            List<RecentContributionResponse> result = new List<RecentContributionResponse>();
            var keyDetail = await goalKeyRepo.GetQueryable().Where(x => x.CycleId == cycle && x.EmployeeId == empId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToListAsync();
            foreach (var key in keyDetail)
            {
                RecentContributionResponse keyDetails = new RecentContributionResponse();
                keyDetails.GoalKeyId = key.GoalKeyId;
                var contributorRecentProgress = await commonService.GetContributorRecentProgress(key.GoalKeyId, lastLoginTime, identity, allEmployee);
                keyDetails.ContributorsRecentProgress = contributorRecentProgress.Where(x => x.ContributorsContribution > 0).ToList();
                if (key.MetricId == (int)MetricType.Boolean)
                {
                    keyDetails.RecentContribution = keyDetails.ContributorsRecentProgress.Any(x => x.ContributorsContribution == 100) ? 100 : 0;
                }
                else
                {
                    keyDetails.RecentContribution = keyDetails.ContributorsRecentProgress.Sum(x => x.ContributorsContribution);
                }
                result.Add(keyDetails);
            }

            return result;
        }

        public async Task<List<EmailTeamLeaderResponse>> GetTeamGoals(long teamId, int cycle, int year)
        {
            List<EmailTeamLeaderResponse> goalList = new List<EmailTeamLeaderResponse>();
            var goalDetails = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycle && x.ImportedType == 1 && x.IsActive && x.Year == year && x.TeamId == teamId && x.GoalStatusId == (int)GoalStatus.Public).OrderByDescending(x => x.GoalObjectiveId).ThenBy(x => x.Sequence).ToListAsync();
            if (goalDetails.Count > 0)
            {
                foreach (var itemResult in goalDetails)
                {
                    var goalKey = new EmailTeamLeaderResponse
                    {
                        GoalId = itemResult.GoalObjectiveId,
                        EmployeeId = itemResult.EmployeeId,
                        ObjectiveName = itemResult.ObjectiveName

                    };
                    goalList.Add(goalKey);
                }
            }
            return goalList;
        }

        public async Task<VirtualAlignmentResponse> GetVirtualAlignment(long goalObjectiveId, UserIdentity identity, string token)
        {
            var virtualAlignmentResponse = new VirtualAlignmentResponse();
            var virtualDetails = new List<VirtualDetail>();
            var getObjectiveDetail = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive);
            if (getObjectiveDetail == null) return virtualAlignmentResponse;
            {
                var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
                var allTeamEmployees = await commonService.GetTeamEmployees();
                var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == getObjectiveDetail.Year);
                var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == getObjectiveDetail.ObjectiveCycleId);
                virtualAlignmentResponse.GoalObjectiveId = getObjectiveDetail.GoalObjectiveId;
                virtualAlignmentResponse.ObjectiveName = getObjectiveDetail.ObjectiveName;
                virtualAlignmentResponse.ObjectiveDescription = getObjectiveDetail.ObjectiveDescription;
                virtualAlignmentResponse.DueYear = getObjectiveDetail.Year;
                virtualAlignmentResponse.DueCycle = quarterDetails == null ? getObjectiveDetail.Year.ToString() : quarterDetails.Symbol + "-" + getObjectiveDetail.Year;
                virtualAlignmentResponse.TeamId = getObjectiveDetail.TeamId;


                if (getObjectiveDetail.TeamId > 0)
                {
                    var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == getObjectiveDetail.TeamId);
                    virtualAlignmentResponse.ColorCode = teamDetails == null ? "" : teamDetails.ColorCode;
                    virtualAlignmentResponse.BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode;
                    virtualAlignmentResponse.TeamName = teamDetails == null ? "" : teamDetails.OrganisationName;
                }

                var virtualObjective = goalObjectiveRepo.GetQueryable().Where(x => x.LinkedObjectiveId == goalObjectiveId && x.IsActive).ToList();
                var allEmployee = commonService.GetAllUserFromUsers(token).Results;
                foreach (var item in virtualObjective)
                {
                    var detail = new VirtualDetail();
                    var virtualEmp = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                    if (virtualEmp != null)
                    {
                        detail.FirstName = virtualEmp.FirstName;
                        detail.LastName = virtualEmp.LastName;
                        detail.EmployeeId = virtualEmp.EmployeeId;
                        detail.ImagePath = virtualEmp.ImagePath == null ? "" : virtualEmp.ImagePath.Trim();
                        detail.Designation = virtualEmp.Designation;
                        detail.GoalObjectiveId = item.GoalObjectiveId;
                        detail.ObjectiveName = item.ObjectiveName;
                        detail.ObjectiveDescription = item.ObjectiveDescription;
                        detail.DueYear = item.Year;
                        detail.DueDate = item.EndDate;
                        detail.DueCycle = quarterDetails == null ? item.Year.ToString() : quarterDetails.Symbol + "-" + item.Year;
                        detail.TeamId = virtualEmp.OrganisationID;
                        if (virtualEmp.OrganisationID > 0)
                        {
                            var empTeamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == virtualEmp.OrganisationID);
                            detail.ColorCode = empTeamDetails == null ? "" : empTeamDetails.ColorCode;
                            detail.BackGroundColorCode = empTeamDetails == null ? "" : empTeamDetails.BackGroundColorCode;
                            detail.TeamName = empTeamDetails == null ? "" : empTeamDetails.OrganisationName;
                        }
                        virtualDetails.Add(detail);
                    }
                }

                virtualAlignmentResponse.VirtualDetails = virtualDetails;
            }
            return virtualAlignmentResponse;
        }

        public async Task<TeamDetails> TeamDetailsById(long teamId, long sourceId, long goalKeyId, string jwtToken)
        {
            var teamDetails = commonService.GetTeamEmployeeByTeamId(teamId, jwtToken);
           
            foreach (var item in teamDetails.TeamEmployees)
            {
                item.IsAlreadyAligned = await IsTeamKrAlradyAligned(item.EmployeeId, sourceId, goalKeyId);
            }
            return teamDetails;
        }

        public async Task<AllOkrDashboardResponse> ArchiveDashboardAsync(long empId, int cycle, int year, string token, UserIdentity identity)
        {
            var dashboardResponse = new AllOkrDashboardResponse();
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            if (quarterDetails != null)
            {
                var lockDate = await commonService.IsOkrLocked(Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), empId, cycle, year, token);
                dashboardResponse.GoalSubmitDate = lockDate.GoalSubmitDate;

                var dashboardOkrResponses = new List<AllOkrDashboardOkrKRResponse>();
                var allEmployee = commonService.GetAllUserFromUsers(token);
                var objectives = GetEmployeeOkrByCycleId(empId, cycle, year).Result.Where(x => x.GoalStatusId != (int)GoalStatus.Draft).ToList();
                var allTeamEmployees = await commonService.GetTeamEmployees();
         
                if (objectives.Count != 0)
                {
                    foreach (var obj in objectives)
                    {
                        var dashboardKeyResponses = new List<AllOkrDashboardKeyResponse>();
                        var keyDetail = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == obj.GoalObjectiveId && x.IsActive 
                        && x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId == (int)GoalStatus.Archived).ToListAsync();
                        foreach (var key in keyDetail)
                        {
                            var teamDetails = new TeamDetails();
                            if (key.TeamId > 0)
                            {
                                teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == key.TeamId);
                            }

                            dashboardKeyResponses.Add(new AllOkrDashboardKeyResponse
                            {
                                GoalKeyId = key.GoalKeyId,
                                DueDate = key.DueDate,
                                Progress = (int)ProgressMaster.NotStarted,
                                KeyDescription = key.KeyDescription,
                                Contributors = await commonService.GetAllContributorAsync((int)GoalType.GoalKey, key.GoalKeyId, allEmployee.Results, identity, token),
                                StartDate = key.StartDate,
                                MetricId = key.MetricId,
                                AssignmentTypeId = key.AssignmentTypeId,
                                CurrencyId = key.CurrencyId,
                                CurrentValue = key.CurrentValue,
                                TargetValue = key.TargetValue,
                                KrStatusId = key.KrStatusId,
                                CurrencyCode = key.CurrencyCode,
                                GoalStatusId = key.GoalStatusId,
                                ContributorValue = key.ContributorValue,
                                StartValue = key.StartValue,
                                KeyNotes = key.KeyNotes,
                                IsLastStatusDraft = key.GoalStatusId == (int)GoalStatus.Archived && key.KrStatusId == (int)KrStatus.Accepted,
                                TeamId = key.TeamId,
                                TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                                ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                                BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,
                                ParentTeamDetail = key.ImportedId > 0 ? commonService.ParentTeamDetails((int)GoalType.GoalKey, key.ImportedId, allTeamEmployees, key.TeamId) : null,
                                ImportedType = key.ImportedType,
                                ImportedId = key.ImportedId
                            });
                        }

                        if (keyDetail.Count > 0 || (obj.GoalStatusId == (int)GoalStatus.Archived && obj.ImportedId == 0))
                        {
                            var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);

                            var teamDetails = new TeamDetails();
                            if (obj.TeamId > 0)
                            {
                                teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == obj.TeamId);
                            }

                            dashboardOkrResponses.Add(new AllOkrDashboardOkrKRResponse()
                            {
                                MyGoalsDetails = dashboardKeyResponses,
                                GoalObjectiveId = obj.GoalObjectiveId,
                                Year = obj.Year,
                                IsPrivate = obj.IsPrivate,
                                ObjectiveDescription = obj.ObjectiveDescription,
                                EmployeeId = obj.EmployeeId,
                                FirstName = objUser == null ? "N" : objUser.FirstName,
                                LastName = objUser == null ? "A" : objUser.LastName,
                                ImagePath = objUser?.ImagePath?.Trim(),
                                ObjectiveName = obj.ObjectiveName,
                                StartDate = obj.StartDate,
                                EndDate = obj.EndDate,
                                DueCycle = quarterDetails.Symbol + "," + year,
                                Progress = (int)ProgressMaster.NotStarted,                                
                                DueDate = obj.EndDate,                                
                                Contributors = commonService.GetDistinctObjContributor(allEmployee.Results, empId, dashboardKeyResponses, obj.IsCoachCreation, obj.Owner),
                                GoalStatusId = (int)GoalStatus.Archived,
                                GoalTypeId = obj.GoalTypeId,
                                AssignmentTypeId = (int)AssignmentType.WithParentObjective,
                                KrStatusId = (int)KrStatus.Accepted,
                                IsLastStatusDraft = obj.GoalStatusId == (int)GoalStatus.Archived && (dashboardKeyResponses.Count == 0 || dashboardKeyResponses.Any(x => x.IsLastStatusDraft)),
                                TeamId = obj.TeamId,
                                TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                                ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                                BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,    
                                ParentTeamDetail = obj.ImportedId > 0 ? commonService.ParentTeamDetails((int)GoalType.GoalObjective, obj.ImportedId, allTeamEmployees, obj.TeamId) : null,
                                TeamOkrRequests = new List<TeamOkrRequest>() { new TeamOkrRequest() { TeamId = obj.TeamId } },
                                ImportedType = obj.ImportedType,
                                ImportedId = obj.ImportedId,
                            });
                        }
                    }
                    dashboardResponse.MyGoalOkrResponses = dashboardOkrResponses;
                }
                
                var orphanKrDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == 0 && x.CycleId == cycle && x.IsActive && x.KrStatusId != (int)KrStatus.Declined
                && x.EmployeeId == empId && x.GoalStatusId == (int)GoalStatus.Archived).ToListAsync();
                var dashboardOrphanKeyResponse = new List<AllOkrDashboardOkrKRResponse>();
                foreach (var orphanKey in orphanKrDetails)
                {
                    var sourceKeyDetails = await GetGoalKeyById(orphanKey.ImportedId);
                    var sourceUserDetails = sourceKeyDetails != null ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId) : null;
                       
                    var teamDetails = new TeamDetails();
                    if (orphanKey.TeamId > 0)
                    {
                        teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == orphanKey.TeamId);
                    }
      
                    dashboardOrphanKeyResponse.Add(new AllOkrDashboardOkrKRResponse
                    {
                        GoalKeyId = orphanKey.GoalKeyId,
                        DueDate = orphanKey.DueDate,
                        Progress = (int)ProgressMaster.NotStarted,
                        EmployeeId = Convert.ToInt64(orphanKey.EmployeeId),
                        KeyDescription = orphanKey.KeyDescription,
                        Contributors = await commonService.GetAllContributorAsync((int)GoalType.GoalKey, orphanKey.GoalKeyId, allEmployee.Results, identity, token),
                        StartDate = orphanKey.StartDate,
                        MetricId = orphanKey.MetricId,
                        AssignmentTypeId = orphanKey.AssignmentTypeId,
                        CurrencyId = orphanKey.CurrencyId,
                        CurrentValue = orphanKey.CurrentValue,
                        TargetValue = orphanKey.TargetValue,
                        KrStatusId = orphanKey.KrStatusId,
                        CurrencyCode = orphanKey.CurrencyCode,
                        GoalStatusId = orphanKey.GoalStatusId,
                        ContributorValue = orphanKey.ContributorValue,
                        FirstName = sourceUserDetails == null ? "N" : sourceUserDetails.FirstName,
                        LastName = sourceUserDetails == null ? "A" : sourceUserDetails.LastName,
                        ImagePath = sourceUserDetails?.ImagePath?.Trim(),
                        StartValue = orphanKey.StartValue,
                        KeyNotes = orphanKey.KeyNotes,
                        DueCycle = quarterDetails.Symbol + "," + year,
                        TeamId = orphanKey.TeamId,
                        TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                        ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                        BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,
                        TeamOkrRequests = new List<TeamOkrRequest>() { new TeamOkrRequest() { TeamId = orphanKey.TeamId } },
                        ImportedType = orphanKey.ImportedType,
                        ImportedId = orphanKey.ImportedId,
                    });
                }

                if (dashboardOrphanKeyResponse.Count != 0)
                {
                    if (dashboardResponse.MyGoalOkrResponses != null)
                    {
                        dashboardResponse.MyGoalOkrResponses.AddRange(dashboardOrphanKeyResponse);
                    }
                    else
                    {
                        dashboardResponse.MyGoalOkrResponses = dashboardOrphanKeyResponse;
                    }
                }      
            }

            return dashboardResponse;
        }

        public async Task<EmployeeScoreResponse> GetEmployeeScoreDetails(long empId, int cycle, int year, string token, UserIdentity identity)
        {
            var employeeScore = new EmployeeScoreResponse();
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            if(quarterDetails != null)
            {
                var okrCount = await commonService.GetOkrCount(empId, cycle);
                var keyCount = await commonService.GetKeyCount(empId, cycle);
                var orphanKey = GetOrphanKey(empId, cycle).Result.Where(x => x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                var objectives = GetEmployeeOkrByCycleId(empId, cycle, year).Result.Where(x => x.GoalStatusId == (int)GoalStatus.Public).ToList();
                var totalGoalCount = objectives.Count + orphanKey.Count;
                var publicOrphanKrAvgScore = orphanKey.Count > 0 ? orphanKey.Select(x => x.Score).Sum() : 0;
                var publicOkrAvgScore = objectives.Count > 0 ? objectives.Select(x => x.Score).Sum() : 0;
                employeeScore.AvgScore = totalGoalCount > 0 ? commonService.KeyScore((publicOkrAvgScore + publicOrphanKrAvgScore) / totalGoalCount) : 0;
                employeeScore.OkrCount = okrCount;
                employeeScore.KrCount = keyCount;
            }
            return employeeScore;
        }

        #region Private Methods

        private int GetContributionsOfEmployee(long employeeId, int cycleId, long directReportId)
        {
            var selfKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == employeeId && x.IsActive && x.CycleId == cycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
            var contributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == directReportId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).ToList();
            return contributorsKeyResults.Count;
        }

        private List<GoalKey> GetContributionsOkrOfEmployee(long employeeId, int cycleId, long directReportId)
        {
            var selfKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == employeeId && x.IsActive && x.CycleId == cycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
            var contributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == directReportId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).ToList();
            return contributorsKeyResults;
        }

        private int GetDistinctContributorsCount(long employeeId, int cycleId)
        {
            var directReportsKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == employeeId && x.IsActive && x.CycleId == cycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
            var directReportsContributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && directReportsKeyResultIds.Contains(x.ImportedId)).ToList();
            var distinctContributors = directReportsContributorsKeyResults.Select(x => x.EmployeeId).Distinct().ToList();
            return distinctContributors.Count;
        }

        private void SeparationKRStatusCount_DashboardAsync(int keyProgressId, ref int keyNotStartedCount, ref int keyAtRiskCount, ref int keyLaggingCount, ref int keyOnTrackCount)
        {
            if (keyProgressId == (int)ProgressMaster.NotStarted)
            {
                keyNotStartedCount++;
            }
            else if (keyProgressId == (int)ProgressMaster.AtRisk)
            {
                keyAtRiskCount++;
            }
            else if (keyProgressId == (int)ProgressMaster.Lagging)
            {
                keyLaggingCount++;
            }
            else if (keyProgressId == (int)ProgressMaster.OnTrack)
            {
                keyOnTrackCount++;
            }
        }
        private void SeparationOkrStatusCount_DashboardAsync(int objProgressId, ref int objNotStartedCount, ref int objAtRiskCount, ref int objLaggingCount, ref int objOnTrackCount)
        {
            if (objProgressId == (int)ProgressMaster.NotStarted)
            {
                objNotStartedCount++;
            }
            else if (objProgressId == (int)ProgressMaster.AtRisk)
            {
                objAtRiskCount++;
            }
            else if (objProgressId == (int)ProgressMaster.Lagging)
            {
                objLaggingCount++;
            }
            else if (objProgressId == (int)ProgressMaster.OnTrack)
            {
                objOnTrackCount++;
            }
        }

        private async Task<bool> IsFirstTimeUserAsync(long employeeId)
        {
            var goalKey = await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.CreatedBy == employeeId);
            if (goalKey == null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsAlreadyCreatedOkr(long employeeId)
        {
            var isAnyOkr = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.CreatedBy == employeeId);
            if (isAnyOkr != null)
                return true;
            else
                return false;
        }

        private async Task<bool> IsTeamKrAlradyAligned(long contribtorId, long sourceId, long goalKeyId)
        {
            if (goalKeyId == 0)
            {
                var alignedStatus = await commonService.GetGoalKeySource(contribtorId, sourceId);
                return alignedStatus.IsAligned;
            }
            else
            {
                var goalKeySource = await goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == contribtorId && x.IsActive && (x.Source == sourceId || x.GoalKeyId == sourceId) && x.ImportedId != goalKeyId).ToListAsync();
                return goalKeySource.Any();
            }
        }
        #endregion
    }
}
