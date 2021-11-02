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
    public class PeopleControllerTestCases
    {
        private readonly Mock<IPeopleService> peopleService;
        private readonly Mock<ICommonService> commonService;
        private readonly PeopleController peopleController;

        public PeopleControllerTestCases()
        {
            peopleService = new Mock<IPeopleService>();
            commonService = new Mock<ICommonService>();
            peopleController = new PeopleController(peopleService.Object, commonService.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task EmployeeView_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await peopleController.EmployeeView(empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task EmployeeView_InvalidEmployeeId()
        {
            long empId = 0;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();

             commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await peopleController.EmployeeView(empId, cycle, year);
            PayloadCustom<PeopleResponse> requData = ((PayloadCustom<PeopleResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task EmployeeView_InvalidCycle()
        {
            long empId = 108;
            int cycle = 0;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();

             commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await peopleController.EmployeeView(empId, cycle, year);
            PayloadCustom<PeopleResponse> requData = ((PayloadCustom<PeopleResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task EmployeeView_InvalidYear()
        {
            long empId = 108;
            int cycle = 1;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

             commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await peopleController.EmployeeView(empId, cycle, year);
            PayloadCustom<PeopleResponse> requData = ((PayloadCustom<PeopleResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task EmployeeView_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            PeopleResponse peopleResponse = new PeopleResponse() { OkrCount = 1 };

             commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            peopleService.Setup(e => e.EmployeeView(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(peopleResponse);

            var result = await peopleController.EmployeeView(empId, cycle, year);
            PayloadCustom<PeopleResponse> requData = ((PayloadCustom<PeopleResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task EmployeeView_NotSuccess()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            PeopleResponse peopleResponse = new PeopleResponse();

             commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            peopleService.Setup(e => e.EmployeeView(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(peopleResponse);

            var result = await peopleController.EmployeeView(empId, cycle, year);
            PayloadCustom<PeopleResponse> requData = ((PayloadCustom<PeopleResponse>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task PeopleView_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            var searTexts = new List<string>();

            commonService.Setup(e => e.GetUserIdentity());

            var result = await peopleController.PeopleView(empId, cycle, year, searTexts) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }


        [Fact]
        public async Task PeopleView_InvalidEmployeeId()
        {
            long empId = 0;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            var searTexts = new List<string>();

             commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await peopleController.PeopleView(empId, cycle, year, searTexts);
            PayloadCustom<List<PeopleViewResponse>> requData = ((PayloadCustom<List<PeopleViewResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }


        [Fact]
        public async Task PeopleView_InvalidCycle()
        {
            long empId = 1;
            int cycle = 0;
            int year = 2020;
            var searTexts = new List<string>();
            UserIdentity userIdentity = new UserIdentity();

             commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await peopleController.PeopleView(empId, cycle, year,searTexts);
            PayloadCustom<List<PeopleViewResponse>> requData = ((PayloadCustom<List<PeopleViewResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }


        [Fact]
        public async Task PeopleView_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2020;
            var searTexts = new List<string>();
            UserIdentity userIdentity = new UserIdentity();
            userIdentity.OrganisationId = 1;
    
            var peopleViewRequest = new PeopleViewRequest()
            {
                EmployeeId = empId,
                CycleId = cycle,
                Year = year,
                IsNested = false,
                OrgId = userIdentity.OrganisationId,
               
                ActionLevel = 0,
                AllEmployee = new EmployeeResult(),
                CycleDetail = new CycleDetails(),
                OrganisationCycleDetails = null,
                ParentObjList = new List<ParentObjectiveResponse>(),
                SourceParentObjList = new List<long>(),
                PeopleViewResponse = new List<PeopleViewResponse>()
            };


            var people = new List<PeopleViewResponse> { new PeopleViewResponse {

                    EmployeeId = 108,
                    TeamId = 1
            
            } };
                

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            peopleService.Setup(e => e.AllPeopleViewResponse(peopleViewRequest, new List<string>())).ReturnsAsync(people);

            var result = await peopleController.PeopleView(empId, cycle, year, searTexts);
            PayloadCustom<List<PeopleViewResponse>> requData = ((PayloadCustom<List<PeopleViewResponse>>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }


        #region Private Methods

        private void SetUserClaimsAndRequest()
        {
            peopleController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            peopleController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };
            string sampleAuthToken = Guid.NewGuid().ToString();
            //peopleController.ControllerHeader = sampleAuthToken;
        }

        #endregion
    }
}
