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
    public class PeopleService : BaseService, IPeopleService
    {
        private readonly IRepositoryAsync<GoalObjective> goalObjectiveRepo;
        private readonly IRepositoryAsync<GoalKey> goalKeyRepo;
        private readonly ICommonService commonService;

        public PeopleService(IServicesAggregator servicesAggregateService, ICommonService commonServices) : base(servicesAggregateService)
        {
            goalObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<GoalObjective>();
            goalKeyRepo = UnitOfWorkAsync.RepositoryAsync<GoalKey>();
            commonService = commonServices;
        }

        public async Task<PeopleResponse> EmployeeView(long empId, int cycle, int year, string token, UserIdentity identity)
        {
            var peopleResponse = new PeopleResponse();
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            var allTeamEmployees = await commonService.GetTeamEmployees();
            if (quarterDetails != null)
            {
                var allEmployee = commonService.GetAllUserFromUsers(token);
                var allFeedback = await commonService.GetAllFeedback(token, empId);
                var employeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == empId);
                var lockDate = await commonService.IsOkrLocked(Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), empId, cycle, year, token);

                peopleResponse.IsLocked = lockDate.IsGaolLocked;
                peopleResponse.IsScoreLocked = lockDate.IsScoreLocked;
                peopleResponse.Team = "";
                peopleResponse.Department = employeeDetails != null ? employeeDetails.OrganisationName : "";
                peopleResponse.Designation = employeeDetails != null ? employeeDetails.Designation : "";

                var keyCount = 0;

                var peopleOkrResponses = new List<PeopleOkrResponse>();

                var objectives = await GetEmployeeOkrByCycleId(empId, cycle, year);
                if (objectives.Count != 0)
                {
                    foreach (var obj in objectives)
                    {
                        var keyDetail = await GetGoalKey(obj.GoalObjectiveId);

                        var peopleKeyResponses = SeparationKeyDetails_EmployeeView(keyDetail, allEmployee.Results, allFeedback, ref keyCount, quarterDetails, cycleDurationDetails.CycleDurationId, token, identity);

                        var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);
                        if (peopleKeyResponses.Count > 0)
                        {
                            var teamDetails = new TeamDetails();
                            if (obj.TeamId > 0)
                            {
                                teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == obj.TeamId);
                            }
                            var objProgressId = commonService.GetProgressIdWithFormula(obj.EndDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), obj.Score, cycleDurationDetails.CycleDurationId);

                            peopleOkrResponses.Add(new PeopleOkrResponse
                            {
                                MyGoalsDetails = peopleKeyResponses,
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
                                Score = commonService.KeyScore(obj.Score),
                                DueDate = obj.EndDate,
                                Progress = objProgressId,
                                Source = obj.Source,
                                IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == obj.GoalObjectiveId),
                                Contributors = new List<ContributorsResponse>(),
                                GoalStatusId = obj.GoalStatusId,
                                GoalTypeId = obj.GoalTypeId,
                                AssignmentTypeId = (int)AssignmentType.WithParentObjective,
                                KrStatusId = (int)KrStatus.Accepted,
                                GoalProgressTime = peopleKeyResponses.Count <= 0 ? obj.CreatedOn : peopleKeyResponses.OrderByDescending(x => x.KeyProgressTime).FirstOrDefault().KeyProgressTime,
                                TeamId = obj.TeamId,
                                TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                                ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                                BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,
                                ParentTeamDetail = obj.ImportedId > 0 ? commonService.ParentTeamDetails((int)GoalType.GoalObjective, obj.ImportedId, allTeamEmployees, obj.TeamId) : null
                            });
                        }
                    }

                    peopleResponse.OkrCount = peopleOkrResponses.Count;
                    peopleResponse.AvgScore = peopleOkrResponses.Count > 0 ? commonService.KeyScore(peopleOkrResponses.Select(x => x.Score).Average()) : 0;
                    peopleResponse.MyGoalOkrResponses = peopleOkrResponses;

                    peopleResponse.NotStarted = peopleOkrResponses.Count(x => x.Progress == (int)ProgressMaster.NotStarted);
                    peopleResponse.AtRisk = peopleOkrResponses.Count(x => x.Progress == (int)ProgressMaster.AtRisk);
                    peopleResponse.Lagging = peopleOkrResponses.Count(x => x.Progress == (int)ProgressMaster.Lagging);
                    peopleResponse.OnTrack = peopleOkrResponses.Count(x => x.Progress == (int)ProgressMaster.OnTrack);
                }

                var orphanKrDetails = await GetOrphanKey(empId, cycle);
                var peopleOrphanKeyResponse = new List<PeopleOkrResponse>();
                foreach (var orphanKey in orphanKrDetails)
                {
                    var sourceKeyDetails = await GetGoalKeyById(orphanKey.ImportedId);
                    var sourceUserDetails = sourceKeyDetails != null ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId) : null;
                    var keyScoreUpdateDetail = commonService.LatestUpdateGoalKey(orphanKey.GoalKeyId);
                    if (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.KrStatusId == (int)KrStatus.Accepted && orphanKey.ImportedId >= 0)
                    {
                        var teamDetails = new TeamDetails();
                        if (orphanKey.TeamId > 0)
                        {
                            teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == orphanKey.TeamId);
                        }
                        var keyProgressId = commonService.GetProgressIdWithFormula(orphanKey.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), orphanKey.Score, cycleDurationDetails.CycleDurationId);
                        keyCount += 1;

                        peopleOrphanKeyResponse.Add(new PeopleOkrResponse
                        {
                            GoalKeyId = orphanKey.GoalKeyId,
                            DueDate = orphanKey.DueDate,
                            Progress = keyProgressId,
                            Score = commonService.KeyScore(orphanKey.Score),
                            Source = orphanKey.Source,
                            ImportedType = orphanKey.ImportedType,
                            ImportedId = orphanKey.ImportedId,
                            KeyDescription = orphanKey.KeyDescription,
                            IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == orphanKey.GoalKeyId),
                            Contributors = await commonService.GetContributorAsync((int)GoalType.GoalKey, orphanKey.GoalKeyId, allEmployee.Results, orphanKey.TargetValue),
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
                            KeyProgressTime = keyScoreUpdateDetail == null ? orphanKey.CreatedOn : keyScoreUpdateDetail.UpdatedOn,
                            DueCycle = quarterDetails.Symbol + "-" + year,
                            TeamId = orphanKey.TeamId,
                            TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                            ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                            BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,
                            IsContributor = commonService.GetGoalKeySource(identity.EmployeeId, orphanKey.Source == 0 ? orphanKey.GoalKeyId : orphanKey.Source).Result.IsAligned
                        });
                    }
                }

                if (peopleOrphanKeyResponse.Count != 0)
                {
                    if (peopleResponse.MyGoalOkrResponses != null)
                    {
                        //Comment this code due to Score mismatch from Dashboard.
                        //var parentOkr = peopleOkrResponses.Count > 0 ? commonService.KeyScore(peopleOkrResponses.Select(x => x.Score).Sum()) : 0;
                        var parentOkr = peopleOkrResponses.Count > 0 ? peopleOkrResponses.Select(x => x.Score).Sum() : 0;
                        var orphanKr = peopleOrphanKeyResponse.Count > 0 ? commonService.KeyScore(peopleOrphanKeyResponse.Select(x => x.Score).Sum()) : 0;
                        var totalGoalCount = peopleOkrResponses.Count + peopleOrphanKeyResponse.Count;
                        peopleResponse.MyGoalOkrResponses.AddRange(peopleOrphanKeyResponse);
                        peopleResponse.AvgScore = totalGoalCount > 0 ? commonService.KeyScore((parentOkr + orphanKr) / totalGoalCount) : 0;
                        peopleResponse.NotStarted += peopleOrphanKeyResponse.Count(x => x.Progress == (int)ProgressMaster.NotStarted);
                        peopleResponse.AtRisk += peopleOrphanKeyResponse.Count(x => x.Progress == (int)ProgressMaster.AtRisk);
                        peopleResponse.Lagging += peopleOrphanKeyResponse.Count(x => x.Progress == (int)ProgressMaster.Lagging);
                        peopleResponse.OnTrack += peopleOrphanKeyResponse.Count(x => x.Progress == (int)ProgressMaster.OnTrack);
                    }
                    else
                    {
                        peopleResponse.MyGoalOkrResponses = peopleOrphanKeyResponse;
                        peopleResponse.AvgScore = peopleOrphanKeyResponse.Count > 0 ? commonService.KeyScore(peopleOrphanKeyResponse.Select(x => x.Score).Average()) : 0;
                        peopleResponse.NotStarted = peopleOrphanKeyResponse.Count(x => x.Progress == (int)ProgressMaster.NotStarted);
                        peopleResponse.AtRisk = peopleOrphanKeyResponse.Count(x => x.Progress == (int)ProgressMaster.AtRisk);
                        peopleResponse.Lagging = peopleOrphanKeyResponse.Count(x => x.Progress == (int)ProgressMaster.Lagging);
                        peopleResponse.OnTrack = peopleOrphanKeyResponse.Count(x => x.Progress == (int)ProgressMaster.OnTrack);
                    }

                    peopleResponse.MyGoalOkrResponses = peopleResponse.MyGoalOkrResponses.OrderByDescending(x => x.GoalObjectiveId).ThenBy(x => x.KrStatusId).ToList();
                }

                peopleResponse.KeyCount = keyCount;
            }

            return peopleResponse;
        }


        public async Task<List<GoalObjective>> GetEmployeeOkrByCycleId(long empId, int cycleId, int year)
        {
            return await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive && x.Year == year && !x.IsPrivate && x.GoalStatusId == (int)GoalStatus.Public).OrderByDescending(x => x.GoalObjectiveId).ToListAsync();
        }

        public async Task<List<GoalKey>> GetGoalKey(long goalObjectiveId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToListAsync();
        }

        public async Task<int> EmployeeOkrCount(long empId, int cycleId)
        {
            return await goalObjectiveRepo.GetQueryable().CountAsync(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive);
        }

        public async Task<List<GoalKey>> GetOrphanKey(long employeeId, int cycleId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == 0 && x.CycleId == cycleId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.EmployeeId == employeeId).ToListAsync();
        }

        public async Task<GoalKey> GetGoalKeyById(long goalKeyId)
        {
            return await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalKeyId == goalKeyId && x.IsActive);
        }

        public async Task<List<PeopleViewResponse>> AllPeopleViewResponse(PeopleViewRequest peopleViewRequest, List<string> searchTexts)
        {
            var loginUserId = peopleViewRequest.EmployeeId;
            var loginUserUniqueId = Guid.NewGuid().ToString();
            List<PeopleViewResponse> finalResponse = new List<PeopleViewResponse>();

            peopleViewRequest.OrganisationCycleDetails = commonService.GetOrganisationCycleDurationId(peopleViewRequest.OrgId, peopleViewRequest.Token);
            peopleViewRequest.CycleDetail = commonService.GetOrganisationCycleDetail(peopleViewRequest.OrgId, peopleViewRequest.Token).FirstOrDefault(x => Convert.ToInt32(x.Year) == peopleViewRequest.Year);
            peopleViewRequest.AllEmployee = commonService.GetAllUserFromUsers(peopleViewRequest.Token);


            var quarterDetails = peopleViewRequest.CycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == peopleViewRequest.CycleId);
            if (quarterDetails != null)
            {
                peopleViewRequest.EmployeeParentId = loginUserUniqueId;
                await PeopleViewSource(peopleViewRequest);
                peopleViewRequest.EmployeeId = loginUserId;
                peopleViewRequest.EmployeeUniqueId = loginUserUniqueId;
                peopleViewRequest.IsNested = false;
                peopleViewRequest.ActionLevel = 0;
                peopleViewRequest.ParentObjList = new List<ParentObjectiveResponse>();
                peopleViewRequest.SourceParentObjList = new List<long>();
                peopleViewRequest.PeopleViewObjectives = null;
                peopleViewRequest.NameList = new List<string>();
                PeopleViewLoginUser(peopleViewRequest);
                peopleViewRequest.EmployeeParentId = loginUserUniqueId;
                await PeopleViewContributor(peopleViewRequest);

                if (searchTexts.Count > 0 && searchTexts != null)
                {
                    foreach (var search in searchTexts)
                    {
                        if (search.ToLower() == Constants.AtRisk)
                        {
                            finalResponse.AddRange(
                                peopleViewRequest.PeopleViewResponse.Where(x =>
                                    x.Progress == (int)ProgressMaster.AtRisk));
                        }
                        else if (search.ToLower() == Constants.Lagging)
                        {
                            finalResponse.AddRange(
                                peopleViewRequest.PeopleViewResponse.Where(x =>
                                    x.Progress == (int)ProgressMaster.Lagging));
                        }
                        else if (search.ToLower() == Constants.OnTrack)
                        {
                            finalResponse.AddRange(
                                peopleViewRequest.PeopleViewResponse.Where(x =>
                                    x.Progress == (int)ProgressMaster.OnTrack));
                        }
                        else if (search.ToLower() == Constants.NotStarted)
                        {
                            finalResponse.AddRange(peopleViewRequest.PeopleViewResponse.Where(x =>
                                x.Progress == (int)ProgressMaster.NotStarted));
                        }
                    }

                    finalResponse.AddRange(
                        peopleViewRequest.PeopleViewResponse.Where(x => x.EmployeeId == peopleViewRequest.EmployeeId));
                }
                else
                {
                    finalResponse = peopleViewRequest.PeopleViewResponse;
                }
            }

            return finalResponse;
        }

        public async Task<List<PeopleViewResponse>> PeopleViewSource(PeopleViewRequest peopleViewRequest)
        {
            try
            {
                if (!peopleViewRequest.IsNested)
                {
                    peopleViewRequest.ActionLevel--;
                }

                var cycleDetails = peopleViewRequest.CycleDetail.QuarterDetails;
                var currentQuarter = cycleDetails.FirstOrDefault(x => x.OrganisationCycleId == peopleViewRequest.CycleId);
                var cycleDurationDetails = peopleViewRequest.OrganisationCycleDetails;

                var sourceKeyResultIds = peopleViewRequest.IsNested ? peopleViewRequest.SourceParentObjList : goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == peopleViewRequest.EmployeeId && x.IsActive && x.CycleId == peopleViewRequest.CycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && x.ImportedId > 0).Select(x => x.ImportedId).ToList();
                var sourceKeyResults = goalKeyRepo.GetQueryable().Where(x => sourceKeyResultIds.Contains(x.GoalKeyId)).ToList();

                var sourceObjectivesKeyResultIds = peopleViewRequest.IsNested ? peopleViewRequest.SourceParentObjList : goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == peopleViewRequest.EmployeeId && x.IsActive && x.CycleId == peopleViewRequest.CycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && x.GoalObjectiveId > 0 && x.ImportedId > 0).Select(x => x.ImportedId).ToList();


                var sourceKeyResultsWithObjectives = goalKeyRepo.GetQueryable().Where(x => sourceObjectivesKeyResultIds.Contains(x.GoalKeyId)).ToList();

                var distinctSource = sourceKeyResults.Select(x => x.EmployeeId).Distinct().ToList();

                foreach (var itemEmpId in distinctSource.ToList())
                {
                    if (peopleViewRequest.PeopleViewResponse.Any(x => x.EmployeeId == itemEmpId && x.ActionLevel == peopleViewRequest.ActionLevel)) continue;
                    var employeeDetail = peopleViewRequest.AllEmployee.Results.FirstOrDefault(x => x.EmployeeId == itemEmpId);
                    if (employeeDetail == null) continue;

                    var sourceParentKeyResultIds = sourceKeyResults.Where(x => x.EmployeeId == employeeDetail.EmployeeId && x.ImportedId > 0).Select(x => x.ImportedId).ToList();
                    var sourceParentKeyResults = goalKeyRepo.GetQueryable().Where(x => sourceParentKeyResultIds.Contains(x.GoalKeyId)).ToList();
                    var distinctParentSource = sourceParentKeyResults.Select(x => x.EmployeeId).Distinct().ToList();

                    var teams = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == itemEmpId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && sourceParentKeyResultIds.Contains(x.ImportedId)).Select(x => x.TeamId).ToList();
                    var teamId = teams.FirstOrDefault(x => x > 0);

                    var objCount = sourceKeyResults.Where(x => x.EmployeeId == itemEmpId && x.GoalObjectiveId > 0).Select(x=>x.GoalObjectiveId).Distinct().ToList().Count;
                    
                    var krCount = sourceKeyResults.FindAll(x => x.EmployeeId == itemEmpId).Count;
                    var avgScore = sourceKeyResults.Where(x => x.EmployeeId == itemEmpId).Average(x => x.Score);
                    var peopleViewObjectives = await GetPeopleViewObjectives(sourceKeyResults.FindAll(x => x.EmployeeId == itemEmpId), peopleViewRequest);

                    var peopleViewDto = Mapper.Map<UserResponse, PeopleViewResponse>(employeeDetail);


                    if (currentQuarter != null)
                        peopleViewDto.Progress = commonService.GetProgressIdWithFormula(Convert.ToDateTime(currentQuarter.EndDate), Convert.ToDateTime(currentQuarter?.StartDate), Convert.ToDateTime(currentQuarter?.EndDate), avgScore, cycleDurationDetails.CycleDurationId);


                    peopleViewRequest.KrCount = krCount;
                    peopleViewRequest.ObjCount = objCount;
                    peopleViewRequest.PeopleViewContributors = new List<PeopleViewContributors>();
                    peopleViewRequest.PeopleViewObjectives = peopleViewObjectives;
                    peopleViewRequest.UserResponse = employeeDetail;
                    peopleViewRequest.AvgScore = avgScore;
                    peopleViewRequest.IsParent = true;
                    peopleViewRequest.EmployeeId = peopleViewRequest.EmployeeId;
                    peopleViewRequest.EmployeeUniqueId = Guid.NewGuid().ToString();
                    peopleViewRequest.IsSourceExist = distinctParentSource.Count > 0;
                    peopleViewRequest.IsContributorExist = false;
                    peopleViewRequest.NameList.Insert(0,employeeDetail.FirstName);
                    peopleViewDto.Parent.Add(peopleViewRequest.EmployeeParentId);
                    PeopleViewCommonResponse(peopleViewDto, peopleViewRequest, teamId, peopleViewRequest.Token);
                    peopleViewRequest.PeopleViewResponse.Add(peopleViewDto);
                    if (peopleViewRequest.NameList.Count > 0)
                        peopleViewRequest.NameList.RemoveAt(0);
                }
                peopleViewRequest.NameList.Clear();
                var nextLevelSourceList = peopleViewRequest.PeopleViewResponse.Where(x => x.ActionLevel == peopleViewRequest.ActionLevel).ToList();
                foreach (var nextLevelSource in nextLevelSourceList)
                {
                    if (nextLevelSource.IsSourceExist)
                    {
                        peopleViewRequest.ActionLevel = nextLevelSource.ActionLevel - 1;
                        peopleViewRequest.IsNested = true;
                        peopleViewRequest.EmployeeId = nextLevelSource.EmployeeId;
                        peopleViewRequest.EmployeeParentId = nextLevelSource.EmployeeUniqueId;
                        peopleViewRequest.NameList.AddRange(nextLevelSource.NameList);
                        peopleViewRequest.SourceParentObjList = sourceKeyResults.Where(x => x.ImportedId > 0 && x.EmployeeId == nextLevelSource.EmployeeId).Select(y => y.ImportedId).ToList();
                        peopleViewRequest.PeopleViewResponse = peopleViewRequest.PeopleViewResponse;
                        await PeopleViewSource(peopleViewRequest);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return peopleViewRequest.PeopleViewResponse;
        }
        public List<PeopleViewResponse> PeopleViewLoginUser(PeopleViewRequest peopleViewRequest)
        {
            try
            {
                var selfKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == peopleViewRequest.EmployeeId && x.IsActive && x.CycleId == peopleViewRequest.CycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
                var contributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).ToList();
                var distinctContributor = contributorsKeyResults.Select(x => x.EmployeeId).Distinct().ToList();

                var employeeDetail = peopleViewRequest.AllEmployee.Results.FirstOrDefault(x => x.EmployeeId == peopleViewRequest.EmployeeId);
                if (employeeDetail != null)
                {
                    var peopleViewDto = Mapper.Map<UserResponse, PeopleViewResponse>(employeeDetail);
                    peopleViewRequest.KrCount = 0;
                    peopleViewRequest.ObjCount = 0;
                    peopleViewRequest.PeopleViewContributors = new List<PeopleViewContributors>();
                    peopleViewRequest.PeopleViewObjectives = new List<PeopleViewObjectives>();
                    peopleViewRequest.UserResponse = employeeDetail;
                    peopleViewRequest.AvgScore = 0;
                    peopleViewRequest.IsParent = true;
                    peopleViewRequest.IsSourceExist = peopleViewRequest.PeopleViewResponse.Count > 0;
                    peopleViewRequest.IsContributorExist = distinctContributor.Count > 0;
                    PeopleViewCommonResponse(peopleViewDto, peopleViewRequest, Constants.Zero,peopleViewRequest.Token);
                    peopleViewRequest.PeopleViewResponse.Add(peopleViewDto);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return peopleViewRequest.PeopleViewResponse;
        }
        public async Task<List<PeopleViewResponse>> PeopleViewContributor(PeopleViewRequest peopleViewRequest)
        {
            try
            {
                List<GoalKey> contributorsKeyResults = new List<GoalKey>();
                List<long> selfKeyResultIds = new List<long>();
                if (!peopleViewRequest.IsNested)
                {
                    peopleViewRequest.ActionLevel++;
                }

                if (peopleViewRequest.ParentObjList.Count == 0)
                {
                    selfKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == peopleViewRequest.EmployeeId && x.IsActive && x.CycleId == peopleViewRequest.CycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
                    contributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).ToList();

                }
                else if (peopleViewRequest.IsNested)
                {
                    foreach (var item in peopleViewRequest.ParentObjList)
                    {
                        if (item.ObjectiveType == (int)GoalType.GoalObjective)
                        {

                            var selfkeys = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && item.ObjectiveId == x.GoalObjectiveId).Select(x => x.GoalKeyId).ToList();
                            selfKeyResultIds.AddRange(selfkeys);
                        }
                        else
                        {
                            var selfKeys = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && item.ObjectiveId == x.GoalKeyId).Select(x => x.GoalKeyId).ToList();
                            selfKeyResultIds.AddRange(selfKeys);

                        }
                    }
                    contributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).ToList();
                }

                var distinctContributor = contributorsKeyResults.Select(x => x.EmployeeId).Distinct().ToList();

                var cycleDetails = peopleViewRequest.CycleDetail.QuarterDetails;
                var currentQuarter = cycleDetails.FirstOrDefault(x => x.OrganisationCycleId == peopleViewRequest.CycleId);
                var cycleDurationDetails = peopleViewRequest.OrganisationCycleDetails;

                foreach (var itemEmpId in distinctContributor.ToList())
                {

                    var actionLevelUser = peopleViewRequest.PeopleViewResponse.FirstOrDefault(x => x.EmployeeId == itemEmpId && x.ActionLevel == peopleViewRequest.ActionLevel);
                    if (actionLevelUser != null)
                    {
                        peopleViewRequest.PeopleViewResponse.FirstOrDefault(x => x.EmployeeUniqueId == peopleViewRequest.EmployeeParentId)?.Parent.Add(actionLevelUser.EmployeeUniqueId);
                    }
                    else
                    {
                        var peopleViewContributors = new List<PeopleViewContributors>();
                        var employeeDetail = peopleViewRequest.AllEmployee.Results.FirstOrDefault(x => x.EmployeeId == itemEmpId);
                        if (employeeDetail != null)
                        {
                            var objCount = contributorsKeyResults.Where(x => x.EmployeeId == itemEmpId && x.GoalObjectiveId > 0).Select(x=>x.GoalObjectiveId).Distinct().ToList().Count;
                           
                            var krCount = contributorsKeyResults.FindAll(x => x.EmployeeId == itemEmpId).Count;
                            var avgScore = contributorsKeyResults.Where(x => x.EmployeeId == itemEmpId).Average(x => x.Score);
                            var peopleViewObjectives = await GetPeopleViewObjectives(contributorsKeyResults.FindAll(x => x.EmployeeId == itemEmpId), peopleViewRequest);

                            var empKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == employeeDetail.EmployeeId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).Select(x => x.GoalKeyId).ToList();
                            var teams = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == employeeDetail.EmployeeId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).Select(x => x.TeamId).ToList();
                            var teamId = teams.FirstOrDefault(x => x > 0);
                            var innerContributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && empKeyResultIds.Contains(x.ImportedId)).ToList();
                            var distinctUserContributor = innerContributorsKeyResults.Select(x => x.EmployeeId).Distinct().ToList();

                            var peopleViewDto = Mapper.Map<UserResponse, PeopleViewResponse>(employeeDetail);

                            foreach (var itemContributorId in distinctUserContributor.ToList())
                            {
                                var empContributor = peopleViewRequest.AllEmployee.Results.FirstOrDefault(x => x.EmployeeId == itemContributorId);
                                var peopleViewContributorsDto = Mapper.Map<UserResponse, PeopleViewContributors>(empContributor);
                                peopleViewContributors.Add(peopleViewContributorsDto);
                            }

                            if (currentQuarter != null)
                                peopleViewDto.Progress = commonService.GetProgressIdWithFormula(Convert.ToDateTime(currentQuarter.EndDate), Convert.ToDateTime(currentQuarter?.StartDate), Convert.ToDateTime(currentQuarter?.EndDate), avgScore, cycleDurationDetails.CycleDurationId);
                            
                            peopleViewRequest.EmployeeUniqueId = Guid.NewGuid().ToString();
                            peopleViewRequest.KrCount = krCount;
                            peopleViewRequest.ObjCount = objCount;
                            peopleViewRequest.PeopleViewContributors = peopleViewContributors;
                            peopleViewRequest.PeopleViewObjectives = peopleViewObjectives;
                            peopleViewRequest.UserResponse = employeeDetail;
                            peopleViewRequest.AvgScore = avgScore;
                            peopleViewRequest.IsParent = false;
                            peopleViewRequest.IsSourceExist = false;
                            peopleViewRequest.IsContributorExist = peopleViewContributors.Count > 0;
                            peopleViewRequest.NameList.Add(employeeDetail.FirstName);
                            PeopleViewCommonResponse(peopleViewDto, peopleViewRequest, teamId,peopleViewRequest.Token);
                            peopleViewRequest.PeopleViewResponse.Add(peopleViewDto);
                            peopleViewRequest.PeopleViewResponse.FirstOrDefault(x => x.EmployeeUniqueId == peopleViewRequest.EmployeeParentId)?.Parent.Add(peopleViewRequest.EmployeeUniqueId);
                            if (peopleViewRequest.NameList.Count > 0)
                                peopleViewRequest.NameList.RemoveAt(peopleViewRequest.NameList.Count - 1);
                        }
                    }
                }
                peopleViewRequest.NameList.Clear();
                var nextLevelContributorList = peopleViewRequest.PeopleViewResponse.Where(x => x.ActionLevel == peopleViewRequest.ActionLevel).ToList();

                foreach (var nextLevelContributor in nextLevelContributorList)
                {
                    if (nextLevelContributor.IsContributorExist)
                    {
                        peopleViewRequest.ActionLevel = nextLevelContributor.ActionLevel + 1;
                        peopleViewRequest.IsNested = true;
                        peopleViewRequest.EmployeeId = nextLevelContributor.EmployeeId;
                        peopleViewRequest.EmployeeParentId = nextLevelContributor.EmployeeUniqueId;
                        peopleViewRequest.NameList.AddRange(nextLevelContributor.NameList);
                        peopleViewRequest.ParentObjList = (from p in nextLevelContributor.PeopleViewObjectives
                                                           select new ParentObjectiveResponse()
                                                           {
                                                               ObjectiveId = p.ObjectiveId,
                                                               ObjectiveType = p.ObjectiveType
                                                           }).ToList();

                        peopleViewRequest.PeopleViewResponse = peopleViewRequest.PeopleViewResponse;
                        await PeopleViewContributor(peopleViewRequest);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return peopleViewRequest.PeopleViewResponse;
        }

        #region Private Methods

        private List<PeopleKeyResponse> SeparationKeyDetails_EmployeeView(List<GoalKey> keyDetail, List<UserResponse> allEmployee, List<FeedbackResponse> allFeedback, ref int keyCount, QuarterDetails quarterDetails, long cycleDurationId, string token, UserIdentity identity)
        {
            var peopleKeyResponses = new List<PeopleKeyResponse>();
            var allTeamEmployees = commonService.GetTeamEmployees().Result;
            foreach (var key in keyDetail)
            {
                var keyProgressId = commonService.GetProgressIdWithFormula(key.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), key.Score, cycleDurationId);
                var keyScoreUpdateDetail = commonService.LatestUpdateGoalKey(key.GoalKeyId);
                keyCount += 1;
                var teamDetails = new TeamDetails();
                if (key.TeamId > 0)
                {
                    teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == key.TeamId);
                }

                peopleKeyResponses.Add(new PeopleKeyResponse
                {
                    GoalKeyId = key.GoalKeyId,
                    DueDate = key.DueDate,
                    Score = commonService.KeyScore(key.Score),
                    Source = key.Source,
                    ImportedType = key.ImportedType,
                    ImportedId = key.ImportedId,
                    KeyDescription = key.KeyDescription,
                    Progress = keyProgressId,
                    IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId),
                    Contributors = commonService.GetContributor((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, key.TargetValue),
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
                    KeyProgressTime = keyScoreUpdateDetail == null ? key.CreatedOn : keyScoreUpdateDetail.UpdatedOn,
                    TeamId = key.TeamId,
                    TeamName = teamDetails == null ? "" : teamDetails.OrganisationName,
                    ColorCode = teamDetails == null ? "" : teamDetails.ColorCode,
                    BackGroundColorCode = teamDetails == null ? "" : teamDetails.BackGroundColorCode,
                    IsContributor = commonService.GetGoalKeySource(identity.EmployeeId, key.Source == 0 ? key.GoalKeyId : key.Source).Result.IsAligned,
                    ParentTeamDetail = key.ImportedId > 0 ? commonService.ParentTeamDetails((int)GoalType.GoalKey, key.ImportedId, allTeamEmployees, key.TeamId) : null
                });
            }

            return peopleKeyResponses;
        }
        private async Task<List<PeopleViewObjectives>> GetPeopleViewObjectives(List<GoalKey> keyResults, PeopleViewRequest peopleViewRequest)
        {
            var objectiveList = new List<PeopleViewObjectives>();
            try
            {
                var cycleDurationDetails = peopleViewRequest.OrganisationCycleDetails;
                var cycleDetail = peopleViewRequest.CycleDetail;
                var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == peopleViewRequest.CycleId);
                var objectives = goalObjectiveRepo.GetQueryable().Where(x => keyResults.Select(y => y.GoalObjectiveId).Contains(x.GoalObjectiveId)).ToList();


                if (objectives.Count != 0)
                {
                    foreach (var obj in objectives)
                    {
                        var peopleViewKeyResults = new List<PeopleViewKeyResults>();
                        var keyDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == obj.GoalObjectiveId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined && keyResults.Select(x => x.GoalKeyId).Contains(x.GoalKeyId)).ToListAsync();
                        foreach (var key in keyDetails)
                        {
                            if (key.GoalObjectiveId > 0 && (key.GoalStatusId == (int)GoalStatus.Draft && key.ImportedId == 0) || (key.GoalStatusId != (int)GoalStatus.Draft && key.ImportedId >= 0))
                            {
                                key.Progress = commonService.GetProgressIdWithFormula(key.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), key.Score, cycleDurationDetails.CycleDurationId);
                                var peopleViewKeyResultDto = Mapper.Map<GoalKey, PeopleViewKeyResults>(key);
                                peopleViewRequest.Cycle = quarterDetails?.Symbol + ", " + peopleViewRequest.Year;
                                peopleViewRequest.TeamId = key.TeamId;
                                PeopleViewKrCommonResponse(peopleViewKeyResultDto, peopleViewRequest);
                                peopleViewKeyResults.Add(peopleViewKeyResultDto);

                            }
                        }

                        if ((keyDetails.Count > 0 && keyDetails.Any(x => x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId != (int)GoalStatus.Draft)) || (obj.GoalStatusId == (int)GoalStatus.Draft && obj.ImportedId == 0))
                        {
                            obj.Progress = commonService.GetProgressIdWithFormula(obj.EndDate, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), obj.Score, cycleDurationDetails.CycleDurationId);
                            var peopleViewResponseDto = Mapper.Map<GoalObjective, PeopleViewObjectives>(obj);
                            peopleViewRequest.PeopleViewObjective = peopleViewResponseDto;
                            peopleViewRequest.PeopleViewKeyResults = peopleViewKeyResults;
                            peopleViewRequest.TeamId = obj.TeamId;
                            peopleViewRequest.GoalType = (int)GoalType.GoalObjective;
                            PeopleViewObjectiveCommonResponse(peopleViewRequest);
                            objectiveList.Add(peopleViewRequest.PeopleViewObjective);
                        }
                    }
                }

                var orphanKrDetails = keyResults.Where(x => x.GoalObjectiveId == 0).ToList();
                foreach (var orphanKey in orphanKrDetails)
                {
                    if ((orphanKey.GoalStatusId != (int)GoalStatus.Draft) || (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
                    {
                        var peopleViewResponseDto = Mapper.Map<GoalKey, PeopleViewObjectives>(orphanKey);
                        peopleViewRequest.PeopleViewObjective = peopleViewResponseDto;
                        peopleViewRequest.PeopleViewKeyResults = new List<PeopleViewKeyResults>();
                        peopleViewRequest.TeamId = peopleViewResponseDto.TeamId;
                        peopleViewRequest.GoalType = (int)GoalType.GoalKey;
                        PeopleViewObjectiveCommonResponse(peopleViewRequest);
                        objectiveList.Add(peopleViewRequest.PeopleViewObjective);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return objectiveList.OrderBy(x => x.ObjectiveType).ToList();
        }
        private void PeopleViewKrCommonResponse(PeopleViewKeyResults peopleViewKeyResult, PeopleViewRequest peopleViewRequest)
        {
            peopleViewKeyResult.TeamName = peopleViewRequest.TeamId > 0 ? commonService.GetTeamEmployeeByTeamId(peopleViewRequest.TeamId, peopleViewRequest.Token).OrganisationName : "";
            peopleViewKeyResult.Cycle = peopleViewRequest.Cycle;
            peopleViewKeyResult.KrLastUpdatedTime = commonService.LatestUpdateGoalKey(peopleViewKeyResult.KrId) == null ? peopleViewKeyResult.CreatedOn : commonService.LatestUpdateGoalKey(peopleViewKeyResult.KrId)?.UpdatedOn;
        }
        private void PeopleViewObjectiveCommonResponse(PeopleViewRequest peopleViewRequest)
        {
            peopleViewRequest.PeopleViewObjective.PeopleViewKeyResults = peopleViewRequest.PeopleViewKeyResults;
            peopleViewRequest.PeopleViewObjective.ObjectiveType = peopleViewRequest.GoalType;
            peopleViewRequest.PeopleViewObjective.TeamName = peopleViewRequest.TeamId > 0 ? commonService.GetTeamEmployeeByTeamId(peopleViewRequest.TeamId, peopleViewRequest.Token).OrganisationName : "";
            peopleViewRequest.PeopleViewObjective.Cycle = peopleViewRequest.Cycle;
            peopleViewRequest.PeopleViewObjective.ObjectiveLastUpdatedTime = peopleViewRequest.PeopleViewKeyResults.Count <= 0 ? peopleViewRequest.PeopleViewObjective.CreatedOn : peopleViewRequest.PeopleViewKeyResults.ToList().OrderByDescending(x => x.KrLastUpdatedTime).FirstOrDefault()?.KrLastUpdatedTime;
        }
        private void PeopleViewCommonResponse(PeopleViewResponse peopleViewResponse, PeopleViewRequest peopleViewRequest, long teamId, string token)
        {
            var teamDetailsById = commonService.GetTeamEmployeeByTeamId(teamId, token);
            var employeeOrganization = commonService.GetOrganizationByEmployeeId(peopleViewResponse.EmployeeId,token);

            peopleViewResponse.IsContributorExist = peopleViewRequest.IsContributorExist;
            peopleViewResponse.IsSourceExist = peopleViewRequest.IsSourceExist;
            peopleViewResponse.KeyResultCount = peopleViewRequest.KrCount;
            peopleViewResponse.ObjectiveCount = peopleViewRequest.ObjCount;
            peopleViewResponse.TeamName = employeeOrganization.OrganisationName;
            peopleViewResponse.Score = Math.Round(peopleViewRequest.AvgScore);
            peopleViewResponse.PeopleViewContributors = peopleViewRequest.PeopleViewContributors;
            peopleViewResponse.PeopleViewObjectives = peopleViewRequest.PeopleViewObjectives;
            peopleViewResponse.EmployeeUniqueId = peopleViewRequest.EmployeeUniqueId;
            peopleViewResponse.ActionLevel = peopleViewRequest.ActionLevel;
            peopleViewResponse.IsParent = peopleViewRequest.IsParent;
            if (teamDetailsById != null)
            {
                peopleViewResponse.ColorCode = teamDetailsById.ColorCode;
                peopleViewResponse.BackGroundColorCode = teamDetailsById.BackGroundColorCode;
            }
            peopleViewResponse.CycleEndDate = peopleViewRequest.CycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == peopleViewRequest.CycleId)?.EndDate;
            peopleViewResponse.NameList.AddRange(peopleViewRequest.NameList);
        }

        #endregion
    }
}
