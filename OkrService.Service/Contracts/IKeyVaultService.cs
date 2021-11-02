using OKRService.ViewModel.Response;
using System.Threading.Tasks;

namespace OKRService.Service.Contracts
{
    public interface IKeyVaultService
    {
        Task<BlobVaultResponse> GetAzureBlobKeysAsync();
        Task<ServiceSettingUrlResponse> GetSettingsAndUrlsAsync();
    }
}
