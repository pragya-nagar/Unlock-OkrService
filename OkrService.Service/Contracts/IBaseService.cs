using System.Net.Http;
using OKRService.EF;

namespace OKRService.Service.Contracts
{
    public interface IBaseService
    {
        IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        IOperationStatus OperationStatus { get; set; }
        OkrServiceDbContext OkrServiceDBContext { get; set; }
        string ConnectionString { get; set; }
        HttpClient GetHttpClient(string jwtToken);
    }
}
