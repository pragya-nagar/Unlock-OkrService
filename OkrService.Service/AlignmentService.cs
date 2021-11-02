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
using MoreLinq;

namespace OKRService.Service
{
    [ExcludeFromCodeCoverage]
    public class AlignmentService : BaseService, IAlignmentService
    {
        private readonly ICommonService commonService;
        private readonly IRepositoryAsync<GoalObjective> goalObjectiveRepo;
        private readonly IRepositoryAsync<GoalKey> goalKeyRepo;
        private readonly IRepositoryAsync<TeamSequence> teamSequenceRepo;

        public AlignmentService(IServicesAggregator servicesAggregateService, ICommonService commonServices) : base(
            servicesAggregateService)
        {
            commonService = commonServices;
            goalObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<GoalObjective>();
            goalKeyRepo = UnitOfWorkAsync.RepositoryAsync<GoalKey>();
            teamSequenceRepo = UnitOfWorkAsync.RepositoryAsync<TeamSequence>();
        }

        public async Task<List<OkrViewResponse>> OkrViewAllLevelResponseAsync(long employeeId, List<string> searchTexts, int cycleId, int year, bool isTeams, long teamId, string token, UserIdentity userIdentity)
        {
            List<OkrViewResponse> finalResponse = new List<OkrViewResponse>();
            List<OkrViewResponse> finalResult = new List<OkrViewResponse>();
            List<OkrViewResponse> finalResultForThirdUser = new List<OkrViewResponse>();
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(userIdentity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails?.CycleDetails?.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycleId);
            var allFeedback = await commonService.GetAllFeedback(token, employeeId);
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var allTeamEmployees = await commonService.GetTeamEmployees();
            if (quarterDetails != null)
            {
                var okrViewResponse = new List<OkrViewResponse>();
                List<GoalObjective> objectives = null;
                // var collaborators = new List<GoalKey>();
                if (employeeId == userIdentity.EmployeeId)
                {
                    if (isTeams)
                    {
                        if (teamId > 0)
                        {
                            objectives = await goalObjectiveRepo.GetQueryable().Where(x =>
                                    x.ObjectiveCycleId == cycleId && x.EmployeeId == employeeId && x.IsActive &&
                                    x.GoalStatusId == (int)GoalStatus.Public &&
                                    x.TeamId > 0 && x.TeamId == teamId).OrderByDescending(x => x.GoalObjectiveId)
                                .OrderBy(x => x.Sequence).ToListAsync();

                            var collaboratorOkrKeys = goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.ImportedId == 0 && x.IsActive && x.Year == year && x.TeamId == teamId && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalObjectiveId).ToList();       //.Select(x => x.TeamId).ToListAsync();
                            var collaboratorOkr = goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.ImportedId == 0 && x.IsActive && x.Year == year && x.TeamId == teamId && x.GoalStatusId == (int)GoalStatus.Public).ToList();       //.Select(x => x.TeamId).ToListAsync();

                            var collaboratorResponse = goalObjectiveRepo.GetQueryable().Where(x => x.IsActive && collaboratorOkrKeys.Contains(x.ImportedId) && x.EmployeeId == employeeId).Select(x => x.ImportedId).ToList();
                            foreach (var item in collaboratorResponse)
                            {
                                collaboratorOkr.RemoveAll(x => x.GoalObjectiveId == item && x.IsActive);          //collaboratorOkr.Except(collaboratorResponse).ToList();
                            }
                            //Duplicate Teams OKR Fix.Kindly verify the same.
                            foreach (var okr in collaboratorOkr)
                            {
                                if (!(objectives.Any(x => x.GoalObjectiveId == okr.GoalObjectiveId && okr.Owner == employeeId)))
                                {
                                    objectives.Add(okr);
                                }
                            }

                        }

                    }
                    else
                    {
                        objectives = await goalObjectiveRepo.GetQueryable()
                            .Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == employeeId && x.IsActive &&
                                        x.GoalStatusId == (int)GoalStatus.Public)
                            .OrderByDescending(x => x.GoalObjectiveId).OrderBy(x => x.Sequence).ToListAsync();
                    }

                }
                else
                {
                    if (isTeams)
                    {
                        if (teamId > 0)
                        {
                            objectives = await goalObjectiveRepo.GetQueryable().Where(x =>
                                    x.ObjectiveCycleId == cycleId && x.EmployeeId == employeeId && x.IsActive &&
                                    x.GoalStatusId == (int)GoalStatus.Public && x.IsPrivate == false
                                    && x.TeamId > 0 && x.TeamId == teamId)
                                .OrderByDescending(x => x.GoalObjectiveId).OrderBy(x => x.Sequence).ToListAsync();
                            var collaboratorOkrKeys = goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.ImportedId == 0 && x.IsActive && x.Year == year && x.TeamId == teamId && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalObjectiveId).ToList();       //.Select(x => x.TeamId).ToListAsync();
                            var collaboratorOkr = goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.ImportedId == 0 && x.IsActive && x.Year == year && x.TeamId == teamId && x.GoalStatusId == (int)GoalStatus.Public).ToList();       //.Select(x => x.TeamId).ToListAsync();

                            var collaboratorResponse = goalObjectiveRepo.GetQueryable().Where(x => x.IsActive && collaboratorOkrKeys.Contains(x.ImportedId)).Select(x => x.ImportedId).ToList();
                            foreach (var item in collaboratorResponse)
                            {
                                collaboratorOkr.RemoveAll(x => x.GoalObjectiveId == item && x.IsActive);          //collaboratorOkr.Except(collaboratorResponse).ToList();
                            }

                            //Duplicate Teams OKR Fix.Kindly verify the same.
                            foreach (var okr in collaboratorOkr)
                            {
                                if (!(objectives.Any(x => x.GoalObjectiveId == okr.GoalObjectiveId && okr.Owner == employeeId)))
                                {
                                    objectives.Add(okr);

                                }
                            }
                        }

                    }
                    else
                    {
                        objectives = await goalObjectiveRepo.GetQueryable()
                            .Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == employeeId && x.IsActive &&
                                        x.GoalStatusId == (int)GoalStatus.Public && x.IsPrivate == false)
                            .OrderByDescending(x => x.GoalObjectiveId).OrderBy(x => x.Sequence).ToListAsync();
                    }

                }

                if (objectives.Count != 0)
                {
                    foreach (var obj in objectives)
                    {
                        var okrViewKeyResults = new List<OkrViewKeyResults>();
                        var keyResults = new List<GoalKey>();
                        var allSourceList = new List<GoalKey>();

                        var parentIds = new List<string>();

                        var keyDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == obj.GoalObjectiveId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined).ToListAsync();
                        var krUniqueIdDetails = new List<KrUniqueIdDetails>();
                        var KrStatusId = new List<int>();

                        foreach (var key in keyDetails)
                        {
                            var empGoalKey = key.IsActive && key.ImportedId == 0 && key.TeamId == teamId && employeeId != key.Owner;

                            if ((key.GoalStatusId != (int)GoalStatus.Draft && key.ImportedId >= 0))
                            {
                                var noUnitScore = new decimal();
                                KrUniqueIdDetails krUniqueIdDetail = new KrUniqueIdDetails();

                                var contributors = goalKeyRepo.GetQueryable().Where(x =>
                                    x.ImportedId == key.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey &&
                                    x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                                keyResults.AddRange(contributors);
                                key.Progress = commonService.GetProgressIdWithFormula(key.DueDate,
                                    Convert.ToDateTime(quarterDetails.StartDate),
                                    Convert.ToDateTime(quarterDetails.EndDate), key.Score,
                                    cycleDurationDetails.CycleDurationId);

                                KrStatusId.Add(key.KrStatusId);
                                if (key.ImportedId != 0)
                                {
                                    var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x =>
                                        x.GoalKeyId == key.ImportedId && x.IsActive &&
                                        x.KrStatusId == (int)KrStatus.Accepted);

                                    if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                    {
                                        parentIds.Add(parentDetails.GoalObjectiveId.ToString());
                                    }
                                    else
                                    {
                                        parentIds.Add(key.ImportedId.ToString());
                                    }

                                    key.ImportedId = key.ImportedId;
                                }

                                var sourceKeyDetails = GetGoalKeyById(key.ImportedId);
                                if (sourceKeyDetails != null)
                                {
                                    allSourceList.Add(sourceKeyDetails);
                                }

                                var isSourceLink = sourceKeyDetails != null;

                                var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(key);
                                okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();
                                okrViewKeyResultDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(key.GoalKeyId, token);
                                okrViewKeyResultDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId);
                                okrViewKeyResultDto.IsCollaborator = empGoalKey;
                                krUniqueIdDetail.KrId = key.GoalKeyId;
                                krUniqueIdDetail.KrUniqueId = okrViewKeyResultDto.KrUniqueId;
                                krUniqueIdDetails.Add(krUniqueIdDetail);
                                okrViewKeyResultDto.OkrViewKeyContributors = await commonService.GetAllContributorAsync(2, okrViewKeyResultDto.KrId, allEmployee.Results, userIdentity, token);

                                OkrViewKeyCommonResponse(okrViewKeyResultDto, isSourceLink, contributors, key.TeamId, token, allFeedback, sourceKeyDetails, key.MetricId, key, allEmployee, userIdentity, employeeId);

                                okrViewKeyResults.Add(okrViewKeyResultDto);
                            }

                        }

                        if (KrStatusId.All(x => x == (int)KrStatus.Pending))
                        {
                            parentIds = new List<string>();
                        }

                        var contributorList = keyResults.Select(x => x.EmployeeId).Distinct().ToList();
                        var contributorResponse = ContributorsCommonResponse(contributorList, allEmployee, allSourceList, employeeId, employeeId, true);


                        if ((keyDetails.Count > 0 && keyDetails.Any(x => x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId != (int)GoalStatus.Draft)))
                        {
                            obj.Progress = commonService.GetProgressIdWithFormula(obj.EndDate,
                                Convert.ToDateTime(quarterDetails?.StartDate),
                                Convert.ToDateTime(quarterDetails?.EndDate), obj.Score,
                                cycleDurationDetails.CycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(obj);
                            objOkrViewResponseDto.Score = commonService.KeyScore(obj.Score);
                            objOkrViewResponseDto.IsMyOkr = true;
                            var link = goalObjectiveRepo.GetQueryable().Any(x => x.LinkedObjectiveId == objOkrViewResponseDto.ObjectiveId && x.IsActive);
                            objOkrViewResponseDto.IsVirtualLink = !(!link);
                            objOkrViewResponseDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(obj.GoalObjectiveId, token);
                            objOkrViewResponseDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == obj.GoalObjectiveId);
                            if (obj.TeamId > 0)
                            {
                                var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == obj.TeamId);
                                var allEmployeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);

                                objOkrViewResponseDto.OwnerDesignation = obj.Owner != 0
                                    ? allEmployeeDetails?.Designation
                                    : "";
                                objOkrViewResponseDto.OwnerEmailId = obj.Owner != 0
                                    ? allEmployeeDetails?.EmailId
                                    : "";
                                objOkrViewResponseDto.OwnerEmployeeCode = obj.Owner != 0
                                    ? allEmployeeDetails?.EmployeeCode
                                    : "";
                                objOkrViewResponseDto.OwnerImagePath = obj.Owner != 0
                                    ? allEmployeeDetails?.ImagePath
                                    : "";
                                objOkrViewResponseDto.OwnerLastName = obj.Owner != 0
                                    ? allEmployeeDetails?.LastName
                                    : "";
                                objOkrViewResponseDto.OwnerFirstName = obj.Owner != 0
                                    ? allEmployeeDetails?.FirstName
                                    : "";
                            }
                            objOkrViewResponseDto.ObjectiveUniqueId = Guid.NewGuid().ToString();

                            bool isAligned = obj.CreatedBy == employeeId && obj.ImportedId != 0;

                            var contributorsCount = keyResults.Select(x => x.EmployeeId).Distinct().Count();
                            if (obj.ImportedId > 0)
                            {
                                var parentObj = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalObjectiveId == obj.ImportedId && x.IsActive);
                                objOkrViewResponseDto.ParentDueDate = parentObj?.EndDate;
                                objOkrViewResponseDto.ParentStartDate = parentObj?.StartDate;
                            }
                            var parentLink = goalObjectiveRepo.GetQueryable().Any(x => x.GoalObjectiveId == objOkrViewResponseDto.LinkedObjectiveId && x.IsActive);
                            objOkrViewResponseDto.IsParentVirtualIcon = !(!parentLink);
                            if (objOkrViewResponseDto.IsParentVirtualIcon)
                            {
                                var uniqueId = await ParentVirtualLink(objOkrViewResponseDto.LinkedObjectiveId, cycleDurationDetails, quarterDetails, token, userIdentity, allEmployee, employeeId, year, okrViewResponse, allTeamEmployees);
                                parentIds.Add(uniqueId);
                            }
                            OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResults, contributorResponse, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, obj.GoalObjectiveId, parentIds, isAligned, keyResults.Any(), allSourceList.Any(), obj.TeamId, quarterDetails?.Symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount, allFeedback, KrStatusId, allTeamEmployees);
                            okrViewResponse.Add(objOkrViewResponseDto);
                            if (objOkrViewResponseDto.IsVirtualLink)
                            {
                                await ContributorVirtualLink(objOkrViewResponseDto.ObjectiveId, cycleDurationDetails, quarterDetails, token, userIdentity, allEmployee, employeeId, year, okrViewResponse, objOkrViewResponseDto.ObjectiveUniqueId, allTeamEmployees);
                            }
                            if (okrViewResponse != null && searchTexts.Count > 0 && searchTexts.Any())
                            {
                                searchTexts = searchTexts.ConvertAll(d => d.ToLower());
                                foreach (var searchText in searchTexts)
                                {
                                    List<OkrViewResponse> queryData = new List<OkrViewResponse>();
                                    if (okrViewResponse.Count > 0)
                                    {
                                        switch (searchText)
                                        {
                                            case Constants.AtRisk:
                                                queryData.AddRange(searchTexts.Any(x => x.Contains(Constants.Private))
                                                    ? okrViewResponse.Where(p =>
                                                            p.Progress == (int)ProgressMaster.AtRisk && p.IsPrivate)
                                                        .ToList()
                                                    : okrViewResponse.Where(p =>
                                                        p.Progress == (int)ProgressMaster.AtRisk).ToList());
                                                break;

                                            case Constants.Lagging:
                                                queryData.AddRange(searchTexts.Any(x => x.Contains(Constants.Private))
                                                    ? okrViewResponse.Where(p =>
                                                            p.Progress == (int)ProgressMaster.Lagging && p.IsPrivate)
                                                        .ToList()
                                                    : okrViewResponse.Where(p =>
                                                        p.Progress == (int)ProgressMaster.Lagging).ToList());
                                                break;

                                            case Constants.NotStarted:
                                                queryData.AddRange(searchTexts.Any(x => x.Contains(Constants.Private))
                                                    ? okrViewResponse.Where(p =>
                                                            p.Progress == (int)ProgressMaster.NotStarted &&
                                                            p.IsPrivate)
                                                        .ToList()
                                                    : okrViewResponse.Where(p =>
                                                        p.Progress == (int)ProgressMaster.NotStarted).ToList());
                                                break;

                                            case Constants.OnTrack:
                                                queryData.AddRange(searchTexts.Any(x => x.Contains(Constants.Private))
                                                    ? okrViewResponse.Where(p =>
                                                            p.Progress == (int)ProgressMaster.OnTrack && p.IsPrivate)
                                                        .ToList()
                                                    : okrViewResponse.Where(p =>
                                                        p.Progress == (int)ProgressMaster.OnTrack).ToList());
                                                break;

                                            case Constants.Assigned:
                                                queryData.AddRange(okrViewResponse.Where(p => p.ParentId > 0).ToList());
                                                break;

                                            case Constants.StandAlone:
                                                queryData.AddRange(okrViewResponse.Where(p => p.ObjectiveType == 2).ToList());
                                                break;

                                            case Constants.Individual:
                                                queryData.AddRange(searchTexts.Any(x => x.Contains(Constants.Private))
                                                    ? okrViewResponse.Where(p =>
                                                        p.TeamId == Constants.Zero && p.IsPrivate).ToList()
                                                    : okrViewResponse.Where(p => p.TeamId == Constants.Zero).ToList());
                                                break;

                                            case Constants.Aligned:
                                                queryData.AddRange(okrViewResponse.Where(p => p.IsAligned == true).ToList());
                                                break;

                                            case Constants.Private:
                                                if (!(searchTexts.Any(x => x.Contains(Constants.AtRisk)) || searchTexts.Any(x => x.Contains(Constants.Lagging)) ||
                                                    searchTexts.Any(x => x.Contains(Constants.OnTrack)) || searchTexts.Any(x => x.Contains(Constants.Individual)) ||
                                                    searchTexts.Any(x => x.Contains(Constants.NotStarted))))
                                                {
                                                    queryData.AddRange(okrViewResponse.Where(p => p.IsPrivate == true).ToList());
                                                }
                                                break;
                                        }
                                        if (queryData.Count > 0)
                                        {
                                            finalResponse.AddRange(queryData);
                                            okrViewResponse = okrViewResponse.Except(finalResponse).ToList();
                                            if (keyResults.Count != Constants.Zero && KrStatusId.Any(x => x == (int)KrStatus.Accepted) && objOkrViewResponseDto.OkrViewKeyResults.Any(x => x.IsCollaborator != true))
                                            {
                                                await OkrViewContributorsResponse(keyResults.Distinct().ToList(),
                                                     finalResponse, krUniqueIdDetails, allEmployee,
                                                     Convert.ToDateTime(quarterDetails?.StartDate),
                                                     Convert.ToDateTime(quarterDetails?.EndDate),
                                                     cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year,
                                                     userIdentity, token, allFeedback, 1, employeeId, objOkrViewResponseDto.ObjectiveUniqueId);
                                            }

                                            if (allSourceList.Count != Constants.Zero && KrStatusId.Any(x => x == (int)KrStatus.Accepted) && objOkrViewResponseDto.OkrViewKeyResults.Any(x => x.IsCollaborator != true))
                                            {
                                                await OkrViewSourceResponse(allSourceList, finalResponse, krUniqueIdDetails,
                                                     allEmployee, Convert.ToDateTime(quarterDetails?.StartDate),
                                                     Convert.ToDateTime(quarterDetails?.EndDate),
                                                     cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year,
                                                     userIdentity, token, allFeedback, -1, employeeId);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (okrViewResponse != null)
                            {
                                finalResponse = okrViewResponse;
                                if (keyResults.Count != Constants.Zero && KrStatusId.Any(x => x == (int)KrStatus.Accepted) && objOkrViewResponseDto.OkrViewKeyResults.Any(x => x.IsCollaborator != true))
                                {
                                    await OkrViewContributorsResponse(keyResults, finalResponse, krUniqueIdDetails,
                                        allEmployee, Convert.ToDateTime(quarterDetails?.StartDate),
                                        Convert.ToDateTime(quarterDetails?.EndDate),
                                        cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, userIdentity,
                                        token, allFeedback, 1, employeeId, objOkrViewResponseDto.ObjectiveUniqueId);
                                }

                                if (allSourceList.Count != Constants.Zero && KrStatusId.Any(x => x == (int)KrStatus.Accepted) && objOkrViewResponseDto.OkrViewKeyResults.Any(x => x.IsCollaborator != true))
                                {
                                    await OkrViewSourceResponse(allSourceList, finalResponse, krUniqueIdDetails, allEmployee,
                                         Convert.ToDateTime(quarterDetails?.StartDate),
                                         Convert.ToDateTime(quarterDetails?.EndDate),
                                         cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, userIdentity,
                                         token, allFeedback, -1, employeeId);
                                }
                            }
                        }
                    }
                }

                var orphanKrDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == 0 && x.CycleId == cycleId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined && x.EmployeeId == employeeId && x.GoalStatusId != (int)GoalStatus.Archived).ToListAsync();
                var score = new decimal();
                if (isTeams)
                {
                    orphanKrDetails = await goalKeyRepo.GetQueryable().Where(x =>
                        x.GoalObjectiveId == 0 && x.CycleId == cycleId && x.IsActive &&
                        x.KrStatusId != (int)KrStatus.Declined && x.EmployeeId == employeeId &&
                        x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId > 0).ToListAsync();
                }

                foreach (var orphanKey in orphanKrDetails)
                {
                    var sourceDetails = new List<GoalKey>();
                    var parentIds = new List<string>();
                    var oprhanKrStatusId = new List<int>();


                    if (orphanKey.ImportedId != 0)
                    {
                        var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x =>
                            x.GoalKeyId == orphanKey.ImportedId && x.IsActive &&
                            x.KrStatusId == (int)KrStatus.Accepted);
                        if (parentDetails != null && parentDetails.GoalObjectiveId != 0 && orphanKey.KrStatusId != (int)KrStatus.Pending)
                        {
                            parentIds.Add(parentDetails.GoalObjectiveId.ToString());
                        }
                        else if (orphanKey.ImportedId != 0)
                        {
                            if (orphanKey.KrStatusId != (int)KrStatus.Pending)
                                parentIds.Add(orphanKey.ImportedId.ToString());
                        }
                    }

                    var contributorsList = goalKeyRepo.GetQueryable()
                         .Where(x => x.ImportedId == orphanKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey &&
                                     x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(y => y.EmployeeId)
                         .ToList();

                    var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == orphanKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

                    var contributorsCount = contributors.Select(x => x.EmployeeId).Distinct().Count();

                    if ((orphanKey.GoalStatusId != (int)GoalStatus.Draft) ||
                        (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
                    {
                        orphanKey.Progress = commonService.GetProgressIdWithFormula(orphanKey.DueDate,
                            (DateTime)quarterDetails?.StartDate, (DateTime)quarterDetails?.EndDate, orphanKey.Score,
                            cycleDurationDetails.CycleDurationId);
                        var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(orphanKey);
                        objOkrViewResponseDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(orphanKey.GoalKeyId, token);
                        objOkrViewResponseDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == orphanKey.GoalKeyId);

                        objOkrViewResponseDto.KrUniqueId = Guid.NewGuid().ToString();
                        objOkrViewResponseDto.ImportedId = orphanKey.ImportedId;
                        objOkrViewResponseDto.IsMyOkr = true;
                        objOkrViewResponseDto.Score = commonService.KeyScore(orphanKey.Score);
                        objOkrViewResponseDto.OkrViewStandAloneContributors = await commonService.GetAllContributorAsync(2, objOkrViewResponseDto.ObjectiveId, allEmployee.Results, userIdentity, token);
                        if (orphanKey.ImportedId > 0)
                        {
                            var parentObj = await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.GoalKeyId == orphanKey.ImportedId && x.IsActive);
                            objOkrViewResponseDto.ParentDueDate = parentObj?.DueDate;
                            objOkrViewResponseDto.ParentStartDate = parentObj?.StartDate;
                        }
                        var sourceKeyDetails = GetGoalKeyById(orphanKey.ImportedId);
                        if (sourceKeyDetails != null)
                        {
                            sourceDetails.Add(sourceKeyDetails);

                        }
                        var contributorList = ContributorsCommonResponse(contributorsList, allEmployee, sourceDetails, employeeId, employeeId, true);
                        var isSourceLink = sourceKeyDetails != null;
                        if (sourceKeyDetails != null)
                        {
                            var ownerFirstName = orphanKey.Owner != 0
                                ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId)?.FirstName
                                : "";
                            var ownerLastName = orphanKey.Owner != 0
                                ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId)?.LastName
                                : "";

                            objOkrViewResponseDto.OkrOwner = ownerFirstName + " " + ownerLastName;
                        }


                        if (orphanKey.TeamId > 0)
                        {
                            var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == orphanKey.TeamId);
                            var allEmployeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);


                            objOkrViewResponseDto.OwnerDesignation = orphanKey.Owner != 0
                                ? allEmployeeDetails?.Designation
                                : "";
                            objOkrViewResponseDto.OwnerEmailId = orphanKey.Owner != 0
                                ? allEmployeeDetails?.EmailId
                                : "";
                            objOkrViewResponseDto.OwnerEmployeeCode = orphanKey.Owner != 0
                                ? allEmployeeDetails?.EmployeeCode
                                : "";
                            objOkrViewResponseDto.OwnerImagePath = orphanKey.Owner != 0
                                ? allEmployeeDetails?.ImagePath
                                : "";
                            objOkrViewResponseDto.OwnerLastName = orphanKey.Owner != 0
                                ? allEmployeeDetails?.LastName
                                : "";
                            objOkrViewResponseDto.OwnerFirstName = orphanKey.Owner != 0
                                ? allEmployeeDetails?.FirstName
                                : "";
                        }

                        objOkrViewResponseDto.IsContributor = commonService.GetGoalKeySource(userIdentity.EmployeeId,
                            orphanKey.Source == 0 ? orphanKey.GoalKeyId : orphanKey.Source).Result.IsAligned;

                        if (orphanKey.MetricId == (int)Metrics.Boolean)
                        {
                            if (contributors.Any(x => x.CurrentValue == 100 && contributors.Count > 0))
                                objOkrViewResponseDto.InValue = 100;
                            else
                                objOkrViewResponseDto.InValue = Constants.Zero;

                            objOkrViewResponseDto.OutValue = orphanKey.CurrentValue;

                        }
                        else if (orphanKey.MetricId == (int)Metrics.NoUnits)
                        {
                            foreach (var item in contributors)
                            {
                                score = score + item.CurrentValue;
                            }

                            objOkrViewResponseDto.InValue = Math.Abs(score);

                            objOkrViewResponseDto.OutValue = Math.Abs(orphanKey.CurrentValue - orphanKey.StartValue);

                        }
                        else if (orphanKey.MetricId == (int)Metrics.Currency && orphanKey.CurrencyId == (int)CurrencyValues.Dollar)
                        {
                            objOkrViewResponseDto.CurrencyInValue = Constants.DollarSymbol + Math.Abs(commonService.KeyScore(orphanKey.ContributorValue));
                            objOkrViewResponseDto.CurrencyOutValue = Constants.DollarSymbol + Math.Abs(orphanKey.CurrentValue);
                        }
                        else if (orphanKey.MetricId == (int)Metrics.Currency && orphanKey.CurrencyId == (int)CurrencyValues.Euro)
                        {
                            objOkrViewResponseDto.CurrencyInValue = Constants.EuroSymbol + Math.Abs(commonService.KeyScore(orphanKey.ContributorValue));
                            objOkrViewResponseDto.CurrencyOutValue = Constants.EuroSymbol + Math.Abs(orphanKey.CurrentValue);
                        }
                        else if (orphanKey.MetricId == (int)Metrics.Currency && orphanKey.CurrencyId == (int)CurrencyValues.Pound)
                        {
                            objOkrViewResponseDto.CurrencyInValue = Constants.PoundSymbol + Math.Abs(commonService.KeyScore(orphanKey.ContributorValue));
                            objOkrViewResponseDto.CurrencyOutValue = Constants.PoundSymbol + Math.Abs(orphanKey.CurrentValue);
                        }
                        else if (orphanKey.MetricId == (int)Metrics.Currency && orphanKey.CurrencyId == (int)CurrencyValues.Rupee)
                        {
                            objOkrViewResponseDto.CurrencyInValue = Constants.RupeeSymbol + Math.Abs(commonService.KeyScore(orphanKey.ContributorValue));
                            objOkrViewResponseDto.CurrencyOutValue = Constants.RupeeSymbol + Math.Abs(orphanKey.CurrentValue);
                        }
                        else if (orphanKey.MetricId == (int)Metrics.Currency && orphanKey.CurrencyId == (int)CurrencyValues.Yen)
                        {
                            objOkrViewResponseDto.CurrencyInValue = Constants.YenSymbol + Math.Abs(commonService.KeyScore(orphanKey.ContributorValue));
                            objOkrViewResponseDto.CurrencyOutValue = Constants.YenSymbol + Math.Abs(orphanKey.CurrentValue);
                        }
                        else
                        {
                            objOkrViewResponseDto.InValue = Math.Abs(commonService.KeyScore(orphanKey.ContributorValue));
                            objOkrViewResponseDto.OutValue = Math.Abs((orphanKey.CurrentValue));

                        }


                        objOkrViewResponseDto.ObjectiveUniqueId = Guid.NewGuid().ToString();

                        var krUniqueIdDetails = new List<KrUniqueIdDetails>();
                        KrUniqueIdDetails krUniqueIdDetail = new KrUniqueIdDetails();
                        krUniqueIdDetail.KrId = orphanKey.GoalKeyId;
                        krUniqueIdDetail.KrUniqueId = objOkrViewResponseDto.KrUniqueId;
                        krUniqueIdDetails.Add(krUniqueIdDetail);

                        var isAligned = orphanKey.CreatedBy == employeeId && orphanKey.ImportedId > 0;
                        oprhanKrStatusId.Add(orphanKey.KrStatusId);

                        OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributorList, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, orphanKey.GoalKeyId, parentIds, isAligned, contributors.Any(), orphanKey.ImportedId != 0, orphanKey.TeamId,
                            quarterDetails?.Symbol + "," + " " + year, (int)GoalType.GoalKey, isSourceLink,
                            contributorsCount, allFeedback, oprhanKrStatusId, allTeamEmployees);
                        if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                        {
                            okrViewResponse.Add(objOkrViewResponseDto);
                        }

                        if (okrViewResponse != null && searchTexts != null && searchTexts.Any())
                        {
                            searchTexts = searchTexts.ConvertAll(d => d.ToLower());
                            foreach (var searchText in searchTexts)
                            {
                                List<OkrViewResponse> queryData = new List<OkrViewResponse>();
                                if (okrViewResponse.Count > 0)
                                {
                                    switch (searchText.ToLower())
                                    {
                                        case Constants.AtRisk:
                                            queryData.AddRange(okrViewResponse
                                                .Where(p => p.Progress == (int)ProgressMaster.AtRisk).ToList());
                                            break;

                                        case Constants.Lagging:
                                            queryData.AddRange(okrViewResponse
                                                .Where(p => p.Progress == (int)ProgressMaster.Lagging).ToList());
                                            break;

                                        case Constants.NotStarted:
                                            queryData.AddRange(okrViewResponse
                                                .Where(p => p.Progress == (int)ProgressMaster.NotStarted).ToList());
                                            break;

                                        case Constants.OnTrack:
                                            queryData.AddRange(okrViewResponse
                                                .Where(p => p.Progress == (int)ProgressMaster.OnTrack).ToList());
                                            break;

                                        case Constants.Assigned:
                                            queryData.AddRange(okrViewResponse.Where(p => p.ParentId > 0).ToList());
                                            break;

                                        case Constants.StandAlone:
                                            queryData.AddRange(
                                                okrViewResponse.Where(p => p.ObjectiveType == 2).ToList());
                                            break;

                                        case Constants.Individual:
                                            queryData.AddRange(okrViewResponse.Where(p => p.TeamId == Constants.Zero)
                                                .ToList());
                                            break;

                                        case Constants.Aligned:
                                            queryData.AddRange(okrViewResponse.Where(p => p.IsAligned == true)
                                                .ToList());
                                            break;

                                    }

                                    if (queryData.Count > 0)
                                    {
                                        finalResponse.AddRange(queryData);
                                        okrViewResponse = okrViewResponse.Except(finalResponse).ToList();

                                        if (contributors.Count != 0 && orphanKey.KrStatusId != (int)KrStatus.Pending)
                                        {
                                            await OkrViewContributorsResponse(contributors, finalResponse,
                                                  new List<KrUniqueIdDetails>(), allEmployee,
                                                  Convert.ToDateTime(quarterDetails?.StartDate),
                                                  Convert.ToDateTime(quarterDetails?.EndDate),
                                                  cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year,
                                                  userIdentity, token, allFeedback, 1, employeeId, objOkrViewResponseDto.ObjectiveUniqueId);
                                        }

                                        if (sourceDetails.Count != 0 && orphanKey.KrStatusId != (int)KrStatus.Pending)
                                        {
                                            await OkrViewSourceResponse(sourceDetails, finalResponse,
                                                 new List<KrUniqueIdDetails>(), allEmployee,
                                                 Convert.ToDateTime(quarterDetails?.StartDate),
                                                 Convert.ToDateTime(quarterDetails?.EndDate),
                                                 cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year,
                                                 userIdentity, token, allFeedback, -1, employeeId);
                                        }
                                    }
                                }
                            }
                        }
                        else if (okrViewResponse != null)
                        {
                            finalResponse = okrViewResponse;

                            if (contributors.Count != 0 && orphanKey.KrStatusId != (int)KrStatus.Pending)
                            {
                                await OkrViewContributorsResponse(contributors, finalResponse, krUniqueIdDetails, allEmployee,
                                     Convert.ToDateTime(quarterDetails?.StartDate),
                                     Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId,
                                     quarterDetails?.Symbol, year, userIdentity, token, allFeedback, 1, employeeId, objOkrViewResponseDto.ObjectiveUniqueId);
                            }

                            if (sourceDetails.Count != 0 && orphanKey.KrStatusId != (int)KrStatus.Pending)
                            {
                                await OkrViewSourceResponse(sourceDetails, finalResponse, krUniqueIdDetails, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, userIdentity, token, allFeedback, -1, employeeId);
                            }
                        }
                    }
                }
            }

            if (isTeams)
            {
                finalResult.AddRange(finalResponse);
            }
            else
            {
                var myOkrs = finalResponse.Where(x => x.IsMyOkr == true).ToList();
                var pendingOrphanKrs = myOkrs.Where(x => x.KrStatusId == (int)KrStatus.Pending).ToList();
                var pendingOkrs = myOkrs
                    .Where(x => x.OkrViewKeyResults.Any(y => y.KrStatusId == (int)KrStatus.Pending)).ToList();
                var remainingKrs = finalResponse.Except(pendingOrphanKrs).Except(pendingOkrs).ToList();
                finalResult.AddRange(pendingOrphanKrs);
                finalResult.AddRange(pendingOkrs);
                finalResult.AddRange(remainingKrs);
            }

            finalResultForThirdUser = finalResult.Where(x => (x.KrStatusId != (int)KrStatus.Pending && x.ObjectiveType == (int)GoalType.GoalKey) || x.OkrViewKeyResults.Any(y => y.KrStatusId != (int)KrStatus.Pending)).ToList();

            var okrViewResult = new List<OkrViewResponse>();

            var groupedList = new List<OkrViewResponse>();
            var distinctParentIds = new List<string>();

            if (employeeId != userIdentity.EmployeeId)
                okrViewResult = finalResultForThirdUser;
            else
                okrViewResult = finalResult;

            var finalList = new List<OkrViewResponse>();

            var newList = from myOkr in okrViewResult.ToList()
             .GroupBy(x => x.Parent.FirstOrDefault())
                          select myOkr.ToList();
            finalList = newList.SelectMany(x => x).ToList();

            var parents = finalList.Where(x => x.Parent.Count > 1 && x.IsMyOkr).Select(y => y.Parent);

            foreach (var array in parents)
            {
                foreach (var item in array)
                {

                    if (!distinctParentIds.Contains(item))
                    {
                        distinctParentIds.Add(item);
                        var similarParentIdParent =
                            finalList.Where(x => x.Parent.Any(y => y == item) && x.IsMyOkr).ToList();
                        foreach (var myOkr in similarParentIdParent)
                        {
                            if (groupedList.Any(x => x.ObjectiveId == myOkr.ObjectiveId))
                            {
                                groupedList.RemoveAll(x => x.ObjectiveId == myOkr.ObjectiveId);
                                groupedList.Add(myOkr);
                            }
                            else
                            {
                                groupedList.Add(myOkr);
                            }

                        }
                    }

                    ///   groupedList.AddRange(similarParentIdParent);
                }

            }

            var newGroupedList = groupedList.Select(x => x.ObjectiveId);

            finalList.RemoveAll(x => newGroupedList.Contains(x.ObjectiveId));
            finalList.AddRange(groupedList);


            //var newList = from myOkr in okrViewResult.ToList()
            //  .GroupBy(x => x.Parent.FirstOrDefault())
            //             select myOkr.ToList();

            // var linqList = newList.ToList();

            // finalList = linqList.SelectMany(x => x).ToList();
            return finalList;
        }
        private async Task ContributorVirtualLink(long goalObjectiveId, OrganisationCycleDetails organisationCycle, QuarterDetails quarterDetails, string token, UserIdentity userIdentity, EmployeeResult employeeResult, long employeeId, int year, List<OkrViewResponse> okrViewResponses, string uniqueId, List<TeamDetails> allTeamEmployees)
        {
            var virtualLink = goalObjectiveRepo.GetQueryable().Where(x => x.LinkedObjectiveId == goalObjectiveId && x.IsActive).ToList();
            foreach (var item in virtualLink)
            {
                var okrViewKeyResults = new List<OkrViewKeyResults>();
                var keyResults = new List<GoalKey>();
                var parentIds = new List<string>();
                var keyDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == item.GoalObjectiveId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined).ToListAsync();
                foreach (var result in keyDetails)
                {
                    if ((result.GoalStatusId != (int)GoalStatus.Draft && result.ImportedId >= 0))
                    {
                        var contributors = goalKeyRepo.GetQueryable().Where(x =>
                                   x.ImportedId == result.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey &&
                                   x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                        keyResults.AddRange(contributors);
                        result.Progress = commonService.GetProgressIdWithFormula(result.DueDate,
                            Convert.ToDateTime(quarterDetails.StartDate),
                            Convert.ToDateTime(quarterDetails.EndDate), result.Score,
                            organisationCycle.CycleDurationId);
                        var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(result);
                        okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();
                        okrViewKeyResultDto.IsVirtualLink = true;
                        OkrViewKeyCommonResponse(okrViewKeyResultDto, false, contributors, result.TeamId, token, new List<FeedbackResponse>(), new GoalKey(), result.MetricId, result, employeeResult, userIdentity, employeeId);
                        okrViewKeyResults.Add(okrViewKeyResultDto);
                    }
                }

                var contributorList = keyResults.Select(x => x.EmployeeId).Distinct().ToList();
                var contributorResponse = ContributorsCommonResponse(contributorList, employeeResult, new List<GoalKey>(), item.EmployeeId, employeeId, true);
                var response = contributorResponse.Where(x => x.EmployeeId == item.EmployeeId).ToList();
                if ((keyDetails.Count > 0 && keyDetails.Any(x => x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId != (int)GoalStatus.Draft)))
                {
                    item.Progress = commonService.GetProgressIdWithFormula(item.EndDate,
                        Convert.ToDateTime(quarterDetails?.StartDate),
                        Convert.ToDateTime(quarterDetails?.EndDate), item.Score,
                        organisationCycle.CycleDurationId);
                    var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(item);
                    objOkrViewResponseDto.Score = commonService.KeyScore(item.Score);
                    objOkrViewResponseDto.IsMyOkr = false;
                    objOkrViewResponseDto.ObjectiveUniqueId = Guid.NewGuid().ToString();
                    parentIds.Add(uniqueId);
                    objOkrViewResponseDto.IsVirtualLink = true;
                    OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResults, response, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, item.GoalObjectiveId, parentIds, false, keyResults.Any(), false, item.TeamId, quarterDetails?.Symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, 0, new List<FeedbackResponse>(), new List<int>(), allTeamEmployees);
                    okrViewResponses.Add(objOkrViewResponseDto);
                }
            }
        }

        private async Task<string> ParentVirtualLink(long linkedId, OrganisationCycleDetails organisationCycle, QuarterDetails quarterDetails, string token, UserIdentity userIdentity, EmployeeResult employeeResult, long employeeId, int year, List<OkrViewResponse> okrViewResponses, List<TeamDetails> allTeamEmployees)
        {
            var okrViewKeyResults = new List<OkrViewKeyResults>();
            var keyResults = new List<GoalKey>();
            var parentIds = new List<string>();
            string uniqueId = Guid.NewGuid().ToString();
            var parentVirtualLink = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == linkedId && x.IsActive);
            var keyDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == parentVirtualLink.GoalObjectiveId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined).ToListAsync();
            foreach (var result in keyDetails)
            {
                if ((result.GoalStatusId != (int)GoalStatus.Draft && result.ImportedId >= 0))
                {
                    var contributors = goalKeyRepo.GetQueryable().Where(x =>
                               x.ImportedId == result.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey &&
                               x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                    keyResults.AddRange(contributors);
                    result.Progress = commonService.GetProgressIdWithFormula(result.DueDate,
                        Convert.ToDateTime(quarterDetails.StartDate),
                        Convert.ToDateTime(quarterDetails.EndDate), result.Score,
                        organisationCycle.CycleDurationId);
                    var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(result);
                    okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();
                    okrViewKeyResultDto.IsParentVirtualLink = true;
                    OkrViewKeyCommonResponse(okrViewKeyResultDto, false, contributors, result.TeamId, token, new List<FeedbackResponse>(), new GoalKey(), result.MetricId, result, employeeResult, userIdentity, employeeId);
                    okrViewKeyResults.Add(okrViewKeyResultDto);
                }
            }
            var contributorList = keyResults.Select(x => x.EmployeeId).Distinct().ToList();
            var contributorResponse = ContributorsCommonResponse(contributorList, employeeResult, new List<GoalKey>(), parentVirtualLink.EmployeeId, employeeId, true);
            var response = contributorResponse.Where(x => x.EmployeeId == parentVirtualLink.EmployeeId).ToList();
            if ((keyDetails.Count > 0 && keyDetails.Any(x => x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId != (int)GoalStatus.Draft)))
            {
                parentVirtualLink.Progress = commonService.GetProgressIdWithFormula(parentVirtualLink.EndDate,
                    Convert.ToDateTime(quarterDetails?.StartDate),
                    Convert.ToDateTime(quarterDetails?.EndDate), parentVirtualLink.Score,
                    organisationCycle.CycleDurationId);
                var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(parentVirtualLink);
                objOkrViewResponseDto.Score = commonService.KeyScore(parentVirtualLink.Score);
                objOkrViewResponseDto.IsMyOkr = false;
                objOkrViewResponseDto.ObjectiveUniqueId = uniqueId;
                objOkrViewResponseDto.IsVirtualLink = false;
                objOkrViewResponseDto.IsParentVirtualLink = true;
                OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResults, response, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, parentVirtualLink.GoalObjectiveId, parentIds, false, keyResults.Any(), false, parentVirtualLink.TeamId, quarterDetails?.Symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, 0, new List<FeedbackResponse>(), new List<int>(), allTeamEmployees);
                okrViewResponses.Add(objOkrViewResponseDto);
            }
            return uniqueId;
        }
        public async Task<List<AllTeamOkrViewResponse>> AllTeamOkr(long empId, List<string> searchTexts, int cycle, int year, string token, UserIdentity identity)
        {
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var teamOkrAlignmentList = new List<AllTeamOkrViewResponse>();
            var allFeedback = await commonService.GetAllFeedback(token, empId);
            var allTeamEmployees = await commonService.GetTeamEmployees();
            if (quarterDetails != null)
            {
                var okrTeamIds = new List<long>();
                var okrTeam = new List<long>();
                var headOrganizations = await commonService.GetLeaderOrganizationsAsync(Constants.ObjFeedbackOnTypeId, token);
                if (headOrganizations.Count > 0)
                {
                    var teamIds = headOrganizations.Select(x => x.OrganisationId).Distinct().ToList();
                    if (teamIds.Count > 0)
                    {
                        okrTeam.AddRange(teamIds);
                    }
                }

                var okrTeamId = await goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == empId && x.CycleId == cycle && x.KrStatusId != (int)KrStatus.Declined && x.TeamId > 0 && x.IsActive).Select(x => x.TeamId).Distinct().ToListAsync();
                var collaboratorOkr = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycle && x.ImportedId == 0 && x.IsActive && x.Year == year && x.TeamId == identity.OrganisationId && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.TeamId).ToListAsync();
                if (okrTeamId.Count > 0)
                {
                    okrTeam.AddRange(okrTeamId);
                }
                if (collaboratorOkr.Count > 0)
                {
                    okrTeam.AddRange(collaboratorOkr);
                }

                okrTeamIds = okrTeam.Distinct().ToList();

                foreach (var team in okrTeamIds)
                {
                    var list = new List<GoalKey>();
                    var resultKey = new List<GoalKey>();
                    var parentsId = new List<string>();
                    var teamKrContributors = new List<long?>();
                    var allTeamOKrById = await goalKeyRepo.GetQueryable().Where(x => x.CycleId == cycle && x.TeamId == team && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId != (int)KrStatus.Declined && x.EmployeeId == empId).ToListAsync();
                    var allTeamOKrs = goalKeyRepo.GetQueryable().AsTracking().Where(x => x.CycleId == cycle && x.TeamId == team && x.ImportedId == 0 && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalObjectiveId).Distinct().ToList();
                    var collaborators = new List<GoalKey>();

                    var teamDetailsById = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == team);
                    var getProgress = await commonService.GetLastSevenDaysProgress(Constants.ZeroId, team, cycle, true, identity, Convert.ToInt64(teamDetailsById?.OrganisationHead), false);
                    foreach (var item in allTeamOKrs)
                    {
                        var collaboratorsKeys = new List<GoalKey>();

                        var keyDetails = goalKeyRepo.GetQueryable().AsNoTracking().Where(x => x.GoalObjectiveId == item && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                        foreach (var key in keyDetails)
                        {
                            var krContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == key.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.EmployeeId).ToList();
                            if (krContributors.Count != 0 && !krContributors.Contains(empId))
                            {
                                teamKrContributors.AddRange(krContributors.Cast<long?>().ToList());
                            }
                            collaboratorsKeys.Add(key);
                        }
                        if (teamKrContributors.Count > 0)
                        {
                            collaborators.AddRange(collaboratorsKeys);

                        }

                    }
                    /// await TeamsOkrCollaboratorResponse(collaborators, teamOkrAlignmentList, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, token, allFeedback, empId, team);

                    var goalcount = goalObjectiveRepo.GetQueryable().Count(x => x.ObjectiveCycleId == cycle && x.TeamId == team && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.ImportedId == Constants.Zero);
                    var okrStatusDetail = await commonService.GetAllOkrFiltersAsync(team, token);

                    var teamSequence = await GetTeamSequenceById(empId, cycle);
                    var result = goalKeyRepo.GetQueryable().Where(x => x.CycleId == cycle && x.TeamId == team && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted && x.ImportedId == Constants.Zero).ToList();

                    var goal = goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycle && x.TeamId == team && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public);
                    var ownerOkr = goal.Where(x => x.EmployeeId == x.Owner).ToList();
                    var avgScore = ownerOkr.Count > 0 ? ownerOkr.Select(x => x.Score).Average() : 0;
                    var teamProgressId = commonService.GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), avgScore, cycleDurationDetails.CycleDurationId);
                    var okrStatusCode = okrStatusDetail.OkrStatusDetails?.FirstOrDefault(x => x.Id == teamProgressId);
                    if (allTeamOKrById.Count > 0)
                    {
                        foreach (var key in allTeamOKrById)
                        {

                            var sourceKeyDetails = GetGoalKeyById(key.ImportedId);
                            var allSourceList = new List<GoalKey>();
                            var keyResults = new List<GoalKey>();

                            await TeamsOkrLoggedInUserResponse(key, teamOkrAlignmentList, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, token, allFeedback, empId, team, true, teamProgressId);

                            var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == key.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).ToList();
                            keyResults.AddRange(contributors);
                            if (contributors.Any())
                            {
                                var parent = key.GoalObjectiveId > 0 ? key.GoalObjectiveId : key.GoalKeyId;
                                await TeamsOkrContributorsResponse(keyResults, teamOkrAlignmentList, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, cycle, token, allFeedback, empId, parent, true, teamProgressId);
                                resultKey.AddRange(contributors);
                            }

                            if (sourceKeyDetails != null)
                            {
                                allSourceList.Add(sourceKeyDetails);
                                await TeamsOkrSourceResponse(allSourceList, teamOkrAlignmentList, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, cycle, token, allFeedback, empId, teamProgressId);
                                var parentId = sourceKeyDetails.GoalObjectiveId > 0 ? sourceKeyDetails.GoalObjectiveId : sourceKeyDetails.GoalKeyId;
                                if (!parentsId.Contains(Convert.ToString(parentId)))
                                {
                                    parentsId.Add(Convert.ToString(parentId));
                                }
                                list.Add(sourceKeyDetails);
                            }

                        }

                        var orgOkr = await TeamOkrViewCommonResponse(new AllTeamOkrViewResponse(), new List<OkrViewKeyResults>(), new List<OkrViewContributors>(), new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, team, parentsId, resultKey.Any(), list.Any(), team, quarterDetails?.Symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, resultKey.Select(x => x.EmployeeId).Distinct().Count(), allFeedback, allEmployee, result, goalcount, avgScore, teamSequence, okrStatusCode, teamProgressId, getProgress.Score);
                        teamOkrAlignmentList.Add(orgOkr);
                    }
                    else
                    {
                        var noKeyResult = await TeamOkrViewCommonResponse(new AllTeamOkrViewResponse(), new List<OkrViewKeyResults>(), new List<OkrViewContributors>(), new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, team, new List<string>(), false, false, team, quarterDetails?.Symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, Constants.Zero, allFeedback, allEmployee, result, goalcount, avgScore, teamSequence, okrStatusCode, teamProgressId, Constants.Zero);
                        teamOkrAlignmentList.Add(noKeyResult);
                    }

                }

            }

            List<AllTeamOkrViewResponse> finalResponse = new List<AllTeamOkrViewResponse>();

            if (searchTexts.Count > 0 && searchTexts != null)
            {
                foreach (var search in searchTexts)
                {
                    if (search.ToLower() == Constants.AtRisk)
                    {
                        finalResponse.AddRange(teamOkrAlignmentList.Where(x => x.TeamProgress == (int)ProgressMaster.AtRisk));
                    }
                    else if (search.ToLower() == Constants.Lagging)
                    {
                        finalResponse.AddRange(teamOkrAlignmentList.Where(x => x.TeamProgress == (int)ProgressMaster.Lagging));
                    }
                    else if (search.ToLower() == Constants.OnTrack)
                    {
                        finalResponse.AddRange(teamOkrAlignmentList.Where(x => x.TeamProgress == (int)ProgressMaster.OnTrack));
                    }
                    else if (search.ToLower() == Constants.NotStarted)
                    {
                        finalResponse.AddRange(teamOkrAlignmentList.Where(x => x.TeamProgress == (int)ProgressMaster.NotStarted));
                    }
                }
            }
            else
            {
                finalResponse = teamOkrAlignmentList;
            }
            return finalResponse;
        }

        private async Task TeamsOkrLoggedInUserResponse(GoalKey goalKey, List<AllTeamOkrViewResponse> allTeamOkrViews, EmployeeResult allEmployee, DateTime startDate, DateTime endDate, long cycleDurationId, string symbol, int year, string token, List<FeedbackResponse> allFeedback, long empId, long parentId, bool isTeamParent, int teamProgressId)
        {
            if (goalKey != null)
            {
                var parentIds = new List<string>();
                var source = new List<GoalKey>();

                if (goalKey.GoalObjectiveId == 0)
                {
                    var sourceKeyDetails = GetGoalKeyById(goalKey.ImportedId);

                    if (sourceKeyDetails != null)
                    {
                        source.Add(sourceKeyDetails);
                    }
                    var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == goalKey.TeamId).Select(x => x.EmployeeId).ToList();
                    var contributors = ContributorsCommonResponse(keyContributors, allEmployee, source, (long)goalKey.EmployeeId, empId, false);


                    if ((goalKey.GoalStatusId != (int)GoalStatus.Draft) || (goalKey.GoalStatusId == (int)GoalStatus.Public && goalKey.ImportedId >= 0))
                    {
                        if (isTeamParent)
                        {
                            parentIds.Add(Convert.ToString(parentId));
                        }
                        else
                        {
                            if (goalKey.ImportedId != 0)
                            {
                                var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalKey.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);
                                if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                {
                                    parentIds.Add(parentDetails.GoalObjectiveId.ToString());
                                }
                                else
                                {
                                    parentIds.Add(goalKey.ImportedId.ToString());
                                }
                            }
                        }
                        var contributorsCount = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == goalKey.TeamId).Select(x => x.EmployeeId).Distinct().ToList().Count;
                        goalKey.Progress = commonService.GetProgressIdWithFormula(goalKey.DueDate, startDate, endDate, goalKey.Score, cycleDurationId);
                        var objOkrViewResponseDto = Mapper.Map<GoalKey, AllTeamOkrViewResponse>(goalKey);

                        await TeamOkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributors, new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, goalKey.GoalKeyId, parentIds, contributorsCount != 0, goalKey.ImportedId != 0, goalKey.TeamId, symbol + "," + " " + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount, allFeedback, allEmployee, new List<GoalKey>(), 0, 0, new List<TeamSequence>(), new OkrStatusDetails(), teamProgressId, Constants.Zero);
                        if (allTeamOkrViews.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                        {
                            allTeamOkrViews.Add(objOkrViewResponseDto);
                        }

                    }
                }

                else if (goalKey.GoalObjectiveId > 0)
                {
                    var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalKey.GoalObjectiveId && x.EmployeeId == goalKey.EmployeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == goalKey.TeamId);
                    var keys = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objective.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                    List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();
                    var parent = new List<string>();
                    var sourceDetails = new List<GoalKey>();

                    foreach (var key in keys)
                    {
                        var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(key);
                        okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();
                        goalKey.Progress = commonService.GetProgressIdWithFormula(goalKey.DueDate, startDate, endDate, goalKey.Score, cycleDurationId);
                        okrViewKeyResultResponse.Add(okrViewKeyResultDto);

                        if (isTeamParent)
                        {
                            parent.Add(Convert.ToString(parentId));
                        }
                        else
                        {
                            if (key.ImportedId != 0)
                            {
                                var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == key.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);

                                if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                {
                                    parent.Add(parentDetails.GoalObjectiveId.ToString());
                                }
                                else
                                {
                                    parent.Add(key.ImportedId.ToString());
                                }
                            }
                        }
                        var sourceKeyDetails = GetGoalKeyById(key.ImportedId);

                        if (sourceKeyDetails != null)
                        {
                            sourceDetails.Add(sourceKeyDetails);
                        }
                    }

                    var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == goalKey.TeamId).Select(x => x.EmployeeId).ToList();
                    var contributorDetails = ContributorsCommonResponse(keyContributors, allEmployee, sourceDetails, (long)goalKey.EmployeeId, empId, false);

                    if (objective != null)
                    {
                        var sourceKeyDetails = GetGoalKeyById(objective.ImportedId);
                        var sourceCount = sourceKeyDetails != null ? 1 : 0;

                        var contributorsCount = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == goalKey.TeamId).Select(x => x.EmployeeId).Distinct().ToList().Count;
                        objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, startDate, endDate, objective.Score, cycleDurationId);
                        var objOkrViewResponseDto = Mapper.Map<GoalObjective, AllTeamOkrViewResponse>(objective);

                        await TeamOkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, goalKey.GoalObjectiveId, parent, contributorsCount != 0, sourceCount != 0, goalKey.TeamId, symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount, allFeedback, allEmployee, new List<GoalKey>(), 0, 0, new List<TeamSequence>(), new OkrStatusDetails(), teamProgressId, Constants.Zero);
                        if (allTeamOkrViews.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                        {
                            allTeamOkrViews.Add(objOkrViewResponseDto);
                        }

                    }

                }

            }
        }



        private async Task TeamsOkrCollaboratorResponse(List<GoalKey> goalKeys, List<OkrViewResponse> allTeamOkrViews, EmployeeResult allEmployee, DateTime startDate, DateTime endDate, long cycleDurationId, string symbol, int year, string token, List<FeedbackResponse> allFeedback, long empId, long parentId)
        {
            if (goalKeys.Count != Constants.Zero)
            {
                var objectives = goalKeys.GroupBy(x => x.GoalObjectiveId).ToList();

                foreach (var obj in objectives)
                {
                    var objectiveDetails = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == obj.Key && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived);

                    var parent = new List<string>();
                    var sourceDetails = new List<GoalKey>();
                    var contributorDetails = new List<OkrViewContributors>();
                    List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();

                    var keys = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == obj.Key && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                    foreach (var goalKey in keys)
                    {
                        var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(goalKey);
                        okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();
                        goalKey.Progress = commonService.GetProgressIdWithFormula(goalKey.DueDate, startDate, endDate, goalKey.Score, cycleDurationId);
                        okrViewKeyResultResponse.Add(okrViewKeyResultDto);

                        parent.Add(Convert.ToString(parentId));

                        var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == goalKey.TeamId).Select(x => x.EmployeeId).ToList();
                        contributorDetails = ContributorsCommonResponse(keyContributors, allEmployee, sourceDetails, (long)goalKey.EmployeeId, empId, false);

                    }

                    if (objectiveDetails != null)
                    {
                        var sourceKeyDetails = GetGoalKeyById(objectiveDetails.ImportedId);
                        var sourceCount = sourceKeyDetails != null ? 1 : 0;

                        objectiveDetails.Progress = commonService.GetProgressIdWithFormula(objectiveDetails.EndDate, startDate, endDate, objectiveDetails.Score, cycleDurationId);
                        var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objectiveDetails);
                        objOkrViewResponseDto.ObjectiveUniqueId = objectiveDetails.GoalObjectiveId.ToString(); ;

                        ///  OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, parent, contributorDetails.Count != 0, contributorDetails.Count != 0, sourceCount != 0, objectiveDetails.TeamId, symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorDetails.Count, allFeedback, new List<int>(), allTeamEmployees);

                        /// TeamOkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, Constants.Zero, parent, contributorDetails.Count != 0, sourceCount != 0, objectiveDetails.TeamId, symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorDetails.Count, allFeedback, allEmployee, new List<GoalKey>(), 0, 0, new List<TeamSequence>(), new OkrStatusDetails(), Constants.Zero, Constants.Zero);
                        if (allTeamOkrViews.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                        {
                            allTeamOkrViews.Add(objOkrViewResponseDto);
                        }
                    }
                }

            }
        }

        public decimal KeyScore(decimal score)
        {
            var keyScore = score;
            if (score < 0)
            {
                keyScore = Constants.MinScore;
            }
            else if (score > 100)
            {
                keyScore = Constants.MaxScore;
            }

            return Math.Round(keyScore);
        }

        public async Task<List<TeamSequence>> GetTeamSequenceById(long empId, int cycleId)
        {
            return await teamSequenceRepo.GetQueryable().Where(x => x.CycleId == cycleId && x.EmployeeId == empId && x.IsActive).ToListAsync();
        }

        public GoalKey GetGoalKeyById(long goalKeyId)
        {
            return goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalKeyId && x.IsActive);
        }

        private OkrViewKeyResults OkrViewKeyCommonResponse(OkrViewKeyResults okrViewKeyResults, bool isSourceExist, List<GoalKey> contributors, long teamId, string token, List<FeedbackResponse> allFeedback, GoalKey sourceKey, int metricId, GoalKey key, EmployeeResult allEmployees, UserIdentity userIdentity, long employeeId)
        {
            var score = new decimal();

            if (sourceKey != null)
            {
                var ownerFirstName = key.Owner != 0 ? allEmployees.Results.FirstOrDefault(x => x.EmployeeId == sourceKey.EmployeeId)?.FirstName : "";

                var ownerLastName = key.Owner != 0 ? allEmployees.Results.FirstOrDefault(x => x.EmployeeId == sourceKey.EmployeeId)?.LastName : "";
                okrViewKeyResults.OkrOwner = ownerFirstName + " " + ownerLastName;
            }

            if (key.MetricId == (int)Metrics.Boolean)
            {
                if (contributors.Count > 0)
                    okrViewKeyResults.InValue = contributors.Any(x => x.CurrentValue == 100) ? 100 : Constants.Zero;
                okrViewKeyResults.OutValue = key.MetricId == (int)Metrics.Boolean ? key.CurrentValue : Math.Abs((key.CurrentValue - key.StartValue) + key.ContributorValue);

            }

            else if (metricId == (int)Metrics.NoUnits)
            {
                foreach (var item in contributors)
                {
                    score = score + item.CurrentValue;
                }

                okrViewKeyResults.InValue = Math.Round(Math.Abs(score));

                okrViewKeyResults.OutValue = Math.Abs((key.CurrentValue - key.StartValue));
            }
            else if (metricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Dollar)
            {
                okrViewKeyResults.CurrencyInValue = Constants.DollarSymbol + Math.Round(Math.Abs(key.ContributorValue));
                okrViewKeyResults.CurrencyOutValue = Constants.DollarSymbol + Math.Round(Math.Abs(okrViewKeyResults.KrCurrentValue));
            }
            else if (metricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Euro)
            {
                okrViewKeyResults.CurrencyInValue = Constants.EuroSymbol + Math.Round(Math.Abs(key.ContributorValue));
                okrViewKeyResults.CurrencyOutValue = Constants.EuroSymbol + Math.Round(Math.Abs(okrViewKeyResults.KrCurrentValue));
            }
            else if (metricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Pound)
            {
                okrViewKeyResults.CurrencyInValue = Constants.PoundSymbol + Math.Round(Math.Abs(key.ContributorValue));
                okrViewKeyResults.CurrencyOutValue = Constants.PoundSymbol + Math.Round(Math.Abs(okrViewKeyResults.KrCurrentValue));
            }
            else if (metricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Rupee)
            {
                okrViewKeyResults.CurrencyInValue = Constants.RupeeSymbol + Math.Round(Math.Abs(key.ContributorValue));
                okrViewKeyResults.CurrencyOutValue = Constants.RupeeSymbol + Math.Round(Math.Abs(okrViewKeyResults.KrCurrentValue));
            }
            else if (metricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Yen)
            {
                okrViewKeyResults.CurrencyInValue = Constants.YenSymbol + Math.Round(Math.Abs((key.ContributorValue)));
                okrViewKeyResults.CurrencyOutValue = Constants.YenSymbol + Math.Round(Math.Abs(okrViewKeyResults.KrCurrentValue));
            }
            else
            {
                okrViewKeyResults.InValue = Math.Round(Math.Abs(key.ContributorValue));
                okrViewKeyResults.OutValue = Math.Round(Math.Abs(okrViewKeyResults.KrCurrentValue));

            }

            okrViewKeyResults.IsSourceLinked = isSourceExist;
            if (teamId > 0)
            {
                var teamDetails = commonService.GetTeamEmployeeByTeamId(teamId, token);
                var teamDetailsById = allEmployees.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);
                okrViewKeyResults.OwnerDesignation = key.Owner != 0 ? teamDetailsById?.Designation : "";
                okrViewKeyResults.OwnerEmailId = key.Owner != 0 ? teamDetailsById?.EmailId : "";
                okrViewKeyResults.OwnerEmployeeCode = key.Owner != 0 ? teamDetailsById?.EmployeeCode : "";
                okrViewKeyResults.OwnerImagePath = key.Owner != 0 ? teamDetailsById?.ImagePath : "";
                okrViewKeyResults.OwnerLastName = key.Owner != 0 ? teamDetailsById?.LastName : "";
                okrViewKeyResults.OwnerFirstName = key.Owner != 0 ? teamDetailsById?.FirstName : "";
                okrViewKeyResults.TeamName = !string.IsNullOrEmpty(teamDetails?.OrganisationName) ? teamDetails?.OrganisationName : "";
            }

            okrViewKeyResults.IsContributor = commonService.GetGoalKeySource(userIdentity.EmployeeId, key.Source == 0 ? key.GoalKeyId : key.Source).Result.IsAligned;
            okrViewKeyResults.ContributorCount = contributors.Count();
            return okrViewKeyResults;
        }

        //private void AlignmentRollUp(OkrViewKeyResults okrViewKeyResults,int metricId,List<GoalKey> contributors)
        //{

        //}


        public async Task<LeaderDetailsAlignmentResponse> GetLeaderDetails(long teamId, string token, EmployeeResult employee)
        {
            var teamDetailsById = commonService.GetTeamEmployeeByTeamId(teamId, token);
            var leaderViewDto = new LeaderDetailsAlignmentResponse();
            if (teamDetailsById != null && teamDetailsById.OrganisationId != 0)
            {

                var leaderDetails = employee.Results.FirstOrDefault(x => x.EmployeeId == teamDetailsById?.OrganisationHead);
                if (leaderDetails != null)
                {
                    var searchUserDetails = await commonService.SearchUserAsync(leaderDetails.FirstName, token);
                    var leaderProfileDetails = searchUserDetails.FirstOrDefault(x => x.EmployeeId == leaderDetails.EmployeeId);
                    if (leaderProfileDetails != null)
                    {
                        leaderViewDto = Mapper.Map<LeaderDetailsResponse, LeaderDetailsAlignmentResponse>(leaderProfileDetails);

                    }

                }

            }
            return leaderViewDto;
        }

        private List<OkrViewContributors> ContributorsCommonResponse(IList<long?> contributorsList, EmployeeResult allEmployee, List<GoalKey> sourceIds, long employeeId, long loggedInUserId, bool isLoginUser)
        {
            var okrViewContributors = new List<OkrViewContributors>();

            var index = Constants.Zero;
            long? sourceEmpId = null;
            if (sourceIds.Select(x => x.GoalKeyId).FirstOrDefault() > Constants.Zero)
            {
                sourceEmpId = goalKeyRepo.FindOne(x => x.GoalKeyId == sourceIds.Select(x => x.GoalKeyId).FirstOrDefault())?.EmployeeId;
                contributorsList?.Insert(index, sourceEmpId);
                index++;
            }

            contributorsList?.Insert(index, employeeId);

            if (contributorsList == null) return okrViewContributors;
            var count = 0;
            foreach (var empId in contributorsList)
            {
                var userResponse = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == empId);
                var okrViewContributorsDto = Mapper.Map<UserResponse, OkrViewContributors>(userResponse);
                if (empId == sourceEmpId && count == 0)
                {
                    okrViewContributorsDto.UserType = empId == sourceEmpId ? UserType.Parent.ToString() : " ";
                    count++;
                }
                okrViewContributors.Add(okrViewContributorsDto);
            }


            return okrViewContributors.OrderByDescending(x => x.UserType).DistinctBy(x => x.EmployeeId).ToList();

        }

        private async Task OkrViewContributorsResponse(List<GoalKey> goalKeys, List<OkrViewResponse> okrViewResponse, List<KrUniqueIdDetails> krUniqueIdDetails, EmployeeResult allEmployee, DateTime startDate, DateTime endDate, long cycleDurationId, string symbol, int year, UserIdentity userIdentity, string token, List<FeedbackResponse> allFeedback, int actionLevel, long loggedInUserId, string objectiveUniqueId)
        {
            var contributorsKeys = new List<GoalKey>();
            foreach (var gk in goalKeys)
            {
                if (gk.GoalObjectiveId > 0)
                {
                    if (!(contributorsKeys.Any(x => x.GoalObjectiveId == gk.GoalObjectiveId)))
                    {
                        contributorsKeys.Add(gk);
                    }
                }
                else if (gk.GoalObjectiveId == 0)
                {
                    contributorsKeys.Add(gk);
                }
            }
            if (contributorsKeys.Count != 0)
            {
                var allTeamEmployees = await commonService.GetTeamEmployees();
                foreach (var cont in contributorsKeys)
                {
                    var sourceDetails = new List<GoalKey>();
                    var parentIds = new List<string>();
                    var score = new decimal();
                    var KrStatusId = new List<int>();

                    if (cont.GoalObjectiveId == 0)
                    {
                        var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).ToList();
                        var sourceKeyDetails = GetGoalKeyById(cont.ImportedId);
                        if (sourceKeyDetails != null)
                        {
                            sourceDetails.Add(sourceKeyDetails);
                        }
                        var contributors = ContributorsCommonResponse(keyContributors, allEmployee, sourceDetails, (long)cont.EmployeeId, loggedInUserId, false);

                        var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

                        if ((cont.GoalStatusId != (int)GoalStatus.Draft) || (cont.GoalStatusId == (int)GoalStatus.Public && cont.ImportedId >= 0))
                        {
                            if (cont.ImportedId != 0)
                            {
                                var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x =>
                                    x.GoalKeyId == cont.ImportedId && x.IsActive &&
                                    x.KrStatusId == (int)KrStatus.Accepted);
                                //if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                //{
                                //    parentIds.Add(parentDetails.GoalObjectiveId.ToString());
                                //}
                                //else
                                //{
                                //    parentIds.Add(cont.ImportedId.ToString());
                                //}

                                parentIds.Add(objectiveUniqueId);

                            }
                            KrStatusId.Add(cont.KrStatusId);

                            var contributorsCount = contributorsList.Select(x => x.EmployeeId).Distinct().Count();
                            cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(cont);
                            objOkrViewResponseDto.KrUniqueId = Guid.NewGuid().ToString();
                            objOkrViewResponseDto.ActionLevel = actionLevel;
                            objOkrViewResponseDto.Score = commonService.KeyScore(cont.Score);
                            objOkrViewResponseDto.OkrViewStandAloneContributors = await commonService.GetAllContributorAsync(2, cont.GoalKeyId, allEmployee.Results, userIdentity, token);

                            ///objOkrViewResponseDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(cont.GoalKeyId, token);
                            objOkrViewResponseDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == cont.GoalKeyId);

                            if (sourceKeyDetails != null)
                            {
                                var ownerFirstName = cont.Owner != 0
                                    ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId)?.FirstName
                                    : "";
                                var ownerLastName = cont.Owner != 0
                                    ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId)?.LastName
                                    : "";

                                objOkrViewResponseDto.OkrOwner = ownerFirstName + " " + ownerLastName;
                            }
                            objOkrViewResponseDto.ObjectiveUniqueId = Guid.NewGuid().ToString();


                            if (cont.TeamId > 0)
                            {
                                var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == cont.TeamId);
                                var allEmployeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);


                                objOkrViewResponseDto.OwnerDesignation = cont.Owner != 0
                                    ? allEmployeeDetails?.Designation
                                    : "";
                                objOkrViewResponseDto.OwnerEmailId = cont.Owner != 0
                                    ? allEmployeeDetails?.EmailId
                                    : "";
                                objOkrViewResponseDto.OwnerEmployeeCode = cont.Owner != 0
                                    ? allEmployeeDetails?.EmployeeCode
                                    : "";
                                objOkrViewResponseDto.OwnerImagePath = cont.Owner != 0
                                    ? allEmployeeDetails?.ImagePath
                                    : "";
                                objOkrViewResponseDto.OwnerLastName = cont.Owner != 0
                                    ? allEmployeeDetails?.LastName
                                    : "";
                                objOkrViewResponseDto.OwnerFirstName = cont.Owner != 0
                                    ? allEmployeeDetails?.FirstName
                                    : "";
                            }

                            objOkrViewResponseDto.IsContributor = commonService.GetGoalKeySource(loggedInUserId, cont.Source == 0 ? cont.GoalKeyId : cont.Source).Result.IsAligned;

                            if (cont.MetricId == (int)Metrics.Boolean)
                            {
                                if (contributorsList.Count > 0)
                                {
                                    objOkrViewResponseDto.InValue = contributorsList.Any(x => x.CurrentValue == 100)
                                        ? 100
                                        : Constants.Zero;
                                }

                                objOkrViewResponseDto.OutValue = cont.CurrentValue;
                            }
                            else if (cont.MetricId == (int)Metrics.NoUnits)
                            {
                                foreach (var item in contributorsList)
                                {
                                    score = score + item.CurrentValue;
                                }

                                objOkrViewResponseDto.InValue = Math.Round(Math.Abs(score));
                                objOkrViewResponseDto.OutValue = Math.Round(Math.Abs(cont.CurrentValue));

                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Dollar)
                            {
                                objOkrViewResponseDto.CurrencyInValue = Constants.DollarSymbol + Math.Round(Math.Abs(cont.ContributorValue));
                                objOkrViewResponseDto.CurrencyOutValue = Constants.DollarSymbol + Math.Round(Math.Abs(cont.CurrentValue - cont.StartValue + cont.ContributorValue));
                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Euro)
                            {
                                objOkrViewResponseDto.CurrencyInValue = Constants.EuroSymbol + Math.Round(Math.Abs((cont.ContributorValue)));
                                objOkrViewResponseDto.CurrencyOutValue = Constants.EuroSymbol + Math.Round(Math.Abs(cont.CurrentValue - cont.StartValue + cont.ContributorValue));
                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Pound)
                            {
                                objOkrViewResponseDto.CurrencyInValue = Constants.PoundSymbol + Math.Round(Math.Abs(cont.ContributorValue));
                                objOkrViewResponseDto.CurrencyOutValue = Constants.PoundSymbol + Math.Abs(cont.CurrentValue - cont.StartValue + cont.ContributorValue);
                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Rupee)
                            {
                                objOkrViewResponseDto.CurrencyInValue = Constants.RupeeSymbol + Math.Round(Math.Abs(cont.ContributorValue));
                                objOkrViewResponseDto.CurrencyOutValue = Constants.RupeeSymbol + Math.Round(Math.Abs(cont.CurrentValue - cont.StartValue + cont.ContributorValue));
                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Yen)
                            {
                                objOkrViewResponseDto.CurrencyInValue = Constants.YenSymbol + Math.Round(Math.Abs(cont.ContributorValue));
                                objOkrViewResponseDto.CurrencyOutValue = Constants.YenSymbol + Math.Round(Math.Abs(cont.CurrentValue - cont.StartValue + cont.ContributorValue));
                            }
                            else
                            {
                                objOkrViewResponseDto.InValue = Math.Round(Math.Abs((cont.ContributorValue)));
                                objOkrViewResponseDto.OutValue = Math.Round(Math.Abs((cont.CurrentValue - cont.StartValue) + cont.ContributorValue));
                            }



                            ////contributors parent id
                            KrUniqueIdDetails krUniqueIdDetail = new KrUniqueIdDetails();
                            krUniqueIdDetail.KrId = cont.GoalKeyId;
                            krUniqueIdDetail.KrUniqueId = objOkrViewResponseDto.KrUniqueId;
                            krUniqueIdDetails.Add(krUniqueIdDetail);

                            var krParent = krUniqueIdDetails.FirstOrDefault(x => x.KrId == cont.ImportedId)?.KrUniqueId;
                            objOkrViewResponseDto.KrParentId = krParent;

                            if (contributorsList.Count > 0)
                            {
                                await OkrViewContributorsResponse(contributorsList, okrViewResponse, krUniqueIdDetails, allEmployee, startDate, endDate, cycleDurationId, symbol, year, userIdentity, token, allFeedback, actionLevel + 1, loggedInUserId, objOkrViewResponseDto.ObjectiveUniqueId);
                            }

                            OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributors, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, cont.GoalKeyId, parentIds, false, contributorsList.Any(), true, cont.TeamId, symbol + "," + " " + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount, allFeedback, KrStatusId, allTeamEmployees);
                            if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                            {
                                okrViewResponse.Add(objOkrViewResponseDto);
                            }
                            else
                            {
                                okrViewResponse.RemoveAll(x => x.ObjectiveId == objOkrViewResponseDto.ObjectiveId && x.SourceResponse == false);
                                okrViewResponse.Add(objOkrViewResponseDto);
                            }
                        }
                    }

                    else if (cont.GoalObjectiveId > 0)
                    {
                        var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == cont.GoalObjectiveId && x.EmployeeId == cont.EmployeeId && x.IsActive);
                        var keys = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objective.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                        List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();
                        var parent = new List<string>();
                        var sourceIds = new List<GoalKey>();
                        var allKeyContributors = new List<GoalKey>();
                        var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).ToList();
                        var KrObjectiveStatusId = new List<int>();


                        foreach (var key in keys)
                        {
                            var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(key);
                            okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();
                            okrViewKeyResultDto.ActionLevel = actionLevel;
                            ///okrViewKeyResultDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(key.GoalKeyId, token);
                            okrViewKeyResultDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId);
                            okrViewKeyResultDto.OkrViewKeyContributors = await commonService.GetAllContributorAsync(2, key.GoalKeyId, allEmployee.Results, userIdentity, token);

                            KrObjectiveStatusId.Add(key.KrStatusId);

                            if (key.TeamId > 0)
                            {
                                var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == key.TeamId);
                                var allEmployeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);


                                okrViewKeyResultDto.OwnerDesignation = key.Owner != 0
                                    ? allEmployeeDetails?.Designation
                                    : "";
                                okrViewKeyResultDto.OwnerEmailId = key.Owner != 0
                                    ? allEmployeeDetails?.EmailId
                                    : "";
                                okrViewKeyResultDto.OwnerEmployeeCode = key.Owner != 0
                                    ? allEmployeeDetails?.EmployeeCode
                                    : "";
                                okrViewKeyResultDto.OwnerImagePath = key.Owner != 0
                                    ? allEmployeeDetails?.ImagePath
                                    : "";
                                okrViewKeyResultDto.OwnerLastName = key.Owner != 0
                                    ? allEmployeeDetails?.LastName
                                    : "";
                                okrViewKeyResultDto.OwnerFirstName = key.Owner != 0
                                    ? allEmployeeDetails?.FirstName
                                    : "";
                            }

                            okrViewKeyResultDto.IsContributor = commonService.GetGoalKeySource(loggedInUserId, key.Source == 0 ? key.GoalKeyId : key.Source).Result.IsAligned;

                            var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == key.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).ToList();
                            allKeyContributors.AddRange(contributors);

                            ////contributors parent id
                            KrUniqueIdDetails krUniqueIdDetail = new KrUniqueIdDetails();
                            krUniqueIdDetail.KrId = key.GoalKeyId;
                            krUniqueIdDetail.KrUniqueId = okrViewKeyResultDto.KrUniqueId;
                            krUniqueIdDetails.Add(krUniqueIdDetail);

                            var krParent = krUniqueIdDetails.FirstOrDefault(x => x.KrId == key.ImportedId)?.KrUniqueId;
                            okrViewKeyResultDto.ParentId = krParent;
                            okrViewKeyResultDto.KrScore = commonService.KeyScore(key.Score);

                            cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
                            okrViewKeyResultResponse.Add(okrViewKeyResultDto);

                            if (key.ImportedId != 0)
                            {
                                var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == key.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);

                                //if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                //{
                                //    parent.Add(parentDetails.GoalObjectiveId.ToString());
                                //}
                                //else
                                //{
                                //    parent.Add(key.ImportedId.ToString());
                                //}

                                parent.Add(objectiveUniqueId);
                            }

                            var sourceKeyDetails = GetGoalKeyById(key.ImportedId);
                            if (sourceKeyDetails != null)
                            {
                                sourceIds.Add(sourceKeyDetails);

                                var ownerFirstName = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId)?.FirstName : "";
                                var ownerLastName = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId)?.LastName : "";
                                okrViewKeyResultDto.OkrOwner = ownerFirstName + " " + ownerLastName;

                            }
                            if (key.MetricId == (int)Metrics.Boolean)
                            {
                                if (contributors.Count > 0)
                                    okrViewKeyResultDto.InValue = contributors.Any(x => x.CurrentValue == 100) ? 100 : Constants.Zero;
                                okrViewKeyResultDto.OutValue = key.MetricId == (int)Metrics.Boolean ? key.CurrentValue : Math.Abs((key.CurrentValue - key.StartValue) + key.ContributorValue);


                            }
                            else if (key.MetricId == (int)Metrics.NoUnits)
                            {
                                foreach (var item in contributors)
                                {
                                    score = score + item.CurrentValue;
                                }

                                okrViewKeyResultDto.InValue = Math.Round(Math.Abs((score)));
                                okrViewKeyResultDto.OutValue = Math.Round(Math.Abs(key.CurrentValue));

                            }
                            else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Dollar)
                            {
                                okrViewKeyResultDto.CurrencyInValue = Constants.DollarSymbol + Math.Round(Math.Abs((key.ContributorValue)));
                                okrViewKeyResultDto.CurrencyOutValue = Constants.DollarSymbol + Math.Round(Math.Abs(key.CurrentValue - key.StartValue + key.ContributorValue));
                            }
                            else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Euro)
                            {
                                okrViewKeyResultDto.CurrencyInValue = Constants.EuroSymbol + Math.Round(Math.Abs(key.ContributorValue));
                                okrViewKeyResultDto.CurrencyOutValue = Constants.EuroSymbol + Math.Round(Math.Abs(key.CurrentValue - key.StartValue + key.ContributorValue));
                            }
                            else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Pound)
                            {
                                okrViewKeyResultDto.CurrencyInValue = Constants.PoundSymbol + Math.Round(Math.Abs(key.ContributorValue));
                                okrViewKeyResultDto.CurrencyOutValue = Constants.PoundSymbol + Math.Round(Math.Abs(key.CurrentValue - key.StartValue + key.ContributorValue));
                            }
                            else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Rupee)
                            {
                                okrViewKeyResultDto.CurrencyInValue = Constants.RupeeSymbol + Math.Round(Math.Abs(key.ContributorValue));
                                okrViewKeyResultDto.CurrencyOutValue = Constants.RupeeSymbol + Math.Round(Math.Abs(key.CurrentValue - key.StartValue + key.ContributorValue));
                            }
                            else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Yen)
                            {
                                okrViewKeyResultDto.CurrencyInValue = Constants.YenSymbol + Math.Round(Math.Abs(key.ContributorValue));
                                okrViewKeyResultDto.CurrencyOutValue = Constants.YenSymbol + Math.Round(Math.Abs(key.CurrentValue - key.StartValue + key.ContributorValue));
                            }
                            else
                            {
                                okrViewKeyResultDto.InValue = Math.Round(Math.Abs(key.ContributorValue));
                                okrViewKeyResultDto.OutValue = key.MetricId == (int)Metrics.Boolean ? key.CurrentValue : Math.Round(Math.Abs((key.CurrentValue - key.StartValue) + key.ContributorValue));

                            }
                        }

                        //var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).Select(x => x.EmployeeId).ToList();
                        var keyContributors = allKeyContributors.Select(x => x.EmployeeId).ToList();
                        var contributorDetails = ContributorsCommonResponse(keyContributors, allEmployee, sourceIds, (long)cont.EmployeeId, loggedInUserId, false);

                        if (objective != null)
                        {
                            var sourceKeyDetails = GetGoalKeyById(objective.ImportedId);
                            var sourceCount = sourceKeyDetails != null ? 1 : 0;

                            var contributorsCount = allKeyContributors.Distinct().Count();
                            objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, startDate, endDate, objective.Score, cycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objective);
                            objOkrViewResponseDto.Score = commonService.KeyScore(objective.Score);
                            objOkrViewResponseDto.ActionLevel = actionLevel;
                            objOkrViewResponseDto.ObjectiveUniqueId = Guid.NewGuid().ToString();
                            objOkrViewResponseDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(objective.GoalObjectiveId, token);
                            objOkrViewResponseDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == objective.GoalObjectiveId);

                            var ownerFirstName = objective.Owner != 0
                                ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == objective.Owner)?.FirstName
                                : "";
                            var ownerLastName = objective.Owner != 0
                                ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == objective.Owner)?.LastName
                                : "";
                            objOkrViewResponseDto.OkrOwner = ownerFirstName + " " + ownerLastName;
                            if (objective.TeamId > 0)
                            {
                                var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == objective.TeamId);
                                var allEmployeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);

                                objOkrViewResponseDto.OwnerDesignation = objective.Owner != 0
                                    ? allEmployeeDetails
                                        ?.Designation
                                    : "";
                                objOkrViewResponseDto.OwnerEmailId = objective.Owner != 0
                                    ? allEmployeeDetails?.EmailId
                                    : "";
                                objOkrViewResponseDto.OwnerEmployeeCode = objective.Owner != 0
                                    ? allEmployeeDetails
                                        ?.EmployeeCode
                                    : "";
                                objOkrViewResponseDto.OwnerImagePath = objective.Owner != 0
                                    ? allEmployeeDetails
                                        ?.ImagePath
                                    : "";
                                objOkrViewResponseDto.OwnerLastName = objective.Owner != 0
                                    ? allEmployeeDetails?.LastName
                                    : "";
                                objOkrViewResponseDto.OwnerFirstName = objective.Owner != 0 ? allEmployeeDetails?.FirstName : "";
                            }


                            if (keyContributors.Count > 0)
                            {
                                await OkrViewContributorsResponse(contributorsList, okrViewResponse, krUniqueIdDetails, allEmployee, startDate, endDate, cycleDurationId, symbol, year, userIdentity, token, allFeedback, actionLevel + 1, loggedInUserId, objOkrViewResponseDto.ObjectiveUniqueId);
                            }

                            OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, cont.GoalObjectiveId, parent, false, contributorsList.Any(), true, cont.TeamId, symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount, allFeedback, KrObjectiveStatusId, allTeamEmployees);
                            if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                            {
                                okrViewResponse.Add(objOkrViewResponseDto);
                            }
                            else
                            {
                                ///when contributors are same but having different parents
                                var duplicateObjectiveIds = okrViewResponse
                                    .Where(x => x.ObjectiveId == objOkrViewResponseDto.ObjectiveId)
                                    .Select(x => x.Parent).ToList();
                                if (duplicateObjectiveIds.Contains(objOkrViewResponseDto.Parent))
                                {
                                    okrViewResponse.RemoveAll(x => x.ObjectiveId == objOkrViewResponseDto.ObjectiveId && x.SourceResponse == false);
                                    okrViewResponse.Add(objOkrViewResponseDto);
                                }
                                else
                                    okrViewResponse.Add(objOkrViewResponseDto);

                            }
                        }

                    }
                }
            }
        }

        private async Task OkrViewSourceResponse(List<GoalKey> goalKeys, List<OkrViewResponse> okrViewResponse, List<KrUniqueIdDetails> krUniqueIdDetails, EmployeeResult allEmployee, DateTime startDate, DateTime endDate, long cycleDurationId, string symbol, int year, UserIdentity userIdentity, string token, List<FeedbackResponse> allFeedback, int actionLevel, long loggedInUserId)
        {
            var contributorsKeys = new List<GoalKey>();
            foreach (var gk in goalKeys)
            {
                if (gk.GoalObjectiveId > 0)
                {
                    if (!(contributorsKeys.Any(x => x.GoalObjectiveId == gk.GoalObjectiveId)))
                    {
                        contributorsKeys.Add(gk);
                    }
                }
                else if (gk.GoalObjectiveId == 0)
                {
                    contributorsKeys.Add(gk);
                }
            }
            if (contributorsKeys.Count != 0)
            {
                var allTeamEmployees = await commonService.GetTeamEmployees();
                foreach (var cont in contributorsKeys)
                {
                    var KrStatusId = new List<int>();

                    if (cont.GoalObjectiveId == 0)
                    {
                        var parentIds = new List<string>();
                        var sourceIds = new List<GoalKey>();
                        var score = new decimal();

                        var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).ToList();

                        var sourceDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == cont.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

                        if ((cont.GoalStatusId != (int)GoalStatus.Draft) || (cont.GoalStatusId == (int)GoalStatus.Public && cont.ImportedId >= 0))
                        {
                            var sourceKeyDetails = GetGoalKeyById(cont.ImportedId);

                            if (sourceKeyDetails != null)
                            {
                                sourceIds.Add(sourceKeyDetails);
                            }

                            var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).ToList();
                            var contributors = ContributorsCommonResponse(keyContributors, allEmployee, sourceIds, (long)cont.EmployeeId, loggedInUserId, false);

                            var sourceCount = sourceKeyDetails != null ? 1 : 0;
                            KrStatusId.Add(cont.KrStatusId);

                            var contributorsCount = contributorsList.Select(x => x.EmployeeId).Distinct().Count();
                            cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(cont);
                            ///objOkrViewResponseDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(cont.GoalKeyId, token);
                            objOkrViewResponseDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == cont.GoalKeyId);
                            objOkrViewResponseDto.OkrViewStandAloneContributors = await commonService.GetAllContributorAsync(2, objOkrViewResponseDto.ObjectiveId, allEmployee.Results, userIdentity, token);

                            objOkrViewResponseDto.ActionLevel = actionLevel;
                            objOkrViewResponseDto.Score = commonService.KeyScore(cont.Score);
                            objOkrViewResponseDto.SourceResponse = true;
                            if (sourceKeyDetails != null)
                            {
                                var ownerFirstName = cont.Owner != 0
                                    ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId)?.FirstName
                                    : "";
                                var ownerLastName = cont.Owner != 0
                                    ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == sourceKeyDetails.EmployeeId)?.LastName
                                    : "";

                                objOkrViewResponseDto.OkrOwner = ownerFirstName + " " + ownerLastName;
                            }

                            if (cont.TeamId > 0)
                            {
                                var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == cont.TeamId);
                                var allEmployeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);


                                objOkrViewResponseDto.OwnerDesignation = cont.Owner != 0
                                    ? allEmployeeDetails?.Designation
                                    : "";
                                objOkrViewResponseDto.OwnerEmailId = cont.Owner != 0
                                    ? allEmployeeDetails?.EmailId
                                    : "";
                                objOkrViewResponseDto.OwnerEmployeeCode = cont.Owner != 0
                                    ? allEmployeeDetails?.EmployeeCode
                                    : "";
                                objOkrViewResponseDto.OwnerImagePath = cont.Owner != 0
                                    ? allEmployeeDetails?.ImagePath
                                    : "";
                                objOkrViewResponseDto.OwnerLastName = cont.Owner != 0
                                    ? allEmployeeDetails?.LastName
                                    : "";
                                objOkrViewResponseDto.OwnerFirstName = cont.Owner != 0
                                    ? allEmployeeDetails?.FirstName
                                    : "";
                            }

                            objOkrViewResponseDto.IsContributor = commonService.GetGoalKeySource(loggedInUserId, cont.Source == 0 ? cont.GoalKeyId : cont.Source).Result.IsAligned;

                            objOkrViewResponseDto.ObjectiveUniqueId = cont.GoalKeyId.ToString();


                            if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Dollar)
                            {
                                var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                foreach (var item in myOkrCurrentValue)
                                {
                                    score = score + item.CurrentValue;
                                }
                                objOkrViewResponseDto.CurrencyInValue = Constants.DollarSymbol + Math.Round(Math.Abs((score)));

                                objOkrViewResponseDto.CurrencyOutValue = Constants.DollarSymbol + Math.Round(Math.Abs(cont.CurrentValue));

                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Euro)
                            {

                                var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                foreach (var item in myOkrCurrentValue)
                                {
                                    score = score + item.CurrentValue;
                                }
                                objOkrViewResponseDto.CurrencyInValue = Constants.EuroSymbol + Math.Round(Math.Abs((score)));

                                objOkrViewResponseDto.CurrencyOutValue = Constants.EuroSymbol + Math.Round(Math.Abs(cont.CurrentValue));

                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Pound)
                            {
                                var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId);
                                foreach (var item in myOkrCurrentValue)
                                {
                                    score = score + item.CurrentValue;
                                }
                                objOkrViewResponseDto.CurrencyInValue = Constants.PoundSymbol + Math.Round(Math.Abs((score)));

                                objOkrViewResponseDto.CurrencyOutValue = Constants.PoundSymbol + Math.Round(Math.Abs(cont.CurrentValue));


                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Rupee)
                            {
                                var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                foreach (var item in myOkrCurrentValue)
                                {
                                    score = score + item.CurrentValue;
                                }
                                objOkrViewResponseDto.CurrencyInValue = Constants.RupeeSymbol + Math.Round(Math.Abs((score)));

                                objOkrViewResponseDto.CurrencyOutValue = Constants.RupeeSymbol + Math.Round(Math.Abs(cont.CurrentValue));

                            }
                            else if (cont.MetricId == (int)Metrics.Currency && cont.CurrencyId == (int)CurrencyValues.Yen)
                            {
                                var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                foreach (var item in myOkrCurrentValue)
                                {
                                    score = score + item.CurrentValue;
                                }
                                objOkrViewResponseDto.CurrencyInValue = Constants.YenSymbol + Math.Round(Math.Abs((score)));

                                objOkrViewResponseDto.CurrencyOutValue = Constants.YenSymbol + Math.Round(Math.Abs(cont.CurrentValue));

                            }
                            else if (cont.MetricId == (int)Metrics.Percentage || cont.MetricId == (int)Metrics.Numbers)
                            {
                                //objOkrViewResponseDto.InValue = Math.Round(Math.Abs(cont.ContributorValue));
                                //objOkrViewResponseDto.OutValue = Math.Round(Math.Abs((cont.CurrentValue - cont.StartValue) + cont.ContributorValue));
                                var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                foreach (var item in myOkrCurrentValue)
                                {
                                    score = score + item.CurrentValue;
                                }
                                objOkrViewResponseDto.InValue = Math.Round(Math.Abs((score)));

                                objOkrViewResponseDto.OutValue = Math.Round(Math.Abs(cont.CurrentValue));
                            }


                            var uniqueIdForNextSource = Guid.NewGuid().ToString();
                            KrUniqueIdDetails krUniqueIdDetail = new KrUniqueIdDetails();

                            if (cont.ImportedId != 0)
                            {
                                var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == cont.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);
                                if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                {

                                    ///when two cards have same parent so instead of removing existing card and adding new card point to existing card
                                    var existingObjectiveDetails = okrViewResponse.Find(x => x.ObjectiveId == parentDetails.GoalObjectiveId);
                                    if (existingObjectiveDetails != null)
                                    {
                                        var krDetails = existingObjectiveDetails.OkrViewKeyResults.FirstOrDefault(x => x.KrId == cont.ImportedId);
                                        if (krDetails != null)
                                        {
                                            objOkrViewResponseDto.KrParentId = krDetails.KrUniqueId;

                                        }

                                    }
                                    else
                                    {
                                        objOkrViewResponseDto.KrParentId = uniqueIdForNextSource;

                                        krUniqueIdDetail.KrUniqueId = uniqueIdForNextSource;
                                        krUniqueIdDetail.KrId = cont.ImportedId;
                                        krUniqueIdDetails.Add(krUniqueIdDetail);
                                    }

                                    parentIds.Add(parentDetails.GoalObjectiveId.ToString());
                                }
                                else
                                {
                                    parentIds.Add(cont.ImportedId.ToString());
                                }

                                //we are providing unique id for the parent from here on the basis of goalkeyid
                               // objOkrViewResponseDto.KrParentId = uniqueIdForNextSource;

                                /////next source goalkeyid and  unique id 
                                //krUniqueIdDetail.KrUniqueId = uniqueIdForNextSource;
                                //krUniqueIdDetail.KrId = cont.ImportedId;
                                //krUniqueIdDetail.KrCurrentValue = cont.CurrentValue;
                                //krUniqueIdDetails.Add(krUniqueIdDetail);
                            }


                            if (actionLevel == -1)
                            {
                                //logged in user parent for bonny
                                objOkrViewResponseDto.KrUniqueId = Guid.NewGuid().ToString();
                                var krParent = okrViewResponse.FirstOrDefault(x => x.ImportedId == cont.GoalKeyId);
                                if (krParent != null)
                                {
                                    krParent.KrParentId = objOkrViewResponseDto.KrUniqueId;
                                }

                                if (cont.MetricId == (int)Metrics.Boolean)
                                {
                                    if (contributorsList.Count > 0)
                                    {
                                        var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId);
                                        objOkrViewResponseDto.InValue =
                                            myOkrCurrentValue.Any(x => x.CurrentValue == 100) ? 100 : 0;
                                    }

                                    objOkrViewResponseDto.OutValue = cont.CurrentValue;

                                }

                                if (cont.MetricId == (int)Metrics.NoUnits)
                                {
                                    var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId);
                                    foreach (var item in myOkrCurrentValue)
                                    {
                                        score = score + item.CurrentValue;
                                    }
                                    objOkrViewResponseDto.InValue = Math.Round(Math.Abs((score)));

                                    objOkrViewResponseDto.OutValue = Math.Round(Math.Abs(cont.CurrentValue));

                                }
                            }
                            if (actionLevel < -1)
                            {
                                var uniqueId = krUniqueIdDetails.FirstOrDefault(x => x.KrId == cont.GoalKeyId)?.KrUniqueId;
                                objOkrViewResponseDto.KrUniqueId = uniqueId == null ? Guid.NewGuid().ToString() : uniqueId;

                                if (cont.MetricId == (int)Metrics.Boolean)
                                {
                                    if (contributorsList.Count > 0)
                                    {
                                        var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId);
                                        objOkrViewResponseDto.InValue = myOkrCurrentValue.Any(x => x.CurrentValue == 100) ? 100 : 0;
                                    }

                                    objOkrViewResponseDto.OutValue = cont.CurrentValue;

                                }

                                if (cont.MetricId == (int)Metrics.NoUnits)
                                {
                                    //A parent can have n number of contributors but on right hand side we only show one parent
                                    var myOkrCurrentValue = contributorsList.Where(x => x.EmployeeId == loggedInUserId);
                                    foreach (var item in myOkrCurrentValue)
                                    {
                                        score = score + item.CurrentValue;
                                    }
                                    objOkrViewResponseDto.InValue = Math.Round(Math.Abs((score)));

                                    objOkrViewResponseDto.OutValue = Math.Round(Math.Abs(cont.CurrentValue));

                                }
                            }

                            if (sourceDetails.Count > 0)
                            {
                                await OkrViewSourceResponse(sourceDetails, okrViewResponse, krUniqueIdDetails, allEmployee, startDate, endDate, cycleDurationId, symbol, year, userIdentity, token, allFeedback, actionLevel - 1, (long)cont.EmployeeId);
                            }

                            OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributors, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, cont.GoalKeyId, parentIds, false, keyContributors.Any(), sourceCount != 0, cont.TeamId, symbol + "," + " " + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount, allFeedback, KrStatusId, allTeamEmployees);
                            if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                            {
                                okrViewResponse.Add(objOkrViewResponseDto);
                            }
                            //else
                            //{
                            //    okrViewResponse.RemoveAll(x => x.ObjectiveId == objOkrViewResponseDto.ObjectiveId && x.SourceResponse == true);
                            //    okrViewResponse.Add(objOkrViewResponseDto);
                            //}

                        }

                    }

                    else if (cont.GoalObjectiveId > 0)
                    {
                        var parent = new List<string>();
                        var source = new List<GoalKey>();
                        var allKeyContributors = new List<GoalKey>();
                        var sourceDetails = new List<GoalKey>();

                        List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();
                        var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == cont.EmployeeId && x.GoalObjectiveId == cont.GoalObjectiveId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived);
                        if (objective != null)
                        {
                            var keys = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objective.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

                            var KrObjectiveStatusId = new List<int>();


                            foreach (var key in keys)
                            {
                                var score = new decimal();
                                KrUniqueIdDetails krUniqueIdDetail = new KrUniqueIdDetails();
                                var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(key);
                                ///okrViewKeyResultDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(key.GoalKeyId, token);
                                okrViewKeyResultDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId);

                                okrViewKeyResultDto.ActionLevel = actionLevel;
                                okrViewKeyResultDto.KrScore = commonService.KeyScore(key.Score);

                                KrObjectiveStatusId.Add(key.KrStatusId);
                                var sourceKeyDetails = GetGoalKeyById(key.ImportedId);

                                okrViewKeyResultDto.OkrViewKeyContributors = await commonService.GetAllContributorAsync(2, key.GoalKeyId, allEmployee.Results, userIdentity, token);


                                if (sourceKeyDetails != null)
                                {
                                    source.Add(sourceKeyDetails);
                                    sourceDetails.Add(sourceKeyDetails);

                                    var keyOwnerFirstName = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.FirstName : "";

                                    var keyOwnerLastName = key.Owner != 0 ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.Owner)?.LastName : "";

                                    okrViewKeyResultDto.OkrOwner = keyOwnerFirstName + " " + keyOwnerLastName;

                                }
                                if (key.TeamId > 0)
                                {
                                    var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == key.TeamId);
                                    var allEmployeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);

                                    okrViewKeyResultDto.OwnerDesignation = key.Owner != 0
                                        ? allEmployeeDetails
                                            ?.Designation
                                        : "";
                                    okrViewKeyResultDto.OwnerEmailId = key.Owner != 0
                                        ? allEmployeeDetails?.EmailId
                                        : "";
                                    okrViewKeyResultDto.OwnerEmployeeCode = key.Owner != 0
                                        ? allEmployeeDetails
                                            ?.EmployeeCode
                                        : "";
                                    okrViewKeyResultDto.OwnerImagePath = key.Owner != 0
                                        ? allEmployeeDetails
                                            ?.ImagePath
                                        : "";
                                    okrViewKeyResultDto.OwnerLastName = key.Owner != 0
                                        ? allEmployeeDetails?.LastName
                                        : "";
                                    okrViewKeyResultDto.OwnerFirstName = key.Owner != 0
                                        ? allEmployeeDetails
                                            ?.FirstName
                                        : "";
                                }

                                okrViewKeyResultDto.IsContributor = commonService
                                    .GetGoalKeySource(loggedInUserId, key.Source == 0 ? key.GoalKeyId : key.Source)
                                    .Result.IsAligned;

                                var contributors = goalKeyRepo.GetQueryable().Where(x =>
                                    x.ImportedId == key.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey &&
                                    x.IsActive && x.KrStatusId == (int)KrStatus.Accepted &&
                                    x.GoalStatusId != (int)GoalStatus.Archived).ToList();
                                allKeyContributors.AddRange(contributors);

                                var uniqueIdForNextSource = Guid.NewGuid().ToString();

                                if (actionLevel == -1)
                                {
                                    //logged in user parent for bonny
                                    okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();

                                    if (okrViewResponse.Any(x =>
                                        x.OkrViewKeyResults.Any(x => x.ImportedId == key.GoalKeyId)))
                                    {
                                        var krParent = okrViewResponse.FirstOrDefault(x =>
                                            x.OkrViewKeyResults.Any(x => x.ImportedId == key.GoalKeyId));
                                        var krDetails =
                                            krParent.OkrViewKeyResults.FirstOrDefault(
                                                x => x.ImportedId == key.GoalKeyId);
                                        krDetails.ParentId = okrViewKeyResultDto.KrUniqueId;

                                    }
                                    else if ((okrViewResponse.Any(x => x.ImportedId == key.GoalKeyId)))
                                    {
                                        var krParent =
                                            okrViewResponse.FirstOrDefault(x => x.ImportedId == key.GoalKeyId);
                                        krParent.KrParentId = okrViewKeyResultDto.KrUniqueId;
                                    }


                                    if (key.MetricId == (int)Metrics.Boolean)
                                    {
                                        if (contributors.Count > 0)
                                        {
                                            var myOkrCurrentValue = contributors.FirstOrDefault(x => x.EmployeeId == loggedInUserId);
                                            if (myOkrCurrentValue != null)
                                            {
                                                okrViewKeyResultDto.InValue = myOkrCurrentValue.CurrentValue;
                                            }

                                        }

                                        okrViewKeyResultDto.OutValue = key.CurrentValue;

                                    }

                                    if (key.MetricId == (int)Metrics.NoUnits)
                                    {
                                        //A parent can have n number of contributors but on right hand side we only show one parent
                                        var myOkrCurrentValue = contributors.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                        foreach (var item in myOkrCurrentValue)
                                        {
                                            score = score + item.CurrentValue;
                                        }
                                        okrViewKeyResultDto.InValue = Math.Round(Math.Abs((score)));

                                        okrViewKeyResultDto.OutValue = Math.Round(Math.Abs(key.CurrentValue));

                                    }



                                }

                                if (actionLevel < -1)
                                {
                                    var uniqueId = krUniqueIdDetails.FirstOrDefault(x => x.KrId == key.GoalKeyId)
                                        ?.KrUniqueId;
                                    okrViewKeyResultDto.KrUniqueId =
                                        uniqueId == null ? Guid.NewGuid().ToString() : uniqueId;

                                    if (key.MetricId == (int)Metrics.Boolean)
                                    {
                                        if (contributors.Count > 0)
                                        {
                                            var myOkrCurrentValue = contributors.FirstOrDefault(x => x.EmployeeId == loggedInUserId);
                                            okrViewKeyResultDto.InValue = myOkrCurrentValue.CurrentValue;
                                        }

                                        okrViewKeyResultDto.OutValue = key.CurrentValue;

                                    }

                                    if (key.MetricId == (int)Metrics.NoUnits)
                                    {
                                        //A parent can have n number of contributors but on right hand side we only show one parent
                                        var myOkrCurrentValue = contributors.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                        foreach (var item in myOkrCurrentValue)
                                        {
                                            score = score + item.CurrentValue;
                                        }
                                        okrViewKeyResultDto.InValue = Math.Round(Math.Abs((score)));

                                        okrViewKeyResultDto.OutValue = Math.Round(Math.Abs(key.CurrentValue));

                                    }

                                }


                                cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate,
                                    cont.Score, cycleDurationId);
                                okrViewKeyResultResponse.Add(okrViewKeyResultDto);

                                if (key.ImportedId != 0)
                                {
                                    var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x =>
                                        x.GoalKeyId == key.ImportedId && x.IsActive &&
                                        x.KrStatusId == (int)KrStatus.Accepted);


                                    if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                    {
                                        parent.Add(parentDetails.GoalObjectiveId.ToString());

                                        ///when two cards have same parent so instead of removing existing card and adding new card point to existing card
                                        var existingObjectiveDetails = okrViewResponse.Find(x => x.ObjectiveId == parentDetails.GoalObjectiveId);
                                        if (existingObjectiveDetails != null)
                                        {
                                            var krDetails = existingObjectiveDetails.OkrViewKeyResults.FirstOrDefault(x => x.KrId == key.ImportedId);
                                            if (krDetails != null)
                                            {
                                                okrViewKeyResultDto.ParentId = krDetails.KrUniqueId;

                                            }

                                        }
                                        else
                                        {
                                            okrViewKeyResultDto.ParentId = uniqueIdForNextSource;

                                            krUniqueIdDetail.KrUniqueId = uniqueIdForNextSource;
                                            krUniqueIdDetail.KrId = key.ImportedId;
                                            krUniqueIdDetails.Add(krUniqueIdDetail);
                                        }


                                    }
                                    else
                                    {
                                        parent.Add(cont.ImportedId.ToString());
                                    }


                                    ///next source goalkeyid and  unique id 
                                    //krUniqueIdDetail.KrUniqueId = uniqueIdForNextSource;
                                    //krUniqueIdDetail.KrId = key.ImportedId;
                                    //krUniqueIdDetails.Add(krUniqueIdDetail);
                                }


                                if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Dollar)
                                {
                                    var myOkrCurrentValue = contributors.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                    foreach (var item in myOkrCurrentValue)
                                    {
                                        score = score + item.CurrentValue;
                                    }
                                    okrViewKeyResultDto.CurrencyInValue = Constants.DollarSymbol + Math.Round(Math.Abs((score)));

                                    okrViewKeyResultDto.CurrencyOutValue = Constants.DollarSymbol + Math.Round(Math.Abs(key.CurrentValue));

                                }
                                else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Euro)
                                {
                                    var myOkrCurrentValue = contributors.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                    foreach (var item in myOkrCurrentValue)
                                    {
                                        score = score + item.CurrentValue;
                                    }
                                    okrViewKeyResultDto.CurrencyInValue = Constants.EuroSymbol + Math.Round(Math.Abs((score)));

                                    okrViewKeyResultDto.CurrencyOutValue = Constants.EuroSymbol + Math.Round(Math.Abs(key.CurrentValue));

                                }
                                else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Pound)
                                {
                                    var myOkrCurrentValue = contributors.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                    foreach (var item in myOkrCurrentValue)
                                    {
                                        score = score + item.CurrentValue;
                                    }
                                    okrViewKeyResultDto.CurrencyInValue = Constants.PoundSymbol + Math.Round(Math.Abs((score)));

                                    okrViewKeyResultDto.CurrencyOutValue = Constants.PoundSymbol + Math.Round(Math.Abs(key.CurrentValue));

                                }
                                else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Rupee)
                                {
                                    var myOkrCurrentValue = contributors.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                    foreach (var item in myOkrCurrentValue)
                                    {
                                        score = score + item.CurrentValue;
                                    }
                                    okrViewKeyResultDto.CurrencyInValue = Constants.RupeeSymbol + Math.Round(Math.Abs((score)));

                                    okrViewKeyResultDto.CurrencyOutValue = Constants.RupeeSymbol + Math.Round(Math.Abs(key.CurrentValue));

                                }
                                else if (key.MetricId == (int)Metrics.Currency && key.CurrencyId == (int)CurrencyValues.Yen)
                                {

                                    var myOkrCurrentValue = contributors.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                    foreach (var item in myOkrCurrentValue)
                                    {
                                        score = score + item.CurrentValue;
                                    }
                                    okrViewKeyResultDto.CurrencyInValue = Constants.YenSymbol + Math.Round(Math.Abs((score)));

                                    okrViewKeyResultDto.CurrencyOutValue = Constants.YenSymbol + Math.Round(Math.Abs(key.CurrentValue));

                                }
                                else if (key.MetricId == (int)Metrics.Percentage || key.MetricId == (int)Metrics.Numbers)
                                {
                                    //A parent can have n number of contributors but on right hand side we only show one parent
                                    var myOkrCurrentValue = contributors.Where(x => x.EmployeeId == loggedInUserId).ToList();
                                    foreach (var item in myOkrCurrentValue)
                                    {
                                        score = score + item.CurrentValue;
                                    }
                                    okrViewKeyResultDto.InValue = Math.Round(Math.Abs((score)));

                                    okrViewKeyResultDto.OutValue = Math.Round(Math.Abs(key.CurrentValue));

                                    // okrViewKeyResultDto.InValue = Math.Round(Math.Abs(key.ContributorValue));
                                    //okrViewKeyResultDto.OutValue = key.MetricId == (int)Metrics.Boolean ? key.CurrentValue : Math.Round(Math.Abs((key.CurrentValue - key.StartValue) + key.ContributorValue));
                                }


                            }

                            var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).Select(x => x.EmployeeId).ToList();
                            var contributorDetails = ContributorsCommonResponse(keyContributors, allEmployee, source, (long)cont.EmployeeId, loggedInUserId, false);

                            var contributorsCount = allKeyContributors.Select(x => x.EmployeeId).Distinct().Count();
                            objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, startDate, endDate, objective.Score, cycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objective);
                            objOkrViewResponseDto.IsUnreadFeedback = await commonService.GetReadFeedbackResponse(objective.GoalObjectiveId, token);
                            objOkrViewResponseDto.IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == objective.GoalObjectiveId);

                            objOkrViewResponseDto.Score = commonService.KeyScore(objective.Score);
                            objOkrViewResponseDto.SourceResponse = true;
                            objOkrViewResponseDto.ObjectiveUniqueId = objective.GoalObjectiveId.ToString();

                            var ownerFirstName = objective.Owner != 0
                                ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == objective.Owner)?.FirstName
                                : "";
                            var ownerLastName = objective.Owner != 0
                                ? allEmployee.Results.FirstOrDefault(x => x.EmployeeId == objective.Owner)?.LastName
                                : "";
                            objOkrViewResponseDto.OkrOwner = ownerFirstName + " " + ownerLastName;

                            if (objective.TeamId > 0)
                            {
                                var teamDetails = allTeamEmployees.FirstOrDefault(x => x.OrganisationId == objective.TeamId);
                                var allEmployeeDetails = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == teamDetails?.OrganisationHead);


                                objOkrViewResponseDto.OwnerDesignation = objective.Owner != 0
                                    ? allEmployeeDetails
                                        ?.Designation
                                    : "";
                                objOkrViewResponseDto.OwnerEmailId = objective.Owner != 0
                                    ? allEmployeeDetails?.EmailId
                                    : "";
                                objOkrViewResponseDto.OwnerEmployeeCode = objective.Owner != 0
                                    ? allEmployeeDetails
                                        ?.EmployeeCode
                                    : "";
                                objOkrViewResponseDto.OwnerImagePath = objective.Owner != 0
                                    ? allEmployeeDetails
                                        ?.ImagePath
                                    : "";
                                objOkrViewResponseDto.OwnerLastName = objective.Owner != 0
                                    ? allEmployeeDetails?.LastName
                                    : "";
                                objOkrViewResponseDto.OwnerFirstName = objective.Owner != 0
                                    ? allEmployeeDetails
                                        ?.FirstName
                                    : "";
                            }

                            if (sourceDetails.Count > 0)
                            {
                                await OkrViewSourceResponse(sourceDetails, okrViewResponse, krUniqueIdDetails, allEmployee, startDate, endDate, cycleDurationId, symbol, year, userIdentity, token, allFeedback, actionLevel - 1, objective.EmployeeId);
                            }

                            OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, objective.GoalObjectiveId, parent, false, keyContributors.Any(), sourceDetails.Count > 0, objective.TeamId, symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount, allFeedback, KrObjectiveStatusId, allTeamEmployees);
                            if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                            {

                                okrViewResponse.Add(objOkrViewResponseDto);
                            }
                            else if (actionLevel == -1)
                            {
                                okrViewResponse.RemoveAll(x => x.ObjectiveId == objOkrViewResponseDto.ObjectiveId && x.SourceResponse == true);
                                okrViewResponse.Add(objOkrViewResponseDto);
                            }

                        }
                    }
                }
            }
        }

        private async Task TeamsOkrContributorsResponse(List<GoalKey> goalKeys, List<AllTeamOkrViewResponse> allTeamOkrViews, EmployeeResult allEmployee, DateTime startDate, DateTime endDate, long cycleDurationId, string symbol, int year, int cycleId, string token, List<FeedbackResponse> allFeedback, long empId, long parentId, bool isTeamParent, int teamProgressId)
        {
            if (goalKeys.Count != 0)
            {
                foreach (var cont in goalKeys)
                {
                    var parentIds = new List<string>();
                    var source = new List<GoalKey>();

                    if (cont.GoalObjectiveId == 0)
                    {
                        var sourceKeyDetails = GetGoalKeyById(cont.ImportedId);

                        if (sourceKeyDetails != null)
                        {
                            source.Add(sourceKeyDetails);
                        }
                        var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == cont.TeamId).Select(x => x.EmployeeId).ToList();
                        var contributors = ContributorsCommonResponse(keyContributors, allEmployee, source, (long)cont.EmployeeId, empId, false);

                        var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == cont.TeamId).ToList();

                        if ((cont.GoalStatusId != (int)GoalStatus.Draft) || (cont.GoalStatusId == (int)GoalStatus.Public && cont.ImportedId >= 0))
                        {
                            if (isTeamParent)
                            {
                                parentIds.Add(Convert.ToString(parentId));
                            }
                            else
                            {
                                if (cont.ImportedId != 0)
                                {
                                    var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == cont.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);
                                    if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                    {
                                        parentIds.Add(parentDetails.GoalObjectiveId.ToString());
                                    }
                                    else
                                    {
                                        parentIds.Add(cont.ImportedId.ToString());
                                    }
                                }
                            }
                            var contributorsCount = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == cont.TeamId).Select(x => x.EmployeeId).Distinct().ToList().Count;
                            cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalKey, AllTeamOkrViewResponse>(cont);

                            if (contributorsList.Count > 0)
                            {
                                await TeamsOkrContributorsResponse(contributorsList, allTeamOkrViews, allEmployee, startDate, endDate, cycleDurationId, symbol, year, cycleId, token, allFeedback, empId, 0, false, teamProgressId);
                            }

                            TeamOkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributors, new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, cont.GoalKeyId, parentIds, contributorsList.Any(), cont.ImportedId != 0, cont.TeamId, symbol + "," + " " + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount, allFeedback, allEmployee, new List<GoalKey>(), 0, 0, new List<TeamSequence>(), new OkrStatusDetails(), teamProgressId, Constants.Zero);
                            if (allTeamOkrViews.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                            {
                                allTeamOkrViews.Add(objOkrViewResponseDto);
                            }


                        }
                    }

                    else if (cont.GoalObjectiveId > 0)
                    {
                        var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == cont.GoalObjectiveId && x.EmployeeId == cont.EmployeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == cont.TeamId);
                        var keys = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objective.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                        List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();
                        var parent = new List<string>();
                        var sourceDetails = new List<GoalKey>();

                        foreach (var key in keys)
                        {
                            var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(key);
                            okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();
                            cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
                            okrViewKeyResultResponse.Add(okrViewKeyResultDto);

                            if (key.ImportedId != 0)
                            {
                                var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == key.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);

                                if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                {
                                    parent.Add(parentDetails.GoalObjectiveId.ToString());
                                }
                                else
                                {
                                    parent.Add(key.ImportedId.ToString());
                                }
                            }
                            var sourceKeyDetails = GetGoalKeyById(key.ImportedId);

                            if (sourceKeyDetails != null)
                            {
                                sourceDetails.Add(sourceKeyDetails);
                            }
                        }

                        var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == cont.TeamId).Select(x => x.EmployeeId).ToList();
                        var contributorDetails = ContributorsCommonResponse(keyContributors, allEmployee, sourceDetails, (long)cont.EmployeeId, empId, false);
                        var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == cont.TeamId).ToList();

                        if (objective != null)
                        {
                            var sourceKeyDetails = GetGoalKeyById(objective.ImportedId);
                            var sourceCount = sourceKeyDetails != null ? 1 : 0;

                            var contributorsCount = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == cont.TeamId).Select(x => x.EmployeeId).Distinct().ToList().Count;
                            objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, startDate, endDate, objective.Score, cycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalObjective, AllTeamOkrViewResponse>(objective);


                            if (contributorsList.Count > 0)
                            {
                                await TeamsOkrContributorsResponse(contributorsList, allTeamOkrViews, allEmployee, startDate, endDate, cycleDurationId, symbol, year, cycleId, token, allFeedback, empId, 0, false, teamProgressId);
                            }

                            await TeamOkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, cont.GoalObjectiveId, parent, contributorsList.Any(), sourceCount != 0, cont.TeamId, symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount, allFeedback, allEmployee, new List<GoalKey>(), 0, 0, new List<TeamSequence>(), new OkrStatusDetails(), teamProgressId, Constants.Zero);
                            if (allTeamOkrViews.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                            {
                                allTeamOkrViews.Add(objOkrViewResponseDto);
                            }

                        }

                    }
                }
            }
        }

        private async Task TeamsOkrSourceResponse(List<GoalKey> goalKeys, List<AllTeamOkrViewResponse> allTeamOkrViews, EmployeeResult allEmployee, DateTime startDate, DateTime endDate, long cycleDurationId, string symbol, int year, int cycleId, string token, List<FeedbackResponse> allFeedback, long empId, int teamProgressId)
        {
            if (goalKeys.Count != 0)
            {
                foreach (var cont in goalKeys)
                {
                    if (cont.GoalObjectiveId == 0)
                    {
                        var parentIds = new List<string>();
                        var source = new List<GoalKey>();

                        if (cont.ImportedId != 0)
                        {
                            var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == cont.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted);
                            if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                            {
                                parentIds.Add(parentDetails.GoalObjectiveId.ToString());
                            }
                            else
                            {
                                parentIds.Add(cont.ImportedId.ToString());
                            }
                        }

                        var sourceDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == cont.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == cont.TeamId).ToList();

                        if ((cont.GoalStatusId != (int)GoalStatus.Draft) || (cont.GoalStatusId == (int)GoalStatus.Public && cont.ImportedId >= 0))
                        {
                            var sourceKeyDetails = GetGoalKeyById(cont.ImportedId);
                            if (sourceKeyDetails != null)
                            {
                                source.Add(sourceKeyDetails);
                            }

                            var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == cont.TeamId).Select(x => x.EmployeeId).ToList();
                            var contributors = ContributorsCommonResponse(keyContributors, allEmployee, source, (long)cont.EmployeeId, empId, false);

                            var sourceCount = sourceKeyDetails != null ? 1 : 0;

                            var contributorsCount = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.TeamId == cont.TeamId).Select(x => x.EmployeeId).Distinct().ToList().Count;
                            cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalKey, AllTeamOkrViewResponse>(cont);

                            if (sourceDetails.Count > 0)
                            {
                                await TeamsOkrSourceResponse(sourceDetails, allTeamOkrViews, allEmployee, startDate, endDate, cycleDurationId, symbol, year, cycleId, token, allFeedback, empId, teamProgressId);
                            }

                            await TeamOkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributors, new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, cont.GoalKeyId, parentIds, keyContributors.Any(), sourceCount != 0, cont.TeamId, symbol + "," + " " + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount, allFeedback, allEmployee, new List<GoalKey>(), 0, 0, new List<TeamSequence>(), new OkrStatusDetails(), teamProgressId, Constants.Zero);

                            if (allTeamOkrViews.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                            {
                                allTeamOkrViews.Add(objOkrViewResponseDto);
                            }

                        }

                    }

                    else if (cont.GoalObjectiveId > 0)
                    {
                        var parent = new List<string>();
                        var sourceIds = new List<GoalKey>();

                        List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();
                        var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == cont.EmployeeId && x.GoalObjectiveId == cont.GoalObjectiveId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == cont.TeamId);
                        if (objective != null)
                        {
                            var keys = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objective.GoalObjectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                            foreach (var key in keys)
                            {
                                var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(key);
                                okrViewKeyResultDto.KrUniqueId = Guid.NewGuid().ToString();
                                cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
                                okrViewKeyResultResponse.Add(okrViewKeyResultDto);

                                if (key.ImportedId != 0)
                                {
                                    var parentDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x =>
                                        x.GoalKeyId == key.ImportedId && x.IsActive &&
                                        x.KrStatusId == (int)KrStatus.Accepted);
                                    if (parentDetails != null && parentDetails.GoalObjectiveId != 0)
                                    {
                                        parent.Add(parentDetails.GoalObjectiveId.ToString());
                                    }
                                    else
                                    {
                                        parent.Add(cont.ImportedId.ToString());
                                    }
                                }

                                var sourceKeyDetails = GetGoalKeyById(key.ImportedId);

                                if (sourceKeyDetails != null)
                                {
                                    sourceIds.Add(sourceKeyDetails);
                                }

                            }
                            var sourceDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == cont.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

                            var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == cont.TeamId).Select(x => x.EmployeeId).ToList();
                            var contributorDetails = ContributorsCommonResponse(keyContributors, allEmployee, sourceIds, (long)cont.EmployeeId, empId, false);

                            var contributorsCount = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived && x.TeamId == cont.TeamId).Select(x => x.EmployeeId).Distinct().ToList().Count;

                            objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, startDate, endDate, objective.Score, cycleDurationId);
                            var objOkrViewResponseDto = Mapper.Map<GoalObjective, AllTeamOkrViewResponse>(objective);

                            if (sourceDetails.Count > 0)
                            {
                                await TeamsOkrSourceResponse(sourceDetails, allTeamOkrViews, allEmployee, startDate, endDate, cycleDurationId, symbol, year, cycleId, token, allFeedback, empId, teamProgressId);
                            }

                            await TeamOkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, new List<AllTeamOkrViewResponse>(), new List<AllTeamOkrViewResponse>(), token, objective.GoalObjectiveId, parent, keyContributors.Any(), cont.ImportedId != 0, cont.TeamId, symbol + "," + " " + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount, allFeedback, allEmployee, new List<GoalKey>(), 0, 0, new List<TeamSequence>(), new OkrStatusDetails(), teamProgressId, Constants.Zero);

                            if (allTeamOkrViews.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
                            {
                                allTeamOkrViews.Add(objOkrViewResponseDto);
                            }

                        }
                    }
                }
            }

        }
        public OkrViewResponse OkrViewCommonResponse(OkrViewResponse okrViewResponse, List<OkrViewKeyResults> okrViewKeyResults, List<OkrViewContributors> contributorResponse, List<OkrViewResponse> okrViewAllContributorsResponses, List<OkrViewResponse> okrViewAllSourceResponses, string token, long objectiveId, List<string> parentIds, bool isAligned, bool isContributorExist, bool isSourceExist, long teamId, string cycle, int objType, bool isSourceLink, int contributorsCount, List<FeedbackResponse> allFeedback, List<int> krStatusId, List<TeamDetails> teamDetails)
        {
            var parentTeam = commonService.ParentTeamDetails((int)GoalType.GoalObjective, okrViewResponse.ImportedId, teamDetails, okrViewResponse.TeamId);
            var teamDetailsById = teamDetails.FirstOrDefault(x => x.OrganisationId == teamId);
            okrViewResponse.OkrViewKeyResults = okrViewKeyResults;
            okrViewResponse.ObjectiveType = objType;
            okrViewResponse.TeamName = !string.IsNullOrEmpty(teamDetailsById?.OrganisationName) ? teamDetailsById?.OrganisationName : "";
            okrViewResponse.Cycle = cycle;
            okrViewResponse.ParentTeamDetails = parentTeam;

            if (krStatusId.Any(x => x == (int)KrStatus.Accepted))
            {
                okrViewResponse.IsContributorExist = isContributorExist;
                okrViewResponse.IsSourceExist = isSourceExist;
            }

            okrViewResponse.OkrViewContributors = contributorResponse;
            okrViewResponse.ContributorCount = contributorsCount;
            okrViewResponse.IsSourceLinked = isSourceLink;
            okrViewResponse.Index = Constants.Zero;
            okrViewResponse.Parent = parentIds.Distinct().ToList();
            //okrViewResponse.IsAnyFeedback = allFeedback != null && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == okrViewResponse.ObjectiveId);
            okrViewResponse.IsAligned = isAligned;
            if (teamDetailsById != null)
            {
                okrViewResponse.ColorCode = teamDetailsById?.ColorCode;
                okrViewResponse.BackGroundColorCode = teamDetailsById?.BackGroundColorCode;
            }

            //okrViewResponse.OkrViewAllContributorsResponse = okrViewAllContributorsResponses;
            //okrViewResponse.OkrViewAllSourceResponse = okrViewAllSourceResponses;

            return okrViewResponse;
        }
        private async Task<AllTeamOkrViewResponse> TeamOkrViewCommonResponse(AllTeamOkrViewResponse allTeamOkrViewResponse, List<OkrViewKeyResults> okrViewKeyResults, List<OkrViewContributors> contributorResponse, List<AllTeamOkrViewResponse> okrViewAllContributorsResponses, List<AllTeamOkrViewResponse> okrViewAllSourceResponses, string token, long objectiveId, List<string> parentIds, bool isContributorExist, bool isSourceExist, long teamId, string cycle, int objType, bool isSourceLink, int contributorsCount, List<FeedbackResponse> allFeedback, EmployeeResult employee, List<GoalKey> keyResultCount, int goalObjectiveCount, decimal avgScore, List<TeamSequence> teamSequence, OkrStatusDetails okrStatusDetails, int teamProgressId, decimal lastSevenDaysProgress)
        {
            var teamDetailsById = commonService.GetTeamEmployeeByTeamId(teamId, token);
            allTeamOkrViewResponse.OkrViewKeyResults = okrViewKeyResults;
            allTeamOkrViewResponse.ObjectiveType = objType;
            allTeamOkrViewResponse.TeamName = !string.IsNullOrEmpty(teamDetailsById?.OrganisationName) ? teamDetailsById?.OrganisationName : "";
            allTeamOkrViewResponse.Cycle = cycle;
            allTeamOkrViewResponse.IsContributorExist = isContributorExist;
            allTeamOkrViewResponse.IsSourceExist = isSourceExist;
            allTeamOkrViewResponse.OkrViewContributors = contributorResponse;
            allTeamOkrViewResponse.ContributorCount = contributorsCount;
            allTeamOkrViewResponse.IsSourceLinked = isSourceLink;
            allTeamOkrViewResponse.Index = Constants.Zero;
            allTeamOkrViewResponse.Parent = parentIds.Distinct().ToList();
            allTeamOkrViewResponse.ObjectiveUniqueId = objectiveId.ToString();
            allTeamOkrViewResponse.IsAnyFeedback = allFeedback != null && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == allTeamOkrViewResponse.ObjectiveId);
            allTeamOkrViewResponse.LeaderDetails = await GetLeaderDetails(teamId, token, employee);
            allTeamOkrViewResponse.TeamLogo = teamDetailsById == null ? "" : teamDetailsById?.ImagePath;
            allTeamOkrViewResponse.KeyResultCount = keyResultCount.Count;
            allTeamOkrViewResponse.ObjectiveCount = goalObjectiveCount;
            allTeamOkrViewResponse.MemberCount = teamDetailsById == null ? 0 : teamDetailsById.MembersCount;
            allTeamOkrViewResponse.TeamScore = KeyScore(avgScore);
            allTeamOkrViewResponse.TeamProgress = teamProgressId;
            allTeamOkrViewResponse.ProgressCode = okrStatusDetails == null ? " " : okrStatusDetails.Color;
            allTeamOkrViewResponse.Sequence = teamSequence.Where(x => x.TeamId == teamId).Select(x => x.Sequence).FirstOrDefault();
            allTeamOkrViewResponse.TeamId = teamId;
            if (teamDetailsById != null)
            {
                allTeamOkrViewResponse.ColorCode = teamDetailsById.ColorCode;
                allTeamOkrViewResponse.BackGroundColorCode = teamDetailsById.BackGroundColorCode;
            }

            allTeamOkrViewResponse.LastSevenDaysProgress = KeyScore(avgScore) - lastSevenDaysProgress <= 0 ? 0 : KeyScore(avgScore) - lastSevenDaysProgress;
            return allTeamOkrViewResponse;
        }

        //public async Task<AlignmentResponse> AlignmentResponse(long empId, int cycle, int year, long orgId, string token)
        //{
        //    CycleDetails cycleDetail = commonService.GetOrganisationCycleDetail(orgId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
        //    var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
        //    var lockDate = await commonService.IsOkrLocked(Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), empId, cycle, year, token);

        //    var alignmentResponse = new AlignmentResponse
        //    {
        //        IsLocked = lockDate.IsGaolLocked,
        //        IsScoreLocked = lockDate.IsScoreLocked
        //    };
        //    int keyCount = 0;
        //    var myGoalOkrResponses = new List<AlignmentMapResponse>();
        //    var allEmployee = commonService.GetAllUserFromUsers(token);
        //    var objectives = myGoalsService.GetEmplyeeOkrByCycleId(empId, cycle, year).OrderByDescending(x => x.GoalObjectiveId);
        //    if (objectives != null)
        //    {
        //        alignmentResponse.OkrCount = objectives.Count();

        //        foreach (var obj in objectives)
        //        {
        //            List<MyGoalKeyResponse> myGoalKeyResponses = new List<MyGoalKeyResponse>();

        //            var keyDetail = myGoalsService.GetGoalKey(obj.GoalObjectiveId);
        //            foreach (var key in keyDetail)
        //            {
        //                keyCount += 1;
        //                var keyTime = (int)DateTime.UtcNow.Subtract(key.CreatedOn).TotalMinutes;
        //                myGoalKeyResponses.Add(new MyGoalKeyResponse
        //                {
        //                    GoalKeyId = key.GoalKeyId,
        //                    IsNewItem = keyTime <= Constants.NewItemTime,
        //                    DueDate = key.DueDate,
        //                    Score = key.Score,
        //                    Source = key.Source,
        //                    ImportedType = key.ImportedType,
        //                    ImportedId = key.ImportedId,
        //                    KeyDescription = key.KeyDescription,
        //                    Contributors = commonService.GetContributor((int)GoalType.GoalKey, key.GoalKeyId, allEmployee.Results)
        //                });
        //            }

        //            var createdTime = (int)DateTime.UtcNow.Subtract(obj.CreatedOn).TotalMinutes;
        //            var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);
        //            myGoalOkrResponses.Add(new AlignmentMapResponse
        //            {
        //                MyGoalsDetails = myGoalKeyResponses,
        //                GoalObjectiveId = obj.GoalObjectiveId,
        //                Year = obj.Year,
        //                IsPrivate = obj.IsPrivate,
        //                ObjectiveDescription = obj.ObjectiveDescription,
        //                EmployeeId = obj.EmployeeId,
        //                FirstName = objUser == null ? "N" : objUser.FirstName,
        //                LastName = objUser == null ? "A" : objUser.LastName,
        //                ImagePath = objUser?.ImagePath?.Trim(),
        //                ObjectiveName = obj.ObjectiveName,
        //                StartDate = obj.StartDate,
        //                EndDate = obj.EndDate,
        //                Source = obj.Source,
        //                DueCycle = quarterDetails == null ? Constants.DueCycleQ3 : quarterDetails.Symbol + "-" + year,
        //                Score = obj.Score,
        //                DueDate = obj.EndDate,
        //                IsNewItem = createdTime <= Constants.NewItemTime,
        //                ImportedId = obj.ImportedId,
        //                ImportedType = obj.ImportedType,
        //                Contributors = commonService.GetContributor((int)GoalType.GoalObjective, obj.GoalObjectiveId, allEmployee.Results),
        //                IsAssigned = myGoalsService.GetAssignedObjectiveByGoalObjectiveId(obj.GoalObjectiveId, cycle, year).ToList().Count > 0,
        //                ObjectiveCycleId = obj.ObjectiveCycleId
        //            });
        //        }
        //        alignmentResponse.KeyCount = keyCount;
        //        alignmentResponse.MyGoalOkrResponses = myGoalOkrResponses;
        //    }

        //    return alignmentResponse;
        //}

        //public List<AlignmentMapResponse> AssociateContributorResponse(long objId, int cycle, int year, bool isAligned, long orgId, string token)
        //{
        //    CycleDetails cycleDetail = commonService.GetOrganisationCycleDetail(orgId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
        //    var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);

        //    var myGoalOkrResponses = new List<AlignmentMapResponse>();
        //    var allEmployee = commonService.GetAllUserFromUsers(token);

        //    List<GoalObjective> objectives = myGoalsService.GetAssignedAlignedObjective(objId, cycle, year, isAligned).OrderByDescending(x => x.GoalObjectiveId).ToList();

        //    if (objectives != null)
        //    {
        //        foreach (var obj in objectives)
        //        {
        //            List<MyGoalKeyResponse> myGoalKeyResponses = new List<MyGoalKeyResponse>();

        //            var keyDetail = myGoalsService.GetGoalKey(obj.GoalObjectiveId);
        //            foreach (var key in keyDetail)
        //            {
        //                var keyTime = (int)DateTime.UtcNow.Subtract(key.CreatedOn).TotalMinutes;
        //                myGoalKeyResponses.Add(new MyGoalKeyResponse
        //                {
        //                    GoalKeyId = key.GoalKeyId,
        //                    IsNewItem = keyTime <= Constants.NewItemTime,
        //                    DueDate = key.DueDate,
        //                    Score = key.Score,
        //                    Source = key.Source,
        //                    ImportedType = key.ImportedType,
        //                    ImportedId = key.ImportedId,
        //                    KeyDescription = key.KeyDescription,
        //                    Contributors = commonService.GetContributor((int)GoalType.GoalKey, key.GoalKeyId, allEmployee.Results)
        //                });
        //            }

        //            var createdTime = (int)DateTime.UtcNow.Subtract(obj.CreatedOn).TotalMinutes;
        //            var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);
        //            myGoalOkrResponses.Add(new AlignmentMapResponse
        //            {
        //                MyGoalsDetails = myGoalKeyResponses,
        //                GoalObjectiveId = obj.GoalObjectiveId,
        //                Year = obj.Year,
        //                IsPrivate = obj.IsPrivate,
        //                ObjectiveDescription = obj.ObjectiveDescription,
        //                EmployeeId = obj.EmployeeId,
        //                FirstName = objUser == null ? "N" : objUser.FirstName,
        //                LastName = objUser == null ? "A" : objUser.LastName,
        //                ImagePath = objUser?.ImagePath?.Trim(),
        //                ObjectiveName = obj.ObjectiveName,
        //                StartDate = obj.StartDate,
        //                EndDate = obj.EndDate,
        //                DueCycle = quarterDetails == null ? Constants.DueCycleQ3 : quarterDetails.Symbol + "-" + year,
        //                Score = obj.Score,
        //                DueDate = obj.EndDate,
        //                IsNewItem = createdTime <= Constants.NewItemTime,
        //                ImportedId = obj.ImportedId,
        //                ImportedType = obj.ImportedType,
        //                Source = obj.Source,
        //                Contributors = commonService.GetContributor((int)GoalType.GoalObjective, obj.GoalObjectiveId, allEmployee.Results),
        //                IsAssigned = myGoalsService.GetAssignedObjectiveByGoalObjectiveId(obj.GoalObjectiveId, cycle, year).ToList().Count > 0
        //            });
        //        }
        //    }

        //    return myGoalOkrResponses;
        //}

        //public List<AlignmentMapResponse> GetAllParentAlignment(AlignParentRequest alignParentRequest)
        //{
        //    var myGoalOkrResponses = alignParentRequest.MyGoalOkrResponses;
        //    long objId = alignParentRequest.ObjId; int cycle = alignParentRequest.Cycle; int year = alignParentRequest.Year;
        //    bool isAligned = alignParentRequest.IsAligned; long orgId = alignParentRequest.OrgId; string token = alignParentRequest.Token;
        //    long importId = 0;

        //    if (alignParentRequest.CycleDetail.QuarterDetails == null)
        //    {
        //        alignParentRequest.CycleDetail = commonService.GetOrganisationCycleDetail(orgId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
        //    }

        //    if (alignParentRequest.AllEmployee.Results == null)
        //    {
        //        alignParentRequest.AllEmployee = commonService.GetAllUserFromUsers(token);
        //    }

        //    var quarterDetails = alignParentRequest.CycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);

        //    List<GoalObjective> objectives = myGoalsService.GetAssignedAlignedObjective(objId, cycle, year, isAligned).OrderByDescending(x => x.GoalObjectiveId).ToList();

        //    foreach (var obj in objectives)
        //    {
        //        if (isAligned)
        //        {
        //            importId = obj.ImportedId;
        //        }

        //        List<MyGoalKeyResponse> myGoalKeyResponses = SeparationGoalKeyResponse_GetAllParentAlignment(obj.GoalObjectiveId, alignParentRequest.AllEmployee.Results);

        //        var createdTime = (int)DateTime.UtcNow.Subtract(obj.CreatedOn).TotalMinutes;
        //        var objUser = alignParentRequest.AllEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);
        //        myGoalOkrResponses.Add(new AlignmentMapResponse
        //        {
        //            MyGoalsDetails = myGoalKeyResponses,
        //            GoalObjectiveId = obj.GoalObjectiveId,
        //            Year = obj.Year,
        //            IsPrivate = obj.IsPrivate,
        //            ObjectiveDescription = obj.ObjectiveDescription,
        //            EmployeeId = obj.EmployeeId,
        //            FirstName = objUser == null ? "N" : objUser.FirstName,
        //            LastName = objUser == null ? "A" : objUser.LastName,
        //            ImagePath = objUser?.ImagePath?.Trim(),
        //            ObjectiveName = obj.ObjectiveName,
        //            StartDate = obj.StartDate,
        //            Source = obj.Source,
        //            EndDate = obj.EndDate,
        //            DueCycle = quarterDetails == null ? Constants.DueCycleQ3 : quarterDetails.Symbol + "-" + year,
        //            Score = obj.Score,
        //            DueDate = obj.EndDate,
        //            IsNewItem = createdTime <= Constants.NewItemTime,
        //            ImportedId = obj.ImportedId,
        //            ImportedType = obj.ImportedType,
        //            Contributors = commonService.GetContributor((int)GoalType.GoalObjective, obj.GoalObjectiveId, alignParentRequest.AllEmployee.Results),
        //            IsAssigned = myGoalsService.GetAssignedObjectiveByGoalObjectiveId(obj.GoalObjectiveId, cycle, year).ToList().Count > 0
        //        });
        //    }

        //    if (isAligned && importId > 0)
        //    {
        //        alignParentRequest.ObjId = importId;
        //        alignParentRequest.MyGoalOkrResponses = myGoalOkrResponses;
        //        GetAllParentAlignment(alignParentRequest);
        //    }

        //    return alignParentRequest.MyGoalOkrResponses;
        //}

        //public async Task<AlignmentResponse> AlignmentMapResponse(long empId, int cycle, int year, long orgId, string token)
        //{
        //    CycleDetails cycleDetail = commonService.GetOrganisationCycleDetail(orgId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
        //    var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
        //    var lockDate = await commonService.IsOkrLocked(Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), empId, cycle, year, token);
        //    UnLockRequest unLockRequest = new UnLockRequest() { EmployeeId = empId, Cycle = cycle, Year = year };
        //    var alignmentResponse = new AlignmentResponse
        //    {
        //        IsLocked = lockDate.IsGaolLocked,
        //        IsScoreLocked = lockDate.IsScoreLocked,
        //        IsUnLockRequested = await myGoalsService.IsAlreadyRequestedAsync(unLockRequest)
        //    };
        //    int keyCount = 0;
        //    var myGoalOkrResponses = new List<AlignmentMapResponse>();
        //    var allEmployee = commonService.GetAllUserFromUsers(token);
        //    var allFeedback = await commonService.GetAllFeedback(token);
        //    var goalMap = await GetAlignmentMapResponse(cycle, year, empId);
        //    if (goalMap != null)
        //    {
        //        alignmentResponse.OkrCount = goalMap.Count(x => x.AlignLevel == 0);

        //        foreach (var goalObj in goalMap)
        //        {
        //            var obj = myGoalsService.GetGoalObjective(goalObj.GoalObjId);

        //            List<MyGoalKeyResponse> myGoalKeyResponses = SeparationGoalKeyResponse_AlignmentMapResponse(obj, allEmployee.Results, allFeedback, goalObj.AlignLevel, ref keyCount);

        //            var createdTime = (int)DateTime.UtcNow.Subtract(obj.CreatedOn).TotalMinutes;
        //            var objUser = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == obj.EmployeeId);
        //            myGoalOkrResponses.Add(new AlignmentMapResponse
        //            {
        //                MyGoalsDetails = myGoalKeyResponses,
        //                GoalObjectiveId = obj.GoalObjectiveId,
        //                Year = obj.Year,
        //                IsPrivate = obj.IsPrivate,
        //                ObjectiveDescription = obj.ObjectiveDescription,
        //                EmployeeId = obj.EmployeeId,
        //                FirstName = objUser == null ? "N" : objUser.FirstName,
        //                LastName = objUser == null ? "A" : objUser.LastName,
        //                ImagePath = objUser?.ImagePath?.Trim(),
        //                ObjectiveName = obj.ObjectiveName,
        //                StartDate = obj.StartDate,
        //                EndDate = obj.EndDate,
        //                Source = obj.Source,
        //                DueCycle = quarterDetails == null ? Constants.DueCycleQ3 : quarterDetails.Symbol + "-" + year,
        //                Score = obj.Score,
        //                DueDate = obj.EndDate,
        //                IsNewItem = createdTime <= Constants.NewItemTime,
        //                ImportedId = obj.ImportedId,
        //                ImportedType = obj.ImportedType,
        //                Contributors = commonService.GetContributor((int)GoalType.GoalObjective, obj.GoalObjectiveId, allEmployee.Results),
        //                IsAssigned = myGoalsService.GetAssignedObjectiveByGoalObjectiveId(obj.GoalObjectiveId, cycle, year).ToList().Count > 0,
        //                ObjectiveCycleId = obj.ObjectiveCycleId,
        //                AlignLevel = goalObj.AlignLevel,
        //                IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.ObjFeedbackOnTypeId && x.FeedbackOnId == obj.GoalObjectiveId)
        //            });
        //        }
        //        alignmentResponse.KeyCount = keyCount;
        //        alignmentResponse.MyGoalOkrResponses = myGoalOkrResponses;
        //    }

        //    return alignmentResponse;
        //}

        //public async Task<List<GoalMapAlignment>> GetAlignmentMapResponse(int cycleId, long year, long employeeId)
        //{
        //    var contributorsList = new List<GoalMapAlignment>();

        //    using (var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand())
        //    {
        //        command.CommandText = "EXEC sp_GetGoalMapAlignment " + cycleId + "," + employeeId + "," + year;
        //        command.CommandType = CommandType.Text;
        //        OkrServiceDBContext.Database.OpenConnection();
        //        var dataReader = await command.ExecuteReaderAsync();

        //        while (dataReader.Read())
        //        {
        //            var item = new GoalMapAlignment
        //            {
        //                GoalObjId = Convert.ToInt64(dataReader["GoalObjId"]),
        //                AlignLevel = Convert.ToInt32(dataReader["AlignLevel"])
        //            };
        //            contributorsList.Add(item);
        //        }
        //        OkrServiceDBContext.Database.CloseConnection();
        //    }

        //    return contributorsList;
        //}

        //#region Graph Methods
        //private static readonly string Host = "gremlindbeval.gremlin.cosmos.azure.com";
        //private static readonly string PrimaryKey = "ljrytvE21zDaN1iVeWOG06gHGqbZKtNgFaiUMHy4Ox1Zlux9eowSE6m9mAbbcuMAXPaE0pSzNrJqsIH5gT2OdA==";
        //private static readonly string Database = "sample-database";
        //private static readonly string Container = "sample-graph";
        //private static readonly bool EnableSSL = true;
        //private static readonly int Port = 443;

        //public GraphMyGoalResponse GetDataToCosmosDB(string emailId)
        //{
        //    var data = new GraphMyGoalResponse();

        //    try
        //    {
        //        string containerLink = "/dbs/" + Database + "/colls/" + Container;
        //        var gremlinServer = new GremlinServer(Host, Port, enableSsl: EnableSSL, username: containerLink, password: PrimaryKey);

        //        ConnectionPoolSettings connectionPoolSettings = new ConnectionPoolSettings()
        //        {
        //            MaxInProcessPerConnection = 10,
        //            PoolSize = 30,
        //            ReconnectionAttempts = 3,
        //            ReconnectionBaseDelay = TimeSpan.FromMilliseconds(500)
        //        };

        //        var webSocketConfiguration = new Action<ClientWebSocketOptions>(options => { options.KeepAliveInterval = TimeSpan.FromSeconds(10); });

        //        data = GetGraphRecrd(gremlinServer, connectionPoolSettings, webSocketConfiguration, emailId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    return data;
        //}

        //private static Task<ResultSet<dynamic>> SubmitRequest(GremlinClient gremlinClient, string value)
        //{
        //    try
        //    {
        //        return gremlinClient.SubmitAsync<dynamic>(value);
        //    }
        //    catch (ResponseException e)
        //    {
        //        Console.WriteLine("\tRequest Error!");

        //        // Print the Gremlin status code.
        //        Console.WriteLine($"\tStatusCode: {e.StatusCode}");

        //        // On error, ResponseException.StatusAttributes will include the common StatusAttributes for successful requests, as well as
        //        // additional attributes for retry handling and diagnostics.
        //        // These include:
        //        //  x-ms-retry-after-ms         : The number of milliseconds to wait to retry the operation after an initial operation was throttled. This will be populated when
        //        //                              : attribute 'x-ms-status-code' returns 429.
        //        //  x-ms-activity-id            : Represents a unique identifier for the operation. Commonly used for troubleshooting purposes.
        //        PrintStatusAttributes(e.StatusAttributes);
        //        Console.WriteLine($"\t[\"x-ms-retry-after-ms\"] : { GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms")}");
        //        Console.WriteLine($"\t[\"x-ms-activity-id\"] : { GetValueAsString(e.StatusAttributes, "x-ms-activity-id")}");

        //        throw;
        //    }
        //    catch (Exception CE)
        //    {
        //        throw;
        //    }
        //}

        //private static void PrintStatusAttributes(IReadOnlyDictionary<string, object> attributes)
        //{
        //    // This includes the following:
        //    //  x-ms-status-code            : This is the sub-status code which is specific to Cosmos DB.
        //    //  x-ms-total-request-charge   : The total request units charged for processing a request.
        //    //  x-ms-total-server-time-ms   : The total time executing processing the request on the server.

        //    Console.WriteLine($"\tStatusAttributes:");
        //    Console.WriteLine($"\t[\"x-ms-status-code\"] : { GetValueAsString(attributes, "x-ms-status-code")}");
        //    Console.WriteLine($"\t[\"x-ms-total-server-time-ms\"] : { GetValueAsString(attributes, "x-ms-total-server-time-ms")}");
        //    Console.WriteLine($"\t[\"x-ms-total-request-charge\"] : { GetValueAsString(attributes, "x-ms-total-request-charge")}");
        //}

        //public static string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        //{
        //    return JsonConvert.SerializeObject(GetValueOrDefault(dictionary, key));
        //}

        //public static object GetValueOrDefault(IReadOnlyDictionary<string, object> dictionary, string key)
        //{
        //    if (dictionary.ContainsKey(key))
        //    {
        //        return dictionary[key];
        //    }

        //    return null;
        //}

        //public GraphMyGoalResponse GetGraphRecrd(GremlinServer gremlinServer, ConnectionPoolSettings connectionPoolSettings, Action<ClientWebSocketOptions> webSocketConfiguration, string emailId)
        //{
        //    GraphMyGoalResponse graphMyGoalResponse = new GraphMyGoalResponse();
        //    try
        //    {
        //        using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType, connectionPoolSettings, webSocketConfiguration))
        //        {
        //            var alignLevel = 0;
        //            var logInUserModel = GetUserDetail(gremlinClient, emailId);
        //            var logedUserObj = "g.V().hasLabel('User').has('EmailId','" + emailId + "').outE('owner_of')";

        //            List<GraphOkrResponse> graphOkrResponses = new List<GraphOkrResponse>();
        //            // Create async task to execute the Gremlin query.
        //            var objResultSet = SubmitRequest(gremlinClient, logedUserObj).Result;
        //            if (objResultSet.Count > 0)
        //            {
        //                foreach (var objResult in objResultSet)
        //                {
        //                    Element element = JsonConvert.DeserializeObject<Element>(JsonConvert.SerializeObject(objResult));

        //                    if (element.inVLabel == "Obj" && element.outVLabel == "User")
        //                    {
        //                        var keyResponse = GetKeyResult(gremlinClient, element.inV);
        //                        var objectiveResponse = GetObjective(gremlinClient, element.inV, logInUserModel, keyResponse, alignLevel);
        //                        graphOkrResponses.Add(objectiveResponse);
        //                    }
        //                }

        //                GetAllLevelAssignObjective(gremlinClient, graphOkrResponses, alignLevel);

        //                GetAllLevelAlignObjective(gremlinClient, graphOkrResponses, alignLevel);

        //                graphMyGoalResponse.MyGoalOkrResponses = graphOkrResponses;
        //            }

        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    return graphMyGoalResponse;
        //}

        //#endregion


        //private List<MyGoalKeyResponse> SeparationGoalKeyResponse_GetAllParentAlignment(long goalId, List<UserResponse> allEmployee)
        //{
        //    List<MyGoalKeyResponse> myGoalKeyResponses = new List<MyGoalKeyResponse>();
        //    var keyDetail = myGoalsService.GetGoalKey(goalId);
        //    foreach (var key in keyDetail)
        //    {
        //        var keyTime = (int)DateTime.UtcNow.Subtract(key.CreatedOn).TotalMinutes;
        //        myGoalKeyResponses.Add(new MyGoalKeyResponse
        //        {
        //            GoalKeyId = key.GoalKeyId,
        //            IsNewItem = keyTime <= Constants.NewItemTime,
        //            DueDate = key.DueDate,
        //            Score = key.Score,
        //            Source = key.Source,
        //            ImportedType = key.ImportedType,
        //            ImportedId = key.ImportedId,
        //            KeyDescription = key.KeyDescription,
        //            Contributors = commonService.GetContributor((int)GoalType.GoalKey, key.GoalKeyId, allEmployee)
        //        });
        //    }

        //    return myGoalKeyResponses;
        //}

        //private List<MyGoalKeyResponse> SeparationGoalKeyResponse_AlignmentMapResponse(GoalObjective obj, List<UserResponse> allEmployee, List<FeedbackResponse> allFeedback, int alignLevel, ref int keyCount)
        //{
        //    List<MyGoalKeyResponse> myGoalKeyResponses = new List<MyGoalKeyResponse>();
        //    var keyDetail = myGoalsService.GetGoalKey(obj.GoalObjectiveId);
        //    foreach (var key in keyDetail)
        //    {
        //        if (alignLevel == 0)
        //            keyCount += 1;

        //        var keyTime = (int)DateTime.UtcNow.Subtract(key.CreatedOn).TotalMinutes;
        //        myGoalKeyResponses.Add(new MyGoalKeyResponse
        //        {
        //            GoalKeyId = key.GoalKeyId,
        //            IsNewItem = keyTime <= Constants.NewItemTime,
        //            DueDate = key.DueDate,
        //            Score = key.Score,
        //            Source = key.Source,
        //            ImportedType = key.ImportedType,
        //            ImportedId = key.ImportedId,
        //            KeyDescription = key.KeyDescription,
        //            IsAnyFeedback = (allFeedback != null) && allFeedback.Any(x => x.FeedbackOnTypeId == Constants.KeyFeedbackOnTypeId && x.FeedbackOnId == key.GoalKeyId),
        //            Contributors = commonService.GetContributor((int)GoalType.GoalKey, key.GoalKeyId, allEmployee)
        //        });
        //    }

        //    return myGoalKeyResponses;
        //}

        //private GraphContributorsResponse GetUserDetailById(GremlinClient gremlinClient, string userId)
        //{
        //    var userResponse = new GraphContributorsResponse();
        //    try
        //    {
        //        var logedUser = "g.V().hasLabel('User').has('id','" + userId + "')";
        //        var userResultSet = SubmitRequest(gremlinClient, logedUser).Result;
        //        if (userResultSet.Count > 0)
        //        {
        //            var userModel = JsonConvert.DeserializeObject<List<UserModel>>(JsonConvert.SerializeObject(userResultSet).ToString());
        //            GraphContributorsResponse userDetail = new GraphContributorsResponse
        //            {
        //                EmployeeId = userModel[0].id,
        //                FirstName = userModel[0].properties.FirstName[0].value,
        //                LastName = userModel[0].properties.LastName[0].value,
        //                Designation = userModel[0].properties.Designation[0].value,
        //                ImagePath = userModel[0].properties.ImagePath[0].value
        //            };
        //            userResponse = userDetail;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        throw;
        //    }

        //    return userResponse;
        //}

        //private GraphContributorsResponse GetUserDetail(GremlinClient gremlinClient, string emailId)
        //{
        //    var userResponse = new GraphContributorsResponse();
        //    try
        //    {
        //        var logedUser = "g.V().hasLabel('User').has('EmailId','" + emailId + "')";
        //        var userResultSet = SubmitRequest(gremlinClient, logedUser).Result;
        //        if (userResultSet.Count > 0)
        //        {
        //            var userModel = JsonConvert.DeserializeObject<List<UserModel>>(JsonConvert.SerializeObject(userResultSet).ToString());
        //            GraphContributorsResponse userDetail = new GraphContributorsResponse
        //            {
        //                EmployeeId = userModel[0].id,
        //                FirstName = userModel[0].properties.FirstName[0].value,
        //                LastName = userModel[0].properties.LastName[0].value,
        //                Designation = userModel[0].properties.Designation[0].value,
        //                ImagePath = userModel[0].properties.ImagePath[0].value
        //            };
        //            userResponse = userDetail;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        throw;
        //    }

        //    return userResponse;
        //}

        //private GraphOkrResponse GetObjective(GremlinClient gremlinClient, string objId, GraphContributorsResponse userDetail, List<GraphGoalKeyResponse> graphGoalKeyResponses, int alignLevel)
        //{
        //    GraphOkrResponse graphObjResponse = new GraphOkrResponse();
        //    try
        //    {
        //        string getObjQuery = "g.V().hasLabel('Obj').has('id','" + objId + "')";
        //        var objDetailResultSet = SubmitRequest(gremlinClient, getObjQuery).Result;
        //        if (objDetailResultSet.Count > 0)
        //        {
        //            var objectiveDetail = JsonConvert.DeserializeObject<List<ObjModel>>(JsonConvert.SerializeObject(objDetailResultSet).ToString());
        //            graphObjResponse = new GraphOkrResponse
        //            {
        //                MyGoalsDetails = graphGoalKeyResponses,
        //                GoalObjectiveId = objectiveDetail[0].id,
        //                Year = Convert.ToInt32(objectiveDetail[0].properties.Year[0]?.value),
        //                IsPrivate = Convert.ToBoolean(objectiveDetail[0].properties.IsPrivate[0]?.value),
        //                ObjectiveDescription = objectiveDetail[0].properties.ObjectiveDescription[0]?.value,
        //                EmployeeId = userDetail.EmployeeId,
        //                FirstName = userDetail.FirstName,
        //                LastName = userDetail.LastName,
        //                ImagePath = userDetail.ImagePath,
        //                ObjectiveName = objectiveDetail[0].properties.ObjectiveName[0].value,
        //                StartDate = Convert.ToDateTime(objectiveDetail[0].properties.StartDate[0]?.value),
        //                EndDate = Convert.ToDateTime(objectiveDetail[0].properties.EndDate[0]?.value),
        //                Score = Convert.ToDecimal(objectiveDetail[0].properties.Score[0]?.value),
        //                DueDate = Convert.ToDateTime(objectiveDetail[0].properties.EndDate[0]?.value),
        //                AlignLevel = alignLevel,
        //                Contributors = GetObjContributors(graphGoalKeyResponses)
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        throw;
        //    }
        //    return graphObjResponse;
        //}

        //private List<GraphGoalKeyResponse> GetKeyResult(GremlinClient gremlinClient, string objId)
        //{
        //    List<GraphGoalKeyResponse> krResponse = new List<GraphGoalKeyResponse>();
        //    try
        //    {
        //        string getKrQuery = "g.V().hasLabel('Obj').has('id','" + objId + "').outE('belongs_to')";
        //        var allKR = SubmitRequest(gremlinClient, getKrQuery).Result;

        //        if (allKR.Count > 0)
        //        {
        //            var allKrDetail = JsonConvert.DeserializeObject<List<Element>>(JsonConvert.SerializeObject(allKR));

        //            foreach (var item in allKrDetail)
        //            {
        //                string getkrQuery = "g.V().hasLabel('" + item.inVLabel + "').has('id','" + item.inV + "')";
        //                var krDetailResultSet = SubmitRequest(gremlinClient, getkrQuery).Result;
        //                if (krDetailResultSet.Count > 0)
        //                {
        //                    var getKrDetail = JsonConvert.DeserializeObject<List<KRModel>>(JsonConvert.SerializeObject(krDetailResultSet).ToString());

        //                    krResponse.Add(new GraphGoalKeyResponse
        //                    {
        //                        GoalKeyId = getKrDetail[0].id,
        //                        DueDate = Convert.ToDateTime(getKrDetail[0].properties.DueDate[0].value),
        //                        Score = Convert.ToDecimal(getKrDetail[0].properties.Score[0].value),
        //                        KeyDescription = getKrDetail[0].properties.KRName[0].value,
        //                        Contributors = GetContributors(gremlinClient, getKrDetail[0].id)
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    return krResponse;
        //}

        //private List<GraphContributorsResponse> GetContributors(GremlinClient gremlinClient, string krId)
        //{
        //    List<GraphContributorsResponse> contributorsResponses = new List<GraphContributorsResponse>();
        //    try
        //    {
        //        string assignedKRQuery = "g.V().hasLabel('KR').has('id','" + krId + "').InE('assigned_to')";
        //        var assignedKRResultSet = SubmitRequest(gremlinClient, assignedKRQuery).Result;

        //        if (assignedKRResultSet.Count > 0)
        //        {
        //            var allAssignedKR = JsonConvert.DeserializeObject<List<Element>>(JsonConvert.SerializeObject(assignedKRResultSet));

        //            foreach (var item in allAssignedKR)
        //            {
        //                string assignedKrObjQuery = "g.V().hasLabel('" + item.outVLabel + "').has('id','" + item.outV + "').inE('belongs_to')";
        //                var assignedKrObjResultSet = SubmitRequest(gremlinClient, assignedKrObjQuery).Result;
        //                if (assignedKrObjResultSet.Count > 0)
        //                {
        //                    var allAssignedKRObj = JsonConvert.DeserializeObject<List<Element>>(JsonConvert.SerializeObject(assignedKrObjResultSet));

        //                    foreach (var innerItem in allAssignedKRObj)
        //                    {
        //                        string assignedKrObjUserQuery = "g.V().hasLabel('" + innerItem.outVLabel + "').has('id','" + innerItem.outV + "').inE('owner_of')";
        //                        var assignedKrObjUserResultSet = SubmitRequest(gremlinClient, assignedKrObjUserQuery).Result;
        //                        if (assignedKrObjUserResultSet.Count > 0)
        //                        {
        //                            var allAssignedKRObjUser = JsonConvert.DeserializeObject<List<Element>>(JsonConvert.SerializeObject(assignedKrObjUserResultSet));
        //                            foreach (var userItem in allAssignedKRObjUser)
        //                            {
        //                                contributorsResponses.Add(GetUserDetailById(gremlinClient, userItem.outV));
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }

        //    return contributorsResponses;
        //}

        //private List<GraphContributorsResponse> GetObjContributors(List<GraphGoalKeyResponse> goalKeyResponses)
        //{
        //    List<GraphContributorsResponse> objContributors = new List<GraphContributorsResponse>();
        //    try
        //    {
        //        foreach (var item in goalKeyResponses)
        //        {
        //            foreach (var contributor in item.Contributors)
        //            {
        //                if (!objContributors.Any(x => x.EmployeeId == contributor.EmployeeId))
        //                {
        //                    objContributors.Add(contributor);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    return objContributors;
        //}

        //private List<GraphOkrResponse> GetAlignedObjective(GremlinClient gremlinClient, GraphOkrResponse obj, int alignLevel)
        //{
        //    List<GraphOkrResponse> graphOkrResponses = new List<GraphOkrResponse>();
        //    try
        //    {
        //        var graphGoalKeyResponses = obj.MyGoalsDetails;
        //        var allKRId = from o in graphGoalKeyResponses select o.GoalKeyId;
        //        var allKRIds = allKRId.Distinct().ToList();

        //        var allEmployeeId = from o in graphGoalKeyResponses from l in o.Contributors select l.EmployeeId;
        //        var allEmployeeIds = allEmployeeId.Distinct().ToList();

        //        foreach (var empId in allEmployeeIds)
        //        {
        //            var logInUserModel = GetUserDetailById(gremlinClient, empId);
        //            var currentUserObj = "g.V().hasLabel('User').has('id','" + empId + "').outE('owner_of')";

        //            // Create async task to execute the Gremlin query.
        //            var objResultSet = SubmitRequest(gremlinClient, currentUserObj).Result;
        //            if (objResultSet.Count > 0)
        //            {
        //                foreach (var objResult in objResultSet)
        //                {
        //                    Element element = JsonConvert.DeserializeObject<Element>(JsonConvert.SerializeObject(objResult));

        //                    if (element.inVLabel == "Obj" && element.outVLabel == "User")
        //                    {
        //                        var isAllignedObj = GetAlignedKeyResult(gremlinClient, element, allKRIds);
        //                        if (isAllignedObj)
        //                        {
        //                            var keyResponse = GetKeyResult(gremlinClient, element.inV);
        //                            var objectiveResponse = GetObjective(gremlinClient, element.inV, logInUserModel, keyResponse, alignLevel);
        //                            objectiveResponse.ImportedId = obj.GoalObjectiveId;
        //                            graphOkrResponses.Add(objectiveResponse);
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    return graphOkrResponses;
        //}
        //private List<GraphOkrResponse> GetAssignedObjective(GremlinClient gremlinClient, GraphOkrResponse obj, int assignLevel)
        //{
        //    List<GraphOkrResponse> graphOkrResponses = new List<GraphOkrResponse>();
        //    try
        //    {
        //        var graphGoalKeyResponses = obj.MyGoalsDetails;
        //        var allKRId = from o in graphGoalKeyResponses select o.GoalKeyId;
        //        var allKRIds = allKRId.Distinct().ToList();
        //        var isAssigned = false;

        //        foreach (var krId in allKRIds)
        //        {
        //            var currentKRObj = "g.V().hasLabel('KR').has('id','" + krId + "').outE('assigned_to')";
        //            var krResultSet = SubmitRequest(gremlinClient, currentKRObj).Result;
        //            if (krResultSet.Count > 0)
        //            {
        //                foreach (var objResult in krResultSet)
        //                {
        //                    Element element = JsonConvert.DeserializeObject<Element>(JsonConvert.SerializeObject(objResult));

        //                    if (element.outVLabel == "KR" && element.inVLabel == "KR")
        //                    {
        //                        var assignKRObj = "g.V().hasLabel('KR').has('id','" + element.inV + "').inE('belongs_to')";

        //                        // Create async task to execute the Gremlin query.
        //                        var assignKRObjResultSet = SubmitRequest(gremlinClient, assignKRObj).Result;
        //                        if (assignKRObjResultSet.Count > 0)
        //                        {
        //                            foreach (var userObj in assignKRObjResultSet)
        //                            {
        //                                Element krElement = JsonConvert.DeserializeObject<Element>(JsonConvert.SerializeObject(userObj));
        //                                if (krElement.outVLabel == "Obj" && krElement.inVLabel == "KR")
        //                                {
        //                                    var assignKRObjUser = "g.V().hasLabel('Obj').has('id','" + krElement.outV + "').inE('owner_of')";
        //                                    var assignKRObjUserResultSet = SubmitRequest(gremlinClient, assignKRObjUser).Result;
        //                                    if (assignKRObjUserResultSet.Count > 0)
        //                                    {
        //                                        foreach (var usetItem in assignKRObjUserResultSet)
        //                                        {
        //                                            Element userElement = JsonConvert.DeserializeObject<Element>(JsonConvert.SerializeObject(usetItem));
        //                                            var logInUserModel = GetUserDetailById(gremlinClient, userElement.outV);
        //                                            var keyResponse = GetKeyResult(gremlinClient, krElement.outV);
        //                                            var objectiveResponse = GetObjective(gremlinClient, krElement.outV, logInUserModel, keyResponse, assignLevel);
        //                                            graphOkrResponses.Add(objectiveResponse);
        //                                            obj.ImportedId = krElement.outV;
        //                                            isAssigned = true;
        //                                        }

        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            if (isAssigned)
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    return graphOkrResponses;
        //}

        //private bool GetAlignedKeyResult(GremlinClient gremlinClient, Element element, List<String> allKRId)
        //{
        //    var isAllignedObj = false;
        //    try
        //    {
        //        string getKrQuery = "g.V().hasLabel('" + element.inVLabel + "').has('id','" + element.inV + "').outE('belongs_to')";
        //        var allKR = SubmitRequest(gremlinClient, getKrQuery).Result;

        //        if (allKR.Count > 0)
        //        {
        //            var allKrDetail = JsonConvert.DeserializeObject<List<Element>>(JsonConvert.SerializeObject(allKR));
        //            foreach (var item in allKrDetail)
        //            {
        //                string getKrAllignQuery = "g.V().hasLabel('" + item.inVLabel + "').has('id','" + item.inV + "').outE('assigned_to')";
        //                var allallignKR = SubmitRequest(gremlinClient, getKrAllignQuery).Result;

        //                if (allallignKR.Count > 0)
        //                {
        //                    var allallignKrDetail = JsonConvert.DeserializeObject<List<Element>>(JsonConvert.SerializeObject(allallignKR));
        //                    foreach (var alignItem in allallignKrDetail)
        //                    {
        //                        isAllignedObj = allKRId.Contains(alignItem.inV);
        //                        if (isAllignedObj)
        //                            return isAllignedObj;
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        throw;
        //    }
        //    return isAllignedObj;
        //}

        //private bool GetAssignedKeyResult(GremlinClient gremlinClient, Element element, List<String> allKRId)
        //{
        //    var isAllignedObj = false;
        //    try
        //    {
        //        string getKrQuery = "g.V().hasLabel('" + element.inVLabel + "').has('id','" + element.inV + "').outE('belongs_to')";
        //        var allKR = SubmitRequest(gremlinClient, getKrQuery).Result;

        //        if (allKR.Count > 0)
        //        {
        //            var allKrDetail = JsonConvert.DeserializeObject<List<Element>>(JsonConvert.SerializeObject(allKR));
        //            foreach (var item in allKrDetail)
        //            {
        //                string getKrAllignQuery = "g.V().hasLabel('" + item.inVLabel + "').has('id','" + item.inV + "').outE('assigned_to')";
        //                var allallignKR = SubmitRequest(gremlinClient, getKrAllignQuery).Result;

        //                if (allallignKR.Count > 0)
        //                {
        //                    var allallignKrDetail = JsonConvert.DeserializeObject<List<Element>>(JsonConvert.SerializeObject(allallignKR));
        //                    foreach (var alignItem in allallignKrDetail)
        //                    {
        //                        isAllignedObj = allKRId.Contains(alignItem.inV);
        //                        if (isAllignedObj)
        //                            return isAllignedObj;
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        throw;
        //    }
        //    return isAllignedObj;
        //}

        //private List<GraphOkrResponse> GetAllLevelAlignObjective(GremlinClient gremlinClient, List<GraphOkrResponse> graphOkrResponses, int alignLevel)
        //{
        //    var currentLevelAlignObj = graphOkrResponses.FindAll(x => x.AlignLevel == alignLevel);
        //    var nextLevelAlignObj = new List<GraphOkrResponse>();
        //    alignLevel++;
        //    foreach (var obj in currentLevelAlignObj)
        //    {
        //        var alignedObjResponse = GetAlignedObjective(gremlinClient, obj, alignLevel);
        //        nextLevelAlignObj.AddRange(alignedObjResponse);
        //    }

        //    graphOkrResponses.AddRange(nextLevelAlignObj);

        //    if (nextLevelAlignObj.Count > 0)
        //    {
        //        GetAllLevelAlignObjective(gremlinClient, graphOkrResponses, alignLevel);
        //    }

        //    return graphOkrResponses;
        //}
        //private List<GraphOkrResponse> GetAllLevelAssignObjective(GremlinClient gremlinClient, List<GraphOkrResponse> graphOkrResponses, int assignLevel)
        //{
        //    var currentLevelAlignObj = graphOkrResponses.FindAll(x => x.AlignLevel == assignLevel);
        //    var prvsLevelAlignObj = new List<GraphOkrResponse>();
        //    assignLevel--;
        //    foreach (var obj in currentLevelAlignObj)
        //    {
        //        var alignedObjResponse = GetAssignedObjective(gremlinClient, obj, assignLevel);
        //        prvsLevelAlignObj.AddRange(alignedObjResponse);
        //    }

        //    graphOkrResponses.AddRange(prvsLevelAlignObj);

        //    if (prvsLevelAlignObj.Count > 0)
        //    {
        //        GetAllLevelAssignObjective(gremlinClient, graphOkrResponses, assignLevel);
        //    }

        //    return graphOkrResponses;
        //}


        //public async Task<AllOkrViewResponse> AllOkrViewResponse(long employeeId, int cycleId, int year, string token, UserIdentity userIdentity)
        //{
        //    var allOkrViewResponse = new AllOkrViewResponse();
        //    var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(userIdentity.OrganisationId, token);
        //    var cycleDetail = cycleDurationDetails?.CycleDetails?.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
        //    var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycleId);

        //    var allEmployee = commonService.GetAllUserFromUsers(token);

        //    var okrViewResponse = new List<OkrViewResponse>();
        //    var objectives = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == employeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived).OrderByDescending(x => x.GoalObjectiveId).OrderBy(x => x.Sequence).ToListAsync();

        //    if (objectives.Count != 0)
        //    {
        //        foreach (var obj in objectives)
        //        {
        //            var okrViewKeyResults = new List<OkrViewKeyResults>();
        //            var keyResults = new List<GoalKey>();

        //            var keyDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == obj.GoalObjectiveId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined).ToListAsync();
        //            var isSourceExists = false;
        //            foreach (var key in keyDetails)
        //            {
        //                if ((key.GoalStatusId == (int)GoalStatus.Draft && key.ImportedId == 0) || (key.GoalStatusId != (int)GoalStatus.Draft && key.ImportedId >= 0))
        //                {
        //                    isSourceExists = goalKeyRepo.GetQueryable().Any(x => x.ImportedId != Constants.Zero);
        //                    var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == key.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
        //                    keyResults.AddRange(contributors);
        //                    key.Progress = commonService.GetProgressIdWithFormula(key.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), key.Score, cycleDurationDetails.CycleDurationId);

        //                    var sourceKeyDetails = GetGoalKeyById(key.ImportedId);

        //                    var sourceCount = sourceKeyDetails != null ? 1 : 0;
        //                    var isSourceLink = sourceKeyDetails != null;
        //                    var contributorsCount = contributors.Count() + sourceCount;


        //                    var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(key);
        //                    OkrViewKeyCommonResponse(okrViewKeyResultDto, isSourceLink, contributorsCount, key.TeamId, token);
        //                    okrViewKeyResults.Add(okrViewKeyResultDto);

        //                }
        //            }

        //            var contributorList = keyResults.Select(x => x.EmployeeId).Distinct().ToList();
        //            var contributorResponse = ContributorsCommonResponse(contributorList, allEmployee, Constants.Zero, employeeId);

        //            if ((keyDetails.Count > 0 && keyDetails.Any(x => x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId != (int)GoalStatus.Draft)) || (obj.GoalStatusId == (int)GoalStatus.Draft && obj.ImportedId == 0))
        //            {
        //                obj.Progress = commonService.GetProgressIdWithFormula(obj.EndDate, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), obj.Score, cycleDurationDetails.CycleDurationId);
        //                var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(obj);
        //                OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResults, contributorResponse, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyResults.Any(), isSourceExists, obj.TeamId, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalObjective, Constants.Falsemsg, Constants.Zero);
        //                if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //                {
        //                    okrViewResponse.Add(objOkrViewResponseDto);
        //                }
        //            }
        //        }
        //    }

        //    var orphanKrDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == 0 && x.CycleId == cycleId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined && x.EmployeeId == employeeId && x.GoalStatusId != (int)GoalStatus.Archived).ToListAsync();
        //    foreach (var orphanKey in orphanKrDetails)
        //    {
        //        var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == orphanKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(y => y.EmployeeId).ToList();
        //        var contributorList = ContributorsCommonResponse(contributorsList, allEmployee, orphanKey.ImportedId, employeeId);
        //        var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == orphanKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

        //        var sourceKeyDetails = GetGoalKeyById(orphanKey.ImportedId);

        //        var sourceCount = sourceKeyDetails != null ? 1 : 0;
        //        var isSourceLink = sourceKeyDetails != null;
        //        var contributorsCount = contributors.Count() + sourceCount;

        //        if ((orphanKey.GoalStatusId != (int)GoalStatus.Draft) || (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
        //        {
        //            orphanKey.Progress = commonService.GetProgressIdWithFormula(orphanKey.DueDate, (DateTime)quarterDetails?.StartDate, (DateTime)quarterDetails?.EndDate, orphanKey.Score, cycleDurationDetails.CycleDurationId);
        //            var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(orphanKey);
        //            OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributorList, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), contributorsList.Any(), orphanKey.ImportedId != 0, 0, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalKey, isSourceLink, contributorsCount);
        //            if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //            {
        //                okrViewResponse.Add(objOkrViewResponseDto);
        //            }
        //        }
        //    }
        //    allOkrViewResponse.OkrViewResponses = okrViewResponse;
        //    return allOkrViewResponse;
        //}

        //public async Task<AllOkrViewResponse> AssociateContributorsResponseAsync(long objectiveId, int objectiveType, int cycleId, int year, string token, UserIdentity userIdentity)
        //{
        //    AllOkrViewResponse allOkrResponse = new AllOkrViewResponse();

        //    var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(userIdentity.OrganisationId, token);
        //    var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
        //    var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycleId);

        //    var allEmployee = commonService.GetAllUserFromUsers(token);

        //    List<OkrViewResponse> okrViewResponse = new List<OkrViewResponse>();

        //    if (objectiveType == (int)GoalType.GoalObjective)
        //    {
        //        var keyDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToListAsync();

        //        if (keyDetails.Count != 0)
        //        {
        //            List<GoalKey> goalKeys = new List<GoalKey>();
        //            foreach (var kr in keyDetails)
        //            {
        //                var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == kr.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
        //                goalKeys.AddRange(contributors);
        //            }

        //            ///All the contributors with objective
        //            var okrs = goalKeys.Where(x => x.GoalObjectiveId > 0 && x.IsActive && x.CycleId == cycleId).ToList();
        //            ///All the contributor with orphan key
        //            var orphanKrs = goalKeys.Where(x => x.GoalObjectiveId == 0 && x.IsActive && x.CycleId == cycleId).ToList();

        //            if (okrs.Count != 0)
        //            {
        //                List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();

        //                foreach (var item in okrs)
        //                {
        //                    var isSourceExists = false;
        //                    isSourceExists = goalKeyRepo.GetQueryable().Any(x => x.ImportedId != Constants.Zero);


        //                    var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == item.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).Select(x => x.EmployeeId).ToList();
        //                    var sourceDetails = GetGoalKeyById(item.ImportedId);
        //                    var SourceLink = sourceDetails != null;
        //                    var source = sourceDetails != null ? 1 : 0;
        //                    var count = keyContributors.Count() + source;

        //                    var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(item);
        //                    item.Progress = commonService.GetProgressIdWithFormula(item.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), item.Score, cycleDurationDetails.CycleDurationId);
        //                    OkrViewKeyCommonResponse(okrViewKeyResultDto, SourceLink, count, item.TeamId, token);

        //                    okrViewKeyResultResponse.Add(okrViewKeyResultDto);


        //                    var contributorResponse = ContributorsCommonResponse(keyContributors, allEmployee, 0, (long)item.EmployeeId);

        //                    var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == item.EmployeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived);
        //                    if (objective != null)
        //                    {
        //                        var sourceKeyDetails = GetGoalKeyById(item.ImportedId);

        //                        var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                        var contributorsCount = keyContributors.Count() + sourceCount;
        //                        objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), objective.Score, cycleDurationDetails.CycleDurationId);
        //                        var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objective);
        //                        OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorResponse, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), isSourceExists, objective.TeamId, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount);
        //                        if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //                        {
        //                            okrViewResponse.Add(objOkrViewResponseDto);
        //                        }
        //                    }

        //                }

        //            }
        //            if (orphanKrs.Count != 0)
        //            {
        //                foreach (var orphanKey in orphanKrs)
        //                {

        //                    var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == orphanKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).ToList();
        //                    var contributorList = ContributorsCommonResponse(keyContributors, allEmployee, orphanKey.ImportedId, (long)orphanKey.EmployeeId);

        //                    if ((orphanKey.GoalStatusId != (int)GoalStatus.Draft) || (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
        //                    {
        //                        var sourceKeyDetails = GetGoalKeyById(orphanKey.ImportedId);

        //                        var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                        var contributorsCount = keyContributors.Count() + sourceCount;
        //                        orphanKey.Progress = commonService.GetProgressIdWithFormula(orphanKey.DueDate, (DateTime)quarterDetails?.StartDate, (DateTime)quarterDetails?.EndDate, orphanKey.Score, cycleDurationDetails.CycleDurationId);
        //                        var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(orphanKey);
        //                        OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributorList, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), orphanKey.ImportedId != 0, 0, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount);
        //                        if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //                        {
        //                            okrViewResponse.Add(objOkrViewResponseDto);
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //    }

        //    ///when type is 2 which is for orphan key
        //    else
        //    {
        //        var orphanKeys = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == objectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
        //        var orphanKeyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == objectiveId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

        //        var okrs = orphanKeyContributors.Where(x => x.GoalObjectiveId > 0 && x.IsActive && x.CycleId == cycleId).ToList();
        //        var orphanKrs = orphanKeyContributors.Where(x => x.GoalObjectiveId == 0 && x.IsActive && x.CycleId == cycleId).ToList();

        //        if (orphanKrs.Count != 0)
        //        {
        //            foreach (var orphanKey in orphanKrs)
        //            {
        //                var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == orphanKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).ToList();
        //                var contributorList = ContributorsCommonResponse(keyContributors, allEmployee, orphanKey.ImportedId, (long)orphanKey.EmployeeId);

        //                if ((orphanKey.GoalStatusId != (int)GoalStatus.Draft) || (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
        //                {
        //                    var sourceKeyDetails = GetGoalKeyById(orphanKey.ImportedId);

        //                    var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                    var contributorsCount = keyContributors.Count() + sourceCount;
        //                    orphanKey.Progress = commonService.GetProgressIdWithFormula(orphanKey.DueDate, (DateTime)quarterDetails?.StartDate, (DateTime)quarterDetails?.EndDate, orphanKey.Score, cycleDurationDetails.CycleDurationId);
        //                    var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(orphanKey);
        //                    OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributorList, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), orphanKey.ImportedId != 0, 0, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount);
        //                    if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //                    {
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }
        //                }
        //            }

        //        }
        //        if (okrs.Count != 0)
        //        {
        //            foreach (var item in okrs)
        //            {
        //                var isSourceExists = false;
        //                List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();

        //                isSourceExists = goalKeyRepo.GetQueryable().Any(x => x.ImportedId != Constants.Zero);

        //                var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == item.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).Select(x => x.EmployeeId).ToList();
        //                var sourceDetails = GetGoalKeyById(item.ImportedId);
        //                var SourceLink = sourceDetails != null;
        //                var source = sourceDetails != null ? 1 : 0;
        //                var count = keyContributors.Count() + source;

        //                var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(item);
        //                item.Progress = commonService.GetProgressIdWithFormula(item.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), item.Score, cycleDurationDetails.CycleDurationId);
        //                OkrViewKeyCommonResponse(okrViewKeyResultDto, SourceLink, count, item.TeamId, token);
        //                okrViewKeyResultResponse.Add(okrViewKeyResultDto);

        //                var contributorResponse = ContributorsCommonResponse(keyContributors, allEmployee, 0, (long)item.EmployeeId);

        //                var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == item.EmployeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived);
        //                if (objective != null)
        //                {
        //                    var sourceKeyDetails = GetGoalKeyById(objective.ImportedId);

        //                    var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                    var contributorsCount = keyContributors.Count() + sourceCount;
        //                    objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), objective.Score, cycleDurationDetails.CycleDurationId);
        //                    var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objective);
        //                    OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorResponse, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), isSourceExists, objective.TeamId, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount);
        //                    okrViewResponse.Add(objOkrViewResponseDto);
        //                }
        //            }
        //        }
        //    }

        //    allOkrResponse.OkrViewResponses = okrViewResponse;
        //    return allOkrResponse;
        //}



        //public async Task<AllOkrViewResponse> AssociateSourceResponseAsync(long objectiveId, int objectiveType, int cycleId, int year, string token, UserIdentity userIdentity)
        //{
        //    AllOkrViewResponse allOkrResponse = new AllOkrViewResponse();

        //    var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(userIdentity.OrganisationId, token);
        //    var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
        //    var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycleId);

        //    var allEmployee = commonService.GetAllUserFromUsers(token);

        //    List<OkrViewResponse> okrViewResponse = new List<OkrViewResponse>();

        //    if (objectiveType == (int)GoalType.GoalObjective)
        //    {
        //        var keyDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToListAsync();
        //        if (keyDetails.Count > 0)
        //        {
        //            List<GoalKey> goalKeys = new List<GoalKey>();
        //            foreach (var kr in keyDetails)
        //            {
        //                var sourceKeyDetails = GetGoalKeyById(kr.ImportedId);
        //                if (sourceKeyDetails != null)
        //                {
        //                    goalKeys.Add(sourceKeyDetails);
        //                }

        //            }

        //            var okrs = goalKeys.Where(x => x.GoalObjectiveId > 0 && x.IsActive && x.CycleId == cycleId).ToList();
        //            ///All the contributor with orphan key
        //            var orphanKrs = goalKeys.Where(x => x.GoalObjectiveId == 0 && x.IsActive && x.CycleId == cycleId).ToList();

        //            if (okrs.Count != 0)
        //            {
        //                List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();

        //                foreach (var item in okrs)
        //                {
        //                    var isSourceExists = false;
        //                    isSourceExists = goalKeyRepo.GetQueryable().Any(x => x.ImportedId != Constants.Zero);
        //                    var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == item.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).Select(x => x.EmployeeId).ToList();
        //                    var sourceDetails = GetGoalKeyById(item.ImportedId);
        //                    var SourceLink = sourceDetails != null;
        //                    var source = sourceDetails != null ? 1 : 0;
        //                    var count = keyContributors.Count() + source;

        //                    var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(item);
        //                    item.Progress = commonService.GetProgressIdWithFormula(item.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), item.Score, cycleDurationDetails.CycleDurationId);
        //                    OkrViewKeyCommonResponse(okrViewKeyResultDto, SourceLink, count, item.TeamId, token);
        //                    okrViewKeyResultResponse.Add(okrViewKeyResultDto);


        //                    var contributorResponse = ContributorsCommonResponse(keyContributors, allEmployee, 0, (long)item.EmployeeId);

        //                    var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == item.EmployeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived && x.GoalObjectiveId == item.GoalObjectiveId);
        //                    if (objective != null)
        //                    {

        //                        var sourceKeyDetails = GetGoalKeyById(objective.ImportedId);
        //                        var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                        var isSourceLink = sourceKeyDetails != null;

        //                        var contributorsCount = keyContributors.Count() + sourceCount;
        //                        objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), objective.Score, cycleDurationDetails.CycleDurationId);
        //                        var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objective);
        //                        OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorResponse, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), isSourceExists, objective.TeamId, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalObjective, isSourceLink, contributorsCount);
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }

        //                }

        //            }

        //            if (orphanKrs.Count != 0)
        //            {
        //                foreach (var orphanKey in orphanKrs)
        //                {

        //                    var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == orphanKey.ImportedId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).ToList();
        //                    var contributorList = ContributorsCommonResponse(keyContributors, allEmployee, orphanKey.ImportedId, (long)orphanKey.EmployeeId);

        //                    if ((orphanKey.GoalStatusId != (int)GoalStatus.Draft) || (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
        //                    {
        //                        var sourceKeyDetails = GetGoalKeyById(orphanKey.ImportedId);

        //                        var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                        var isSourceLink = sourceKeyDetails != null;

        //                        var contributorsCount = keyContributors.Count() + sourceCount;
        //                        orphanKey.Progress = commonService.GetProgressIdWithFormula(orphanKey.DueDate, (DateTime)quarterDetails?.StartDate, (DateTime)quarterDetails?.EndDate, orphanKey.Score, cycleDurationDetails.CycleDurationId);
        //                        var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(orphanKey);
        //                        OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributorList, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), orphanKey.ImportedId != 0, 0, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalKey, isSourceLink, contributorsCount);
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var orphanKeys = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == objectiveId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
        //        if (orphanKeys.Count > 0)
        //        {
        //            List<GoalKey> goalKeys = new List<GoalKey>();
        //            foreach (var kr in orphanKeys)
        //            {
        //                var sourceKeyDetails = GetGoalKeyById(kr.ImportedId);
        //                if (sourceKeyDetails != null)
        //                {
        //                    goalKeys.Add(sourceKeyDetails);
        //                }

        //            }
        //            var okrs = goalKeys.Where(x => x.GoalObjectiveId > 0 && x.IsActive && x.CycleId == cycleId).ToList();
        //            var orphanKrs = goalKeys.Where(x => x.GoalObjectiveId == 0 && x.IsActive && x.CycleId == cycleId).ToList();

        //            if (orphanKrs.Count != 0)
        //            {
        //                foreach (var orphanKey in orphanKrs)
        //                {
        //                    var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == orphanKey.ImportedId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).ToList();
        //                    var contributorList = ContributorsCommonResponse(keyContributors, allEmployee, orphanKey.ImportedId, (long)orphanKey.EmployeeId);

        //                    if ((orphanKey.GoalStatusId != (int)GoalStatus.Draft) || (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
        //                    {
        //                        var sourceKeyDetails = GetGoalKeyById(orphanKey.ImportedId);

        //                        var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                        var isSourceLink = sourceKeyDetails != null;

        //                        var contributorsCount = keyContributors.Count() + sourceCount;
        //                        orphanKey.Progress = commonService.GetProgressIdWithFormula(orphanKey.DueDate, (DateTime)quarterDetails?.StartDate, (DateTime)quarterDetails?.EndDate, orphanKey.Score, cycleDurationDetails.CycleDurationId);
        //                        var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(orphanKey);
        //                        OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributorList, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), orphanKey.ImportedId != 0, 0, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalKey, isSourceLink, contributorsCount);
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }
        //                }
        //            }

        //            if (okrs.Count != 0)
        //            {
        //                foreach (var item in okrs)
        //                {
        //                    var isSourceExists = false;
        //                    List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();

        //                    isSourceExists = goalKeyRepo.GetQueryable().Any(x => x.ImportedId != Constants.Zero);
        //                    var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == item.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).Select(x => x.EmployeeId).ToList();
        //                    var sourceDetails = GetGoalKeyById(item.ImportedId);
        //                    var SourceLink = sourceDetails != null;
        //                    var source = sourceDetails != null ? 1 : 0;
        //                    var count = keyContributors.Count() + source;

        //                    var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(item);
        //                    item.Progress = commonService.GetProgressIdWithFormula(item.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), item.Score, cycleDurationDetails.CycleDurationId);
        //                    OkrViewKeyCommonResponse(okrViewKeyResultDto, SourceLink, count, item.TeamId, token);
        //                    okrViewKeyResultResponse.Add(okrViewKeyResultDto);
        //                    var contributorResponse = ContributorsCommonResponse(keyContributors, allEmployee, 0, (long)item.EmployeeId);

        //                    var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == item.EmployeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived && x.GoalObjectiveId == item.GoalObjectiveId);
        //                    if (objective != null)
        //                    {
        //                        var sourceKeyDetails = GetGoalKeyById(objective.ImportedId);

        //                        var sourceCount = sourceKeyDetails != null ? 1 : 0;
        //                        var isSourceLink = sourceKeyDetails != null;

        //                        var contributorsCount = keyContributors.Count() + sourceCount;
        //                        objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), objective.Score, cycleDurationDetails.CycleDurationId);
        //                        var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objective);
        //                        OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorResponse, new List<OkrViewResponse>(), new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), isSourceExists, objective.TeamId, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalObjective, isSourceLink, contributorsCount);
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }
        //                }
        //            }


        //        }

        //    }

        //    allOkrResponse.OkrViewResponses = okrViewResponse;
        //    return allOkrResponse;
        //}

        //public async Task<AllOkrViewResponse> OkrViewAllLevelNestedResponseAsync(long employeeId, int cycleId, int year, string token, UserIdentity userIdentity)
        //{
        //    var allOkrViewResponse = new AllOkrViewResponse();
        //    var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(userIdentity.OrganisationId, token);
        //    var cycleDetail = cycleDurationDetails?.CycleDetails?.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
        //    var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycleId);

        //    var allEmployee = commonService.GetAllUserFromUsers(token);

        //    var okrViewResponse = new List<OkrViewResponse>();
        //    var objectives = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == employeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived).OrderByDescending(x => x.GoalObjectiveId).OrderBy(x => x.Sequence).ToListAsync();

        //    if (objectives.Count != 0)
        //    {
        //        foreach (var obj in objectives)
        //        {
        //            var okrViewKeyResults = new List<OkrViewKeyResults>();
        //            var keyResults = new List<GoalKey>();
        //            var allSourceList = new List<GoalKey>();

        //            var keyDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == obj.GoalObjectiveId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined).ToListAsync();
        //            foreach (var key in keyDetails)
        //            {
        //                if ((key.GoalStatusId == (int)GoalStatus.Draft && key.ImportedId == 0) || (key.GoalStatusId != (int)GoalStatus.Draft && key.ImportedId >= 0))
        //                {
        //                    var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == key.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();
        //                    keyResults.AddRange(contributors);
        //                    key.Progress = commonService.GetProgressIdWithFormula(key.DueDate, Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), key.Score, cycleDurationDetails.CycleDurationId);

        //                    var sourceKeyDetails = GetGoalKeyById(key.ImportedId);

        //                    if (sourceKeyDetails != null)
        //                    {
        //                        allSourceList.Add(sourceKeyDetails);
        //                    }

        //                    var sourceCount = sourceKeyDetails != null ? 1 : 0;
        //                    var isSourceLink = sourceKeyDetails != null;
        //                    var contributorsCount = contributors.Count() + sourceCount;


        //                    var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(key);
        //                    OkrViewKeyCommonResponse(okrViewKeyResultDto, isSourceLink, contributorsCount, key.TeamId, token);
        //                    okrViewKeyResults.Add(okrViewKeyResultDto);

        //                }
        //            }

        //            var contributorList = keyResults.Select(x => x.EmployeeId).Distinct().ToList();
        //            var contributorResponse = ContributorsCommonResponse(contributorList, allEmployee, Constants.Zero, employeeId);

        //            if ((keyDetails.Count > 0 && keyDetails.Any(x => x.KrStatusId != (int)KrStatus.Declined && x.GoalStatusId != (int)GoalStatus.Draft)) || (obj.GoalStatusId == (int)GoalStatus.Draft && obj.ImportedId == 0))
        //            {
        //                obj.Progress = commonService.GetProgressIdWithFormula(obj.EndDate, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), obj.Score, cycleDurationDetails.CycleDurationId);
        //                var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(obj);


        //                var allLevelContributors = ContributorsNestedResponse(keyResults, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, cycleId, token);
        //                var allLevelSource = SourceNestedResponse(allSourceList, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, cycleId, token);


        //                OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResults, contributorResponse, allLevelContributors, allLevelSource, token, Constants.Zero, new List<string>(), keyResults.Any(), allSourceList.Any(), obj.TeamId, quarterDetails?.Symbol + "," + year, (int)GoalType.GoalObjective, Constants.Falsemsg, Constants.Zero);
        //                okrViewResponse.Add(objOkrViewResponseDto);
        //            }

        //        }
        //    }

        //    var orphanKrDetails = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == 0 && x.CycleId == cycleId && x.IsActive && x.KrStatusId != (int)KrStatus.Declined && x.EmployeeId == employeeId && x.GoalStatusId != (int)GoalStatus.Archived).ToListAsync();
        //    foreach (var orphanKey in orphanKrDetails)
        //    {
        //        var sourceDetails = new List<GoalKey>();
        //        var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == orphanKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(y => y.EmployeeId).ToList();
        //        var contributorList = ContributorsCommonResponse(contributorsList, allEmployee, orphanKey.ImportedId, employeeId);
        //        var contributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == orphanKey.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

        //        var sourceKeyDetails = GetGoalKeyById(orphanKey.ImportedId);
        //        if (sourceKeyDetails != null)
        //        {
        //            sourceDetails.Add(sourceKeyDetails);
        //        }

        //        var sourceCount = sourceKeyDetails != null ? 1 : 0;
        //        var isSourceLink = sourceKeyDetails != null;

        //        var contributorsCount = contributors.Count() + sourceCount;

        //        if ((orphanKey.GoalStatusId != (int)GoalStatus.Draft) || (orphanKey.GoalStatusId == (int)GoalStatus.Public && orphanKey.ImportedId >= 0))
        //        {
        //            orphanKey.Progress = commonService.GetProgressIdWithFormula(orphanKey.DueDate, (DateTime)quarterDetails?.StartDate, (DateTime)quarterDetails?.EndDate, orphanKey.Score, cycleDurationDetails.CycleDurationId);
        //            var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(orphanKey);

        //            var allLevelContributors = ContributorsNestedResponse(contributors, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, cycleId, token);
        //            var allLevelSource = SourceNestedResponse(sourceDetails, allEmployee, Convert.ToDateTime(quarterDetails?.StartDate), Convert.ToDateTime(quarterDetails?.EndDate), cycleDurationDetails.CycleDurationId, quarterDetails?.Symbol, year, cycleId, token);


        //            OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributorList, allLevelContributors, allLevelSource, token, Constants.Zero, new List<string>(), contributorsList.Any(), orphanKey.ImportedId != 0, 0, quarterDetails?.Symbol + "," + " " + year, (int)GoalType.GoalKey, isSourceLink, contributorsCount);
        //            okrViewResponse.Add(objOkrViewResponseDto);
        //        }
        //    }
        //    allOkrViewResponse.OkrViewResponses = okrViewResponse;
        //    return allOkrViewResponse;
        //}



        //private List<OkrViewResponse> ContributorsNestedResponse(List<GoalKey> goalKeys, EmployeeResult allEmployee, DateTime startDate, DateTime endDate, long cycleDurationId, string symbol, int year, int cycleId, string token , List<FeedbackResponse> allFeedback)
        //{
        //    var okrViewResponse = new List<OkrViewResponse>();
        //    if (goalKeys.Count != 0)
        //    {
        //        foreach (var cont in goalKeys)
        //        {
        //            var allLevelContributors = new List<OkrViewResponse>();

        //            if (cont.GoalObjectiveId == 0)
        //            {
        //                var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).ToList();
        //                var contributors = ContributorsCommonResponse(keyContributors, allEmployee, cont.ImportedId, (long)cont.EmployeeId);

        //                var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

        //                if ((cont.GoalStatusId != (int)GoalStatus.Draft) || (cont.GoalStatusId == (int)GoalStatus.Public && cont.ImportedId >= 0))
        //                {
        //                    var sourceKeyDetails = GetGoalKeyById(cont.ImportedId);

        //                    var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                    var contributorsCount = keyContributors.Count() + sourceCount;
        //                    cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
        //                    var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(cont);

        //                    if (contributorsList.Count > 0)
        //                    {
        //                        allLevelContributors = ContributorsNestedResponse(contributorsList, allEmployee, startDate, endDate, cycleDurationId, symbol, year, cycleId, token, allFeedback);
        //                    }

        //                    OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributors, allLevelContributors, new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), cont.ImportedId != 0, 0, symbol + "," + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount, allFeedback);
        //                    if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //                    {
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }

        //                }

        //            }

        //            else if (cont.GoalObjectiveId > 0)
        //            {
        //                List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();

        //                var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(cont);
        //                cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
        //                okrViewKeyResultResponse.Add(okrViewKeyResultDto);

        //                var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).Select(x => x.EmployeeId).ToList();
        //                var contributorDetails = ContributorsCommonResponse(keyContributors, allEmployee, 0, (long)cont.EmployeeId);

        //                var contributorsList = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).ToList();

        //                var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == cont.EmployeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived);

        //                if (objective != null)
        //                {
        //                    var sourceKeyDetails = GetGoalKeyById(objective.ImportedId);
        //                    var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                    var contributorsCount = keyContributors.Count() + sourceCount;
        //                    objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, startDate, endDate, objective.Score, cycleDurationId);
        //                    var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objective);

        //                    if (contributorsList.Count > 0)
        //                    {
        //                        allLevelContributors = ContributorsNestedResponse(contributorsList, allEmployee, startDate, endDate, cycleDurationId, symbol, year, cycleId, token, allFeedback);
        //                    }

        //                    OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, allLevelContributors, new List<OkrViewResponse>(), token, Constants.Zero, new List<string>(), keyContributors.Any(), sourceCount != 0, 0, symbol + "," + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount, allFeedback);
        //                    if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //                    {
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }
        //                }

        //            }
        //        }
        //    }

        //    return okrViewResponse;
        //}

        //private List<OkrViewResponse> SourceNestedResponse(List<GoalKey> goalKeys, EmployeeResult allEmployee, DateTime startDate, DateTime endDate, long cycleDurationId, string symbol, int year, int cycleId, string token,List<FeedbackResponse> allFeedback)
        //{
        //    var okrViewResponse = new List<OkrViewResponse>();
        //    if (goalKeys.Count != 0)
        //    {
        //        foreach (var cont in goalKeys)
        //        {
        //            var allLevelSource = new List<OkrViewResponse>();

        //            if (cont.GoalObjectiveId == 0)
        //            {
        //                var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).Select(x => x.EmployeeId).ToList();
        //                var contributors = ContributorsCommonResponse(keyContributors, allEmployee, cont.ImportedId, (long)cont.EmployeeId);

        //                var sourceDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == cont.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

        //                if ((cont.GoalStatusId != (int)GoalStatus.Draft) || (cont.GoalStatusId == (int)GoalStatus.Public && cont.ImportedId >= 0))
        //                {
        //                    var sourceKeyDetails = GetGoalKeyById(cont.ImportedId);

        //                    var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                    var contributorsCount = keyContributors.Count() + sourceCount;
        //                    cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
        //                    var objOkrViewResponseDto = Mapper.Map<GoalKey, OkrViewResponse>(cont);

        //                    if (sourceDetails.Count > 0)
        //                    {
        //                        allLevelSource = SourceNestedResponse(sourceDetails, allEmployee, startDate, endDate, cycleDurationId, symbol, year, cycleId, token, allFeedback);
        //                    }

        //                    OkrViewCommonResponse(objOkrViewResponseDto, new List<OkrViewKeyResults>(), contributors, new List<OkrViewResponse>(), allLevelSource, token, Constants.Zero, new List<string>(), keyContributors.Any(), sourceCount != 0, 0, symbol + "," + year, (int)GoalType.GoalKey, Constants.Falsemsg, contributorsCount, allFeedback);
        //                    if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //                    {
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }

        //                }

        //            }

        //            else if (cont.GoalObjectiveId > 0)
        //            {
        //                List<OkrViewKeyResults> okrViewKeyResultResponse = new List<OkrViewKeyResults>();

        //                var okrViewKeyResultDto = Mapper.Map<GoalKey, OkrViewKeyResults>(cont);
        //                cont.Progress = commonService.GetProgressIdWithFormula(cont.DueDate, startDate, endDate, cont.Score, cycleDurationId);
        //                okrViewKeyResultResponse.Add(okrViewKeyResultDto);

        //                var keyContributors = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == cont.GoalKeyId && x.ImportedType == (int)GoalType.GoalKey && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId != (int)GoalStatus.Archived).Select(x => x.EmployeeId).ToList();
        //                var contributorDetails = ContributorsCommonResponse(keyContributors, allEmployee, 0, (long)cont.EmployeeId);

        //                var sourceDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == cont.ImportedId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted).ToList();

        //                var objective = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == cont.EmployeeId && x.IsActive && x.GoalStatusId != (int)GoalStatus.Archived);

        //                if (objective != null)
        //                {
        //                    var sourceKeyDetails = GetGoalKeyById(objective.ImportedId);
        //                    var sourceCount = sourceKeyDetails != null ? 1 : 0;

        //                    var contributorsCount = keyContributors.Count() + sourceCount;
        //                    objective.Progress = commonService.GetProgressIdWithFormula(objective.EndDate, startDate, endDate, objective.Score, cycleDurationId);
        //                    var objOkrViewResponseDto = Mapper.Map<GoalObjective, OkrViewResponse>(objective);

        //                    if (sourceDetails.Count > 0)
        //                    {
        //                        allLevelSource = SourceNestedResponse(sourceDetails, allEmployee, startDate, endDate, cycleDurationId, symbol, year, cycleId, token,allFeedback);
        //                    }

        //                    OkrViewCommonResponse(objOkrViewResponseDto, okrViewKeyResultResponse, contributorDetails, new List<OkrViewResponse>(), allLevelSource, token, Constants.Zero, new List<string>(), keyContributors.Any(), cont.ImportedId != 0, 0, symbol + "," + year, (int)GoalType.GoalObjective, Constants.Falsemsg, contributorsCount, allFeedback);
        //                    if (okrViewResponse.All(x => x.ObjectiveId != objOkrViewResponseDto.ObjectiveId))
        //                    {
        //                        okrViewResponse.Add(objOkrViewResponseDto);
        //                    }
        //                }

        //            }
        //        }
        //    }

        //    return okrViewResponse;
        //}


    }
}
