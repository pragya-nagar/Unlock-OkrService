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
    public class MyGoalsControllerUnitTest
    {
        private readonly Mock<IMyGoalsService> myGoalService;
        private readonly Mock<ICommonService> commonService;
        private readonly Mock<IProgressBarCalculationService> progressBarCalculationService;
        private readonly MyGoalsController myGoalsController;

        public MyGoalsControllerUnitTest()
        {
            myGoalService = new Mock<IMyGoalsService>();
            commonService = new Mock<ICommonService>();
            progressBarCalculationService = new Mock<IProgressBarCalculationService>();
            myGoalsController = new MyGoalsController(myGoalService.Object, commonService.Object, progressBarCalculationService.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task InsertGoalObjective_DraftIsSuccessful()
        {
            var contributors = new List<ContributorDetails>
            {
                new ContributorDetails
                {
                    EmployeeId = 795,
                    AssignmentTypeId = 3,
                    KeyResult = "Test",
                    Score = 0,
                    StartDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    CurrentValue = 0,
                    TargetValue = 0
                }
            };

            var myGoalDetailsList = new List<MyGoalsDetails> { new MyGoalsDetails
            {
                GoalKeyId = 1,
                KeyDescription = "Test KeyResult",
                Score = 0,
                DueDate = DateTime.Now,
                ImportedId = 0,
                ImportedType = 0, 
                Progress = 1,
                Source = 29,
                Contributors = contributors,
                MetricId = 5,
                CurrencyId= 0,
                CurrentValue = 0,
                TargetValue = 1000

            }};

            var myGoalRequestList = new List<MyGoalsRequest> { new MyGoalsRequest
            {
                GoalObjectiveId = 1,
                EmployeeId = 14823,
                Year = 2020,
                IsPrivate = false,
                ObjectiveName = "Test Objective",
                ObjectiveDescription = "Test Objective",
                ObjectiveCycleId = 32,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                MyGoalsDetails = myGoalDetailsList,
                GoalStatusId = 1

            }};

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.InsertGoalObjective(It.IsAny<List<MyGoalsRequest>>(), identity, It.IsAny<string>())).ReturnsAsync(myGoalRequestList);

            var result = await myGoalsController.InsertGoalObjective(myGoalRequestList);

            PayloadCustom<MyGoalsRequest> reqData = ((PayloadCustom<MyGoalsRequest>)((ObjectResult)result).Value);

            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task InsertGoalObjective_Error()
        {
            var contributors = new List<ContributorDetails>();
            var myGoalDetailsList = new List<MyGoalsDetails> { new MyGoalsDetails
            {
                GoalKeyId = 1,
                KeyDescription = "",
                Score = 0,
                DueDate = DateTime.Now,
                ImportedId = 0,
                ImportedType = 0,
                Contributors = contributors
            }};
            var myGoalRequestList = new List<MyGoalsRequest> { new MyGoalsRequest
            {
                GoalObjectiveId = 1,
                EmployeeId = 14823,
                Year = 2020,
                IsPrivate = false,
                ObjectiveName = "",
                ObjectiveDescription = "Test Objective",
                ObjectiveCycleId = 32,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                MyGoalsDetails = myGoalDetailsList,
                GoalStatusId = 2
            }};

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.InsertGoalObjective(It.IsAny<List<MyGoalsRequest>>(), identity, It.IsAny<string>())).ReturnsAsync(myGoalRequestList);

            var result = await myGoalsController.InsertGoalObjective(myGoalRequestList);
            PayloadCustom<MyGoalsRequest> reqData = ((PayloadCustom<MyGoalsRequest>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task InsertGoalObjective_IsSuccessful()
        {
            var contributors = new List<ContributorDetails>
            {
                new ContributorDetails
                {
                    EmployeeId = 795,
                    AssignmentTypeId = 3,
                    KeyResult = "Test",
                    Score = 0,
                    StartDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    CurrentValue = 0,
                    TargetValue = 0
                }
            };

            var myGoalDetailsList = new List<MyGoalsDetails> { new MyGoalsDetails
            {
                GoalKeyId = 1,
                KeyDescription = "Test KeyResult",
                Score = 0,
                DueDate = DateTime.Now,
                ImportedId = 0,
                ImportedType = 0,
                Progress = 1,
                Source = 29,
                Contributors = contributors,
                MetricId = 5,
                CurrencyId= 0,
                CurrentValue = 0,
                TargetValue = 1000

            }};

            var myGoalRequestList = new List<MyGoalsRequest> { new MyGoalsRequest
            {
                GoalObjectiveId = 1,
                EmployeeId = 14823,
                Year = 2020,
                IsPrivate = false,
                ObjectiveName = "Test Objective",
                ObjectiveDescription = "Test Objective",
                ObjectiveCycleId = 32,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                MyGoalsDetails = myGoalDetailsList,
                GoalStatusId = 2

            }};

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.InsertGoalObjective(It.IsAny<List<MyGoalsRequest>>(), identity, It.IsAny<string>())).ReturnsAsync(myGoalRequestList);

            var result = await myGoalsController.InsertGoalObjective(myGoalRequestList);
            PayloadCustom<MyGoalsRequest> reqData = ((PayloadCustom<MyGoalsRequest>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task InsertGoalObjective_InvalidToken()
        {
            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.InsertGoalObjective(null) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeleteOkr_IsSuccessful()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var goalObjective = new GoalObjective() { EmployeeId = 14254 };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.DeleteOkrKr(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);
            myGoalService.Setup(e => e.GetGoalObjective(It.IsAny<long>())).Returns(goalObjective);

            var result = await myGoalsController.DeleteOkr(employeeId, goalObjectiveId, goalType);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.True(finalData);
        }

        [Fact]
        public async Task DeleteOkr_Error()
        {
            long employeeId = 14254;
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };
            int goalType = 1;
            long goalObjectiveId = 1;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.DeleteOkr(employeeId, goalObjectiveId, goalType);
            PayloadCustom<bool> reqData = (PayloadCustom<bool>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task DeleteOkr_InvalidToken()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.DeleteOkr(employeeId, goalObjectiveId, goalType) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeleteOkr_NotSuccessful()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            GoalObjective goalObjective = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.DeleteOkrKr(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);
            myGoalService.Setup(e => e.GetGoalObjective(It.IsAny<long>())).Returns(goalObjective);

            var result = await myGoalsController.DeleteOkr(employeeId, goalObjectiveId, goalType);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.False(finalData);
        }

        [Fact]
        public async Task DeleteOkr_EmpError()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            GoalObjective goalObjective = new GoalObjective();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.DeleteOkrKr(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);
            myGoalService.Setup(e => e.GetGoalObjective(It.IsAny<long>())).Returns(goalObjective);

            var result = await myGoalsController.DeleteOkr(employeeId, goalObjectiveId, goalType);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.False(finalData);
        }

        [Fact]
        public async Task GetTypeOfGoalCreation_IsSuccessful()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "Token"
            };
            var typeOfGoalCreation = new List<TypeOfGoalCreation> { new TypeOfGoalCreation
            {
                TypeOfGoalCreationId = 1,
                PrimaryText = "PrimaryText"
            }};

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetTypeOfGoalCreations()).Returns(typeOfGoalCreation);

            var result = await myGoalsController.GetTypeOfGoalCreation();
            PayloadCustom<TypeOfGoalCreation> reqData = (PayloadCustom<TypeOfGoalCreation>)((ObjectResult)result).Value;
            Assert.Equal(typeOfGoalCreation, reqData.EntityList);
        }

        [Fact]
        public async Task GetTypeOfGoalCreation_Error()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "Token"
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.GetTypeOfGoalCreation();
            PayloadCustom<TypeOfGoalCreation> reqData = (PayloadCustom<TypeOfGoalCreation>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetTypeOfGoalCreation_InvalidToken()
        {
            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.GetTypeOfGoalCreation() as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateObjective_IsSuccessful()
        {
            var contributors = new List<ContributorDetails>
            {
                new ContributorDetails
                {
                    EmployeeId = 795,
                    AssignmentTypeId = 3,
                    KeyResult = "Test",
                    Score = 0,
                    StartDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    CurrentValue = 0,
                    TargetValue = 0
                }
            };

            var myGoalDetailsList = new List<MyGoalsDetails> { new MyGoalsDetails
            {
                GoalKeyId = 1,
                KeyDescription = "Test KeyResult",
                Score = 0,
                DueDate = DateTime.Now,
                ImportedId = 0,
                ImportedType = 0,
                Progress = 1,
                Source = 29,
                Contributors = contributors,
                MetricId = 5,
                CurrencyId= 0,
                CurrentValue = 0,
                TargetValue = 1000

            }};

            var myGoalRequestList = new MyGoalsRequest
            {
                GoalObjectiveId = 1,
                EmployeeId = 14823,
                Year = 2020,
                IsPrivate = false,
                ObjectiveName = "Test Objective",
                ObjectiveDescription = "Test Objective",
                ObjectiveCycleId = 32,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                MyGoalsDetails = myGoalDetailsList,
                GoalStatusId = 1

            };

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateObjective(It.IsAny<MyGoalsRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(myGoalRequestList);

            var result = await myGoalsController.UpdateObjective(myGoalRequestList);
            PayloadCustom<MyGoalsRequest> reqData = ((PayloadCustom<MyGoalsRequest>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateObjective_Error()
        {
            var contributors = new List<ContributorDetails>
            {
                new ContributorDetails
                {
                    EmployeeId = 795,
                    AssignmentTypeId = 3,
                    KeyResult = "Test",
                    Score = 0,
                    StartDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    CurrentValue = 0,
                    TargetValue = 0
                }
            };

            var myGoalDetailsList = new List<MyGoalsDetails> { new MyGoalsDetails
            {
                GoalKeyId = 1,
                KeyDescription = "Test KeyResult",
                Score = 0,
                DueDate = DateTime.Now,
                ImportedId = 0,
                ImportedType = 0,
                Progress = 1,
                Source = 29,
                Contributors = contributors,
                MetricId = 0,
                CurrencyId= 0,
                CurrentValue = 0,
                TargetValue = 1000

            }};

            var myGoalRequestList = new MyGoalsRequest
            {
                GoalObjectiveId = 1,
                EmployeeId = 14823,
                Year = 2020,
                IsPrivate = false,
                ObjectiveName = "Test Objective",
                ObjectiveDescription = "Test Objective",
                ObjectiveCycleId = 32,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                MyGoalsDetails = myGoalDetailsList,
                GoalStatusId = 1

            };

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.UpdateObjective(myGoalRequestList);
            PayloadCustom<MyGoalsRequest> reqData = (PayloadCustom<MyGoalsRequest>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateObjective_InvalidToken()
        {
            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UpdateObjective(null) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateKrContributors_IsSuccessful()
        {
            long updatedContributorKr = 1;
            ContributorKeyResultRequest contributorKeyResultRequest = new ContributorKeyResultRequest
            {
                GoalKeyId = 238,
                GoalObjectiveId = 234,
                KrStatusId = 2
            };

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            GoalKey goalKey = new GoalKey
            {
                KrStatusId = 1
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateContributorsKeyResult(It.IsAny<ContributorKeyResultRequest>(),It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(updatedContributorKr);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goalKey);

            var result = await myGoalsController.UpdateKrContributor(contributorKeyResultRequest);
            PayloadCustom<long> reqData = ((PayloadCustom<long>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateKrContributors_Error()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            GoalKey goalKey = new GoalKey
            {
                KrStatusId = 1
            };
            var request = new ContributorKeyResultRequest();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goalKey);
            
            var result = await myGoalsController.UpdateKrContributor(request);
            PayloadCustom<long> reqData = (PayloadCustom<long>)((ObjectResult)result).Value;

            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateKrContributors_StatusError()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            GoalKey goalKey = new GoalKey
            {
                KrStatusId = 2
            };
            ContributorKeyResultRequest contributorKeyResultRequest = new ContributorKeyResultRequest
            {
                GoalKeyId = 238,
                GoalObjectiveId = 234,
                KrStatusId = 2
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goalKey);
            
            var result = await myGoalsController.UpdateKrContributor(contributorKeyResultRequest);
            PayloadCustom<long> reqData = (PayloadCustom<long>)((ObjectResult)result).Value;

            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateKrContributors_Accepted()
        {
            long updatedContributorKr = 1;
            ContributorKeyResultRequest contributorKeyResultRequest = new ContributorKeyResultRequest
            {
                GoalKeyId = 238,
                KrStatusId = 2
            };

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            GoalKey goalKey = new GoalKey
            {
                KrStatusId = 1
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateContributorsKeyResult(It.IsAny<ContributorKeyResultRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(updatedContributorKr);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goalKey);

            var result = await myGoalsController.UpdateKrContributor(contributorKeyResultRequest);
            PayloadCustom<long> reqData = ((PayloadCustom<long>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateKrContributors_Declined()
        {
            long updatedContributorKr = 1;
            ContributorKeyResultRequest contributorKeyResultRequest = new ContributorKeyResultRequest
            {
                GoalKeyId = 238,
                KrStatusId = 3
            };

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            GoalKey goalKey = new GoalKey
            {
                KrStatusId = 1
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateContributorsKeyResult(It.IsAny<ContributorKeyResultRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(updatedContributorKr);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goalKey);

            var result = await myGoalsController.UpdateKrContributor(contributorKeyResultRequest);
            PayloadCustom<long> reqData = ((PayloadCustom<long>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }


        [Fact]
        public async Task UpdateKrContributors_InvalidToken()
        {
            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UpdateKrContributor(null) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }


        [Fact]
        public async Task BecomeContributors_InvalidToken()
        {
            commonService.Setup(e => e.GetUserIdentity());
            AddContributorRequest addContributorRequest = new AddContributorRequest();

            var result = await myGoalsController.BecomeContributor(addContributorRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task BecomeContributors_Error()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            AddContributorRequest addContributorRequest = new AddContributorRequest
            {
                GoalKeyId = 0
            };

            var result = await myGoalsController.BecomeContributor(addContributorRequest);
            PayloadCustom<long> reqData = (PayloadCustom<long>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task BecomeContributors_Successful()
        {

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            AddContributorRequest addContributorRequest = new AddContributorRequest
            {
                GoalKeyId = 1,
                GoalObjectiveId = 1
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.BecomeContributor(It.IsAny<AddContributorRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(1);

            var result = await myGoalsController.BecomeContributor(addContributorRequest);
            PayloadCustom<long> reqData = (PayloadCustom<long>)((ObjectResult)result).Value;
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task MyGoal_IsSuccessful()
        {
            var response = new MyGoalResponse();
            long empId = 1; int cycle = 1; int year = 2020;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.MyGoal(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), identity)).ReturnsAsync(response);

            var result = await myGoalsController.MyGoal(empId, cycle, year);
            PayloadCustom<MyGoalResponse> reqData = ((PayloadCustom<MyGoalResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task MyGoal_Error()
        {
            long empId = 0; int cycle = 1; int year = 2020;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.MyGoal(empId, cycle, year);
            PayloadCustom<MyGoalResponse> reqData = (PayloadCustom<MyGoalResponse>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task MyGoal_InvalidToken()
        {
            long empId = 1; int cycle = 1; int year = 2020;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.MyGoal(empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeleteContributors_IsSuccessful()
        {
            string deleteResponse = "";
            long goalId = 1; long employeeId = 14245;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.DeleteContributors(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<UserIdentity>(),It.IsAny<string>())).ReturnsAsync(deleteResponse);

            var result = await myGoalsController.DeleteContributors(employeeId, goalId);
            PayloadCustom<string> reqData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task DeleteContributors_Error()
        {
            long goalId = 1; long employeeId = 14245;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.DeleteContributors(employeeId, goalId);
            PayloadCustom<string> reqData = (PayloadCustom<string>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task DeleteContributors_InvalidToken()
        {
            long goalId = 1; long employeeId = 14245;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.DeleteContributors(employeeId, goalId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetContributorsByGoalTypeAndId_IsSuccessful()
        {
            var resonse = new List<ContributorsResponse> { new ContributorsResponse
            {
               EmployeeId = 1
            }};
            int goalType = 1; long goalId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetContributorsByGoalTypeAndId(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).Returns(resonse);

            var result = await myGoalsController.GetContributorsByGoalTypeAndId(goalType, goalId);
            PayloadCustomGenric<ContributorsResponse> reqData = ((PayloadCustomGenric<ContributorsResponse>)((ObjectResult)result).Value);
            Assert.Equal(resonse, reqData.EntityList);
        }

        [Fact]
        public async Task GetContributorsByGoalTypeAndId_Error()
        {
            int goalType = 1; long goalId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.GetContributorsByGoalTypeAndId(goalType, goalId);
            PayloadCustomGenric<ContributorsResponse> reqData = (PayloadCustomGenric<ContributorsResponse>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetContributorsByGoalTypeAndId_InvalidToken()
        {
            int goalType = 1; long goalId = 1;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.GetContributorsByGoalTypeAndId(goalType, goalId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateKeyScore_IsSuccessful()
        {
            var response = "";
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var keyScoreUpdate = new KeyScoreUpdate
            {
                GoalObjectiveId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateKeyScore(keyScoreUpdate, identity, It.IsAny<string>())).ReturnsAsync(response);

            var result = await myGoalsController.UpdateKeyScore(keyScoreUpdate);
            PayloadCustom<string> reqData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            Assert.Equal(response, reqData.Entity);
        }

        [Fact]
        public async Task UpdateKeyScore_Error()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var keyScoreUpdate = new KeyScoreUpdate
            {
                GoalObjectiveId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.UpdateKeyScore(keyScoreUpdate);
            PayloadCustom<string> reqData = (PayloadCustom<string>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateKeyScore_InvalidToken()
        {
            var keyScoreUpdate = new KeyScoreUpdate
            {
                GoalObjectiveId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UpdateKeyScore(keyScoreUpdate) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AlignOkr_IsSuccessful()
        {
            var response = new AlignResponse();
            long empId = 1; int cycle = 1; int year = 2020;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.AlignObjective(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), identity)).ReturnsAsync(response);

            var result = await myGoalsController.AlignOkr(empId, cycle, year);
            PayloadCustom<AlignResponse> reqData = ((PayloadCustom<AlignResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task AlignOkr_Error()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            long empId = 1; int cycle = 1; int year = 2020;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.AlignOkr(empId, cycle, year);
            PayloadCustom<AlignResponse> reqData = (PayloadCustom<AlignResponse>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task AlignOkr_InvalidToken()
        {
            long empId = 1; int cycle = 1; int year = 2020;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.AlignOkr(empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UnLockObjectives_IsSuccessful()
        {
            var response = "";
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var unLockRequest = new UnLockRequest
            {
                EmployeeId = 14254,
                Year = 2020,
                Cycle = 33
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.IsAlreadyRequestedAsync(unLockRequest)).ReturnsAsync(false);
            myGoalService.Setup(e => e.LockGoals(unLockRequest, It.IsAny<string>())).ReturnsAsync(response);

            var result = await myGoalsController.UnLockObjectives(unLockRequest);
            PayloadCustom<string> reqData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            Assert.NotNull(reqData);
        }

        [Fact]
        public async Task UnLockObjectives_Error()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            UnLockRequest unLockRequest = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.IsAlreadyRequestedAsync(unLockRequest)).ReturnsAsync(false);

            var result = await myGoalsController.UnLockObjectives(unLockRequest);
            PayloadCustom<string> reqData = (PayloadCustom<string>)((ObjectResult)result).Value;
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UnLockObjectives_LockGoals()
        {
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var unLockRequest = new UnLockRequest
            {
                EmployeeId = 14254,
                Year = 2020,
                Cycle = 33
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.IsAlreadyRequestedAsync(unLockRequest)).ReturnsAsync(false);
            myGoalService.Setup(e => e.LockGoals(It.IsAny<UnLockRequest>(), It.IsAny<string>())).ReturnsAsync("");

            var result = await myGoalsController.UnLockObjectives(unLockRequest);
            PayloadCustom<string> reqData = (PayloadCustom<string>)((ObjectResult)result).Value;
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UnLockObjectives_InvalidToken()
        {
            var unLockRequest = new UnLockRequest
            {
                EmployeeId = 14254,
                Year = 2020,
                Cycle = 33
            };

            commonService.Setup(e => e.GetUserIdentity());
            myGoalService.Setup(e => e.IsAlreadyRequestedAsync(unLockRequest)).ReturnsAsync(false);

            var result = await myGoalsController.UnLockObjectives(unLockRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetKeyDetail_InvalidToken()
        {
            int type = 1;
            long typeId = 1;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.GetKeyDetail(type, typeId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetKeyDetail_IsSuccessful()
        {
            int type = 1;
            long typeId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            MyGoalDetailResponse myGoalDetailResponse = new MyGoalDetailResponse();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.MyGoalDetailResponse(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).Returns(myGoalDetailResponse);

            var result = await myGoalsController.GetKeyDetail(type, typeId);
            PayloadCustom<MyGoalDetailResponse> reqData = ((PayloadCustom<MyGoalDetailResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetKeyDetail_NotSuccess()
        {
            int type = 1;
            long typeId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            MyGoalDetailResponse myGoalDetailResponse = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.MyGoalDetailResponse(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).Returns(myGoalDetailResponse);

            var result = await myGoalsController.GetKeyDetail(type, typeId);
            PayloadCustom<MyGoalDetailResponse> reqData = ((PayloadCustom<MyGoalDetailResponse>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task UnLockApprove_InvalidToken()
        {
            UnLockRequest unLockRequest = new UnLockRequest();

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UnLockApprove(unLockRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UnLockApprove_IsSuccessful()
        {
            UnLockRequest unLockRequest = new UnLockRequest() { EmployeeId = 1, Cycle = 1, Year = 2020 };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            UnLockLog unLockLog = new UnLockLog();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UnLockGoal(It.IsAny<UnLockRequest>(), It.IsAny<UserIdentity>())).ReturnsAsync(unLockLog);

            var result = await myGoalsController.UnLockApprove(unLockRequest);
            PayloadCustom<UnLockLog> reqData = ((PayloadCustom<UnLockLog>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UnLockApprove_Error()
        {
            UnLockRequest unLockRequest = new UnLockRequest() { EmployeeId = 0, Cycle = 1, Year = 2020 };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            UnLockLog unLockLog = new UnLockLog();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UnLockGoal(It.IsAny<UnLockRequest>(), It.IsAny<UserIdentity>())).ReturnsAsync(unLockLog);

            var result = await myGoalsController.UnLockApprove(unLockRequest);
            PayloadCustom<UnLockLog> reqData = ((PayloadCustom<UnLockLog>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task BulkUnlockApprove_InvalidToken()
        {
            List<UnLockRequest> unLockRequest = new List<UnLockRequest>();

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.BulkUnlockApprove(unLockRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task BulkUnlockApprove_IsSuccessful()
        {
            List<UnLockRequest> unLockRequest = new List<UnLockRequest>();
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.BulkUnlockApprove(It.IsAny<List<UnLockRequest>>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(0);

            var result = await myGoalsController.BulkUnlockApprove(unLockRequest);
            PayloadCustom<long> reqData = ((PayloadCustom<long>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task BulkUnlockApprove_Success()
        {
            List<UnLockRequest> unLockRequest = new List<UnLockRequest>();
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.BulkUnlockApprove(It.IsAny<List<UnLockRequest>>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(1);

            var result = await myGoalsController.BulkUnlockApprove(unLockRequest);
            PayloadCustom<long> reqData = ((PayloadCustom<long>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UnlockLog_InvalidToken()
        {
            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UnlockLog() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UnlockLog_IsSuccessful()
        {
            List<UnLockLog> unLockLog = new List<UnLockLog>();
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UnlockLog()).Returns(unLockLog);

            var result = await myGoalsController.UnlockLog();
            PayloadCustom<UnLockLog> reqData = ((PayloadCustom<UnLockLog>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UnlockLog_Success()
        {
            List<UnLockLog> unLockLog = null;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UnlockLog()).Returns(unLockLog);

            var result = await myGoalsController.UnlockLog();
            PayloadCustom<UnLockLog> reqData = ((PayloadCustom<UnLockLog>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task AlignStatus_InvalidToken()
        {
            long employeeId = 1;
            int sourceType = 1;
            long sourceId = 1;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.AlignStatus(employeeId, sourceType, sourceId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AlignStatus_IsSuccessful()
        {
            long employeeId = 1;
            int sourceType = 1;
            long sourceId = 1;

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var alignStatusRes = new AlignStatusResponse()
            {
                AlignStatus = 0,
                IsAligned = false
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.AlignStatus(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>())).Returns(alignStatusRes);

            var result = await myGoalsController.AlignStatus(employeeId, sourceType, sourceId);
            PayloadCustom<AlignStatusResponse> reqData = ((PayloadCustom<AlignStatusResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task AlignStatus_NotSuccessful()
        {
            long employeeId = 1;
            int sourceType = 1;
            long sourceId = 1;

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            AlignStatusResponse alignStatusRes = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.AlignStatus(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>())).Returns(alignStatusRes);

            var result = await myGoalsController.AlignStatus(employeeId, sourceType, sourceId);
            PayloadCustom<AlignStatusResponse> reqData = ((PayloadCustom<AlignStatusResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task DownloadPdf_Error()
        {
            long empId = 0; int cycle = 1; int year = 2020;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.DownloadPdf(empId, cycle, year);
            PayloadCustom<MyGoalsPdfResponse> reqData = (PayloadCustom<MyGoalsPdfResponse>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task DownloadPdf_IsSuccessful()
        {
            var response = new MyGoalsPdfResponse();
            long empId = 1; int cycle = 1; int year = 2020;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.DownloadPDf(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), identity)).ReturnsAsync(response);

            var result = await myGoalsController.DownloadPdf(empId, cycle, year);
            PayloadCustom<MyGoalsPdfResponse> reqData = ((PayloadCustom<MyGoalsPdfResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task DownloadPdf_InvalidToken()
        {
            long empId = 1; int cycle = 1; int year = 2020;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.DownloadPdf(empId, cycle, year) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }


        [Fact]
        public async Task GetGoalByGoalObjectiveId_InvalidToken()
        {
            long objectiveId = 1;
            commonService.Setup(e => e.GetUserIdentity());
            var result = await myGoalsController.GetGoalByGoalObjectiveId(objectiveId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetGoalByGoalObjectiveId_IsSuccessful()
        {
            GoalObjectiveResponse goalObjectiveResponse = new GoalObjectiveResponse
            {
                GoalObjectiveId = 2,
                EmployeeId = 795,
                Year = 2021,
                IsPrivate = false,
                ObjectiveName = "Test",
                ObjectiveDescription = "Test",
                ObjectiveCycleId = 644,
                ImportedType = 1,
                ImportedId = 238,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                Progress = 1,
                Source = 238,
                Sequence = 0,
                GoalStatusId = 3,
                GoalTypeId = 1,
                Score = 56
            };
            long objectiveId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetGoalByGoalObjectiveId(It.IsAny<long>())).ReturnsAsync(goalObjectiveResponse);

            var result = await myGoalsController.GetGoalByGoalObjectiveId(objectiveId);
            PayloadCustom<GoalObjectiveResponse> reqData = ((PayloadCustom<GoalObjectiveResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetKeyDetails_InvalidToken()
        {
            long goalKeyId = 1;
            commonService.Setup(e => e.GetUserIdentity());
            var result = await myGoalsController.GetKeyDetails(goalKeyId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetKeyDetails_IsSuccessful()
        {
            KeyDetailsResponse keyDetailsResponse = new KeyDetailsResponse
            {
                GoalKeyId = 2,
                KeyDescription = "Test",
                StartDate = DateTime.Now,
                DueDate = DateTime.Now,
                Progress = 1,
                Score = 56
            };
            long goalKeyId = 1;

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var goalKey = new GoalKey(){IsActive = true};

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetKeyDetails(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(keyDetailsResponse);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goalKey);

            var result = await myGoalsController.GetKeyDetails(goalKeyId);
            PayloadCustom<KeyDetailsResponse> reqData = ((PayloadCustom<KeyDetailsResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetKeyDetails_NotSuccessful()
        {
            KeyDetailsResponse keyDetailsResponse = new KeyDetailsResponse();
            long goalKeyId = 0;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var goalKey = new GoalKey() { IsActive = true };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetKeyDetails(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(keyDetailsResponse);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goalKey);

            var result = await myGoalsController.GetKeyDetails(goalKeyId);
            PayloadCustom<KeyDetailsResponse> reqData = ((PayloadCustom<KeyDetailsResponse>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task GetKeyDetails_Forbidden()
        {
            long goalKeyId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var goalKey = new GoalKey() { IsActive = false };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goalKey);

            var result = await myGoalsController.GetKeyDetails(goalKeyId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task UpdateGoalKeyAttributes_IsSuccessful()
        {
            long updatedContributorKr = 1;
            var myGoalsDetails = new MyGoalsDetails
            {
                GoalKeyId = 1,
                DueDate = Convert.ToDateTime("2021-03-10"),
                AssignmentTypeId = 2,
                StartDate = Convert.ToDateTime("2021-03-01")

            };

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var contributerList =new  List<KrContributors>();
            var goalKey = new List<GoalKey>() {new GoalKey() {AssignmentTypeId = 2}};
            var goals = new GoalKey() { AssignmentTypeId = 2, GoalKeyId = 1, DueDate = Convert.ToDateTime("2021-03-10")};

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateGoalAttributes(It.IsAny<MyGoalsDetails>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(updatedContributorKr);
            myGoalService.Setup(e => e.GetKeyContributorAsync(It.IsAny<int>(), It.IsAny<long>())).ReturnsAsync(goalKey);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goals);
            commonService.Setup(e => e.GetAllLevelKrContributors(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>())).Returns(contributerList);

            var result = await myGoalsController.UpdateGoalKeyAttributes(myGoalsDetails);
            PayloadCustom<long> reqData = ((PayloadCustom<long>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateGoalKeyAttributes_Error()
        {
            var myGoalsDetails = new MyGoalsDetails
            {
                GoalKeyId = 0,
                DueDate = Convert.ToDateTime("2021-03-10")
            };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var goalKey = new List<GoalKey>() { new GoalKey() { AssignmentTypeId = 2} };
            var goals = new GoalKey() { AssignmentTypeId = 2, GoalKeyId = 1, DueDate = Convert.ToDateTime("2021-03-15") };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetKeyContributorAsync(It.IsAny<int>(), It.IsAny<long>())).ReturnsAsync(goalKey);
            myGoalService.Setup(e => e.GetGoalKeyDetails(It.IsAny<long>())).Returns(goals);

            var result = await myGoalsController.UpdateGoalKeyAttributes(myGoalsDetails);
            PayloadCustom<long> reqData = (PayloadCustom<long>)((ObjectResult)result).Value;

            Assert.False(reqData.IsSuccess);
        }


        [Fact]
        public async Task UpdateGoalKeyAttributes_InvalidToken()
        {
            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UpdateGoalKeyAttributes(null) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }


        [Fact]
        public async Task UpdateGoalObjective_InvalidToken()
        {
            UpdateGoalRequest updateGoalRequest = new UpdateGoalRequest();

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UpdateGoalObjective(updateGoalRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateGoalObjective_NotSuccessful()
        {
            UpdateGoalRequest updateGoalRequest = new UpdateGoalRequest { GoalObjectiveId = 0 };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            long id = 1;
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateGoalObjective(It.IsAny<UpdateGoalRequest>(), It.IsAny<UserIdentity>(),It.IsAny<string>())).ReturnsAsync(id);

            var result = await myGoalsController.UpdateGoalObjective(updateGoalRequest);
            PayloadCustom<long> reqData = ((PayloadCustom<long>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateGoalObjective_IsSuccessful()
        {
            UpdateGoalRequest updateGoalRequest = new UpdateGoalRequest { GoalObjectiveId = 2};
            GoalObjective goalObjective = new GoalObjective { GoalObjectiveId = 2, IsPrivate = true };

            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            long id = 1;
            var goalkIeyList = new List<GoalKey>();
            var contributerList = new List<KrContributors>();
            var allLevelObjectives = new List<AllLevelObjectiveResponse>();
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateGoalObjective(It.IsAny<UpdateGoalRequest>(),It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(id);
            myGoalService.Setup(e => e.GetGoalObjective( It.IsAny<long>())).Returns(goalObjective);
            commonService.Setup(e => e.GetAllLevelKrContributors(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>())).Returns(contributerList);
            myGoalService.Setup(e => e.GetGoalKey(It.IsAny<long>())).Returns(goalkIeyList);
            commonService.Setup(e => e.GetObjectiveSubCascading(It.IsAny<long>())).Returns(allLevelObjectives);


            var result = await myGoalsController.UpdateGoalObjective(updateGoalRequest);
            PayloadCustom<long> reqData = ((PayloadCustom<long>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task AddContributor_InvalidToken()
        {
            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.AddContributor(null) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AddContributor_NotSuccessful()
        {
            var updateGoalRequest = new ContributorDetailRequest();
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var key = new GoalKey(){EmployeeId = 0};
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetKeyFromKeyId(It.IsAny<long>())).Returns(key);

            var result = await myGoalsController.AddContributor(updateGoalRequest);
            PayloadCustom<ContributorDetailRequest> reqData = ((PayloadCustom<ContributorDetailRequest>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task AddContributor_Error()
        {
            var updateGoalRequest = new ContributorDetailRequest(){ EmployeeId = 1,ImportedId = 1,ImportedType = 1 };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var key = new GoalKey() { EmployeeId = 2 };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.IsKeyImportedGoal(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            myGoalService.Setup(e => e.GetKeyFromKeyId(It.IsAny<long>())).Returns(key);

            var result = await myGoalsController.AddContributor(updateGoalRequest);
            PayloadCustom<ContributorDetailRequest> reqData = ((PayloadCustom<ContributorDetailRequest>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task AddContributor_Success()
        {
            var updateGoalRequest = new ContributorDetailRequest() { EmployeeId = 1, ImportedId = 1, ImportedType = 1 };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var key = new GoalKey() { EmployeeId = 2 };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.IsKeyImportedGoal(It.IsAny<long>(), It.IsAny<long>())).Returns(false);
            myGoalService.Setup(e => e.GetKeyFromKeyId(It.IsAny<long>())).Returns(key);
            myGoalService.Setup(e => e.AddContributor(It.IsAny<ContributorDetailRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(updateGoalRequest);

            var result = await myGoalsController.AddContributor(updateGoalRequest);
            PayloadCustom<ContributorDetailRequest> reqData = ((PayloadCustom<ContributorDetailRequest>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateKrValue_InvalidToken()
        {
            KrValueUpdate krValueUpdate = new KrValueUpdate();

            commonService.Setup(e => e.GetUserIdentity());

            var result =  await myGoalsController.UpdateKrValue(krValueUpdate) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateKrValue_IsSuccessful()
        {
            KrValueUpdate krValueUpdate = new KrValueUpdate();
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            progressBarCalculationService.Setup(e => e.UpdateKrValue(It.IsAny<KrValueUpdate>(), It.IsAny<UserIdentity>(), It.IsAny<string>(),It.IsAny<GoalKey>(),false));

            var result = await myGoalsController.UpdateKrValue(krValueUpdate);
            PayloadCustom<KrCalculationResponse> reqData = ((PayloadCustom<KrCalculationResponse>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateGoalDescription_InvalidToken()
        {
            UpdateGoalDescriptionRequest updateGoalDescriptionRequest = new UpdateGoalDescriptionRequest();

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UpdateGoalDescription(updateGoalDescriptionRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateGoalDescription_IsSuccessful()
        {
            UpdateGoalDescriptionRequest updateGoalDescriptionRequest = new UpdateGoalDescriptionRequest(){ GoalId = 1, Description = "test", GoalType = 1 };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateGoalDescription(It.IsAny<UpdateGoalDescriptionRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);

            var result = await myGoalsController.UpdateGoalDescription(updateGoalDescriptionRequest);
            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task UpdateGoalDescription_UnSuccessful()
        {
            UpdateGoalDescriptionRequest updateGoalDescriptionRequest = new UpdateGoalDescriptionRequest();
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateGoalDescription(It.IsAny<UpdateGoalDescriptionRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);

            var result = await myGoalsController.UpdateGoalDescription(updateGoalDescriptionRequest);
            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task DeleteOkrTeam_IsSuccessful()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;
            long teamId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.DeleteOkrKrTeam(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);

            var result = await myGoalsController.DeleteOkrTeam(employeeId, goalObjectiveId, goalType,teamId);

            PayloadCustom<bool> requestData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requestData.Entity;
            Assert.True(finalData);
        }

        [Fact]
        public async Task DeleteOkrTeam_Error()
        {
            long employeeId = 14254;
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };
            int goalType = 1;
            long goalObjectiveId = 1;
            long teamId = 1;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.DeleteOkrTeam(employeeId, goalObjectiveId, goalType, teamId);
            PayloadCustom<bool> requestData = (PayloadCustom<bool>)((ObjectResult)result).Value;
            Assert.False(requestData.IsSuccess);
        }

        [Fact]
        public async Task DeleteOkrTeam_InvalidToken()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;
            long teamId = 1;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.DeleteOkrTeam(employeeId, goalObjectiveId, goalType, teamId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateGoalSequence_InvalidToken()
        {
            List<UpdateSequenceRequest> updateSequenceRequests = new List<UpdateSequenceRequest>();

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UpdateGoalSequence(updateSequenceRequests) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateGoalSequence_IsSuccessful()
        {
            List<UpdateSequenceRequest> updateSequenceRequests = new List<UpdateSequenceRequest>();
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateOkrSequence(It.IsAny<List<UpdateSequenceRequest>>(), It.IsAny<UserIdentity>())).ReturnsAsync(true);

            var result = await myGoalsController.UpdateGoalSequence(updateSequenceRequests);

            PayloadCustom<bool> requestData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requestData.Entity;
            Assert.True(finalData);
        }

        [Fact]
        public async Task UpdateGoalSequence_Error()
        {
            List<UpdateSequenceRequest> updateSequenceRequests = new List<UpdateSequenceRequest>();
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateOkrSequence(It.IsAny<List<UpdateSequenceRequest>>(), It.IsAny<UserIdentity>())).ReturnsAsync(false);

            var result = await myGoalsController.UpdateGoalSequence(updateSequenceRequests);

            PayloadCustom<bool> requestData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requestData.Entity;
            Assert.False(finalData);
        }

        [Fact]
        public async Task GetKrStatusContributors_InvalidToken()
        {
            int goalType = 1;
            long goalId = 1;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.GetKrStatusContributors(goalType, goalId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetKrStatusContributors_IsSuccessful()
        {
            int goalType = 1;
            long goalId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };
            var krStatusContributor = new List<KrStatusContributorResponse>();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetKrStatusContributors(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).Returns(krStatusContributor);

            var result = await myGoalsController.GetKrStatusContributors(goalType, goalId);

            PayloadCustomGenric<KrStatusContributorResponse> requestData = ((PayloadCustomGenric<KrStatusContributorResponse>)((ObjectResult)result).Value);
            var finalData = requestData.IsSuccess;
            Assert.True(finalData);
        }

        [Fact]
        public async Task GetKrStatusContributors_Error()
        {
            int goalType = 1;
            long goalId = 0;
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };
            var krStatusContributor = new List<KrStatusContributorResponse>();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.GetKrStatusContributors(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).Returns(krStatusContributor);

            var result = await myGoalsController.GetKrStatusContributors(goalType, goalId);

            PayloadCustomGenric<KrStatusContributorResponse> requestData = ((PayloadCustomGenric<KrStatusContributorResponse>)((ObjectResult)result).Value);
            var finalData = requestData.IsSuccess;
            Assert.False(finalData);
        }

        [Fact]
        public async Task LinkObjectivesAsync_InvalidToken()
        {
            var empId = 331;
            var cycleId = 24;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.LinkObjectivesAsync(empId, cycleId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task LinkObjectivesAsync_IsSuccessful()
        {
            var empId = 331;
            var cycleId = 24;
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };
            var linkedObjectiveResponse = new List<LinkedObjectiveResponse>(){new LinkedObjectiveResponse(){ObjectiveName = "team"}};

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.LinkObjectivesAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(linkedObjectiveResponse);

            var result = await myGoalsController.LinkObjectivesAsync(empId, cycleId);

            PayloadCustom<LinkedObjectiveResponse> requestData = ((PayloadCustom<LinkedObjectiveResponse>)((ObjectResult)result).Value);
            var finalData = requestData.IsSuccess;
            Assert.True(finalData);
        }

        [Fact]
        public async Task LinkObjectivesAsync_Error()
        {
            var empId = 331;
            var cycleId = 24;
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };
            var linkedObjectiveResponse = new List<LinkedObjectiveResponse>();
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.LinkObjectivesAsync(It.IsAny<long>(), It.IsAny<int>(),It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(linkedObjectiveResponse);

            var result = await myGoalsController.LinkObjectivesAsync(empId, cycleId);

            PayloadCustom<LinkedObjectiveResponse> requestData = ((PayloadCustom<LinkedObjectiveResponse>)((ObjectResult)result).Value);
            var finalData = requestData.IsSuccess;
            Assert.True(finalData);
        }

        [Fact]
        public async Task ResetOkr_IsSuccessful()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var goalObjective = new GoalObjective(){ EmployeeId = 14254 };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.ResetOkr(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);
            myGoalService.Setup(e => e.GetGoalObjective(It.IsAny<long>())).Returns(goalObjective);

            var result = await myGoalsController.ResetOkr(employeeId, goalObjectiveId, goalType);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.True(finalData);
        }

        [Fact]
        public async Task ResetOkr_Error()
        {
            long employeeId = 0;
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };
            int goalType = 0;
            long goalObjectiveId = 0;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.ResetOkr(employeeId, goalObjectiveId, goalType);
            PayloadCustom<bool> reqData = (PayloadCustom<bool>)((ObjectResult)result).Value;
            Assert.False(reqData.IsSuccess);
        }

        [Fact]
        public async Task ResetOkr_UnSuccessful()
        {
            long employeeId = 14254;
            var identity = new UserIdentity
            {
                EmployeeId = 13485,
                FirstName = "TestName"
            };
            int goalType = 1;
            long goalObjectiveId = 1;
            var goalObjective = new GoalObjective() { EmployeeId = 14254 };

            myGoalService.Setup(e => e.ResetOkr(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(false);
            myGoalService.Setup(e => e.GetGoalObjective(It.IsAny<long>())).Returns(goalObjective);
            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.ResetOkr(employeeId, goalObjectiveId, goalType);
            PayloadCustom<bool> reqData = (PayloadCustom<bool>)((ObjectResult)result).Value;
            Assert.True(reqData.IsSuccess);
        }

        [Fact]
        public async Task ResetOkr_InvalidToken()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.ResetOkr(employeeId, goalObjectiveId, goalType) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ResetOkr_NotSuccessful()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            GoalObjective goalObjective = null;

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.ResetOkr(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);
            myGoalService.Setup(e => e.GetGoalObjective(It.IsAny<long>())).Returns(goalObjective);

            var result = await myGoalsController.ResetOkr(employeeId, goalObjectiveId, goalType);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.False(finalData);
        }

        [Fact]
        public async Task ResetOkr_EmpError()
        {
            long employeeId = 14254;
            int goalType = 1;
            long goalObjectiveId = 1;
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };
            var goalObjective = new GoalObjective();

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.ResetOkr(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);
            myGoalService.Setup(e => e.GetGoalObjective(It.IsAny<long>())).Returns(goalObjective);

            var result = await myGoalsController.ResetOkr(employeeId, goalObjectiveId, goalType);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.False(finalData);
        }

        [Fact]
        public async Task UpdateTeamLeaderOkr_InvalidToken()
        {
            UpdateTeamLeaderOkrRequest teamLeaderOkrRequest = new UpdateTeamLeaderOkrRequest();

            commonService.Setup(e => e.GetUserIdentity());

            var result = await myGoalsController.UpdateTeamLeaderOkr(teamLeaderOkrRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateTeamLeaderOkr_Error()
        {
            UpdateTeamLeaderOkrRequest teamLeaderOkrRequest = new UpdateTeamLeaderOkrRequest();
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await myGoalsController.UpdateTeamLeaderOkr(teamLeaderOkrRequest);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.False(finalData);
        }

        [Fact]
        public async Task UpdateTeamLeaderOkr_Success()
        {
            UpdateTeamLeaderOkrRequest teamLeaderOkrRequest = new UpdateTeamLeaderOkrRequest() { OldLeader = 331, NewLeader = 332, TeamId = 407, CycleId = 334 };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateTeamLeaderOkr(It.IsAny<UpdateTeamLeaderOkrRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);

            var result = await myGoalsController.UpdateTeamLeaderOkr(teamLeaderOkrRequest);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.True(finalData);
        }

        [Fact]
        public async Task UpdateTeamLeaderOkr_NotSuccess()
        {
            UpdateTeamLeaderOkrRequest teamLeaderOkrRequest = new UpdateTeamLeaderOkrRequest() { OldLeader = 331, NewLeader = 332, TeamId = 407, CycleId = 334 };
            var identity = new UserIdentity
            {
                EmployeeId = 14254
            };

            commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            myGoalService.Setup(e => e.UpdateTeamLeaderOkr(It.IsAny<UpdateTeamLeaderOkrRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(false);

            var result = await myGoalsController.UpdateTeamLeaderOkr(teamLeaderOkrRequest);

            PayloadCustom<bool> reqData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = reqData.Entity;
            Assert.False(finalData);
        }

        #region Private Methods

        private void SetUserClaimsAndRequest()
        {
            myGoalsController.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            myGoalsController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };
            string sampleAuthToken = Guid.NewGuid().ToString();
            
        }

        #endregion
    }
}
