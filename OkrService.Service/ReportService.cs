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
using System.Linq;
using System.Threading.Tasks;

namespace OKRService.Service
{
    [ExcludeFromCodeCoverage]
    public class ReportService : BaseService, IReportService
    {
        private readonly IRepositoryAsync<GoalObjective> goalObjectiveRepo;
        private readonly IRepositoryAsync<GoalKey> goalKeyRepo;
        private readonly IRepositoryAsync<Constant> constantRepo;
        private readonly IRepositoryAsync<GoalKeyAudit> goalKeyAuditRepo;
        private readonly IRepositoryAsync<GoalKeyHistory> goalKeyHistoryRepo;
        private readonly IMyGoalsService myGoalsService;
        private readonly ICommonService commonService;
        private readonly IRepositoryAsync<UnlockSupportTeam> unlockSupportTeamRepo;

        public ReportService(IServicesAggregator servicesAggregateService, IMyGoalsService myGoalsServices, ICommonService commonServices) : base(servicesAggregateService)
        {
            goalObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<GoalObjective>();
            goalKeyRepo = UnitOfWorkAsync.RepositoryAsync<GoalKey>();
            constantRepo = UnitOfWorkAsync.RepositoryAsync<Constant>();
            goalKeyAuditRepo = UnitOfWorkAsync.RepositoryAsync<GoalKeyAudit>();
            goalKeyHistoryRepo = UnitOfWorkAsync.RepositoryAsync<GoalKeyHistory>();
            myGoalsService = myGoalsServices;
            commonService = commonServices;
            unlockSupportTeamRepo = UnitOfWorkAsync.RepositoryAsync<UnlockSupportTeam>();
        }

        public List<ReportContributorsWithScore> GetContributorsByObj(int goalType, long goalId, List<UserResponse> allEmployee)
        {
            var contributorsResponse = new List<ReportContributorsWithScore>();
            var contributors = GetObjContributlorsByGolaTypeAndId(goalType, goalId);

            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var key = new List<GoalKey>();
                    if (goalType == (int)GoalType.GoalObjective)
                    {
                        var objective = goalObjectiveRepo.GetQueryable().Where(x => x.ImportedId == goalId && x.EmployeeId == item.EmployeeId && x.IsActive).FirstOrDefault();
                        key = goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == objective.GoalObjectiveId && x.EmployeeId == item.EmployeeId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                    }
                    else if (goalType == (int)GoalType.GoalKey)
                    { 
                        key = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalId && x.EmployeeId == item.EmployeeId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToList();
                    }

                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

                    if (key.Count > 0 && key.Any())
                    {
                        contributorsResponse.Add(new ReportContributorsWithScore
                        {
                            EmployeeId = item.EmployeeId,
                            FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                            LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                            ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                            Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                            Score = item.Score,
                            EndDate = item.EndDate,
                            SecondLevelContributors = NLevelContributors(item.EmployeeId, goalId, goalType, allEmployee)
                        });
                    }
                }
            }

