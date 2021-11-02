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
    public class ProgressBarCalculationService : BaseService, IProgressBarCalculationService
    {
        private readonly IRepositoryAsync<GoalKey> goalKeyRepo;
        private readonly ICommonService commonService;
        private readonly IRepositoryAsync<GoalObjective> goalObjectiveRepo;
        private readonly IRepositoryAsync<GoalKeyHistory> goalKeyHistoryRepo;
        private readonly INotificationService notificationService;
        public ProgressBarCalculationService(IServicesAggregator servicesAggregateService, ICommonService commonServices, INotificationService notification) : base(servicesAggregateService)
        {
            goalKeyRepo = UnitOfWorkAsync.RepositoryAsync<GoalKey>();
            goalObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<GoalObjective>();
            commonService = commonServices;
            goalKeyHistoryRepo = UnitOfWorkAsync.RepositoryAsync<GoalKeyHistory>();
            notificationService = notification;

        }
        public async Task<KrCalculationResponse> UpdateKrValue(KrValueUpdate krValueUpdate, UserIdentity userIdentity, string token, GoalKey goalKeyRecord = null, bool isScoreUpdate = false)
        {
            var krCalculationResponse = new KrCalculationResponse();
            var isAchieved = false;

            var keyDetails = new GoalKey();
            if (goalKeyRecord == null)
            {
                keyDetails = goalKeyRepo.GetQueryable().AsNoTracking().FirstOrDefault(x => x.GoalKeyId == krValueUpdate.GoalKeyId && x.IsActive);
            }
            else
            {
                keyDetails = goalKeyRecord;
            }

            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(userIdentity.OrganisationId, token);
            CycleDetails cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == krValueUpdate.Year);
            var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == keyDetails.CycleId);


            if (quarterDetails != null)
            {
                int mailLoop = 0;
                if (keyDetails.MetricId == (int)MetricType.NoUnits)
                {
                    long? previousContributor = 0;
                    keyDetails.CurrentValue = krValueUpdate.CurrentValue;
                    keyDetails.UpdatedBy = userIdentity.EmployeeId;
                    keyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

                    var allContributors = goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.ImportedId == keyDetails.GoalKeyId && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive).ToList();

                    keyDetails.Score = allContributors.Count > 0 ? Math.Round((allContributors.Select(x => x.Score).Sum() + keyDetails.CurrentValue) / (allContributors.Count + 1), 2) : krValueUpdate.CurrentValue;
                    var progress = commonService.GetProgressIdWithFormula(keyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), keyDetails.Score, cycleDurationDetails.CycleDurationId);
                    keyDetails.Progress = progress;

                    UpdateGoalKeyAndMaintainHistory(keyDetails, userIdentity);

                    if (keyDetails.ImportedId > 0)
                    {
                        var importedId = keyDetails.ImportedId;                       
                        do
                        {
                            mailLoop += 1;
                            GoalKey goalKeyDetails = new GoalKey();
                            goalKeyDetails = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == importedId);

                            if (goalKeyDetails == null)
                                break;

                            var upperLevelContributors = goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.ImportedId == goalKeyDetails.GoalKeyId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToList();

                            goalKeyDetails.Score = Math.Round((upperLevelContributors.Select(x => x.Score).Sum() + goalKeyDetails.CurrentValue) / (upperLevelContributors.Count + 1), 2);
                            var sourceProgress = commonService.GetProgressIdWithFormula(keyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), keyDetails.Score, cycleDurationDetails.CycleDurationId);
                            goalKeyDetails.Progress = sourceProgress;
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

                            UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);


                            if (isScoreUpdate && mailLoop == 1)
                            {
                                await Task.Run(async () =>
                                {
                                    await notificationService.UpdateProgress(previousContributor == 0 ? userIdentity.EmployeeId : previousContributor, goalKeyDetails, token, krValueUpdate.GoalKeyId, krValueUpdate.CurrentValue, krValueUpdate.Year).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }

                            previousContributor = goalKeyDetails.EmployeeId;

                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                GoalObjective objectiveDetails = new GoalObjective();
                                objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                                var objectiveKeyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive);

                                objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                goalObjectiveRepo.Update(objectiveDetails);
                                UnitOfWorkAsync.SaveChanges();
                            }
                            importedId = goalKeyDetails.ImportedId;



                        }
                        while (importedId != 0);
                    }

                }


                else if (keyDetails.MetricId == (int)Metrics.Boolean)
                {
                    var contribution = commonService.GetKRContributorAsync(2, keyDetails.GoalKeyId).Result;
                    keyDetails.CurrentValue = krValueUpdate.CurrentValue;
                    isAchieved = contribution.Any(x => x.Score == 100);

                    if (keyDetails.CurrentValue > 0 || keyDetails.ContributorValue > 0 || (contribution.Count > 0 && contribution.Any(x => x.Score == 100)))
                    {

                        keyDetails.Score = 100;
                    }
                    else
                    {
                        keyDetails.Score = 0;
                    }
                    keyDetails.UpdatedBy = userIdentity.EmployeeId;
                    keyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                    var progress = commonService.GetProgressIdWithFormula(keyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), keyDetails.Score, cycleDurationDetails.CycleDurationId);
                    keyDetails.Progress = progress;

                    UpdateGoalKeyAndMaintainHistory(keyDetails, userIdentity);
                    long? previousContributor = 0;

                    if (keyDetails.ImportedId > 0)
                    {
                        var importedId = keyDetails.ImportedId;
                        do
                        {
                            mailLoop += 1;
                            GoalKey goalKeyDetails = new GoalKey();
                            goalKeyDetails = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == importedId);

                            if (goalKeyDetails == null)
                                break;

                            var allContributors = commonService.GetKRContributorAsync(2, goalKeyDetails.GoalKeyId).Result;

                            goalKeyDetails.ContributorValue = (allContributors.Count > 0 && allContributors.Any(x => x.Score == 100)) ? 100 : krValueUpdate.CurrentValue;
                            if (goalKeyDetails.CurrentValue > 0 || goalKeyDetails.ContributorValue > 0 || (allContributors.Count > 0 && allContributors.Any(x => x.Score == 100)))
                            {
                                goalKeyDetails.Score = 100;
                            }
                            else
                            {
                                goalKeyDetails.Score = 0;
                            }
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                            var sourceProgress = commonService.GetProgressIdWithFormula(goalKeyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), goalKeyDetails.Score, cycleDurationDetails.CycleDurationId);
                            goalKeyDetails.Progress = sourceProgress;

                            UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);

                            importedId = goalKeyDetails.ImportedId;


                            if (isScoreUpdate && mailLoop == 1)
                            {
                                await Task.Run(async () =>
                                {
                                    await notificationService.UpdateProgress(previousContributor == 0 ? userIdentity.EmployeeId : previousContributor, goalKeyDetails, token, krValueUpdate.GoalKeyId, krValueUpdate.CurrentValue, krValueUpdate.Year).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }

                            previousContributor = goalKeyDetails.EmployeeId;


                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                GoalObjective objectiveDetails = new GoalObjective();
                                objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                                var objectiveKeyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive);

                                objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                goalObjectiveRepo.Update(objectiveDetails);
                                UnitOfWorkAsync.SaveChanges();
                            }

                        }
                        while (importedId != 0);

                    }
                }

                else
                {
                    ////var keyDetails =goalKey;
                    ////var keyDetails =  goalKeyRepo.GetQueryable().AsNoTracking().FirstOrDefault(x => x.GoalKeyId == krValueUpdate.GoalKeyId);

                    var variance = krValueUpdate.CurrentValue - keyDetails.CurrentValue;
                    var currentValue = (krValueUpdate.CurrentValue - keyDetails.StartValue) + keyDetails.ContributorValue;
                    keyDetails.CurrentValue = krValueUpdate.CurrentValue;
                    keyDetails.UpdatedBy = userIdentity.EmployeeId;
                    keyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                    var score = (currentValue / (keyDetails.TargetValue - keyDetails.StartValue)) * 100;
                    if (score < 0)
                    {
                        keyDetails.Score = 0;
                    }
                    else
                    {
                        keyDetails.Score = Math.Round(score) > 100 ? 100 : Math.Round(score);
                    }
                    var progress = commonService.GetProgressIdWithFormula(keyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), keyDetails.Score, cycleDurationDetails.CycleDurationId);
                    keyDetails.Progress = progress;

                    UpdateGoalKeyAndMaintainHistory(keyDetails, userIdentity);
                    long? previousContributor = 0;

                    if (keyDetails.ImportedId > 0)
                    {
                        long importedId = keyDetails.ImportedId;
                        do
                        {
                            mailLoop += 1;
                            GoalKey goalKeyDetails = new GoalKey();
                            goalKeyDetails = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == importedId);
                            ////var obj = goalObjectiveRepo.GetQueryable()
                            ////    .FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                            ////var goalKeyDetails = await GetGoalKeyDetail(importedId);
                            if (goalKeyDetails == null)
                                break;
                            var contributorValue = goalKeyDetails.ContributorValue + variance;
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

                            var sourceProgress = commonService.GetProgressIdWithFormula(goalKeyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), goalKeyDetails.Score, cycleDurationDetails.CycleDurationId);
                            goalKeyDetails.Progress = sourceProgress;
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

                            UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);

                            importedId = goalKeyDetails.ImportedId;

                            if (isScoreUpdate && mailLoop == 1)
                            {
                                await Task.Run(async () =>
                                {
                                    await notificationService.UpdateProgress(previousContributor == 0 ? userIdentity.EmployeeId : previousContributor, goalKeyDetails, token, krValueUpdate.GoalKeyId, krValueUpdate.CurrentValue, krValueUpdate.Year).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }

                            previousContributor = goalKeyDetails.EmployeeId;

                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                GoalObjective objectiveDetails = new GoalObjective();
                                objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId && x.IsActive);
                                var objectiveKeyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);

                                objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                goalObjectiveRepo.Update(objectiveDetails);
                                UnitOfWorkAsync.SaveChanges();
                            }

                        }
                        while (importedId != 0);
                    }

                }

                if (keyDetails.GoalObjectiveId != 0)
                {
                    var calculatedGoalKey = new List<GoalKey>();
                    GoalObjective objectiveDetails = new GoalObjective();
                    objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == keyDetails.GoalObjectiveId);
                    var objectiveKeyDetails = goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                    foreach (var item in objectiveKeyDetails)
                    {
                        if (item.Score > 100)
                        {
                            item.Score = 100;
                        }
                        else if (item.Score < 0)
                        {
                            item.Score = 0;
                        }
                        calculatedGoalKey.Add(item);

                    }
                    objectiveDetails.Score = calculatedGoalKey.Select(x => x.Score).Average();

                    goalObjectiveRepo.Update(objectiveDetails);
                    UnitOfWorkAsync.SaveChanges();
                }
            }
            krCalculationResponse = Mapper.Map<KrCalculationResponse>(keyDetails);
            krCalculationResponse.isAchieved = isAchieved;
            return krCalculationResponse;
        }

        private void MaintainGoalKeyHistory(GoalKey goalKey, UserIdentity userIdentity)
        {
            var goalKeyHistory = new GoalKeyHistory
            {
                GoalKeyId = goalKey.GoalKeyId,
                CurrentValue = goalKey.CurrentValue,
                ContributorValue = goalKey.ContributorValue,
                Score = goalKey.Score,
                CreatedBy = userIdentity.EmployeeId,
                CreatedOn = DateTime.UtcNow,
                Progress = goalKey.Progress
            };
            goalKeyHistoryRepo.Add(goalKeyHistory);
            UnitOfWorkAsync.SaveChanges();
        }

        public void UpdateGoalKeyAndMaintainHistory(GoalKey goalKey, UserIdentity userIdentity)
        {
            goalKeyRepo.Update(goalKey);
            UnitOfWorkAsync.SaveChanges();
            MaintainGoalKeyHistory(goalKey, userIdentity);
        }

        public async Task<List<KrCalculationAlignmentMapResponse>> UpdateKrValueAlignmentMap(KrValueUpdate krValueUpdate, UserIdentity userIdentity, string token, GoalKey goalKeyRecord = null, bool isScoreUpdate = false)
        {
            KrCalculationAlignmentMapResponse krCalculationResponse;
            List<KrCalculationAlignmentMapResponse> krCalculationAlignmentMapResponses = new List<KrCalculationAlignmentMapResponse>();
            var isAchieved = false;

            var keyDetails = new GoalKey();
            if (goalKeyRecord == null)
            {
                keyDetails = goalKeyRepo.GetQueryable().AsNoTracking()
                    .FirstOrDefault(x => x.GoalKeyId == krValueUpdate.GoalKeyId && x.IsActive);
            }
            else
            {
                keyDetails = goalKeyRecord;
            }

            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(userIdentity.OrganisationId, token);
            CycleDetails cycleDetail =
                cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == krValueUpdate.Year);
            var quarterDetails =
                cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == keyDetails.CycleId);


            if (quarterDetails != null)
            {
                if (keyDetails.MetricId == (int)MetricType.NoUnits)
                {
                    var score = new decimal();
                    long? previousContributor = 0;
                    keyDetails.CurrentValue = krValueUpdate.CurrentValue;
                    keyDetails.UpdatedBy = userIdentity.EmployeeId;
                    keyDetails.UpdatedOn =
                        Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

                    var allContributors = goalKeyRepo.GetQueryable().AsNoTracking().Where(x =>
                        x.ImportedId == keyDetails.GoalKeyId && x.GoalStatusId == (int)GoalStatus.Public &&
                        x.KrStatusId == (int)KrStatus.Accepted && x.IsActive).ToList();

                    keyDetails.Score = allContributors.Count > 0
                        ? Math.Round(
                            (allContributors.Select(x => x.Score).Sum() + keyDetails.CurrentValue) /
                            (allContributors.Count + 1), 2)
                        : krValueUpdate.CurrentValue;
                    var progress = commonService.GetProgressIdWithFormula(keyDetails.DueDate,
                        Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate),
                        keyDetails.Score, cycleDurationDetails.CycleDurationId);
                    keyDetails.Progress = progress;

                    UpdateGoalKeyAndMaintainHistory(keyDetails, userIdentity);

                    krCalculationResponse = Mapper.Map<KrCalculationAlignmentMapResponse>(keyDetails);

                    ///In and Out Value

                    var inValueOfLoggedInUser = allContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                    foreach (var item in inValueOfLoggedInUser)
                    {
                        score = score + item.CurrentValue;
                    }
                    krCalculationResponse.InValue = Math.Abs(score);

                    krCalculationResponse.OutValue = Math.Abs(krValueUpdate.CurrentValue);
                    if (keyDetails.GoalObjectiveId != 0)
                    {
                        GoalObjective objectiveDetails = new GoalObjective();
                        objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking()
                            .FirstOrDefault(x => x.GoalObjectiveId == keyDetails.GoalObjectiveId);
                        var objectiveKeyDetails = goalKeyRepo.GetQueryable()
                            .Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive);

                        objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();
                        krCalculationResponse.OkrScore = commonService.KeyScore(objectiveDetails.Score);
                        krCalculationResponse.OkrId = keyDetails.GoalObjectiveId;

                        goalObjectiveRepo.Update(objectiveDetails);
                        UnitOfWorkAsync.SaveChanges();
                    }
                    krCalculationAlignmentMapResponses.Add(krCalculationResponse);


                    if (keyDetails.ImportedId > 0)
                    {
                        var importedId = keyDetails.ImportedId;
                        var empId = userIdentity.EmployeeId;
                        var count = 0;
                        do
                        {
                            var scoreInLoop = new decimal();
                            GoalKey goalKeyDetails = new GoalKey();
                            goalKeyDetails = goalKeyRepo.GetQueryable().AsTracking()
                                .FirstOrDefault(x => x.GoalKeyId == importedId);

                            if (goalKeyDetails == null)
                                break;

                            var upperLevelContributors = goalKeyRepo.GetQueryable().AsNoTracking()
                                .Where(x => x.ImportedId == goalKeyDetails.GoalKeyId && x.IsActive).ToList();

                            goalKeyDetails.Score =
                                Math.Round(
                                    (upperLevelContributors.Select(x => x.Score).Sum() + goalKeyDetails.CurrentValue) /
                                    (upperLevelContributors.Count + 1), 2);
                            var sourceProgress = commonService.GetProgressIdWithFormula(keyDetails.DueDate,
                                Convert.ToDateTime(quarterDetails.StartDate),
                                Convert.ToDateTime(quarterDetails.EndDate), keyDetails.Score,
                                cycleDurationDetails.CycleDurationId);
                            goalKeyDetails.Progress = sourceProgress;
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn =
                                Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

                            UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);


                            if (isScoreUpdate)
                            {
                                await Task.Run(async () =>
                                {
                                    await notificationService
                                        .UpdateProgress(
                                            previousContributor == 0
                                                ? userIdentity.EmployeeId
                                                : previousContributor, goalKeyDetails, token, krValueUpdate.GoalKeyId, krValueUpdate.CurrentValue, krValueUpdate.Year).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }

                            previousContributor = goalKeyDetails.EmployeeId;

                            krCalculationResponse = Mapper.Map<KrCalculationAlignmentMapResponse>(goalKeyDetails);

                            if (count == 0)
                            {
                                var inValueOfNextLevelUser =
                                    upperLevelContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                                foreach (var item in inValueOfNextLevelUser)
                                {
                                    scoreInLoop = scoreInLoop + item.CurrentValue;
                                }
                                krCalculationResponse.InValue = Math.Abs(scoreInLoop);

                            }
                            else
                            {
                                var inValueOfNextLevelUser =
                                    upperLevelContributors.Where(x => x.EmployeeId == empId).ToList();
                                foreach (var item in inValueOfNextLevelUser)
                                {
                                    scoreInLoop = scoreInLoop + item.CurrentValue;
                                }
                                krCalculationResponse.InValue = Math.Abs(scoreInLoop);
                            }

                            krCalculationResponse.OutValue = Math.Abs(goalKeyDetails.CurrentValue);

                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                GoalObjective objectiveDetails = new GoalObjective();
                                objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking()
                                    .FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                                var objectiveKeyDetails = goalKeyRepo.GetQueryable()
                                    .Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive);

                                objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                krCalculationResponse.OkrScore = commonService.KeyScore(objectiveDetails.Score);
                                krCalculationResponse.OkrId = goalKeyDetails.GoalObjectiveId;

                                goalObjectiveRepo.Update(objectiveDetails);
                                UnitOfWorkAsync.SaveChanges();
                            }

                            krCalculationAlignmentMapResponses.Add(krCalculationResponse);

                            importedId = goalKeyDetails.ImportedId;
                            empId = (long)goalKeyDetails.EmployeeId;
                            count = count + 1;

                        } while (importedId != 0);
                    }

                }


                else if (keyDetails.MetricId == (int)Metrics.Boolean)
                {
                    var contribution = commonService.GetKRContributorAsync(2, keyDetails.GoalKeyId).Result;
                    keyDetails.CurrentValue = krValueUpdate.CurrentValue;
                    isAchieved = contribution.Any(x => x.Score == 100);

                    if (keyDetails.CurrentValue > 0 || keyDetails.ContributorValue > 0 || (contribution.Count > 0 && contribution.Any(x => x.Score == 100)))
                    {

                        keyDetails.Score = 100;
                    }
                    else
                    {
                        keyDetails.Score = 0;
                    }
                    keyDetails.UpdatedBy = userIdentity.EmployeeId;
                    keyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                    var progress = commonService.GetProgressIdWithFormula(keyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), keyDetails.Score, cycleDurationDetails.CycleDurationId);
                    keyDetails.Progress = progress;

                    UpdateGoalKeyAndMaintainHistory(keyDetails, userIdentity);
                    long? previousContributor = 0;

                    krCalculationResponse = Mapper.Map<KrCalculationAlignmentMapResponse>(keyDetails);

                    ///In and Out Value

                    if (contribution.Any(x => x.CurrentValue == 100 && contribution.Count > 0))
                        krCalculationResponse.InValue = 100;
                    else
                    {
                        krCalculationResponse.InValue = Constants.Zero;
                    }

                    krCalculationResponse.OutValue = krValueUpdate.CurrentValue;

                    if (keyDetails.GoalObjectiveId != 0)
                    {
                        GoalObjective objectiveDetails = new GoalObjective();
                        objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking()
                            .FirstOrDefault(x => x.GoalObjectiveId == keyDetails.GoalObjectiveId);
                        var objectiveKeyDetails = goalKeyRepo.GetQueryable()
                            .Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive);

                        objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();
                        krCalculationResponse.OkrScore = commonService.KeyScore(objectiveDetails.Score);
                        krCalculationResponse.OkrId = keyDetails.GoalObjectiveId;

                        goalObjectiveRepo.Update(objectiveDetails);
                        UnitOfWorkAsync.SaveChanges();
                    }

                    krCalculationAlignmentMapResponses.Add(krCalculationResponse);

                    if (keyDetails.ImportedId > 0)
                    {
                        var count = 0;
                        var importedId = keyDetails.ImportedId;
                        do
                        {
                            GoalKey goalKeyDetails = new GoalKey();
                            goalKeyDetails = goalKeyRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalKeyId == importedId);

                            if (goalKeyDetails == null)
                                break;

                            var allContributors = commonService.GetKRContributorAsync(2, goalKeyDetails.GoalKeyId).Result;

                            goalKeyDetails.ContributorValue = krValueUpdate.CurrentValue;
                            if (goalKeyDetails.CurrentValue > 0 || (goalKeyDetails.ContributorValue > 0 && allContributors.Count > 0 && allContributors.Any(x => x.Score == 100)))
                            {
                                goalKeyDetails.Score = 100;
                            }
                            else
                            {
                                goalKeyDetails.Score = 0;
                            }
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                            var sourceProgress = commonService.GetProgressIdWithFormula(goalKeyDetails.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), goalKeyDetails.Score, cycleDurationDetails.CycleDurationId);
                            goalKeyDetails.Progress = sourceProgress;

                            UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);

                            importedId = goalKeyDetails.ImportedId;


                            if (isScoreUpdate)
                            {
                                await Task.Run(async () =>
                                {
                                    await notificationService.UpdateProgress(previousContributor == 0 ? userIdentity.EmployeeId : previousContributor, goalKeyDetails, token, krValueUpdate.GoalKeyId, krValueUpdate.CurrentValue, krValueUpdate.Year).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }

                            previousContributor = goalKeyDetails.EmployeeId;
                            krCalculationResponse = Mapper.Map<KrCalculationAlignmentMapResponse>(goalKeyDetails);

                            if (count == 0)
                            {
                                var inValueOfNextLevelUser =
                                    allContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                                if (inValueOfNextLevelUser.Any(x =>
                                    x.CurrentValue == 100 && allContributors.Count > 0 &&
                                    inValueOfNextLevelUser != null))
                                    krCalculationResponse.InValue = 100;
                                else
                                    krCalculationResponse.InValue = 0;

                            }
                            else
                            {

                                var inValueOfNextLevelUser =
                                    allContributors.Where(x => x.EmployeeId == previousContributor).ToList();

                                if (inValueOfNextLevelUser.Any(x =>
                                    x.CurrentValue == 100 && allContributors.Count > 0 &&
                                    inValueOfNextLevelUser != null))
                                    krCalculationResponse.InValue = 100;
                                else
                                    krCalculationResponse.InValue = 0;

                            }

                            krCalculationResponse.OutValue = goalKeyDetails.CurrentValue;

                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                GoalObjective objectiveDetails = new GoalObjective();
                                objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                                var objectiveKeyDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive);

                                objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                goalObjectiveRepo.Update(objectiveDetails);
                                krCalculationResponse.OkrId = goalKeyDetails.GoalObjectiveId;
                                krCalculationResponse.OkrScore = commonService.KeyScore(objectiveDetails.Score);
                                UnitOfWorkAsync.SaveChanges();
                            }
                            krCalculationAlignmentMapResponses.Add(krCalculationResponse);
                            count = count + 1;
                        }
                        while (importedId != 0);

                    }
                }

                else
                {
                    ////var keyDetails =goalKey;
                    ////var keyDetails =  goalKeyRepo.GetQueryable().AsNoTracking().FirstOrDefault(x => x.GoalKeyId == krValueUpdate.GoalKeyId);

                    var variance = krValueUpdate.CurrentValue - keyDetails.CurrentValue;
                    var currentValue = (krValueUpdate.CurrentValue - keyDetails.StartValue) +
                                       keyDetails.ContributorValue;
                    keyDetails.CurrentValue = krValueUpdate.CurrentValue;
                    keyDetails.UpdatedBy = userIdentity.EmployeeId;
                    keyDetails.UpdatedOn =
                        Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                    var score = (currentValue / (keyDetails.TargetValue - keyDetails.StartValue)) * 100;
                    if (score < 0)
                    {
                        keyDetails.Score = 0;
                    }
                    else
                    {
                        keyDetails.Score = Math.Round(score) > 100 ? 100 : Math.Round(score);
                    }

                    var progress = commonService.GetProgressIdWithFormula(keyDetails.DueDate,
                        Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate),
                        keyDetails.Score, cycleDurationDetails.CycleDurationId);
                    keyDetails.Progress = progress;



                    UpdateGoalKeyAndMaintainHistory(keyDetails, userIdentity);

                    krCalculationResponse = Mapper.Map<KrCalculationAlignmentMapResponse>(keyDetails);

                    ///In and Out Value

                    if (keyDetails.MetricId == (int)Metrics.Currency && keyDetails.CurrencyId == (int)CurrencyValues.Dollar)
                    {
                        krCalculationResponse.CurrencyInValue = Constants.DollarSymbol + Math.Round(Math.Abs(keyDetails.ContributorValue));
                        krCalculationResponse.CurrencyOutValue = Constants.DollarSymbol + Math.Abs(krValueUpdate.CurrentValue);
                    }
                    else if (keyDetails.MetricId == (int)Metrics.Currency && keyDetails.CurrencyId == (int)CurrencyValues.Euro)
                    {
                        krCalculationResponse.CurrencyInValue = Constants.EuroSymbol + Math.Round(Math.Abs(keyDetails.ContributorValue));
                        krCalculationResponse.CurrencyOutValue = Constants.EuroSymbol + Math.Abs(krValueUpdate.CurrentValue);
                    }
                    else if (keyDetails.MetricId == (int)Metrics.Currency && keyDetails.CurrencyId == (int)CurrencyValues.Pound)
                    {
                        krCalculationResponse.CurrencyInValue = Constants.PoundSymbol + Math.Round(Math.Abs(keyDetails.ContributorValue));
                        krCalculationResponse.CurrencyOutValue = Constants.PoundSymbol + Math.Abs(krValueUpdate.CurrentValue);
                    }
                    else if (keyDetails.MetricId == (int)Metrics.Currency && keyDetails.CurrencyId == (int)CurrencyValues.Rupee)
                    {
                        krCalculationResponse.CurrencyInValue = Constants.RupeeSymbol + Math.Round(Math.Abs(keyDetails.ContributorValue));
                        krCalculationResponse.CurrencyOutValue = Constants.RupeeSymbol + Math.Abs(krValueUpdate.CurrentValue);
                    }
                    else if (keyDetails.MetricId == (int)Metrics.Currency && keyDetails.CurrencyId == (int)CurrencyValues.Yen)
                    {
                        krCalculationResponse.CurrencyInValue = Constants.YenSymbol + Math.Round(Math.Abs(keyDetails.ContributorValue));
                        krCalculationResponse.CurrencyOutValue = Constants.YenSymbol + Math.Abs(krValueUpdate.CurrentValue);
                    }
                    else
                    {
                        krCalculationResponse.InValue = Math.Round(Math.Abs(keyDetails.ContributorValue));
                        krCalculationResponse.OutValue = Math.Abs(krValueUpdate.CurrentValue);

                    }

                    if (keyDetails.GoalObjectiveId != 0)
                    {
                        GoalObjective objectiveDetails = new GoalObjective();
                        objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking()
                            .FirstOrDefault(x => x.GoalObjectiveId == keyDetails.GoalObjectiveId);
                        var objectiveKeyDetails = goalKeyRepo.GetQueryable()
                            .Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive);

                        objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                        krCalculationResponse.OkrScore = commonService.KeyScore(objectiveDetails.Score);
                        krCalculationResponse.OkrId = keyDetails.GoalObjectiveId;

                        goalObjectiveRepo.Update(objectiveDetails);
                        UnitOfWorkAsync.SaveChanges();
                    }

                    krCalculationAlignmentMapResponses.Add(krCalculationResponse);

                    long? previousContributor = 0;

                    if (keyDetails.ImportedId > 0)
                    {
                        var count = 0;
                        long importedId = keyDetails.ImportedId;
                        do
                        {
                            GoalKey goalKeyDetails = new GoalKey();
                            goalKeyDetails = goalKeyRepo.GetQueryable().AsNoTracking()
                                .FirstOrDefault(x => x.GoalKeyId == importedId && x.IsActive);
                            ////var obj = goalObjectiveRepo.GetQueryable()
                            ////    .FirstOrDefault(x => x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId);
                            ////var goalKeyDetails = await GetGoalKeyDetail(importedId);
                            if (goalKeyDetails == null)
                                break;
                            var contributorValue = goalKeyDetails.ContributorValue + variance;
                            goalKeyDetails.ContributorValue = contributorValue;

                            var upperLevelContributors = goalKeyRepo.GetQueryable().AsNoTracking()
                                .Where(x => x.ImportedId == goalKeyDetails.GoalKeyId && x.IsActive).ToList();

                            var sourceScore =
                                (contributorValue + (goalKeyDetails.CurrentValue - goalKeyDetails.StartValue)) /
                                Math.Round(goalKeyDetails.TargetValue - goalKeyDetails.StartValue) * 100;
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

                            var sourceProgress = commonService.GetProgressIdWithFormula(goalKeyDetails.DueDate,
                                Convert.ToDateTime(quarterDetails.StartDate),
                                Convert.ToDateTime(quarterDetails.EndDate), goalKeyDetails.Score,
                                cycleDurationDetails.CycleDurationId);
                            goalKeyDetails.Progress = sourceProgress;
                            goalKeyDetails.UpdatedBy = userIdentity.EmployeeId;
                            goalKeyDetails.UpdatedOn =
                                Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

                            UpdateGoalKeyAndMaintainHistory(goalKeyDetails, userIdentity);

                            importedId = goalKeyDetails.ImportedId;

                            if (isScoreUpdate)
                            {
                                await Task.Run(async () =>
                                {
                                    await notificationService
                                        .UpdateProgress(
                                            previousContributor == 0
                                                ? userIdentity.EmployeeId
                                                : previousContributor, goalKeyDetails, token, krValueUpdate.GoalKeyId, krValueUpdate.CurrentValue, krValueUpdate.Year).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }


                            krCalculationResponse = Mapper.Map<KrCalculationAlignmentMapResponse>(goalKeyDetails);

                            if (goalKeyDetails.MetricId == (int)Metrics.Currency && goalKeyDetails.CurrencyId == (int)CurrencyValues.Dollar)
                            {
                                var scoreInLoop = new decimal();
                                if (count == 0)
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.DollarSymbol + Math.Round(Math.Abs(scoreInLoop));

                                }
                                else
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == previousContributor).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.DollarSymbol + Math.Round(Math.Abs(scoreInLoop));
                                }
                                krCalculationResponse.CurrencyOutValue = Constants.DollarSymbol + Math.Round(Math.Abs(goalKeyDetails.CurrentValue));


                            }
                            else if (goalKeyDetails.MetricId == (int)Metrics.Currency && goalKeyDetails.CurrencyId == (int)CurrencyValues.Euro)
                            {
                                var scoreInLoop = new decimal();
                                if (count == 0)
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.EuroSymbol + Math.Round(Math.Abs(scoreInLoop));

                                }
                                else
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == previousContributor).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.EuroSymbol + Math.Round(Math.Abs(scoreInLoop));
                                }
                                krCalculationResponse.CurrencyOutValue = Constants.EuroSymbol + Math.Round(Math.Abs(goalKeyDetails.CurrentValue));


                            }
                            else if (goalKeyDetails.MetricId == (int)Metrics.Currency && goalKeyDetails.CurrencyId == (int)CurrencyValues.Pound)
                            {
                                var scoreInLoop = new decimal();
                                if (count == 0)
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.PoundSymbol + Math.Round(Math.Abs(scoreInLoop));

                                }
                                else
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == previousContributor).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.PoundSymbol + Math.Round(Math.Abs(scoreInLoop));
                                }
                                krCalculationResponse.CurrencyOutValue = Constants.PoundSymbol + Math.Round(Math.Abs(goalKeyDetails.CurrentValue));

                            }
                            else if (goalKeyDetails.MetricId == (int)Metrics.Currency && goalKeyDetails.CurrencyId == (int)CurrencyValues.Rupee)
                            {
                                var scoreInLoop = new decimal();
                                if (count == 0)
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.RupeeSymbol + Math.Round(Math.Abs(scoreInLoop));

                                }
                                else
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == previousContributor).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.RupeeSymbol + Math.Round(Math.Abs(scoreInLoop));
                                }
                                krCalculationResponse.CurrencyOutValue = Constants.RupeeSymbol + Math.Round(Math.Abs(goalKeyDetails.CurrentValue));

                            }
                            else if (goalKeyDetails.MetricId == (int)Metrics.Currency && goalKeyDetails.CurrencyId == (int)CurrencyValues.Yen)
                            {
                                var scoreInLoop = new decimal();
                                if (count == 0)
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.YenSymbol + Math.Round(Math.Abs(scoreInLoop));

                                }
                                else
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == previousContributor).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.CurrencyInValue = Constants.YenSymbol + Math.Round(Math.Abs(scoreInLoop));
                                }
                                krCalculationResponse.CurrencyOutValue = Constants.YenSymbol + Math.Round(Math.Abs(goalKeyDetails.CurrentValue));
                            }
                            else
                            {
                                var scoreInLoop = new decimal();
                                if (count == 0)
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == userIdentity.EmployeeId).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.InValue = Math.Abs(scoreInLoop);

                                }
                                else
                                {
                                    var inValueOfNextLevelUser =
                                        upperLevelContributors.Where(x => x.EmployeeId == previousContributor).ToList();
                                    foreach (var item in inValueOfNextLevelUser)
                                    {
                                        scoreInLoop = scoreInLoop + item.CurrentValue;
                                    }
                                    krCalculationResponse.InValue = Math.Abs(scoreInLoop);
                                }
                                krCalculationResponse.OutValue = Math.Abs(goalKeyDetails.CurrentValue);
                            }

                            previousContributor = goalKeyDetails.EmployeeId;

                            if (goalKeyDetails.GoalObjectiveId != 0)
                            {
                                GoalObjective objectiveDetails = new GoalObjective();
                                objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking().FirstOrDefault(x =>
                                    x.GoalObjectiveId == goalKeyDetails.GoalObjectiveId && x.IsActive);
                                var objectiveKeyDetails = goalKeyRepo.GetQueryable().Where(x =>
                                    x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);


                                objectiveDetails.Score = objectiveKeyDetails.Select(x => x.Score).Average();

                                krCalculationResponse.OkrScore = objectiveDetails.Score;
                                krCalculationResponse.OkrScore = commonService.KeyScore(objectiveDetails.Score);
                                krCalculationResponse.OkrId = goalKeyDetails.GoalObjectiveId;

                                goalObjectiveRepo.Update(objectiveDetails);
                                UnitOfWorkAsync.SaveChanges();
                            }

                            krCalculationAlignmentMapResponses.Add(krCalculationResponse);

                        } while (importedId != 0);
                    }

                }

                if (keyDetails.GoalObjectiveId != 0)
                {
                    var calculatedGoalKey = new List<GoalKey>();
                    GoalObjective objectiveDetails = new GoalObjective();
                    objectiveDetails = goalObjectiveRepo.GetQueryable().AsTracking()
                        .FirstOrDefault(x => x.GoalObjectiveId == keyDetails.GoalObjectiveId);
                    var objectiveKeyDetails = goalKeyRepo.GetQueryable().AsNoTracking()
                        .Where(x => x.GoalObjectiveId == objectiveDetails.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                    foreach (var item in objectiveKeyDetails)
                    {
                        if (item.Score > 100)
                        {
                            item.Score = 100;
                        }
                        else if (item.Score < 0)
                        {
                            item.Score = 0;
                        }

                        calculatedGoalKey.Add(item);

                    }

                    objectiveDetails.Score = calculatedGoalKey.Select(x => x.Score).Average();

                    goalObjectiveRepo.Update(objectiveDetails);
                    UnitOfWorkAsync.SaveChanges();
                }
            }

            return krCalculationAlignmentMapResponses;
        }
    }
}

