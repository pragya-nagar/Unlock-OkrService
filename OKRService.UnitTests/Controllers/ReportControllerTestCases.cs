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
    public class ReportControllerTestCases
    {
        private readonly Mock<IReportService> reportService;
        private readonly Mock<ICommonService> commonService;
        private readonly ReportController reportController;

        public ReportControllerTestCases()
        {
            reportService = new Mock<IReportService>();
            commonService = new Mock<ICommonService>();
            reportController = new ReportController(reportService.Object, commonService.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task MostLeastOkrRisk_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            long orgId = 24;
            int year = 2020;

           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.MostLeastOkrRisk(empId, cycle, orgId, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task MostLeastOkrRisk_InvalidEmployeeId()
        {
            long empId = 0;
            int cycle = 1;
            long orgId = 24;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await reportController.MostLeastOkrRisk(empId, cycle, orgId, year);
            PayloadCustom<List<ReportMostLeastObjective>> requData = ((PayloadCustom<List<ReportMostLeastObjective>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task MostLeastOkrRisk_InvalidCycle()
        {
            long empId = 108;
            int cycle = 0;
            long orgId = 24;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await reportController.MostLeastOkrRisk(empId, cycle, orgId, year);
            PayloadCustom<List<ReportMostLeastObjective>> requData = ((PayloadCustom<List<ReportMostLeastObjective>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task MostLeastOkrRisk_InvalidYear()
        {
            long empId = 108;
            int cycle = 1;
            long orgId = 24;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await reportController.MostLeastOkrRisk(empId, cycle, orgId, year);
            PayloadCustom<List<ReportMostLeastObjective>> requData = ((PayloadCustom<List<ReportMostLeastObjective>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task MostLeastOkrRisk_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            long orgId = 24;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<ReportMostLeastObjective> reportMostLeastObjective = new List<ReportMostLeastObjective>();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.ReportMostLeastObjective(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).Returns(reportMostLeastObjective);

            var result = await reportController.MostLeastOkrRisk(empId, cycle, orgId, year);
            PayloadCustom<List<ReportMostLeastObjective>> requData = ((PayloadCustom<List<ReportMostLeastObjective>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task MostLeastOkrKeyResultRisk_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            long orgId = 24;
            int year = 2020;

           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.MostLeastOkrKeyResultRisk(empId, cycle, orgId, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task MostLeastOkrKeyResultRisk_InvalidEmployeeId()
        {
            long empId = 0;
            int cycle = 1;
            long orgId = 24;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await reportController.MostLeastOkrKeyResultRisk(empId, cycle, orgId, year);
            PayloadCustom<List<ReportMostLeastObjectiveKeyResult>> requData = ((PayloadCustom<List<ReportMostLeastObjectiveKeyResult>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task MostLeastOkrKeyResultRisk_InvalidCycle()
        {
            long empId = 108;
            int cycle = 0;
            long orgId = 24;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await reportController.MostLeastOkrKeyResultRisk(empId, cycle, orgId, year);
            PayloadCustom<List<ReportMostLeastObjectiveKeyResult>> requData = ((PayloadCustom<List<ReportMostLeastObjectiveKeyResult>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task MostLeastOkrKeyResultRisk_InvalidYear()
        {
            long empId = 108;
            int cycle = 1;
            long orgId = 24;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await reportController.MostLeastOkrKeyResultRisk(empId, cycle, orgId, year);
            PayloadCustom<List<ReportMostLeastObjectiveKeyResult>> requData = ((PayloadCustom<List<ReportMostLeastObjectiveKeyResult>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task MostLeastOkrKeyResultRisk_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            long orgId = 24;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<ReportMostLeastObjectiveKeyResult> reportMostLeastObjective = new List<ReportMostLeastObjectiveKeyResult>();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.ReportMostLeastObjectiveKeyResult(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).Returns(reportMostLeastObjective);

            var result = await reportController.MostLeastOkrKeyResultRisk(empId, cycle, orgId, year);
            PayloadCustom<List<ReportMostLeastObjectiveKeyResult>> requData = ((PayloadCustom<List<ReportMostLeastObjectiveKeyResult>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAvgOkrScoreReport_InvalidToken()
        {
            long empId = 108;
            int cycleId = 1;
            int year = 2020;

           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.GetAvgOkrScoreReport(empId, cycleId, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAvgOkrScoreReport_ValidToken()
        {
            long empId = 108;
            int cycleId = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            AvgOkrScoreResponse avgOkrScoreResponse = new AvgOkrScoreResponse();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.GetAvgOkrScoreReport(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(avgOkrScoreResponse);

            var result = await reportController.GetAvgOkrScoreReport(empId, cycleId, year);
            PayloadCustom<AvgOkrScoreResponse> requData = ((PayloadCustom<AvgOkrScoreResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetWeeklyKrUpdates_InvalidToken()
        {
            long empId = 108;
            int cycleId = 1;
            int year = 2020;

           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.GetWeeklyKrUpdates(empId, cycleId, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetWeeklyKrUpdates_ValidToken()
        {
            long empId = 108;
            int cycleId = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<UserGoalKeyResponse> userGoalKeyResponse = new List<UserGoalKeyResponse>();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.GetWeeklyKrUpdatesReport(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(userGoalKeyResponse);

            var result = await reportController.GetWeeklyKrUpdates(empId, cycleId, year);
            PayloadCustom<UserGoalKeyResponse> requData = ((PayloadCustom<UserGoalKeyResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetWeeklyReport_InvalidToken()
        {
            long empId = 108;
            int cycleId = 1;
            int year = 2020;

           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.GetWeeklyReport(empId, cycleId, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetWeeklyReport_ValidToken()
        {
            long empId = 108;
            int cycleId = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            WeeklyReportResponse weeklyReportResponse = new WeeklyReportResponse();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.WeeklyReportResponse(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(weeklyReportResponse);

            var result = await reportController.GetWeeklyReport(empId, cycleId, year);
            PayloadCustom<WeeklyReportResponse> requData = ((PayloadCustom<WeeklyReportResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task ProgressReport_InvalidToken()
        {
            int cycle = 1;
            int year = 2020;

           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.ProgressReport(cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ProgressReport_InvalidCycle()
        {
            int cycle = 0;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<ProgressReportResponse> progressReportResponses = new List<ProgressReportResponse>() { new ProgressReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.ProgressReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(progressReportResponses);

            var result = await reportController.ProgressReport(cycle, year);
            PayloadCustom<List<ProgressReportResponse>> requData = ((PayloadCustom<List<ProgressReportResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ProgressReport_InvalidYear()
        {
            int cycle = 1;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();
            List<ProgressReportResponse> progressReportResponses = new List<ProgressReportResponse>() { new ProgressReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.ProgressReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(progressReportResponses);

            var result = await reportController.ProgressReport(cycle, year);
            PayloadCustom<List<ProgressReportResponse>> requData = ((PayloadCustom<List<ProgressReportResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ProgressReport_ValidToken()
        {
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<ProgressReportResponse> progressReportResponses = new List<ProgressReportResponse>() { new ProgressReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity); 
            reportService.Setup(e => e.ProgressReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(progressReportResponses);

            var result = await reportController.ProgressReport(cycle, year);
            PayloadCustom<List<ProgressReportResponse>> requData = ((PayloadCustom<List<ProgressReportResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task QuarterReport_InvalidToken()
        {
            int cycle = 1;
            int year = 2020;

           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.QuarterReport(cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task QuarterReport_InvalidCycle()
        {
            int cycle = 0;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<QuarterReportResponse> quarterReportResponses = new List<QuarterReportResponse>() { new QuarterReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.QuarterReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).Returns(quarterReportResponses);

            var result = await reportController.QuarterReport(cycle, year);
            PayloadCustom<List<QuarterReportResponse>> requData = ((PayloadCustom<List<QuarterReportResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task QuarterReport_InvalidYear()
        {
            int cycle = 1;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();
            List<QuarterReportResponse> quarterReportResponses = new List<QuarterReportResponse>() { new QuarterReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.QuarterReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).Returns(quarterReportResponses);

            var result = await reportController.QuarterReport(cycle, year);
            PayloadCustom<List<QuarterReportResponse>> requData = ((PayloadCustom<List<QuarterReportResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task QuarterReport_ValidToken()
        {
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<QuarterReportResponse> quarterReportResponses = new List<QuarterReportResponse>() { new QuarterReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.QuarterReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).Returns(quarterReportResponses);

            var result = await reportController.QuarterReport(cycle, year);
            PayloadCustom<List<QuarterReportResponse>> requData = ((PayloadCustom<List<QuarterReportResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task StatusReport_InvalidToken()
        {
            int cycle = 1;
            int year = 2020;

           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.StatusReport(cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task StatusReport_InvalidCycle()
        {
            int cycle = 0;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<StatusReportResponse> statusReportResponses = new List<StatusReportResponse>() { new StatusReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.StatusReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).Returns(statusReportResponses);

            var result = await reportController.StatusReport(cycle, year);
            PayloadCustom<List<StatusReportResponse>> requData = ((PayloadCustom<List<StatusReportResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task StatusReport_InvalidYear()
        {
            int cycle = 1;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();
            List<StatusReportResponse> statusReportResponses = new List<StatusReportResponse>() { new StatusReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.StatusReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).Returns(statusReportResponses);

            var result = await reportController.StatusReport(cycle, year);
            PayloadCustom<List<StatusReportResponse>> requData = ((PayloadCustom<List<StatusReportResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task StatusReport_ValidToken()
        {
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            List<StatusReportResponse> statusReportResponses = new List<StatusReportResponse>() { new StatusReportResponse() { EmployeeId = 1, FirstName = "xyz" } };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.StatusReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).Returns(statusReportResponses);

            var result = await reportController.StatusReport(cycle, year);
            PayloadCustom<List<StatusReportResponse>> requData = ((PayloadCustom<List<StatusReportResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }              

        [Fact]
        public async Task TeamPerformance_InvalidToken()
        {
            long empId = 108;
            int cycleId = 1;
            int year = 2020;
           commonService.Setup(e => e.GetUserIdentity());

            var result = await reportController.TeamPerformance(empId, cycleId, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task TeamPerformance_ValidToken()
        {
            long empId = 108;
            int cycleId = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            TeamPerformanceResponse teamPerformanceResponse = new TeamPerformanceResponse();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            reportService.Setup(e => e.TeamPerformance(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(teamPerformanceResponse);

            var result = await reportController.TeamPerformance(empId, cycleId, year);
            PayloadCustom<TeamPerformanceResponse> requData = ((PayloadCustom<TeamPerformanceResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        #region Private Methods

        private void SetUserClaimsAndRequest()
        {
            reportController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            reportController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };
            string sampleAuthToken = Guid.NewGuid().ToString();
            //reportController.ControllerHeader = sampleAuthToken;
        }

        #endregion
    }
}
