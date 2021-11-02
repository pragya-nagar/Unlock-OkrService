using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using OKRService.WebCore.Controllers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace OKRService.UnitTests.Controllers
{
    public class DashboardControllerTestCases
    {
        private readonly Mock<IDashboardService> dashboardService;
        private readonly Mock<ICommonService> commonService;
        private readonly DashboardController dashboardController;

        public DashboardControllerTestCases()
        {
            dashboardService = new Mock<IDashboardService>();
            commonService = new Mock<ICommonService>();
            dashboardController = new DashboardController(dashboardService.Object, commonService.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task Archive_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.ArchiveDashboardAsync(empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Archive_Error()
        {
            long empId = 0;
            int cycle = 0;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await dashboardController.ArchiveDashboardAsync(empId, cycle, year);
            PayloadCustom<AllOkrDashboardResponse> reqData = ((PayloadCustom<AllOkrDashboardResponse>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task Archive_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity() { EmployeeId = 108 };
            AllOkrDashboardResponse dashboardResponse = new AllOkrDashboardResponse();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.ArchiveDashboardAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(dashboardResponse);

            var result = await dashboardController.ArchiveDashboardAsync(empId, cycle, year);
            PayloadCustom<AllOkrDashboardResponse> reqData = ((PayloadCustom<AllOkrDashboardResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task Archive_NotSuccess()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            AllOkrDashboardResponse dashboardResponse = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.ArchiveDashboardAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(dashboardResponse);

            var result = await dashboardController.ArchiveDashboardAsync(empId, cycle, year);
            PayloadCustom<AllOkrDashboardResponse> reqData = ((PayloadCustom<AllOkrDashboardResponse>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetGoalDetailById_InvalidToken()
        {
            long objId = 123;
            int cycle = 1;
            int year = 2020;

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.GetGoalDetailById(objId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetGoalDetailById_ValidToken()
        {
            long objId = 123;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            DashboardOkrResponse dashboardResponse = new DashboardOkrResponse() { GoalObjectiveId = 123 };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.GetGoalDetailById(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(dashboardResponse);

            var result = await dashboardController.GetGoalDetailById(objId, cycle, year);
            PayloadCustom<DashboardOkrResponse> reqData = ((PayloadCustom<DashboardOkrResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetGoalDetailById_NotSuccess()
        {
            long objId = 123;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            DashboardOkrResponse dashboardResponse = new DashboardOkrResponse();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.GetGoalDetailById(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(dashboardResponse);

            var result = await dashboardController.GetGoalDetailById(objId, cycle, year);
            PayloadCustom<DashboardOkrResponse> reqData = ((PayloadCustom<DashboardOkrResponse>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetGoalDetailById_Error()
        {
            long objId = 0;
            int cycle = 0;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await dashboardController.GetGoalDetailById(objId, cycle, year);
            PayloadCustom<DashboardOkrResponse> reqData = ((PayloadCustom<DashboardOkrResponse>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetGoalDetailById_Forbidden()
        {
            long objId = 10;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            GoalObjective goalObjective = new GoalObjective() { GoalObjectiveId = 10 };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.GetDeletedGoalObjective(It.IsAny<long>())).ReturnsAsync(goalObjective);

            var result = await dashboardController.GetGoalDetailById(objId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task GetTeamOkrCardDetails_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.GetTeamOkrCardDetails(empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetTeamOkrCardDetails_Error()
        {
            long empId = 0;
            int cycle = 0;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await dashboardController.GetTeamOkrCardDetails(empId, cycle, year);
            PayloadCustom<TeamOkrCardResponse> reqData = ((PayloadCustom<TeamOkrCardResponse>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetTeamOkrCardDetails_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            var teamOkrCardResponse = new List<TeamOkrCardResponse>() { new TeamOkrCardResponse() { TeamId = 1 } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.GetTeamOkrCardDetails(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(teamOkrCardResponse);

            var result = await dashboardController.GetTeamOkrCardDetails(empId, cycle, year);
            PayloadCustom<TeamOkrCardResponse> reqData = ((PayloadCustom<TeamOkrCardResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetTeamOkrCardDetails_NotSuccess()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            var teamOkrCardResponse = new List<TeamOkrCardResponse>();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.GetTeamOkrCardDetails(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(teamOkrCardResponse);

            var result = await dashboardController.GetTeamOkrCardDetails(empId, cycle, year);
            PayloadCustom<TeamOkrCardResponse> requData = ((PayloadCustom<TeamOkrCardResponse>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task NudgeTeamAsync_InvalidToken()
        {
            var nudgeTeam = new NudgeTeamRequest();

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.NudgeTeamAsync(nudgeTeam) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task NudgeTeamAsync_Error()
        {
            var nudgeTeam = new NudgeTeamRequest() { TeamId = 0, Cycle = 0, Year = 0 };
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await dashboardController.NudgeTeamAsync(nudgeTeam);
            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task NudgeTeamAsync_ValidToken()
        {
            var nudgeTeam = new NudgeTeamRequest() { TeamId = 1, Cycle = 1, Year = 1 };
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.NudgeTeamAsync(It.IsAny<NudgeTeamRequest>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(true);

            var result = await dashboardController.NudgeTeamAsync(nudgeTeam);
            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task NudgeTeamAsync_NotSuccess()
        {
            var nudgeTeam = new NudgeTeamRequest() { TeamId = 1, Cycle = 1, Year = 1 };
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.NudgeTeamAsync(It.IsAny<NudgeTeamRequest>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(false);

            var result = await dashboardController.NudgeTeamAsync(nudgeTeam);
            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetTeamOkrGoalDetailsById_InvalidToken()
        {
            long teamId = 1;
            long empId = 108;
            int cycle = 1;
            int year = 2020;

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.GetTeamOkrGoalDetailsById(teamId, empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetTeamOkrGoalDetailsById_Error()
        {
            long teamId = 0;
            long empId = 0;
            int cycle = 0;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await dashboardController.GetTeamOkrGoalDetailsById(teamId, empId, cycle, year);
            PayloadCustom<TeamOkrResponse> reqData = ((PayloadCustom<TeamOkrResponse>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetTeamOkrGoalDetailsById_ValidToken()
        {
            long teamId = 1;
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            TeamOkrResponse teamResponse = new TeamOkrResponse() { OkrCount = 1, MyGoalOkrResponses = new List<DashboardOkrKRResponse>() { new DashboardOkrKRResponse() { ObjectiveName = "Test" } } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.GetTeamOkrGoalDetailsById(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(teamResponse);

            var result = await dashboardController.GetTeamOkrGoalDetailsById(teamId, empId, cycle, year);
            PayloadCustom<TeamOkrResponse> reqData = ((PayloadCustom<TeamOkrResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetTeamOkrGoalDetailsById_NotSuccess()
        {
            long teamId = 1;
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            TeamOkrResponse teamResponse = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.GetTeamOkrGoalDetailsById(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(teamResponse);

            var result = await dashboardController.GetTeamOkrGoalDetailsById(teamId, empId, cycle, year);
            PayloadCustom<TeamOkrResponse> reqData = ((PayloadCustom<TeamOkrResponse>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateTeamOkrCardSequence_InvalidToken()
        {
            List<UpdateTeamSequenceRequest> updateTeamSequenceRequests = new List<UpdateTeamSequenceRequest>();

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.UpdateTeamOkrCardSequence(updateTeamSequenceRequests) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateTeamOkrCardSequence_ValidToken()
        {
            List<UpdateTeamSequenceRequest> updateTeamSequenceRequests = new List<UpdateTeamSequenceRequest>();
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.UpdateTeamOkrCardSequence(It.IsAny<List<UpdateTeamSequenceRequest>>(), It.IsAny<UserIdentity>())).ReturnsAsync(true);

            var result = await dashboardController.UpdateTeamOkrCardSequence(updateTeamSequenceRequests);
            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateTeamOkrCardSequence_NotSuccess()
        {
            List<UpdateTeamSequenceRequest> updateTeamSequenceRequests = new List<UpdateTeamSequenceRequest>();
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.UpdateTeamOkrCardSequence(It.IsAny<List<UpdateTeamSequenceRequest>>(), It.IsAny<UserIdentity>())).ReturnsAsync(false);

            var result = await dashboardController.UpdateTeamOkrCardSequence(updateTeamSequenceRequests);
            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task DirectReports_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;

            dashboardService.Setup(e => e.AllDirectReportsResponseAsync(It.IsAny<long>(), new List<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>(),It.IsAny<string>()));

            var result = await dashboardController.DirectReports(empId, new List<string>(), cycle, year,string.Empty) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Direct_ValidToken()
        {
            long empId = 79;
            int cycle = 671;
            int year = 2021;
            UserIdentity userIdentity = new UserIdentity();

            var directReportsResponse = new List<DirectReportsResponse>() {new DirectReportsResponse() {EmployeeId = 795}};

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.AllDirectReportsResponseAsync(It.IsAny<long>(), new List<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(directReportsResponse); ;

            var result = await dashboardController.DirectReports(empId, new List<string>(), cycle, year,string.Empty);
            PayloadCustom<DirectReportsResponse> reqData = ((PayloadCustom<DirectReportsResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task Direct_NotSuccess()
        {
            long empId = 0;
            int cycle = 0;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();
            var directReportsResponse = new List<DirectReportsResponse>();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.AllDirectReportsResponseAsync(It.IsAny<long>(), new List<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(directReportsResponse); ;

            var result = await dashboardController.DirectReports(empId, new List<string>(), cycle, year,string.Empty);
            PayloadCustom<DirectReportsResponse> reqData = ((PayloadCustom<DirectReportsResponse>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task AllOkrDashboardAsync_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.AllOkrDashboardAsync(empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AllOkrDashboardAsync_Error()
        {
            long empId = 0;
            int cycle = 0;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await dashboardController.AllOkrDashboardAsync(empId, cycle, year);
            PayloadCustom<AllOkrDashboardResponse> reqData = ((PayloadCustom<AllOkrDashboardResponse>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task AllOkrDashboardAsync_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            AllOkrDashboardResponse dashboardResponse = new AllOkrDashboardResponse();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.AllOkrDashboardAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(dashboardResponse);

            var result = await dashboardController.AllOkrDashboardAsync(empId, cycle, year);
            PayloadCustom<AllOkrDashboardResponse> reqData = ((PayloadCustom<AllOkrDashboardResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task AllOkrDashboardAsync_NotSuccess()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            AllOkrDashboardResponse dashboardResponse = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.AllOkrDashboardAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(dashboardResponse);

            var result = await dashboardController.AllOkrDashboardAsync(empId, cycle, year);
            PayloadCustom<AllOkrDashboardResponse> reqData = ((PayloadCustom<AllOkrDashboardResponse>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task TeamDetailsById_InvalidToken()
        {
            long teamId = 407;
            int sourceId = 123;
            int goalKeyId = 12354;

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.TeamDetailsById(teamId, sourceId, goalKeyId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }
        [Fact]
        public async Task TeamDetailsById_Error()
        {
            long teamId = 407;
            int sourceId = 123;
            int goalKeyId = 12354;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await dashboardController.TeamDetailsById(teamId, sourceId, goalKeyId);
            PayloadCustom<TeamDetails> reqData = ((PayloadCustom<TeamDetails>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }
        [Fact]
        public async Task TeamDetailsById_ValidToken()
        {
            long teamId = 407;
            int sourceId = 123;
            int goalKeyId = 12354;
            UserIdentity userIdentity = new UserIdentity();
            TeamDetails dashboardResponse = new TeamDetails();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.TeamDetailsById(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(dashboardResponse);

            var result = await dashboardController.TeamDetailsById(teamId, sourceId, goalKeyId);
            PayloadCustom<TeamDetails> reqData = ((PayloadCustom<TeamDetails>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }
        [Fact]
        public async Task TeamDetailsById_NotSuccess()
        {
            long teamId = 407;
            int sourceId = 123;
            int goalKeyId = 12354;
            UserIdentity userIdentity = new UserIdentity();
            TeamDetails teamDetails = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            dashboardService.Setup(e => e.TeamDetailsById(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(teamDetails);

            var result = await dashboardController.TeamDetailsById(teamId, sourceId, goalKeyId);
            PayloadCustom<TeamDetails> reqData = ((PayloadCustom<TeamDetails>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }
        [Fact]
        public async Task DeltaScore_NotSuccess()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            DeltaResponse deltaResponse = null;
            EmployeeResult employeeResult = new EmployeeResult() { Results = new List<UserResponse>() { new UserResponse { EmployeeId = 108 } } };
            OrganisationCycleDetails organisationCycleDetails = new OrganisationCycleDetails() { CycleDetails = new List<CycleDetails>() { new CycleDetails() { Year = "2020" , QuarterDetails = new List<QuarterDetails>() { new QuarterDetails() { OrganisationCycleId = 1 } } } }  };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            commonService.Setup(e => e.GetAllUserFromUsers(It.IsAny<string>())).Returns(employeeResult);
            commonService.Setup(e => e.GetOrganisationCycleDurationId(It.IsAny<long>(), It.IsAny<string>())).Returns(organisationCycleDetails);
            dashboardService.Setup(e => e.DeltaScore(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>(), It.IsAny<EmployeeResult>(), It.IsAny<QuarterDetails>(), It.IsAny<OrganisationCycleDetails>())).ReturnsAsync(deltaResponse);

            var result = await dashboardController.DeltaScore(empId, cycle, year);
            PayloadCustom<DeltaResponse> reqData = ((PayloadCustom<DeltaResponse>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task DeltaScore_Success()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity() { EmployeeId = 108 };
            DeltaResponse deltaResponse = new DeltaResponse(){ LastSevenDaysProgress = 10, AtRisk = 10 };
            EmployeeResult employeeResult = new EmployeeResult() { Results = new List<UserResponse>() { new UserResponse { EmployeeId = 108 } } };
            OrganisationCycleDetails organisationCycleDetails = new OrganisationCycleDetails() { CycleDetails = new List<CycleDetails>() { new CycleDetails() { Year = "2020", QuarterDetails = new List<QuarterDetails>() { new QuarterDetails() { OrganisationCycleId = 1 } } } } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            commonService.Setup(e => e.GetAllUserFromUsers(It.IsAny<string>())).Returns(employeeResult);
            commonService.Setup(e => e.GetOrganisationCycleDurationId(It.IsAny<long>(), It.IsAny<string>())).Returns(organisationCycleDetails);
            dashboardService.Setup(e => e.DeltaScore(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>(), It.IsAny<EmployeeResult>(), It.IsAny<QuarterDetails>(), It.IsAny<OrganisationCycleDetails>())).ReturnsAsync(deltaResponse);

            var result = await dashboardController.DeltaScore(empId, cycle, year);
            PayloadCustom<DeltaResponse> reqData = ((PayloadCustom<DeltaResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task DeltaScore_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await dashboardController.DeltaScore(empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeltaScore_Error()
        {
            long empId = 0;
            int cycle = 0;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();
            EmployeeResult employeeResult = new EmployeeResult() { Results = new List<UserResponse>() { new UserResponse { EmployeeId = 106 } } };
            OrganisationCycleDetails organisationCycleDetails = new OrganisationCycleDetails() { CycleDetails = new List<CycleDetails>() { new CycleDetails() { Year = "2020", QuarterDetails = new List<QuarterDetails>() { new QuarterDetails() { OrganisationCycleId = 2 } } } } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            commonService.Setup(e => e.GetAllUserFromUsers(It.IsAny<string>())).Returns(employeeResult);
            commonService.Setup(e => e.GetOrganisationCycleDurationId(It.IsAny<long>(), It.IsAny<string>())).Returns(organisationCycleDetails);

            var result = await dashboardController.DeltaScore(empId, cycle, year);
            PayloadCustom<DeltaResponse> reqData = ((PayloadCustom<DeltaResponse>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task DeltaScore_TokenError()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity() { EmployeeId = 106};
            EmployeeResult employeeResult = new EmployeeResult() { Results = new List<UserResponse>() { new UserResponse { EmployeeId = 108 } } };
            OrganisationCycleDetails organisationCycleDetails = new OrganisationCycleDetails() { CycleDetails = new List<CycleDetails>() { new CycleDetails() { Year = "2020", QuarterDetails = new List<QuarterDetails>() { new QuarterDetails() { OrganisationCycleId = 1 } } } } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            commonService.Setup(e => e.GetAllUserFromUsers(It.IsAny<string>())).Returns(employeeResult);
            commonService.Setup(e => e.GetOrganisationCycleDurationId(It.IsAny<long>(), It.IsAny<string>())).Returns(organisationCycleDetails);

            var result = await dashboardController.DeltaScore(empId, cycle, year);
            PayloadCustom<DeltaResponse> reqData = ((PayloadCustom<DeltaResponse>)((ObjectResult)result).Value);
            var finalData = reqData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        #region Private Methods

        private void SetUserClaimsAndRequest()
        {
            dashboardController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            dashboardController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };
            string sampleAuthToken = Guid.NewGuid().ToString();
            //dashboardController.ControllerHeader = sampleAuthToken;
        }

        #endregion
    }
}
