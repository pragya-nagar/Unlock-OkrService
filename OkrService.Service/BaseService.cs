using System;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OKRService.EF;
using OKRService.Service.Contracts;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using OKRService.Common;
using System.Text.RegularExpressions;

namespace OKRService.Service
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseService : IBaseService
    {
        public IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        public IOperationStatus OperationStatus { get; set; }
        public OkrServiceDbContext OkrServiceDBContext { get; set; }
        public IConfiguration Configuration { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }
        protected IMapper Mapper { get; private set; }
        protected ILogger Logger { get; private set; }
        protected HttpContext HttpContext => new HttpContextAccessor().HttpContext;
        protected string LoggedInUserEmail => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        protected string UserToken => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "token")?.Value;
        protected string TenantId => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value;
        protected bool IsTokenActive => (!string.IsNullOrEmpty(LoggedInUserEmail) && !string.IsNullOrEmpty(UserToken));
        private IKeyVaultService keyVaultService;
        public IKeyVaultService KeyVaultService => keyVaultService ??= HttpContext.RequestServices.GetRequiredService<IKeyVaultService>();
        public string ConnectionString
        {
            get => OkrServiceDBContext?.Database.GetDbConnection().ConnectionString;
            set
            {
                if (OkrServiceDBContext != null)
                    OkrServiceDBContext.Database.GetDbConnection().ConnectionString = value;
            }
        }

        protected BaseService(IServicesAggregator servicesAggregateService)
        {
            UnitOfWorkAsync = servicesAggregateService.UnitOfWorkAsync;
            OkrServiceDBContext = UnitOfWorkAsync.DataContext as OkrServiceDbContext;
            OperationStatus = servicesAggregateService.OperationStatus;
            Configuration = servicesAggregateService.Configuration;
            HostingEnvironment = servicesAggregateService.HostingEnvironment;
            Mapper = servicesAggregateService.Mapper;
            Logger = Log.Logger;
        }

        public HttpClient GetHttpClient(string jwtToken)
        {
            var settings = KeyVaultService.GetSettingsAndUrlsAsync().Result;
            var hasTenant = HttpContext.Request.Headers.TryGetValue("TenantId", out var tenantId);
            if ((!hasTenant && HttpContext.Request.Host.Value.Contains("localhost")))
                tenantId = Configuration.GetValue<string>("TenantId");
            string domain;
            var hasOrigin = HttpContext.Request.Headers.TryGetValue("OriginHost", out var origin);
            if (!hasOrigin && HttpContext.Request.Host.Value.Contains("localhost"))
                domain = Configuration.GetValue<string>("FrontEndUrl").ToString();
            else
                domain = string.IsNullOrEmpty(origin) ? string.Empty : origin.ToString();
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Configuration.GetValue<string>("User:BaseUrl"))
            };
            var token = !string.IsNullOrEmpty(jwtToken) ? jwtToken : UserToken;
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            httpClient.DefaultRequestHeaders.Add("TenantId", tenantId.ToString());
            httpClient.DefaultRequestHeaders.Add("OriginHost", domain);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
