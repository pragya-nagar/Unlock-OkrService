using Microsoft.EntityFrameworkCore;
using OKRService.Common;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OKRService.Service
{
    [ExcludeFromCodeCoverage]
    public class MyGoalsService : BaseService, IMyGoalsService
    {
        private readonly IRepositoryAsync<GoalObjective> goalObjectiveRepo;
        private readonly IRepositoryAsync<GoalKey> goalKeyRepo;
        private readonly IRepositoryAsync<UnLockLog> unlockLogRepo;
        private readonly IRepositoryAsync<TypeOfGoalCreation> typeOfGoalCreationRepo;
        private readonly INotificationService notificationService;
        private readonly ICommonService commonService;
        private readonly IRepositoryAsync<KrStatusMessage> krStatusMessageRepo;
        private readonly IRepositoryAsync<GoalSequence> goalSequenceRepo;
        private readonly IProgressBarCalculationService progressBarCalculationService;
        private readonly IRepositoryAsync<GoalKeyHistory> goalKeyHistoryRepo;

        public MyGoalsService(IServicesAggregator servicesAggregateService, INotificationService notificationServices, ICommonService commonServices, IProgressBarCalculationService progressBarCalculationServices) : base(servicesAggregateService)
        {
            goalObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<GoalObjective>();
            goalKeyRepo = UnitOfWorkAsync.RepositoryAsync<GoalKey>();
            unlockLogRepo = UnitOfWorkAsync.RepositoryAsync<UnLockLog>();
            typeOfGoalCreationRepo = UnitOfWorkAsync.RepositoryAsync<TypeOfGoalCreation>();
            notificationService = notificationServices;
            commonService = commonServices;
            krStatusMessageRepo = UnitOfWorkAsync.RepositoryAsync<KrStatusMessage>();
            goalSequenceRepo = UnitOfWorkAsync.RepositoryAsync<GoalSequence>();
            progressBarCalculationService = progressBarCalculationServices;
            goalKeyHistoryRepo = UnitOfWorkAsync.RepositoryAsync<GoalKeyHistory>();
        }

        public List<TypeOfGoalCreation> GetTypeOfGoalCreations()
        {
            return typeOfGoalCreationRepo.GetQueryable().Where(x => x.IsActive).ToList();
        }

        public async Task<MyGoalsRequest> UpdateObjective(MyGoalsRequest myGoalsRequests, UserIdentity loginUser, string jwtToken)
        {
            var goalObjective = GetGoalObjective(myGoalsRequests.GoalObjectiveId);
            if (myGoalsRequests.GoalObjectiveId > 0 && goalObjective != null)
            {
                if (myGoalsRequests.GoalTypeId == 1)
                {
                    await UpdateTeamOkr(myGoalsRequests, loginUser, jwtToken);
                    return myGoalsRequests;
                }
                ////GoalStatusId will be 2 for public
                ////GoalTypeId will be 2 for individual
                goalObjective.ObjectiveDescription = myGoalsRequests.ObjectiveDescription;
                goalObjective.ObjectiveName = myGoalsRequests.ObjectiveName;
                goalObjective.IsPrivate = myGoalsRequests.IsPrivate;
                goalObjective.StartDate = myGoalsRequests.StartDate;
                goalObjective.EmployeeId = (myGoalsRequests.GoalStatusId == (int)GoalStatus.Public && myGoalsRequests.ImportedId == 0)
                        ? myGoalsRequests.Owner == 0 ? myGoalsRequests.EmployeeId : myGoalsRequests.Owner
                        : myGoalsRequests.EmployeeId;
                goalObjective.EndDate = myGoalsRequests.EndDate;
                goalObjective.GoalStatusId = myGoalsRequests.GoalStatusId;
                goalObjective.GoalTypeId = myGoalsRequests.GoalTypeId;
                goalObjective.UpdatedOn = DateTime.UtcNow;
                goalObjective.UpdatedBy = loginUser.EmployeeId;
                goalObjective.Owner = myGoalsRequests.Owner;
                goalObjective.LinkedObjectiveId = myGoalsRequests.LinkedObjectiveId;
                goalObjective.TeamId = 0;
                await UpdateObjectiveAsync(goalObjective);

                ////if (myGoalsRequests.LinkedObjectiveId > 0 && goalObjective.GoalStatusId == (int) GoalStatus.Public)
                ////{
                ////    var linkedObjectiveDetail = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == myGoalsRequests.LinkedObjectiveId && x.IsActive);
                ////    if (linkedObjectiveDetail != null)
                ////    {
                ////        await Task.Run(async () =>
                ////        {
                ////            await notificationService.VirtualLinkingNotifications(linkedObjectiveDetail.EmployeeId, loginUser, jwtToken).ConfigureAwait(false);
                ////        }).ConfigureAwait(false);
                ////    }
                ////}

                if (goalObjective.GoalStatusId == (int)GoalStatus.Draft && myGoalsRequests.IsSavedAsDraft)
                {
                    await Task.Run(async () =>
                    {
                        await notificationService.DraftOkrNotifications(jwtToken, loginUser, goalObjective).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }

                ////if (goalObjective.ImportedType == 1 && goalObjective.ImportedId > 0)
                //// {
                //// await Task.Run(async () =>
                //// {
                ////      await notificationService.AlignObjNotifications(jwtToken, loginUser.EmployeeId, goalObjective.ImportedType, goalObjective.ImportedId).ConfigureAwait(false);
                ////   }).ConfigureAwait(false);
                ////  }
            }

            if (myGoalsRequests.MyGoalsDetails.Any(x => x.GoalKeyId >= 0))
            {
                foreach (var data in myGoalsRequests.MyGoalsDetails)
                {
                    var goalKey = new GoalKey();
                    goalKey = GetGoalKeyDetails(data.GoalKeyId);
                    ////var krObjective = GetGoalObjective(goalKey.GoalObjectiveId);

                    if (goalKey != null)
                    {
                        goalKey.DueDate = data.DueDate > myGoalsRequests.EndDate ? myGoalsRequests.EndDate : data.DueDate;
                        goalKey.KeyDescription = data.KeyDescription;
                        goalKey.UpdatedBy = loginUser.EmployeeId;
                        goalKey.UpdatedOn = DateTime.UtcNow;
                        goalKey.StartDate = data.StartDate;
                        goalKey.CurrentValue = data.CurrentValue == 0 ? data.StartValue : data.CurrentValue;
                        goalKey.CurrencyId = data.CurrencyId;
                        goalKey.CurrencyCode = data.CurrencyCode;
                        goalKey.TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : data.TargetValue;
                        goalKey.KrStatusId = data.KrStatusId;
                        goalKey.GoalStatusId = data.GoalStatusId;
                        goalKey.KeyNotes = data.KeyNotes;
                        goalKey.MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId;
                        goalKey.AssignmentTypeId = data.AssignmentTypeId;
                        goalKey.StartValue = data.StartValue;
                        goalKey.Owner = data.Owner; ////goalObjective.Owner; 
                        goalKey.EmployeeId = goalKey.ImportedId > 0 ? goalKey.EmployeeId : data.Owner; ////(data.GoalStatusId == (int)GoalStatus.Public && data.ImportedId == 0) ? data.Owner == 0 ? goalKey.EmployeeId : data.Owner : goalKey.EmployeeId;
                        goalKey.TeamId = 0;
                        await UpdateKeyResults(goalKey);
                    }
                    else
                    {
                        var goals = new GoalKey
                        {
                            DueDate = data.DueDate > myGoalsRequests.EndDate ? myGoalsRequests.EndDate : data.DueDate,
                            GoalObjectiveId = myGoalsRequests.GoalObjectiveId,
                            KeyDescription = data.KeyDescription,
                            CreatedBy = loginUser.EmployeeId,
                            Score = data.Score,
                            ImportedType = data.ImportedType,
                            ImportedId = data.ImportedId,
                            Source = 0,//data.Source,
                            Progress = (int)ProgressMaster.NotStarted,
                            StartDate = data.StartDate,
                            MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId,
                            AssignmentTypeId = data.AssignmentTypeId,
                            CurrencyId = data.CurrencyId,
                            CurrentValue = data.StartValue,
                            TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : data.TargetValue,
                            CycleId = myGoalsRequests.ObjectiveCycleId,
                            CurrencyCode = data.CurrencyCode,
                            GoalStatusId = data.GoalStatusId,
                            ContributorValue = data.ContributorValue,
                            KrStatusId = data.KrStatusId,
                            StartValue = data.StartValue,
                            KeyNotes = data.KeyNotes,
                            Owner = data.Owner, ////goalObjective.Owner,
                            EmployeeId = data.Owner, ////(data.GoalStatusId == (int)GoalStatus.Public && data.ImportedId == 0)? data.Owner == 0 ? myGoalsRequests.EmployeeId : data.Owner: myGoalsRequests.EmployeeId
                            TeamId = 0
                        };
                        var goalDetails = await InsertKeyResults(goals);
                        data.GoalKeyId = goalDetails.GoalKeyId;
                        var updateKrValue = new KrValueUpdate
                        {
                            GoalKeyId = goalDetails.GoalKeyId,
                            CurrentValue = goalDetails.CurrentValue,
                            Year = goalObjective.Year
                        };

                        await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goals);

                    }

                    ////If new contributors is added
                    if (data.Contributors.Any())
                    {
                        var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
                        foreach (var item in data.Contributors)
                        {
                            var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                            var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                from data2 in cycle.QuarterDetails
                                                where data2.IsCurrentQuarter
                                                select new CycleLockDetails
                                                {
                                                    StartDate = Convert.ToDateTime(data2.StartDate),
                                                    EndDate = Convert.ToDateTime(data2.EndDate),
                                                    OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                    Year = Int32.Parse(cycle.Year)
                                                }).FirstOrDefault();


                            var isOkrLocked = await commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, item.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken);

                            ////if contributor exists or not
                            var contributors = GetKeyContributor((int)GoalType.GoalKey, data.GoalKeyId, item.EmployeeId);
                            ////If null add the contributor otherwise update the existing contributor
                            if (contributors == null)
                            {
                                var goalObjectives = new GoalObjective();
                                var isObjectiveImported = IsObjectiveImported(goalObjective.GoalObjectiveId, item.EmployeeId);
                                if (!isObjectiveImported)
                                {
                                    if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                    {
                                        goalObjectives.Year = item.Year;
                                        goalObjectives.ObjectiveCycleId = item.CycleId;
                                        goalObjectives.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                        goalObjectives.ObjectiveName = item.ObjectiveName;
                                        goalObjectives.IsPrivate = goalObjective.IsPrivate;
                                        goalObjectives.StartDate = goalObjective.StartDate;
                                        goalObjectives.EndDate = goalObjective.EndDate;
                                        goalObjectives.ImportedId = goalObjective.GoalObjectiveId;
                                        goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                        goalObjectives.Source = goalObjective.GoalObjectiveId;
                                        goalObjectives.CreatedBy = loginUser.EmployeeId;
                                        goalObjectives.Score = 0;////item.Score;
                                        goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                        goalObjectives.EmployeeId = item.EmployeeId;
                                        goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                        goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : (int)GoalRequest.GoalStatusId;
                                        goalObjectives.GoalTypeId = (int)GoalRequest.GoalTypeId;
                                        goalObjectives.Owner = goalObjective.Owner;
                                      ////  goalObjectives.LinkedObjectiveId = goalObjective.LinkedObjectiveId;

                                        await InsertObjective(goalObjectives);

                                        ////if (goalObjectives.GoalStatusId != (int)GoalStatus.Draft)
                                        ////{
                                        ////    await Task.Run(async () =>
                                        ////    {
                                        ////        await notificationService.ObjContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, goalObjective.GoalObjectiveId, goalObjectives.GoalObjectiveId).ConfigureAwait(false);
                                        ////    }).ConfigureAwait(false);
                                        ////}
                                    }
                                }
                                else
                                {
                                    goalObjectives = GetGoalObjectiveByImportedId(goalObjective.GoalObjectiveId, item.EmployeeId);                              
                                }

                                var goalKeys = new GoalKey
                                {
                                    StartDate = item.StartDate < data.StartDate ? DateTime.Now : item.StartDate,
                                    DueDate = item.DueDate > data.DueDate ? data.DueDate : item.DueDate,
                                    GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                    KeyDescription = item.KeyResult,
                                    CreatedBy = loginUser.EmployeeId,
                                    Score = 0,//item.Score,
                                    ImportedType = (int)GoalRequest.KeyImportedType,
                                    EmployeeId = item.EmployeeId,
                                    ImportedId = data.GoalKeyId,
                                    Source = data.ImportedId == 0 ? data.GoalKeyId : data.Source,
                                    Progress = (int)ProgressMaster.NotStarted,
                                    MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId,
                                    CurrencyId = data.CurrencyId,
                                    CurrentValue = item.StartValue,
                                    TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                    CycleId = item.CycleId,
                                    KrStatusId = item.KrStatusId,
                                    GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                    AssignmentTypeId = item.AssignmentTypeId,
                                    StartValue = item.StartValue,
                                    Owner = data.Owner, ////goalObjective.Owner,
                                    KeyNotes = data.KeyNotes
                                };
                                await InsertKeyResults(goalKeys);

                                var krStatusMessage = new KrStatusMessage
                                {
                                    AssignerGoalKeyId = data.GoalKeyId,
                                    AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                    KrAssignerMessage = item.KrAssignerMessage,
                                    CreatedOnAssigner = DateTime.Now,
                                    CreatedOnAssignee = DateTime.Now,
                                    IsActive = true

                                };
                                await InsertMessagesOfKr(krStatusMessage);

                                if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                                {
                                    await Task.Run(async () =>
                                    {
                                        await notificationService.KeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, data.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                    }).ConfigureAwait(false);
                                }

                                ////if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                ////{
                                ////    KrValueUpdate updateKrValue = new KrValueUpdate();
                                ////    updateKrValue.GoalKeyId = goalKeys.GoalKeyId;
                                ////    updateKrValue.CurrentValue = goalKeys.CurrentValue;
                                ////    updateKrValue.Year = goalObjective.Year;

                                ////    await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goalKeys);

                                ////}

                            }

                            ////Updating the existing contributor
                            else
                            {
                                if (item.AssignmentTypeId == contributors.AssignmentTypeId)
                                {
                                    var contributorsOldGoalStatusId = contributors.GoalStatusId;
                                    contributors.StartDate = item.StartDate < data.StartDate ? DateTime.Now : item.StartDate;
                                    contributors.DueDate = item.DueDate > data.DueDate ? data.DueDate : item.DueDate;
                                    if (item.AssignmentTypeId == 2 && contributors.GoalObjectiveId > 0)
                                    {
                                        var goalDetails = GetGoalObjective(contributors.GoalObjectiveId);
                                        if (goalDetails != null)
                                        {
                                            goalDetails.ObjectiveName = item.ObjectiveName;
                                            goalDetails.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                            goalDetails.GoalStatusId = item.GoalStatusId;                                           
                                            await UpdateObjectiveAsync(goalDetails);
                                        }
                                    }
                                    contributors.KeyDescription = item.KeyResult;
                                    contributors.KeyNotes = data.KeyNotes;
                                    contributors.CurrentValue = item.CurrentValue;
                                    contributors.TargetValue = item.TargetValue;
                                    contributors.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                    contributors.UpdatedOn = DateTime.UtcNow;
                                    contributors.UpdatedBy = loginUser.EmployeeId;

                                    await UpdateKeyResults(contributors);
                                    if (contributorsOldGoalStatusId == (int)GoalStatus.Draft && contributors.GoalStatusId != (int)GoalStatus.Draft)
                                    {
                                        await Task.Run(async () =>
                                        {
                                            await notificationService.KeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, data.GoalKeyId, contributors.GoalKeyId, contributors).ConfigureAwait(false);
                                        }).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return myGoalsRequests;
        }
        public GoalObjective UpdateObjective(GoalObjective goalObjective)
        {
            goalObjectiveRepo.Update(goalObjective);
            UnitOfWorkAsync.SaveChanges();
            return goalObjective;
        }

        public async Task<long> UpdateGoalAttributes(MyGoalsDetails data, UserIdentity loginUser, string jwtToken)
        {
            if (data.GoalKeyId > 0)
            {
                var goalKey = GetGoalKeyDetails(data.GoalKeyId);
                var krObjective = GetGoalObjective(goalKey.GoalObjectiveId);
                if (goalKey != null)
                {
                    if (goalKey.GoalStatusId == (int)GoalStatus.Draft)
                    {
                        goalKey.MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId;
                        goalKey.StartValue = data.StartValue;
                        goalKey.CurrentValue = data.StartValue;
                        goalKey.TargetValue = data.TargetValue;
                        if (goalKey.MetricId == (int)Metrics.Currency)
                        {
                            goalKey.CurrencyId = data.CurrencyId;
                        }
                    }

                    goalKey.DueDate = data.DueDate;
                    goalKey.KeyDescription = data.KeyDescription;
                    goalKey.UpdatedBy = loginUser.EmployeeId;
                    goalKey.UpdatedOn = DateTime.UtcNow;
                    goalKey.StartDate = data.StartDate;
                    goalKey.KeyNotes = data.KeyNotes;
                    await UpdateKeyResults(goalKey);
                }

                var allTeamEmployees = await commonService.GetTeamEmployees();
                var contributorId = new List<long>();
                if (data.Contributors.Any())
                {
                    foreach (var item in data.Contributors)
                    {
                        var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
                        if (item.IsTeamSelected)
                        {
                            var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == item.TeamId);
                            var employeeDetail = teamDetails?.TeamEmployees;
                            if (employeeDetail != null)
                            {
                                foreach (var employees in employeeDetail)
                                {
                                    if (employees.EmployeeId != goalKey.Owner)
                                    {
                                        var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == employees.EmployeeId);
                                        var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                                        var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                            from data2 in cycle.QuarterDetails
                                                            where data2.IsCurrentQuarter
                                                            select new CycleLockDetails
                                                            {
                                                                StartDate = Convert.ToDateTime(data2.StartDate),
                                                                EndDate = Convert.ToDateTime(data2.EndDate),
                                                                OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                                Year = Int32.Parse(cycle.Year)
                                                            }).FirstOrDefault();


                                        var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, employees.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                                        ////if contributor exists or not
                                        var contributors = GetKeyContributor((int)GoalType.GoalKey, data.GoalKeyId, employees.EmployeeId);
                                        var goalObjectives = new GoalObjective();
                                        ////If null add the contributor otherwise update the existing contributor
                                        if (contributors == null)
                                        {
                                            var alignStatus = AlignStatus(employees.EmployeeId, (int)GoalType.GoalKey, data.Source);
                                            if (!alignStatus.IsAligned)
                                            {
                                                if (krObjective != null)
                                                {
                                                    bool isObjectiveImported;
                                                    if (krObjective.TeamId > 0)
                                                    {
                                                        isObjectiveImported = IsTeamObjectiveImported(krObjective.GoalObjectiveId, employees.EmployeeId, goalKey.TeamId);
                                                    }
                                                    else
                                                    {
                                                        isObjectiveImported = IsObjectiveImported(krObjective.GoalObjectiveId, employees.EmployeeId);
                                                    }                                                   
                                                    if (!isObjectiveImported)
                                                    {
                                                        if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective && krObjective != null)
                                                        {
                                                            goalObjectives.Year = item.Year;
                                                            goalObjectives.ObjectiveCycleId = item.CycleId;
                                                            goalObjectives.ObjectiveDescription = krObjective.ObjectiveDescription;
                                                            goalObjectives.ObjectiveName = item.ObjectiveName ?? krObjective.ObjectiveName;
                                                            goalObjectives.IsPrivate = krObjective.IsPrivate;
                                                            goalObjectives.StartDate = item.StartDate;
                                                            goalObjectives.EndDate = item.DueDate;
                                                            goalObjectives.ImportedId = krObjective.GoalObjectiveId;
                                                            goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                                            goalObjectives.Source = krObjective.GoalObjectiveId;
                                                            goalObjectives.CreatedBy = loginUser.EmployeeId;
                                                            goalObjectives.Score = 0.00M; ////item.Score;
                                                            goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                                            goalObjectives.EmployeeId = employees.EmployeeId;
                                                            goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                                            goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                                            goalObjectives.GoalTypeId = krObjective.GoalTypeId;
                                                            goalObjectives.TeamId = goalKey.TeamId;
                                                            goalObjectives.Owner = krObjective.Owner;
                                                          ////  goalObjectives.LinkedObjectiveId = krObjective.LinkedObjectiveId;
                                                            goalObjectives.GoalTypeId = krObjective.GoalTypeId;
                                                            await InsertObjective(goalObjectives);
                                                        }
                                                    }

                                                    else
                                                    {
                                                        if (krObjective.TeamId > 0)
                                                        {
                                                            goalObjectives = GetTeamGoalObjectiveByImportedId(krObjective.GoalObjectiveId, employees.EmployeeId, goalKey.TeamId);
                                                        }
                                                        else
                                                        {
                                                            goalObjectives = GetGoalObjectiveByImportedId(krObjective.GoalObjectiveId, employees.EmployeeId);
                                                        }
                                                        
                                                        goalObjectives.ObjectiveName = item.ObjectiveName ?? krObjective.ObjectiveName;
                                                        await UpdateObjectiveAsync(goalObjectives);
                                                    }
                                                }

                                                var goalKeys = new GoalKey
                                                {
                                                    StartDate = item.StartDate < goalKey.StartDate ? DateTime.Now : item.StartDate,
                                                    DueDate = item.DueDate > goalKey.DueDate ? goalKey.DueDate : item.DueDate,
                                                    GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                                    KeyDescription = item.KeyResult,
                                                    CreatedBy = loginUser.EmployeeId,
                                                    Score = 0.00M, //item.Score,
                                                    ImportedType = (int)GoalRequest.KeyImportedType,
                                                    EmployeeId = employees.EmployeeId,
                                                    ImportedId = goalKey.GoalKeyId,
                                                    Source = goalKey.ImportedId == 0 ? goalKey.GoalKeyId : goalKey.Source,
                                                    Progress = (int)ProgressMaster.NotStarted,
                                                    MetricId = goalKey.MetricId == 0 ? (int)Metrics.NoUnits : goalKey.MetricId,
                                                    CurrencyId = goalKey.CurrencyId,
                                                    CurrentValue = item.StartValue,
                                                    TargetValue = goalKey.MetricId == (int)Metrics.Boolean || goalKey.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                                    CycleId = item.CycleId,
                                                    StartValue = item.StartValue,
                                                    KrStatusId = item.KrStatusId,
                                                    AssignmentTypeId = item.AssignmentTypeId,
                                                    TeamId = goalKey.TeamId,
                                                    GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                                    Owner = goalKey.Owner,
                                                    KeyNotes = goalKey.KeyNotes
                                                };
                                                await InsertKeyResults(goalKeys);
                                                contributorId.Add(Convert.ToInt64(goalKeys.EmployeeId));
                                                var krStatusMessage = new KrStatusMessage
                                                {
                                                    AssignerGoalKeyId = goalKey.GoalKeyId,
                                                    AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                                    KrAssignerMessage = item.KrAssignerMessage,
                                                    CreatedOnAssigner = DateTime.Now,
                                                    CreatedOnAssignee = DateTime.Now,
                                                    IsActive = true
                                                };
                                                await InsertMessagesOfKr(krStatusMessage);

                                                if (goalKeys.GoalStatusId != (int)GoalStatus.Draft && goalKeys.TeamId > 0)
                                                {
                                                    await Task.Run(async () =>
                                                    {
                                                        await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, employees.EmployeeId, goalKey.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                                    }).ConfigureAwait(false);
                                                }

                                                else if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                                                {
                                                    await Task.Run(async () =>
                                                    {
                                                        await notificationService.KeyContributorsNotifications(jwtToken, loginUser.EmployeeId, employees.EmployeeId, goalKey.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                                    }).ConfigureAwait(false);
                                                }

                                                ////if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                                ////{
                                                ////    KrValueUpdate updateKrValue = new KrValueUpdate();
                                                ////    updateKrValue.GoalKeyId = goalKeys.GoalKeyId;
                                                ////    updateKrValue.CurrentValue = goalKeys.CurrentValue;
                                                ////    updateKrValue.Year = goalObjectives.Year;

                                                ////    await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goalKeys);
                                                ////}

                                            }
                                        }

                                        ////Updating the existing contributor
                                        else
                                        {
                                            ////If Assignment type is not update then ok
                                            if (item.AssignmentTypeId == contributors.AssignmentTypeId)
                                            {
                                                if (item.AssignmentTypeId == 2 && contributors.GoalObjectiveId > 0)
                                                {
                                                    var goalDetails = GetGoalObjective(contributors.GoalObjectiveId);
                                                    if (goalDetails != null)
                                                    {
                                                        goalDetails.ObjectiveName = item.ObjectiveName ?? krObjective.ObjectiveName;
                                                        await UpdateObjectiveAsync(goalDetails);
                                                    }
                                                }

                                                contributors.StartDate = item.StartDate < goalKey.StartDate ? goalKey.StartDate : item.StartDate;
                                                contributors.DueDate = item.DueDate > goalKey.DueDate ? goalKey.DueDate : item.DueDate;
                                                contributors.KeyDescription = item.KeyResult;
                                                contributors.KeyNotes = goalKey.KeyNotes;
                                                contributors.CurrentValue = item.CurrentValue;
                                                contributors.TargetValue = item.TargetValue;
                                                contributors.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                                contributors.UpdatedOn = DateTime.UtcNow;
                                                contributors.UpdatedBy = loginUser.EmployeeId;

                                                await UpdateKeyResults(contributors);

                                                contributorId.Add(Convert.ToInt64(contributors.EmployeeId));

                                                var isMessageRecordExists = krStatusMessageRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.AssignerGoalKeyId == contributors.ImportedId && x.IsActive);
                                                if (isMessageRecordExists != null)
                                                {
                                                    isMessageRecordExists.KrAssignerMessage = item.KrAssignerMessage;
                                                    isMessageRecordExists.CreatedOnAssigner = DateTime.Now;
                                                    await UpdateMessagesOfKr(isMessageRecordExists);
                                                }

                                                await Task.Run(async () =>
                                                {
                                                    await notificationService.UpdateContributorsKeyNotifications(contributors, loginUser, jwtToken).ConfigureAwait(false);
                                                }).ConfigureAwait(false);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (item.EmployeeId != goalKey.Owner)
                        {
                            var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                            var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                from data2 in cycle.QuarterDetails
                                                where data2.IsCurrentQuarter
                                                select new CycleLockDetails
                                                {
                                                    StartDate = Convert.ToDateTime(data2.StartDate),
                                                    EndDate = Convert.ToDateTime(data2.EndDate),
                                                    OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                    Year = Int32.Parse(cycle.Year)
                                                }).FirstOrDefault();


                            var isOkrLocked = await commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, item.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken);
                            ////var goalStatusId = 0;
                            ////if (goalKey.TeamId == 0)
                            ////{
                            ////    goalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                            ////}
                            ////if contributor exists or not
                            var contributors = GetKeyContributor((int)GoalType.GoalKey, data.GoalKeyId, item.EmployeeId);
                            var goalObjectives = new GoalObjective();
                            ////If null add the contributor otherwise update the existing contributor
                            if (contributors == null)
                            {
                                var alignStatus = AlignStatus(item.EmployeeId, (int)GoalType.GoalKey, data.Source);
                                if (!alignStatus.IsAligned)
                                {
                                    if (krObjective != null)
                                    {
                                        bool isObjectiveImported;
                                        if (krObjective.TeamId > 0)
                                        {
                                            isObjectiveImported = IsTeamObjectiveImported(krObjective.GoalObjectiveId, item.EmployeeId, goalKey.TeamId);
                                        }
                                        else
                                        {
                                            isObjectiveImported = IsObjectiveImported(krObjective.GoalObjectiveId, item.EmployeeId);
                                        }
                                        if (!isObjectiveImported)
                                        {
                                            if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective && krObjective != null)
                                            {
                                                goalObjectives.Year = item.Year;
                                                goalObjectives.ObjectiveCycleId = item.CycleId;
                                                goalObjectives.ObjectiveDescription = krObjective.ObjectiveDescription;
                                                goalObjectives.ObjectiveName = item.ObjectiveName ?? krObjective.ObjectiveName;
                                                goalObjectives.IsPrivate = krObjective.IsPrivate;
                                                goalObjectives.StartDate = item.StartDate;
                                                goalObjectives.EndDate = item.DueDate;
                                                goalObjectives.ImportedId = krObjective.GoalObjectiveId;
                                                goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                                goalObjectives.Source = krObjective.GoalObjectiveId;
                                                goalObjectives.CreatedBy = loginUser.EmployeeId;
                                                goalObjectives.Score = 0.00M; ////item.Score;
                                                goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                                goalObjectives.EmployeeId = item.EmployeeId;
                                                goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                                goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId; ////goalStatusId == 0 ? item.GoalStatusId : goalStatusId;
                                                goalObjectives.GoalTypeId = krObjective.GoalTypeId;
                                                goalObjectives.TeamId = goalKey.TeamId;
                                                goalObjectives.Owner = krObjective.Owner;
                                                ////goalObjectives.LinkedObjectiveId = krObjective.LinkedObjectiveId;
                                                goalObjectives.GoalTypeId = krObjective.GoalTypeId;
                                                await InsertObjective(goalObjectives);

                                                ////if (goalObjectives.GoalStatusId != (int)GoalStatus.Draft)
                                                ////{
                                                ////    await Task.Run(async () =>
                                                ////    {
                                                ////        await notificationService.ObjContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, krObjective.GoalObjectiveId, goalObjectives.GoalObjectiveId).ConfigureAwait(false);
                                                ////    }).ConfigureAwait(false);
                                                ////}
                                            }
                                        }

                                        else
                                        {
                                            if (krObjective.TeamId > 0)
                                            {
                                                goalObjectives = GetTeamGoalObjectiveByImportedId(krObjective.GoalObjectiveId, item.EmployeeId, goalKey.TeamId);
                                            }
                                            else
                                            {
                                                goalObjectives = GetGoalObjectiveByImportedId(krObjective.GoalObjectiveId, item.EmployeeId);
                                            }
                                            goalObjectives.ObjectiveName = item.ObjectiveName ?? krObjective.ObjectiveName;
                                            await UpdateObjectiveAsync(goalObjectives);
                                        }
                                    }

                                    var goalKeys = new GoalKey
                                    {
                                        StartDate = item.StartDate < goalKey.StartDate ? DateTime.Now : item.StartDate,
                                        DueDate = item.DueDate > goalKey.DueDate ? goalKey.DueDate : item.DueDate,
                                        GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                        KeyDescription = item.KeyResult,
                                        CreatedBy = loginUser.EmployeeId,
                                        Score = 0.00M, //item.Score,
                                        ImportedType = (int)GoalRequest.KeyImportedType,
                                        EmployeeId = item.EmployeeId,
                                        ImportedId = goalKey.GoalKeyId,
                                        Source = goalKey.ImportedId == 0 ? goalKey.GoalKeyId : goalKey.Source,
                                        Progress = (int)ProgressMaster.NotStarted,
                                        MetricId = goalKey.MetricId == 0 ? (int)Metrics.NoUnits : goalKey.MetricId,
                                        CurrencyId = goalKey.CurrencyId,
                                        CurrentValue = item.StartValue,
                                        TargetValue = goalKey.MetricId == (int)Metrics.Boolean || goalKey.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                        CycleId = item.CycleId,
                                        StartValue = item.StartValue,
                                        KrStatusId = item.KrStatusId,
                                        AssignmentTypeId = item.AssignmentTypeId,
                                        TeamId = goalKey.TeamId,
                                        GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId, ////goalStatusId == 0 ? item.GoalStatusId : goalStatusId,
                                        Owner = goalKey.Owner,
                                        KeyNotes = goalKey.KeyNotes
                                    };
                                    await InsertKeyResults(goalKeys);
                                    contributorId.Add(Convert.ToInt64(goalKeys.EmployeeId));
                                    var krStatusMessage = new KrStatusMessage
                                    {
                                        AssignerGoalKeyId = goalKey.GoalKeyId,
                                        AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                        KrAssignerMessage = item.KrAssignerMessage,
                                        CreatedOnAssigner = DateTime.Now,
                                        CreatedOnAssignee = DateTime.Now,
                                        IsActive = true
                                    };
                                    await InsertMessagesOfKr(krStatusMessage);

                                    if (goalKeys.GoalStatusId != (int)GoalStatus.Draft && goalKeys.TeamId > 0)
                                    {
                                        await Task.Run(async () =>
                                        {
                                            await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, goalKey.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                        }).ConfigureAwait(false);
                                    }
                                    else if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                                    {
                                        await Task.Run(async () =>
                                        {
                                            await notificationService.KeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, goalKey.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                        }).ConfigureAwait(false);
                                    }

                                    ////if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                    ////{
                                    ////    KrValueUpdate updateKrValue = new KrValueUpdate();
                                    ////    updateKrValue.GoalKeyId = goalKeys.GoalKeyId;
                                    ////    updateKrValue.CurrentValue = goalKeys.CurrentValue;
                                    ////    updateKrValue.Year = goalObjectives.Year;

                                    ////    await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goalKeys);
                                    ////}
                                }
                            }

                            ////Updating the existing contributor
                            else
                            {
                                ////If Assignment type is not update then ok
                                if (item.AssignmentTypeId == contributors.AssignmentTypeId)
                                {
                                    if (item.AssignmentTypeId == 2 && contributors.GoalObjectiveId > 0)
                                    {
                                        var goalDetails = GetGoalObjective(contributors.GoalObjectiveId);
                                        if (goalDetails != null)
                                        {
                                            goalDetails.ObjectiveName = item.ObjectiveName ?? krObjective.ObjectiveName;
                                            await UpdateObjectiveAsync(goalDetails);
                                        }
                                    }
                                    contributors.StartDate = item.StartDate < goalKey.StartDate ? goalKey.StartDate : item.StartDate;
                                    contributors.DueDate = item.DueDate > goalKey.DueDate ? goalKey.DueDate : item.DueDate;
                                    contributors.KeyDescription = item.KeyResult;
                                    contributors.KeyNotes = goalKey.KeyNotes;
                                    contributors.CurrentValue = item.CurrentValue;
                                    contributors.TargetValue = item.TargetValue;
                                    contributors.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId; ////goalStatusId == 0 ? item.GoalStatusId : goalStatusId;
                                    contributors.UpdatedOn = DateTime.UtcNow;
                                    contributors.UpdatedBy = loginUser.EmployeeId;

                                    await UpdateKeyResults(contributors);
                                    contributorId.Add(Convert.ToInt64(contributors.EmployeeId));

                                    var isMessageRecordExists = krStatusMessageRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.AssignerGoalKeyId == contributors.ImportedId && x.IsActive);
                                    if (isMessageRecordExists != null)
                                    {
                                        isMessageRecordExists.KrAssignerMessage = item.KrAssignerMessage;
                                        isMessageRecordExists.CreatedOnAssigner = DateTime.Now;
                                        await UpdateMessagesOfKr(isMessageRecordExists);
                                    }

                                }
                            }
                        }
                    }
                }
                var updateDueDate = new UpdateDueDateRequest()
                {
                    StartDate = data.StartDate,
                    GoalType = Constants.GoalKeyType,
                    EndDate = data.DueDate,
                    GoalId = data.GoalKeyId
                };

                await UpdateKrDueDate(updateDueDate, loginUser, jwtToken, true, contributorId, false, data.StartDate, data.DueDate);

            }

            return 1;
        }

        public async Task<long> BecomeContributor(AddContributorRequest addContributorRequest, UserIdentity loginUser, string jwtToken)
        {
            var goalObjective = new GoalObjective();

            if (addContributorRequest.GoalKeyId > 0)
            {
                var goalKey = GetGoalKeyDetails(addContributorRequest.GoalKeyId);
                ////  var isAlign = AlignStatus(addContributorRequest.EmployeeId, (int)GoalType.GoalKey, goalKey.Source);
                ////  if(!isAlign)
                ////  {
                if (!addContributorRequest.IsSelf)
                {
                    await BecomeContributorWithTeam(addContributorRequest, loginUser, jwtToken, goalKey);
                }
                else
                {
                    if (addContributorRequest.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                    {
                        bool isObjectiveImported;
                        var krObjective = GetGoalObjective(addContributorRequest.GoalObjectiveId);
                        if (krObjective.TeamId > 0)
                        {
                            isObjectiveImported = IsTeamObjectiveImported(krObjective.GoalObjectiveId, addContributorRequest.EmployeeId, krObjective.TeamId);
                        }
                        else
                        {
                            isObjectiveImported = IsObjectiveImported(krObjective.GoalObjectiveId, addContributorRequest.EmployeeId);
                        }
                        if (!isObjectiveImported)
                        {
                            goalObjective.ObjectiveName = addContributorRequest.ObjectiveName;
                            goalObjective.Year = krObjective.Year;
                            goalObjective.IsPrivate = krObjective.IsPrivate;
                            goalObjective.StartDate = addContributorRequest.StartDate;
                            goalObjective.EndDate = addContributorRequest.DueDate;
                            goalObjective.EmployeeId = addContributorRequest.EmployeeId;
                            goalObjective.ImportedId = krObjective.GoalObjectiveId;
                            goalObjective.ImportedType = (int)GoalRequest.ImportedType;
                            goalObjective.GoalStatusId = addContributorRequest.GoalStatusId;
                            goalObjective.Progress = (int)ProgressMaster.NotStarted;
                            goalObjective.Sequence = (int)GoalRequest.Sequence;
                            goalObjective.Source = krObjective.GoalObjectiveId;
                            goalObjective.CreatedBy = loginUser.EmployeeId;
                            goalObjective.GoalTypeId = addContributorRequest.GoalTypeId;
                            goalObjective.Score = 0; ////addContributorRequest.Score;
                            goalObjective.ObjectiveCycleId = addContributorRequest.ObjectiveCycleId;
                            goalObjective.Owner = krObjective.Owner;
                           //// goalObjective.LinkedObjectiveId = goalObjective.LinkedObjectiveId;
                            goalObjective.TeamId = krObjective.TeamId;
                            await InsertObjective(goalObjective);
                        }
                        else
                        {
                            if (krObjective.TeamId > 0)
                            {
                                goalObjective = GetTeamGoalObjectiveByImportedId(krObjective.GoalObjectiveId, addContributorRequest.EmployeeId, krObjective.TeamId);
                            }
                            else
                            {
                                goalObjective = GetGoalObjectiveByImportedId(krObjective.GoalObjectiveId, addContributorRequest.EmployeeId);
                            }                          
                            goalObjective.ObjectiveName = addContributorRequest.ObjectiveName;
                            await UpdateObjectiveAsync(goalObjective);
                        }
                    }

                    long key = 0;

                    if (addContributorRequest.AssignmentTypeId == (int)AssignmentType.WithParentObjective && goalObjective != null)
                    {
                        key = goalObjective.GoalObjectiveId;
                    }
                    else
                    {
                        key = addContributorRequest.GoalObjectiveId;
                    }
                    var goalKeys = new GoalKey
                    {
                        StartDate = addContributorRequest.StartDate < goalKey.StartDate ? DateTime.Now : addContributorRequest.StartDate,
                        DueDate = addContributorRequest.DueDate > goalKey.DueDate ? goalKey.DueDate : addContributorRequest.DueDate,
                        GoalObjectiveId = key > 0 ? key : 0,
                        KeyDescription = addContributorRequest.KeyDescription,
                        CreatedBy = loginUser.EmployeeId,
                        Score = 0, //addContributorRequest.Score,
                        ImportedType = (int)GoalRequest.KeyImportedType,
                        EmployeeId = addContributorRequest.EmployeeId,
                        ImportedId = goalKey.GoalKeyId,
                        Source = goalKey.ImportedId == 0 ? goalKey.GoalKeyId : goalKey.Source,
                        Progress = (int)ProgressMaster.NotStarted,
                        MetricId = addContributorRequest.MetricId == 0 ? (int)Metrics.NoUnits : addContributorRequest.MetricId,
                        CurrencyId = addContributorRequest.CurrencyId,
                        CurrentValue = addContributorRequest.StartValue,
                        TargetValue = goalKey.MetricId == (int)Metrics.Boolean || goalKey.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : addContributorRequest.TargetValue,
                        CycleId = addContributorRequest.ObjectiveCycleId,
                        StartValue = addContributorRequest.StartValue,
                        KrStatusId = addContributorRequest.KrStatusId,
                        AssignmentTypeId = addContributorRequest.AssignmentTypeId,
                        GoalStatusId = addContributorRequest.GoalStatusId,
                        Owner = goalKey.Owner,
                        TeamId = goalKey.TeamId
                    };
                    await InsertKeyResults(goalKeys);

                    var krStatusMessage = new KrStatusMessage
                    {
                        AssignerGoalKeyId = goalKey.GoalKeyId,
                        AssigneeGoalKeyId = goalKeys.GoalKeyId,
                        KrAssigneeMessage = addContributorRequest.KrAssignerMessage,
                        CreatedOnAssigner = DateTime.UtcNow,
                        CreatedOnAssignee = DateTime.UtcNow,
                        IsActive = true
                    };
                    await InsertMessagesOfKr(krStatusMessage);

                    await Task.Run(async () =>
                    {
                        await notificationService.AligningParentObjective(jwtToken, loginUser.EmployeeId, addContributorRequest, goalKeys).ConfigureAwait(false);
                    }).ConfigureAwait(false);

                    ////   }
                    
                    if (addContributorRequest.Contributors.Any())
                    {
                        var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
                        foreach (var item in addContributorRequest.Contributors)
                        {
                            if (item.EmployeeId != goalKeys.Owner)
                            {
                                var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                                var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                                var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                    from data2 in cycle.QuarterDetails
                                                    where data2.IsCurrentQuarter
                                                    select new CycleLockDetails
                                                    {
                                                        StartDate = Convert.ToDateTime(data2.StartDate),
                                                        EndDate = Convert.ToDateTime(data2.EndDate),
                                                        OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                        Year = Int32.Parse(cycle.Year)
                                                    }).FirstOrDefault();

                                var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, item.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                                ////if contributor exists or not
                                var contributors = GetKeyContributor((int)GoalType.GoalKey, goalKeys.GoalKeyId, item.EmployeeId);
                                ////If null add the contributor otherwise update the existing contributor
                                if (contributors == null)
                                {
                                    var goalObjectives = new GoalObjective();
                                    var isObjectiveImported = IsTeamObjectiveImported(goalObjective.GoalObjectiveId, item.EmployeeId, goalObjective.TeamId);
                                    if (!isObjectiveImported)
                                    {
                                        if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                        {
                                            goalObjectives.Year = item.Year;
                                            goalObjectives.ObjectiveCycleId = item.CycleId;
                                            goalObjectives.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                            goalObjectives.ObjectiveName = item.ObjectiveName;
                                            goalObjectives.IsPrivate = goalObjective.IsPrivate;
                                            goalObjectives.StartDate = goalObjective.StartDate;
                                            goalObjectives.EndDate = goalObjective.EndDate;
                                            goalObjectives.ImportedId = goalObjective.GoalObjectiveId;
                                            goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                            goalObjectives.Source = goalObjective.GoalObjectiveId;
                                            goalObjectives.CreatedBy = loginUser.EmployeeId;
                                            goalObjectives.Score = 0; ////item.Score;
                                            goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                            goalObjectives.EmployeeId = item.EmployeeId;
                                            goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                            goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                            goalObjectives.GoalTypeId = goalObjective.GoalTypeId;
                                            goalObjectives.TeamId = goalObjective.TeamId;
                                            goalObjectives.Owner = goalObjective.Owner;
                                          ////  goalObjectives.LinkedObjectiveId = goalObjective.LinkedObjectiveId;
                                            InsertObjectiveNonAsync(goalObjectives);
                                        }
                                    }
                                    else
                                    {
                                        goalObjectives = GetTeamGoalObjectiveByImportedId(goalObjective.GoalObjectiveId, item.EmployeeId, goalObjective.TeamId);
                                        goalObjectives.ObjectiveName = item.ObjectiveName;
                                        await UpdateObjectiveAsync(goalObjectives);
                                    }

                                    var goals = new GoalKey
                                    {
                                        StartDate = item.StartDate < goalKeys.StartDate ? DateTime.Now : item.StartDate,
                                        DueDate = item.DueDate > goalKeys.DueDate ? goalKeys.DueDate : item.DueDate,
                                        GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                        KeyDescription = item.KeyResult,
                                        CreatedBy = loginUser.EmployeeId,
                                        Score = 0, //item.Score,
                                        ImportedType = (int)GoalRequest.KeyImportedType,
                                        EmployeeId = item.EmployeeId,
                                        ImportedId = goalKeys.GoalKeyId,
                                        Source = goalKeys.ImportedId == 0 ? goalKeys.GoalKeyId : goalKeys.Source,
                                        Progress = (int)ProgressMaster.NotStarted,
                                        MetricId = goalKeys.MetricId == 0 ? (int)Metrics.NoUnits : goalKeys.MetricId,
                                        CurrencyId = goalKeys.CurrencyId,
                                        CurrentValue = item.StartValue,
                                        TargetValue = goalKeys.MetricId == (int)Metrics.Boolean || goalKeys.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                        CycleId = item.CycleId,
                                        KrStatusId = item.KrStatusId,
                                        GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                        AssignmentTypeId = item.AssignmentTypeId,
                                        StartValue = item.StartValue,
                                        TeamId = goalObjective.TeamId,
                                        Owner = goalKeys.Owner, ////goalObjective.Owner,
                                        KeyNotes = goalKeys.KeyNotes
                                    };

                                    InsertKeyResultNonAsync(goals);

                                    var krStatusMessages = new KrStatusMessage
                                    {
                                        AssignerGoalKeyId = goalKeys.GoalKeyId,
                                        AssigneeGoalKeyId = goals.GoalKeyId,
                                        KrAssignerMessage = item.KrAssignerMessage,
                                        CreatedOnAssigner = DateTime.Now,
                                        CreatedOnAssignee = DateTime.Now,
                                        IsActive = true
                                    };

                                    await InsertMessagesOfKr(krStatusMessages);

                                    if (goals.GoalStatusId != (int)GoalStatus.Draft)
                                    {
                                        await Task.Run(async () =>
                                        {
                                            await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, goalKeys.GoalKeyId, goals.GoalKeyId, goals).ConfigureAwait(false);
                                        }).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contributorKeyResultRequest"></param>
        /// <param name="userIdentity"></param>
        /// <returns></returns>
        public async Task<long> UpdateContributorsKeyResult(ContributorKeyResultRequest contributorKeyResultRequest, UserIdentity userIdentity, string jwtToken)
        {
            if (contributorKeyResultRequest.GoalKeyId > 0)
            {
                var isGoalKeyExists = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == contributorKeyResultRequest.GoalKeyId && x.IsActive);
                if (isGoalKeyExists != null)
                {
                    if (contributorKeyResultRequest.KrStatusId > 0 && contributorKeyResultRequest.KrStatusId == (int)KrStatus.Declined)
                    {
                        ////if someone declined [if krStatusId is 3]
                        isGoalKeyExists.KrStatusId = contributorKeyResultRequest.KrStatusId;
                        isGoalKeyExists.UpdatedBy = userIdentity.EmployeeId;
                        isGoalKeyExists.UpdatedOn = DateTime.Now;
                        await UpdateKeyResults(isGoalKeyExists);

                        var messageDetails = krStatusMessageRepo.GetQueryable().FirstOrDefault(x => x.AssignerGoalKeyId == contributorKeyResultRequest.ImportedId && x.AssigneeGoalKeyId == contributorKeyResultRequest.GoalKeyId && x.IsActive);
                        messageDetails.KrAssigneeMessage = contributorKeyResultRequest.KrAssigneeMessage;
                        messageDetails.CreatedOnAssignee = DateTime.Now;
                        await UpdateMessagesOfKr(messageDetails);

                        await Task.Run(async () =>
                        {
                            await notificationService.DeclineKr(jwtToken, userIdentity.EmployeeId, contributorKeyResultRequest).ConfigureAwait(false);
                        }).ConfigureAwait(false);

                    }
                    else if (!contributorKeyResultRequest.IsSelf)
                    {
                        await AcceptWithTeam(contributorKeyResultRequest, userIdentity, jwtToken, isGoalKeyExists);
                    }
                    ////Someone is updating KR status from declined to something else
                    else if (contributorKeyResultRequest.KrStatusId > 0 && contributorKeyResultRequest.KrStatusId != (int)KrStatus.Declined)
                    {
                        var sourceYear = new int();
                        var goalObjectiveDetails = new GoalObjective();
                        if (isGoalKeyExists.GoalObjectiveId > 0)
                        {
                            goalObjectiveDetails = GetGoalObjective(isGoalKeyExists.GoalObjectiveId);
                            ////Here whatever objective user has typed get enter in database
                            goalObjectiveDetails.ObjectiveName = contributorKeyResultRequest.ObjectiveName;
                            goalObjectiveDetails.GoalStatusId = contributorKeyResultRequest.GoalStatusId;
                            goalObjectiveDetails.ObjectiveDescription = contributorKeyResultRequest.ObjectiveDescription;
                            await UpdateObjectiveAsync(goalObjectiveDetails);
                            sourceYear = goalObjectiveDetails.Year;
                        }

                        isGoalKeyExists.KeyDescription = contributorKeyResultRequest.KeyDescription;
                        isGoalKeyExists.GoalObjectiveId = contributorKeyResultRequest.GoalObjectiveId;
                        isGoalKeyExists.StartDate = contributorKeyResultRequest.StartDate;
                        isGoalKeyExists.DueDate = contributorKeyResultRequest.DueDate;
                        isGoalKeyExists.KrStatusId = contributorKeyResultRequest.KrStatusId;
                        isGoalKeyExists.TargetValue = contributorKeyResultRequest.TargetValue;
                        isGoalKeyExists.CurrencyId = contributorKeyResultRequest.CurrencyId;
                        isGoalKeyExists.CurrencyCode = contributorKeyResultRequest.CurrencyCode;
                        isGoalKeyExists.UpdatedBy = userIdentity.EmployeeId;
                        isGoalKeyExists.UpdatedOn = DateTime.Now;
                        isGoalKeyExists.StartValue = contributorKeyResultRequest.StartValue;
                        isGoalKeyExists.CurrentValue = contributorKeyResultRequest.StartValue;
                        isGoalKeyExists.GoalStatusId = contributorKeyResultRequest.GoalStatusId;
                        isGoalKeyExists.KeyNotes = contributorKeyResultRequest.KeyNotes;
                        ////isGoalKeyExists.Owner = contributorKeyResultRequest.Owner;
                        await UpdateKeyResults(isGoalKeyExists);

                        if (sourceYear == Constants.Zero)
                        {
                            var sourceId = commonService.GetSourceId(isGoalKeyExists.ImportedId, isGoalKeyExists.ImportedType);
                            var sourceGoalKeyExists = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == sourceId && x.IsActive);
                            var sourceObjectiveDetails = GetGoalObjective(sourceGoalKeyExists.GoalObjectiveId);
                            sourceYear = sourceObjectiveDetails.Year;
                        }

                        KrValueUpdate krValueUpdate = new KrValueUpdate()
                        {
                            GoalKeyId = isGoalKeyExists.GoalKeyId,
                            CurrentValue = isGoalKeyExists.CurrentValue, //Constants.Zero,
                            Year = sourceYear
                        };

                        await progressBarCalculationService.UpdateKrValue(krValueUpdate, userIdentity, jwtToken, isGoalKeyExists);

                        var isMessageRecordExists = krStatusMessageRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.AssignerGoalKeyId == contributorKeyResultRequest.ImportedId && x.AssigneeGoalKeyId == contributorKeyResultRequest.GoalKeyId && x.IsActive);
                        if (isMessageRecordExists != null)
                        {
                            isMessageRecordExists.KrAssigneeMessage = contributorKeyResultRequest.KrAssigneeMessage;
                            isMessageRecordExists.CreatedOnAssignee = DateTime.Now;
                            await UpdateMessagesOfKr(isMessageRecordExists);

                            await Task.Run(async () =>
                            {
                                await notificationService.AcceptsOkr(jwtToken, userIdentity.EmployeeId, contributorKeyResultRequest).ConfigureAwait(false);
                            }).ConfigureAwait(false);
                        }

                        if (contributorKeyResultRequest.Contributors.Any())
                        {
                            var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
                            foreach (var item in contributorKeyResultRequest.Contributors)
                            {
                                if (item.EmployeeId != isGoalKeyExists.Owner)
                                {
                                    var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                                    var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                                    var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                        from data2 in cycle.QuarterDetails
                                                        where data2.IsCurrentQuarter
                                                        select new CycleLockDetails
                                                        {
                                                            StartDate = Convert.ToDateTime(data2.StartDate),
                                                            EndDate = Convert.ToDateTime(data2.EndDate),
                                                            OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                            Year = Int32.Parse(cycle.Year)
                                                        }).FirstOrDefault();

                                    var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, item.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                                    ////if contributor exists or not
                                    var contributors = GetKeyContributor((int)GoalType.GoalKey, isGoalKeyExists.GoalKeyId, item.EmployeeId);
                                    ////If null add the contributor otherwise update the existing contributor
                                    if (contributors == null)
                                    {
                                        var goalObjectives = new GoalObjective();
                                        var isObjectiveImported = IsTeamObjectiveImported(goalObjectiveDetails.GoalObjectiveId, item.EmployeeId, goalObjectiveDetails.TeamId);
                                        if (!isObjectiveImported)
                                        {
                                            if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                            {
                                                goalObjectives.Year = item.Year;
                                                goalObjectives.ObjectiveCycleId = item.CycleId;
                                                goalObjectives.ObjectiveDescription = goalObjectiveDetails.ObjectiveDescription;
                                                goalObjectives.ObjectiveName = item.ObjectiveName;
                                                goalObjectives.IsPrivate = goalObjectiveDetails.IsPrivate;
                                                goalObjectives.StartDate = goalObjectiveDetails.StartDate;
                                                goalObjectives.EndDate = goalObjectiveDetails.EndDate;
                                                goalObjectives.ImportedId = goalObjectiveDetails.GoalObjectiveId;
                                                goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                                goalObjectives.Source = goalObjectiveDetails.GoalObjectiveId;
                                                goalObjectives.CreatedBy = userIdentity.EmployeeId;
                                                goalObjectives.Score = 0; ////item.Score;
                                                goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                                goalObjectives.EmployeeId = item.EmployeeId;
                                                goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                                goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                                goalObjectives.GoalTypeId = goalObjectiveDetails.GoalTypeId;
                                                goalObjectives.TeamId = goalObjectiveDetails.TeamId;
                                                goalObjectives.Owner = goalObjectiveDetails.Owner;
                                             ////   goalObjectives.LinkedObjectiveId = goalObjectiveDetails.LinkedObjectiveId;
                                                InsertObjectiveNonAsync(goalObjectives);
                                            }
                                        }
                                        else
                                        {
                                            goalObjectives = GetTeamGoalObjectiveByImportedId(goalObjectiveDetails.GoalObjectiveId, item.EmployeeId, goalObjectiveDetails.TeamId);
                                            goalObjectives.ObjectiveName = item.ObjectiveName;
                                            await UpdateObjectiveAsync(goalObjectives);
                                        }

                                        var goals = new GoalKey
                                        {
                                            StartDate = item.StartDate < isGoalKeyExists.StartDate ? DateTime.Now : item.StartDate,
                                            DueDate = item.DueDate > isGoalKeyExists.DueDate ? isGoalKeyExists.DueDate : item.DueDate,
                                            GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                            KeyDescription = item.KeyResult,
                                            CreatedBy = userIdentity.EmployeeId,
                                            Score = 0, //item.Score,
                                            ImportedType = (int)GoalRequest.KeyImportedType,
                                            EmployeeId = item.EmployeeId,
                                            ImportedId = isGoalKeyExists.GoalKeyId,
                                            Source = isGoalKeyExists.ImportedId == 0 ? isGoalKeyExists.GoalKeyId : isGoalKeyExists.Source,
                                            Progress = (int)ProgressMaster.NotStarted,
                                            MetricId = isGoalKeyExists.MetricId == 0 ? (int)Metrics.NoUnits : isGoalKeyExists.MetricId,
                                            CurrencyId = isGoalKeyExists.CurrencyId,
                                            CurrentValue = item.StartValue,
                                            TargetValue = isGoalKeyExists.MetricId == (int)Metrics.Boolean || isGoalKeyExists.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                            CycleId = item.CycleId,
                                            KrStatusId = item.KrStatusId,
                                            GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                            AssignmentTypeId = item.AssignmentTypeId,
                                            StartValue = item.StartValue,
                                            TeamId = goalObjectiveDetails.TeamId,
                                            Owner = isGoalKeyExists.Owner, ////goalObjective.Owner,
                                            KeyNotes = isGoalKeyExists.KeyNotes
                                        };

                                        InsertKeyResultNonAsync(goals);

                                        var krStatusMessages = new KrStatusMessage
                                        {
                                            AssignerGoalKeyId = isGoalKeyExists.GoalKeyId,
                                            AssigneeGoalKeyId = goals.GoalKeyId,
                                            KrAssignerMessage = item.KrAssignerMessage,
                                            CreatedOnAssigner = DateTime.Now,
                                            CreatedOnAssignee = DateTime.Now,
                                            IsActive = true
                                        };

                                        await InsertMessagesOfKr(krStatusMessages);

                                        if (goals.GoalStatusId != (int)GoalStatus.Draft)
                                        {
                                            await Task.Run(async () =>
                                            {
                                                await notificationService.TeamKeyContributorsNotifications(jwtToken, userIdentity.EmployeeId, item.EmployeeId, isGoalKeyExists.GoalKeyId, goals.GoalKeyId, goals).ConfigureAwait(false);
                                            }).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }
                    }                 
                }
            }

            return 1;
        }

        public GoalKey GetKeyContributor(int goalType, long goalKeyId, long employeeId)
        {
            GoalKey goalKey = new GoalKey();
            goalKey = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.ImportedType == goalType && x.ImportedId == goalKeyId && x.EmployeeId == employeeId && x.IsActive);
            return goalKey;
        }

        public async Task<GoalObjective> UpdateObjectiveAsync(GoalObjective goalObjective)
        {
            goalObjectiveRepo.Update(goalObjective);
            await UnitOfWorkAsync.SaveChangesAsync();
            return goalObjective;
        }

        public async Task<MyGoalResponse> MyGoal(long empId, int cycle, int year, string token, UserIdentity identity)
        {
            var myGoalResponse = new MyGoalResponse();
            var cycleDetail = commonService.GetOrganisationCycleDetail(identity.OrganisationId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            if (quarterDetails != null)
            {
                var lockDate = await commonService.IsOkrLocked(Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), empId, cycle, year, token);

                myGoalResponse.IsLocked = lockDate.IsGaolLocked;
                myGoalResponse.IsScoreLocked = lockDate.IsScoreLocked;

                var keyCount = 0;
                var myGoalOkrResponses = new List<MyGoalOkrResponse>();
                var allEmployee = commonService.GetAllUserFromUsers(token);
                var allFeedback = await commonService.GetAllFeedback(token, empId);
                var objectives = GetEmployeeOkrByCycleId(empId, cycle, year).OrderByDescending(x => x.GoalObjectiveId).ThenBy(x => x.Sequence).ToList();
                if (objectives.Count != 0)
                {
                    myGoalResponse.OkrCount = objectives.Count;

                    foreach (var obj in objectives)
                    {
                        var myGoalKeyResponses = new List<MyGoalKeyResponse>();

                        var keyDetail = GetGoalKey(obj.GoalObjectiveId);
                        foreach (var key in keyDetail)
                        {
                            keyCount += 1;
                            var keyTime = (int)DateTime.UtcNow.Subtract(key.CreatedOn).TotalMinutes;
                            var keyScoreUpdateDetail = commonService.LatestUpdateGoalKey(key.GoalKeyId);

                            myGoalKeyResponses.Add(new MyGoalKeyResponse
                            {
                                GoalKeyId = key.GoalKeyId,
                                IsNewItem = keyTime <= Constants.NewItemTime,
                                DueDate = key.DueDate,
                                Score = key.Score,
                                Source = key.Source,
                                ImportedType = key.ImportedType,
                                ImportedId = key.ImportedId,
                                KeyDescription = key.KeyDescription,
                                KeyProgressTime = keyScoreUpdateDetail == null ? key.CreatedOn : keyScoreUpdateDetail.UpdatedOn,
                                IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId),
                                Contributors = commonService.GetContributor((int)GoalType.GoalKey, key.GoalKeyId, allEmployee.Results)
                            });
                        }

                        var createdTime = (int)DateTime.UtcNow.Subtract(obj.CreatedOn).TotalMinutes;
                        var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);
                        myGoalOkrResponses.Add(new MyGoalOkrResponse
                        {
                            MyGoalsDetails = myGoalKeyResponses,
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
                            DueCycle = quarterDetails.Symbol + "-" + year,
                            Score = obj.Score,
                            GoalProgressTime = myGoalKeyResponses.Count <= 0 ? obj.CreatedOn : myGoalKeyResponses.OrderByDescending(x => x.KeyProgressTime).FirstOrDefault().KeyProgressTime,
                            Source = obj.Source,
                            DueDate = obj.EndDate,
                            IsNewItem = createdTime <= Constants.NewItemTime,
                            IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == obj.GoalObjectiveId),
                            Contributors = commonService.GetContributor((int)GoalType.GoalObjective, obj.GoalObjectiveId, allEmployee.Results)
                        });
                    }

                    myGoalResponse.KeyCount = keyCount;
                    myGoalResponse.MyGoalOkrResponses = myGoalOkrResponses;
                }
            }

            return myGoalResponse;
        }

        public async Task<AlignResponse> AlignObjective(long empId, int cycle, int year, string token, UserIdentity loginUser)
        {
            var cycleDetail = commonService.GetOrganisationCycleDetail(loginUser.OrganisationId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);

            var alignResponse = new AlignResponse();
            var alignOkrResponses = new List<AlignOkrResponse>();
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var objectives = GetEmployeeOkrByCycleId(empId, cycle, year).Where(x => !x.IsPrivate).OrderByDescending(x => x.GoalObjectiveId).ThenBy(x => x.Sequence).ToList();
            if (objectives.Count != 0)
            {
                foreach (var obj in objectives)
                {
                    var alignKeyResponses = await SeparationAlignKeyResponse_AlignObjective(obj.GoalObjectiveId, allEmployee.Results, loginUser);

                    alignOkrResponses.Add(new AlignOkrResponse
                    {
                        MyGoalsDetails = alignKeyResponses,
                        GoalObjectiveId = obj.GoalObjectiveId,
                        Year = obj.Year,
                        IsPrivate = obj.IsPrivate,
                        ObjectiveDescription = obj.ObjectiveDescription,
                        EmployeeId = obj.EmployeeId,
                        ObjectiveName = obj.ObjectiveName,
                        StartDate = obj.StartDate,
                        EndDate = obj.EndDate,
                        DueCycle = quarterDetails == null ? Constants.DueCycleQ3 : quarterDetails.Symbol + "-" + year,
                        Score = obj.Score,
                        Source = obj.Source,
                        DueDate = obj.EndDate,
                        IsObjectiveDisabled = commonService.GetGoalObjectiveSource(loginUser.EmployeeId, obj.Source == 0 ? obj.GoalObjectiveId : obj.Source).Result.IsAligned
                    });
                }

                alignResponse.MyGoalOkrResponses = alignOkrResponses;

            }

            return alignResponse;
        }

        public async Task<string> DeleteContributors(long employeeId, long goalKeyId, UserIdentity userIdentity, string jwtToken)
        {
            var result = "";
            var keyDetails = goalKeyRepo.GetQueryable().AsNoTracking().FirstOrDefault(x => x.GoalKeyId == goalKeyId && x.IsActive);
            if (keyDetails != null)
            {
                var count = GetCount(goalKeyId);
                var getAllLevelContributorsList = commonService.GetAllLevelKrContributors((int)GoalType.GoalKey, goalKeyId, employeeId);//// KR contributors for Notifications             
                var deleteKrList = new List<DeleteKrResponse>();
                var currentCycle = await commonService.GetCurrentCycleAsync(userIdentity.OrganisationId);
                await DeleteScoreAndContributorDownwards(goalKeyId, Convert.ToDateTime(currentCycle.CycleStartDate), Convert.ToDateTime(currentCycle.CycleEndDate), currentCycle.OrganisationCycleId, userIdentity);

                if (keyDetails.MetricId == (int)MetricType.NoUnits)
                {
                    if (keyDetails.ImportedId > 0)
                    {
                        var importedId = keyDetails.ImportedId;
                        do
                        {
                            var goalKeyDetails = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == importedId);

                            if (goalKeyDetails == null)
                                break;

                            var upperLevelContributors = goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.ImportedId == goalKeyDetails.GoalKeyId && x.IsActive).ToList();

                            goalKeyDetails.Score = Math.Round((upperLevelContributors.Select(x => x.Score).Sum() + goalKeyDetails.CurrentValue) / (upperLevelContributors.Count + 1), 2);
                            var sourceProgress = commonService.GetProgressIdWithFormula(keyDetails.DueDate, Convert.ToDateTime(currentCycle.CycleStartDate), Convert.ToDateTime(currentCycle.CycleEndDate), keyDetails.Score, currentCycle.OrganisationCycleId);
                            goalKeyDetails.Progress = sourceProgress;
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

                            progressBarCalculationService.UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);

                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                var objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                                if (objectiveDetails != null)
                                {
                                    var objectiveKeyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId);

                                    objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                    goalObjectiveRepo.Update(objectiveDetails);
                                    UnitOfWorkAsync.SaveChanges();
                                }
                            }
                            importedId = goalKeyDetails.ImportedId;
                        }
                        while (importedId != 0);
                    }
                }
                else if (keyDetails.MetricId == (int)MetricType.Boolean)
                {

                    if (keyDetails.ImportedId > 0)
                    {
                        var importedId = keyDetails.ImportedId;
                        do
                        {
                            var goalKeyDetails = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == importedId);

                            if (goalKeyDetails == null)
                                break;

                            var allContributors = commonService.GetKRContributorAsync(2, goalKeyDetails.GoalKeyId).Result;

                            goalKeyDetails.ContributorValue = 0;
                            if (goalKeyDetails.CurrentValue > 0 || (goalKeyDetails.ContributorValue > 0 && allContributors.Count > 0 && allContributors.All(x => x.Score == 100)))
                            {
                                goalKeyDetails.Score = 100;
                            }
                            else
                            {
                                goalKeyDetails.Score = 0;
                            }
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                            var sourceProgress = commonService.GetProgressIdWithFormula(goalKeyDetails.DueDate, Convert.ToDateTime(currentCycle.CycleStartDate), Convert.ToDateTime(currentCycle.CycleEndDate), goalKeyDetails.Score, currentCycle.OrganisationCycleId);
                            goalKeyDetails.Progress = sourceProgress;

                            progressBarCalculationService.UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);

                            importedId = goalKeyDetails.ImportedId;


                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                var objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                                if (objectiveDetails != null)
                                {
                                    var objectiveKeyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId);

                                    objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                    goalObjectiveRepo.Update(objectiveDetails);
                                    UnitOfWorkAsync.SaveChanges();
                                }
                            }
                        }
                        while (importedId != 0);

                    }
                }
                else
                {

                    if (keyDetails.ImportedId > 0)
                    {
                        var importedId = keyDetails.ImportedId;
                        do
                        {
                            var variance = keyDetails.CurrentValue + keyDetails.ContributorValue;
                            var goalKeyDetails = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == importedId);
                            ////var obj = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                            ////var goalKeyDetails = await GetGoalKeyDetail(importedId);
                            if (goalKeyDetails == null)
                                break;
                            var contributorValue = goalKeyDetails.ContributorValue - variance;
                            goalKeyDetails.ContributorValue = contributorValue;

                            var sourceScore = (contributorValue + (goalKeyDetails.CurrentValue - goalKeyDetails.StartValue)) / Math.Round(goalKeyDetails.TargetValue - goalKeyDetails.StartValue) * 100;
                            if (sourceScore < 0)
                            {
                                goalKeyDetails.Score = 0;
                            }
                            else if (sourceScore > 100)
                            {
                                goalKeyDetails.Score = 100;
                            }
                            else
                            {
                                goalKeyDetails.Score = Math.Round(sourceScore);
                            }

                            var sourceProgress = commonService.GetProgressIdWithFormula(goalKeyDetails.DueDate, Convert.ToDateTime(currentCycle.CycleStartDate), Convert.ToDateTime(currentCycle.CycleEndDate), goalKeyDetails.Score, currentCycle.OrganisationCycleId);
                            goalKeyDetails.Progress = sourceProgress;
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

                            progressBarCalculationService.UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);

                            importedId = goalKeyDetails.ImportedId;
                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                var objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                                if (objectiveDetails != null)
                                {
                                    var objectiveKeyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.IsActive);

                                    objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                    goalObjectiveRepo.Update(objectiveDetails);
                                    UnitOfWorkAsync.SaveChanges();
                                }
                            }
                        }
                        while (importedId != 0);
                    }


                    ////if (currentCycle != null)
                    ////{
                    ////    await using var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand();
                    ////    command.CommandText = Constants.SpDeleteContributor + employeeId + Constants.Comma + goalKeyId + Constants.Comma + Constants.Quotation + currentCycle.CycleStartDate.ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + Constants.Quotation + Constants.Comma + Constants.Quotation + Convert.ToDateTime(currentCycle.CycleEndDate).ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + Constants.Quotation;
                    ////    command.CommandType = CommandType.Text;
                    ////    await OkrServiceDBContext.Database.OpenConnectionAsync();
                    ////    await command.ExecuteReaderAsync();
                    ////    await OkrServiceDBContext.Database.CloseConnectionAsync();
                    ////    await Task.Run(async () =>
                    ////    {
                    ////        await notificationService.DeleteKrNotifications(count, deleteKrList, getAllLevelContributorsList, (int)GoalType.GoalKey, goalKeyId, employeeId, jwtToken).ConfigureAwait(false);
                    ////    }).ConfigureAwait(false);
                    ////}
                }
                await Task.Run(async () =>
                {
                    await notificationService.DeleteKrNotifications(count, deleteKrList, getAllLevelContributorsList, (int)GoalType.GoalKey, goalKeyId, employeeId, jwtToken).ConfigureAwait(false);
                }).ConfigureAwait(false);

            }

            return result;
        }

        public async Task<bool> IsAnyOkr(long employeeId)
        {
            var isAnyOkr = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Draft);
            if (isAnyOkr != null)
                return true;
            else
                return false;
        }

        public async Task<string> DeleteOkr(long goalKeyId, long goalObjectiveId, UserIdentity loginUser)
        {
            var result = "";
            if (goalKeyId == 0)
            {
                var objectiveList = GetGoalObjective(goalObjectiveId);
                if (objectiveList != null)
                {
                    objectiveList.IsActive = false;
                    objectiveList.UpdatedOn = DateTime.UtcNow;
                    objectiveList.UpdatedBy = loginUser.EmployeeId;
                    await UpdateObjectiveAsync(objectiveList);
                }
                var keyList = GetGoalKeyByObjectiveId(goalObjectiveId);
                if (keyList != null)
                {
                    keyList.IsActive = false;
                    keyList.UpdatedOn = DateTime.UtcNow;
                    keyList.UpdatedBy = loginUser.EmployeeId;
                    await UpdateKeyResults(keyList);
                }
            }
            else
            {
                var keyResult = GetKeyFromKeyId(goalKeyId);
                if (keyResult != null)
                {
                    keyResult.IsActive = false;
                    keyResult.UpdatedOn = DateTime.UtcNow;
                    keyResult.UpdatedBy = loginUser.EmployeeId;
                    await UpdateKeyResults(keyResult);
                }
            }

            return result;
        }

        /// <summary>
        /// SQL to C# convert SP Name = sp_DeleteObjective
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="goalObjectiveId"></param>
        /// <param name="goalType"></param>
        /// <param name="userIdentity"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task<long> DeleteOkr(long employeeId, long goalObjectiveId, int goalType, UserIdentity userIdentity, string jwtToken)
        {
            var getAllKeyOfObjective = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive).ToList();
            foreach (var key in getAllKeyOfObjective)
            {
                await DeleteContributors(employeeId, key.GoalKeyId, userIdentity, jwtToken);
            }
            var goalObjective = await goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefaultAsync(x => x.GoalObjectiveId == goalObjectiveId);
            goalObjective.UpdatedBy = userIdentity.EmployeeId;
            goalObjective.UpdatedOn = DateTime.UtcNow;
            goalObjective.IsActive = false;
            await UpdateObjectiveAsync(goalObjective);
            return getAllKeyOfObjective.Count;
        }

        public async Task<bool> DeleteOkrKr(long employeeId, long goalObjectiveId, int goalType, UserIdentity userIdentity, string jwtToken)
        {
            var resultOutput = false;
            var getAllLevelContributorsOkrList = new List<long>();
            if (goalType == 1)
            {
                getAllLevelContributorsOkrList = commonService.GetAllLevelContributors((int)GoalType.GoalObjective, goalObjectiveId, employeeId);//// OKR Contributors for Notifications
            }

            ////var count = GetCount(goalObjectiveId);
            var currentCycle = await commonService.GetCurrentCycleAsync(userIdentity.OrganisationId);
            if (currentCycle != null)
            {
                ////var result = await DeleteOkrKr(employeeId, goalObjectiveId, currentCycle.CycleStartDate, Convert.ToDateTime(currentCycle.CycleEndDate));
                var result = await DeleteOkr(employeeId, goalObjectiveId, goalType, userIdentity, jwtToken);
                if (result > 0)
                {
                    if (goalType == 1)
                    {
                        await Task.Run(async () =>
                        {
                            await notificationService.DeleteOkrNotifications(getAllLevelContributorsOkrList, goalType, goalObjectiveId, employeeId, jwtToken).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                    ////else
                    ////{
                    ////    await Task.Run(async () =>
                    ////    {
                    ////        await notificationService.DeleteKrNotifications(count, result, getAllLevelContributorsList, goalType, goalObjectiveId, employeeId, jwtToken).ConfigureAwait(false);
                    ////    }).ConfigureAwait(false);
                    ////}

                }

                resultOutput = true;
            }

            return resultOutput;
        }

        public async Task<List<DeleteKrResponse>> DeleteOkrKr(long employeeId, long goalObjectiveId, DateTime cycleStartDate, DateTime cycleEndDate)
        {
            var deleteKrResponses = new List<DeleteKrResponse>();
            await using var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = Constants.SpDeleteObjective + employeeId + Constants.Comma + goalObjectiveId + Constants.Comma + Constants.Quotation + cycleStartDate.ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + Constants.Quotation + Constants.Comma + Constants.Quotation + cycleEndDate.ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + Constants.Quotation;
            command.CommandType = CommandType.Text;
            await OkrServiceDBContext.Database.OpenConnectionAsync();
            var dataReader = await command.ExecuteReaderAsync();
            while (await dataReader.ReadAsync())
            {
                var deleteKrResponse = new DeleteKrResponse
                {
                    GoalType = Convert.ToInt32(dataReader["GoalType"].ToString()),
                    GoalId = Convert.ToInt64(dataReader["GoalId"].ToString())
                };
                deleteKrResponses.Add(deleteKrResponse);
            }

            await OkrServiceDBContext.Database.CloseConnectionAsync();

            return deleteKrResponses;
        }

        public List<ContributorsResponse> GetContributorsByGoalTypeAndId(int goalType, long goalId, string token)
        {
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var contributorsResponse = commonService.GetContributor(goalType, goalId, allEmployee.Results);
            return contributorsResponse;
        }

        public async Task<string> UpdateKeyScore(KeyScoreUpdate keyScoreUpdate, UserIdentity userIdentity, string token)
        {
            var result = KeyScore(keyScoreUpdate, userIdentity.EmployeeId);

            await Task.Run(async () =>
            {
                await notificationService.KrUpdateNotifications(token, userIdentity.EmployeeId, keyScoreUpdate.GoalKeyId, keyScoreUpdate.GoalObjectiveId, keyScoreUpdate.GoalObjectiveProgress, keyScoreUpdate.GoalKeyProgress).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return result;
        }


        /// <summary>
        /// Adding OKR & Kr with and without contributor
        /// </summary>
        /// <param name="myGoalsRequests"></param>
        /// <param name="loginUser"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task<List<MyGoalsRequest>> InsertGoalObjective(List<MyGoalsRequest> myGoalsRequests, UserIdentity loginUser, string jwtToken)
        {
            var myGoals = new List<MyGoalsRequest>();
            foreach (var item in myGoalsRequests)
            {
                ////Inserting Team OKR
                if (item.GoalTypeId == 1)
                {
                    await InsertTeamOkr(item, loginUser, jwtToken);
                    myGoals.Add(item);
                }
                else
                {

                    var goalObjective = new GoalObjective();


                    ////Not imported normal objective with KR

                    ////GoalStatusId will be 2 for public
                    ////GoalTypeId will be 2 for individual

                    var isObjectiveImported = IsObjectiveImported(item.ImportedId, item.EmployeeId);
                    if (!isObjectiveImported)
                    {
                        goalObjective = Mapper.Map<GoalObjective>(item);
                        goalObjective.CreatedBy = loginUser.EmployeeId;
                        goalObjective.Progress = (int)ProgressMaster.NotStarted;
                        goalObjective.Sequence = (int)GoalRequest.Sequence;
                        goalObjective.GoalStatusId = item.GoalStatusId;
                        goalObjective.GoalTypeId = item.GoalTypeId;
                        goalObjective.EmployeeId = (item.GoalStatusId == (int)GoalStatus.Public && item.ImportedId == 0)
                            ? item.Owner == 0 ? item.EmployeeId : item.Owner
                            : item.EmployeeId;
                        goalObjective.Owner = item.Owner;
                        goalObjective.LinkedObjectiveId = item.LinkedObjectiveId;
                        await InsertObjective(goalObjective);

                        item.GoalObjectiveId = goalObjective.GoalObjectiveId;

                        if (goalObjective.ImportedType == 1 && goalObjective.ImportedId > 0)
                        {
                            await Task.Run(async () =>
                            {
                                await notificationService.AlignObjNotifications(jwtToken, loginUser.EmployeeId, goalObjective.ImportedType, goalObjective.ImportedId).ConfigureAwait(false);
                            }).ConfigureAwait(false);
                        }

                        ////if (item.LinkedObjectiveId > 0 && goalObjective.GoalStatusId == (int)GoalStatus.Public)
                        ////{
                        ////    var linkedObjectiveDetail = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == item.LinkedObjectiveId && x.IsActive);
                        ////    if (linkedObjectiveDetail != null)
                        ////    {
                        ////        await Task.Run(async () =>
                        ////        {
                        ////            await notificationService.VirtualLinkingNotifications(linkedObjectiveDetail.EmployeeId, loginUser, jwtToken).ConfigureAwait(false);
                        ////        }).ConfigureAwait(false);
                        ////    }
                        ////}
                    }
                    else
                    {
                        goalObjective = GetGoalObjectiveByImportedId(item.ImportedId, item.EmployeeId);
                    }

                    ////Kr creation when no contributors assigned
                    foreach (var data in item.MyGoalsDetails)
                    {
                        var isAlreadyAligned = false;
                        if (data.ImportedId != 0)
                        {
                            isAlreadyAligned = IsKrAlreadyAligned(data.ImportedId, goalObjective.EmployeeId);
                        }

                        if (!isAlreadyAligned)
                        {
                            var dueDate = data.DueDate > goalObjective.EndDate ? goalObjective.EndDate : data.DueDate;
                            var goalKey = await InsertKeyResults(dueDate, goalObjective, loginUser.EmployeeId, data, loginUser, jwtToken);

                            if (data.Contributors.Any())
                            {
                                ////Adding objective for Contributor if someone wants to assign KR with parent objective
                                await SeparationAddObjForContributor_InsertGoalObjective(data.Contributors, goalObjective, loginUser, jwtToken);

                                ////Adding key for login user and his contributor 
                                await SeparationAddKeyForContributor_InsertGoalObjective(data.Contributors, goalObjective, goalKey, loginUser, jwtToken);
                            }
                        }
                    }

                    myGoals.Add(item);
                }
            }


            return myGoals;
        }

        public bool IsKrAlreadyAligned(long importedId, long empId)
        {
            var goalObjectiveDetail = goalKeyRepo.GetQueryable().Count(x => x.EmployeeId == empId && x.ImportedId == importedId && x.ImportedType == 2 && x.EmployeeId == empId && x.IsActive);
            if (goalObjectiveDetail > 0)
            {
                return true; ////already there is one objective with same importedId
            }
            return false;
        }

        public async Task<string> LockGoals(UnLockRequest unLockRequest, string jwtToken)
        {
            var unLockLog = new UnLockLog
            {
                EmployeeId = unLockRequest.EmployeeId,
                Year = unLockRequest.Year,
                Cycle = unLockRequest.Cycle,
                CreatedBy = unLockRequest.EmployeeId,
                Status = 1,
                LockedOn = DateTime.UtcNow,
                LockedTill = DateTime.UtcNow
            };
            await SaveUnlockLog(unLockLog);

            var result = "";

            await Task.Run(async () =>
            {
                await notificationService.LockGoalsNotifications(unLockRequest.EmployeeId, jwtToken).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return result;
        }

        public async Task<bool> IsAlreadyRequestedAsync(UnLockRequest unLockRequest)
        {
            var unlockData = await GetUnLockLogAsync(unLockRequest);
            return (unlockData != null);
        }

        public MyGoalDetailResponse MyGoalDetailResponse(int type, long typeId, string token)
        {
            var myGoalDetailResponse = new MyGoalDetailResponse();
            if (type == 1)
            {
                var goalObjResult = GetGoalObjective(typeId);
                if (goalObjResult != null)
                {
                    var teamName = string.Empty;
                    if (goalObjResult.TeamId > 0)
                    {
                        var teamDetails = commonService.GetTeamEmployeeByTeamId(goalObjResult.TeamId, token);
                        teamName = teamDetails == null ? "" : teamDetails.OrganisationName;
                    }
                    myGoalDetailResponse.GoalObjectiveId = goalObjResult.GoalObjectiveId;
                    myGoalDetailResponse.ObjectiveDescription = goalObjResult.ObjectiveDescription;
                    myGoalDetailResponse.ObjectiveName = goalObjResult.ObjectiveName;
                    myGoalDetailResponse.EmployeeId = goalObjResult.EmployeeId;
                    var keyResult = GetGoalKey(goalObjResult.GoalObjectiveId);
                    myGoalDetailResponse.KeyDetails = Mapper.Map<List<GoalKeyDetails>>(keyResult);
                    myGoalDetailResponse.TeamId = goalObjResult.TeamId;
                    myGoalDetailResponse.TeamName = teamName;
                }
            }
            else
            {
                var requestKey = GetKeyFromKeyId(typeId);
                if (requestKey != null)
                {
                    var teamName = string.Empty;
                    if (requestKey.TeamId > 0)
                    {
                        var teamDetails = commonService.GetTeamEmployeeByTeamId(requestKey.TeamId, token);
                        teamName = teamDetails == null ? "" : teamDetails.OrganisationName;
                    }
                    var objResult = GetGoalObjective(requestKey.GoalObjectiveId);
                    if (objResult != null)
                    {
                        myGoalDetailResponse.GoalObjectiveId = objResult.GoalObjectiveId;
                        myGoalDetailResponse.ObjectiveDescription = objResult.ObjectiveDescription;
                        myGoalDetailResponse.ObjectiveName = objResult.ObjectiveName;
                        myGoalDetailResponse.EmployeeId = objResult.EmployeeId;
                    }
                    myGoalDetailResponse.KeyDetails.Add(Mapper.Map<GoalKeyDetails>(requestKey));
                    myGoalDetailResponse.TeamId = requestKey.TeamId;
                    myGoalDetailResponse.TeamName = teamName;
                }
            }

            return myGoalDetailResponse;
        }

        public async Task<UnLockLog> UnLockGoal(UnLockRequest unLockRequest, UserIdentity userIdentity)
        {
            var unLockLog = new UnLockLog();
            var data = UnlockLog().FirstOrDefault(x => x.IsActive && x.LockedTill > DateTime.UtcNow && x.EmployeeId == unLockRequest.EmployeeId && x.Cycle == unLockRequest.Cycle && x.Year == unLockRequest.Year && x.Status == 1);
            if (data != null)
            {
                data.LockedOn = DateTime.UtcNow;
                data.LockedTill = DateTime.UtcNow.AddHours(48);
                data.Status = 2;
                unLockLog = await UpdateUnlockLog(data);
            }
            else
            {
                unLockLog.EmployeeId = unLockRequest.EmployeeId;
                unLockLog.Year = unLockRequest.Year;
                unLockLog.Cycle = unLockRequest.Cycle;
                unLockLog.CreatedBy = userIdentity.EmployeeId;
                unLockLog.Status = 2;
                unLockLog.LockedOn = DateTime.UtcNow;
                unLockLog.LockedTill = DateTime.UtcNow.AddHours(48);
                unLockLog = await SaveUnlockLog(unLockLog);
            }

            return unLockLog;
        }

        public List<UnLockLog> UnlockLog()
        {
            return unlockLogRepo.GetQueryable().Where(x => x.IsActive).ToList();
        }

        public async Task<long> BulkUnlockApprove(List<UnLockRequest> unLockRequest, UserIdentity userIdentity, string jwtToken)
        {
            long unlockCounter = 0;

            foreach (var item in unLockRequest)
            {
                var previousLog = await unlockLogRepo.GetQueryable().AsNoTracking().Where(x => x.IsActive && x.EmployeeId == item.EmployeeId && x.Status == 2).ToListAsync();
                foreach (var logData in previousLog)
                {
                    logData.IsActive = false;
                    await UpdateUnlockLog(logData);
                }
                var unLockLog = new UnLockLog();
                var data = UnlockLog().FirstOrDefault(x => x.IsActive && x.LockedTill > DateTime.UtcNow && x.EmployeeId == item.EmployeeId && x.Cycle == item.Cycle && x.Year == item.Year && x.Status == 1); //this scenario is useless because even if dateTime will be greater but status will never be 1 in that case
                if (data != null)
                {
                    data.LockedOn = DateTime.UtcNow;
                    data.LockedTill = DateTime.UtcNow.AddHours(48);
                    data.Status = 2;
                    await UpdateUnlockLog(data);
                    unlockCounter++;
                }
                else
                {
                    var prevApprovedUnLockLog = UnlockLog().FirstOrDefault(x => x.EmployeeId == item.EmployeeId && x.LockedTill >= DateTime.UtcNow && x.IsActive);//Every time new record will be added for the approval
                    if (prevApprovedUnLockLog == null)
                    {
                        unLockLog.EmployeeId = item.EmployeeId;
                        unLockLog.Year = item.Year;
                        unLockLog.Cycle = item.Cycle;
                        unLockLog.CreatedBy = item.EmployeeId;
                        unLockLog.Status = 2;
                        unLockLog.LockedOn = DateTime.UtcNow;
                        unLockLog.LockedTill = DateTime.UtcNow.AddHours(48);
                        unLockLog = await SaveUnlockLog(unLockLog);
                        if (unLockLog != null)
                        {
                            var users = commonService.GetAllUserFromUsers(jwtToken);

                            await Task.Run(async () =>
                            {
                                await notificationService.BulkUnlockApproveNotificationsAndEmails(users, item, jwtToken).ConfigureAwait(false);
                            }).ConfigureAwait(false);
                        }
                        unlockCounter++;
                    }
                }
            }

            return unlockCounter;
        }

        public AlignStatusResponse AlignStatus(long employeeId, int sourceType, long sourceId)
        {
            if (sourceType == (int)GoalType.GoalKey)
            {
                return commonService.GetGoalKeySource(employeeId, sourceId).Result;
            }
            else
            {
                return commonService.GetGoalObjectiveSource(employeeId, sourceId).Result;
            }
        }

        public async Task<MyGoalsPdfResponse> DownloadPDf(long empId, int cycle, int year, string token, UserIdentity loginUser)
        {
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(loginUser.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            var myGoalsPdfResponse = new MyGoalsPdfResponse();
            var allEmployee = commonService.GetAllUserFromUsers(token).Results;
            var employeeDetails = allEmployee.FirstOrDefault(x => x.EmployeeId == empId);
            var pdfOkrResponses = new List<PdfOkrResponse>();
            var pdfUniqueContributors = new List<PdfUniqueContributorsResponse>();
            var keyCount = 0;
            var progressList = new List<int>();


            var objectives = GetEmployeeOkrByCycleIdForDownloadPdf(empId, cycle, year);
            if (employeeDetails != null && objectives.Count > 0 && quarterDetails != null)
            {
                foreach (var obj in objectives)
                {
                    var objectivesContributors = commonService.GetContributor(1, obj.GoalObjectiveId, allEmployee).ToList();
                    foreach (var item in objectivesContributors)
                    {
                        pdfUniqueContributors.Add(new PdfUniqueContributorsResponse
                        {
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            ImagePath = item.ImagePath,
                            EmployeeId = item.EmployeeId,
                        });
                    }
                    var pdfGoalKeyResponses = new List<PdfGoalKeyResponse>();
                    var keyDetail = GetGoalKey(obj.GoalObjectiveId);

                    foreach (var key in keyDetail)
                    {
                        keyCount += 1;
                        pdfGoalKeyResponses.Add(new PdfGoalKeyResponse
                        {
                            DueDate = key.DueDate,
                            Score = key.Score,
                            KeyDescription = key.KeyDescription,
                            Contributors = commonService.GetContributorForPdf(2, key.GoalKeyId, allEmployee)
                        });
                    }

                    pdfOkrResponses.Add(new PdfOkrResponse
                    {
                        pdfGoalKeyResponses = pdfGoalKeyResponses,
                        ObjectiveName = obj.ObjectiveName,
                        Score = obj.Score,
                        StartDate = obj.StartDate,
                        EndDate = obj.EndDate
                    });

                    progressList.Add(commonService.GetProgressIdWithFormula(obj.EndDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), obj.Score, cycleDurationDetails.CycleDurationId));

                }

                myGoalsPdfResponse.KeyCount = keyCount;
                myGoalsPdfResponse.pdfOkrResponses = pdfOkrResponses;
                myGoalsPdfResponse.OkrCount = objectives.Count;
                myGoalsPdfResponse.Name = employeeDetails.FirstName + " " + employeeDetails.LastName ?? "";
                myGoalsPdfResponse.ImagePath = employeeDetails.ImagePath ?? "";
                myGoalsPdfResponse.MyGoalsPeriod = quarterDetails.Symbol + " - " + year;
                myGoalsPdfResponse.AverageScore = Math.Round(objectives.Select(x => x.Score).Average());
                myGoalsPdfResponse.NotStarted = progressList.Count(x => x == 1);
                myGoalsPdfResponse.AtRisk = progressList.Count(x => x == 2);
                myGoalsPdfResponse.Lagging = progressList.Count(x => x == 3);
                myGoalsPdfResponse.OnTrack = progressList.Count(x => x == 4);
                myGoalsPdfResponse.Team = "";
                myGoalsPdfResponse.Department = employeeDetails.OrganisationName ?? "";
                myGoalsPdfResponse.Designation = employeeDetails.Designation ?? "";

                var distinctObjectivesContributors = pdfUniqueContributors.GroupBy(x => x.EmployeeId).Select(x => x.FirstOrDefault()).ToList();
                myGoalsPdfResponse.pdfUniqueContributorsResponses = distinctObjectivesContributors;
            }

            return myGoalsPdfResponse;
        }

        public GoalObjective GetGoalObjective(long goalObjectiveId)
        {
            return goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive);
        }

        public List<GoalObjective> GetObjectiveContributor(int goalType, long goalId)
        {
            return commonService.GetObjectiveContributor(goalType, goalId);
        }

        public async Task<GoalObjective> InsertObjective(GoalObjective goalObjective)
        {
            goalObjectiveRepo.Add(goalObjective);
            await UnitOfWorkAsync.SaveChangesAsync();
            return goalObjective;
        }

        public GoalObjective InsertObjectiveNonAsync(GoalObjective goalObjective)
        {
            goalObjectiveRepo.Add(goalObjective);
            UnitOfWorkAsync.SaveChanges();
            return goalObjective;
        }

        public async Task<GoalKey> InsertKeyResults(GoalKey goalKey)
        {
            goalKeyRepo.Add(goalKey);
            await UnitOfWorkAsync.SaveChangesAsync();
            return goalKey;
        }

        public GoalKey InsertKeyResultNonAsync(GoalKey goalKey)
        {
            goalKeyRepo.Add(goalKey);
            UnitOfWorkAsync.SaveChanges();
            return goalKey;
        }

        public async Task<KrStatusMessage> InsertMessagesOfKr(KrStatusMessage krStatusMessage)
        {
            krStatusMessageRepo.Add(krStatusMessage);
            await UnitOfWorkAsync.SaveChangesAsync();
            return krStatusMessage;
        }

        public async Task<KrStatusMessage> UpdateMessagesOfKr(KrStatusMessage krStatusMessage)
        {
            krStatusMessageRepo.Update(krStatusMessage);
            await UnitOfWorkAsync.SaveChangesAsync();
            return krStatusMessage;
        }

        public async Task<GoalKey> UpdateKeyResults(GoalKey goalKey)
        {
            goalKeyRepo.Update(goalKey);
            await UnitOfWorkAsync.SaveChangesAsync();
            return goalKey;
        }

        public GoalKey UpdateKeyResultNonAsync(GoalKey goalKey)
        {
            goalKeyRepo.Update(goalKey);
            UnitOfWorkAsync.SaveChanges();
            return goalKey;
        }

        public GoalKey GetGoalKeyByObjectiveId(long goalObjectiveId)
        {
            return goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive);
        }

        public GoalKey GetKeyFromKeyId(long goalKeyId)
        {
            return goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalKeyId && x.IsActive);
        }

        public string GetKeyDesc(long goalKeyId)
        {
            var result = "";
            result = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalKeyId && x.IsActive).KeyDescription;
            return result;
        }

        public List<GoalObjective> GetEmployeeOkrByCycleId(long empId, int cycleId, int year)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive && x.Year == year).ToList();
        }

        public List<GoalObjective> GetEmployeeOkrByCycleIdForDownloadPdf(long empId, int cycleId, int year)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive && x.Year == year).OrderByDescending(x => x.GoalObjectiveId).OrderBy(x => x.Sequence).ToList();
        }

        public GoalKey GetGoalKeyDetails(long goalKeyId)
        {
            return goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == goalKeyId && x.IsActive);
        }

        public List<GoalKey> GetGoalKey(long goalObjectiveId)
        {
            return goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive).ToList();
        }

        public int GetKeyCount(long goalObjectiveId)
        {
            var result = 0;
            result = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId).Count();
            return result;
        }

        public string KeyScore(KeyScoreUpdate keyScoreUpdate, long loginEmpId)
        {
            var result = "";
            using (var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC sp_KeyScore " + keyScoreUpdate.GoalObjectiveId + "," + keyScoreUpdate.GoalObjectiveScore + "," + keyScoreUpdate.GoalObjectiveProgress + "," + keyScoreUpdate.GoalKeyId + "," + keyScoreUpdate.GoalKeyScore + "," + keyScoreUpdate.GoalKeyProgress + "," + loginEmpId;
                command.CommandType = CommandType.Text;
                OkrServiceDBContext.Database.OpenConnection();
                command.ExecuteReader();
                OkrServiceDBContext.Database.CloseConnection();
            }

            return result;
        }

        public GoalObjective GetChildObjective(long empId, long goalObjectiveId)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == empId && x.ImportedId == goalObjectiveId && x.ImportedType == 1 && x.IsActive);
        }

        public bool IsKeyImported(long goalKeyId, long empId)
        {
            var goalKeyDetail = goalKeyRepo.GetQueryable().Count(x => x.ImportedType == 2 && x.ImportedId == goalKeyId && x.IsActive);
            return goalKeyDetail != 0;
        }

        public bool IsObjectiveImported(long goalObjectiveId, long empId)
        {
            var goalObjectiveDetail = goalObjectiveRepo.GetQueryable().Count(x => x.ImportedType == 1 && x.ImportedId == goalObjectiveId && x.EmployeeId == empId && x.IsActive);
            return goalObjectiveDetail != 0;
        }

        public bool IsKeyImportedGoal(long goalKeyId, long empId)
        {
            var goalKeyDetail = goalKeyRepo.GetQueryable().Count(x => x.ImportedType == 2 && x.ImportedId == goalKeyId && x.EmployeeId == empId && x.IsActive);
            return goalKeyDetail != 0;
        }

        public GoalObjective DeleteGoalObjective(int importedType, long importedId, long empId)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ImportedId == importedId && x.ImportedType == importedType && x.EmployeeId == empId);
        }

        public List<GoalObjective> GetAssignedObjectiveByGoalObjectiveId(long goalObjId, int cycleId, int year)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.ImportedId == goalObjId && x.IsActive && x.Year == year).ToList();
        }

        public List<GoalObjective> GetAssignedObjectiveByImportId(long importId, int cycleId, int year)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.GoalObjectiveId == importId && x.IsActive && x.Year == year).ToList();
        }

        public List<GoalObjective> GetAssignedAlignedObjective(long goalId, int cycleId, int year, bool isAlign)
        {
            if (isAlign)
            {
                return GetAssignedObjectiveByImportId(goalId, cycleId, year);
            }
            else
            {
                return GetAssignedObjectiveByGoalObjectiveId(goalId, cycleId, year);
            }
        }

        public async Task<UnLockLog> SaveUnlockLog(UnLockLog unLockLog)
        {
            unlockLogRepo.Add(unLockLog);
            await UnitOfWorkAsync.SaveChangesAsync();
            return unLockLog;
        }

        public async Task<UnLockLog> UpdateUnlockLog(UnLockLog unLockLog)
        {
            unlockLogRepo.Update(unLockLog);
            await UnitOfWorkAsync.SaveChangesAsync();
            return unLockLog;
        }

        public async Task<UnLockLog> GetUnLockLogAsync(UnLockRequest unLockRequest)
        {
            return await unlockLogRepo.GetQueryable().FirstOrDefaultAsync(x => x.IsActive && x.EmployeeId == unLockRequest.EmployeeId && x.Cycle == unLockRequest.Cycle && x.Year == unLockRequest.Year && x.Status == 1);
        }

        public GoalKey GetImportedKey(long importedId)
        {
            return goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == importedId);
        }

        public UnLockLog GetUnLockLogDetails(long empId, int year, int cycle)
        {
            return unlockLogRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == empId && x.Year == year && x.Cycle == cycle && x.Status == 2 && x.LockedTill >= DateTime.UtcNow);
        }

        public List<GoalObjective> GetObjectiveSource(long goalObjectiveId)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.Source == goalObjectiveId && !x.IsActive).ToList();
        }

        public GoalObjective GetGoalObjectiveByImportedId(long importedId, long empId)
        {
            var goalObjectiveDetail = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ImportedType == 1 && x.ImportedId == importedId && x.EmployeeId == empId && x.IsActive);
            return goalObjectiveDetail;
        }

        public int GetCountOfKeyResult(long goalId)
        {
            return goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalId && !x.IsActive).Count();
        }

        public int GetCount(long goalId)
        {
            var count = 0;
            var result = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalId && x.IsActive)?.GoalObjectiveId;
            if (result != null)
            {
                count = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == result && x.IsActive).Count();
            }

            return count;
        }

        public async Task<bool> UpdateOkrSequence(List<UpdateSequenceRequest> updateSequenceRequests, UserIdentity userIdentity)
        {
            var result = false;
            foreach (var item in updateSequenceRequests)
            {
                var obj = GetGoalSequence(item, userIdentity.EmployeeId);
                if (obj != null)
                {
                    obj.Sequence = item.Sequence;
                    obj.UpdatedOn = DateTime.UtcNow;
                    await UpdateGoalSequence(obj);
                    result = true;
                }
                else
                {
                    var newGoalSequence = new GoalSequence()
                    {
                        SequenceId = 0,
                        GoalType = item.GoalType,
                        GoalCycleId = item.GoalCycleId,
                        GoalId = item.GoalId,
                        EmployeeId = userIdentity.EmployeeId,
                        Sequence = item.Sequence,
                        UpdatedOn = DateTime.UtcNow,
                        IsActive = true,
                    };
                    await InsertGoalSequence(newGoalSequence);
                    result = true;
                }
            }

            return result;
        }

        public List<KrStatusContributorResponse> GetKrStatusContributors(int goalType, long goalKeyId, string token)
        {
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var contributorsResponse = commonService.GetKrStatusContributor(goalType, goalKeyId, allEmployee.Results);
            return contributorsResponse;
        }

        public async Task<GoalObjectiveResponse> GetGoalByGoalObjectiveId(long goalObjectiveId)
        {
            var goalObjectiveResponse = new GoalObjectiveResponse();
            var objectiveDetail = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == goalObjectiveId);
            if (objectiveDetail != null)
            {
                goalObjectiveResponse = Mapper.Map<GoalObjectiveResponse>(objectiveDetail);
            }

            return goalObjectiveResponse;
        }

        public async Task<long> UpdateGoalObjective(UpdateGoalRequest updateGoalRequest, UserIdentity userIdentity, string token)
        {
            if (updateGoalRequest.GoalObjectiveId > 0)
            {
                var objectiveDetail = await goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefaultAsync(x => x.GoalObjectiveId == updateGoalRequest.GoalObjectiveId && x.IsActive);
                if (objectiveDetail != null)
                {
                    ////var oldLinkedObjectiveId = objectiveDetail.LinkedObjectiveId;
                    objectiveDetail.ObjectiveName = updateGoalRequest.ObjectiveName;
                    objectiveDetail.ObjectiveDescription = updateGoalRequest.ObjectiveDescription;
                    objectiveDetail.Score = updateGoalRequest.Score;
                    objectiveDetail.Progress = updateGoalRequest.Progress;
                    objectiveDetail.EmployeeId = (objectiveDetail.GoalStatusId == (int)GoalStatus.Public && objectiveDetail.ImportedId == 0)
                        ? updateGoalRequest.Owner == 0 ? objectiveDetail.EmployeeId : updateGoalRequest.Owner
                        : objectiveDetail.EmployeeId;

                    if (updateGoalRequest.StartDate > objectiveDetail.StartDate || updateGoalRequest.StartDate < objectiveDetail.StartDate)
                    {
                        objectiveDetail.StartDate = updateGoalRequest.StartDate;
                    }

                    if (updateGoalRequest.EndDate < objectiveDetail.EndDate || updateGoalRequest.EndDate > objectiveDetail.EndDate)
                    {
                        objectiveDetail.EndDate = updateGoalRequest.EndDate;
                    }

                    objectiveDetail.IsPrivate = updateGoalRequest.IsPrivate;
                    objectiveDetail.LinkedObjectiveId = updateGoalRequest.LinkedObjectiveId;
                    objectiveDetail.GoalTypeId = updateGoalRequest.GoalTypeId;
                    objectiveDetail.TeamId = updateGoalRequest.TeamId;
                    objectiveDetail.UpdatedBy = userIdentity.EmployeeId;
                    objectiveDetail.UpdatedOn = DateTime.UtcNow;
                    objectiveDetail.Owner = updateGoalRequest.Owner;
                    objectiveDetail.IsCoachCreation = objectiveDetail.GoalStatusId == (int)GoalStatus.Draft && updateGoalRequest.IsCoach;
                    await UpdateObjectiveAsync(objectiveDetail);

                    var goalKeyDetails = await goalKeyRepo.GetQueryable().AsTracking().Where(x => x.GoalObjectiveId == objectiveDetail.GoalObjectiveId).ToListAsync();
                    foreach (var item in goalKeyDetails)
                    {
                        if (item.DueDate > objectiveDetail.EndDate)
                        {
                            item.DueDate = updateGoalRequest.EndDate;
                        }

                        if (item.StartDate < objectiveDetail.StartDate)
                        {
                            item.StartDate = updateGoalRequest.StartDate;
                        }

                        item.EmployeeId = objectiveDetail.IsCoachCreation ? objectiveDetail.Owner : objectiveDetail.EmployeeId;
                        item.Owner = objectiveDetail.Owner;
                        item.TeamId = objectiveDetail.TeamId;
                        item.UpdatedBy = userIdentity.EmployeeId;
                        item.UpdatedOn = DateTime.UtcNow;
                        await UpdateKeyResults(item);
                    }
                    var contributorId = new List<long>();
                    var contributors = await goalObjectiveRepo.GetQueryable().AsTracking().Where(x => x.ImportedId == updateGoalRequest.GoalObjectiveId && x.IsActive).ToListAsync();
                    foreach (var contributor in contributors)
                    {
                        contributor.ObjectiveName = updateGoalRequest.ObjectiveName;
                        contributor.ObjectiveDescription = updateGoalRequest.ObjectiveDescription;
                        contributor.Score = updateGoalRequest.Score;
                        contributor.Progress = updateGoalRequest.Progress;
                        if (updateGoalRequest.StartDate > contributor.StartDate || updateGoalRequest.StartDate < contributor.StartDate)
                        {
                            contributor.StartDate = updateGoalRequest.StartDate;
                        }
                        if (updateGoalRequest.EndDate < contributor.EndDate || updateGoalRequest.EndDate > contributor.EndDate)
                        {
                            contributor.EndDate = updateGoalRequest.EndDate;
                        }
                        contributor.IsPrivate = updateGoalRequest.IsPrivate;
                        contributor.UpdatedBy = userIdentity.EmployeeId;
                        contributor.UpdatedOn = DateTime.UtcNow;
                        contributor.Owner = updateGoalRequest.Owner;
                        await UpdateObjectiveAsync(contributor);

                        var contributorKeyDetails = await goalKeyRepo.GetQueryable().AsTracking().Where(x => x.GoalObjectiveId == contributor.GoalObjectiveId).ToListAsync();
                        foreach (var key in contributorKeyDetails)
                        {
                            if (key.DueDate > objectiveDetail.EndDate)
                            {
                                key.DueDate = updateGoalRequest.EndDate;
                            }

                            if (key.StartDate < objectiveDetail.StartDate)
                            {
                                key.StartDate = updateGoalRequest.StartDate;
                            }

                            key.UpdatedBy = userIdentity.EmployeeId;
                            key.UpdatedOn = DateTime.UtcNow;
                            key.Owner = contributor.Owner;
                            await UpdateKeyResults(key);
                        }
                        contributorId.Add(contributor.EmployeeId);
                    }

                    var updateDueDate = new UpdateDueDateRequest()
                    {
                        StartDate = updateGoalRequest.StartDate,
                        GoalType = Constants.GoalObjectiveType,
                        EndDate = updateGoalRequest.EndDate,
                        GoalId = updateGoalRequest.GoalObjectiveId
                    };
                    await UpdateOkrDueDate(updateDueDate, userIdentity, token, true, objectiveDetail, goalKeyDetails, false, contributorId);
                }
            }

            return 1;
        }

        public async Task<List<DueDateResponse>> UpdateDueDateAlignment(UpdateDueDateRequest updateDueDateRequest, UserIdentity userIdentity, string jwtToken, bool IsNotifications)
        {
            var contributorId = new List<long>();
            if (updateDueDateRequest.GoalType == 1)
            {
                var goalObjective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == updateDueDateRequest.GoalId && x.IsActive);
                var goalKeys = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == updateDueDateRequest.GoalId && x.IsActive).ToList();
                return await UpdateOkrDueDate(updateDueDateRequest, userIdentity, jwtToken, IsNotifications, goalObjective, goalKeys, true, contributorId);
            }
            else
            {
                return await UpdateKrDueDate(updateDueDateRequest, userIdentity, jwtToken, IsNotifications, contributorId, true, updateDueDateRequest.StartDate, updateDueDateRequest.EndDate);
            }


        }
        public async Task<KeyDetailsResponse> GetKeyDetails(long goalKeyId, string token)
        {
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var goalKey = GetGoalKeyDetails(goalKeyId);
            var keyDetails = new KeyDetailsResponse();
            if (goalKey != null)
            {
                var keyDetailsResponse = new KeyDetailsResponse
                {
                    GoalKeyId = goalKeyId,
                    KeyDescription = goalKey.KeyDescription,
                    ObjectiveId = goalKey.GoalObjectiveId,
                    Score = goalKey.Score,
                    StartDate = goalKey.StartDate,
                    DueDate = goalKey.DueDate,
                    MetricId = goalKey.MetricId,
                    AssignmentTypeId = goalKey.AssignmentTypeId,
                    CurrentValue = goalKey.CurrentValue,
                    TargetValue = goalKey.TargetValue,
                    CurrencyId = goalKey.CurrencyId,
                    Progress = goalKey.Progress,
                    KrStatusId = goalKey.KrStatusId,
                    CurrencyCode = goalKey.CurrencyCode,
                    GoalStatusId = goalKey.GoalStatusId,
                    StartValue = goalKey.StartValue,
                    ImportedId = goalKey.ImportedId,
                    ImportedType = goalKey.ImportedType,
                    Contributors = await commonService.GetKRContributorAsync((int)GoalType.GoalKey, goalKeyId, allEmployee.Results, goalKey.TargetValue),

                };
                keyDetails = keyDetailsResponse;
            }

            return keyDetails;
        }

        /// <summary>
        /// API to insert contributor which will be called in case of change Contributor
        /// </summary>
        /// <param name="contributorDetails"></param>
        /// <param name="jwtToken"></param>
        /// <param name="loginUser"></param>
        /// <returns></returns>
        public async Task<ContributorDetailRequest> AddContributor(ContributorDetailRequest contributorDetails, UserIdentity loginUser, string jwtToken)
        {
            var goalObjectiveId = GetKeyFromKeyId(contributorDetails.ImportedId).GoalObjectiveId;

            if (goalObjectiveId != 0 && contributorDetails.ImportedId > 0 && contributorDetails.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
            {
                var getGoalObjectiveDetails = GetGoalObjective(goalObjectiveId);


                if (getGoalObjectiveDetails != null)
                {
                    var goalObjective = new GoalObjective()
                    {
                        EmployeeId = contributorDetails.EmployeeId,
                        IsPrivate = getGoalObjectiveDetails.IsPrivate,
                        ObjectiveName = contributorDetails.ObjectiveName,
                        ////  ObjectiveDescription = getGoalObjectiveDetails.ObjectiveDescription,
                        ObjectiveCycleId = getGoalObjectiveDetails.ObjectiveCycleId,
                        StartDate = getGoalObjectiveDetails.StartDate,
                        EndDate = getGoalObjectiveDetails.EndDate,
                        ImportedType = getGoalObjectiveDetails.ImportedType,
                        ImportedId = getGoalObjectiveDetails.GoalObjectiveId,
                        Score = getGoalObjectiveDetails.Score,
                        CreatedBy = loginUser.EmployeeId,
                        CreatedOn = DateTime.Now,
                        Year = getGoalObjectiveDetails.Year,
                        Progress = (int)ProgressMaster.NotStarted,
                        Source = GetSourceId(getGoalObjectiveDetails.ImportedId, getGoalObjectiveDetails.ImportedType),
                        GoalStatusId = getGoalObjectiveDetails.GoalStatusId,
                        GoalTypeId = getGoalObjectiveDetails.GoalTypeId
                    };

                    await InsertObjective(goalObjective);

                    var goalKeyDetail = Mapper.Map<GoalKey>(contributorDetails);
                    goalKeyDetail.Source = GetSourceId(contributorDetails.ImportedId, contributorDetails.ImportedType);
                    goalKeyDetail.CreatedBy = loginUser.EmployeeId;
                    goalKeyDetail.GoalObjectiveId = goalObjective.GoalObjectiveId;
                    goalKeyDetail.CreatedOn = DateTime.Now;
                    goalKeyDetail.Progress = (int)ProgressMaster.NotStarted;

                    await InsertKeyResults(goalKeyDetail);
                    contributorDetails.Source = goalKeyDetail.Source;

                    var updateKrValue = new KrValueUpdate
                    {
                        GoalKeyId = goalKeyDetail.GoalKeyId,
                        CurrentValue = goalKeyDetail.CurrentValue,
                        Year = goalObjective.Year
                    };

                    await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goalKeyDetail);

                    var krStatusMessage = new KrStatusMessage
                    {
                        AssignerGoalKeyId = contributorDetails.ImportedId,
                        AssigneeGoalKeyId = goalKeyDetail.GoalKeyId,
                        KrAssignerMessage = contributorDetails.KrAssignerMessage,
                        CreatedOnAssigner = DateTime.Now,
                        CreatedOnAssignee = DateTime.Now,
                        IsActive = true
                    };
                    await InsertMessagesOfKr(krStatusMessage);
                }
            }

            else
            {
                var goalKey = Mapper.Map<GoalKey>(contributorDetails);
                goalKey.Source = GetSourceId(contributorDetails.ImportedId, contributorDetails.ImportedType);
                goalKey.CreatedBy = loginUser.EmployeeId;
                goalKey.CreatedOn = DateTime.Now;
                goalKey.IsActive = true;
                goalKey.Progress = (int)ProgressMaster.NotStarted;

                await InsertKeyResults(goalKey);
                contributorDetails.Source = goalKey.Source;

                var krStatusMessage = new KrStatusMessage
                {
                    AssignerGoalKeyId = contributorDetails.ImportedId,
                    AssigneeGoalKeyId = goalKey.GoalKeyId,
                    KrAssignerMessage = contributorDetails.KrAssignerMessage,
                    CreatedOnAssigner = DateTime.Now,
                    CreatedOnAssignee = DateTime.Now,
                    IsActive = true

                };
                await InsertMessagesOfKr(krStatusMessage);
            }
            return contributorDetails;
        }

        ////public async Task UpdateKrValue(KrValueUpdate krValueUpdate, UserIdentity userIdentity, string token)
        ////{
        ////    var keyDetails = await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalKeyId == krValueUpdate.GoalKeyId);
        ////    var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(userIdentity.OrganisationId, token);
        ////    CycleDetails cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == krValueUpdate.Year);
        ////    var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == keyDetails.CycleId);

        ////    if (quarterDetails != null)
        ////    {
        ////        var variance = krValueUpdate.CurrentValue - keyDetails.CurrentValue;
        ////        var currentValue = (krValueUpdate.CurrentValue - keyDetails.StartValue) + keyDetails.ContributorValue;
        ////        keyDetails.CurrentValue = krValueUpdate.CurrentValue;
        ////        keyDetails.UpdatedBy = userIdentity.EmployeeId;
        ////        keyDetails.UpdatedOn = DateTime.UtcNow;
        ////        var score = (currentValue / (keyDetails.TargetValue - keyDetails.StartValue)) * 100;
        ////        if (krValueUpdate.CurrentValue == keyDetails.StartValue || krValueUpdate.CurrentValue < keyDetails.StartValue)
        ////        {
        ////            keyDetails.Score = 0;
        ////        }
        ////        else
        ////        {
        ////            keyDetails.Score = Math.Round(score) > 100 ? 100 : Math.Round(score);
        ////        }
        ////        var progress = commonService.GetProgressIdWithFormula(keyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), keyDetails.Score, cycleDurationDetails.CycleDurationId);
        ////        keyDetails.Progress = progress;
        ////        goalKeyRepo.Update(keyDetails);
        ////        await UnitOfWorkAsync.SaveChangesAsync();

        ////        if (keyDetails.ImportedId > 0)
        ////        {
        ////            long importedId = keyDetails.ImportedId;
        ////            do
        ////            {
        ////                var goalKeyDetails = await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalKeyId == importedId);
        ////                if (goalKeyDetails == null)
        ////                    break;
        ////                var contributorValue = goalKeyDetails.ContributorValue + variance;
        ////                goalKeyDetails.ContributorValue = contributorValue;

        ////                var sourceScore = (contributorValue + (goalKeyDetails.CurrentValue - goalKeyDetails.StartValue)) / Math.Round(goalKeyDetails.TargetValue - goalKeyDetails.StartValue) * 100;
        ////                if (contributorValue == goalKeyDetails.StartValue || contributorValue < goalKeyDetails.StartValue)
        ////                {
        ////                    goalKeyDetails.Score = 0;
        ////                }
        ////                else
        ////                {
        ////                    goalKeyDetails.Score = Math.Round(sourceScore) > 100 ? 100 : Math.Round(sourceScore);
        ////                }
        ////                var sourceProgress = commonService.GetProgressIdWithFormula(keyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), keyDetails.Score, cycleDurationDetails.CycleDurationId);
        ////                goalKeyDetails.Progress = sourceProgress;
        ////                goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
        ////                goalKeyDetails.UpdatedOn = DateTime.UtcNow;
        ////                goalKeyRepo.Update(goalKeyDetails);
        ////                await UnitOfWorkAsync.SaveChangesAsync();
        ////                importedId = goalKeyDetails.ImportedId;
        ////            }
        ////            while (importedId != 0);
        ////        }

        ////    }
        ////}

        ////public void KeyValueUpdate(long empId, long goalKeyId, decimal currentValue, DateTime dueDate, DateTime cycleStartDate, DateTime cycleEndDate)
        ////{
        ////    using var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand();
        ////    command.CommandText = Constants.SpUpdateKrValue + empId + "," + goalKeyId + "," + currentValue + "," + "'" + dueDate.ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + "'" + "," + "'" + cycleStartDate.ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + "'" + "," + "'" + cycleEndDate.ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + "'";
        ////    command.CommandType = CommandType.Text;
        ////    OkrServiceDBContext.Database.OpenConnection();
        ////    command.ExecuteReader();
        ////    OkrServiceDBContext.Database.CloseConnection();
        ////}
        public GoalSequence GetGoalSequence(UpdateSequenceRequest sequenceRequest, long employeeId)
        {
            return goalSequenceRepo.GetQueryable().FirstOrDefault(x => x.GoalType == sequenceRequest.GoalType && x.GoalId == sequenceRequest.GoalId && x.EmployeeId == employeeId && x.GoalCycleId == sequenceRequest.GoalCycleId && x.IsActive);
        }
        public async Task<GoalSequence> UpdateGoalSequence(GoalSequence goalSequence)
        {
            goalSequenceRepo.Update(goalSequence);
            await UnitOfWorkAsync.SaveChangesAsync();
            return goalSequence;
        }
        public async Task<GoalSequence> InsertGoalSequence(GoalSequence goalSequence)
        {
            goalSequenceRepo.Add(goalSequence);
            await UnitOfWorkAsync.SaveChangesAsync();
            return goalSequence;
        }
        public async Task<bool> UpdateGoalDescription(UpdateGoalDescriptionRequest updateGoalDescriptionRequest, UserIdentity userIdentity, string token)
        {
            var result = false;
            if (updateGoalDescriptionRequest.GoalType == (int)GoalType.GoalObjective)
            {
                var goalObjective = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == updateGoalDescriptionRequest.GoalId);
                if (goalObjective != null)
                {
                    goalObjective.ObjectiveName = updateGoalDescriptionRequest.Description;
                    goalObjective.EndDate = updateGoalDescriptionRequest.DueDate;
                    goalObjective.UpdatedOn = DateTime.UtcNow;
                    goalObjective.UpdatedBy = userIdentity.EmployeeId;

                    await UpdateObjectiveAsync(goalObjective);
                    result = true;
                }

                var contributors = goalObjectiveRepo.GetQueryable().Where(x => x.ImportedId == updateGoalDescriptionRequest.GoalId && x.IsActive).ToList();
                foreach (var contributor in contributors)
                {
                    contributor.ObjectiveName = updateGoalDescriptionRequest.Description;
                    contributor.EndDate = updateGoalDescriptionRequest.DueDate;
                    contributor.UpdatedBy = userIdentity.EmployeeId;
                    contributor.UpdatedOn = DateTime.UtcNow;
                    await UpdateObjectiveAsync(contributor);

                    await Task.Run(async () =>
                    {
                        await notificationService.UpdateContributorsOkrNotifications(contributor, userIdentity, token).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
            }
            else if (updateGoalDescriptionRequest.GoalType == (int)GoalType.GoalKey)
            {
                var goalKey = await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalKeyId == updateGoalDescriptionRequest.GoalId);
                if (goalKey != null)
                {
                    goalKey.KeyDescription = updateGoalDescriptionRequest.Description;
                    goalKey.DueDate = updateGoalDescriptionRequest.DueDate;
                    goalKey.UpdatedOn = DateTime.UtcNow;
                    goalKey.UpdatedBy = userIdentity.EmployeeId;

                    await UpdateKeyResults(goalKey);
                    result = true;
                }

                var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == updateGoalDescriptionRequest.GoalId && x.IsActive).ToList();
                foreach (var contributor in contributors)
                {
                    contributor.KeyDescription = updateGoalDescriptionRequest.Description;
                    contributor.DueDate = updateGoalDescriptionRequest.DueDate;
                    contributor.UpdatedBy = userIdentity.EmployeeId;
                    contributor.UpdatedOn = DateTime.UtcNow;
                    await UpdateKeyResults(contributor);

                    await Task.Run(async () =>
                    {
                        await notificationService.UpdateContributorsKeyNotifications(contributor, userIdentity, token).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
            }

            return result;
        }
        public long GetSourceId(long importedId, int importedType)
        {
            var sourceGoalId = new long();

            if (importedType == (int)GoalType.GoalKey)
            {
                do
                {
                    var goalKeyDetails = GetGoalKeyDetails(importedId);
                    if (goalKeyDetails == null)
                        break;

                    importedId = goalKeyDetails.ImportedId;
                    sourceGoalId = goalKeyDetails.GoalKeyId;

                }
                while (importedId != 0);
            }
            else
            {
                do
                {
                    var goalObjectiveDetails = GetGoalObjective(importedId);
                    if (goalObjectiveDetails == null)
                        break;

                    importedId = goalObjectiveDetails.ImportedId;
                    sourceGoalId = goalObjectiveDetails.GoalObjectiveId;

                }
                while (importedId != 0);
            }

            return sourceGoalId;
        }
        public async Task<List<GoalKey>> GetKeyContributorAsync(int goalType, long goalKeyId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.ImportedType == goalType && x.ImportedId == goalKeyId && x.IsActive).ToListAsync();
        }
        public async Task<bool> DeleteOkrKrTeam(long employeeId, long goalObjectiveId, int goalType, long teamId, UserIdentity userIdentity, string jwtToken)
        {
            var resultOutput = false;
            var getAllLevelTeamContributorsList = commonService.GetAllLevelTeamKrContributors((int)GoalType.GoalKey, goalObjectiveId, employeeId, teamId);//// KR contributors for Notifications
            var getAllLevelTeamContributorsOkrList = commonService.GetAllLevelTeamContributors((int)GoalType.GoalObjective, goalObjectiveId, goalObjectiveId, teamId);//// OKR Contributors for Notifications

            var count = GetCount(goalObjectiveId);
            var currentCycle = await commonService.GetCurrentCycleAsync(userIdentity.OrganisationId);
            if (currentCycle != null)
            {
                var result = await DeleteTeamOkrKr(employeeId, goalObjectiveId, teamId, currentCycle.CycleStartDate, Convert.ToDateTime(currentCycle.CycleEndDate));

                if (result.Count > 0)
                {
                    if (goalType == 1)
                    {
                        await Task.Run(async () =>
                        {
                            await notificationService.DeleteOkrNotifications(getAllLevelTeamContributorsOkrList, goalType, goalObjectiveId, employeeId, jwtToken).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await Task.Run(async () =>
                        {
                            await notificationService.DeleteKrNotifications(count, result, getAllLevelTeamContributorsList, goalType, goalObjectiveId, employeeId, jwtToken).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                }

                resultOutput = true;
            }

            return resultOutput;
        }
        public async Task<List<DeleteKrResponse>> DeleteTeamOkrKr(long employeeId, long goalObjectiveId, long teamId, DateTime cycleStartDate, DateTime cycleEndDate)
        {
            var deleteKrResponses = new List<DeleteKrResponse>();
            await using var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = Constants.SpDeleteObjective_Team + employeeId + Constants.Comma + goalObjectiveId + Constants.Comma + Constants.Quotation + cycleStartDate.ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + Constants.Quotation + Constants.Comma + Constants.Quotation + cycleEndDate.ToString(Constants.DateFormat, CultureInfo.InvariantCulture) + Constants.Quotation + Constants.Comma + teamId;
            command.CommandType = CommandType.Text;
            await OkrServiceDBContext.Database.OpenConnectionAsync();
            var dataReader = await command.ExecuteReaderAsync();
            while (await dataReader.ReadAsync())
            {
                var deleteKrResponse = new DeleteKrResponse
                {
                    GoalType = Convert.ToInt32(dataReader["GoalType"].ToString()),
                    GoalId = Convert.ToInt64(dataReader["GoalId"].ToString())
                };
                deleteKrResponses.Add(deleteKrResponse);
            }

            await OkrServiceDBContext.Database.CloseConnectionAsync();

            return deleteKrResponses;
        }
        public async Task<List<LinkedObjectiveResponse>> LinkObjectivesAsync(long searchEmployeeId, int searchEmployeeCycleId, string token, UserIdentity identity)
        {
            var response = new List<LinkedObjectiveResponse>();
            var currentCycle = await commonService.GetCurrentCycleAsync(identity.OrganisationId);
            var allTeamEmployees = await commonService.GetTeamEmployees();
            var teamDetailsById = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == identity.OrganisationId);
            if (currentCycle != null)
            {
                var loginUserObj = await goalObjectiveRepo.GetQueryable().Where(x => x.EmployeeId == identity.EmployeeId && x.ObjectiveCycleId == currentCycle.OrganisationCycleId && x.GoalStatusId == (int)GoalStatus.Public && x.IsActive).ToListAsync();

                if (searchEmployeeId == Constants.ZeroId && teamDetailsById != null && identity.EmployeeId != teamDetailsById.OrganisationHead)
                {
                    var leaderObj = await goalObjectiveRepo.GetQueryable().Where(x => x.EmployeeId == teamDetailsById.OrganisationHead && x.TeamId == teamDetailsById.OrganisationId && x.ObjectiveCycleId == currentCycle.OrganisationCycleId && x.GoalStatusId == (int)GoalStatus.Public && x.IsActive).ToListAsync();
                    foreach (var item in leaderObj)
                    {
                        var importedObj = loginUserObj.FirstOrDefault(x => x.ImportedId == item.GoalObjectiveId);
                        if (importedObj == null)
                        {
                            response.Add(new LinkedObjectiveResponse()
                            {
                                ObjectiveId = item.GoalObjectiveId,
                                ObjectiveName = item.ObjectiveName,
                                DueCycle = currentCycle.Symbol + " - " + currentCycle.CycleYear,
                                TeamId = item.TeamId,
                                TeamName = teamDetailsById.OrganisationName,
                                ColorCode = teamDetailsById.ColorCode,
                                BackGroundColorCode = teamDetailsById.BackGroundColorCode
                            });
                        }
                    }
                }

                else if (searchEmployeeId > Constants.ZeroId)
                {
                    var searchUserObj = await goalObjectiveRepo.GetQueryable().Where(x => x.EmployeeId == searchEmployeeId && x.ObjectiveCycleId == searchEmployeeCycleId && x.GoalStatusId == (int)GoalStatus.Public && !x.IsPrivate && x.IsActive).ToListAsync();
                    foreach (var objective in searchUserObj)
                    {
                        var teamDetails = new TeamDetails();
                        if (objective.TeamId > 0)
                        {
                            teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == objective.TeamId);
                        }

                        var goalDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objective.GoalObjectiveId && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive).ToListAsync();
                        var importedObj = loginUserObj.FirstOrDefault(x => x.ImportedId == objective.GoalObjectiveId);
                        var isAligned = AlignStatus(identity.EmployeeId, 1, objective.Source);
                        if (importedObj == null && goalDetails.Count > 0 && !isAligned.IsAligned)
                        {
                            response.Add(new LinkedObjectiveResponse()
                            {
                                ObjectiveId = objective.GoalObjectiveId,
                                ObjectiveName = objective.ObjectiveName,
                                DueCycle = currentCycle.Symbol + " - " + currentCycle.CycleYear,
                                TeamId = objective.TeamId,
                                TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                                ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                                BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode
                            });
                        }
                    }
                }
            }

            return response;
        }
        public async Task<bool> ChangeOwner(UserIdentity identity, long goalObjectiveId, long newOwner, string jwtToken)
        {
            bool result = false;
            var getObjectiveDetail = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive);
            if (getObjectiveDetail != null)
            {
                if (getObjectiveDetail.GoalStatusId == (int)GoalStatus.Public)
                {
                    var keyDetails = await DeleteNewOwnerFromContributor(identity, goalObjectiveId, newOwner, jwtToken);

                    foreach (var key in keyDetails)
                    {
                        key.Owner = newOwner;
                        key.EmployeeId = newOwner;
                        key.UpdatedBy = identity.EmployeeId;
                        key.UpdatedOn = DateTime.UtcNow;
                        UpdateKeyResultNonAsync(key);
                        await UpdateOwnerInKeyContributor(identity, key.GoalKeyId, newOwner);
                    }

                    getObjectiveDetail.Owner = newOwner;
                    getObjectiveDetail.EmployeeId = newOwner;
                    getObjectiveDetail.UpdatedBy = identity.EmployeeId;
                    getObjectiveDetail.UpdatedOn = DateTime.UtcNow;
                    await UpdateObjectiveAsync(getObjectiveDetail);

                    var objectiveContributor = goalObjectiveRepo.GetQueryable().AsNoTracking().Where(x => x.ImportedId == goalObjectiveId && x.IsActive).ToList();
                    foreach (var obj in objectiveContributor)
                    {
                        obj.Owner = newOwner;
                        obj.UpdatedBy = identity.EmployeeId;
                        obj.UpdatedOn = DateTime.UtcNow;
                        await UpdateObjectiveAsync(obj);
                    }
                    result = true;

                }

            }

            return result;
        }
        public List<CteResponse> GetSubCascading(long goalKeyId)
        {
            List<CteResponse> cteList = new List<CteResponse>();
            List<CteResponse> matches = new List<CteResponse>();

            var keyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId && x.IsActive).ToList();
            if (keyDetails.Count > 0)
            {
                foreach (var itemResult in keyDetails)
                {
                    var goalKey = new CteResponse
                    {
                        GoalKeyId = itemResult.GoalKeyId,
                        GoalObjectiveId = itemResult.GoalObjectiveId,
                        Score = itemResult.Score,
                        StartValue = itemResult.StartValue,
                        CurrentValue = itemResult.CurrentValue,
                        ContributorValue = itemResult.ContributorValue,
                        TargetValue = itemResult.TargetValue,
                        ImportedId = itemResult.ImportedId,
                        Source = itemResult.Source,
                        IsActive = itemResult.IsActive
                    };
                    matches.Add(goalKey);
                }
            }

            if (matches.Any())
            {
                cteList.AddRange(TraverseSubs(matches));
            }

            return cteList;
        }
        public List<CteResponse> TraverseSubs(List<CteResponse> resultSet)
        {
            var compList = new List<CteResponse>();

            compList.AddRange(resultSet);

            var childrenList = new List<CteResponse>();
            for (var i = 0; i < resultSet.Count; i++)
            {
                ////Get all subCompList of each                 

                var keyDetails = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == resultSet[i].GoalKeyId && x.IsActive).ToList();

                if (keyDetails.Count > 0)
                {
                    foreach (var itemResult in keyDetails)
                    {
                        var goalKey = new CteResponse
                        {
                            GoalKeyId = itemResult.GoalKeyId,
                            GoalObjectiveId = itemResult.GoalObjectiveId,
                            Score = itemResult.Score,
                            StartValue = itemResult.StartValue,
                            CurrentValue = itemResult.CurrentValue,
                            ContributorValue = itemResult.ContributorValue,
                            TargetValue = itemResult.TargetValue,
                            ImportedId = itemResult.ImportedId,
                            Source = itemResult.Source,
                            IsActive = itemResult.IsActive
                        };
                        childrenList.Add(goalKey);
                    }
                }

                if (childrenList.Any())
                {
                    compList.AddRange(TraverseSubs(childrenList));
                }
            }

            return compList;
        }
        public async Task<bool> ResetOkr(long employeeId, long goalObjectiveId, int goalType, UserIdentity userIdentity, string jwtToken)
        {
            bool resultOutput = false;
            var getAllActiveKr = goalKeyRepo.GetQueryable().AsTracking().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive).ToList();
            await DeleteOkr(employeeId, goalObjectiveId, goalType, userIdentity, jwtToken);

            var getGoalObjective = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalObjectiveId);
            if (getGoalObjective != null)
            {
                getGoalObjective.IsActive = true;
                getGoalObjective.TeamId = 0;
                getGoalObjective.Score = 0;
                getGoalObjective.UpdatedBy = userIdentity.EmployeeId;
                getGoalObjective.UpdatedOn = DateTime.UtcNow;
                await UpdateObjectiveAsync(getGoalObjective);
            }

            foreach (var item in getAllActiveKr)
            {
                item.IsActive = true;
                item.TeamId = 0;
                item.CurrentValue = item.StartValue;
                item.Score = 0;
                item.ContributorValue = 0;
                item.UpdatedBy = userIdentity.EmployeeId;
                item.UpdatedOn = DateTime.UtcNow;
                await UpdateKeyResults(item);

            }

            resultOutput = true;

            return resultOutput;
        }
        public async Task<bool> UpdateTeamLeaderOkr(UpdateTeamLeaderOkrRequest teamLeaderOkrRequest, UserIdentity identity, string jwtToken)
        {
            var goalKeyList = new List<GoalKey>();
            bool result = false;
            var goalObjectiveDetails = await goalObjectiveRepo.GetQueryable().Where(x => x.TeamId == teamLeaderOkrRequest.TeamId && x.ObjectiveCycleId == teamLeaderOkrRequest.CycleId && (x.EmployeeId == teamLeaderOkrRequest.OldLeader || x.Owner == teamLeaderOkrRequest.OldLeader) && x.IsActive).ToListAsync();
            foreach (var goalObjective in goalObjectiveDetails)
            {
                if (goalObjective.EmployeeId == teamLeaderOkrRequest.OldLeader && goalObjective.Owner == teamLeaderOkrRequest.OldLeader)
                {
                    goalObjective.EmployeeId = teamLeaderOkrRequest.NewLeader;
                    goalObjective.Owner = teamLeaderOkrRequest.NewLeader;
                    goalObjective.UpdatedBy = identity.EmployeeId;
                    goalObjective.UpdatedOn = DateTime.UtcNow;
                    await UpdateObjectiveAsync(goalObjective);
                }
                else if (goalObjective.EmployeeId == teamLeaderOkrRequest.OldLeader && goalObjective.Owner != teamLeaderOkrRequest.OldLeader)
                {
                    goalObjective.EmployeeId = teamLeaderOkrRequest.NewLeader;
                    goalObjective.UpdatedBy = identity.EmployeeId;
                    goalObjective.UpdatedOn = DateTime.UtcNow;
                    await UpdateObjectiveAsync(goalObjective);
                    if (teamLeaderOkrRequest.NewLeader == goalObjective.Owner)
                    {
                        await DeleteNewOwnerFromContributor(identity, goalObjective.ImportedId, teamLeaderOkrRequest.NewLeader, jwtToken);
                    }
                }
                else if (goalObjective.EmployeeId != teamLeaderOkrRequest.OldLeader && goalObjective.Owner == teamLeaderOkrRequest.OldLeader)
                {
                    goalObjective.Owner = teamLeaderOkrRequest.NewLeader;
                    goalObjective.UpdatedBy = identity.EmployeeId;
                    goalObjective.UpdatedOn = DateTime.UtcNow;
                    await UpdateObjectiveAsync(goalObjective);
                    if (teamLeaderOkrRequest.NewLeader == goalObjective.EmployeeId)
                    {
                        var goalList = await DeleteNewOwnerFromContributor(identity, goalObjective.ImportedId, teamLeaderOkrRequest.NewLeader, jwtToken);
                        goalKeyList.AddRange(goalList);
                    }
                }

                result = true;
            }

            var goalId = goalKeyList.Select(x => x.GoalKeyId).Distinct().ToList();
            var goalKeyDetails = await goalKeyRepo.GetQueryable().Where(x => x.TeamId == teamLeaderOkrRequest.TeamId && x.CycleId == teamLeaderOkrRequest.CycleId && (x.EmployeeId == teamLeaderOkrRequest.OldLeader || x.Owner == teamLeaderOkrRequest.OldLeader) && x.IsActive && !goalId.Contains(x.GoalKeyId)).ToListAsync();
            goalKeyList.AddRange(goalKeyDetails);
            foreach (var goalKey in goalKeyList)
            {
                if (goalKey.EmployeeId == teamLeaderOkrRequest.OldLeader && goalKey.Owner == teamLeaderOkrRequest.OldLeader)
                {
                    goalKey.EmployeeId = teamLeaderOkrRequest.NewLeader;
                    goalKey.Owner = teamLeaderOkrRequest.NewLeader;
                    goalKey.UpdatedBy = identity.EmployeeId;
                    goalKey.UpdatedOn = DateTime.UtcNow;
                    await UpdateKeyResults(goalKey);
                }
                else if (goalKey.EmployeeId == teamLeaderOkrRequest.OldLeader && goalKey.Owner != teamLeaderOkrRequest.OldLeader)
                {
                    goalKey.EmployeeId = teamLeaderOkrRequest.NewLeader;
                    goalKey.UpdatedBy = identity.EmployeeId;
                    goalKey.UpdatedOn = DateTime.UtcNow;
                    await UpdateKeyResults(goalKey);
                }
                else if (goalKey.EmployeeId != teamLeaderOkrRequest.OldLeader && goalKey.Owner == teamLeaderOkrRequest.OldLeader)
                {
                    goalKey.Owner = teamLeaderOkrRequest.NewLeader;
                    goalKey.UpdatedBy = identity.EmployeeId;
                    goalKey.UpdatedOn = DateTime.UtcNow;
                    await UpdateKeyResults(goalKey);
                }

                var goalKeyHistoryDetails = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == goalKey.GoalKeyId && x.CreatedBy == teamLeaderOkrRequest.OldLeader).ToListAsync();
                foreach (var goalKeyHistory in goalKeyHistoryDetails)
                {
                    goalKeyHistory.CreatedBy = teamLeaderOkrRequest.NewLeader;
                    goalKeyHistoryRepo.Update(goalKeyHistory);
                    await UnitOfWorkAsync.SaveChangesAsync();
                }

                result = true;
            }

            return result;
        }
        public bool IsTeamObjectiveImported(long goalObjectiveId, long empId, long teamId)
        {
            var goalObjectiveDetail = goalObjectiveRepo.GetQueryable().Count(x => x.ImportedType == 1 && x.ImportedId == goalObjectiveId && x.EmployeeId == empId && x.IsActive && x.TeamId == teamId);
            return goalObjectiveDetail != 0;
        }
        public GoalObjective GetTeamGoalObjectiveByImportedId(long importedId, long empId, long teamId)
        {
            var goalObjectiveDetail = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ImportedType == 1 && x.ImportedId == importedId && x.EmployeeId == empId && x.IsActive && x.TeamId == teamId);
            return goalObjectiveDetail;
        }

        #region Private Methods
        private async Task<List<GoalKey>> DeleteNewOwnerFromContributor(UserIdentity identity, long goalObjectiveId, long newOwner, string jwtToken)
        {
            var goalKeyList = new List<GoalKey>();
            var getNewOwnerObjective = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x =>
                x.ImportedId == goalObjectiveId && x.EmployeeId == newOwner && x.IsActive);
            if (getNewOwnerObjective != null)
            {
                var getNewOwnerKey = goalKeyRepo.GetQueryable().AsNoTracking().Where(x =>
                    x.GoalObjectiveId == getNewOwnerObjective.GoalObjectiveId && x.IsActive).ToList();
                foreach (var key in getNewOwnerKey)
                {
                    var getSourceKey = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == key.ImportedId && x.IsActive);
                    if (getSourceKey != null)
                    {
                        if (key.MetricId == (int)Metrics.Boolean)
                        {
                            getSourceKey.CurrentValue = key.CurrentValue == 0 ? (getSourceKey.CurrentValue + key.CurrentValue) : key.CurrentValue;
                            getSourceKey.Score = key.Score == 0 ? (getSourceKey.Score + key.Score) : key.Score;
                            UpdateKeyResultNonAsync(getSourceKey);
                            await DeleteContributors(identity.EmployeeId, key.GoalKeyId, identity, jwtToken);
                        }
                        else ////if (key.MetricId == (int)Metrics.NoUnits)
                        {
                            if (getSourceKey.StartValue > getSourceKey.TargetValue)
                            {
                                getSourceKey.CurrentValue = getSourceKey.CurrentValue + getSourceKey.ContributorValue;
                                getSourceKey.ContributorValue = 0.0M;
                                UpdateKeyResultNonAsync(getSourceKey);
                                key.IsActive = false;
                                UpdateKeyResultNonAsync(key);

                                var count = goalKeyRepo.GetQueryable().Count(x => x.GoalObjectiveId == key.GoalObjectiveId && x.IsActive && x.EmployeeId == key.EmployeeId);
                                if (count == 0)
                                {
                                    var objective = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == key.GoalObjectiveId && x.IsActive);
                                    if (objective != null)
                                    {
                                        objective.IsActive = false;
                                        await UpdateObjectiveAsync(objective);
                                    }
                                }

                                ////await DeleteContributors(identity.EmployeeId, key.GoalKeyId, identity, jwtToken);
                            }
                            else
                            {
                                getSourceKey.CurrentValue = getSourceKey.CurrentValue + key.CurrentValue;
                                UpdateKeyResultNonAsync(getSourceKey);
                                await DeleteContributors(identity.EmployeeId, key.GoalKeyId, identity, jwtToken);
                            }
                        }

                        goalKeyList.Add(getSourceKey);

                        ////else
                        ////{
                        ////    getSourceKey.CurrentValue = getSourceKey.CurrentValue + key.CurrentValue;
                        ////    await UpdateKeyResults(getSourceKey);
                        ////    await DeleteContributors(identity.EmployeeId, key.GoalKeyId, identity, jwtToken);
                        ////}
                    }
                }

                ////getNewOwnerObjective.IsActive = false;
                ////getNewOwnerObjective.UpdatedBy = identity.EmployeeId;
                ////getNewOwnerObjective.UpdatedOn = DateTime.UtcNow;
                ////await UpdateObjectiveAsync(getNewOwnerObjective);
            }
            else
            {
                goalKeyList = goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive).ToList();
            }

            return goalKeyList;
        }
        private async Task UpdateOwnerInKeyContributor(UserIdentity identity, long goalKeyId, long newOwner)
        {
            var getKeyContributor = await goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalKeyId && x.IsActive).ToListAsync();
            foreach (var key in getKeyContributor)
            {
                key.Owner = newOwner;
                key.UpdatedBy = identity.EmployeeId;
                key.UpdatedOn = DateTime.UtcNow;
                await UpdateKeyResults(key);
            }

        }
        private async Task<List<AlignKeyResponse>> SeparationAlignKeyResponse_AlignObjective(long goalId, List<UserResponse> allEmployee, UserIdentity loginUser)
        {
            var alignKeyResponses = new List<AlignKeyResponse>();
            var keyDetail = GetGoalKey(goalId);
            foreach (var key in keyDetail)
            {
                alignKeyResponses.Add(new AlignKeyResponse
                {
                    GoalKeyId = key.GoalKeyId,
                    DueDate = key.DueDate,
                    Score = key.Score,
                    Source = key.Source,
                    ImportedType = key.ImportedType,
                    ImportedId = key.ImportedId,
                    KeyDescription = key.KeyDescription,
                    Contributors = await commonService.GetContributorAsync((int)GoalType.GoalKey, key.GoalKeyId, allEmployee),
                    IsKeyDisabled = commonService.GetGoalKeySource(loginUser.EmployeeId, key.Source == 0 ? key.GoalKeyId : key.Source).Result.IsAligned,
                    MetricId = key.MetricId,
                    CurrencyId = key.CurrencyId,
                    CurrentValue = key.CurrentValue,
                    CurrencyCode = key.CurrencyCode,
                    TargetValue = key.TargetValue,
                    AssignmentTypeId = key.AssignmentTypeId,
                    KrStatusId = key.KrStatusId,
                    GoalStatusId = key.GoalStatusId,
                    ContributorValue = key.ContributorValue

                });
            }

            return alignKeyResponses;
        }

        /// <summary>
        /// If contributors are there and assignment type will be 2 then only okr will be created for the contributor
        /// </summary>
        /// <param name="contributorDetails"></param>
        /// <param name="goalObjective"></param>
        /// <param name="loginUser"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        private async Task SeparationAddObjForContributor_InsertGoalObjective(List<ContributorDetails> contributorDetails, GoalObjective goalObjective, UserIdentity loginUser, string jwtToken)
        {
            foreach (var item in contributorDetails)
            {
                if (item.AssignmentTypeId == 2)
                {
                    var isObjectiveImported = IsObjectiveImported(goalObjective.GoalObjectiveId, item.EmployeeId);
                    if (!isObjectiveImported)
                    {

                        var goalObjectives = await InsertGoal(goalObjective, item, loginUser, item.EmployeeId, jwtToken);

                        ////if (goalObjectives.GoalStatusId != (int)GoalStatus.Draft)
                        ////{
                        ////    await Task.Run(async () =>
                        ////    {
                        ////        await notificationService.ObjContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, goalObjective.GoalObjectiveId, goalObjectives.GoalObjectiveId).ConfigureAwait(false);
                        ////    }).ConfigureAwait(false);
                        ////}
                    }
                }
            }
        }
        private async Task<GoalObjective> InsertGoal(GoalObjective goalObjective, ContributorDetails contributorDetails, UserIdentity loginEmpId, long contributorEmpId, string jwtToken)
        {
            var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
            var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == contributorEmpId);
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

            var currentCycle = (from data in cycleDurationDetails.CycleDetails
                                from data2 in data.QuarterDetails
                                where data2.IsCurrentQuarter
                                select new CycleLockDetails
                                {
                                    StartDate = Convert.ToDateTime(data2.StartDate),
                                    EndDate = Convert.ToDateTime(data2.EndDate),
                                    OrganisationCycleId = (int)data2.OrganisationCycleId,
                                    Year = Int32.Parse(data.Year)
                                }).FirstOrDefault();


            var isOkrLocked = await commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, contributorEmpId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken);
            var goalObjectives = new GoalObjective
            {
                Year = contributorDetails.Year,
                ObjectiveCycleId = contributorDetails.CycleId,
                //// ObjectiveDescription = goalObjective.ObjectiveDescription,
                ObjectiveName = contributorDetails.ObjectiveName ?? goalObjective.ObjectiveName,
                IsPrivate = goalObjective.IsPrivate,
                StartDate = goalObjective.StartDate,
                EndDate = goalObjective.EndDate,
                ImportedId = goalObjective.GoalObjectiveId,
                ImportedType = (int)GoalRequest.ImportedType,
                Source = goalObjective.GoalObjectiveId,
                CreatedBy = loginEmpId.EmployeeId,
                Score = 0,//contributorDetails.Score,
                Progress = (int)ProgressMaster.NotStarted,
                EmployeeId = contributorEmpId,
                Sequence = (int)GoalRequest.Sequence,
                GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : goalObjective.GoalStatusId,
                GoalTypeId = goalObjective.GoalTypeId,
                Owner = goalObjective.Owner,
               //// LinkedObjectiveId = goalObjective.LinkedObjectiveId

            };
            await InsertObjective(goalObjectives);
            return goalObjectives;
        }
        private async Task SeparationAddKeyForContributor_InsertGoalObjective(List<ContributorDetails> contributorDetails, GoalObjective goalObjective, GoalKey goalKey, UserIdentity loginUser, string jwtToken)
        {
            var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
            foreach (var item in contributorDetails)
            {
                var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                var currentCycle = (from data in cycleDurationDetails.CycleDetails
                                    from data2 in data.QuarterDetails
                                    where data2.IsCurrentQuarter
                                    select new CycleLockDetails
                                    {
                                        StartDate = Convert.ToDateTime(data2.StartDate),
                                        EndDate = Convert.ToDateTime(data2.EndDate),
                                        OrganisationCycleId = (int)data2.OrganisationCycleId,
                                        Year = Int32.Parse(data.Year)
                                    }).FirstOrDefault();

                var isOkrLocked = await commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, item.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken);

                var contributorGoalDetails = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == item.EmployeeId && x.ImportedType == 1 && x.ImportedId == goalObjective.GoalObjectiveId && x.IsActive);

                var goalKeys = new GoalKey
                {

                    StartDate = item.StartDate < goalKey.StartDate ? DateTime.Now : item.StartDate,
                    DueDate = item.DueDate > goalKey.DueDate ? goalKey.DueDate : item.DueDate,
                    GoalObjectiveId = item.AssignmentTypeId == 2 && contributorGoalDetails != null ? contributorGoalDetails.GoalObjectiveId : 0,
                    KeyDescription = item.KeyResult,
                    CreatedBy = loginUser.EmployeeId,
                    Score = 0, //item.Score,
                    ImportedType = (int)GoalRequest.KeyImportedType,
                    EmployeeId = item.EmployeeId,
                    ImportedId = goalKey.GoalKeyId,
                    CycleId = item.CycleId,
                    Source = goalKey.ImportedId == 0 ? goalKey.GoalKeyId : goalKey.Source,
                    Progress = (int)ProgressMaster.NotStarted,
                    MetricId = goalKey.MetricId == 0 ? (int)Metrics.NoUnits : goalKey.MetricId,
                    CurrencyId = goalKey.CurrencyId,
                    CurrentValue = item.StartValue,
                    TargetValue = goalKey.MetricId == (int)Metrics.Boolean || goalKey.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                    AssignmentTypeId = item.AssignmentTypeId,
                    KrStatusId = item.KrStatusId, ////Initially it should be pending which is 1
                    GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : goalObjective.GoalStatusId,
                    StartValue = item.StartValue,
                    Owner = goalKey.Owner

                };
                await InsertKeyResults(goalKeys);


                if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                {
                    var updateKrValue = new KrValueUpdate
                    {
                        GoalKeyId = goalKeys.GoalKeyId,
                        CurrentValue = goalKeys.CurrentValue,
                        Year = goalObjective.Year
                    };

                    await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goalKeys);
                }

                var krStatusMessage = new KrStatusMessage
                {
                    AssignerGoalKeyId = goalKey.GoalKeyId,
                    AssigneeGoalKeyId = goalKeys.GoalKeyId,
                    KrAssignerMessage = item.KrAssignerMessage,
                    CreatedOnAssigner = DateTime.Now,
                    CreatedOnAssignee = DateTime.Now,
                    IsActive = true

                };
                await InsertMessagesOfKr(krStatusMessage);
                if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                {
                    await Task.Run(async () =>
                    {
                        await notificationService.KeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, goalKey.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
            }

        }
        private async Task<bool> DeleteScoreAndContributorDownwards(long goalKeyId, DateTime cycleStartDate, DateTime cycleEndDate, long cycleDurationId, UserIdentity userIdentity)
        {
            ////Get all GoalKeys downward the tree
            var tempGoalKeyNuList = GetSubCascading(goalKeyId);

            ////Get Only GoalKeyId from tempGoalKeyNuList for using filter
            var goalKeyIdList = new List<long>();

            ////Get Only GoalObjectiveId from tempGoalKeyNuList for using filter
            var goalObjectiveIdList = new List<long>();

            foreach (var item in tempGoalKeyNuList)
            {
                goalKeyIdList.Add(item.GoalKeyId);
                if (item.GoalObjectiveId != 0)
                {
                    goalObjectiveIdList.Add(item.GoalObjectiveId);
                }
            }

            ////update the status of GoalKeys downward the tree as inactive
            var goalKeyRecords = await goalKeyRepo.GetQueryable().AsTracking().Where(x => goalKeyIdList.Contains(x.GoalKeyId)).ToListAsync();
            if (goalKeyRecords.Count > 0)
            {
                goalKeyRecords.ForEach(a =>
                {
                    a.IsActive = false;
                    a.UpdatedBy = userIdentity.EmployeeId;
                    a.UpdatedOn = DateTime.UtcNow;
                    goalKeyRepo.Update(a);
                    UnitOfWorkAsync.SaveChanges();

                });

            }

            var tempGoalObjectiveNu = new List<GoalScore>();

            if (goalObjectiveIdList.Count > 0)
            {
                ////get updated score for GoalObjectives due to deleted GoalKeys


                var goalObjectiveScore = (from gob in goalObjectiveRepo.GetQueryable()
                                          join goalKey in goalKeyRepo.GetQueryable().Where(x => x.IsActive)
                                          on gob.GoalObjectiveId equals goalKey.GoalObjectiveId
                                          into orderTemp
                                          from ord in orderTemp.DefaultIfEmpty()
                                          where goalObjectiveIdList.Contains(ord.GoalObjectiveId)
                                          select new GoalScore
                                          {
                                              GoalObjectiveId = ord.GoalObjectiveId,
                                              Score = ord.Score
                                          }).ToList();


                tempGoalObjectiveNu = (from ab in goalObjectiveScore
                                       group ab by new { ab.GoalObjectiveId } into summary
                                       select new GoalScore
                                       {
                                           GoalObjectiveId = summary.Key.GoalObjectiveId,
                                           Score = summary.Average(m => m == null ? 0 : m.Score)
                                       }).ToList();

            }

            ////Get Only GoalObjectiveId from temp_GoalObjective_NU_list for using filter
            var tempGoalObjectiveIdNuList = new List<long>();
            if (tempGoalObjectiveNu.Count > 0)
            {
                foreach (var item in tempGoalObjectiveNu)
                {
                    tempGoalObjectiveIdNuList.Add(item.GoalObjectiveId);
                }


                ////update the score and progress on GoalObjectives                        
                foreach (var item in tempGoalObjectiveNu)
                {

                    var goalDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == item.GoalObjectiveId);
                    if (goalDetails != null)
                    {
                        var sourceProgress = commonService.GetProgressIdWithFormula(goalDetails.EndDate, cycleStartDate, cycleEndDate, item.Score, cycleDurationId);
                        goalDetails.Progress = sourceProgress;
                        goalDetails.Score = item.Score;
                        goalDetails.UpdatedBy = userIdentity.EmployeeId;
                        goalDetails.UpdatedOn = DateTime.UtcNow;
                        goalObjectiveRepo.Update(goalDetails);
                        UnitOfWorkAsync.SaveChanges();
                    }
                }

                ////Delete Objectives not containing any active KR
                foreach (var item in tempGoalObjectiveIdNuList)
                {
                    var keyCount = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == item && x.IsActive).ToList().Count;
                    if (keyCount == 0)
                    {
                        var goalObjectiveRecords = goalObjectiveRepo.GetQueryable().AsTracking().Where(x => x.GoalObjectiveId == item && x.IsActive).ToList();
                        if (goalObjectiveRecords.Count > 0)
                        {
                            goalObjectiveRecords.ForEach(a =>
                            {
                                a.IsActive = false;
                                a.UpdatedBy = userIdentity.EmployeeId;
                                a.UpdatedOn = DateTime.UtcNow;
                                goalObjectiveRepo.Update(a);
                                UnitOfWorkAsync.SaveChanges();
                            });
                        }
                    }
                }
            }

            var goalKeyObjectiveIds = goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.IsActive && x.CycleId == cycleDurationId).Select(x => x.GoalObjectiveId).Distinct().ToList();
            var final = (from gob in goalObjectiveRepo.GetQueryable()
                         where !goalKeyObjectiveIds.Contains(gob.GoalObjectiveId) &&
                               gob.GoalStatusId == (int)GoalStatus.Public && gob.ObjectiveCycleId == cycleDurationId && gob.IsActive
                         select gob).AsTracking().ToList();

            if (final.Count > 0)
            {
                foreach (var finalList in final)
                {
                    finalList.IsActive = false;
                    goalObjectiveRepo.Update(finalList);
                    UnitOfWorkAsync.SaveChanges();
                }
            }

            return true;
        }
        private async Task<List<DueDateResponse>> UpdateOkrDueDate(UpdateDueDateRequest updateDueDateRequest, UserIdentity userIdentity, string jwtToken, bool IsNotifications, GoalObjective goalObjective, List<GoalKey> goalKeys, bool isAlignment, List<long> contributorId)
        {
            List<DueDateResponse> dueDates = new List<DueDateResponse>();

            if (updateDueDateRequest.GoalType == Constants.GoalObjectiveType)
            {
                if (goalObjective != null)
                {
                    if (isAlignment)
                    {
                        /// Source Objective & KR update
                        var dueDateResponse = new DueDateResponse();
                        if (updateDueDateRequest.EndDate < goalObjective.EndDate || updateDueDateRequest.EndDate > goalObjective.EndDate)
                        {
                            goalObjective.EndDate = updateDueDateRequest.EndDate;
                        }
                        goalObjective.UpdatedOn = DateTime.UtcNow;
                        goalObjective.UpdatedBy = userIdentity.EmployeeId;
                        if (updateDueDateRequest.StartDate > goalObjective.StartDate || updateDueDateRequest.StartDate < goalObjective.StartDate)
                        {
                            goalObjective.StartDate = updateDueDateRequest.StartDate;
                        }

                        await UpdateObjectiveAsync(goalObjective);
                        dueDateResponse.DueDate = goalObjective.EndDate;
                        dueDateResponse.OkrId = updateDueDateRequest.GoalId;
                        dueDateResponse.EmployeeId = goalObjective.EmployeeId;

                        foreach (var keys in goalKeys)
                        {
                            if (keys.DueDate > goalObjective.EndDate)
                            {
                                keys.DueDate = updateDueDateRequest.EndDate;
                            }

                            if (keys.StartDate < goalObjective.StartDate)
                            {
                                keys.StartDate = updateDueDateRequest.StartDate;
                            }
                            keys.UpdatedOn = DateTime.UtcNow;
                            keys.UpdatedBy = userIdentity.EmployeeId;
                            UpdateKeyResultNonAsync(keys);
                            dueDateResponse.KrId.Add(keys.GoalKeyId);
                        }

                        dueDates.Add(dueDateResponse);
                    }

                    /// With Parent OKR Update of contributor
                    var allLevelObjContributors = new List<AllLevelObjectiveResponse>();
                    if (isAlignment)
                    {
                        allLevelObjContributors = commonService.GetObjectiveSubCascading(updateDueDateRequest.GoalId).Where(x => x.GoalId != updateDueDateRequest.GoalId && x.GoalId > 0).ToList();
                    }
                    else
                    {
                        allLevelObjContributors = commonService.GetObjectiveSubCascading(updateDueDateRequest.GoalId).Where(x => x.GoalId != updateDueDateRequest.GoalId && x.GoalId > 0).ToList();

                        foreach (var item in contributorId)
                        {

                            var list = allLevelObjContributors.FirstOrDefault(x => x.EmployeeId == item);
                            if (list != null)
                            {
                                DueDateResponse due = new DueDateResponse();
                                due.DueDate = list.EndDate;
                                due.OkrId = list.GoalId;
                                due.EmployeeId = list.EmployeeId;
                                dueDates.Add(due);
                            }

                        }

                        foreach (var item in contributorId)
                        {
                            allLevelObjContributors.RemoveAll(x => x.EmployeeId == item);
                        }
                    }

                    if (allLevelObjContributors.Count > 0)
                    {
                        var objcontri = allLevelObjContributors
                        .Select(x => new GoalObjective()
                        {
                            GoalObjectiveId = x.GoalId,
                            EmployeeId = x.EmployeeId,
                            StartDate = x.StartDate,
                            EndDate = x.EndDate,
                            ImportedType = x.ImportedType,
                            IsActive = x.IsActive,
                            ObjectiveCycleId = x.ObjectiveCycleId,
                            CreatedBy = x.CreatedBy,
                            CreatedOn = x.CreatedOn,
                            Year = x.Year,
                            Source = x.Source,
                            Sequence = x.Sequence,
                            GoalStatusId = x.GoalStatusId,
                            GoalTypeId = x.GoalTypeId,
                            TeamId = x.TeamId,
                            IsCoachCreation = x.IsCoachCreation,
                            ImportedId = x.ImportedId,
                            Score = x.Score,
                            ObjectiveName = x.ObjectiveName,
                            ObjectiveDescription = x.ObjectiveDescription,
                            Progress = x.Progress,
                            IsPrivate = x.IsPrivate,
                            Owner = x.Owner,
                            LinkedObjectiveId = x.LinkedObjectiveId,
                            UpdatedOn = x.UpdatedOn,
                            UpdatedBy = x.UpdatedBy
                        }).GroupBy(x => x.GoalObjectiveId).Select(x => x.FirstOrDefault()).ToList();

                        foreach (var obj in objcontri)
                        {
                            DueDateResponse dateResponse = new DueDateResponse();
                            if (obj.EndDate < goalObjective.EndDate || obj.EndDate > goalObjective.EndDate)
                            {
                                obj.EndDate = updateDueDateRequest.EndDate;
                            }
                            if (obj.StartDate > goalObjective.StartDate || obj.StartDate < goalObjective.StartDate)
                            {
                                obj.StartDate = updateDueDateRequest.StartDate;
                            }
                            obj.UpdatedBy = userIdentity.EmployeeId;
                            obj.UpdatedOn = DateTime.UtcNow;
                            obj.ObjectiveName = goalObjective.ObjectiveName;
                            obj.ObjectiveDescription = goalObjective.ObjectiveDescription;                           
                            await UpdateObjectiveAsync(obj);
                            dateResponse.OkrId = obj.GoalObjectiveId;
                            dateResponse.DueDate = obj.EndDate;
                            dateResponse.EmployeeId = obj.EmployeeId;
                            var keyResults = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == obj.GoalObjectiveId).ToListAsync();
                            if (keyResults != null)
                            {
                                foreach (var key in keyResults)
                                {
                                    if (key.DueDate > goalObjective.EndDate)
                                    {
                                        key.DueDate = updateDueDateRequest.EndDate;
                                    }
                                    if (key.StartDate < goalObjective.StartDate)
                                    {
                                        key.StartDate = updateDueDateRequest.StartDate;
                                    }
                                    key.UpdatedOn = DateTime.UtcNow;
                                    key.UpdatedBy = userIdentity.EmployeeId;
                                    await UpdateKeyResults(key);
                                    dateResponse.KrId.Add(key.GoalKeyId);
                                }
                            }

                            dueDates.Add(dateResponse);
                        }
                    }

                    ///Standalone KR update of contributors
                    foreach (var key in goalKeys)
                    {
                        var allLevelKrContributors = commonService.GetAllLevelKrContributors(Constants.GoalKeyType, key.GoalKeyId, Convert.ToInt64(key.EmployeeId)).Where(x => x.GoalObjectiveId == 0).ToList();
                        var goalKeyDetail = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == key.GoalKeyId && x.IsActive);
                        if (allLevelKrContributors.Count > 0)
                        {
                            var contri = allLevelKrContributors
                            .Select(x => new GoalKey()
                            {
                                GoalKeyId = x.GoalId,
                                EmployeeId = x.EmployeeId,
                                StartDate = x.StartDate,
                                KeyDescription = x.KeyDescription,
                                Score = x.Score,
                                ImportedType = x.ImportedType,
                                CreatedBy = x.CreatedBy,
                                CreatedOn = x.CreatedOn,
                                Progress = x.Progress,
                                Source = x.Source,
                                MetricId = x.MetricId,
                                AssignmentTypeId = x.AssignmentTypeId,
                                CurrencyId = x.CurrencyId,
                                CurrentValue = x.CurrentValue,
                                TargetValue = x.TargetValue,
                                KrStatusId = x.KrStatusId,
                                CycleId = x.CycleId,
                                CurrencyCode = x.CurrencyCode,
                                GoalStatusId = x.GoalStatusId,
                                ContributorValue = x.ContributorValue,
                                StartValue = x.StartValue,
                                KeyNotes = x.KeyNotes,
                                TeamId = x.TeamId,
                                ImportedId = x.ImportedId,
                                UpdatedOn = x.UpdatedOn,
                                UpdatedBy = x.UpdatedBy,
                                DueDate = x.DueDate,
                                Owner = x.Owner,
                                GoalObjectiveId = x.GoalObjectiveId,

                            }).GroupBy(x => x.GoalKeyId).Select(x => x.FirstOrDefault()).ToList();

                            foreach (var contriKeys in contri)
                            {
                                if (contriKeys.EmployeeId != key.EmployeeId)
                                {
                                    DueDateResponse dateResponse = new DueDateResponse();

                                    if (contriKeys.StartDate < goalObjective.StartDate)
                                    {
                                        contriKeys.StartDate = updateDueDateRequest.StartDate;
                                    }
                                    if (contriKeys.DueDate > goalObjective.EndDate)
                                    {
                                        contriKeys.DueDate = updateDueDateRequest.EndDate;
                                    }
                                    contriKeys.UpdatedOn = DateTime.UtcNow;
                                    contriKeys.UpdatedBy = userIdentity.EmployeeId;
                                    contriKeys.KeyDescription = goalKeyDetail == null ? "" : goalKeyDetail.KeyDescription;
                                    contriKeys.KeyNotes = goalKeyDetail == null ? "" : goalKeyDetail.KeyNotes;
                                    await UpdateKeyResults(contriKeys);
                                    dateResponse.KrId.Add(contriKeys.GoalKeyId);
                                    dateResponse.DueDate = contriKeys.DueDate;
                                    dateResponse.EmployeeId = (long)contriKeys.EmployeeId;
                                    dueDates.Add(dateResponse);
                                }
                            }
                        }
                    }

                    if (IsNotifications && goalObjective.GoalStatusId == (int)GoalStatus.Public)
                    {
                        var notifications = dueDates.GroupBy(x => x.EmployeeId).Select(x => x.FirstOrDefault()).ToList();
                        if (notifications.Count > 0)
                        {
                            await Task.Run(async () =>
                            {
                                await notificationService.UpdateDueDateNotifications(notifications, updateDueDateRequest.GoalType, jwtToken, userIdentity.EmployeeId).ConfigureAwait(false);
                            }).ConfigureAwait(false);
                        }
                    }

                }
            }

            return dueDates;
        }
        private async Task<List<DueDateResponse>> UpdateKrDueDate(UpdateDueDateRequest updateDueDateRequest, UserIdentity userIdentity, string jwtToken, bool IsNotifications, List<long> contributorId, bool isAlignment, DateTime startDate, DateTime dueDate)
        {
            var dueDates = new List<DueDateResponse>();
            var allLevelKrContributors = new List<KrContributors>();
            var goalKeyDetail = new GoalKey();
            if (isAlignment)
            {
                allLevelKrContributors = commonService.GetAllLevelKrContributors(updateDueDateRequest.GoalType, updateDueDateRequest.GoalId, userIdentity.EmployeeId);
            }
            else
            {
                goalKeyDetail = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == updateDueDateRequest.GoalId && x.IsActive);
                allLevelKrContributors = commonService.GetAllLevelKrContributors(updateDueDateRequest.GoalType, updateDueDateRequest.GoalId, userIdentity.EmployeeId).Where(x => x.EmployeeId != goalKeyDetail.EmployeeId).ToList();
                foreach (var item in contributorId)
                {
                    var list = allLevelKrContributors.FirstOrDefault(x => x.EmployeeId == item);
                    if (list != null)
                    {
                        DueDateResponse due = new DueDateResponse();
                        due.DueDate = list.DueDate;
                        due.KrId.Add(list.GoalId);
                        due.EmployeeId = list.EmployeeId;
                        dueDates.Add(due);
                    }

                }
                foreach (var item in contributorId)
                {
                    allLevelKrContributors.RemoveAll(x => x.EmployeeId == item);
                }
            }

            if (allLevelKrContributors.Count > 0)
            {
                var contri = allLevelKrContributors
                .Select(x => new GoalKey()
                {
                    GoalKeyId = x.GoalId,
                    EmployeeId = x.EmployeeId,
                    StartDate = x.StartDate,
                    KeyDescription = x.KeyDescription,
                    Score = x.Score,
                    ImportedType = x.ImportedType,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn,
                    Progress = x.Progress,
                    Source = x.Source,
                    MetricId = x.MetricId,
                    AssignmentTypeId = x.AssignmentTypeId,
                    CurrencyId = x.CurrencyId,
                    CurrentValue = x.CurrentValue,
                    TargetValue = x.TargetValue,
                    KrStatusId = x.KrStatusId,
                    CycleId = x.CycleId,
                    CurrencyCode = x.CurrencyCode,
                    GoalStatusId = x.GoalStatusId,
                    ContributorValue = x.ContributorValue,
                    StartValue = x.StartValue,
                    KeyNotes = x.KeyNotes,
                    TeamId = x.TeamId,
                    ImportedId = x.ImportedId,
                    UpdatedOn = x.UpdatedOn,
                    UpdatedBy = x.UpdatedBy,
                    DueDate = x.DueDate,
                    Owner = x.Owner,
                    GoalObjectiveId = x.GoalObjectiveId,
                }).GroupBy(x => x.GoalKeyId).Select(x => x.FirstOrDefault()).ToList();

                foreach (var contriKeys in contri)
                {
                    DueDateResponse due = new DueDateResponse();
                    if (isAlignment)
                    {
                        if (contriKeys.EmployeeId == userIdentity.EmployeeId)
                        {
                            contriKeys.StartDate = updateDueDateRequest.StartDate;
                            contriKeys.DueDate = updateDueDateRequest.EndDate;
                        }
                        else
                        {
                            if (contriKeys.StartDate < startDate)
                            {
                                contriKeys.StartDate = updateDueDateRequest.StartDate;
                            }
                            if (contriKeys.DueDate > dueDate)
                            {
                                contriKeys.DueDate = updateDueDateRequest.EndDate;
                            }
                        }
                    }
                    else
                    {
                        if (contriKeys.StartDate < startDate)
                        {
                            contriKeys.StartDate = updateDueDateRequest.StartDate;
                        }
                        if (contriKeys.DueDate > dueDate)
                        {
                            contriKeys.DueDate = updateDueDateRequest.EndDate;
                        }

                    }
                    contriKeys.UpdatedOn = DateTime.UtcNow;
                    contriKeys.UpdatedBy = userIdentity.EmployeeId;                   
                    await UpdateKeyResults(contriKeys);
                    due.DueDate = contriKeys.DueDate;
                    due.KrId.Add(contriKeys.GoalKeyId);
                    due.EmployeeId = (long)contriKeys.EmployeeId;
                    dueDates.Add(due);
                }

            }

            if (IsNotifications)
            {
                var notifications = dueDates.GroupBy(x => x.EmployeeId).Select(x => x.FirstOrDefault()).ToList();
                if (notifications.Count > 0)
                {
                    await Task.Run(async () =>
                    {
                        await notificationService.UpdateDueDateNotifications(notifications, updateDueDateRequest.GoalType, jwtToken, userIdentity.EmployeeId).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
            }

            return dueDates;
        }
        private async Task<long> UpdateTeamOkr(MyGoalsRequest myGoalsRequest, UserIdentity loginUser, string jwtToken)
        {
            var teamCount = 1;
            var allTeamEmployees = await commonService.GetTeamEmployees();
            foreach (var item in myGoalsRequest.TeamOkrRequests)
            {
                var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == item.TeamId);
                var employeeDetail = teamDetails?.TeamEmployees;
                var goalObjectiveForOwner = new GoalObjective();
                if (teamCount == 1 && employeeDetail != null)
                {
                    goalObjectiveForOwner = GetGoalObjective(myGoalsRequest.GoalObjectiveId);
                    goalObjectiveForOwner.ObjectiveDescription = myGoalsRequest.ObjectiveDescription;
                    goalObjectiveForOwner.ObjectiveName = myGoalsRequest.ObjectiveName;
                    goalObjectiveForOwner.IsPrivate = myGoalsRequest.IsPrivate;
                    goalObjectiveForOwner.StartDate = myGoalsRequest.StartDate;
                    goalObjectiveForOwner.EndDate = myGoalsRequest.EndDate;
                    goalObjectiveForOwner.GoalStatusId = myGoalsRequest.GoalStatusId;
                    goalObjectiveForOwner.GoalTypeId = myGoalsRequest.GoalTypeId;
                    goalObjectiveForOwner.UpdatedOn = DateTime.UtcNow;
                    goalObjectiveForOwner.UpdatedBy = loginUser.EmployeeId;
                    goalObjectiveForOwner.TeamId = teamDetails.OrganisationId; //To Update TeamId
                    goalObjectiveForOwner.Owner = myGoalsRequest.Owner;
                    goalObjectiveForOwner.EmployeeId = (myGoalsRequest.GoalStatusId == (int)GoalStatus.Public && myGoalsRequest.ImportedId == 0)
                        ? myGoalsRequest.Owner == 0 ? myGoalsRequest.EmployeeId : myGoalsRequest.Owner
                        : myGoalsRequest.EmployeeId;
                    goalObjectiveForOwner.LinkedObjectiveId = myGoalsRequest.LinkedObjectiveId;
                    goalObjectiveForOwner.IsCoachCreation = myGoalsRequest.GoalStatusId == (int)GoalStatus.Draft && myGoalsRequest.IsCoach;
                    UpdateObjective(goalObjectiveForOwner);
                    await CreateAndUpdateTeamKr(goalObjectiveForOwner, myGoalsRequest.MyGoalsDetails, loginUser, jwtToken, false);

                    if (goalObjectiveForOwner.GoalStatusId == (int)GoalStatus.Draft && myGoalsRequest.IsSavedAsDraft)
                    {
                        await Task.Run(async () =>
                        {
                            await notificationService.DraftOkrNotifications(jwtToken, loginUser, goalObjectiveForOwner).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                }
                else if (employeeDetail != null)
                {
                    goalObjectiveForOwner = Mapper.Map<GoalObjective>(myGoalsRequest);
                    goalObjectiveForOwner.GoalObjectiveId = 0;
                    goalObjectiveForOwner.CreatedBy = loginUser.EmployeeId;
                    goalObjectiveForOwner.Progress = (int)ProgressMaster.NotStarted;
                    goalObjectiveForOwner.Sequence = (int)GoalRequest.Sequence;
                    goalObjectiveForOwner.GoalStatusId = myGoalsRequest.GoalStatusId;
                    goalObjectiveForOwner.GoalTypeId = myGoalsRequest.GoalTypeId;
                    goalObjectiveForOwner.TeamId = teamDetails.OrganisationId;
                    goalObjectiveForOwner.Owner = myGoalsRequest.Owner;
                    goalObjectiveForOwner.EmployeeId = (myGoalsRequest.GoalStatusId == (int)GoalStatus.Public && myGoalsRequest.ImportedId == 0)
                        ? myGoalsRequest.Owner == 0 ? myGoalsRequest.EmployeeId : myGoalsRequest.Owner
                        : myGoalsRequest.EmployeeId;
                    goalObjectiveForOwner.LinkedObjectiveId = myGoalsRequest.LinkedObjectiveId;
                    goalObjectiveForOwner.IsCoachCreation = myGoalsRequest.GoalStatusId == (int)GoalStatus.Draft && myGoalsRequest.IsCoach;
                    InsertObjectiveNonAsync(goalObjectiveForOwner);
                    await CreateAndUpdateTeamKr(goalObjectiveForOwner, myGoalsRequest.MyGoalsDetails, loginUser, jwtToken, true);


                    if (goalObjectiveForOwner.GoalStatusId == (int)GoalStatus.Draft && myGoalsRequest.IsSavedAsDraft)
                    {
                        await Task.Run(async () =>
                        {
                            await notificationService.DraftOkrNotifications(jwtToken, loginUser, goalObjectiveForOwner).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                }

                teamCount++;
            }

            return 1;
        }
        private async Task<long> AcceptWithTeam(ContributorKeyResultRequest contriRequest, UserIdentity loginUser, string jwtToken, GoalKey goalKey)
        {
            var myGoalsRequest = await goalObjectiveRepo.GetQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.GoalObjectiveId == contriRequest.GoalObjectiveId && x.IsActive);
            if (myGoalsRequest != null)
            {
                var goalObjectiveForOwner = new GoalObjective();
                var goalKeyDetails = await goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.GoalObjectiveId == myGoalsRequest.GoalObjectiveId && x.IsActive).ToListAsync();
                if (goalKeyDetails.Count > 1 && myGoalsRequest.TeamId != contriRequest.TeamId)
                {
                    //// Check if already have team OKR with same team name 
                    var goalObjective = await goalObjectiveRepo.GetQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.ImportedId == myGoalsRequest.ImportedId && x.TeamId == contriRequest.TeamId && x.IsActive);
                    if(goalObjective != null)
                    {
                        goalObjectiveForOwner = goalObjective;
                        goalObjectiveForOwner.ObjectiveDescription = contriRequest.ObjectiveDescription;
                        goalObjectiveForOwner.ObjectiveName = contriRequest.ObjectiveName;
                        goalObjectiveForOwner.GoalStatusId = contriRequest.GoalStatusId;
                        goalObjectiveForOwner.GoalTypeId = contriRequest.GoalTypeId;
                        goalObjectiveForOwner.UpdatedOn = DateTime.UtcNow;
                        goalObjectiveForOwner.UpdatedBy = loginUser.EmployeeId;
                        goalObjectiveForOwner.TeamId = contriRequest.TeamId; //To Update TeamId
                        goalObjectiveForOwner.Owner = goalObjective.EmployeeId;
                        UpdateObjective(goalObjectiveForOwner);
                    }
                    else
                    {
                        goalObjectiveForOwner.Year = myGoalsRequest.Year;
                        goalObjectiveForOwner.IsPrivate = myGoalsRequest.IsPrivate;
                        goalObjectiveForOwner.ObjectiveName = contriRequest.ObjectiveName;
                        goalObjectiveForOwner.ObjectiveDescription = contriRequest.ObjectiveDescription;
                        goalObjectiveForOwner.GoalStatusId = contriRequest.GoalStatusId;
                        goalObjectiveForOwner.StartDate = contriRequest.StartDate;
                        goalObjectiveForOwner.EndDate = contriRequest.DueDate;
                        goalObjectiveForOwner.EmployeeId = myGoalsRequest.EmployeeId;
                        goalObjectiveForOwner.ImportedId = myGoalsRequest.ImportedId;
                        goalObjectiveForOwner.ImportedType = (int)GoalRequest.ImportedType;
                        goalObjectiveForOwner.GoalTypeId = contriRequest.GoalTypeId;
                        goalObjectiveForOwner.Progress = (int)ProgressMaster.NotStarted;
                        goalObjectiveForOwner.Sequence = (int)GoalRequest.Sequence;
                        goalObjectiveForOwner.Source = myGoalsRequest.Source;
                        goalObjectiveForOwner.CreatedBy = myGoalsRequest.CreatedBy;
                        goalObjectiveForOwner.Score = 0;
                        goalObjectiveForOwner.ObjectiveCycleId = myGoalsRequest.ObjectiveCycleId;
                        goalObjectiveForOwner.TeamId = contriRequest.TeamId;
                        goalObjectiveForOwner.Owner = myGoalsRequest.EmployeeId;
                        goalObjectiveForOwner.LinkedObjectiveId = myGoalsRequest.LinkedObjectiveId;

                        await InsertObjective(goalObjectiveForOwner);
                    }                    
                }
                else 
                {
                    if (myGoalsRequest.TeamId != contriRequest.TeamId)
                    {
                        //// Check if already have team OKR with same team name 
                        var goalObjective = await goalObjectiveRepo.GetQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.ImportedId == myGoalsRequest.ImportedId && x.TeamId == contriRequest.TeamId && x.IsActive);
                        if (goalObjective != null)
                        {
                            goalObjectiveForOwner = goalObjective;
                            goalObjectiveForOwner.ObjectiveDescription = contriRequest.ObjectiveDescription;
                            goalObjectiveForOwner.ObjectiveName = contriRequest.ObjectiveName;
                            goalObjectiveForOwner.GoalStatusId = contriRequest.GoalStatusId;
                            goalObjectiveForOwner.GoalTypeId = contriRequest.GoalTypeId;
                            goalObjectiveForOwner.UpdatedOn = DateTime.UtcNow;
                            goalObjectiveForOwner.UpdatedBy = loginUser.EmployeeId;
                            goalObjectiveForOwner.TeamId = contriRequest.TeamId; //To Update TeamId
                            goalObjectiveForOwner.Owner = goalObjective.EmployeeId;
                            UpdateObjective(goalObjectiveForOwner);
                        }
                        else
                        {
                            goalObjectiveForOwner.Year = myGoalsRequest.Year;
                            goalObjectiveForOwner.IsPrivate = myGoalsRequest.IsPrivate;
                            goalObjectiveForOwner.ObjectiveName = contriRequest.ObjectiveName;
                            goalObjectiveForOwner.ObjectiveDescription = contriRequest.ObjectiveDescription;
                            goalObjectiveForOwner.GoalStatusId = contriRequest.GoalStatusId;
                            goalObjectiveForOwner.StartDate = contriRequest.StartDate;
                            goalObjectiveForOwner.EndDate = contriRequest.DueDate;
                            goalObjectiveForOwner.EmployeeId = myGoalsRequest.EmployeeId;
                            goalObjectiveForOwner.ImportedId = myGoalsRequest.ImportedId;
                            goalObjectiveForOwner.ImportedType = (int)GoalRequest.ImportedType;
                            goalObjectiveForOwner.GoalTypeId = contriRequest.GoalTypeId;
                            goalObjectiveForOwner.Progress = (int)ProgressMaster.NotStarted;
                            goalObjectiveForOwner.Sequence = (int)GoalRequest.Sequence;
                            goalObjectiveForOwner.Source = myGoalsRequest.Source;
                            goalObjectiveForOwner.CreatedBy = myGoalsRequest.CreatedBy;
                            goalObjectiveForOwner.Score = 0;
                            goalObjectiveForOwner.ObjectiveCycleId = myGoalsRequest.ObjectiveCycleId;
                            goalObjectiveForOwner.TeamId = contriRequest.TeamId;
                            goalObjectiveForOwner.Owner = myGoalsRequest.EmployeeId;
                            goalObjectiveForOwner.LinkedObjectiveId = myGoalsRequest.LinkedObjectiveId;

                            await InsertObjective(goalObjectiveForOwner);
                        }
                    }
                    else
                    {
                        goalObjectiveForOwner = myGoalsRequest;
                        goalObjectiveForOwner.ObjectiveDescription = contriRequest.ObjectiveDescription;
                        goalObjectiveForOwner.ObjectiveName = contriRequest.ObjectiveName;
                        goalObjectiveForOwner.GoalStatusId = contriRequest.GoalStatusId;
                        goalObjectiveForOwner.GoalTypeId = contriRequest.GoalTypeId;
                        goalObjectiveForOwner.UpdatedOn = DateTime.UtcNow;
                        goalObjectiveForOwner.UpdatedBy = loginUser.EmployeeId;
                        goalObjectiveForOwner.TeamId = contriRequest.TeamId; //To Update TeamId
                        goalObjectiveForOwner.Owner = myGoalsRequest.EmployeeId;
                        UpdateObjective(goalObjectiveForOwner);
                    }
                    
                }

                await AcceptWithTeamKrUpdate(goalObjectiveForOwner, contriRequest, loginUser, jwtToken, goalKey, myGoalsRequest);
            }
            
            return 1;
        }
        private async Task<long> AcceptWithTeamKrUpdate(GoalObjective goalObjective, ContributorKeyResultRequest contriRequest, UserIdentity loginUser, string jwtToken, GoalKey goalKey, GoalObjective myGoalsRequest)
        {
            var data = goalKey;
            if (data != null && data.GoalKeyId > 0)
            {
                data.GoalObjectiveId = goalObjective.GoalObjectiveId;
                data.KeyDescription = contriRequest.KeyDescription;
                data.UpdatedBy = loginUser.EmployeeId;
                data.UpdatedOn = DateTime.UtcNow;
                data.StartDate = contriRequest.StartDate;
                data.CurrentValue = contriRequest.CurrentValue == 0 ? contriRequest.StartValue : contriRequest.CurrentValue;
                data.TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : contriRequest.TargetValue;
                data.KrStatusId = contriRequest.KrStatusId;
                data.GoalStatusId = contriRequest.GoalStatusId;
                data.KeyNotes = contriRequest.KeyNotes;
                data.StartValue = contriRequest.StartValue;
                data.TeamId = contriRequest.TeamId;
                data.Owner = goalObjective.Owner;
                UpdateKeyResultNonAsync(data);

                var goalKeyDetails = await goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.GoalObjectiveId == myGoalsRequest.GoalObjectiveId && x.IsActive).ToListAsync();
                if (goalKeyDetails.Count == 0)
                {
                    myGoalsRequest.IsActive = false;
                    myGoalsRequest.UpdatedBy = loginUser.EmployeeId;
                    myGoalsRequest.UpdatedOn = DateTime.UtcNow;
                    UpdateObjective(myGoalsRequest);
                }
            }

            var allTeamEmployees = await commonService.GetTeamEmployees();
            if (contriRequest.Contributors.Any())
            {
                var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
                foreach (var item in contriRequest.Contributors)
                {
                    if (item.IsTeamSelected)
                    {
                        var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == item.TeamId);
                        var employeeDetail = teamDetails?.TeamEmployees;
                        if (employeeDetail != null)
                        {
                            foreach (var employees in employeeDetail)
                            {
                                if (employees.EmployeeId != data.Owner)
                                {
                                    var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == employees.EmployeeId);
                                    var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                                    var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                        from data2 in cycle.QuarterDetails
                                                        where data2.IsCurrentQuarter
                                                        select new CycleLockDetails
                                                        {
                                                            StartDate = Convert.ToDateTime(data2.StartDate),
                                                            EndDate = Convert.ToDateTime(data2.EndDate),
                                                            OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                            Year = Int32.Parse(cycle.Year)
                                                        }).FirstOrDefault();


                                    var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, employees.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                                    ////if contributor exists or not
                                    var contributors = GetKeyContributor((int)GoalType.GoalKey, data.GoalKeyId, employees.EmployeeId);
                                    ////If null add the contributor otherwise update the existing contributor
                                    if (contributors == null)
                                    {
                                        var goalObjectives = new GoalObjective();
                                        var isObjectiveImported = IsTeamObjectiveImported(goalObjective.GoalObjectiveId, employees.EmployeeId, goalObjective.TeamId);
                                        if (!isObjectiveImported)
                                        {
                                            if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                            {
                                                goalObjectives.Year = item.Year;
                                                goalObjectives.ObjectiveCycleId = item.CycleId;
                                                goalObjectives.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                                goalObjectives.ObjectiveName = item.ObjectiveName;
                                                goalObjectives.IsPrivate = goalObjective.IsPrivate;
                                                goalObjectives.StartDate = goalObjective.StartDate;
                                                goalObjectives.EndDate = goalObjective.EndDate;
                                                goalObjectives.ImportedId = goalObjective.GoalObjectiveId;
                                                goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                                goalObjectives.Source = goalObjective.GoalObjectiveId;
                                                goalObjectives.CreatedBy = loginUser.EmployeeId;
                                                goalObjectives.Score = 0; ////item.Score;
                                                goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                                goalObjectives.EmployeeId = employees.EmployeeId;
                                                goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                                goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                                goalObjectives.GoalTypeId = goalObjective.GoalTypeId;
                                                goalObjectives.TeamId = goalObjective.TeamId;
                                                goalObjectives.Owner = goalObjective.Owner;
                                             ////   goalObjectives.LinkedObjectiveId = goalObjective.LinkedObjectiveId;
                                                InsertObjectiveNonAsync(goalObjectives);
                                            }
                                        }
                                        else
                                        {
                                            goalObjectives = GetTeamGoalObjectiveByImportedId(goalObjective.GoalObjectiveId, employees.EmployeeId, goalObjective.TeamId);
                                            goalObjectives.ObjectiveName = item.ObjectiveName;
                                            await UpdateObjectiveAsync(goalObjectives);
                                        }

                                        var goalKeys = new GoalKey
                                        {
                                            StartDate = item.StartDate < data.StartDate ? DateTime.Now : item.StartDate,
                                            DueDate = item.DueDate > data.DueDate ? data.DueDate : item.DueDate,
                                            GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                            KeyDescription = item.KeyResult,
                                            CreatedBy = loginUser.EmployeeId,
                                            Score = 0, //item.Score,
                                            ImportedType = (int)GoalRequest.KeyImportedType,
                                            EmployeeId = employees.EmployeeId,
                                            ImportedId = data.GoalKeyId,
                                            Source = data.ImportedId == 0 ? data.GoalKeyId : data.Source,
                                            Progress = (int)ProgressMaster.NotStarted,
                                            MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId,
                                            CurrencyId = data.CurrencyId,
                                            CurrentValue = item.StartValue,
                                            TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                            CycleId = item.CycleId,
                                            KrStatusId = item.KrStatusId,
                                            GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                            AssignmentTypeId = item.AssignmentTypeId,
                                            StartValue = item.StartValue,
                                            TeamId = goalObjective.TeamId,
                                            Owner = data.Owner, ////goalObjective.Owner,
                                            KeyNotes = data.KeyNotes
                                        };

                                        InsertKeyResultNonAsync(goalKeys);

                                        var krStatusMessage = new KrStatusMessage
                                        {
                                            AssignerGoalKeyId = data.GoalKeyId,
                                            AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                            KrAssignerMessage = item.KrAssignerMessage,
                                            CreatedOnAssigner = DateTime.Now,
                                            CreatedOnAssignee = DateTime.Now,
                                            IsActive = true

                                        };

                                        await InsertMessagesOfKr(krStatusMessage);

                                        if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                                        {
                                            await Task.Run(async () =>
                                            {
                                                await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, employees.EmployeeId, data.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                            }).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (item.EmployeeId != data.Owner)
                    {
                        var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                        var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                        var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                            from data2 in cycle.QuarterDetails
                                            where data2.IsCurrentQuarter
                                            select new CycleLockDetails
                                            {
                                                StartDate = Convert.ToDateTime(data2.StartDate),
                                                EndDate = Convert.ToDateTime(data2.EndDate),
                                                OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                Year = Int32.Parse(cycle.Year)
                                            }).FirstOrDefault();


                        var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, item.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                        ////if contributor exists or not
                        var contributors = GetKeyContributor((int)GoalType.GoalKey, data.GoalKeyId, item.EmployeeId);
                        ////If null add the contributor otherwise update the existing contributor
                        if (contributors == null)
                        {
                            var goalObjectives = new GoalObjective();
                            var isObjectiveImported = IsTeamObjectiveImported(goalObjective.GoalObjectiveId, item.EmployeeId, goalObjective.TeamId);
                            if (!isObjectiveImported)
                            {
                                if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                {
                                    goalObjectives.Year = item.Year;
                                    goalObjectives.ObjectiveCycleId = item.CycleId;
                                    goalObjectives.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                    goalObjectives.ObjectiveName = item.ObjectiveName;
                                    goalObjectives.IsPrivate = goalObjective.IsPrivate;
                                    goalObjectives.StartDate = goalObjective.StartDate;
                                    goalObjectives.EndDate = goalObjective.EndDate;
                                    goalObjectives.ImportedId = goalObjective.GoalObjectiveId;
                                    goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                    goalObjectives.Source = goalObjective.GoalObjectiveId;
                                    goalObjectives.CreatedBy = loginUser.EmployeeId;
                                    goalObjectives.Score = 0; ////item.Score;
                                    goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                    goalObjectives.EmployeeId = item.EmployeeId;
                                    goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                    goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                    goalObjectives.GoalTypeId = goalObjective.GoalTypeId;
                                    goalObjectives.TeamId = goalObjective.TeamId;
                                    goalObjectives.Owner = goalObjective.Owner;
                                 ////   goalObjectives.LinkedObjectiveId = goalObjective.LinkedObjectiveId;
                                    InsertObjectiveNonAsync(goalObjectives);
                                }
                            }
                            else
                            {
                                goalObjectives = GetTeamGoalObjectiveByImportedId(goalObjective.GoalObjectiveId, item.EmployeeId, goalObjective.TeamId);
                                goalObjectives.ObjectiveName = item.ObjectiveName;
                                await UpdateObjectiveAsync(goalObjectives);
                            }

                            var goalKeys = new GoalKey
                            {
                                StartDate = item.StartDate < data.StartDate ? DateTime.Now : item.StartDate,
                                DueDate = item.DueDate > data.DueDate ? data.DueDate : item.DueDate,
                                GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                KeyDescription = item.KeyResult,
                                CreatedBy = loginUser.EmployeeId,
                                Score = 0, //item.Score,
                                ImportedType = (int)GoalRequest.KeyImportedType,
                                EmployeeId = item.EmployeeId,
                                ImportedId = data.GoalKeyId,
                                Source = data.ImportedId == 0 ? data.GoalKeyId : data.Source,
                                Progress = (int)ProgressMaster.NotStarted,
                                MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId,
                                CurrencyId = data.CurrencyId,
                                CurrentValue = item.StartValue,
                                TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                CycleId = item.CycleId,
                                KrStatusId = item.KrStatusId,
                                GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                AssignmentTypeId = item.AssignmentTypeId,
                                StartValue = item.StartValue,
                                TeamId = goalObjective.TeamId,
                                Owner = data.Owner, ////goalObjective.Owner,
                                KeyNotes = data.KeyNotes
                            };

                            InsertKeyResultNonAsync(goalKeys);

                            var krStatusMessage = new KrStatusMessage
                            {
                                AssignerGoalKeyId = data.GoalKeyId,
                                AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                KrAssignerMessage = item.KrAssignerMessage,
                                CreatedOnAssigner = DateTime.Now,
                                CreatedOnAssignee = DateTime.Now,
                                IsActive = true

                            };

                            await InsertMessagesOfKr(krStatusMessage);

                            if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                            {
                                await Task.Run(async () =>
                                {
                                    await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, data.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            return 1;
        }
        private async Task<long> InsertTeamOkr(MyGoalsRequest myGoalsRequest, UserIdentity loginUser, string jwtToken)
        {
            foreach (var item in myGoalsRequest.TeamOkrRequests)
            {
                ////var teamDetails = commonService.GetTeamEmployeeByTeamId(item.TeamId, jwtToken);
                ////var employeeDetail = teamDetails?.TeamEmployees;
                var teamDetail = item;
                var goalObjectiveOwner = new GoalObjective();
                goalObjectiveOwner = Mapper.Map<GoalObjective>(myGoalsRequest);
                goalObjectiveOwner.CreatedBy = loginUser.EmployeeId;
                goalObjectiveOwner.EmployeeId =
                    (myGoalsRequest.GoalStatusId == (int)GoalStatus.Public && myGoalsRequest.ImportedId == 0)
                        ? myGoalsRequest.Owner == 0 ? myGoalsRequest.EmployeeId : myGoalsRequest.Owner
                        : myGoalsRequest.EmployeeId;
                goalObjectiveOwner.Progress = (int)ProgressMaster.NotStarted;
                goalObjectiveOwner.Sequence = (int)GoalRequest.Sequence;
                goalObjectiveOwner.GoalStatusId = myGoalsRequest.GoalStatusId;
                goalObjectiveOwner.GoalTypeId = myGoalsRequest.GoalTypeId;
                goalObjectiveOwner.TeamId = teamDetail.TeamId;
                goalObjectiveOwner.Owner = myGoalsRequest.Owner;
                goalObjectiveOwner.LinkedObjectiveId = myGoalsRequest.LinkedObjectiveId;
                goalObjectiveOwner.IsCoachCreation = myGoalsRequest.GoalStatusId == (int)GoalStatus.Draft && myGoalsRequest.IsCoach;
                await InsertObjective(goalObjectiveOwner);

                await CreateAndUpdateTeamKr(goalObjectiveOwner, myGoalsRequest.MyGoalsDetails, loginUser, jwtToken, true);

                ////if (myGoalsRequest.LinkedObjectiveId > 0 && goalObjectiveOwner.GoalStatusId == (int)GoalStatus.Public)
                ////{
                ////    var linkedObjectiveDetail = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == myGoalsRequest.LinkedObjectiveId && x.IsActive);
                ////    if (linkedObjectiveDetail != null)
                ////    {
                ////        await Task.Run(async () =>
                ////        {
                ////            await notificationService.VirtualLinkingNotifications(linkedObjectiveDetail.EmployeeId, loginUser, jwtToken).ConfigureAwait(false);
                ////        }).ConfigureAwait(false);
                ////    }
                ////}

                ////var value = myGoalsRequest.MyGoalsDetails.Any(x => x.Contributors.Any());
                ////if (myGoalsRequest.GoalStatusId == 2 && employeeDetail != null && !value)
                ////{
                ////    foreach (var employee in employeeDetail)
                ////    {
                ////        if (employee.EmployeeId != goalObjectiveOwner.Owner)
                ////        {
                ////            var goalObjective = new GoalObjective();
                ////            goalObjective = Mapper.Map<GoalObjective>(myGoalsRequest);
                ////            goalObjective.CreatedBy = loginUser.EmployeeId;
                ////            goalObjective.Progress = (int)ProgressMaster.NotStarted;
                ////            goalObjective.Sequence = (int)GoalRequest.Sequence;
                ////            goalObjective.GoalStatusId = myGoalsRequest.GoalStatusId;
                ////            goalObjective.GoalTypeId = myGoalsRequest.GoalTypeId;
                ////            goalObjective.TeamId = goalObjectiveOwner.TeamId;
                ////            goalObjective.ImportedId = goalObjectiveOwner.GoalObjectiveId;
                ////            goalObjective.ImportedType = 1;
                ////            goalObjective.EmployeeId = employee.EmployeeId;
                ////            goalObjective.Owner = goalObjectiveOwner.Owner;
                ////            goalObjective.LinkedObjectiveId = goalObjectiveOwner.LinkedObjectiveId;
                ////            await InsertObjective(goalObjective);
                ////            var ownerKeyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveOwner.GoalObjectiveId && x.IsActive).ToList();
                ////            await CreateTeamMemberKr(goalObjective, loginUser, jwtToken, ownerKeyDetails);
                ////        }
                ////    }
                ////}
            }

            return 1;
        }
        private async Task<long> CreateAndUpdateTeamKr(GoalObjective goalObjective, List<MyGoalsDetails> myGoalsDetails, UserIdentity loginUser, string jwtToken, bool isNewTeamKr)
        {
            foreach (var data in myGoalsDetails)
            {
                var goalKey = new GoalKey();
                data.GoalKeyId = isNewTeamKr ? 0 : data.GoalKeyId;
                var dueDateOwner = data.DueDate > goalObjective.EndDate ? goalObjective.EndDate : data.DueDate;
                if (data.GoalKeyId > 0)
                {
                    goalKey = GetGoalKeyDetails(data.GoalKeyId);
                    if (goalKey != null)
                    {
                        goalKey.DueDate = data.DueDate > goalObjective.EndDate ? goalObjective.EndDate : data.DueDate;
                        goalKey.KeyDescription = data.KeyDescription;
                        goalKey.UpdatedBy = loginUser.EmployeeId;
                        goalKey.UpdatedOn = DateTime.UtcNow;
                        goalKey.StartDate = data.StartDate;
                        goalKey.CurrentValue = data.CurrentValue == 0 ? data.StartValue : data.CurrentValue;
                        goalKey.CurrencyId = data.CurrencyId;
                        goalKey.CurrencyCode = data.CurrencyCode;
                        goalKey.TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : data.TargetValue;
                        goalKey.KrStatusId = data.KrStatusId;
                        goalKey.GoalStatusId = data.GoalStatusId;
                        goalKey.KeyNotes = data.KeyNotes;
                        goalKey.MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId;
                        goalKey.AssignmentTypeId = data.AssignmentTypeId;
                        goalKey.StartValue = data.StartValue;
                        goalKey.TeamId = goalObjective.TeamId;
                        goalKey.Owner = data.Owner; ////goalObjective.Owner;
                        goalKey.EmployeeId = goalKey.ImportedId > 0 ? goalKey.EmployeeId : data.Owner; ////(data.GoalStatusId == (int)GoalStatus.Public && data.ImportedId == 0) ? data.Owner == 0 ? goalKey.EmployeeId : data.Owner : goalKey.EmployeeId;
                        UpdateKeyResultNonAsync(goalKey);
                    }
                }
                else
                {
                    var goals = new GoalKey
                    {
                        DueDate = dueDateOwner,
                        GoalObjectiveId = goalObjective.GoalObjectiveId,
                        KeyDescription = data.KeyDescription,
                        CreatedBy = loginUser.EmployeeId,
                        Score = data.Score,
                        EmployeeId = data.Owner, ////(data.GoalStatusId == (int)GoalStatus.Public && data.ImportedId == 0) ? data.Owner == 0 ? goalKey.EmployeeId : data.Owner : goalKey.EmployeeId, //goalObjective.EmployeeId,
                        ImportedType = data.ImportedType,
                        ImportedId = data.ImportedId,
                        Source = data.Source,
                        Progress = (int)ProgressMaster.NotStarted,
                        StartDate = data.StartDate,
                        MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId,
                        AssignmentTypeId = data.AssignmentTypeId,
                        CurrencyId = data.CurrencyId,
                        CurrentValue = data.GoalKeyId > 0 ? data.CurrentValue : data.StartValue,
                        TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : data.TargetValue,
                        CycleId = goalObjective.ObjectiveCycleId,
                        CurrencyCode = data.CurrencyCode,
                        GoalStatusId = goalObjective.GoalStatusId,
                        ContributorValue = data.ContributorValue,
                        KrStatusId = data.KrStatusId,
                        StartValue = data.StartValue,
                        KeyNotes = data.KeyNotes,
                        TeamId = goalObjective.TeamId,
                        Owner = data.Owner ////goalObjective.Owner
                    };

                    var goalDetails = InsertKeyResultNonAsync(goals);
                    data.GoalKeyId = goalDetails.GoalKeyId;

                    var updateKrValue = new KrValueUpdate
                    {
                        GoalKeyId = goalDetails.GoalKeyId,
                        CurrentValue = goalDetails.CurrentValue,
                        Year = goalObjective.Year
                    };

                    await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goals);

                }

                var allTeamEmployees = await commonService.GetTeamEmployees();
                if (data.Contributors.Any())
                {
                    var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
                    foreach (var item in data.Contributors)
                    {
                        if (item.IsTeamSelected)
                        {
                            var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == item.TeamId);
                            var employeeDetail = teamDetails?.TeamEmployees;
                            if (employeeDetail != null)
                            {
                                foreach (var employees in employeeDetail)
                                {
                                    if (employees.EmployeeId != data.Owner)
                                    {
                                        var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == employees.EmployeeId);
                                        var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                                        var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                            from data2 in cycle.QuarterDetails
                                                            where data2.IsCurrentQuarter
                                                            select new CycleLockDetails
                                                            {
                                                                StartDate = Convert.ToDateTime(data2.StartDate),
                                                                EndDate = Convert.ToDateTime(data2.EndDate),
                                                                OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                                Year = Int32.Parse(cycle.Year)
                                                            }).FirstOrDefault();


                                        var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, employees.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                                        ////if contributor exists or not
                                        var contributors = GetKeyContributor((int)GoalType.GoalKey, data.GoalKeyId, employees.EmployeeId);
                                        ////If null add the contributor otherwise update the existing contributor
                                        if (contributors == null)
                                        {
                                            var goalObjectives = new GoalObjective();
                                            var isObjectiveImported = IsTeamObjectiveImported(goalObjective.GoalObjectiveId, employees.EmployeeId, goalObjective.TeamId);
                                            if (!isObjectiveImported)
                                            {
                                                if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                                {
                                                    goalObjectives.Year = item.Year;
                                                    goalObjectives.ObjectiveCycleId = item.CycleId;
                                                    goalObjectives.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                                    goalObjectives.ObjectiveName = item.ObjectiveName;
                                                    goalObjectives.IsPrivate = goalObjective.IsPrivate;
                                                    goalObjectives.StartDate = goalObjective.StartDate;
                                                    goalObjectives.EndDate = goalObjective.EndDate;
                                                    goalObjectives.ImportedId = goalObjective.GoalObjectiveId;
                                                    goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                                    goalObjectives.Source = goalObjective.GoalObjectiveId;
                                                    goalObjectives.CreatedBy = loginUser.EmployeeId;
                                                    goalObjectives.Score = 0; ////item.Score;
                                                    goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                                    goalObjectives.EmployeeId = employees.EmployeeId;
                                                    goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                                    goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                                    goalObjectives.GoalTypeId = goalObjective.GoalTypeId;
                                                    goalObjectives.TeamId = goalObjective.TeamId;
                                                    goalObjectives.Owner = goalObjective.Owner;
                                              ////      goalObjectives.LinkedObjectiveId = goalObjective.LinkedObjectiveId;
                                                    InsertObjectiveNonAsync(goalObjectives);
                                                }
                                            }
                                            else
                                            {
                                                goalObjectives = GetTeamGoalObjectiveByImportedId(goalObjective.GoalObjectiveId, employees.EmployeeId, goalObjective.TeamId);                                            
                                            }

                                            var goalKeys = new GoalKey
                                            {
                                                StartDate = item.StartDate < data.StartDate ? DateTime.Now : item.StartDate,
                                                DueDate = item.DueDate > data.DueDate ? data.DueDate : item.DueDate,
                                                GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                                KeyDescription = item.KeyResult,
                                                CreatedBy = loginUser.EmployeeId,
                                                Score = 0, //item.Score,
                                                ImportedType = (int)GoalRequest.KeyImportedType,
                                                EmployeeId = employees.EmployeeId,
                                                ImportedId = data.GoalKeyId,
                                                Source = data.ImportedId == 0 ? data.GoalKeyId : data.Source,
                                                Progress = (int)ProgressMaster.NotStarted,
                                                MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId,
                                                CurrencyId = data.CurrencyId,
                                                CurrentValue = item.StartValue,
                                                TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                                CycleId = item.CycleId,
                                                KrStatusId = item.KrStatusId,
                                                GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                                AssignmentTypeId = item.AssignmentTypeId,
                                                StartValue = item.StartValue,
                                                TeamId = goalObjective.TeamId,
                                                Owner = data.Owner, ////goalObjective.Owner,
                                                KeyNotes = data.KeyNotes
                                            };

                                            InsertKeyResultNonAsync(goalKeys);

                                            var krStatusMessage = new KrStatusMessage
                                            {
                                                AssignerGoalKeyId = data.GoalKeyId,
                                                AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                                KrAssignerMessage = item.KrAssignerMessage,
                                                CreatedOnAssigner = DateTime.Now,
                                                CreatedOnAssignee = DateTime.Now,
                                                IsActive = true

                                            };

                                            await InsertMessagesOfKr(krStatusMessage);

                                            if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                                            {
                                                await Task.Run(async () =>
                                                {
                                                    await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, employees.EmployeeId, data.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                                }).ConfigureAwait(false);
                                            }

                                            ////if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                            ////{
                                            ////    KrValueUpdate updateKrValue = new KrValueUpdate();
                                            ////    updateKrValue.GoalKeyId = goalKeys.GoalKeyId;
                                            ////    updateKrValue.CurrentValue = goalKeys.CurrentValue;
                                            ////    updateKrValue.Year = goalObjective.Year;

                                            ////    await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goalKeys);

                                            ////}

                                        }

                                        ////Updating the existing contributor
                                        else
                                        {
                                            if (item.AssignmentTypeId == contributors.AssignmentTypeId)
                                            {
                                                var contributorsOldGoalStatusId = contributors.GoalStatusId;
                                                contributors.StartDate = item.StartDate < data.StartDate ? DateTime.Now : item.StartDate;
                                                contributors.DueDate = item.DueDate > data.DueDate ? data.DueDate : item.DueDate;
                                                if (item.AssignmentTypeId == 2 && contributors.GoalObjectiveId > 0)
                                                {
                                                    var goalDetails = GetGoalObjective(contributors.GoalObjectiveId);
                                                    if (goalDetails != null)
                                                    {
                                                        goalDetails.ObjectiveName = item.ObjectiveName;
                                                        goalDetails.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                                        goalDetails.GoalStatusId = item.GoalStatusId;                                                     
                                                        UpdateObjective(goalDetails);
                                                    }
                                                }

                                                contributors.KeyDescription = item.KeyResult;
                                                contributors.KeyNotes = data.KeyNotes;
                                                contributors.CurrentValue = item.CurrentValue;
                                                contributors.TargetValue = item.TargetValue;
                                                contributors.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                                contributors.UpdatedOn = DateTime.UtcNow;
                                                contributors.UpdatedBy = loginUser.EmployeeId;

                                                UpdateKeyResultNonAsync(contributors);

                                                if (contributorsOldGoalStatusId == (int)GoalStatus.Draft && contributors.GoalStatusId != (int)GoalStatus.Draft)
                                                {
                                                    await Task.Run(async () =>
                                                    {
                                                        await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, employees.EmployeeId, data.GoalKeyId, contributors.GoalKeyId, contributors).ConfigureAwait(false);
                                                    }).ConfigureAwait(false);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (item.EmployeeId != data.Owner)
                        {
                            var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                            var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                from data2 in cycle.QuarterDetails
                                                where data2.IsCurrentQuarter
                                                select new CycleLockDetails
                                                {
                                                    StartDate = Convert.ToDateTime(data2.StartDate),
                                                    EndDate = Convert.ToDateTime(data2.EndDate),
                                                    OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                    Year = Int32.Parse(cycle.Year)
                                                }).FirstOrDefault();


                            var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, item.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                            ////if contributor exists or not
                            var contributors = GetKeyContributor((int)GoalType.GoalKey, data.GoalKeyId, item.EmployeeId);
                            ////If null add the contributor otherwise update the existing contributor
                            if (contributors == null)
                            {
                                var goalObjectives = new GoalObjective();
                                var isObjectiveImported = IsTeamObjectiveImported(goalObjective.GoalObjectiveId, item.EmployeeId, goalObjective.TeamId);
                                if (!isObjectiveImported)
                                {
                                    if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                    {
                                        goalObjectives.Year = item.Year;
                                        goalObjectives.ObjectiveCycleId = item.CycleId;
                                        goalObjectives.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                        goalObjectives.ObjectiveName = item.ObjectiveName;
                                        goalObjectives.IsPrivate = goalObjective.IsPrivate;
                                        goalObjectives.StartDate = goalObjective.StartDate;
                                        goalObjectives.EndDate = goalObjective.EndDate;
                                        goalObjectives.ImportedId = goalObjective.GoalObjectiveId;
                                        goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                        goalObjectives.Source = goalObjective.GoalObjectiveId;
                                        goalObjectives.CreatedBy = loginUser.EmployeeId;
                                        goalObjectives.Score = 0; ////item.Score;
                                        goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                        goalObjectives.EmployeeId = item.EmployeeId;
                                        goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                        goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                        goalObjectives.GoalTypeId = goalObjective.GoalTypeId;
                                        goalObjectives.TeamId = goalObjective.TeamId;
                                        goalObjectives.Owner = goalObjective.Owner;
                                   ////     goalObjectives.LinkedObjectiveId = goalObjective.LinkedObjectiveId;
                                        InsertObjectiveNonAsync(goalObjectives);
                                    }
                                }
                                else
                                {
                                    goalObjectives = GetTeamGoalObjectiveByImportedId(goalObjective.GoalObjectiveId, item.EmployeeId, goalObjective.TeamId);                                
                                }

                                var goalKeys = new GoalKey
                                {
                                    StartDate = item.StartDate < data.StartDate ? DateTime.Now : item.StartDate,
                                    DueDate = item.DueDate > data.DueDate ? data.DueDate : item.DueDate,
                                    GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                    KeyDescription = item.KeyResult,
                                    CreatedBy = loginUser.EmployeeId,
                                    Score = 0, //item.Score,
                                    ImportedType = (int)GoalRequest.KeyImportedType,
                                    EmployeeId = item.EmployeeId,
                                    ImportedId = data.GoalKeyId,
                                    Source = data.ImportedId == 0 ? data.GoalKeyId : data.Source,
                                    Progress = (int)ProgressMaster.NotStarted,
                                    MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId,
                                    CurrencyId = data.CurrencyId,
                                    CurrentValue = item.StartValue,
                                    TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                    CycleId = item.CycleId,
                                    KrStatusId = item.KrStatusId,
                                    GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                    AssignmentTypeId = item.AssignmentTypeId,
                                    StartValue = item.StartValue,
                                    TeamId = goalObjective.TeamId,
                                    Owner = data.Owner, ////goalObjective.Owner,
                                    KeyNotes = data.KeyNotes
                                };

                                InsertKeyResultNonAsync(goalKeys);

                                var krStatusMessage = new KrStatusMessage
                                {
                                    AssignerGoalKeyId = data.GoalKeyId,
                                    AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                    KrAssignerMessage = item.KrAssignerMessage,
                                    CreatedOnAssigner = DateTime.Now,
                                    CreatedOnAssignee = DateTime.Now,
                                    IsActive = true

                                };

                                await InsertMessagesOfKr(krStatusMessage);

                                if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                                {
                                    await Task.Run(async () =>
                                    {
                                        await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, data.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                    }).ConfigureAwait(false);
                                }

                                ////if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                ////{
                                ////    KrValueUpdate updateKrValue = new KrValueUpdate();
                                ////    updateKrValue.GoalKeyId = goalKeys.GoalKeyId;
                                ////    updateKrValue.CurrentValue = goalKeys.CurrentValue;
                                ////    updateKrValue.Year = goalObjective.Year;

                                ////    await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwtToken, goalKeys);

                                ////}

                            }

                            ////Updating the existing contributor
                            else
                            {
                                if (item.AssignmentTypeId == contributors.AssignmentTypeId)
                                {
                                    var contributorsOldGoalStatusId = contributors.GoalStatusId;
                                    contributors.StartDate = item.StartDate < data.StartDate ? DateTime.Now : item.StartDate;
                                    contributors.DueDate = item.DueDate > data.DueDate ? data.DueDate : item.DueDate;
                                    if (item.AssignmentTypeId == 2 && contributors.GoalObjectiveId > 0)
                                    {
                                        var goalDetails = GetGoalObjective(contributors.GoalObjectiveId);
                                        if (goalDetails != null)
                                        {
                                            goalDetails.ObjectiveName = item.ObjectiveName;
                                            goalDetails.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                            goalDetails.GoalStatusId = item.GoalStatusId;                                          
                                            UpdateObjective(goalDetails);
                                        }
                                    }

                                    contributors.KeyDescription = item.KeyResult;
                                    contributors.KeyNotes = data.KeyNotes;
                                    contributors.CurrentValue = item.CurrentValue;
                                    contributors.TargetValue = item.TargetValue;
                                    contributors.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                    contributors.UpdatedOn = DateTime.UtcNow;
                                    contributors.UpdatedBy = loginUser.EmployeeId;

                                    UpdateKeyResultNonAsync(contributors);

                                    if (contributorsOldGoalStatusId == (int)GoalStatus.Draft && contributors.GoalStatusId != (int)GoalStatus.Draft)
                                    {
                                        await Task.Run(async () =>
                                        {
                                            await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, data.GoalKeyId, contributors.GoalKeyId, contributors).ConfigureAwait(false);
                                        }).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return 1;
        }
        private async Task<GoalKey> InsertKeyResults(DateTime dueDate, GoalObjective goalObjective, long loginEmpId, MyGoalsDetails data, UserIdentity loginUser, string jwToken)
        {
            var goalKey = new GoalKey
            {
                DueDate = dueDate,
                GoalObjectiveId = goalObjective.GoalObjectiveId,
                KeyDescription = data.KeyDescription,
                CreatedBy = loginEmpId,
                Score = data.Score,
                EmployeeId = (data.GoalStatusId == (int)GoalStatus.Public && data.ImportedId == 0)
                    ? data.Owner == 0 ? goalObjective.EmployeeId : data.Owner
                    : goalObjective.EmployeeId,
                ImportedType = data.ImportedType,
                ImportedId = data.ImportedId,
                Source = data.Source,
                Progress = (int)ProgressMaster.NotStarted,
                StartDate = data.StartDate,
                MetricId = data.MetricId == 0 ? (int)Metrics.NoUnits : data.MetricId,
                AssignmentTypeId = data.AssignmentTypeId,
                CurrencyId = data.CurrencyId,
                CurrentValue = data.GoalKeyId > 0 ? data.CurrentValue : data.StartValue,
                TargetValue = data.MetricId == (int)Metrics.Boolean || data.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : data.TargetValue,
                CycleId = goalObjective.ObjectiveCycleId,
                CurrencyCode = data.CurrencyCode,
                GoalStatusId = goalObjective.GoalStatusId,
                ContributorValue = data.ContributorValue,
                KrStatusId = data.KrStatusId,
                StartValue = data.StartValue,
                KeyNotes = data.KeyNotes,
                Owner = data.Owner

            };

            if (data.GoalKeyId > 0)
            {
                await UpdateKeyResults(goalKey);
            }
            else
            {
                var goalDetail = await InsertKeyResults(goalKey);
                data.GoalKeyId = goalDetail.GoalKeyId;

                var updateKrValue = new KrValueUpdate
                {
                    GoalKeyId = data.GoalKeyId,
                    CurrentValue = goalDetail.CurrentValue,
                    Year = goalObjective.Year
                };


                await progressBarCalculationService.UpdateKrValue(updateKrValue, loginUser, jwToken, goalKey);
            }
            return goalKey;

        }
        private async Task<long> BecomeContributorWithTeam(AddContributorRequest addContributorRequest, UserIdentity loginUser, string jwtToken, GoalKey goalKey)
        {
            var myGoalsRequest = GetGoalObjective(addContributorRequest.GoalObjectiveId);
            if (myGoalsRequest != null)
            {
                var goalObjectiveForOwner = new GoalObjective();                        
                var isObjectiveImported = IsTeamObjectiveImported(myGoalsRequest.GoalObjectiveId, addContributorRequest.EmployeeId, addContributorRequest.TeamId);
                if (!isObjectiveImported)
                {
                    goalObjectiveForOwner.Year = myGoalsRequest.Year;
                    goalObjectiveForOwner.IsPrivate = myGoalsRequest.IsPrivate;
                    goalObjectiveForOwner.ObjectiveName = addContributorRequest.ObjectiveName;
                    goalObjectiveForOwner.GoalStatusId = addContributorRequest.GoalStatusId;
                    goalObjectiveForOwner.StartDate = addContributorRequest.StartDate;
                    goalObjectiveForOwner.EndDate = addContributorRequest.DueDate;
                    goalObjectiveForOwner.EmployeeId = addContributorRequest.EmployeeId;
                    goalObjectiveForOwner.ImportedId = myGoalsRequest.GoalObjectiveId;
                    goalObjectiveForOwner.ImportedType = (int)GoalRequest.ImportedType;
                    goalObjectiveForOwner.GoalTypeId = addContributorRequest.GoalTypeId;
                    goalObjectiveForOwner.Progress = (int)ProgressMaster.NotStarted;
                    goalObjectiveForOwner.Sequence = (int)GoalRequest.Sequence;
                    goalObjectiveForOwner.Source = myGoalsRequest.GoalObjectiveId;
                    goalObjectiveForOwner.CreatedBy = loginUser.EmployeeId;
                    goalObjectiveForOwner.Score = 0;
                    goalObjectiveForOwner.ObjectiveCycleId = addContributorRequest.ObjectiveCycleId;
                    goalObjectiveForOwner.TeamId = addContributorRequest.TeamId;
                    goalObjectiveForOwner.Owner = addContributorRequest.EmployeeId;
                 ////   goalObjectiveForOwner.LinkedObjectiveId = myGoalsRequest.LinkedObjectiveId;

                    await InsertObjective(goalObjectiveForOwner);
                }
                else
                {
                    goalObjectiveForOwner = GetTeamGoalObjectiveByImportedId(myGoalsRequest.GoalObjectiveId, addContributorRequest.EmployeeId, addContributorRequest.TeamId);
                    goalObjectiveForOwner.ObjectiveName = addContributorRequest.ObjectiveName;
                    await UpdateObjectiveAsync(goalObjectiveForOwner);
                }
                
                await BecomeContributorWithTeamKrUpdate(goalObjectiveForOwner, addContributorRequest, loginUser, jwtToken, goalKey);
            }
            return 1;      
        }
        private async Task<long> BecomeContributorWithTeamKrUpdate(GoalObjective goalObjective, AddContributorRequest contriRequest, UserIdentity loginUser, string jwtToken, GoalKey goalKey)
        {
            var goals = new GoalKey
            {
                StartDate = contriRequest.StartDate < goalKey.StartDate ? DateTime.Now : contriRequest.StartDate,
                DueDate = contriRequest.DueDate > goalKey.DueDate ? goalKey.DueDate : contriRequest.DueDate,
                GoalObjectiveId = goalObjective.GoalObjectiveId,
                KeyDescription = contriRequest.KeyDescription,
                CreatedBy = loginUser.EmployeeId,
                Score = 0, //addContributorRequest.Score,
                ImportedType = (int)GoalRequest.KeyImportedType,
                EmployeeId = contriRequest.EmployeeId,
                ImportedId = goalKey.GoalKeyId,
                Source = goalKey.ImportedId == 0 ? goalKey.GoalKeyId : goalKey.Source,
                Progress = (int)ProgressMaster.NotStarted,
                MetricId = contriRequest.MetricId == 0 ? (int)Metrics.NoUnits : contriRequest.MetricId,
                CurrencyId = contriRequest.CurrencyId,
                CurrentValue = contriRequest.StartValue,
                TargetValue = goalKey.MetricId == (int)Metrics.Boolean || goalKey.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : contriRequest.TargetValue,
                CycleId = contriRequest.ObjectiveCycleId,
                StartValue = contriRequest.StartValue,
                KrStatusId = contriRequest.KrStatusId,
                AssignmentTypeId = contriRequest.AssignmentTypeId,
                GoalStatusId = contriRequest.GoalStatusId,
                Owner = contriRequest.EmployeeId,
                TeamId = contriRequest.TeamId
            };
            await InsertKeyResults(goals);

            var krStatusMessages = new KrStatusMessage
            {
                AssignerGoalKeyId = goalKey.GoalKeyId,
                AssigneeGoalKeyId = goals.GoalKeyId,
                KrAssigneeMessage = contriRequest.KrAssignerMessage,
                CreatedOnAssigner = DateTime.UtcNow,
                CreatedOnAssignee = DateTime.UtcNow,
                IsActive = true
            };
            await InsertMessagesOfKr(krStatusMessages);

            await Task.Run(async () =>
            {
                await notificationService.AligningParentObjective(jwtToken, loginUser.EmployeeId, contriRequest, goals).ConfigureAwait(false);
            }).ConfigureAwait(false);

            if (contriRequest.Contributors.Any())
            {
                var getContributorsOrganization = commonService.GetAllUserFromUsers(jwtToken);
                foreach (var item in contriRequest.Contributors)
                {
                    if (item.IsTeamSelected)
                    {
                        var allTeamEmployees = await commonService.GetTeamEmployees();
                        var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == item.TeamId);
                        var employeeDetail = teamDetails?.TeamEmployees;
                        if (employeeDetail != null)
                        {
                            foreach (var employees in employeeDetail)
                            {
                                if (employees.EmployeeId != goals.Owner)
                                {
                                    var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == employees.EmployeeId);
                                    var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                                    var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                                        from data2 in cycle.QuarterDetails
                                                        where data2.IsCurrentQuarter
                                                        select new CycleLockDetails
                                                        {
                                                            StartDate = Convert.ToDateTime(data2.StartDate),
                                                            EndDate = Convert.ToDateTime(data2.EndDate),
                                                            OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                            Year = Int32.Parse(cycle.Year)
                                                        }).FirstOrDefault();


                                    var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, employees.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                                    ////if contributor exists or not
                                    var contributors = GetKeyContributor((int)GoalType.GoalKey, goals.GoalKeyId, employees.EmployeeId);
                                    ////If null add the contributor otherwise update the existing contributor
                                    if (contributors == null)
                                    {
                                        var goalObjectives = new GoalObjective();
                                        var isObjectiveImported = IsTeamObjectiveImported(goalObjective.GoalObjectiveId, employees.EmployeeId, goalObjective.TeamId);
                                        if (!isObjectiveImported)
                                        {
                                            if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                            {
                                                goalObjectives.Year = item.Year;
                                                goalObjectives.ObjectiveCycleId = item.CycleId;
                                                goalObjectives.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                                goalObjectives.ObjectiveName = item.ObjectiveName;
                                                goalObjectives.IsPrivate = goalObjective.IsPrivate;
                                                goalObjectives.StartDate = goalObjective.StartDate;
                                                goalObjectives.EndDate = goalObjective.EndDate;
                                                goalObjectives.ImportedId = goalObjective.GoalObjectiveId;
                                                goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                                goalObjectives.Source = goalObjective.GoalObjectiveId;
                                                goalObjectives.CreatedBy = loginUser.EmployeeId;
                                                goalObjectives.Score = 0; ////item.Score;
                                                goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                                goalObjectives.EmployeeId = employees.EmployeeId;
                                                goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                                goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                                goalObjectives.GoalTypeId = goalObjective.GoalTypeId;
                                                goalObjectives.TeamId = goalObjective.TeamId;
                                                goalObjectives.Owner = goalObjective.Owner;
                                               //// goalObjectives.LinkedObjectiveId = goalObjective.LinkedObjectiveId;
                                                InsertObjectiveNonAsync(goalObjectives);
                                            }
                                        }
                                        else
                                        {
                                            goalObjectives = GetTeamGoalObjectiveByImportedId(goalObjective.GoalObjectiveId, employees.EmployeeId, goalObjective.TeamId);
                                            goalObjectives.ObjectiveName = item.ObjectiveName;
                                            await UpdateObjectiveAsync(goalObjectives);
                                        }

                                        var goalKeys = new GoalKey
                                        {
                                            StartDate = item.StartDate < goals.StartDate ? DateTime.Now : item.StartDate,
                                            DueDate = item.DueDate > goals.DueDate ? goals.DueDate : item.DueDate,
                                            GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                            KeyDescription = item.KeyResult,
                                            CreatedBy = loginUser.EmployeeId,
                                            Score = 0, //item.Score,
                                            ImportedType = (int)GoalRequest.KeyImportedType,
                                            EmployeeId = employees.EmployeeId,
                                            ImportedId = goals.GoalKeyId,
                                            Source = goals.ImportedId == 0 ? goals.GoalKeyId : goals.Source,
                                            Progress = (int)ProgressMaster.NotStarted,
                                            MetricId = goals.MetricId == 0 ? (int)Metrics.NoUnits : goals.MetricId,
                                            CurrencyId = goals.CurrencyId,
                                            CurrentValue = item.StartValue,
                                            TargetValue = goals.MetricId == (int)Metrics.Boolean || goals.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                            CycleId = item.CycleId,
                                            KrStatusId = item.KrStatusId,
                                            GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                            AssignmentTypeId = item.AssignmentTypeId,
                                            StartValue = item.StartValue,
                                            TeamId = goalObjective.TeamId,
                                            Owner = goals.Owner, ////goalObjective.Owner,
                                            KeyNotes = goals.KeyNotes
                                        };

                                        InsertKeyResultNonAsync(goalKeys);

                                        var krStatusMessage = new KrStatusMessage
                                        {
                                            AssignerGoalKeyId = goals.GoalKeyId,
                                            AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                            KrAssignerMessage = item.KrAssignerMessage,
                                            CreatedOnAssigner = DateTime.Now,
                                            CreatedOnAssignee = DateTime.Now,
                                            IsActive = true

                                        };

                                        await InsertMessagesOfKr(krStatusMessage);

                                        if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                                        {
                                            await Task.Run(async () =>
                                            {
                                                await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, employees.EmployeeId, goals.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                            }).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (item.EmployeeId != goals.Owner)
                    {
                        var getContributorsDetails = getContributorsOrganization.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                        var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(getContributorsDetails.OrganisationID, jwtToken);

                        var currentCycle = (from cycle in cycleDurationDetails.CycleDetails
                                            from data2 in cycle.QuarterDetails
                                            where data2.IsCurrentQuarter
                                            select new CycleLockDetails
                                            {
                                                StartDate = Convert.ToDateTime(data2.StartDate),
                                                EndDate = Convert.ToDateTime(data2.EndDate),
                                                OrganisationCycleId = (int)data2.OrganisationCycleId,
                                                Year = Int32.Parse(cycle.Year)
                                            }).FirstOrDefault();


                        var isOkrLocked = commonService.IsOkrLocked(currentCycle.StartDate, currentCycle.EndDate, item.EmployeeId, currentCycle.OrganisationCycleId, currentCycle.Year, jwtToken).Result;

                        ////if contributor exists or not
                        var contributors = GetKeyContributor((int)GoalType.GoalKey, goals.GoalKeyId, item.EmployeeId);
                        ////If null add the contributor otherwise update the existing contributor
                        if (contributors == null)
                        {
                            var goalObjectives = new GoalObjective();
                            var isObjectiveImported = IsTeamObjectiveImported(goalObjective.GoalObjectiveId, item.EmployeeId, goalObjective.TeamId);
                            if (!isObjectiveImported)
                            {
                                if (item.AssignmentTypeId == (int)AssignmentType.WithParentObjective)
                                {
                                    goalObjectives.Year = item.Year;
                                    goalObjectives.ObjectiveCycleId = item.CycleId;
                                    goalObjectives.ObjectiveDescription = goalObjective.ObjectiveDescription;
                                    goalObjectives.ObjectiveName = item.ObjectiveName;
                                    goalObjectives.IsPrivate = goalObjective.IsPrivate;
                                    goalObjectives.StartDate = goalObjective.StartDate;
                                    goalObjectives.EndDate = goalObjective.EndDate;
                                    goalObjectives.ImportedId = goalObjective.GoalObjectiveId;
                                    goalObjectives.ImportedType = (int)GoalRequest.ImportedType;
                                    goalObjectives.Source = goalObjective.GoalObjectiveId;
                                    goalObjectives.CreatedBy = loginUser.EmployeeId;
                                    goalObjectives.Score = 0; ////item.Score;
                                    goalObjectives.Progress = (int)ProgressMaster.NotStarted;
                                    goalObjectives.EmployeeId = item.EmployeeId;
                                    goalObjectives.Sequence = (int)GoalRequest.Sequence;
                                    goalObjectives.GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId;
                                    goalObjectives.GoalTypeId = goalObjective.GoalTypeId;
                                    goalObjectives.TeamId = goalObjective.TeamId;
                                    goalObjectives.Owner = goalObjective.Owner;
                                   //// goalObjectives.LinkedObjectiveId = goalObjective.LinkedObjectiveId;
                                    InsertObjectiveNonAsync(goalObjectives);
                                }
                            }
                            else
                            {
                                goalObjectives = GetTeamGoalObjectiveByImportedId(goalObjective.GoalObjectiveId, item.EmployeeId, goalObjective.TeamId);
                                goalObjectives.ObjectiveName = item.ObjectiveName;
                                await UpdateObjectiveAsync(goalObjectives);
                            }

                            var goalKeys = new GoalKey
                            {
                                StartDate = item.StartDate < goals.StartDate ? DateTime.Now : item.StartDate,
                                DueDate = item.DueDate > goals.DueDate ? goals.DueDate : item.DueDate,
                                GoalObjectiveId = item.AssignmentTypeId == (int)AssignmentType.WithParentObjective ? goalObjectives.GoalObjectiveId : 0,
                                KeyDescription = item.KeyResult,
                                CreatedBy = loginUser.EmployeeId,
                                Score = 0, //item.Score,
                                ImportedType = (int)GoalRequest.KeyImportedType,
                                EmployeeId = item.EmployeeId,
                                ImportedId = goals.GoalKeyId,
                                Source = goals.ImportedId == 0 ? goals.GoalKeyId : goals.Source,
                                Progress = (int)ProgressMaster.NotStarted,
                                MetricId = goals.MetricId == 0 ? (int)Metrics.NoUnits : goals.MetricId,
                                CurrencyId = goals.CurrencyId,
                                CurrentValue = item.StartValue,
                                TargetValue = goals.MetricId == (int)Metrics.Boolean || goals.MetricId == (int)Metrics.NoUnits ? Constants.DefaultTargetValue : item.TargetValue,
                                CycleId = item.CycleId,
                                KrStatusId = item.KrStatusId,
                                GoalStatusId = isOkrLocked.IsGaolLocked ? (int)GoalStatus.Archived : item.GoalStatusId,
                                AssignmentTypeId = item.AssignmentTypeId,
                                StartValue = item.StartValue,
                                TeamId = goalObjective.TeamId,
                                Owner = goals.Owner, ////goalObjective.Owner,
                                KeyNotes = goals.KeyNotes
                            };

                            InsertKeyResultNonAsync(goalKeys);

                            var krStatusMessage = new KrStatusMessage
                            {
                                AssignerGoalKeyId = goals.GoalKeyId,
                                AssigneeGoalKeyId = goalKeys.GoalKeyId,
                                KrAssignerMessage = item.KrAssignerMessage,
                                CreatedOnAssigner = DateTime.Now,
                                CreatedOnAssignee = DateTime.Now,
                                IsActive = true

                            };

                            await InsertMessagesOfKr(krStatusMessage);

                            if (goalKeys.GoalStatusId != (int)GoalStatus.Draft)
                            {
                                await Task.Run(async () =>
                                {
                                    await notificationService.TeamKeyContributorsNotifications(jwtToken, loginUser.EmployeeId, item.EmployeeId, goals.GoalKeyId, goalKeys.GoalKeyId, goalKeys).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            return 1;
        }
        #endregion
    }
}
