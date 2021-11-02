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
    public class AlignmentControllerTestCases
    {
        private readonly Mock<IAlignmentService> alignmentService;
        private readonly Mock<ICommonService> commonService;
        private readonly AlignmentController alignmentController;

        public AlignmentControllerTestCases()
        {
            alignmentService = new Mock<IAlignmentService>();
            commonService = new Mock<ICommonService>();
            alignmentController = new AlignmentController(alignmentService.Object, commonService.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task AllOkrViewResponse_InvalidToken()
        {
            long empId = 108;
            int cycle = 1;
            bool isTeams = false;
            long teamId = 1;
            int year = 2020;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await alignmentController.OkrView(empId,new List<string>(), cycle,year, isTeams,teamId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AllOkrViewResponse_InvalidEmployeeId()
        {
            long empId = 0;
            int cycle = 1;
            bool isTeams = false;
            long teamId = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            AllOkrViewResponse allOkrViewResponse = new AllOkrViewResponse();

            var result = await alignmentController.OkrView(empId,new List<string>(), cycle, year, isTeams,teamId);
            PayloadCustom<OkrViewResponse> requData = ((PayloadCustom<OkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task AllOkrViewResponse_InvalidCycle()
        {
            long empId = 108;
            int cycle = 0;
            bool isTeams = false;
            long teamId = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            AllOkrViewResponse allOkrViewResponse = new AllOkrViewResponse();

            var result = await alignmentController.OkrView(empId,new List<string>(), cycle, year,isTeams,teamId);
            PayloadCustom<OkrViewResponse> requData = ((PayloadCustom<OkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task AllOkrViewResponse_InvalidYear()
        {
            long empId = 108;
            int cycle = 1;
            bool isTeams = false;
            long teamId = 1;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await alignmentController.OkrView(empId,new List<string>(), cycle, year,isTeams,teamId);
            PayloadCustom<OkrViewResponse> requData = ((PayloadCustom<OkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task AllOkrViewResponse_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2021;
            bool isTeams = false;
            long teamId = 1;
            UserIdentity userIdentity = new UserIdentity();

           var OkrViewResponses = new List<OkrViewResponse>
                {
                   new OkrViewResponse
                   {
                       TeamId = 1,
                       EmployeeId = 108

                   }
                };

        
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            alignmentService.Setup(e => e.OkrViewAllLevelResponseAsync(It.IsAny<long>(),new List<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),It.IsAny<long>(), It.IsAny<string>(),It.IsAny<UserIdentity>())).ReturnsAsync(OkrViewResponses);

            var result = await alignmentController.OkrView(empId,new List<string>(), cycle, year,isTeams,teamId);
            PayloadCustom<OkrViewResponse> requData = ((PayloadCustom<OkrViewResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }



        [Fact]
        public async Task TeamsOkr_InvalidEmployeeId()
        {
            long empId = 0;
            int cycle = 1;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            var result = await alignmentController.TeamsOkr(empId, new List<string>(), cycle, year);
            PayloadCustom<AllTeamOkrViewResponse> requData = ((PayloadCustom<AllTeamOkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task TeamsOkr_InvalidCycle()
        {
            long empId = 108;
            int cycle = 0;
            int year = 2020;
            UserIdentity userIdentity = new UserIdentity();
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            var result = await alignmentController.TeamsOkr(empId, new List<string>(), cycle, year);
            PayloadCustom<AllTeamOkrViewResponse> requData = ((PayloadCustom<AllTeamOkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task TeamsOkr_InvalidYear()
        {
            long empId = 108;
            int cycle = 1;
            int year = 0;
            UserIdentity userIdentity = new UserIdentity();
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            var result = await alignmentController.TeamsOkr(empId,new List<string>(), cycle, year);
            PayloadCustom<AllTeamOkrViewResponse> requData = ((PayloadCustom<AllTeamOkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }


        [Fact]
        public async Task TeamsOkr_ValidToken()
        {
            long empId = 108;
            int cycle = 1;
            int year = 2021;
            UserIdentity userIdentity = new UserIdentity();

            var TeamViewResponses = new List<AllTeamOkrViewResponse>
                {
                   new AllTeamOkrViewResponse
                   {
                       TeamId = 1,
                       EmployeeId = 108

                   }
                };
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            alignmentService.Setup(e => e.AllTeamOkr(It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(TeamViewResponses);

            var result = await alignmentController.TeamsOkr(empId,new List<string>(), cycle, year);
            PayloadCustom<AllTeamOkrViewResponse> requData = ((PayloadCustom<AllTeamOkrViewResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }




        //[Fact]
        //public async Task AssociateContributorsResponse_InvalidToken()
        //{
        //    long objId = 1;
        //    int cycle = 1;
        //    int objType = 1;
        //    int year = 2020;


        //    commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

        //    var result = await alignmentController.AssociateContributorsResponse(objId, objType, cycle, year) as StatusCodeResult;
        //    Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        //}

        //[Fact]
        //public async Task AssociateContributorsResponse_InvalidObjId()
        //{
        //    long objId = 0;
        //    int cycle = 1;
        //    int objType = 1;
        //    int year = 2020;
        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AssociateContributorsResponse(objId, objType, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}


        //[Fact]
        //public async Task AssociateSourceResponse_InvalidObjId()
        //{
        //    long objId = 0;
        //    int cycle = 1;
        //    int objType = 1;
        //    int year = 2020;
        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AssociateSourceResponse(objId, objType, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}


        //[Fact]
        //public async Task AssociateContributorsResponse_InvalidCycle()
        //{
        //    long objId = 1;
        //    int cycle = 0;
        //    int objType = 1;
        //    int year = 2020;
        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AssociateContributorsResponse(objId, objType, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}

        //[Fact]
        //public async Task AssociateSourceResponse_InvalidCycle()
        //{
        //    long objId = 1;
        //    int cycle = 0;
        //    int objType = 1;
        //    int year = 2020;
        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AssociateSourceResponse(objId, objType, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}

        //[Fact]
        //public async Task AssociateContributorsResponse_InvalidYear()
        //{
        //    long objId = 1;
        //    int cycle = 1;
        //    int objType = 1;
        //    int year = 0;

        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AssociateContributorsResponse(objId, objType, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}


        //[Fact]
        //public async Task AssociateSourceResponse_InvalidYear()
        //{
        //    long objId = 1;
        //    int cycle = 1;
        //    int objType = 1;
        //    int year = 0;

        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AssociateSourceResponse(objId, objType, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}


        //[Fact]
        //public async Task AssociateContributorsResponse_ValidToken()
        //{
        //    long objId = 1;
        //    int cycle = 1;
        //    int objType = 1;
        //    int year = 2020;

        //    UserIdentity userIdentity = new UserIdentity();
        //    AllOkrViewResponse allOkrViewResponse = new AllOkrViewResponse()
        //    {
        //        OkrViewResponses = new List<OkrViewResponse>
        //        {
        //           new OkrViewResponse
        //           {
        //               TeamId = 1,
        //               EmployeeId = 2021

        //           }
        //        }

        //    };

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
        //     alignmentService.Setup(e => e.AssociateContributorsResponseAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(allOkrViewResponse);

        //    var result = await alignmentController.AssociateContributorsResponse(objId, objType, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    Assert.True(requData.IsSuccess);
        //}


        //[Fact]
        //public async Task AssociateSourceResponse_ValidToken()
        //{
        //    long objId = 1;
        //    int cycle = 1;
        //    int objType = 1;
        //    int year = 2020;

        //    UserIdentity userIdentity = new UserIdentity();
        //    AllOkrViewResponse allOkrViewResponse = new AllOkrViewResponse()
        //    {
        //        OkrViewResponses = new List<OkrViewResponse>
        //        {
        //           new OkrViewResponse
        //           {
        //               TeamId = 1,
        //               EmployeeId = 2021

        //           }
        //        }

        //    };

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
        //    alignmentService.Setup(e => e.AssociateSourceResponseAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(allOkrViewResponse);

        //    var result = await alignmentController.AssociateSourceResponse(objId, objType, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    Assert.True(requData.IsSuccess);
        //}



        [Fact]
        public async Task AllLevelOkrViewResponse_InvalidToken()
        {
            long empId = 1;
            int cycle = 1;
            bool isTeams = false;
            int year = 2020;
            long teamId = 1;

            commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

            var result = await alignmentController.OkrView(empId,new List<string>(), cycle, year,isTeams,teamId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AllLevelOkrViewResponse_InvalidEmpId()
        {
            long empId = 0;
            int cycle = 1;
            bool isTeams = false;
            int year = 2020;
            long teamId = 1;

            UserIdentity userIdentity = new UserIdentity();
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
            var result = await alignmentController.OkrView(empId,new List<string>(), cycle, year, isTeams,teamId);
            PayloadCustom<OkrViewResponse> requData = ((PayloadCustom<OkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task AllLevelOkrViewResponse_InvalidCycle()
        {
            long empId = 1;
            int cycle = 0;
            bool isTeams = false;
            int year = 2020;
            long teamId = 1;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await alignmentController.OkrView(empId,new List<string>(), cycle, year, isTeams,teamId);
            PayloadCustom<OkrViewResponse> requData = ((PayloadCustom<OkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task AllLevelOkrViewResponse_InvalidYear()
        {
            long empId = 1;
            int cycle = 1;
            bool isTeams = false;
            int year = 0;
            long teamId = 1;
            UserIdentity userIdentity = new UserIdentity();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

            var result = await alignmentController.OkrView(empId,new List<string>(), cycle, year,isTeams,teamId);
            PayloadCustom<OkrViewResponse> requData = ((PayloadCustom<OkrViewResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }


        //[Fact]
        //public async Task AllLevelOkrViewResponse_ValidToken()
        //{
        //    long empId = 1;
        //    int cycle = 1;

        //    int year = 1;
        //    UserIdentity userIdentity = new UserIdentity();
        //    AllOkrViewResponse allOkrViewResponse = new AllOkrViewResponse()
        //    {
        //        OkrViewResponses = new List<OkrViewResponse>
        //        {
        //           new OkrViewResponse
        //           {
        //               TeamId = 1,
        //               EmployeeId = 2021

        //           }
        //        }

        //    };

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
        //    alignmentService.Setup(e => e.OkrViewAllLevelNestedResponseAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(allOkrViewResponse);

        //    var result = await alignmentController.AllLevelOkrViewResponse(empId, cycle, year);
        //    PayloadCustom<AllOkrViewResponse> requData = ((PayloadCustom<AllOkrViewResponse>)((ObjectResult)result).Value);
        //    Assert.True(requData.IsSuccess);
        //}

        //[Fact]
        //public async Task AlignmentGoalMap_InvalidToken()
        //{
        //    long empId = 1;
        //    int cycle = 1;
        //    long orgId = 24;
        //    int year = 2020;

        //    commonService.Setup(e => e.GetUserIdentity(It.IsAny<string>()));

        //    var result = await alignmentController.AlignmentGoalMap(empId, cycle, orgId, year) as StatusCodeResult;
        //    Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        //}

        //[Fact]
        //public async Task AlignmentGoalMap_InvalidEmpId()
        //{
        //    long empId = 0;
        //    int cycle = 1;
        //    long orgId = 24;
        //    int year = 2020;
        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AlignmentGoalMap(empId, cycle, orgId, year);
        //    PayloadCustom<AlignmentResponse> requData = ((PayloadCustom<AlignmentResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}

        //[Fact]
        //public async Task AlignmentGoalMap_InvalidCycle()
        //{
        //    long empId = 1;
        //    int cycle = 0;
        //    long orgId = 24;
        //    int year = 2020;
        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AlignmentGoalMap(empId, cycle, orgId, year);
        //    PayloadCustom<AlignmentResponse> requData = ((PayloadCustom<AlignmentResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}

        //[Fact]
        //public async Task AlignmentGoalMap_InvalidYear()
        //{
        //    long empId = 1;
        //    int cycle = 1;
        //    long orgId = 24;
        //    int year = 0;
        //    UserIdentity userIdentity = new UserIdentity();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);

        //    var result = await alignmentController.AlignmentGoalMap(empId, cycle, orgId, year);
        //    PayloadCustom<AlignmentResponse> requData = ((PayloadCustom<AlignmentResponse>)((ObjectResult)result).Value);
        //    var finalData = requData.Status;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        //}

        //[Fact]
        //public async Task AlignmentGoalMap_ValidToken()
        //{
        //    long objId = 1;
        //    int cycle = 1;
        //    long orgId = 24;
        //    int year = 2020;
        //    UserIdentity userIdentity = new UserIdentity();
        //    AlignmentResponse alignmentResponse = new AlignmentResponse();

        //    commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(userIdentity);
        //    alignmentService.Setup(e => e.AlignmentMapResponse(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(alignmentResponse);

        //    var result = await alignmentController.AlignmentGoalMap(objId, cycle, orgId, year);
        //    PayloadCustom<AlignmentResponse> requData = ((PayloadCustom<AlignmentResponse>)((ObjectResult)result).Value);
        //    Assert.True(requData.IsSuccess);
        //}

        #region Private Methods

        private void SetUserClaimsAndRequest()
        {
            alignmentController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            alignmentController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };
            string sampleAuthToken = Guid.NewGuid().ToString();
            //alignmentController.ControllerHeader = sampleAuthToken;
        }

        #endregion
    }
}