            return contributorsResponse;
        }

        public List<ReportMostLeastObjective> ReportMostLeastObjective(long empId, int cycle, int year, long orgId, string token)
        {
            CycleDetails cycleDetail = commonService.GetOrganisationCycleDetail(orgId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);

            var objectiveResponses = new List<ReportMostLeastObjective>();
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var objectives = myGoalsService.GetEmployeeOkrByCycleId(empId, cycle, year).OrderByDescending(x => x.GoalObjectiveId);
            if (objectives != null)
            {
                foreach (var obj in objectives)
                {
                    objectiveResponses.Add(new ReportMostLeastObjective
                    {
                        GoalObjectiveId = obj.GoalObjectiveId,
                        Year = obj.Year,
                        IsPrivate = obj.IsPrivate,
                        ObjectiveDescription = obj.ObjectiveDescription,
                        ObjectiveName = obj.ObjectiveName,
                        EndDate = obj.EndDate,
                        DueCycle = quarterDetails == null ? string.Empty : quarterDetails.Symbol + "-" + year,
                        Score = obj.Score,
                        UpdatedOn = obj.UpdatedOn ?? obj.CreatedOn,
                        Contributors = GetContributorsByObj((int)GoalType.GoalObjective, obj.GoalObjectiveId, allEmployee.Results)
                    });
                }
            }

            return objectiveResponses;
        }

        public async Task<TeamPerformanceResponse> TeamPerformance(long empId, int cycleId, int year, string token, UserIdentity user)
        {
            TeamPerformanceResponse teamScore = new TeamPerformanceResponse
            {
                AvgOrganisationalScore = await AvgOrganisationalScore(cycleId, year),
                MinimumOrganisationThreshold = MinimumOrganisationThreshold()
            };

            List<ContributorsKrResponse> contributorsKrResponses = new List<ContributorsKrResponse>();
            var allEmployee = commonService.GetAllUserFromUsers(token).Results;
            var getAllKr = await goalKeyRepo.GetQueryable()
                .Where(x => x.EmployeeId == empId && x.CycleId == cycleId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToArrayAsync();


            if (getAllKr != null)
            {
                foreach (var kr in getAllKr)
                {
                    var getKrContributor = await goalKeyRepo.GetQueryable()
                        .Where(x => x.ImportedId == kr.GoalKeyId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted).ToListAsync();

                    if (getKrContributor.Count > 0)
                    {
                        foreach (var item in getKrContributor)
                        {
                            var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;
                            contributorsKrResponses.Add(new ContributorsKrResponse
                            {
                                GoalKeyId = item.GoalKeyId,
                                EmployeeId = item.EmployeeId,
                                FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                                LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                                ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                                Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                                Score = item.Score,
                                ScoreRange = item.Score < Convert.ToDecimal(teamScore.MinimumOrganisationThreshold) ? Constants.Lowest : Constants.Highest,
                                Progress = item.Progress,
                                KeyDescription = item.KeyDescription,
                                KeyNote = item.KeyNotes,
                                KrCount = 0,//GetKrCount(item.GoalObjectiveId),
                                OrganisationId = user.OrganisationId,
                                DueDate = item.DueDate
                            });
                        }
                    }
                }

                teamScore.ContributorsKrResponse = contributorsKrResponses;
            }

            return teamScore;
        }
        public async Task<AvgOkrScoreResponse> GetAvgOkrScoreReport(long empId, int cycleId, int year, string token, UserIdentity user)
        {
            AvgOkrScoreResponse avgOkr = new AvgOkrScoreResponse
            {
                AvgOrganisationalScore = await AvgOrganisationalScore(cycleId, year),
                MinimumOrganisationThreshold = MinimumOrganisationThreshold()
            };

            List<ContributorsDotResponse> avgOkrScores = new List<ContributorsDotResponse>();
            var allEmployee = commonService.GetAllUserFromUsers(token);
            var objectives = GetEmplyeeOkrByCycleId(empId, cycleId, year).OrderByDescending(x => x.GoalObjectiveId);
            if (objectives != null)
            {
                foreach (var obj in objectives)
                {
                    var contributorslist = commonService.GetContributors((int)GoalType.GoalObjective, obj.GoalObjectiveId, allEmployee.Results, user.OrganisationId);
                    if (contributorslist.Count > 0)
                    {
                        foreach (var item in contributorslist)
                        {
                            avgOkrScores.Add(item);
                        }
                    }
                }

                avgOkr.ContributorsDotResponses = avgOkrScores;
            }

            return avgOkr;
        }

        public List<ReportMostLeastObjectiveKeyResult> ReportMostLeastObjectiveKeyResult(long empId, int cycle, int year, long orgId, string token)
        {
            CycleDetails cycleDetail = commonService.GetOrganisationCycleDetail(orgId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);

            var objectiveResponses = new List<ReportMostLeastObjectiveKeyResult>();
            var allEmployee = commonService.GetAllUserFromUsers(token);

            var keyDetail = goalKeyRepo.GetQueryable()
                .Where(x => x.CycleId == cycle && x.EmployeeId == empId && x.IsActive).ToList();

            foreach (var key in keyDetail)
            {
                objectiveResponses.Add(new ReportMostLeastObjectiveKeyResult
                {
                    GoalKeyId = key.GoalKeyId,
                    Year = year,
                    EndDate = key.DueDate,
                    DueCycle = quarterDetails == null ? string.Empty : quarterDetails.Symbol + "-" + year,
                    Score = key.Score,
                    KeyDescription = key.KeyDescription,
                    UpdatedOn = key.UpdatedOn ?? key.CreatedOn,
                    Contributors = GetContributorsByObj((int)GoalType.GoalKey, key.GoalKeyId, allEmployee.Results)
                });
            }
            // }
            //}

            return objectiveResponses;
        }

        public async Task<List<UserGoalKeyResponse>> GetWeeklyKrUpdatesReport(long empId, int cycleId, int year, string token, UserIdentity user)
        {
            List<UserGoalKeyResponse> userGoalKeyResponses = new List<UserGoalKeyResponse>();
            var allEmployee = commonService.GetAllUserFromUsers(token).Results;

            var allKr = await goalKeyRepo.GetQueryable().Where(x =>
                x.CycleId == cycleId && x.EmployeeId == empId && x.GoalStatusId == (int)GoalStatus.Public &&
                x.KrStatusId == (int)KrStatus.Accepted && x.IsActive).ToListAsync();
            if (allKr != null)
            {
                foreach (var key in allKr)
                {
                    var contributorDetails = GetKeyContributors((int) GoalType.GoalKey, key.GoalKeyId, allEmployee,
                        user.OrganisationId);
                    var getUserUpdate = GetKrScoreHistory(key.GoalKeyId, empId, false) - 1;
                    userGoalKeyResponses.Add(new UserGoalKeyResponse
                    {
                        GoalKeyId = key.GoalKeyId,
                        ObjectiveName = key.GoalObjectiveId == 0 ? "" : GetObjectiveName(key.GoalObjectiveId),
                        ObjectiveDescription = key.GoalObjectiveId == 0 ? "" : GetObjectiveDescription(key.GoalObjectiveId),
                        EmployeeId = key.EmployeeId,
                        DueDate = key.DueDate,
                        Score = key.Score,
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == key.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == key.EmployeeId)?.LastName,
                        ImagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == key.EmployeeId)?.ImagePath,
                        KeyDescription = key.KeyDescription,
                        Progress = key.Progress,
                        UserKrUpdate = getUserUpdate,
                        KeyContributorsResponses = contributorDetails, 
                        KrUpdate = contributorDetails.Any() ? (contributorDetails.Sum(x=>x.KrUpdates) + getUserUpdate) : getUserUpdate
                    });

                }
            }

            var userKrUpdates = userGoalKeyResponses.OrderByDescending(p => p.KrUpdate).Take(6).ToList();
            return userKrUpdates;
        }

        public async Task<WeeklyReportResponse> WeeklyReportResponse(long empId, int cycle, int year, string token, UserIdentity identity)
        {
            WeeklyReportResponse weeklyReportResponses = new WeeklyReportResponse();
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(identity.OrganisationId, token);
            CycleDetails cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            var allEmployee = commonService.GetAllUserFromUsers(token);

            var objectives = GetEmplyeeOkrByCycleIdOrderbyScore(empId, cycle, year);
            if (objectives != null)
            {
                foreach (var obj in objectives)
                {
                    var keyDetail = await GetGoalScoreDesc(obj.GoalObjectiveId);

                    foreach (var key in keyDetail)
                    {
                        if (cycleDurationDetails.CycleDurationId == 1)
                        {
                            weeklyReportResponses = SeparationQuarterly_WeeklyReportResponse(weeklyReportResponses, quarterDetails, identity, allEmployee.Results, key, obj);
                        }
                        else if (cycleDurationDetails.CycleDurationId == 2)
                        {
                            weeklyReportResponses = SeparationHalfYearly_WeeklyReportResponse(weeklyReportResponses, quarterDetails, identity, allEmployee.Results, key, obj);
                        }
                        else if (cycleDurationDetails.CycleDurationId == 3)
                        {
                            weeklyReportResponses = SeparationYearly_WeeklyReportResponse(weeklyReportResponses, quarterDetails, identity, allEmployee.Results, key, obj);
                        }
                        else if (cycleDurationDetails.CycleDurationId == 4)
                        {
                            weeklyReportResponses = SeparationThreeYearly_WeeklyReportResponse(weeklyReportResponses, quarterDetails, identity, allEmployee.Results, key, obj);
                        }
                    }
                }
            }

            return weeklyReportResponses;
        }

        public List<ProgressReportResponse> ProgressReport(int cycle, int year, string token)
        {
            var allEmployee = commonService.GetAllUserFromUsers(token);
            List<UnlockSupportTeam> unlockSupportTeam = unlockSupportTeamRepo.GetQueryable().ToList();
            List<UserResponse> userResponseList = new List<UserResponse>();

            if (unlockSupportTeam != null)
            {
                foreach (var user in unlockSupportTeam)
                {
                    var userResponse = new UserResponse
                    {
                        EmailId = user.EmailId
                    };
                    userResponseList.Add(userResponse);
                }

                allEmployee.Results.RemoveAll(x => userResponseList.Any(y => y.EmailId == x.EmailId));
            }

            return GetProgressReport(cycle, year, allEmployee.Results);
        }

        public List<QuarterReportResponse> QuarterReport(int cycle, int year, string token, UserIdentity identity)
        {
            var allEmployee = commonService.GetAllUserFromUsers(token);
            CycleDetails cycleDetail = commonService.GetOrganisationCycleDetail(identity.OrganisationId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            var employeeResponse = allEmployee.Results;

            List<UnlockSupportTeam> unlockSupportTeam = unlockSupportTeamRepo.GetQueryable().ToList();
            List<UserResponse> userResponseList = new List<UserResponse>();
            if (unlockSupportTeam != null)
            {
                foreach (var user in unlockSupportTeam)
                {
                    var userResponse = new UserResponse
                    {
                        EmailId = user.EmailId
                    };
                    userResponseList.Add(userResponse);
                }

                employeeResponse.RemoveAll(x => userResponseList.Any(y => y.EmailId == x.EmailId));
            }

            List<QuarterReportResponse> quarterReportResponses = GetQuarterReport(cycle, year, allEmployee.Results);

            foreach (var item in quarterReportResponses)
            {
                var reportingTo = employeeResponse.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ReportingTo;

                item.EmployeeCode = employeeResponse.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.EmployeeCode;
                item.FirstName = employeeResponse.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName;
                item.LastName = employeeResponse.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName;
                if (reportingTo != null && reportingTo != "")
                {
                    item.FirstNameRTo = employeeResponse.FirstOrDefault(x => x.EmployeeId == Convert.ToInt64(reportingTo))?.FirstName;
                    item.LastNameRTo = employeeResponse.FirstOrDefault(x => x.EmployeeId == Convert.ToInt64(reportingTo))?.LastName;
                }

                item.Cycle = quarterDetails == null ? "NA" : quarterDetails.Symbol + "-" + year;
            }

            return quarterReportResponses;
        }

        public List<StatusReportResponse> StatusReport(int cycle, int year, string token, UserIdentity identity)
        {
            List<StatusReportResponse> statusReportResponses = new List<StatusReportResponse>();
            var allEmployee = commonService.GetAllUserFromUsers(token);
            CycleDetails cycleDetail = commonService.GetOrganisationCycleDetail(identity.OrganisationId, token).FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);
            var employeeResponse = allEmployee.Results.Where(x => x.IsActive);
            int lockDuration = Convert.ToInt32(Configuration.GetSection("OkrLockDuration").Value);
            var goalAutoLockDate = quarterDetails.StartDate?.AddDays(lockDuration);
            string ssoLogin = Configuration.GetSection("Passport:SsoLogin").Value;

            List<UnlockSupportTeam> unlockSupportTeam = unlockSupportTeamRepo.GetQueryable().ToList();
            List<UserResponse> userResponseList = new List<UserResponse>();

            if (unlockSupportTeam != null)
            {
                foreach (var user in unlockSupportTeam)
                {
                    var userResponse = new UserResponse
                    {
                        EmailId = user.EmailId
                    };
                    userResponseList.Add(userResponse);
                }

                allEmployee.Results.RemoveAll(x => userResponseList.Any(y => y.EmailId == x.EmailId));
            }

            foreach (var item in employeeResponse)
            {
                var status = "NotStarted";
                var employeeId = item.EmployeeId;

                var goalObjective = GetObjectiveByEmployee(employeeId, cycle, year);

                if (goalObjective != null && DateTime.UtcNow <= goalAutoLockDate)
                    status = "Started";
                else if (goalObjective != null && DateTime.UtcNow > goalAutoLockDate)
                    status = "Submitted";

                StatusReportResponse statusReport = new StatusReportResponse
                {
                    EmployeeCode = item.EmployeeCode,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    EmailId = item.EmailId,
                    Status = status
                };

                statusReportResponses.Add(statusReport);
            }

            return statusReportResponses;
        }

        public List<ReportContributorsWithScore> NLevelContributors(long empId, long goalId, int goalType, List<UserResponse> allEmployee)
        {
            var result = new List<ReportContributorsWithScore>();
            if (goalType == 1)
            {
                var objective = goalObjectiveRepo.GetQueryable().Where(x => x.ImportedId == goalId && x.EmployeeId == empId && x.IsActive).ToList();
                foreach (var obj in objective)
                {
                    result = AlignObjectiveSeparation_NLevelContributors(1, obj.GoalObjectiveId, allEmployee);
                }
            }
            else
            {
                var objective = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalId && x.EmployeeId == empId && x.IsActive).ToList();
                foreach (var obj in objective)
                {
                    result = AlignObjectiveSeparation_NLevelContributors((int)GoalType.GoalKey, obj.GoalKeyId, allEmployee);
                }
            }

            return result;
        }

        public List<ObjectiveContributors> GetObjContributlorsByGolaTypeAndId(int goalType, long goalId, long employeeId = 0)
        {
            var contributorsList = new List<ObjectiveContributors>();

            using (var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC sp_GetAllContributorsWithScore " + goalType + "," + goalId + "," + employeeId;
                command.CommandType = CommandType.Text;
                OkrServiceDBContext.Database.OpenConnection();
                var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    var item = new ObjectiveContributors
                    {
                        EmployeeId = Convert.ToInt64(dataReader["EmployeeId"].ToString()),
                        GoalType = goalType,
                        GoalId = goalId,
                        Score = Convert.ToDecimal(dataReader["Score"].ToString()),
                        EndDate = Convert.ToDateTime(dataReader["EndDate"])
                    };
                    contributorsList.Add(item);
                }
                OkrServiceDBContext.Database.CloseConnection();
            }

            return contributorsList;
        }

        public async Task<decimal> AvgOrganisationalScore(int cycleId, int Year)
        {
            decimal score = 0;

            //var okr = await goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.Year == Year && !x.IsPrivate && x.IsActive).ToListAsync();
            //if (okr.Count > 0)
            //{
            //    score = okr.Select(x => x.Score).Average();
            //}

            var allKr = await goalKeyRepo.GetQueryable().Where(x => x.CycleId == cycleId && x.IsActive).ToListAsync();
            if (allKr.Count > 0)
            {
                score = allKr.Select(x => x.Score).Average();
            }

            return score;
        }

        public string MinimumOrganisationThreshold()
        {
            string result = "";
            result = constantRepo.GetQueryable().FirstOrDefault(x => x.ConstantId == 1 && x.IsActive).ConstantValue;
            return result;
        }

        public List<GoalObjective> GetEmplyeeOkrByCycleId(long empId, int cycleId, int year)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive && x.Year == year).ToList();
        }

        public List<GoalObjective> GetEmplyeeOkrByCycleIdOrderbyScore(long empId, int cycleId, int year)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.ObjectiveCycleId == cycleId && x.EmployeeId == empId && x.IsActive && x.Year == year).OrderByDescending(x => x.Score).ToList();
        }

        public async Task<List<GoalKey>> GetGoalKey(long goalObjectiveId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive).ToListAsync();
        }

        public int GetKrUpdates(long GoalKeyId)
        {
            return goalKeyAuditRepo.GetQueryable().Count(x => x.UpdatedGoalKeyId == GoalKeyId && x.UpdatedColumn == Constants.Score);
        }

        public int GetKrScoreHistory(long GoalKeyId, long? empId, bool isContributor)
        {
            if(!isContributor)
            {
                return goalKeyHistoryRepo.GetQueryable().Count(x => x.GoalKeyId == GoalKeyId && x.CreatedBy == empId);
            }
            else
            {
                return goalKeyHistoryRepo.GetQueryable().Count(x => x.GoalKeyId == GoalKeyId);
            }
            
        }

        public string GetObjectiveName(long goalobjectiveId)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalobjectiveId).ObjectiveName;
        }

        public string GetObjectiveDescription(long goalobjectiveId)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalobjectiveId).ObjectiveDescription;
        }

        public string GetUpdatedScore(DateTime? StartDate, DateTime endDate, long GoalkeyId)
        {
            string score = "";
            var result = goalKeyAuditRepo.GetQueryable().Where(x => x.UpdatedOn >= StartDate && x.UpdatedOn <= endDate && x.UpdatedColumn == Constants.Score && x.UpdatedGoalKeyId == GoalkeyId);
            if (result != null)
            {
                foreach (var item in result)
                {
                    score = item.NewValue;
                }
            }

            return score;
        }

        public int GetCycleStatus(DateTime? StartDate, DateTime? endDate)
        {
            int result = 0;
            if (endDate < DateTime.Now)
            {
                result = 0;
            }
            else if (StartDate < DateTime.Now)
            {
                result = 1;
            }
            else if (StartDate >= DateTime.Now && endDate >= DateTime.Now)
            {
                result = 2;
            }
            return result;
        }

        public async Task<List<GoalKey>> GetGoalScoreDesc(long goalObjectiveId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == goalObjectiveId && x.IsActive).OrderByDescending(x => x.Score).ToListAsync();
        }

        public List<ProgressReportResponse> GetProgressReport(int cycleId, long year, List<UserResponse> allEmployee)
        {
            var contributorsList = new List<ProgressReportResponse>();

            using (var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC sp_ProgressReport " + cycleId + "," + year;
                command.CommandType = CommandType.Text;
                OkrServiceDBContext.Database.OpenConnection();
                var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    long employeeId = Convert.ToInt64(dataReader["EmployeeId"].ToString());

                    if (allEmployee.Any(x => x.EmployeeId == employeeId))
                    {
                        var employeeCode = allEmployee.FirstOrDefault(x => x.EmployeeId == employeeId)?.EmployeeCode;
                        var item = new ProgressReportResponse
                        {
                            EmployeeId = employeeId,
                            EmployeeCode = employeeCode,
                            FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == employeeId)?.FirstName,
                            LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == employeeId)?.LastName,
                            PrivateOkrScore = Convert.ToDecimal(dataReader["PrivateOkrScore"].ToString()),
                            OrgOkrScore = Convert.ToDecimal(dataReader["OrgOkrScore"].ToString()),
                            TotalOkrScore = Convert.ToDecimal(dataReader["TotalScore"].ToString())
                        };
                        contributorsList.Add(item);
                    }
                }
                OkrServiceDBContext.Database.CloseConnection();
            }

            return contributorsList;
        }

        public List<QuarterReportResponse> GetQuarterReport(int cycleId, long year, List<UserResponse> allEmployee)
        {
            var contributorsList = new List<QuarterReportResponse>();

            using (var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC sp_QuarterReport " + cycleId + "," + year;
                command.CommandType = CommandType.Text;
                OkrServiceDBContext.Database.OpenConnection();
                var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    long employeeId = Convert.ToInt64(dataReader["EmployeeId"].ToString());

                    if (allEmployee.Any(x => x.EmployeeId == employeeId))
                    {
                        var item = new QuarterReportResponse
                        {
                            EmployeeId = employeeId,
                            ObjectiveName = Convert.ToString(dataReader["ObjectiveName"]),
                            ObjectiveDesc = Convert.ToString(dataReader["ObjectiveDesc"]),                        
                            Score = Convert.ToDecimal(dataReader["Score"].ToString())
                        };

                        contributorsList.Add(item);
                    }
                }
                OkrServiceDBContext.Database.CloseConnection();
            }

            return contributorsList;
        }

        public GoalObjective GetObjectiveByEmployee(long employeeId, int cycleId, int year)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == employeeId && x.Year == year && x.ObjectiveCycleId == cycleId && x.IsActive);
        }

        #region Private Methods

        private WeeklyReportResponse SeparationQuarterly_WeeklyReportResponse(WeeklyReportResponse weeklyReportResponses, QuarterDetails quarterDetails, UserIdentity identity, List<UserResponse> allEmployee, GoalKey key, GoalObjective obj)
        {
            var startdate = quarterDetails.StartDate;
            var endDate2Week = Convert.ToDateTime(quarterDetails.StartDate).AddDays(14);

            var Startdate4week = endDate2Week;
            var endDate4Week = Startdate4week.AddDays(14);

            var StartDate6week = endDate4Week;
            var Endate6week = StartDate6week.AddDays(14);

            var StartDate8week = Endate6week;
            var Endate8week = StartDate8week.AddDays(14);

            var StartDate10week = Endate8week;
            var Endate10week = StartDate10week.AddDays(14);

            var StartDate12week = Endate10week;
            var Endate12week = Convert.ToDateTime(quarterDetails.EndDate);

            weeklyReportResponses.Week2.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(startdate, endDate2Week, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(startdate, endDate2Week),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Week4.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(Startdate4week, endDate4Week, key.GoalKeyId) == "" ? GetUpdatedScore(startdate, endDate2Week, key.GoalKeyId) : GetUpdatedScore(Startdate4week, endDate4Week, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(Startdate4week, endDate4Week),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Week6.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartDate6week, Endate6week, key.GoalKeyId) == "" ? GetUpdatedScore(Startdate4week, endDate4Week, key.GoalKeyId) : GetUpdatedScore(StartDate6week, Endate6week, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartDate6week, Endate6week),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Week8.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartDate8week, Endate8week, key.GoalKeyId) == "" ? GetUpdatedScore(StartDate6week, Endate6week, key.GoalKeyId) : GetUpdatedScore(StartDate8week, Endate8week, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartDate8week, Endate8week),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Week10.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartDate10week, Endate10week, key.GoalKeyId) == "" ? GetUpdatedScore(StartDate8week, Endate8week, key.GoalKeyId) : GetUpdatedScore(StartDate10week, Endate10week, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartDate10week, Endate10week),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Week12.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartDate12week, Endate12week, key.GoalKeyId) == "" ? GetUpdatedScore(StartDate10week, Endate10week, key.GoalKeyId) : GetUpdatedScore(StartDate12week, Endate12week, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartDate12week, Endate12week),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            return weeklyReportResponses;
        }

        private WeeklyReportResponse SeparationHalfYearly_WeeklyReportResponse(WeeklyReportResponse weeklyReportResponses, QuarterDetails quarterDetails, UserIdentity identity, List<UserResponse> allEmployee, GoalKey key, GoalObjective obj)
        {
            var StartMonth = quarterDetails.StartDate;
            var endMonth1 = Convert.ToDateTime(StartMonth).AddMonths(1);

            var StartMonth2 = endMonth1;
            var EndMonth2 = StartMonth2.AddMonths(1);

            var StartMonth3 = EndMonth2;
            var EndMonth3 = StartMonth3.AddMonths(1);

            var StartMonth4 = EndMonth3;
            var EndMonth4 = StartMonth4.AddMonths(1);

            var StartMonth5 = EndMonth4;
            var EndMonth5 = StartMonth5.AddMonths(1);

            var StartMonth6 = EndMonth5;
            var EndMonth6 = StartMonth6.AddMonths(1);

            weeklyReportResponses.Month1.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth, endMonth1, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth, endMonth1),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Month2.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth2, EndMonth2, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth, endMonth1, key.GoalKeyId) : GetUpdatedScore(StartMonth2, EndMonth2, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth2, EndMonth2),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Month3.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth3, EndMonth3, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth2, EndMonth2, key.GoalKeyId) : GetUpdatedScore(StartMonth3, EndMonth3, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth3, EndMonth3),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Month4.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth4, EndMonth4, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth3, EndMonth3, key.GoalKeyId) : GetUpdatedScore(StartMonth4, EndMonth4, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth4, StartMonth4),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Month5.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth5, EndMonth5, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth4, EndMonth4, key.GoalKeyId) : GetUpdatedScore(StartMonth5, EndMonth5, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth5, StartMonth5),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Month6.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth6, EndMonth6, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth5, EndMonth5, key.GoalKeyId) : GetUpdatedScore(StartMonth6, EndMonth6, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth6, StartMonth6),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            return weeklyReportResponses;
        }

        private WeeklyReportResponse SeparationYearly_WeeklyReportResponse(WeeklyReportResponse weeklyReportResponses, QuarterDetails quarterDetails, UserIdentity identity, List<UserResponse> allEmployee, GoalKey key, GoalObjective obj)
        {
            var StartMonth = quarterDetails.StartDate;
            var endMonth1 = Convert.ToDateTime(StartMonth).AddMonths(1);

            var StartMonth2 = endMonth1;
            var EndMonth2 = StartMonth2.AddMonths(1);

            var StartMonth3 = EndMonth2;
            var EndMonth3 = StartMonth3.AddMonths(1);

            var StartMonth4 = EndMonth3;
            var EndMonth4 = StartMonth4.AddMonths(1);

            var StartMonth5 = EndMonth4;
            var EndMonth5 = StartMonth5.AddMonths(1);

            var StartMonth6 = EndMonth5;
            var EndMonth6 = StartMonth6.AddMonths(1);

            var StartMonth7 = EndMonth6;
            var EndMonth7 = StartMonth7.AddMonths(1);

            var StartMonth8 = EndMonth7;
            var EndMonth8 = StartMonth8.AddMonths(1);

            var StartMonth9 = EndMonth8;
            var EndMonth9 = StartMonth9.AddMonths(1);

            var StartMonth10 = EndMonth9;
            var EndMonth10 = StartMonth10.AddMonths(1);

            var StartMonth11 = EndMonth10;
            var EndMonth11 = StartMonth11.AddMonths(1);

            var StartMonth12 = EndMonth11;
            var EndMonth12 = StartMonth12.AddMonths(1);

            weeklyReportResponses.Months1.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth, endMonth1, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth, endMonth1),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months2.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth2, EndMonth2, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth, endMonth1, key.GoalKeyId) : GetUpdatedScore(StartMonth2, EndMonth2, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth2, EndMonth2),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months3.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth3, EndMonth3, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth2, EndMonth2, key.GoalKeyId) : GetUpdatedScore(StartMonth3, EndMonth3, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth3, EndMonth3),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months4.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth4, EndMonth4, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth3, EndMonth3, key.GoalKeyId) : GetUpdatedScore(StartMonth4, EndMonth4, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth4, EndMonth4),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months5.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth5, EndMonth5, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth4, EndMonth4, key.GoalKeyId) : GetUpdatedScore(StartMonth5, EndMonth5, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth5, EndMonth5),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months6.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth6, EndMonth6, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth5, EndMonth5, key.GoalKeyId) : GetUpdatedScore(StartMonth6, EndMonth6, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth6, EndMonth6),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months7.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth7, EndMonth7, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth6, EndMonth6, key.GoalKeyId) : GetUpdatedScore(StartMonth7, EndMonth7, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth7, EndMonth7),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months8.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth8, EndMonth8, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth7, EndMonth7, key.GoalKeyId) : GetUpdatedScore(StartMonth8, EndMonth8, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth8, EndMonth8),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months9.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth9, EndMonth9, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth8, EndMonth8, key.GoalKeyId) : GetUpdatedScore(StartMonth9, EndMonth9, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth9, EndMonth9),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months10.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth10, EndMonth10, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth9, EndMonth9, key.GoalKeyId) : GetUpdatedScore(StartMonth10, EndMonth10, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth10, EndMonth10),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months11.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth11, EndMonth11, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth10, EndMonth10, key.GoalKeyId) : GetUpdatedScore(StartMonth11, EndMonth11, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth11, EndMonth11),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Months12.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartMonth12, EndMonth12, key.GoalKeyId) == "" ? GetUpdatedScore(StartMonth11, EndMonth11, key.GoalKeyId) : GetUpdatedScore(StartMonth12, EndMonth12, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartMonth12, EndMonth12),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            return weeklyReportResponses;
        }

        private WeeklyReportResponse SeparationThreeYearly_WeeklyReportResponse(WeeklyReportResponse weeklyReportResponses, QuarterDetails quarterDetails, UserIdentity identity, List<UserResponse> allEmployee, GoalKey key, GoalObjective obj)
        {
            var StartYear = quarterDetails.StartDate;
            var EndYear = Convert.ToDateTime(StartYear).AddYears(1);

            var StartYear1 = EndYear;
            var EndYear1 = StartYear1.AddYears(1);

            var StartYear2 = EndYear1;
            var EndYear2 = StartYear2.AddYears(1);

            weeklyReportResponses.Year1.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartYear, EndYear, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartYear, EndYear),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Year2.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartYear1, EndYear1, key.GoalKeyId) == "" ? GetUpdatedScore(StartYear, EndYear, key.GoalKeyId) : GetUpdatedScore(StartYear1, EndYear1, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartYear1, EndYear1),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            weeklyReportResponses.Year3.Add(new KrGrowthCycleResponse
            {
                GoalKeyId = key.GoalKeyId,
                ObjectiveName = GetObjectiveName(obj.GoalObjectiveId),
                ObjectiveDescription = GetObjectiveDescription(obj.GoalObjectiveId),
                EmployeeId = key.EmployeeId,
                DueDate = key.DueDate,
                Score = GetUpdatedScore(StartYear2, EndYear2, key.GoalKeyId) == "" ? GetUpdatedScore(StartYear1, EndYear1, key.GoalKeyId) : GetUpdatedScore(StartYear2, EndYear2, key.GoalKeyId),
                GoalScore = key.Score,
                KeyDescription = key.KeyDescription,
                Progress = key.Progress,
                KrUpdate = GetKrUpdates(key.GoalKeyId),
                CycleStatus = GetCycleStatus(StartYear2, EndYear2),
                KeyContributorsResponses = commonService.GetKeyContributors((int)GoalType.GoalKey, key.GoalKeyId, allEmployee, identity.OrganisationId)
            });

            return weeklyReportResponses;
        }

        private List<ReportContributorsWithScore> AlignObjectiveSeparation_NLevelContributors(int goalType, long goalId, List<UserResponse> allEmployee)
        {
            var result = new List<ReportContributorsWithScore>();
            dynamic alignObj = null;
            if (goalType == 1)
            {
                alignObj = goalObjectiveRepo.GetQueryable().Where(x => x.ImportedId == goalId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public).ToList();
            }
            else
            {
                alignObj = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int) KrStatus.Accepted).ToList();
            }

            foreach (var item in alignObj)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

                    result.Add(new ReportContributorsWithScore
                    {
                        EmployeeId = item.EmployeeId,
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                        Score = item.Score,
                        EndDate = goalType == 1 ? item.EndDate : item.DueDate
                    });
                }
            }

            return result;
        }

        private List<KeyContributorsResponse> GetKeyContributors(int goalType, long goalId, List<UserResponse> allEmployee, long organisationId)
        {
            var contributorsKey = new List<KeyContributorsResponse>();
            var contributors =  goalKeyRepo.GetQueryable().Where(x =>
                x.ImportedId == goalId && x.IsActive && x.GoalStatusId == (int) GoalStatus.Public &&
                x.KrStatusId == (int) KrStatus.Accepted && x.IsActive).ToList();

            if (contributors.Count > 0)
            {
                foreach (var item in contributors)
                {
                    if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                    {
                        var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;
                        int count_GetKrScoreHistory = GetKrScoreHistory(item.GoalKeyId, item.EmployeeId, true) - 1;

                        contributorsKey.Add(new KeyContributorsResponse
                        {
                            GoalKeyId = item.GoalKeyId,
                            EmployeeId = item.EmployeeId,
                            OrganisationId = organisationId,
                            FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                            LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                            ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                            Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                            Score = item.Score,
                            KeyDescription = item.KeyDescription,
                            Progress = item.Progress,
                            KrUpdates = count_GetKrScoreHistory == -1 ? 0 : count_GetKrScoreHistory
                        });
                    }
                }
            }

            return contributorsKey;
        }
        #endregion
    }
}

