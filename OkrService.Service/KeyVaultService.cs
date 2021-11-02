using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OKRService.Common;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;

namespace OKRService.Service
{
    public class KeyVaultService : BaseService, IKeyVaultService
    {
        public KeyVaultService(IServicesAggregator servicesAggregateService) : base(servicesAggregateService)
        {
        }
        public async Task<BlobVaultResponse> GetAzureBlobKeysAsync()
        {
            if (!IsTokenActive) return null;
            BlobVaultResponse blobVaultResponse = new BlobVaultResponse();
            var hasTenant = HttpContext.Request.Headers.TryGetValue("TenantId", out var tenantId);
            if ((!hasTenant && HttpContext.Request.Host.Value.Contains("localhost")))
                tenantId = Configuration.GetValue<string>("TenantId");

            if (!string.IsNullOrEmpty(tenantId))
            {
                var tenantString = Encryption.DecryptRijndael(tenantId, Configuration.GetValue<string>("Encryption:PrivateKey"));
                blobVaultResponse.BlobCdnUrl = Configuration.GetValue<string>("AzureBlob:BlobCdnUrl");
                blobVaultResponse.BlobCdnCommonUrl = Configuration.GetValue<string>("AzureBlob:BlobCdnUrl") + "common/";
            }

            return await Task.FromResult(blobVaultResponse);
        }

        public async Task<ServiceSettingUrlResponse> GetSettingsAndUrlsAsync()
        {
            if (!IsTokenActive) return null;
            string domain;
            var hasOrigin = HttpContext.Request.Headers.TryGetValue("OriginHost", out var origin);
            if (!hasOrigin && HttpContext.Request.Host.Value.Contains("localhost"))
                domain = Configuration.GetValue<string>("FrontEndUrl").ToString();
            else
                domain = string.IsNullOrEmpty(origin) ? string.Empty : origin.ToString();
            ServiceSettingUrlResponse settingsResponse = new ServiceSettingUrlResponse
            {
                UnlockLog = Configuration.GetValue<string>("OkrService:UnlockLog"),
                OkrBaseAddress = Configuration.GetValue<string>("OkrService:BaseUrl"),
                OkrUnlockTime = Configuration.GetValue<string>("OkrService:UnlockTime"),
                FrontEndUrl = domain,
                ResetPassUrl = Configuration.GetValue<string>("ResetPassUrl"),
                NotificationBaseAddress = Configuration.GetValue<string>("Notifications:BaseUrl")
            };

            return await Task.FromResult(settingsResponse);
        }



    }
}
