using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OKRService.Service
{
    [ExcludeFromCodeCoverage]
    public class CommonService : BaseService, ICommonService
    {
        private readonly IRepositoryAsync<GoalObjective> goalObjectiveRepo;
        private readonly IRepositoryAsync<GoalKey> goalKeyRepo;
        private readonly IRepositoryAsync<GoalKeyAudit> goalKeyAuditRepo;
        private readonly IRepositoryAsync<ErrorLog> errorLogRepo;
        private readonly IRepositoryAsync<UnLockLog> unlockLogRepo;
        private readonly IRepositoryAsync<Constant> constantRepo;
        private readonly IRepositoryAsync<KrStatusMessage> krStatusMessageRepo;
        private readonly IRepositoryAsync<GoalKeyHistory> goalKeyHistoryRepo;
        protected readonly IDistributedCache _distributedCache;

        public CommonService(IServicesAggregator servicesAggregateService, IDistributedCache distributedCache) : base(servicesAggregateService)
        {
            goalObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<GoalObjective>();
            goalKeyRepo = UnitOfWorkAsync.RepositoryAsync<GoalKey>();
            goalKeyAuditRepo = UnitOfWorkAsync.RepositoryAsync<GoalKeyAudit>();
            errorLogRepo = UnitOfWorkAsync.RepositoryAsync<ErrorLog>();
            unlockLogRepo = UnitOfWorkAsync.RepositoryAsync<UnLockLog>();
            constantRepo = UnitOfWorkAsync.RepositoryAsync<Constant>();
            krStatusMessageRepo = UnitOfWorkAsync.RepositoryAsync<KrStatusMessage>();
            goalKeyHistoryRepo = UnitOfWorkAsync.RepositoryAsync<GoalKeyHistory>();
            _distributedCache = distributedCache;
        }

        public EmployeeResult GetAllUserFromUsers(string jwtToken)
        {
            var employeeResponse = new EmployeeResult();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                var cacheKey = TenantId + Constants.GetAllUsers;
                string serializedList;
                var redisList = _distributedCache.Get(cacheKey);
                if (redisList != null)
                {
                    serializedList = Encoding.UTF8.GetString(redisList);
                    var resDeserializeObject = JsonConvert.DeserializeObject<List<UserResponse>>(serializedList);
                    employeeResponse.Results = resDeserializeObject;
                }
                else
                {
                    using var response = httpClient.GetAsync($"api/User/GetAllusers?pageIndex=1&pageSize=9999").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = response.Content.ReadAsStringAsync().Result;
                        var user = JsonConvert.DeserializeObject<PayloadCustomList<PageResults<UserResponse>>>(apiResponse);
                        employeeResponse.Results = user.Entity.Records;

                        serializedList = JsonConvert.SerializeObject(user.Entity.Records);
                        redisList = Encoding.UTF8.GetBytes(serializedList);
                        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(Configuration.GetValue<int>("Redis:ExpiryTime")))
                            .SetSlidingExpiration(TimeSpan.FromMinutes(Configuration.GetValue<int>("Redis:SlidingExpireTime")));
                        _distributedCache.Set(cacheKey, redisList, options);
                    }
                }

            }
            return employeeResponse;
        }

        public async Task<UserIdentity> GetUserIdentity()
        {
            Logger.Information("GetUserIdentity called");
            var hasIdentity = HttpContext.Request.Headers.TryGetValue("UserIdentity", out var userIdentity);
            Logger.Information("is found the user identity in  header-" + hasIdentity);

            Logger.Information("Path - " + HttpContext.Request.Path);
            Logger.Information("Method - " + HttpContext.Request.Method);

            if (!hasIdentity) return await GetUserIdentity(UserToken);
            Logger.Information("Value found the user identity in  header-" + userIdentity);
            var decryptVal = Encryption.DecryptStringAes(userIdentity, Configuration.GetValue<string>("Encryption:SecretKey"),
                Configuration.GetValue<string>("Encryption:SecretIVKey"));
            var identity = JsonConvert.DeserializeObject<UserIdentity>(decryptVal);
            Logger.Information("User information is received for employee id" + identity.EmployeeId);
            return await Task.FromResult(identity).ConfigureAwait(false);
        }

        public async Task<UserIdentity> GetUserIdentity(string jwtToken)
        {
            UserIdentity loginUserDetail = new UserIdentity();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                using var response = await httpClient.PostAsync($"api/User/Identity", new StringContent(""));
                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<PayloadCustom<UserIdentity>>(apiResponse);
                    loginUserDetail = user.Entity;
                }
            }
            return loginUserDetail;
        }

        public List<CycleDetails> GetOrganisationCycleDetail(long orgId, string jwtToken)
        {
            List<CycleDetails> cycleDetails = new List<CycleDetails>();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                var response = httpClient.GetAsync($"api/Organisation/GetOrganisationCycleDetails?organisationId=" + orgId).Result;
                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = response.Content.ReadAsStringAsync().Result;
                    var organizationCycleDetails = JsonConvert.DeserializeObject<PayloadCustom<OrganisationCycleDetails>>(apiResponse);
                    cycleDetails = organizationCycleDetails.Entity.CycleDetails;
                }
            }
            return cycleDetails;
        }

        public OrganisationCycleDetails GetOrganisationCycleDurationId(long orgId, string jwtToken)
        {
            var organizationCycle = new OrganisationCycleDetails();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                var cacheKey = TenantId + Constants.OrganizationCycleDetails + orgId;
                string serializedList;
                var redisList =  _distributedCache.Get(cacheKey);
                if (redisList != null)
                {
                    serializedList = Encoding.UTF8.GetString(redisList);
                    var resDeserializeObject = JsonConvert.DeserializeObject<OrganisationCycleDetails>(serializedList);
                    organizationCycle = resDeserializeObject;
                }
                else
                {
                    var response = httpClient.GetAsync($"api/Organisation/GetOrganisationCycleDetails?organisationId=" + orgId).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = response.Content.ReadAsStringAsync().Result;
                        var organizationCycleDetails = JsonConvert.DeserializeObject<PayloadCustom<OrganisationCycleDetails>>(apiResponse);
                        organizationCycle = organizationCycleDetails.Entity;

                        serializedList = JsonConvert.SerializeObject(organizationCycle);
                        redisList = Encoding.UTF8.GetBytes(serializedList);
                        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(Configuration.GetValue<int>("Redis:ExpiryTime")))
                            .SetSlidingExpiration(TimeSpan.FromMinutes(Configuration.GetValue<int>("Redis:SlidingExpireTime")));
                        _distributedCache.Set(cacheKey, redisList, options);
                    }
                }
            }
            return organizationCycle;
        }

        public List<ContributorsDotResponse> GetContributors(int goalType, long goalId, List<UserResponse> allEmployee, long organisationId)
        {
            var contributors = GetObjectiveContributor(goalType, goalId);
            var minimumOrganisationThreshold = MinimumOrganisationThreshold();
            List<ContributorsDotResponse> contributorsResponse = new List<ContributorsDotResponse>();
            if (contributors.Count > 0)
            {
                contributorsResponse = SeparationContributors_GetContributors(contributors, allEmployee, organisationId, minimumOrganisationThreshold);
            }

            return contributorsResponse;
        }

        public List<KeyContributorsResponse> GetKeyContributors(int goalType, long goalId, List<UserResponse> allEmployee, long organisationId)
        {
            var contributorsKey = new List<KeyContributorsResponse>();
            var contributors = GetKRContributor(goalType, goalId);

            if (contributors.Count > 0)
            {
                foreach (var item in contributors)
                {
                    if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                    {
                        var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

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
                            KrUpdates = GetKrUpdates(item.GoalKeyId)
                        });
                    }
                }
            }

            return contributorsKey;
        }

        public int GetContributeKrUpdates(int goalType, long goalId)
        {
            int result = 0;
            var contributors = GetKRContributor(goalType, goalId);
            foreach (var item in contributors)
            {
                result += GetKrUpdates(item.GoalKeyId);
            }

            return result;
        }

        public List<ContributorsResponse> GetObjectiveContributor(int goalType, long goalId, List<UserResponse> allEmployee)
        {
            var contributorsResponse = new List<ContributorsResponse>();
            var contributors = GetObjectiveContributor(goalType, goalId);

            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

                    contributorsResponse.Add(new ContributorsResponse
                    {
                        EmployeeId = item.EmployeeId,
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                        AssignmentTypeId = (int)AssignmentType.WithParentObjective,
                        GoalId = item.GoalObjectiveId,
                        GoalType = goalType,
                        DueDate = item.EndDate,
                        ObjectiveName = item.GoalObjectiveId != 0 ? GetGoalObjective(item.GoalObjectiveId)?.ObjectiveName : null,
                        StartDate = item.StartDate,
                        GoalStatusId = item.GoalStatusId

                    });
                }
            }

            return contributorsResponse;
        }

        public List<GoalObjective> GetObjectiveContributor(int goalType, long goalId)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.ImportedType == goalType && x.ImportedId == goalId && x.IsActive).ToList();
        }

        public List<ContributorPdfResponse> GetObjectiveContributorForPdf(int goalType, long goalId, List<UserResponse> allEmployee)
        {
            var contributorsResponse = new List<ContributorPdfResponse>();
            var contributors = GetObjectiveContributor(goalType, goalId);

            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;
                    contributorsResponse.Add(new ContributorPdfResponse
                    {
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath?.Trim(),
                    });
                }
            }

            return contributorsResponse;
        }

        public async Task<List<ContributorsResponse>> GetObjectiveContributorAsync(int goalType, long goalId, List<UserResponse> allEmployee)
        {
            var contributorsResponse = new List<ContributorsResponse>();
            var contributors = await GetObjectiveContributorAsync(goalType, goalId);

            foreach (var item in contributors)
            {
                var keyContributors = await goalKeyRepo.GetQueryable().Where(x => x.GoalObjectiveId == item.GoalObjectiveId).ToListAsync();
                foreach (var key in keyContributors)
                {
                    if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                    {
                        var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

                        contributorsResponse.Add(new ContributorsResponse
                        {
                            EmployeeId = item.EmployeeId,
                            FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                            LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                            ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                            Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                            AssignmentTypeId = (int)AssignmentType.WithParentObjective,
                            GoalId = item.GoalObjectiveId,
                            GoalType = goalType,
                            DueDate = item.EndDate,
                            ObjectiveName = item.GoalObjectiveId != 0 ? GetGoalObjective(item.GoalObjectiveId)?.ObjectiveName : null,
                            StartDate = item.StartDate,
                            GoalStatusId = item.GoalStatusId
                        });
                    }
                }
            }

            return contributorsResponse;
        }

        public async Task<List<GoalObjective>> GetObjectiveContributorAsync(int goalType, long goalId)
        {
            return await goalObjectiveRepo.GetQueryable().Where(x => x.ImportedType == goalType && x.ImportedId == goalId && x.IsActive).ToListAsync();
        }

        public List<ContributorsResponse> GetKRContributor(int goalType, long goalId, List<UserResponse> allEmployee, decimal sourceTargetValue = 0)
        {
            var contributorsResponse = new List<ContributorsResponse>();
            var contributors = GetKRContributor(goalType, goalId);
            var targetValue = sourceTargetValue == 0 ? 1 : sourceTargetValue;
            var index = Constants.Zero;
            var sourceKeyId = Constants.ZeroId;
            var goalKey = goalKeyRepo.FindOne(x => x.GoalKeyId == goalId);
            sourceKeyId = goalKey.GoalKeyId;
            if (goalKey.ImportedId > Constants.Zero)
            {
                var sourceKey = goalKeyRepo.FindOne(x => x.GoalKeyId == goalKey.ImportedId);
                contributors?.Insert(index, sourceKey);
                sourceKeyId = sourceKey.GoalKeyId;
                index++;
            }

            contributors?.Insert(index, goalKey);


            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

                    contributorsResponse.Add(new ContributorsResponse
                    {
                        GoalType = goalType,
                        GoalId = item.GoalKeyId,
                        EmployeeId = item.EmployeeId,
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                        DueDate = item.DueDate,
                        TargetValue = item.TargetValue,
                        CurrentValue = item.CurrentValue,
                        KeyResult = item.KeyDescription,
                        ObjectiveName = item.GoalObjectiveId != 0 ? GetGoalObjective(item.GoalObjectiveId)?.ObjectiveName : null,
                        KrStatusId = item.KrStatusId,
                        KrAssigneeMessage = GetKrAssigneeMessage(goalId, item.GoalKeyId)?.KrAssigneeMessage,
                        KrAssignerMessage = GetKrAssigneeMessage(goalId, item.GoalKeyId)?.KrAssignerMessage,
                        AssignmentTypeId = item.AssignmentTypeId,
                        StartDate = item.StartDate,
                        GoalStatusId = item.GoalStatusId,
                        StartValue = item.StartValue,
                        ContributorsContribution = item.Score > 100 ? 100.00m : item.Score,
                        UpdatedOn = LatestUpdateGoalKey(item.GoalKeyId) == null ? item.CreatedOn : LatestUpdateGoalKey(item.GoalKeyId)?.UpdatedOn,
                        IsSource = item.GoalKeyId == sourceKeyId
                    });
                }
            }

            return contributorsResponse;
        }

        /// <summary>
        /// This method will return all the necessary details that come after clicking on show all contributors button
        /// </summary>
        /// <param name="goalType"></param>
        /// <param name="goalKeyId"></param>
        /// <param name="allEmployee"></param>
        /// <returns></returns>
        public List<KrStatusContributorResponse> GetKrStatusContributor(int goalType, long goalKeyId, List<UserResponse> allEmployee)
        {
            var contributorsResponse = new List<KrStatusContributorResponse>();
            var contributors = GetKRContributor(goalType, goalKeyId);

            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

                    contributorsResponse.Add(new KrStatusContributorResponse
                    {
                        GoalKeyId = item.GoalKeyId,
                        EmployeeId = item.EmployeeId,
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                        DueDate = item.DueDate,
                        TargetValue = item.TargetValue,
                        CurrentValue = item.CurrentValue,
                        KeyResult = item.KeyDescription,
                        ObjectiveName = item.GoalObjectiveId != 0 ? GetGoalObjective(item.GoalObjectiveId).ObjectiveName : null,
                        KrStatusId = item.KrStatusId,
                        KrAssigneeMessage = GetKrAssigneeMessage(goalKeyId, item.GoalKeyId).KrAssigneeMessage,
                        KrAssignerMessage = GetKrAssigneeMessage(goalKeyId, item.GoalKeyId).KrAssignerMessage,
                        AssignmentTypeId = item.AssignmentTypeId,
                        StartDate = item.StartDate
                    });
                }
            }

            return contributorsResponse;
        }

        public List<KrContributorResponse> GetKrContributor(int goalType, long goalKeyId, List<UserResponse> allEmployee)
        {
            var krContributorsResponse = new List<KrContributorResponse>();
            var contributors = GetKRContributor(goalType, goalKeyId);

            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

                    krContributorsResponse.Add(new KrContributorResponse
                    {
                        GoalKeyId = item.GoalKeyId,
                        EmployeeId = item.EmployeeId,
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                        DueDate = item.DueDate,
                        TargetValue = item.TargetValue,
                        CurrentValue = item.CurrentValue,
                        KeyResult = item.KeyDescription,
                        ObjectiveName = item.GoalObjectiveId != 0 ? GetGoalObjective(item.GoalObjectiveId).ObjectiveName : null,
                        StartDate = item.StartDate,
                        AssignmentTypeId = item.AssignmentTypeId,
                    });
                }
            }

            return krContributorsResponse;
        }

        public GoalObjective GetGoalObjective(long GoalObjectiveId)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == GoalObjectiveId && x.IsActive);
        }

        public KrStatusMessage GetKrAssigneeMessage(long goalKeyId, long assigneeGoalKeyId)
        {
            var message = krStatusMessageRepo.GetQueryable().FirstOrDefault(x => x.AssignerGoalKeyId == goalKeyId && x.AssigneeGoalKeyId == assigneeGoalKeyId && x.IsActive);
            return message;
        }

        public List<GoalKey> GetKRContributor(int goalType, long goalId)
        {
            return goalKeyRepo.GetQueryable().Where(x => x.ImportedType == goalType && x.ImportedId == goalId && x.IsActive).ToList();
        }

        public List<ContributorPdfResponse> GetKRContributorForPdf(int goalType, long goalId, List<UserResponse> allEmployee)
        {
            var contributorsResponse = new List<ContributorPdfResponse>();
            var contributors = GetKRContributor(goalType, goalId);

            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;

                    contributorsResponse.Add(new ContributorPdfResponse
                    {
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath?.Trim()
                    });
                }
            }

            return contributorsResponse;
        }

        public async Task<List<ContributorsResponse>> GetKRContributorAsync(int goalType, long goalId, List<UserResponse> allEmployee, decimal sourceTargetValue = 0)
        {
            var contributorsResponse = new List<ContributorsResponse>();
            var contributors = await GetKRContributorAsync(goalType, goalId);
            var targetValue = sourceTargetValue == 0 ? 1 : sourceTargetValue;
            var index = Constants.Zero;
            var sourceKeyId = Constants.ZeroId;
            var goalKey = await goalKeyRepo.FindOneAsync(x => x.GoalKeyId == goalId);
            sourceKeyId = goalKey.GoalKeyId;
            if (goalKey.ImportedId > Constants.Zero)
            {
                var sourceKey = await goalKeyRepo.FindOneAsync(x => x.GoalKeyId == goalKey.ImportedId);
                contributors?.Insert(index, sourceKey);
                sourceKeyId = sourceKey.GoalKeyId;
                index++;
            }

            contributors?.Insert(index, goalKey);

            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;
                    var sourceStatusMessage = GetKrAssigneeMessage(item.GoalKeyId, goalId);
                    var contriStatusMessage = GetKrAssigneeMessage(goalId, item.GoalKeyId);

                    contributorsResponse.Add(new ContributorsResponse
                    {
                        GoalType = goalType,
                        GoalId = item.GoalKeyId,
                        EmployeeId = item.EmployeeId,
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                        DueDate = item.DueDate,
                        TargetValue = item.TargetValue,
                        CurrentValue = item.CurrentValue,
                        KeyResult = item.KeyDescription,
                        ObjectiveName = item.GoalObjectiveId != 0 ? GetGoalObjective(item.GoalObjectiveId)?.ObjectiveName : null,
                        KrStatusId = item.KrStatusId,
                        KrAssigneeMessage = item.GoalKeyId == sourceKeyId ? sourceStatusMessage?.KrAssigneeMessage : contriStatusMessage?.KrAssigneeMessage,
                        KrAssignerMessage = item.GoalKeyId == sourceKeyId ? sourceStatusMessage?.KrAssignerMessage : contriStatusMessage?.KrAssignerMessage,
                        AssignmentTypeId = item.AssignmentTypeId,
                        StartDate = item.StartDate,
                        GoalStatusId = item.GoalStatusId,
                        StartValue = item.StartValue,
                        ContributorsContribution = item.Score > 100 ? 100.00m : item.Score,
                        UpdatedOn = LatestUpdateGoalKey(item.GoalKeyId) == null ? item.CreatedOn : LatestUpdateGoalKey(item.GoalKeyId)?.UpdatedOn,
                        IsSource = item.GoalKeyId == sourceKeyId
                    });
                }
            }

            return contributorsResponse;
        }

        public async Task<List<ContributorsResponse>> GetAllContributorAsync(int goalType, long goalId, List<UserResponse> allEmployee, UserIdentity identity, string token)
        {
            bool isExternal = false;
            var contributorsResponse = new List<ContributorsResponse>();
            var contributors = await GetKRContributorAsync(goalType, goalId);

            var index = Constants.Zero;
            var sourceKeyId = Constants.ZeroId;
            var goalKey = await goalKeyRepo.FindOneAsync(x => x.GoalKeyId == goalId);
            sourceKeyId = goalKey.GoalKeyId;
            if (goalKey.ImportedId > Constants.Zero)
            {
                var sourceKey = await goalKeyRepo.FindOneAsync(x => x.GoalKeyId == goalKey.ImportedId);
                contributors?.Insert(index, sourceKey);
                sourceKeyId = sourceKey.GoalKeyId;
                index++;
            }

            contributors?.Insert(index, goalKey);

            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var itemUser = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;
                    var sourceStatusMessage = GetKrAssigneeMessage(item.GoalKeyId, goalId);
                    var contriStatusMessage = GetKrAssigneeMessage(goalId, item.GoalKeyId);
                    var contributorContribution = item.Score > 100 ? 100.00m : item.Score < 0 ? 0.00m : item.Score;
                    var contributorlastLoginScore = 0.0m;
                    var teamDetails = new TeamDetails();
                    var teamName = string.Empty;
                    if (item.TeamId > 0)
                    {
                        isExternal = item.TeamId != itemUser?.OrganisationID;
                        teamDetails = GetTeamEmployeeByTeamId(isExternal ? Convert.ToInt64(itemUser?.OrganisationID) : item.TeamId, token);
                        teamName = teamDetails == null ? " " : teamDetails.OrganisationName;
                    }

                    if (identity.LastLoginDateTime != null)
                    {
                        var goalKeyHistory = goalKeyHistoryRepo.GetQueryable().Where(x => x.CreatedOn <= identity.LastLoginDateTime && x.GoalKeyId == item.GoalKeyId).OrderByDescending(x => x.HistoryId).FirstOrDefault();
                        if (goalKeyHistory != null)
                            contributorlastLoginScore = goalKeyHistory.Score;
                    }

                    contributorsResponse.Add(new ContributorsResponse
                    {
                        GoalType = goalType,
                        GoalId = item.GoalKeyId,
                        EmployeeId = item.EmployeeId,
                        FirstName = itemUser?.FirstName,
                        LastName = itemUser?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = itemUser?.Designation,
                        DueDate = item.DueDate,
                        TargetValue = item.TargetValue,
                        CurrentValue = item.CurrentValue,
                        KeyResult = item.KeyDescription,
                        ObjectiveName = item.GoalObjectiveId != 0 ? GetGoalObjective(item.GoalObjectiveId)?.ObjectiveName : null,
                        KrStatusId = item.KrStatusId,
                        AssignmentTypeId = item.AssignmentTypeId,
                        StartDate = item.StartDate,
                        GoalStatusId = item.GoalStatusId,
                        StartValue = item.StartValue,
                        ContributorsContribution = contributorContribution,
                        UpdatedOn = LatestUpdateGoalKey(item.GoalKeyId) == null ? item.CreatedOn : LatestUpdateGoalKey(item.GoalKeyId)?.UpdatedOn,
                        IsSource = item.GoalKeyId == sourceKeyId,
                        KrAssigneeMessage = item.GoalKeyId == sourceKeyId ? sourceStatusMessage?.KrAssigneeMessage : contriStatusMessage?.KrAssigneeMessage,
                        KrAssignerMessage = item.GoalKeyId == sourceKeyId ? sourceStatusMessage?.KrAssignerMessage : contriStatusMessage?.KrAssignerMessage,
                        CreatedOnAssignee = item.GoalKeyId == sourceKeyId ? sourceStatusMessage?.CreatedOnAssignee : contriStatusMessage?.CreatedOnAssignee,
                        CreatedOnAssigner = item.GoalKeyId == sourceKeyId ? sourceStatusMessage?.CreatedOnAssigner : contriStatusMessage?.CreatedOnAssigner,
                        LastLoginScore = contributorContribution - contributorlastLoginScore,
                        TeamId = teamDetails == null ? 0 : teamDetails.OrganisationId,
                        TeamName = teamName,
                        IsExternal = item.TeamId != itemUser?.OrganisationID,
                        IsSelfCreation = IsSelfCreation(item),
                        ColorCode = teamDetails == null ? "null" : teamDetails.ColorCode,
                        BackGroundColorCode = teamDetails == null ? "null" : teamDetails.BackGroundColorCode,
                        MetricId = item.MetricId,
                        CurrencyId = item.CurrencyId
                    });
                }
            }

            return contributorsResponse;
        }

        public async Task<List<GoalKey>> GetKRContributorAsync(int goalType, long goalId)
        {
            return await goalKeyRepo.GetQueryable().Where(x => x.ImportedType == goalType && x.ImportedId == goalId && x.IsActive).ToListAsync();
        }

        public List<ContributorsResponse> GetContributor(int goalType, long goalId, List<UserResponse> allEmployee, decimal sourceTargetValue = 0)
        {
            if (goalType == (int)GoalType.GoalObjective)
            {
                return GetObjectiveContributor(goalType, goalId, allEmployee);
            }
            else
            {
                return GetKRContributor(goalType, goalId, allEmployee, sourceTargetValue);
            }
        }
        public async Task<List<ContributorsResponse>> GetContributorAsync(int goalType, long goalId, List<UserResponse> allEmployee, decimal sourceTargetValue = 0)
        {
            if (goalType == (int)GoalType.GoalObjective)
            {
                return await GetObjectiveContributorAsync(goalType, goalId, allEmployee);
            }
            else
            {
                return await GetKRContributorAsync(goalType, goalId, allEmployee, sourceTargetValue);
            }
        }

        public List<ContributorPdfResponse> GetContributorForPdf(int goalType, long goalId, List<UserResponse> allEmployee)
        {
            return goalType == (int)GoalType.GoalObjective ? GetObjectiveContributorForPdf(goalType, goalId, allEmployee)
                : GetKRContributorForPdf(goalType, goalId, allEmployee);
        }

        public async Task<CycleLockDate> IsOkrLocked(DateTime startDate, DateTime dueDate, long empId, int cycleId, int year, string jwtToken)
        {
            CycleLockDate cycleLockDate = new CycleLockDate();
            var goalLockedDate = await GetGoalLockDateAsync(cycleId, jwtToken);
            int lockDuration = Convert.ToInt32(Configuration.GetSection("OkrLockDuration").Value);
            DateTime goalSubmitDate = goalLockedDate.Count != 0
                ? goalLockedDate.FirstOrDefault(x => x.Type == 1).SubmitDate
                : startDate.AddDays(lockDuration);
            DateTime scoreSubmitDate =
                goalLockedDate.Count != 0 && goalLockedDate.FirstOrDefault(x => x.Type == 2) != null
                    ? goalLockedDate.FirstOrDefault(x => x.Type == 2).SubmitDate
                    : dueDate.AddDays(lockDuration);
            cycleLockDate.GoalSubmitDate = goalSubmitDate.Date;
            if (goalSubmitDate.Date >= DateTime.UtcNow.Date)
            {
                cycleLockDate.IsGaolLocked = false;
            }
            else if (goalSubmitDate.Date < DateTime.UtcNow.Date)
            {
                var data = GetApproveUnlockLog(empId, cycleId, year, 2);
                if (data != null)
                {
                    cycleLockDate.IsGaolLocked = false;
                    cycleLockDate.IsScoreLocked = false;
                }
            }
            if (scoreSubmitDate.Date >= DateTime.UtcNow.Date)
            {
                cycleLockDate.IsScoreLocked = false;
            }

            return cycleLockDate;
        }

        public UnLockLog GetApproveUnlockLog(long empId, int cycleId, int year, int status)
        {
            return unlockLogRepo.GetQueryable().FirstOrDefault(x => x.IsActive && x.EmployeeId == empId && x.Cycle == cycleId && x.Year == year && x.Status == 2 && x.LockedTill > DateTime.UtcNow.Date);
        }

        public async Task<MailerTemplate> GetMailerTemplateAsync(string templateCode, string jwtToken = null)
        {
            MailerTemplate template = new MailerTemplate();
            HttpClient httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetSection("Notifications:BaseUrl").Value);
            var response = await httpClient.GetAsync($"api/v2/OkrNotifications/GetTemplate?templateCode=" + templateCode);
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<MailerTemplate>>(await response.Content.ReadAsStringAsync());
                template = payload.Entity;
            }

            return template;
        }

        public async Task<bool> SentMailAsync(MailRequest mailRequest, string jwtToken = null)
        {
            HttpClient httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetSection("Notifications:BaseUrl").Value);
            PayloadCustom<bool> payload = new PayloadCustom<bool>();
            var response = await httpClient.PostAsJsonAsync($"api/v2/OkrNotifications/SentMailAsync", mailRequest);
            if (response.IsSuccessStatusCode)
            {
                payload = JsonConvert.DeserializeObject<PayloadCustom<bool>>(await response.Content.ReadAsStringAsync());
            }

            return payload.IsSuccess;
        }

        public async Task SaveNotificationAsync(NotificationsRequest notificationsResponse, string jwtToken = null)
        {
            HttpClient httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetSection("Notifications:BaseUrl").Value);
            var response = await httpClient.PostAsJsonAsync($"api/v2/OkrNotifications/InsertNotificationsDetailsAsync", notificationsResponse);
            if (response.IsSuccessStatusCode)
                Console.Write("Success");

            else
                Console.Write("Error");
        }

        public void SaveLog(string pageName, string functionName, string errorDetail)
        {
            ErrorLog errorLog = new ErrorLog
            {
                PageName = pageName,
                FunctionName = functionName,
                ErrorDetail = errorDetail
            };
            errorLogRepo.Add(errorLog);
            UnitOfWorkAsync.SaveChanges();
        }

        public List<PassportEmployeeResponse> GetAllPassportUsers()
        {
            var employees = new List<PassportEmployeeResponse>();
            HttpClient httpClient = GetHttpClient(UserToken);
            var response = httpClient.GetAsync($"api/PassportSso/GetAllPassportUsers").Result;
            if (response.IsSuccessStatusCode)
            {
                var apirResponse = response.Content.ReadAsStringAsync().Result;
                var passportUsers = JsonConvert.DeserializeObject<PayloadCustom<List<PassportEmployeeResponse>>>(apirResponse);
                employees = passportUsers.Entity;
            }
            return employees;
        }

        public async Task<List<GoalUnlockDate>> GetGoalLockDateAsync(long cycleId, string jwtToken = null)
        {
            List<GoalUnlockDate> goalUnlockDates = new List<GoalUnlockDate>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.GetAsync($"api/User/GoalLockDate?organisationCycleId=" + cycleId);
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<GoalUnlockDate>>(await response.Content.ReadAsStringAsync());
                goalUnlockDates = payload.EntityList;
            }

            return goalUnlockDates;
        }

        public async Task SaveNotificationWithAuthentication(string jwtToken, NotificationsRequest notificationsResponse)
        {
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("Notifications:BaseUrl"));
                using var response = await httpClient.PostAsJsonAsync($"api/v2/OkrNotifications/InsertNotificationsDetailsAsync", notificationsResponse);
                Console.Write(response.IsSuccessStatusCode ? "Success" : "Error");
            }
        }

        public async Task<List<FeedbackResponse>> GetAllFeedback(string jwtToken, long employeeId)
        {
            var feedbackResponse = new List<FeedbackResponse>();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("Feedback:BaseUrl"));
                using var response = await httpClient.GetAsync($"api/Feedback/AllFeedbackByUser?employeeId=" + employeeId, HttpCompletionOption.ResponseHeadersRead);
                string apiResponse = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<PayloadCustom<FeedbackResponse>>(apiResponse);
                feedbackResponse = user.EntityList;
            }
            return feedbackResponse;
        }

        public async Task NotificationsAsync(NotificationsCommonRequest notificationsCommonRequest)
        {
            List<long> notificationTo = new List<long>();
            NotificationsRequest notificationsRequest = new NotificationsRequest();

            if (notificationsCommonRequest.NotificationToList == null)
            {
                notificationTo.Add(notificationsCommonRequest.To);
                notificationsRequest.To = notificationTo;
            }
            else
            {
                notificationsRequest.To = notificationsCommonRequest.NotificationToList;
            }
            notificationsRequest.By = notificationsCommonRequest.By;
            notificationsRequest.Url = notificationsCommonRequest.Url;
            notificationsRequest.Text = notificationsCommonRequest.NotificationText;
            notificationsRequest.AppId = notificationsCommonRequest.AppId;
            notificationsRequest.NotificationType = notificationsCommonRequest.NotificationType;
            notificationsRequest.MessageType = notificationsCommonRequest.MessageType;

            await (notificationsCommonRequest.JwtToken == null ? SaveNotificationWithoutAuthenticationAsync(notificationsRequest) : SaveNotificationAsync(notificationsRequest, notificationsCommonRequest.JwtToken));
        }

        public async Task SaveNotificationWithoutAuthenticationAsync(NotificationsRequest notificationsResponse, string jwtToken = null)
        {
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.PostAsJsonAsync($"api/OkrNotifications/InsertNotificationsDetailsAsync", notificationsResponse);
            if (response.IsSuccessStatusCode)
                Console.Write("Success");
            else
                Console.Write("Error");
        }

        public List<long> GetAllLevelContributors(int goalType, long goalId, long empId)
        {

            List<AllLevelObjectiveResponse> allLevelObjectiveResponseList = GetObjectiveSubCascading(goalId);

            List<long> contributorsList = allLevelObjectiveResponseList.Select(x => x.EmployeeId).ToList();

            return contributorsList;
        }

        public List<KrContributors> GetAllLevelKrContributors(int goalType, long goalId, long empId)
        {
            var AllLevelKrResponseList = GetKeySubCascading(goalId);

            List<KrContributors> contributorsList = (from kr in AllLevelKrResponseList
                                                     select new KrContributors
                                                     {
                                                         EmployeeId = kr.EmployeeId.Value,
                                                         GoalId = kr.GoalId,
                                                         StartDate = kr.StartDate,
                                                         DueDate = kr.DueDate,
                                                         Owner = kr.Owner,
                                                         UpdatedOn = kr.UpdatedOn,
                                                         UpdatedBy = kr.UpdatedBy,
                                                         KeyDescription = kr.KeyDescription,
                                                         Score = kr.Score,
                                                         ImportedType = kr.ImportedType,
                                                         CreatedBy = kr.CreatedBy,
                                                         CreatedOn = kr.CreatedOn,
                                                         Progress = kr.Progress,
                                                         Source = kr.Source,
                                                         MetricId = kr.MetricId,
                                                         AssignmentTypeId = kr.AssignmentTypeId,
                                                         CurrencyId = kr.CurrencyId,
                                                         CurrentValue = kr.CurrentValue,
                                                         TargetValue = kr.TargetValue,
                                                         KrStatusId = kr.KrStatusId,
                                                         CycleId = kr.CycleId,
                                                         CurrencyCode = kr.CurrencyCode,
                                                         GoalStatusId = kr.GoalStatusId,
                                                         ContributorValue = kr.ContributorValue,
                                                         StartValue = kr.StartValue,
                                                         KeyNotes = kr.KeyNotes,
                                                         TeamId = kr.TeamId,
                                                         ImportedId = kr.ImportedId,
                                                         GoalObjectiveId = kr.GoalobjectiveId,

                                                     }).ToList();
            return contributorsList;
        }

        public List<GoalObjective> GetObjectiveContributorDeleted(int goalType, long goalId)
        {
            return goalObjectiveRepo.GetQueryable().Where(x => x.ImportedType == goalType && x.ImportedId == goalId && !x.IsActive).ToList();
        }

        //TODO - Obj does not have any KR status.
        public async Task<AlignStatusResponse> GetGoalObjectiveSource(long employeeId, long sourceId)
        {
            var alignStatusResponse = new AlignStatusResponse()
            {
                AlignStatus = 0,
                IsAligned = false
            };
            if (sourceId == 0)
            {
                return alignStatusResponse;
            }
            var goalObjSource = await goalObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.ImportedType != 2 && x.EmployeeId == employeeId && x.IsActive && (x.Source == sourceId || x.GoalObjectiveId == sourceId));
            if (goalObjSource == null) return alignStatusResponse;
            alignStatusResponse.AlignStatus = (int)KrStatus.Pending;
            alignStatusResponse.IsAligned = true;

            return alignStatusResponse;
        }

        public async Task<AlignStatusResponse> GetGoalKeySource(long employeeId, long sourceId)
        {
            var alignStatusResponse = new AlignStatusResponse()
            {
                AlignStatus = 0,
                IsAligned = false
            };
            if (sourceId == 0)
            {
                return alignStatusResponse;
            }

            var goalKeySource = await goalKeyRepo.GetQueryable().FirstOrDefaultAsync(x => x.ImportedType != 1 && x.EmployeeId == employeeId && x.IsActive && (x.Source == sourceId || x.GoalKeyId == sourceId));
            if (goalKeySource == null) return alignStatusResponse;
            alignStatusResponse.AlignStatus = goalKeySource.KrStatusId;
            alignStatusResponse.IsAligned = true;

            return alignStatusResponse;
        }

        public GoalKeyAudit LatestUpdateGoalKey(long goalKeyId)
        {
            return goalKeyAuditRepo.GetQueryable().OrderByDescending(x => x.UpdatedOn).FirstOrDefault(x => x.UpdatedGoalKeyId == goalKeyId);
        }

        public string MinimumOrganisationThreshold()
        {
            string result = "";
            result = constantRepo.GetQueryable().FirstOrDefault(x => x.ConstantId == 1 && x.IsActive).ConstantValue;
            return result;
        }

        public DateTime DeltaProgressLastDay()
        {
            DateTime deltaDay = new DateTime();
            var result = constantRepo.GetQueryable().FirstOrDefault(x => x.ConstantName == "DeltaDays" && x.IsActive)?.ConstantValue;
            int data = Convert.ToInt32(result ?? "7");
            deltaDay = DateTime.UtcNow.AddDays(-data).Date;
            return deltaDay;
        }



        public int GetKrUpdates(long GoalKeyId)
        {
            return goalKeyAuditRepo.GetQueryable().Count(x => x.UpdatedGoalKeyId == GoalKeyId && x.UpdatedColumn == Constants.Score);
        }

        public long GetGoalkeyId(long goalobjective)
        {
            long goalKeyId = 0;
            var result = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalobjective);
            if (result != null)
                goalKeyId = result.GoalKeyId;

            return goalKeyId;
        }

        public int GetKrCount(long goalObjectiveId)
        {
            int result = 0;
            result = goalKeyRepo.GetQueryable().Count(x => x.GoalObjectiveId == goalObjectiveId);
            return result;
        }

        public int GetObjCount(long goalObjectiveId)
        {
            int result = 0;
            result = goalObjectiveRepo.GetQueryable().Count(x => x.GoalObjectiveId == goalObjectiveId);
            return result;
        }

        public DateTime GetDueDate(long goalkeyId)
        {
            return goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalkeyId).DueDate;
        }

        public async Task<OrganisationCycleResponse> GetCurrentCycleAsync(long orgId, string jwtToken = null)
        {
            var currentCycle = new OrganisationCycleResponse();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.GetAsync($"api/Organisation/GetCurrentCycle?organisationId=" + orgId);
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<OrganisationCycleResponse>>(await response.Content.ReadAsStringAsync());
                currentCycle = payload.Entity;
            }

            return currentCycle;
        }

        public async Task<OrganisationCycleResponse> GetCurrentCycleAsync(long orgId)
        {
            Logger.Information("GetCurrentCycleAsync called");
            var hasCurrentCycle = HttpContext.Request.Headers.TryGetValue("CurrentCycle", out var currentCycle);
            Logger.Information("is found the current cycle in  header-" + hasCurrentCycle);

            Logger.Information("Path - " + HttpContext.Request.Path);
            Logger.Information("Method - " + HttpContext.Request.Method);

            if (!hasCurrentCycle) return await GetCurrentCycleAsync(orgId, UserToken);
            Logger.Information("Value found the current cycle in  header-" + currentCycle);
            var decryptVal = Encryption.DecryptStringAes(currentCycle, Configuration.GetValue<string>("Encryption:SecretKey"),
                Configuration.GetValue<string>("Encryption:SecretIVKey"));
            var cycle = JsonConvert.DeserializeObject<OrganisationCycleResponse>(decryptVal);
            Logger.Information("Cycle Detail is received for org id" + orgId);
            return await Task.FromResult(cycle).ConfigureAwait(false);
        }

        public long GetSourceId(long importedId, int importedType)
        {
            var sourceGoalId = new long();

            if (importedType == (int)GoalRequest.KeyImportedType)
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

        public GoalKey GetGoalKeyDetails(long goalKeyId)
        {
            return goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalKeyId && x.IsActive);
        }

        public async Task UpdateNotificationText(UpdateNotificationURLRequest updateNotificationUrlRequest, string jwtToken = null)
        {
            HttpClient httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetSection("Notifications:BaseUrl").Value);
            var response = await httpClient.PutAsJsonAsync($"api/v2/OkrNotifications/UpdateNotificationURL", updateNotificationUrlRequest);
            if (response.IsSuccessStatusCode)
                Console.Write("Success");

            else
                Console.Write("Error");
        }

        public async Task<NotificationsDetails> GetNotifications(long id, string jwtToken = null)
        {
            NotificationsDetails details = new NotificationsDetails();
            HttpClient httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetSection("Notifications:BaseUrl").Value);
            var response = await httpClient.GetAsync($"api/v2/OkrNotifications/GetNotifications?id=" + id);
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<NotificationsDetails>>(await response.Content.ReadAsStringAsync());
                details = payload.Entity;
            }

            return details;
        }

        public TeamDetails GetTeamEmployeeByTeamId(long teamId, string jwtToken)
        {
            TeamDetails teamDetails = new TeamDetails();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var cacheKey = TenantId + Constants.TeamsById+ teamId;
            string serializedList;
            var redisList = _distributedCache.Get(cacheKey);
            if (redisList != null)
            {
                serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<TeamDetails>(serializedList);
                teamDetails = resDeserializeObject;
            }
            else
            {
                var response = httpClient.GetAsync($"api/Organisation/TeamDetailsById?teamId=" + teamId).Result;
                if (response.IsSuccessStatusCode)
                {
                    var payload = JsonConvert.DeserializeObject<PayloadCustom<TeamDetails>>(response.Content.ReadAsStringAsync().Result);
                    teamDetails = payload.Entity;

                    serializedList = JsonConvert.SerializeObject(payload.Entity);
                    redisList = Encoding.UTF8.GetBytes(serializedList);
                    var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(Configuration.GetValue<int>("Redis:ExpiryTime")))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(Configuration.GetValue<int>("Redis:SlidingExpireTime")));
                    _distributedCache.Set(cacheKey, redisList, options);
                }
            }
            return teamDetails;
        }

        public async Task<List<TeamDetails>> GetTeamEmployees()
        {
            var teamDetails = new List<TeamDetails>();
            var httpClient = GetHttpClient(UserToken);
            var cacheKey = TenantId + Constants.TeamsDetails;
            string serializedList;
            var redisList = await _distributedCache.GetAsync(cacheKey);
            if (redisList != null)
            {
                serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<List<TeamDetails>>(serializedList);
                teamDetails = resDeserializeObject;
            }
            else
            {
                var response = await httpClient.GetAsync($"api/Organisation/TeamEmployeesDetails");
                if (!response.IsSuccessStatusCode) return teamDetails;
                var payload = JsonConvert.DeserializeObject<PayloadCustom<TeamDetails>>(await response.Content.ReadAsStringAsync());
                teamDetails = payload.EntityList;

                serializedList = JsonConvert.SerializeObject(teamDetails);
                redisList = Encoding.UTF8.GetBytes(serializedList);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(Configuration.GetValue<int>("Redis:ExpiryTime")))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(Configuration.GetValue<int>("Redis:SlidingExpireTime")));
                await _distributedCache.SetAsync(cacheKey, redisList, options);
            }

            return teamDetails;
        }

        public async Task<OkrStatusMasterDetails> GetAllOkrFiltersAsync(long orgId, string jwtToken = null)
        {
            var okrStatus = new OkrStatusMasterDetails();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var cacheKey = TenantId + Constants.OkrFilters+ orgId;
            string serializedList;
            var redisList = await _distributedCache.GetAsync(cacheKey);
            if (redisList != null)
            {
                serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<OkrStatusMasterDetails>(serializedList);
                okrStatus = resDeserializeObject;
            }
            else
            {
                var response = await httpClient.GetAsync($"api/Master/GetAllOkrFilters?organisationId=" + orgId);
                if (response.IsSuccessStatusCode)
                {
                    var payload = JsonConvert.DeserializeObject<PayloadCustom<OkrStatusMasterDetails>>(await response.Content.ReadAsStringAsync());
                    okrStatus = payload.Entity;

                    serializedList = JsonConvert.SerializeObject(payload.Entity);
                    redisList = Encoding.UTF8.GetBytes(serializedList);
                    var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(Configuration.GetValue<int>("Redis:ExpiryTime")))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(Configuration.GetValue<int>("Redis:SlidingExpireTime")));
                    await _distributedCache.SetAsync(cacheKey, redisList, options);
                }

            }
            return okrStatus;
        }

        public async Task<List<LeaderDetailsResponse>> SearchUserAsync(string finder, string jwtToken = null)
        {
            var okrStatus = new List<LeaderDetailsResponse>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.GetAsync($"api/User/Search?finder=" + finder + "&page=" + 1 + "&pagesize=" + 99999);
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustomGenric<LeaderDetailsResponse>>(await response.Content.ReadAsStringAsync());
                okrStatus = payload.EntityList;
            }

            return okrStatus;
        }
        public async Task<List<TeamHeadDetails>> GetLeaderOrganizationsAsync(int goalType, string jwtToken = null, long empId = 0, bool isCoach = false)
        {
            var headOrganization = new List<TeamHeadDetails>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.GetAsync($"api/Organisation/TeamDetails?goalType=" + goalType + "&empId=" + empId + "&isCoach=" + isCoach);
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<TeamHeadDetails>>(await response.Content.ReadAsStringAsync());
                headOrganization = payload.EntityList;
            }

            return headOrganization;
        }

        public async Task<string> GetBase64ImagePath(string imagePath)
        {
            string strBase64 = "";
            if (!string.IsNullOrEmpty(imagePath))
            {
                HttpClient httpClient = new HttpClient();
                Uri url = new Uri(imagePath);
                var bytes = await httpClient.GetByteArrayAsync(url);
                strBase64 = "data:image/png;base64," + Convert.ToBase64String(bytes);

            }

            return strBase64;

        }

        public int GetProgressIdWithFormula(DateTime dueDate, DateTime cycleStartDate, DateTime cycleEndDate, decimal score, long cycleDurationId)
        {
            decimal timeLeft = 0;
            int progress;
            decimal getPercent = Math.Round(score);
            decimal targetNotAchieved = 100 - getPercent;
            var currentDate = DateTime.UtcNow;
            var okrDueDate = dueDate;
            var differenceInDays = okrDueDate - currentDate;
            var numberOfDays = cycleEndDate - cycleStartDate;

            if (cycleDurationId == Constants.Quarterly)
            {
                timeLeft = Math.Round(((decimal)differenceInDays.TotalDays / (decimal)numberOfDays.TotalDays) * 100);
            }
            else if (cycleDurationId == Constants.HalfYearly)
            {
                timeLeft = Math.Round(((decimal)differenceInDays.TotalDays / (decimal)numberOfDays.TotalDays) * 100);
            }
            else if (cycleDurationId == Constants.Annually)
            {
                timeLeft = Math.Round(((decimal)differenceInDays.TotalDays / (decimal)numberOfDays.TotalDays) * 100);
            }
            else if (cycleDurationId == Constants.ThreeYears)
            {
                timeLeft = Math.Round(((decimal)differenceInDays.TotalDays / (decimal)numberOfDays.TotalDays) * 100);
            }

            timeLeft = Math.Sign(timeLeft) == -1 ? 0 : timeLeft;
            var deviation = targetNotAchieved - timeLeft;

            progress = (int)Common.ProgressMaster.NotStarted;
            if (getPercent > 0)
            {
                if (deviation >= 20 && deviation <= 30)
                {
                    progress = (int)Common.ProgressMaster.Lagging;
                }
                else if (deviation > 30)
                {
                    progress = (int)Common.ProgressMaster.AtRisk;
                }
                else if (deviation < 20)
                {
                    progress = (int)Common.ProgressMaster.OnTrack;
                }
            }
            return progress;
        }

        public async Task<AllOkrMasterData> GetAllOkrMasterData(string jwtToken)
        {
            var metricResponse = new AllOkrMasterData();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                using var response = await httpClient.PostAsync($"api/Master/OkrMasterData", new StringContent(""));
                string apiResponse = await response.Content.ReadAsStringAsync();
                var okrMasterCustom = JsonConvert.DeserializeObject<PayloadCustom<AllOkrMasterData>>(apiResponse);
                metricResponse = okrMasterCustom.Entity;
            }
            return metricResponse;
        }

        public async Task<KrStatusMessage> UpdateKrStatus(long goalKeyId, string message)
        {
            var krStatusRecord = await krStatusMessageRepo.GetQueryable().FirstOrDefaultAsync(x => x.AssigneeGoalKeyId == goalKeyId && x.IsActive);
            if (krStatusRecord != null)
            {
                krStatusRecord.KrAssigneeMessage = message;
                krStatusRecord.CreatedOnAssignee = DateTime.UtcNow;

            }
            return krStatusRecord;
        }

        public List<KrContributors> GetAllLevelTeamKrContributors(int goalType, long goalId, long empId, long teamId)
        {
            var contributorsList = new List<KrContributors>();
            using (var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC sp_GetAllLevelContributors_WithTeam " + goalType + "," + goalId + "," + empId + "," + teamId;
                command.CommandType = CommandType.Text;
                OkrServiceDBContext.Database.OpenConnection();
                var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    KrContributors krContributors = new KrContributors
                    {
                        EmployeeId = Convert.ToInt64(dataReader["EmployeeId"].ToString()),
                        GoalId = Convert.ToInt64(dataReader["GoalId"].ToString())
                    };
                    contributorsList.Add(krContributors);
                }
                OkrServiceDBContext.Database.CloseConnection();
            }

            return contributorsList;
        }

        public List<long> GetAllLevelTeamContributors(int goalType, long goalId, long empId, long teamId)
        {
            var contributorsList = new List<long>();
            using (var command = OkrServiceDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC sp_GetAllLevelContributors_WithTeam " + goalType + "," + goalId + "," + empId + "," + teamId;
                command.CommandType = CommandType.Text;
                OkrServiceDBContext.Database.OpenConnection();
                var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    var employeeId = Convert.ToInt64(dataReader["EmployeeId"].ToString());
                    contributorsList.Add(employeeId);
                }
                OkrServiceDBContext.Database.CloseConnection();
            }

            return contributorsList;
        }

        public List<AllLevelObjectiveResponse> GetObjectiveSubCascading(long goalKeyId)
        {
            List<AllLevelObjectiveResponse> cteList = new List<AllLevelObjectiveResponse>();
            List<AllLevelObjectiveResponse> matches = new List<AllLevelObjectiveResponse>();

            var goalDetails = goalObjectiveRepo.GetQueryable().AsNoTracking().Where(x => x.GoalObjectiveId == goalKeyId && x.IsActive).ToList();
            if (goalDetails.Count > 0)
            {
                foreach (var itemResult in goalDetails)
                {
                    var goalKey = new AllLevelObjectiveResponse
                    {

                        GoalId = itemResult.GoalObjectiveId,
                        EmployeeId = itemResult.EmployeeId,
                        GoalImportedId = itemResult.ImportedId,
                        ObjLevel = 0,
                        ObjectiveName = itemResult.ObjectiveName,
                        ObjectiveDescription = itemResult.ObjectiveDescription,
                        Score = itemResult.Score,
                        Progress = itemResult.Progress,
                        StartDate = itemResult.StartDate,
                        EndDate = itemResult.EndDate,
                        IsPrivate = itemResult.IsPrivate,
                        Owner = itemResult.Owner,
                        LinkedObjectiveId = itemResult.LinkedObjectiveId,
                        UpdatedOn = itemResult.UpdatedOn,
                        UpdatedBy = itemResult.UpdatedBy,
                        ImportedType = itemResult.ImportedType,
                        IsActive = itemResult.IsActive,
                        ObjectiveCycleId = itemResult.ObjectiveCycleId,
                        CreatedBy = itemResult.CreatedBy,
                        CreatedOn = itemResult.CreatedOn,
                        Year = itemResult.Year,
                        Source = itemResult.Source,
                        Sequence = itemResult.Sequence,
                        GoalStatusId = itemResult.GoalStatusId,
                        GoalTypeId = itemResult.GoalTypeId,
                        TeamId = itemResult.TeamId,
                        IsCoachCreation = itemResult.IsCoachCreation,
                        ImportedId = itemResult.ImportedId
                    };
                    matches.Add(goalKey);
                }
            }

            if (matches.Any())
            {
                cteList.AddRange(ObjectiveTraverseSubs(matches));
            }

            return cteList;
        }

        public List<AllLevelObjectiveResponse> ObjectiveTraverseSubs(List<AllLevelObjectiveResponse> resultSet)
        {
            int objLevelCount = 1;
            List<AllLevelObjectiveResponse> compList = new List<AllLevelObjectiveResponse>();

            compList.AddRange(resultSet);

            List<AllLevelObjectiveResponse> childrenList = new List<AllLevelObjectiveResponse>();
            for (int i = 0; i < resultSet.Count; i++)
            {
                ////Get all subCompList of each                 

                var goalDetails = goalObjectiveRepo.GetQueryable().AsNoTracking().Where(x => x.ImportedId == resultSet[i].GoalId && x.IsActive).ToList();

                if (goalDetails.Count > 0)
                {
                    foreach (var itemResult in goalDetails)
                    {
                        var goalKey = new AllLevelObjectiveResponse
                        {
                            GoalId = itemResult.GoalObjectiveId,
                            EmployeeId = itemResult.EmployeeId,
                            GoalImportedId = itemResult.ImportedId,
                            ObjLevel = objLevelCount,
                            ObjectiveName = itemResult.ObjectiveName,
                            ObjectiveDescription = itemResult.ObjectiveDescription,
                            Score = itemResult.Score,
                            Progress = itemResult.Progress,
                            StartDate = itemResult.StartDate,
                            EndDate = itemResult.EndDate,
                            IsPrivate = itemResult.IsPrivate,
                            Owner = itemResult.Owner,
                            LinkedObjectiveId = itemResult.LinkedObjectiveId,
                            UpdatedOn = itemResult.UpdatedOn,
                            UpdatedBy = itemResult.UpdatedBy,
                            ImportedType = itemResult.ImportedType,
                            IsActive = itemResult.IsActive,
                            ObjectiveCycleId = itemResult.ObjectiveCycleId,
                            CreatedBy = itemResult.CreatedBy,
                            CreatedOn = itemResult.CreatedOn,
                            Year = itemResult.Year,
                            Source = itemResult.Source,
                            Sequence = itemResult.Sequence,
                            GoalStatusId = itemResult.GoalStatusId,
                            GoalTypeId = itemResult.GoalTypeId,
                            TeamId = itemResult.TeamId,
                            IsCoachCreation = itemResult.IsCoachCreation,
                            ImportedId = itemResult.ImportedId

                        };
                        childrenList.Add(goalKey);
                    }
                }

                if (childrenList.Any())
                {
                    objLevelCount = objLevelCount + 1;
                    compList.AddRange(ObjectiveTraverseSubs(childrenList));
                }
            }

            return compList;
        }

        public List<AllLevelKrResponse> GetKeySubCascading(long goalKeyId)
        {
            List<AllLevelKrResponse> cteList = new List<AllLevelKrResponse>();
            List<AllLevelKrResponse> matches = new List<AllLevelKrResponse>();

            var goalDetails = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId && x.IsActive).ToList();
            if (goalDetails.Count > 0)
            {
                foreach (var itemResult in goalDetails)
                {
                    var goalKey = new AllLevelKrResponse
                    {

                        GoalId = itemResult.GoalKeyId,
                        EmployeeId = itemResult.EmployeeId,
                        GoalImportedId = itemResult.ImportedId,
                        ObjLevel = 0,
                        StartDate = itemResult.StartDate,
                        DueDate = itemResult.DueDate,
                        Owner = itemResult.Owner,
                        GoalobjectiveId = itemResult.GoalObjectiveId,
                        UpdatedOn = itemResult.UpdatedOn,
                        UpdatedBy = itemResult.UpdatedBy,
                        KeyDescription = itemResult.KeyDescription,
                        Score = itemResult.Score,
                        ImportedType = itemResult.ImportedType,
                        CreatedBy = itemResult.CreatedBy,
                        CreatedOn = itemResult.CreatedOn,
                        Progress = itemResult.Progress,
                        Source = itemResult.Source,
                        MetricId = itemResult.MetricId,
                        AssignmentTypeId = itemResult.AssignmentTypeId,
                        CurrencyId = itemResult.CurrencyId,
                        CurrentValue = itemResult.CurrentValue,
                        TargetValue = itemResult.TargetValue,
                        KrStatusId = itemResult.KrStatusId,
                        CycleId = itemResult.CycleId,
                        CurrencyCode = itemResult.CurrencyCode,
                        GoalStatusId = itemResult.GoalStatusId,
                        ContributorValue = itemResult.ContributorValue,
                        StartValue = itemResult.StartValue,
                        KeyNotes = itemResult.KeyNotes,
                        TeamId = itemResult.TeamId,
                        ImportedId = itemResult.ImportedId
                    };
                    matches.Add(goalKey);
                }
            }

            if (matches.Any())
            {
                cteList.AddRange(KrTraverseSubs(matches));
            }

            return cteList;
        }

        public List<AllLevelKrResponse> KrTraverseSubs(List<AllLevelKrResponse> resultSet)
        {
            int objLevelCount = 1;
            List<AllLevelKrResponse> compList = new List<AllLevelKrResponse>();

            compList.AddRange(resultSet);

            List<AllLevelKrResponse> childrenList = new List<AllLevelKrResponse>();
            for (int i = 0; i < resultSet.Count; i++)
            {
                ////Get all subCompList of each                 

                var goalDetails = goalKeyRepo.GetQueryable().Where(x => x.ImportedId == resultSet[i].GoalId && x.IsActive).ToList();

                if (goalDetails.Count > 0)
                {
                    foreach (var itemResult in goalDetails)
                    {
                        var goalKey = new AllLevelKrResponse
                        {
                            GoalId = itemResult.GoalKeyId,
                            EmployeeId = itemResult.EmployeeId,
                            GoalImportedId = itemResult.ImportedId,
                            ObjLevel = objLevelCount,
                            StartDate = itemResult.StartDate,
                            DueDate = itemResult.DueDate,
                            Owner = itemResult.Owner,
                            GoalobjectiveId = itemResult.GoalObjectiveId,
                            UpdatedOn = itemResult.UpdatedOn,
                            UpdatedBy = itemResult.UpdatedBy,
                            KeyDescription = itemResult.KeyDescription,
                            Score = itemResult.Score,
                            ImportedType = itemResult.ImportedType,
                            CreatedBy = itemResult.CreatedBy,
                            CreatedOn = itemResult.CreatedOn,
                            Progress = itemResult.Progress,
                            Source = itemResult.Source,
                            MetricId = itemResult.MetricId,
                            AssignmentTypeId = itemResult.AssignmentTypeId,
                            CurrencyId = itemResult.CurrencyId,
                            CurrentValue = itemResult.CurrentValue,
                            TargetValue = itemResult.TargetValue,
                            KrStatusId = itemResult.KrStatusId,
                            CycleId = itemResult.CycleId,
                            CurrencyCode = itemResult.CurrencyCode,
                            GoalStatusId = itemResult.GoalStatusId,
                            ContributorValue = itemResult.ContributorValue,
                            StartValue = itemResult.StartValue,
                            KeyNotes = itemResult.KeyNotes,
                            TeamId = itemResult.TeamId,
                            ImportedId = itemResult.ImportedId
                        };
                        childrenList.Add(goalKey);
                    }
                }

                if (childrenList.Any())
                {
                    objLevelCount = objLevelCount + 1;
                    compList.AddRange(KrTraverseSubs(childrenList));
                }
            }

            return compList;
        }

        public List<DirectReportsDetails> GetDirectReportsById(long employeeId, string jwtToken)
        {
            var reportsDetails = new List<DirectReportsDetails>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = httpClient.GetAsync($"api/Organisation/DirectReportsById?employeeId=" + employeeId).Result;
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<DirectReportsDetails>>(response.Content.ReadAsStringAsync().Result);
                reportsDetails = payload.EntityList;
            }
            return reportsDetails;
        }

        public async Task<LastSevenDaysProgressResponse> GetLastSevenDaysProgress(long empId, long teamId, int cycle, bool isTeamOkrDashboard, UserIdentity identity, long teamLeaderEmployeeId, bool isOwner)
        {
            var progress = new LastSevenDaysProgressResponse();
            var result = new List<ScoringKey>();
            var goalKeyDetails = new List<GoalKey>();
            var goalKey = new List<GoalKeyHistory>();
            var goalKeyHistory = new List<GoalKeyHistory>();
            var distinctObjectives = new List<GoalObjective>();
            decimal avgScore;
            var timeDifference = DeltaProgressLastDay().Date;
            long ownerId;
            if (isTeamOkrDashboard)
            {
                goalKeyDetails = await goalKeyRepo.GetQueryable().Where(x => x.TeamId == teamId && x.IsActive && x.CycleId == cycle && x.ImportedId == 0 && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && x.CreatedOn.Date <= timeDifference.Date).ToListAsync();
                ownerId = teamLeaderEmployeeId;
            }
            else
            {
                goalKeyDetails = await goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == empId && x.IsActive && x.CycleId == cycle && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && x.CreatedOn.Date <= timeDifference.Date).ToListAsync();
                ownerId = identity.EmployeeId;
            }

            foreach (var item in goalKeyDetails)
            {
                ScoringKey scoringKey = new ScoringKey();
                ////var currentScore = item.Score;
                if (isOwner)
                {
                    ////goalKeyHistory = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == item.GoalKeyId && x.CreatedOn.Date == timeDifference.Date && x.CreatedBy == ownerId).OrderByDescending(x => x.CreatedOn).ToListAsync();
                    goalKeyHistory = await GetLastSevenUpdateScoreGoalKeyHistory(item.GoalKeyId, ownerId, true);
                }
                else
                {
                    //// goalKeyHistory = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == item.GoalKeyId && x.CreatedOn.Date == timeDifference.Date).OrderByDescending(x => x.CreatedOn).ToListAsync();
                    goalKeyHistory = await GetLastSevenUpdateScoreGoalKeyHistory(item.GoalKeyId, ownerId, false);
                }

                if (goalKeyHistory.Count > 0)
                {
                    var timeDifferenceScore = goalKeyHistory.First().Score; ////currentScore - goalKeyHistory.First().Score;
                    scoringKey.GoalObjectiveId = item.GoalObjectiveId;
                    scoringKey.GoalKeyId = item.GoalKeyId;
                    scoringKey.DeltaScore = timeDifferenceScore;
                    result.Add(scoringKey);
                    goalKey.Add(goalKeyHistory.First());
                }
                else
                {
                    var timeDifferenceScore = 0;
                    scoringKey.GoalObjectiveId = item.GoalObjectiveId;
                    scoringKey.GoalKeyId = item.GoalKeyId;
                    scoringKey.DeltaScore = timeDifferenceScore;
                    result.Add(scoringKey);
                }
            }

            var getKeyWithObjective = result.Where(x => x.GoalObjectiveId != 0).ToList();

            var distinctObjectivesId = getKeyWithObjective.Select(x => x.GoalObjectiveId).Distinct().ToList();
            foreach (var objectives in distinctObjectivesId)
            {
                var objectivesDetails = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == objectives && x.IsActive && x.ObjectiveCycleId == cycle && x.GoalStatusId == (int)GoalStatus.Public);
                if (objectivesDetails != null)
                {
                    var score = getKeyWithObjective.Where(x => x.GoalObjectiveId == objectives).Sum(x => x.DeltaScore);
                    objectivesDetails.Score =
                        score / getKeyWithObjective.Count(x => x.GoalObjectiveId == objectives);
                    distinctObjectives.Add(objectivesDetails);
                }

            }
            var standaloneKey = result.Where(x => x.GoalObjectiveId == 0).ToList();

            if (standaloneKey.Count > 0 && distinctObjectives.Count > 0)
            {
                var totalGoalCount = distinctObjectives.Count + standaloneKey.Count;
                var standaloneKeyAvgScore = standaloneKey.Select(x => x.DeltaScore).Sum();
                var okrAvgScore = distinctObjectives.Select(x => x.Score).Sum();
                avgScore = KeyScore((standaloneKeyAvgScore + okrAvgScore) / totalGoalCount);
            }
            else if (standaloneKey.Count > 0)
            {
                avgScore = standaloneKey.Select(x => x.DeltaScore).Average();
            }
            else
            {
                avgScore = distinctObjectives.Count > 0 ? distinctObjectives.Select(x => x.Score).Average() : 0;
            }


            progress.Score = KeyScore(avgScore);
            progress.GoalProgressTime = goalKey.Count > 0 ? goalKey.OrderByDescending(x => x.CreatedOn).Select(x => x.CreatedOn).First() : (DateTime?)null;
            return progress;
        }

        public decimal AvgScoreForProgressCard(List<ScoringKey> result, long cycle, int progressId)
        {
            var distinctObjectives = new List<GoalObjective>();
            decimal avgScore = 0.0M;
            result = result.Where(x => x.ProgressId == progressId).ToList();
            var getKeyWithObjective = result.Where(x => x.GoalObjectiveId != 0).ToList();

            var distinctObjectivesId = getKeyWithObjective.Select(x => x.GoalObjectiveId).Distinct().ToList();
            foreach (var objectives in distinctObjectivesId)
            {
                var objectivesDetails = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == objectives && x.IsActive && x.ObjectiveCycleId == cycle && x.GoalStatusId == (int)GoalStatus.Public);
                if (objectivesDetails != null)
                {
                    var score = getKeyWithObjective.Where(x => x.GoalObjectiveId == objectives).Sum(x => x.DeltaScore);
                    objectivesDetails.Score =
                        score / getKeyWithObjective.Count(x => x.GoalObjectiveId == objectives);
                    distinctObjectives.Add(objectivesDetails);
                }
            }
            var standaloneKey = result.Where(x => x.GoalObjectiveId == 0).ToList();
            ///Delta Score
            if (standaloneKey.Count > 0 && distinctObjectives.Count > 0)
            {
                var totalGoalCount = distinctObjectives.Count + standaloneKey.Count;
                var standaloneKeyAvgScore = standaloneKey.Select(x => x.DeltaScore).Sum();
                var okrAvgScore = distinctObjectives.Select(x => x.Score).Sum();
                avgScore = KeyScore((standaloneKeyAvgScore + okrAvgScore) / totalGoalCount);
            }
            else if (standaloneKey.Count > 0)
            {
                avgScore = standaloneKey.Select(x => x.DeltaScore).Average();
            }
            else
            {
                avgScore = distinctObjectives.Count > 0 ? distinctObjectives.Select(x => x.Score).Average() : 0;
            }


            return KeyScore(avgScore);
        }

        public decimal AvgCurrentScoreForProgressCard(List<ScoringKey> result, long cycle, int progressId)
        {
            var distinctObjectives = new List<GoalObjective>();
            decimal avgScore = 0.0M;
            result = result.Where(x => x.ProgressId == progressId).ToList();
            var getKeyWithObjective = result.Where(x => x.GoalObjectiveId != 0).ToList();

            var distinctObjectivesId = getKeyWithObjective.Select(x => x.GoalObjectiveId).Distinct().ToList();
            foreach (var objectives in distinctObjectivesId)
            {
                var objectivesDetails = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == objectives && x.IsActive && x.ObjectiveCycleId == cycle && x.GoalStatusId == (int)GoalStatus.Public);
                if (objectivesDetails != null)
                {
                    var score = getKeyWithObjective.Where(x => x.GoalObjectiveId == objectives).Sum(x => x.CurrentScore);
                    objectivesDetails.Score =
                        score / getKeyWithObjective.Count(x => x.GoalObjectiveId == objectives);
                    distinctObjectives.Add(objectivesDetails);
                }
            }
            var standaloneKey = result.Where(x => x.GoalObjectiveId == 0).ToList();
            ///Delta Score
            if (standaloneKey.Count > 0 && distinctObjectives.Count > 0)
            {
                var totalGoalCount = distinctObjectives.Count + standaloneKey.Count;
                var standaloneKeyAvgScore = standaloneKey.Select(x => x.CurrentScore).Sum();
                var okrAvgScore = distinctObjectives.Select(x => x.Score).Sum();
                avgScore = KeyScore((standaloneKeyAvgScore + okrAvgScore) / totalGoalCount);
            }
            else if (standaloneKey.Count > 0)
            {
                avgScore = standaloneKey.Select(x => x.CurrentScore).Average();
            }
            else
            {
                avgScore = distinctObjectives.Count > 0 ? distinctObjectives.Select(x => x.Score).Average() : 0;
            }



            return KeyScore(avgScore);
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

        public async Task<List<ContributorsLastSevenDaysProgressResponse>> GetContributorsLastUpdateSevenDays(long employeeId, long teamId, int cycleId, bool isTeamOkrDashboard, List<UserResponse> userResponses, UserIdentity identity, long teamLeaderEmployeeId, DateTime cycleEndDate)
        {
            var allEmployee = userResponses;
            var index = Constants.Zero;
            var contributorsKey = new List<ContributorsLastSevenDaysProgressResponse>();
            decimal avgScore;
            var selfKeyResultIds = new List<long>();
            var contributorsKeyResults = new List<long?>();
            var loginEmpKeyResultIds = new List<long>();
            var contributingKeyResults = new List<GoalKey>();
            var scoreDate = new LastSevenDaysProgressResponse();
            var lastDeltaDate = Convert.ToDateTime(DeltaProgressLastDay().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
            if (isTeamOkrDashboard)
            {
                selfKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.TeamId == teamId && x.IsActive && x.CycleId == cycleId && x.ImportedId == 0 && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
                contributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).Select(x => x.EmployeeId).Distinct().ToList();
                contributorsKeyResults?.Insert(index, employeeId);
            }
            else
            {
                selfKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == employeeId && x.IsActive && x.CycleId == cycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
                contributorsKeyResults = goalKeyRepo.GetQueryable().Where(x => x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && selfKeyResultIds.Contains(x.ImportedId)).Select(x => x.EmployeeId).Distinct().ToList();
                contributorsKeyResults?.Insert(index, employeeId);
            }

            var distinctContributorsKeyResults = contributorsKeyResults.Distinct().ToList();

            foreach (var item in distinctContributorsKeyResults)
            {
                if (allEmployee.Any(x => x.EmployeeId == item))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item)?.ImagePath;
                    if (isTeamOkrDashboard)
                    {
                        loginEmpKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.TeamId == teamId && x.IsActive && x.CycleId == cycleId && x.ImportedId == 0 && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
                    }
                    else
                    {
                        loginEmpKeyResultIds = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == employeeId && x.IsActive && x.CycleId == cycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToList();
                    }

                    if (item == employeeId)
                    {
                        contributingKeyResults = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == item && x.IsActive && x.CycleId == cycleId && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).ToList();
                        scoreDate = await GetLastSevenDaysProgress(Convert.ToInt64(item), Constants.ZeroId, cycleId, false, identity, Constants.ZeroId, false);
                    }
                    else
                    {
                        contributingKeyResults = goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == item && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && loginEmpKeyResultIds.Contains(x.ImportedId)).ToList();
                        scoreDate = await GetContributorLastSevenDaysProgress(Convert.ToInt64(item), Constants.ZeroId, cycleId, false, identity, teamLeaderEmployeeId, item == identity.EmployeeId || item == teamLeaderEmployeeId, employeeId);
                    }

                    var getKeyWithObjective = contributingKeyResults.Where(x => x.GoalObjectiveId != 0).ToList();
                    var distinctObjectivesId = getKeyWithObjective.Select(x => x.GoalObjectiveId).Distinct().ToList();
                    var distinctObjectives = new List<GoalObjective>();
                    DateTime? lastUpdateTime = new DateTime();
                    foreach (var objectives in distinctObjectivesId)
                    {
                        var objectivesDetails = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == objectives && x.IsActive && x.ObjectiveCycleId == cycleId && x.GoalStatusId == (int)GoalStatus.Public);
                        if (objectivesDetails != null)
                        {
                            distinctObjectives.Add(objectivesDetails);
                        }
                    }
                    var standaloneKey = contributingKeyResults.Where(x => x.GoalObjectiveId == 0).ToList();

                    if (standaloneKey.Count > 0 && distinctObjectives.Count > 0)
                    {
                        var totalGoalCount = distinctObjectives.Count + standaloneKey.Count;
                        var standaloneKeyAvgScore = standaloneKey.Select(x => x.Score).Sum();
                        var okrAvgScore = distinctObjectives.Select(x => x.Score).Sum();
                        avgScore = KeyScore((standaloneKeyAvgScore + okrAvgScore) / totalGoalCount);
                        lastUpdateTime = standaloneKey.OrderByDescending(x => x.UpdatedOn).FirstOrDefault().UpdatedOn > distinctObjectives.OrderByDescending(x => x.UpdatedOn).FirstOrDefault().UpdatedOn
                            ? standaloneKey.OrderByDescending(x => x.UpdatedOn).FirstOrDefault().UpdatedOn : distinctObjectives.OrderByDescending(x => x.UpdatedOn).FirstOrDefault().UpdatedOn;
                        ////if (lastUpdateTime == null)
                        ////{
                        ////    lastUpdateTime = standaloneKey.OrderByDescending(x => x.CreatedOn).FirstOrDefault().CreatedOn > distinctObjectives.OrderByDescending(x => x.CreatedOn).FirstOrDefault().CreatedOn
                        ////        ? standaloneKey.OrderByDescending(x => x.CreatedOn).FirstOrDefault().CreatedOn : distinctObjectives.OrderByDescending(x => x.CreatedOn).FirstOrDefault().CreatedOn;
                        ////}
                    }
                    else if (standaloneKey.Count > 0)
                    {
                        avgScore = standaloneKey.Select(x => x.Score).Average();
                        lastUpdateTime = standaloneKey.OrderByDescending(x => x.UpdatedOn).FirstOrDefault().UpdatedOn;
                        ////if (lastUpdateTime == null)
                        ////{
                        ////    lastUpdateTime = standaloneKey.OrderByDescending(x => x.CreatedOn).FirstOrDefault().CreatedOn;
                        ////}
                    }
                    else
                    {
                        avgScore = distinctObjectives.Count > 0 ? distinctObjectives.Select(x => x.Score).Average() : 0;
                        lastUpdateTime = distinctObjectives.Count > 0 ? distinctObjectives.OrderByDescending(x => x.UpdatedOn).FirstOrDefault().UpdatedOn : null;
                        ////if (lastUpdateTime == null && distinctObjectives.Count > 0)
                        ////{
                        ////    lastUpdateTime = distinctObjectives.OrderByDescending(x => x.CreatedOn).FirstOrDefault().CreatedOn;
                        ////}
                    }

                    var contributorCurrentScore = KeyScore(avgScore);
                    DateTime? updateonValue = (scoreDate.GoalProgressTime ?? lastUpdateTime) < lastDeltaDate.Date ? lastDeltaDate.Date : (((scoreDate.GoalProgressTime ?? lastUpdateTime) == null && scoreDate.Score == 0) ? null : scoreDate.GoalProgressTime ?? lastUpdateTime);
                    if (updateonValue == null)
                    {
                        updateonValue = lastDeltaDate.Date;
                    }
                    contributorsKey.Add(new ContributorsLastSevenDaysProgressResponse
                    {
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item)?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item)?.Designation,
                        OrganisationId = allEmployee.FirstOrDefault(x => x.EmployeeId == item)?.OrganisationID,
                        EmployeeId = allEmployee.FirstOrDefault(x => x.EmployeeId == item)?.EmployeeId,
                        ContributorsContribution = contributorCurrentScore - scoreDate.Score <= 0 ? 0 : contributorCurrentScore - scoreDate.Score,
                        UpdatedOn = updateonValue,
                        DueDate = cycleEndDate
                    });
                }
            }

            return contributorsKey;
        }

        public EmployeeOrganizationDetails GetOrganizationByEmployeeId(long employeeId, string jwtToken)
        {
            var employeeOrganizationDetails = new PayloadCustom<EmployeeOrganizationDetails>();
            using var httpClient = GetHttpClient(jwtToken);
            using var response = httpClient.GetAsync($"api/Organisation/OrganizationByEmployeeId?employeeId=" + employeeId).Result;
            if (response.IsSuccessStatusCode)
            {
                string apiResponse = response.Content.ReadAsStringAsync().Result;
                employeeOrganizationDetails = JsonConvert.DeserializeObject<PayloadCustom<EmployeeOrganizationDetails>>(apiResponse);

            }
            return employeeOrganizationDetails.Entity;
        }

        public async Task<LastSevenDaysProgressResponse> GetContributorLastSevenDaysProgress(long empId, long teamId, int cycle, bool isTeamOkrDashboard, UserIdentity identity, long teamLeaderEmployeeId, bool isOwner, long sourceEmpId)
        {
            var progress = new LastSevenDaysProgressResponse();
            var result = new List<ScoringKey>();
            var goalKeyDetails = new List<long>();
            var goalKey = new List<GoalKeyHistory>();
            var goalKeyHistory = new List<GoalKeyHistory>();
            var distinctObjectives = new List<GoalObjective>();
            decimal avgScore;
            var timeDifference = DeltaProgressLastDay().Date;
            long ownerId;
            if (isTeamOkrDashboard)
            {
                goalKeyDetails = await goalKeyRepo.GetQueryable().Where(x => x.TeamId == teamId && x.IsActive && x.CycleId == cycle && x.ImportedId == 0 && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToListAsync();
                ownerId = teamLeaderEmployeeId;
            }
            else
            {
                goalKeyDetails = await goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == sourceEmpId && x.IsActive && x.CycleId == cycle && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).Select(x => x.GoalKeyId).ToListAsync();
                ownerId = identity.EmployeeId;
            }

            var contributingKeyResults = await goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == empId && x.IsActive && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public && goalKeyDetails.Contains(x.ImportedId) && x.CreatedOn.Date <= timeDifference.Date).ToListAsync();

            foreach (var item in contributingKeyResults)
            {
                ScoringKey scoringKey = new ScoringKey();
                ////var currentScore = item.Score;
                if (isOwner)
                {
                    ////goalKeyHistory = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == item.GoalKeyId && x.CreatedOn.Date == timeDifference.Date && x.CreatedBy == ownerId).OrderByDescending(x => x.CreatedOn).ToListAsync();
                    goalKeyHistory = await GetLastSevenUpdateScoreGoalKeyHistory(item.GoalKeyId, ownerId, true);
                }
                else
                {
                    ////goalKeyHistory = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == item.GoalKeyId && x.CreatedOn.Date == timeDifference.Date).OrderByDescending(x => x.CreatedOn).ToListAsync();
                    goalKeyHistory = await GetLastSevenUpdateScoreGoalKeyHistory(item.GoalKeyId, ownerId, false);
                }

                if (goalKeyHistory.Count > 0)
                {
                    var timeDifferenceScore = goalKeyHistory.First().Score; ////currentScore - goalKeyHistory.First().Score;
                    scoringKey.GoalObjectiveId = item.GoalObjectiveId;
                    scoringKey.GoalKeyId = item.GoalKeyId;
                    scoringKey.DeltaScore = timeDifferenceScore;
                    result.Add(scoringKey);
                    goalKey.Add(goalKeyHistory.First());
                }
                else
                {
                    var timeDifferenceScore = 0;
                    scoringKey.GoalObjectiveId = item.GoalObjectiveId;
                    scoringKey.GoalKeyId = item.GoalKeyId;
                    scoringKey.DeltaScore = timeDifferenceScore;
                    result.Add(scoringKey);
                }
            }

            var getKeyWithObjective = result.Where(x => x.GoalObjectiveId != 0).ToList();

            var distinctObjectivesId = getKeyWithObjective.Select(x => x.GoalObjectiveId).Distinct().ToList();
            foreach (var objectives in distinctObjectivesId)
            {
                var objectivesDetails = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == objectives && x.IsActive && x.ObjectiveCycleId == cycle && x.GoalStatusId == (int)GoalStatus.Public);
                if (objectivesDetails != null)
                {
                    var score = getKeyWithObjective.Where(x => x.GoalObjectiveId == objectives).Sum(x => x.DeltaScore);
                    objectivesDetails.Score =
                        score / getKeyWithObjective.Count(x => x.GoalObjectiveId == objectives);
                    distinctObjectives.Add(objectivesDetails);
                }

            }
            var standaloneKey = result.Where(x => x.GoalObjectiveId == 0).ToList();

            if (standaloneKey.Count > 0 && distinctObjectives.Count > 0)
            {
                var totalGoalCount = distinctObjectives.Count + standaloneKey.Count;
                var standaloneKeyAvgScore = standaloneKey.Select(x => x.DeltaScore).Sum();
                var okrAvgScore = distinctObjectives.Select(x => x.Score).Sum();
                avgScore = KeyScore((standaloneKeyAvgScore + okrAvgScore) / totalGoalCount);
            }
            else if (standaloneKey.Count > 0)
            {
                avgScore = standaloneKey.Select(x => x.DeltaScore).Average();
            }
            else
            {
                avgScore = distinctObjectives.Count > 0 ? distinctObjectives.Select(x => x.Score).Average() : 0;
            }


            progress.Score = KeyScore(avgScore);
            progress.GoalProgressTime = goalKey.Count > 0 ? goalKey.OrderByDescending(x => x.CreatedOn).Select(x => x.CreatedOn).First() : (DateTime?)null;
            return progress;
        }

        public async Task<LastSevenDaysStatusCardProgress> GetLastSevenDaysStatusCardProgress(long empId, long teamId, int cycle, bool isTeamOkrDashboard, QuarterDetails quarterDetails, OrganisationCycleDetails cycleDurationDetails, UserIdentity identity, long teamLeaderEmployeeId)
        {
            var progress = new LastSevenDaysStatusCardProgress();
            var result = new List<ScoringKey>();
            var goalKeyDetails = new List<GoalKey>();
            var okrAtRisk = new List<decimal>();
            var okrLagging = new List<decimal>();
            var okrOnTrack = new List<decimal>();
            var timeDifferenceScore = 0.0M;
            var timeDifference = DeltaProgressLastDay().Date;
            long ownerId;
            var goalKeyHistory = new List<GoalKeyHistory>();
            if (isTeamOkrDashboard)
            {
                goalKeyDetails = await goalKeyRepo.GetQueryable().Where(x => x.TeamId == teamId && x.CreatedOn.Date <= timeDifference.Date && x.IsActive && x.CycleId == cycle && x.ImportedId == 0 && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).ToListAsync();
                ownerId = teamLeaderEmployeeId;
            }
            else
            {
                goalKeyDetails = await goalKeyRepo.GetQueryable().Where(x => x.EmployeeId == empId && x.CreatedOn.Date <= timeDifference.Date && x.IsActive && x.CycleId == cycle && x.KrStatusId == (int)KrStatus.Accepted && x.GoalStatusId == (int)GoalStatus.Public).ToListAsync();
                ownerId = identity.EmployeeId;
            }

            foreach (var item in goalKeyDetails)
            {
                ScoringKey scoringKey = new ScoringKey();
                var currentScore = item.Score;

                if (isTeamOkrDashboard)
                {
                    goalKeyHistory = await GetLastSevenUpdateScoreGoalKeyHistory(item.GoalKeyId, ownerId, false);
                }
                else
                {
                    goalKeyHistory = await GetLastSevenUpdateScoreGoalKeyHistory(item.GoalKeyId, ownerId, true);
                }
                var okrProgressId = item.Progress;
                if (goalKeyHistory.Count > 0)
                {
                    timeDifferenceScore = goalKeyHistory.First().Score;

                }
                else
                {
                    timeDifferenceScore = 0;
                }

                ////okrProgressId = GetProgressIdWithFormula(Convert.ToDateTime(quarterDetails.EndDate), Convert.ToDateTime(quarterDetails.StartDate), Convert.ToDateTime(quarterDetails.EndDate), timeDifferenceScore, cycleDurationDetails.CycleDurationId);

                if (okrProgressId == (int)ProgressMaster.AtRisk)
                {
                    scoringKey.GoalKeyId = item.GoalKeyId;
                    scoringKey.GoalObjectiveId = item.GoalObjectiveId;
                    scoringKey.DeltaScore = timeDifferenceScore;
                    scoringKey.ProgressId = okrProgressId;
                    scoringKey.CurrentScore = currentScore;
                    okrAtRisk.Add(timeDifferenceScore);

                }
                else if (okrProgressId == (int)ProgressMaster.Lagging)
                {
                    scoringKey.GoalKeyId = item.GoalKeyId;
                    scoringKey.GoalObjectiveId = item.GoalObjectiveId;
                    scoringKey.DeltaScore = timeDifferenceScore;
                    scoringKey.ProgressId = okrProgressId;
                    scoringKey.CurrentScore = currentScore;
                    okrLagging.Add(timeDifferenceScore);
                }
                else if (okrProgressId == (int)ProgressMaster.OnTrack)
                {
                    scoringKey.GoalKeyId = item.GoalKeyId;
                    scoringKey.GoalObjectiveId = item.GoalObjectiveId;
                    scoringKey.DeltaScore = timeDifferenceScore;
                    scoringKey.ProgressId = okrProgressId;
                    scoringKey.CurrentScore = currentScore;
                    okrOnTrack.Add(timeDifferenceScore);
                }
                result.Add(scoringKey);

            }

            progress.LastSevenDaysProgressAtRisk = AvgScoreForProgressCard(result, cycle, (int)ProgressMaster.AtRisk);////AvgCurrentScoreForProgressCard(result, cycle, (int)ProgressMaster.AtRisk) - AvgScoreForProgressCard(result, cycle, (int) ProgressMaster.AtRisk);//okrAtRisk.Count > 0 ? KeyScore(okrAtRisk.Average()) : 0;
            progress.LastSevenDaysProgressLagging = AvgScoreForProgressCard(result, cycle, (int)ProgressMaster.Lagging);////AvgCurrentScoreForProgressCard(result, cycle, (int)ProgressMaster.Lagging) - AvgScoreForProgressCard(result, cycle, (int)ProgressMaster.Lagging);//okrLagging.Count > 0 ? KeyScore(okrLagging.Average()) : 0;
            progress.LastSevenDaysProgressOnTrack = AvgScoreForProgressCard(result, cycle, (int)ProgressMaster.OnTrack);////AvgCurrentScoreForProgressCard(result, cycle, (int)ProgressMaster.OnTrack) - AvgScoreForProgressCard(result, cycle, (int)ProgressMaster.OnTrack);//okrOnTrack.Count > 0 ? KeyScore(okrOnTrack.Average()) : 0;

            return progress;
        }

        public async Task<List<GoalKeyHistory>> GetLastSevenUpdateScoreGoalKeyHistory(long goalKeyId, long ownerId, bool isOwner)
        {
            List<GoalKeyHistory> goalKeyHistoryList = new List<GoalKeyHistory>();


            var timeDifference = DeltaProgressLastDay().Date;
            if (isOwner)
            {
                goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId
                        && x.CreatedOn.Date == timeDifference.Date && x.CreatedBy == ownerId)
                    .OrderByDescending(x => x.CreatedOn).ToListAsync();

                if (goalKeyHistoryList.Count == 0)
                {
                    goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId
                            && x.CreatedOn.Date < timeDifference.Date && x.CreatedBy == ownerId && x.Score != 0).OrderByDescending(x => x.CreatedOn)
                        .ThenByDescending(x => x.HistoryId).ToListAsync();

                }
            }
            else
            {
                goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId && x.CreatedOn.Date == timeDifference.Date).OrderByDescending(x => x.CreatedOn).ToListAsync();

                if (goalKeyHistoryList.Count == 0)
                {
                    goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable()
                        .Where(x => x.GoalKeyId == goalKeyId && x.CreatedOn.Date < timeDifference.Date && x.Score != 0).OrderByDescending(x => x.CreatedOn).ThenByDescending(x => x.HistoryId).ToListAsync();
                }

            }



            return goalKeyHistoryList;

        }

        public async Task<decimal> GetGoalKeyRecentProgress(long goalKeyId, DateTime? lastLoginTime)
        {


            var timeDifference = lastLoginTime;
            var goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId
                       && x.CreatedOn == timeDifference)
                   .OrderByDescending(x => x.CreatedOn).ToListAsync();

            if (goalKeyHistoryList.Count == 0)
            {
                goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId
                        && x.CreatedOn < timeDifference && x.Score != 0).OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.HistoryId).ToListAsync();

            }

            return goalKeyHistoryList.Count > 0 ? goalKeyHistoryList.First().Score : 0;

        }

        public async Task<decimal> GetGoalKeyRecentContributorValue(long goalKeyId, DateTime? lastLoginTime)
        {


            var timeDifference = lastLoginTime;
            var goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId
                    && x.CreatedOn == timeDifference)
                .OrderByDescending(x => x.CreatedOn).ToListAsync();

            if (goalKeyHistoryList.Count == 0)
            {
                goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == goalKeyId
                        && x.CreatedOn < timeDifference && x.ContributorValue != 0).OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.HistoryId).ToListAsync();

            }

            return goalKeyHistoryList.Count > 0 ? goalKeyHistoryList.First().ContributorValue : 0;

        }

        public async Task<List<ContributorsLastSevenDaysProgressResponse>> GetContributorRecentProgress(long goalKeyId, DateTime? lastLoginTime, UserIdentity identity, EmployeeResult allEmployee)
        {
            List<ContributorsLastSevenDaysProgressResponse> result = new List<ContributorsLastSevenDaysProgressResponse>();
            var getContributorKr = await goalKeyRepo.GetQueryable().Where(x => x.ImportedId == goalKeyId && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted && x.IsActive).ToListAsync();
            var timeDifference = lastLoginTime;

            foreach (var key in getContributorKr)
            {
                var contributor = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == key.EmployeeId);
                ContributorsLastSevenDaysProgressResponse data = new ContributorsLastSevenDaysProgressResponse();
                var goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == key.GoalKeyId
                        && x.CreatedOn == timeDifference)
                    .OrderByDescending(x => x.CreatedOn).ToListAsync();



                if (key.MetricId == (int)MetricType.Boolean)
                {
                    if (goalKeyHistoryList.Count == 0)
                    {

                        goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == key.GoalKeyId
                                && x.CreatedOn > timeDifference).OrderByDescending(x => x.CreatedOn)
                            .ThenByDescending(x => x.HistoryId).ToListAsync();

                    }
                    data.ContributorsContribution = goalKeyHistoryList.Any()
                        ? goalKeyHistoryList.First().Score
                        : key.Score;
                }
                else
                {
                    if (goalKeyHistoryList.Count == 0)
                    {

                        goalKeyHistoryList = await goalKeyHistoryRepo.GetQueryable().Where(x => x.GoalKeyId == key.GoalKeyId
                                && x.CreatedOn < timeDifference && x.Score != 0).OrderByDescending(x => x.CreatedOn)
                            .ThenByDescending(x => x.HistoryId).ToListAsync();

                    }
                    data.ContributorsContribution = goalKeyHistoryList.Any()
                        ? key.Score - goalKeyHistoryList.First().Score
                        : key.Score;
                }

                data.EmployeeId = key.EmployeeId;
                if (contributor != null)
                {
                    data.OrganisationId = contributor.OrganisationID;
                    data.FirstName = contributor.FirstName;
                    data.LastName = contributor.LastName;
                    data.ImagePath = contributor.ImagePath;
                    data.Designation = contributor.Designation;
                }

                result.Add(data);
            }


            return result;

        }

        public async Task<bool> GetReadFeedbackResponse(long okrId, string jwtToken = null)
        {
            var isAnyFeedback = new bool();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("Feedback:BaseUrl"));
                using var response = await httpClient.GetAsync($"api/Feedback/ReadFeedbackResponse?okrId=" + okrId, HttpCompletionOption.ResponseHeadersRead);
                var payload = JsonConvert.DeserializeObject<PayloadCustom<bool>>(await response.Content.ReadAsStringAsync());
                isAnyFeedback = payload.Entity;
            }

            return isAnyFeedback;
        }

        public async Task<List<ActiveOrganisations>> GetAllActiveOrganisations(string jwtToken = null)
        {
            var activeOrganization = new List<ActiveOrganisations>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.GetAsync($"api/Organisation/GetAllActiveOrganisations");
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<ActiveOrganisations>>(await response.Content.ReadAsStringAsync());
                activeOrganization = payload.EntityList;
            }

            return activeOrganization;
        }

        public async Task<OnBoardingControlResponse> OnBoardingControlDetailById(string jwtToken = null)
        {
            var onBoardingControlResponse = new OnBoardingControlResponse();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("OnBoarding:BaseUrl"));
                using var response = await httpClient.GetAsync($"api/OnBoarding/OnBoardingControlDetailById");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<OnBoardingControlResponse>>(await response.Content.ReadAsStringAsync());
                onBoardingControlResponse = payload.Entity;
            }

            return onBoardingControlResponse;
        }

        public List<ContributorsResponse> GetDistinctObjContributor(List<UserResponse> allEmployee, long empId, List<AllOkrDashboardKeyResponse> keyResponse, bool isCoach, long ownerId)
        {
            var contributorsResponse = new List<ContributorsResponse>();
            var index = Constants.Zero;
            if (isCoach)
            {
                var ownerDetails = allEmployee.FirstOrDefault(x => x.EmployeeId == ownerId);
                var contri = new ContributorsResponse()
                {
                    EmployeeId = ownerDetails?.EmployeeId,
                    FirstName = ownerDetails?.FirstName,
                    LastName = ownerDetails?.LastName,
                    Designation = ownerDetails?.Designation,
                    ImagePath = ownerDetails?.ImagePath
                };
                contributorsResponse?.Insert(index, contri);
            }
            else
            {
                foreach (var item in keyResponse)
                {
                    var contributors = RemoveParentFromContributorList(item.Contributors, empId);
                    foreach (var contri in contributors)
                    {
                        if (contri.EmployeeId == empId && (contributorsResponse.Count == 0 || !contributorsResponse.Any(x => x.EmployeeId == contri.EmployeeId)))
                        {
                            contributorsResponse?.Insert(index, contri);
                        }
                        else if (contri.KrStatusId == (int)KrStatus.Accepted && contri.GoalStatusId == (int)GoalStatus.Public
                            && (contributorsResponse.Count == 0 || !contributorsResponse.Any(x => x.EmployeeId == contri.EmployeeId)))
                        {
                            contributorsResponse.Add(contri);
                        }
                    }
                }
            }
            return contributorsResponse;
        }

        public ParentTeamDetails ParentTeamDetails(int goalType, long goalId, List<TeamDetails> teamDetails, long teamId)
        {
            ParentTeamDetails parentTeamDetail = new ParentTeamDetails();
            long parentTeamId = 0;
            if (goalId != 0)
            {
                if (goalType == (int)GoalType.GoalObjective)
                {
                    var parentGoal = goalObjectiveRepo.GetQueryable()
                        .FirstOrDefault(x => x.GoalObjectiveId == goalId);
                    if (parentGoal != null) parentTeamId = parentGoal.TeamId;
                }
                else
                {
                    var parentGoal = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalId);
                    if (parentGoal != null) parentTeamId = parentGoal.TeamId;
                }

                if (teamId != parentTeamId)
                {
                    var parentTeam = teamDetails.FirstOrDefault(x => x.OrganisationId == parentTeamId);
                    if (parentTeam != null)
                    {
                        parentTeamDetail.TeamId = parentTeam.OrganisationId;
                        parentTeamDetail.TeamName = parentTeam.OrganisationName;
                        parentTeamDetail.ColorCode = parentTeam.ColorCode;
                        parentTeamDetail.BackGroundColorCode = parentTeam.BackGroundColorCode;
                        parentTeamDetail.ImagePath = parentTeam.ImagePath;
                    }
                }
            }

            return parentTeamDetail;
        }
        public async Task<int> GetKeyCount(long empId,int cycleId)
        {
            return await goalKeyRepo.GetQueryable().CountAsync(x => x.EmployeeId == empId && x.CycleId == cycleId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public && x.KrStatusId == (int)KrStatus.Accepted);
        }

        public async Task<int> GetOkrCount(long empId, int cycleId)
        {
            return await goalObjectiveRepo.GetQueryable().CountAsync(x => x.EmployeeId == empId && x.ObjectiveCycleId == cycleId && x.IsActive && x.GoalStatusId == (int)GoalStatus.Public);
        }
        #region Private Methods
        private List<ContributorsDotResponse> SeparationContributors_GetContributors(List<GoalObjective> contributors, List<UserResponse> allEmployee, long organisationId, string MinimumOrganisationThreshold)
        {
            List<ContributorsDotResponse> contributorsResponse = new List<ContributorsDotResponse>();
            foreach (var item in contributors)
            {
                if (allEmployee.Any(x => x.EmployeeId == item.EmployeeId))
                {
                    var imagePath = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.ImagePath;
                    var GoalkeyId = GetGoalkeyId(item.GoalObjectiveId);
                    contributorsResponse.Add(new ContributorsDotResponse
                    {
                        GoalObjectiveId = item.GoalObjectiveId,
                        EmployeeId = item.EmployeeId,
                        FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.FirstName,
                        LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.LastName,
                        ImagePath = imagePath != null ? imagePath.Trim() : imagePath,
                        Designation = allEmployee.FirstOrDefault(x => x.EmployeeId == item.EmployeeId)?.Designation,
                        Score = item.Score,
                        ScoreRange = item.Score < Convert.ToDecimal(MinimumOrganisationThreshold) ? Constants.Lowest : Constants.Highest,
                        Progress = item.Progress,
                        ObjectiveDescription = item.ObjectiveDescription,
                        ObjectiveName = item.ObjectiveName,
                        KrCount = GetKrCount(item.GoalObjectiveId),
                        OrganisationId = organisationId,
                        ObjCount = GetObjCount(item.GoalObjectiveId),
                        DueDate = GoalkeyId != 0 ? GetDueDate(GoalkeyId) : DateTime.UtcNow
                    });
                }
            }

            return contributorsResponse;
        }

        private bool IsSelfCreation(GoalKey goalKey)
        {
            bool isSelfCreated = true;

            if (goalKey.ImportedId > 0)
            {
                isSelfCreated = goalKey.CreatedBy == Convert.ToInt64(goalKey.EmployeeId);
            }

            return isSelfCreated;
        }

        private List<ContributorsResponse> RemoveParentFromContributorList(List<ContributorsResponse> contributorsResponses, long empId)
        {
            List<ContributorsResponse> item = new List<ContributorsResponse>(contributorsResponses);
            if (item.Count > 0 && item[0].EmployeeId != empId)
            {
                item.Remove(item[0]);
            }

            return item;
        }
        #endregion Private Methods
    }
}
